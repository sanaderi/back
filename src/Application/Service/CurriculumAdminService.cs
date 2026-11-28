namespace GamaEdtech.Application.Service
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text.RegularExpressions;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.UnitOfWork;
    using GamaEdtech.Common.Service;
    using GamaEdtech.Data.Dto.Curriculum;
    using GamaEdtech.Domain.Entity.Curriculum;
    using GamaEdtech.Domain.Enumeration;

    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using static GamaEdtech.Common.Core.Constants;

    public sealed partial class CurriculumAdminService(Lazy<IUnitOfWorkProvider> unitOfWorkProvider, Lazy<IHttpContextAccessor> httpContextAccessor,
        Lazy<ILogger<CurriculumAdminService>> logger, Lazy<IMigrationRouter> migrationRouter)
        : ServiceBase<CurriculumAdminService>(unitOfWorkProvider, httpContextAccessor, logger), ICurriculumAdminService
    {
        #region Boards

        public Task<ResultData<IReadOnlyList<CurriculumBoardDto>>> GetBoardsAsync() => RunAsync<IReadOnlyList<CurriculumBoardDto>>(async uow =>
            Ok<IReadOnlyList<CurriculumBoardDto>>(await uow.GetRepository<CurriculumBoard, int>().GetManyQueryable()
                .OrderBy(t => t.ListOrder).ThenBy(t => t.Name).Select(t => ToDto(t)).ToListAsync()));

        public Task<ResultData<CurriculumBoardDetailDto>> GetBoardAsync(int id) => RunAsync(async uow =>
        {
            var board = await uow.GetRepository<CurriculumBoard, int>().GetAsync(id, tracking: false);
            if (board is null)
            {
                return NotFound<CurriculumBoardDetailDto>();
            }

            var tabs = await uow.GetRepository<CurriculumBoardTab, int>().GetManyQueryable(t => t.BoardId == id).ToListAsync();
            var kinds = await uow.GetRepository<CurriculumVariantKind, int>().GetManyQueryable(t => t.BoardId == id).OrderBy(t => t.ListOrder).ToListAsync();
            var kindIds = kinds.Select(t => t.Id).ToList();
            var options = await uow.GetRepository<CurriculumVariantOption, int>().GetManyQueryable(t => kindIds.Contains(t.VariantKindId)).OrderBy(t => t.ListOrder).ToListAsync();

            return Ok(new CurriculumBoardDetailDto
            {
                Board = ToDto(board),
                Filters = await uow.GetRepository<CurriculumBoardFilter, int>().GetManyQueryable(t => t.BoardId == id).OrderBy(t => t.ListOrder)
                    .Select(t => new CurriculumBoardFilterDto { Key = t.Key, Label = t.Label, Style = t.Style, ListOrder = t.ListOrder, IsEnabled = t.IsEnabled }).ToListAsync(),
                Tabs = [.. CurriculumLinkRules.Tabs.Select(tab => tabs.Find(t => t.Tab == tab) is { } set
                    ? new CurriculumBoardTabDto { Tab = tab, Scope = set.Scope }
                    : new CurriculumBoardTabDto { Tab = tab, Scope = CurriculumLinkRules.DefaultScope(tab), IsDefault = true })],
                LinkOrder = await uow.GetRepository<CurriculumBoardLinkOrder, int>().GetManyQueryable(t => t.BoardId == id).OrderBy(t => t.ListOrder).Select(t => t.OtherBoardId).ToListAsync(),
                SeriesTypes = await uow.GetRepository<CurriculumSeriesType, int>().GetManyQueryable(t => t.BoardId == id).OrderBy(t => t.ListOrder).Select(t => new CurriculumSeriesTypeDto
                {
                    Id = t.Id,
                    QualificationId = t.QualificationId,
                    Name = t.Name,
                    Code = t.Code,
                    FileCode = t.FileCode,
                    SeoName = t.SeoName,
                    Months = t.Months,
                    IsSupplementary = t.IsSupplementary,
                    ListOrder = t.ListOrder,
                }).ToListAsync(),
                VariantKinds = [.. kinds.Select(k => new CurriculumVariantKindDto
                {
                    Id = k.Id,
                    Key = k.Key,
                    Label = k.Label,
                    ListOrder = k.ListOrder,
                    Options = [.. options.Where(o => o.VariantKindId == k.Id).Select(o => new CurriculumVariantOptionDto { Id = o.Id, LegacyId = o.LegacyId, Label = o.Label, Code = o.Code, ListOrder = o.ListOrder })],
                })],
                SeoTemplates = [.. (await uow.GetRepository<CurriculumSeoTemplate, int>().GetManyQueryable(t => t.BoardId == id).ToListAsync()).OrderBy(t => t.Page!.Value).Select(ToDto)],
            });
        });

        public Task<ResultData<int>> SaveBoardAsync([NotNull] CurriculumBoardDto board, long userId) => RunAsync(async uow =>
        {
            if (SwitchRefusal<int>() is { } notReady)
            {
                return notReady;
            }

            if (Invalid<int>(Slug(board.Slug), Required(board.Name, 200, "name"), Optional(board.ShortName, 50, "shortName"),
                board.CountryCode is { Length: > 0 } country && !CountryCode().IsMatch(country) ? "countryCode" : null,
                Optional(board.Logo, 300, "logo"), Optional(board.Website, 300, "website"), Optional(board.CodeTemplate, 200, "codeTemplate")) is { } invalid)
            {
                return invalid;
            }

            var repository = uow.GetRepository<CurriculumBoard, int>();
            var entity = board.Id == 0 ? new CurriculumBoard { CreationDate = DateTimeOffset.UtcNow, CreationUserId = userId } : await repository.GetAsync(board.Id);
            if (entity is null)
            {
                return NotFound<int>();
            }

            if (Copied<int>(entity.LegacyId) is { } refused)
            {
                return refused;
            }

            if (await repository.AnyAsync(t => t.Slug == board.Slug && t.Id != board.Id))
            {
                return Duplicate<int>("slug");
            }

            if (board.IsMaster && await repository.AnyAsync(t => t.IsMaster && t.Id != board.Id))
            {
                return Refused<int>("Another board is already the master board (GAMA).", "isMaster");
            }

            if (board.IsDefault && await repository.AnyAsync(t => t.IsDefault && t.Id != board.Id))
            {
                return Refused<int>("Another board is already the default.", "isDefault");
            }

            if (board.ParentBoardId is int parent && (parent == board.Id || !await repository.AnyAsync(t => t.Id == parent)))
            {
                return Refused<int>("The parent board must be another existing board.", "parentBoardId");
            }

            entity.Slug = board.Slug;
            entity.Name = board.Name!.Trim();
            entity.ShortName = Trim(board.ShortName);
            entity.CountryCode = Trim(board.CountryCode)?.ToUpperInvariant();
            entity.ParentBoardId = board.ParentBoardId;
            entity.Logo = Trim(board.Logo);
            entity.Website = Trim(board.Website);
            entity.IsMaster = board.IsMaster;
            entity.IsDefault = board.IsDefault;
            entity.IsEnabled = board.IsEnabled;
            entity.ListOrder = board.ListOrder;
            entity.CodeTemplate = Trim(board.CodeTemplate);
            entity.LanguageId = board.LanguageId;
            Touch(entity, board.Id, userId, repository);
            _ = await uow.SaveChangesAsync();
            if (MirrorOn)
            {
                await CurriculumMirror.MirrorBoardAsync(uow, entity);
                _ = await uow.SaveChangesAsync();
            }

            return Ok(entity.Id);
        });

        public Task<ResultData<bool>> DeleteBoardAsync(int id) => RunAsync(async uow =>
        {
            var repository = uow.GetRepository<CurriculumBoard, int>();
            var board = await repository.GetAsync(id);
            if (board is null)
            {
                return NotFound<bool>();
            }

            if (board.LegacyId is not null)
            {
                return Refused<bool>("A copied board is kept: old links point at it. Turn it off instead.", "legacyId");
            }

            if (await uow.GetRepository<CurriculumQualification, int>().AnyAsync(t => t.BoardId == id)
                || await repository.AnyAsync(t => t.ParentBoardId == id)
                || await uow.GetRepository<CurriculumBoardLinkOrder, int>().AnyAsync(t => t.OtherBoardId == id))
            {
                return InUse<bool>();
            }

            // Its own lists (filters, tabs, link order, series and variant kinds, SEO templates) go with it (cascade).
            repository.Remove(board);
            _ = await uow.SaveChangesAsync();
            return Ok(true);
        });

        public Task<ResultData<bool>> SaveBoardFiltersAsync(int boardId, [NotNull] IReadOnlyList<CurriculumBoardFilterDto> filters) => RunAsync(async uow =>
        {
            if (!await uow.GetRepository<CurriculumBoard, int>().AnyAsync(t => t.Id == boardId))
            {
                return NotFound<bool>();
            }

            foreach (var filter in filters)
            {
                if (!CurriculumFilterKeys.IsKnown(filter.Key) || filter.Key!.Length > 30)
                {
                    return Refused<bool>($"Unknown filter \"{filter.Key}\".", "key");
                }

                if (Invalid<bool>(Required(filter.Label, 100, "label")) is { } invalid)
                {
                    return invalid;
                }
            }

            if (filters.GroupBy(t => t.Key).Any(t => t.Count() > 1))
            {
                return Duplicate<bool>("key");
            }

            var repository = uow.GetRepository<CurriculumBoardFilter, int>();
            var existing = await repository.GetManyQueryable(t => t.BoardId == boardId, tracking: true).ToListAsync();
            foreach (var old in existing.Where(t => filters.All(f => f.Key != t.Key)))
            {
                repository.Remove(old);
            }

            foreach (var filter in filters)
            {
                var row = existing.Find(t => t.Key == filter.Key);
                if (row is null)
                {
                    row = new CurriculumBoardFilter { BoardId = boardId, Key = filter.Key };
                    repository.Add(row);
                }

                row.Label = filter.Label!.Trim();
                row.Style = filter.Style ?? CurriculumFilterStyle.Select;
                row.ListOrder = filter.ListOrder;
                row.IsEnabled = filter.IsEnabled;
            }

            _ = await uow.SaveChangesAsync();
            return Ok(true);
        });

        public Task<ResultData<bool>> SaveBoardLinksAsync(int boardId, [NotNull] CurriculumBoardLinksDto links) => RunAsync(async uow =>
        {
            var boards = uow.GetRepository<CurriculumBoard, int>();
            if (!await boards.AnyAsync(t => t.Id == boardId))
            {
                return NotFound<bool>();
            }

            List<int> order = [.. links.LinkOrder.Distinct()];
            if (order.Contains(boardId) || await boards.CountAsync(t => order.Contains(t.Id)) != order.Count)
            {
                return Refused<bool>("The order lists other existing boards only.", "linkOrder");
            }

            if (links.Tabs.GroupBy(t => t.Tab).Any(t => t.Count() > 1))
            {
                return Duplicate<bool>("tabs");
            }

            // A tab set back to its default is not stored, so a later change of the default reaches it.
            var tabs = uow.GetRepository<CurriculumBoardTab, int>();
            var existing = await tabs.GetManyQueryable(t => t.BoardId == boardId, tracking: true).ToListAsync();
            foreach (var row in existing)
            {
                tabs.Remove(row);
            }

            foreach (var tab in links.Tabs.Where(t => t.Scope != CurriculumLinkRules.DefaultScope(t.Tab)))
            {
                tabs.Add(new CurriculumBoardTab { BoardId = boardId, Tab = tab.Tab, Scope = tab.Scope });
            }

            var orders = uow.GetRepository<CurriculumBoardLinkOrder, int>();
            foreach (var row in await orders.GetManyQueryable(t => t.BoardId == boardId, tracking: true).ToListAsync())
            {
                orders.Remove(row);
            }

            for (var i = 0; i < order.Count; i++)
            {
                orders.Add(new CurriculumBoardLinkOrder { BoardId = boardId, OtherBoardId = order[i], ListOrder = i });
            }

            _ = await uow.SaveChangesAsync();
            return Ok(true);
        });

        public Task<ResultData<bool>> SaveSeriesTypesAsync(int boardId, [NotNull] IReadOnlyList<CurriculumSeriesTypeDto> seriesTypes) => RunAsync(async uow =>
        {
            if (!await uow.GetRepository<CurriculumBoard, int>().AnyAsync(t => t.Id == boardId))
            {
                return NotFound<bool>();
            }

            List<int> qualificationIds = [.. seriesTypes.Where(t => t.QualificationId is not null).Select(t => t.QualificationId!.Value).Distinct()];
            if (await uow.GetRepository<CurriculumQualification, int>().CountAsync(t => t.BoardId == boardId && qualificationIds.Contains(t.Id)) != qualificationIds.Count)
            {
                return Refused<bool>("A series type's qualification must be one of the board's.", "qualificationId");
            }

            foreach (var item in seriesTypes)
            {
                if (Invalid<bool>(Required(item.Name, 100, "name"), Optional(item.Code, 20, "code"), Optional(item.FileCode, 5, "fileCode"), Optional(item.SeoName, 100, "seoName"),
                    item.Months is { Length: > 0 } months && !Months().IsMatch(months) ? "months" : null) is { } invalid)
                {
                    return invalid;
                }
            }

            var repository = uow.GetRepository<CurriculumSeriesType, int>();
            var existing = await repository.GetManyQueryable(t => t.BoardId == boardId, tracking: true).ToListAsync();
            if (seriesTypes.Any(t => t.Id != 0 && existing.TrueForAll(e => e.Id != t.Id)))
            {
                return NotFound<bool>();
            }

            foreach (var old in existing.Where(t => seriesTypes.All(s => s.Id != t.Id)))
            {
                repository.Remove(old);
            }

            foreach (var item in seriesTypes)
            {
                var row = item.Id == 0 ? null : existing.Find(t => t.Id == item.Id);
                if (row is null)
                {
                    row = new CurriculumSeriesType { BoardId = boardId };
                    repository.Add(row);
                }

                row.QualificationId = item.QualificationId;
                row.Name = item.Name!.Trim();
                row.Code = Trim(item.Code);
                row.FileCode = Trim(item.FileCode);
                row.SeoName = Trim(item.SeoName);
                row.Months = Trim(item.Months);
                row.IsSupplementary = item.IsSupplementary;
                row.ListOrder = item.ListOrder;
            }

            _ = await uow.SaveChangesAsync();
            return Ok(true);
        });

        public Task<ResultData<bool>> SaveVariantKindsAsync(int boardId, [NotNull] IReadOnlyList<CurriculumVariantKindDto> variantKinds) => RunAsync(async uow =>
        {
            if (SwitchRefusal<bool>() is { } notReady)
            {
                return notReady;
            }

            if (!await uow.GetRepository<CurriculumBoard, int>().AnyAsync(t => t.Id == boardId))
            {
                return NotFound<bool>();
            }

            foreach (var kind in variantKinds)
            {
                if (Invalid<bool>(Key(kind.Key), Required(kind.Label, 100, "label")) is { } invalid)
                {
                    return invalid;
                }

                foreach (var option in kind.Options)
                {
                    if (Invalid<bool>(Required(option.Label, 50, "label"), Optional(option.Code, 20, "code")) is { } invalidOption)
                    {
                        return invalidOption;
                    }
                }
            }

            if (variantKinds.GroupBy(t => t.Key).Any(t => t.Count() > 1))
            {
                return Duplicate<bool>("key");
            }

            var kinds = uow.GetRepository<CurriculumVariantKind, int>();
            var options = uow.GetRepository<CurriculumVariantOption, int>();
            var existingKinds = await kinds.GetManyQueryable(t => t.BoardId == boardId, tracking: true).ToListAsync();
            var kindIds = existingKinds.Select(t => t.Id).ToList();
            var existingOptions = await options.GetManyQueryable(t => kindIds.Contains(t.VariantKindId), tracking: true).ToListAsync();

            // A copied option (it carries a LegacyId) can't be removed while the copy still writes them.
            var removed = existingOptions.Where(t => variantKinds.SelectMany(k => k.Options).All(o => o.Id != t.Id)).ToList();
            if (removed.Exists(t => t.LegacyId is not null) && CopyRefusal<bool>() is { } refused)
            {
                return refused;
            }

            foreach (var old in removed)
            {
                options.Remove(old);
            }

            foreach (var old in existingKinds.Where(t => variantKinds.All(k => k.Id != t.Id)))
            {
                kinds.Remove(old);
            }

            foreach (var kind in variantKinds)
            {
                var row = kind.Id == 0 ? null : existingKinds.Find(t => t.Id == kind.Id);
                if (row is null)
                {
                    row = new CurriculumVariantKind { BoardId = boardId };
                    kinds.Add(row);
                }

                row.Key = kind.Key;
                row.Label = kind.Label!.Trim();
                row.ListOrder = kind.ListOrder;
                _ = await uow.SaveChangesAsync();

                foreach (var option in kind.Options)
                {
                    var optionRow = option.Id == 0 ? null : existingOptions.Find(t => t.Id == option.Id);
                    if (optionRow is null)
                    {
                        optionRow = new CurriculumVariantOption();
                        options.Add(optionRow);
                    }

                    optionRow.VariantKindId = row.Id;
                    optionRow.Label = option.Label!.Trim();
                    optionRow.Code = Trim(option.Code);
                    optionRow.ListOrder = option.ListOrder;
                }
            }

            _ = await uow.SaveChangesAsync();
            if (MirrorOn)
            {
                List<int> savedKinds = [.. await kinds.GetManyQueryable(t => t.BoardId == boardId).Select(t => t.Id).ToListAsync()];
                foreach (var option in await options.GetManyQueryable(t => savedKinds.Contains(t.VariantKindId), tracking: true).ToListAsync())
                {
                    await CurriculumMirror.MirrorVariantAsync(uow, option);
                }

                _ = await uow.SaveChangesAsync();
            }

            return Ok(true);
        });

        #endregion

        #region Qualifications, subjects, syllabi, papers, document types

        public Task<ResultData<IReadOnlyList<CurriculumQualificationDto>>> GetQualificationsAsync(int boardId) => RunAsync<IReadOnlyList<CurriculumQualificationDto>>(async uow =>
            Ok<IReadOnlyList<CurriculumQualificationDto>>(await uow.GetRepository<CurriculumQualification, int>().GetManyQueryable(t => t.BoardId == boardId)
                .OrderBy(t => t.ListOrder).ThenBy(t => t.Name).Select(t => new CurriculumQualificationDto
                {
                    Id = t.Id,
                    LegacyId = t.LegacyId,
                    BoardId = t.BoardId,
                    Slug = t.Slug,
                    Name = t.Name,
                    ShortName = t.ShortName,
                    LanguageId = t.LanguageId,
                    ListOrder = t.ListOrder,
                }).ToListAsync()));

        public Task<ResultData<int>> SaveQualificationAsync([NotNull] CurriculumQualificationDto qualification, long userId) => RunAsync(async uow =>
        {
            if (SwitchRefusal<int>() is { } notReady)
            {
                return notReady;
            }

            if (Invalid<int>(Slug(qualification.Slug), Required(qualification.Name, 200, "name"), Optional(qualification.ShortName, 50, "shortName")) is { } invalid)
            {
                return invalid;
            }

            var repository = uow.GetRepository<CurriculumQualification, int>();
            var entity = qualification.Id == 0 ? new CurriculumQualification { CreationDate = DateTimeOffset.UtcNow, CreationUserId = userId } : await repository.GetAsync(qualification.Id);
            if (entity is null || !await uow.GetRepository<CurriculumBoard, int>().AnyAsync(t => t.Id == qualification.BoardId))
            {
                return NotFound<int>();
            }

            if (Copied<int>(entity.LegacyId) is { } refused)
            {
                return refused;
            }

            if (await repository.AnyAsync(t => t.BoardId == qualification.BoardId && t.Slug == qualification.Slug && t.Id != qualification.Id))
            {
                return Duplicate<int>("slug");
            }

            entity.BoardId = qualification.BoardId;
            entity.Slug = qualification.Slug;
            entity.Name = qualification.Name!.Trim();
            entity.ShortName = Trim(qualification.ShortName);
            entity.LanguageId = qualification.LanguageId;
            entity.ListOrder = qualification.ListOrder;
            Touch(entity, qualification.Id, userId, repository);
            _ = await uow.SaveChangesAsync();
            if (MirrorOn)
            {
                await CurriculumMirror.MirrorQualificationAsync(uow, entity);
                _ = await uow.SaveChangesAsync();
            }

            return Ok(entity.Id);
        });

        public Task<ResultData<bool>> DeleteQualificationAsync(int id) => RunAsync(async uow =>
        {
            var repository = uow.GetRepository<CurriculumQualification, int>();
            var entity = await repository.GetAsync(id);
            if (entity is null)
            {
                return NotFound<bool>();
            }

            if (entity.LegacyId is not null)
            {
                return Refused<bool>("A copied qualification is kept: old links and content point at it.", "legacyId");
            }

            if (await uow.GetRepository<CurriculumSyllabus, int>().AnyAsync(t => t.QualificationId == id)
                || await uow.GetRepository<CurriculumSeriesType, int>().AnyAsync(t => t.QualificationId == id))
            {
                return InUse<bool>();
            }

            repository.Remove(entity);
            _ = await uow.SaveChangesAsync();
            return Ok(true);
        });

        public Task<ResultData<IReadOnlyList<CurriculumSubjectDto>>> GetSubjectsAsync() => RunAsync<IReadOnlyList<CurriculumSubjectDto>>(async uow =>
            Ok<IReadOnlyList<CurriculumSubjectDto>>(await uow.GetRepository<CurriculumSubject, int>().GetManyQueryable().OrderBy(t => t.ListOrder).ThenBy(t => t.Name)
                .Select(t => new CurriculumSubjectDto { Id = t.Id, Slug = t.Slug, Name = t.Name, Picture = t.Picture, ListOrder = t.ListOrder }).ToListAsync()));

        public Task<ResultData<int>> SaveSubjectAsync([NotNull] CurriculumSubjectDto subject) => RunAsync(async uow =>
        {
            if (Invalid<int>(Slug(subject.Slug), Required(subject.Name, 200, "name"), Optional(subject.Picture, 300, "picture")) is { } invalid)
            {
                return invalid;
            }

            var repository = uow.GetRepository<CurriculumSubject, int>();
            var entity = subject.Id == 0 ? new CurriculumSubject() : await repository.GetAsync(subject.Id);
            if (entity is null)
            {
                return NotFound<int>();
            }

            if (await repository.AnyAsync(t => t.Slug == subject.Slug && t.Id != subject.Id))
            {
                return Duplicate<int>("slug");
            }

            entity.Slug = subject.Slug;
            entity.Name = subject.Name!.Trim();
            entity.Picture = Trim(subject.Picture);
            entity.ListOrder = subject.ListOrder;
            if (subject.Id == 0)
            {
                repository.Add(entity);
            }

            _ = await uow.SaveChangesAsync();
            return Ok(entity.Id);
        });

        public Task<ResultData<bool>> DeleteSubjectAsync(int id) => RunAsync(async uow =>
        {
            var repository = uow.GetRepository<CurriculumSubject, int>();
            var entity = await repository.GetAsync(id);
            if (entity is null)
            {
                return NotFound<bool>();
            }

            if (await uow.GetRepository<CurriculumSyllabus, int>().AnyAsync(t => t.SubjectId == id))
            {
                return InUse<bool>();
            }

            repository.Remove(entity);
            _ = await uow.SaveChangesAsync();
            return Ok(true);
        });

        public Task<ResultData<IReadOnlyList<CurriculumSyllabusDto>>> GetSyllabiAsync(int qualificationId) => RunAsync<IReadOnlyList<CurriculumSyllabusDto>>(async uow =>
            Ok<IReadOnlyList<CurriculumSyllabusDto>>(await uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => t.QualificationId == qualificationId)
                .OrderBy(t => t.ListOrder).ThenBy(t => t.Name).Select(t => new CurriculumSyllabusDto
                {
                    Id = t.Id,
                    LegacyId = t.LegacyId,
                    QualificationId = t.QualificationId,
                    SubjectId = t.SubjectId,
                    Code = t.Code,
                    Slug = t.Slug,
                    Name = t.Name,
                    YearFrom = t.YearFrom,
                    YearTo = t.YearTo,
                    Picture = t.Picture,
                    ListOrder = t.ListOrder,
                }).ToListAsync()));

        public Task<ResultData<int>> SaveSyllabusAsync([NotNull] CurriculumSyllabusDto syllabus, long userId) => RunAsync(async uow =>
        {
            if (SwitchRefusal<int>() is { } notReady)
            {
                return notReady;
            }

            if (Invalid<int>(Slug(syllabus.Slug), Required(syllabus.Name, 200, "name"), Optional(syllabus.Code, 30, "code"), Optional(syllabus.Picture, 300, "picture"),
                syllabus.YearFrom is { } from && syllabus.YearTo is { } to && from > to ? "yearTo" : null) is { } invalid)
            {
                return invalid;
            }

            var repository = uow.GetRepository<CurriculumSyllabus, int>();
            var entity = syllabus.Id == 0 ? new CurriculumSyllabus { CreationDate = DateTimeOffset.UtcNow, CreationUserId = userId } : await repository.GetAsync(syllabus.Id);
            if (entity is null || !await uow.GetRepository<CurriculumQualification, int>().AnyAsync(t => t.Id == syllabus.QualificationId)
                || (syllabus.SubjectId is int subject && !await uow.GetRepository<CurriculumSubject, int>().AnyAsync(t => t.Id == subject)))
            {
                return NotFound<int>();
            }

            if (Copied<int>(entity.LegacyId) is { } refused)
            {
                return refused;
            }

            if (await repository.AnyAsync(t => t.QualificationId == syllabus.QualificationId && t.Slug == syllabus.Slug && t.Id != syllabus.Id))
            {
                return Duplicate<int>("slug");
            }

            entity.QualificationId = syllabus.QualificationId;
            entity.SubjectId = syllabus.SubjectId;
            entity.Code = Trim(syllabus.Code);
            entity.Slug = syllabus.Slug;
            entity.Name = syllabus.Name!.Trim();
            entity.YearFrom = syllabus.YearFrom;
            entity.YearTo = syllabus.YearTo;
            entity.Picture = Trim(syllabus.Picture);
            entity.ListOrder = syllabus.ListOrder;
            Touch(entity, syllabus.Id, userId, repository);
            _ = await uow.SaveChangesAsync();
            if (MirrorOn)
            {
                await CurriculumMirror.MirrorSyllabusAsync(uow, entity);
                _ = await uow.SaveChangesAsync();
            }

            return Ok(entity.Id);
        });

        public Task<ResultData<bool>> DeleteSyllabusAsync(int id) => RunAsync(async uow =>
        {
            var repository = uow.GetRepository<CurriculumSyllabus, int>();
            var entity = await repository.GetAsync(id);
            if (entity is null)
            {
                return NotFound<bool>();
            }

            if (entity.LegacyId is not null)
            {
                return Refused<bool>("A copied syllabus is kept: old links and content point at it.", "legacyId");
            }

            if (await uow.GetRepository<CurriculumTopic, int>().AnyAsync(t => t.SyllabusId == id))
            {
                return InUse<bool>();
            }

            repository.Remove(entity);
            _ = await uow.SaveChangesAsync();
            return Ok(true);
        });

        public Task<ResultData<IReadOnlyList<CurriculumPaperDto>>> GetPapersAsync(int syllabusId) => RunAsync<IReadOnlyList<CurriculumPaperDto>>(async uow =>
            Ok<IReadOnlyList<CurriculumPaperDto>>(await uow.GetRepository<CurriculumPaper, int>().GetManyQueryable(t => t.SyllabusId == syllabusId).OrderBy(t => t.ListOrder)
                .Select(t => new CurriculumPaperDto { Id = t.Id, LegacyId = t.LegacyId, Name = t.Name, ShortLabel = t.ShortLabel, Tier = t.Tier, ListOrder = t.ListOrder }).ToListAsync()));

        public Task<ResultData<bool>> SavePapersAsync(int syllabusId, [NotNull] IReadOnlyList<CurriculumPaperDto> papers) => RunAsync(async uow =>
        {
            if (SwitchRefusal<bool>() is { } notReady)
            {
                return notReady;
            }

            if (!await uow.GetRepository<CurriculumSyllabus, int>().AnyAsync(t => t.Id == syllabusId))
            {
                return NotFound<bool>();
            }

            foreach (var paper in papers)
            {
                if (Invalid<bool>(Required(paper.Name, 100, "name"), Required(paper.ShortLabel, 20, "shortLabel"), Optional(paper.Tier, 50, "tier")) is { } invalid)
                {
                    return invalid;
                }
            }

            var repository = uow.GetRepository<CurriculumPaper, int>();
            var existing = await repository.GetManyQueryable(t => t.SyllabusId == syllabusId, tracking: true).ToListAsync();
            if (papers.Any(t => t.Id != 0 && existing.TrueForAll(e => e.Id != t.Id)))
            {
                return NotFound<bool>();
            }

            // Copied papers can't be removed or renamed while the copy still writes them.
            var removed = existing.Where(t => papers.All(p => p.Id != t.Id)).ToList();
            var changed = existing.Where(t => t.LegacyId is not null && papers.Any(p => p.Id == t.Id && (p.Name?.Trim() != t.Name || p.ShortLabel?.Trim() != t.ShortLabel || Trim(p.Tier) != t.Tier)));
            if ((removed.Exists(t => t.LegacyId is not null) || changed.Any()) && CopyRefusal<bool>() is { } refused)
            {
                return refused;
            }

            foreach (var old in removed)
            {
                repository.Remove(old);
            }

            foreach (var paper in papers)
            {
                var row = paper.Id == 0 ? null : existing.Find(t => t.Id == paper.Id);
                if (row is null)
                {
                    row = new CurriculumPaper { SyllabusId = syllabusId };
                    repository.Add(row);
                }

                row.Name = paper.Name!.Trim();
                row.ShortLabel = paper.ShortLabel!.Trim();
                row.Tier = Trim(paper.Tier);
                row.ListOrder = paper.ListOrder;
            }

            _ = await uow.SaveChangesAsync();
            if (MirrorOn)
            {
                foreach (var row in await repository.GetManyQueryable(t => t.SyllabusId == syllabusId, tracking: true).ToListAsync())
                {
                    await CurriculumMirror.MirrorPaperAsync(uow, row);
                }

                _ = await uow.SaveChangesAsync();
            }

            return Ok(true);
        });

        public Task<ResultData<IReadOnlyList<CurriculumDocumentTypeDto>>> GetDocumentTypesAsync() => RunAsync<IReadOnlyList<CurriculumDocumentTypeDto>>(async uow =>
            Ok<IReadOnlyList<CurriculumDocumentTypeDto>>(await uow.GetRepository<CurriculumDocumentType, int>().GetManyQueryable().OrderBy(t => t.ListOrder).ThenBy(t => t.Name)
                .Select(t => new CurriculumDocumentTypeDto { Id = t.Id, LegacyId = t.LegacyId, Code = t.Code, Name = t.Name, ShortLabel = t.ShortLabel, Icon = t.Icon, ListOrder = t.ListOrder })
                .ToListAsync()));

        public Task<ResultData<int>> SaveDocumentTypeAsync([NotNull] CurriculumDocumentTypeDto documentType) => RunAsync(async uow =>
        {
            if (Invalid<int>(Key(documentType.Code, "code"), Required(documentType.Name, 100, "name"), Optional(documentType.ShortLabel, 20, "shortLabel"),
                Optional(documentType.Icon, 100, "icon")) is { } invalid)
            {
                return invalid;
            }

            var repository = uow.GetRepository<CurriculumDocumentType, int>();
            var entity = documentType.Id == 0 ? new CurriculumDocumentType() : await repository.GetAsync(documentType.Id);
            if (entity is null)
            {
                return NotFound<int>();
            }

            if (Copied<int>(entity.LegacyId) is { } refused)
            {
                return refused;
            }

            if (await repository.AnyAsync(t => t.Code == documentType.Code && t.Id != documentType.Id))
            {
                return Duplicate<int>("code");
            }

            entity.Code = documentType.Code;
            entity.Name = documentType.Name!.Trim();
            entity.ShortLabel = Trim(documentType.ShortLabel);
            entity.Icon = Trim(documentType.Icon);
            entity.ListOrder = documentType.ListOrder;
            if (documentType.Id == 0)
            {
                repository.Add(entity);
            }

            _ = await uow.SaveChangesAsync();
            return Ok(entity.Id);
        });

        public Task<ResultData<bool>> DeleteDocumentTypeAsync(int id) => RunAsync(async uow =>
        {
            var repository = uow.GetRepository<CurriculumDocumentType, int>();
            var entity = await repository.GetAsync(id);
            if (entity is null)
            {
                return NotFound<bool>();
            }

            if (entity.LegacyId is not null)
            {
                return Refused<bool>("A copied document type is kept: paper files point at it.", "legacyId");
            }

            repository.Remove(entity);
            _ = await uow.SaveChangesAsync();
            return Ok(true);
        });

        #endregion

        #region Topics and links

        public Task<ResultData<IReadOnlyList<CurriculumTopicDto>>> GetTopicsAsync(int syllabusId) => RunAsync<IReadOnlyList<CurriculumTopicDto>>(async uow =>
        {
            var topics = await uow.GetRepository<CurriculumTopic, int>().GetManyQueryable(t => t.SyllabusId == syllabusId).ToListAsync();
            List<CurriculumTopicDto> ordered = [];
            foreach (var unit in topics.Where(t => t.ParentTopicId is null).OrderBy(t => t.ListOrder).ThenBy(t => t.Id))
            {
                ordered.Add(ToDto(unit));
                ordered.AddRange(topics.Where(t => t.ParentTopicId == unit.Id).OrderBy(t => t.ListOrder).ThenBy(t => t.Id).Select(ToDto));
            }

            // A chapter whose unit is gone (or in another syllabus) still shows, last.
            HashSet<int> shown = [.. ordered.Select(t => t.Id)];
            ordered.AddRange(topics.Where(t => !shown.Contains(t.Id)).OrderBy(t => t.ListOrder).ThenBy(t => t.Id).Select(ToDto));
            return Ok<IReadOnlyList<CurriculumTopicDto>>(ordered);
        });

        public Task<ResultData<int>> SaveTopicAsync([NotNull] CurriculumTopicDto topic, long userId) => RunAsync(async uow =>
        {
            if (SwitchRefusal<int>() is { } notReady)
            {
                return notReady;
            }

            if (Invalid<int>(Required(topic.Name, 500, "name")) is { } invalid)
            {
                return invalid;
            }

            var repository = uow.GetRepository<CurriculumTopic, int>();
            var entity = topic.Id == 0 ? new CurriculumTopic { CreationDate = DateTimeOffset.UtcNow, CreationUserId = userId } : await repository.GetAsync(topic.Id);
            if (entity is null || !await uow.GetRepository<CurriculumSyllabus, int>().AnyAsync(t => t.Id == topic.SyllabusId))
            {
                return NotFound<int>();
            }

            if (Copied<int>(entity.LegacyId) is { } refused)
            {
                return refused;
            }

            // Two levels, as the syllabuses have: a unit, and its chapters in the same syllabus.
            if (topic.ParentTopicId is int parent
                && (parent == topic.Id || !await repository.AnyAsync(t => t.Id == parent && t.SyllabusId == topic.SyllabusId && t.ParentTopicId == null)))
            {
                return Refused<int>("A chapter's parent must be a unit of the same syllabus.", "parentTopicId");
            }

            if (topic.Id != 0 && topic.ParentTopicId is not null && await repository.AnyAsync(t => t.ParentTopicId == topic.Id))
            {
                return Refused<int>("A unit with chapters can't become a chapter.", "parentTopicId");
            }

            entity.SyllabusId = topic.SyllabusId;
            entity.ParentTopicId = topic.ParentTopicId;
            entity.Name = topic.Name!.Trim();
            entity.IsUnit = topic.ParentTopicId is null && topic.IsUnit;
            entity.ListOrder = topic.ListOrder;
            Touch(entity, topic.Id, userId, repository);
            _ = await uow.SaveChangesAsync();
            if (MirrorOn)
            {
                await CurriculumMirror.MirrorTopicsAsync(uow, entity.SyllabusId);
                _ = await uow.SaveChangesAsync();
            }

            return Ok(entity.Id);
        });

        public Task<ResultData<bool>> DeleteTopicAsync(int id) => RunAsync(async uow =>
        {
            var repository = uow.GetRepository<CurriculumTopic, int>();
            var entity = await repository.GetAsync(id);
            if (entity is null)
            {
                return NotFound<bool>();
            }

            if (entity.LegacyId is not null)
            {
                return Refused<bool>("A copied topic is kept: content is tagged with it.", "legacyId");
            }

            if (await repository.AnyAsync(t => t.ParentTopicId == id)
                || await uow.GetRepository<CurriculumTopicLink, int>().AnyAsync(t => t.MasterTopicId == id || t.TopicId == id))
            {
                return InUse<bool>();
            }

            repository.Remove(entity);
            _ = await uow.SaveChangesAsync();
            return Ok(true);
        });

        public Task<ResultData<IReadOnlyList<CurriculumTopicLinkDto>>> GetTopicLinksAsync(int topicId) => RunAsync<IReadOnlyList<CurriculumTopicLinkDto>>(async uow =>
        {
            var links = await uow.GetRepository<CurriculumTopicLink, int>().GetManyQueryable(t => t.MasterTopicId == topicId || t.TopicId == topicId).ToListAsync();
            var places = await TopicPlacesAsync(uow, [.. links.SelectMany(t => new[] { t.MasterTopicId, t.TopicId })]);
            return Ok<IReadOnlyList<CurriculumTopicLinkDto>>([.. links
                .Select(t => new CurriculumTopicLinkDto
                {
                    Id = t.Id,
                    MasterTopicId = t.MasterTopicId,
                    TopicId = t.TopicId,
                    Kind = t.Kind,
                    MasterTopicName = places.GetValueOrDefault(t.MasterTopicId)?.TopicName,
                    TopicName = places.GetValueOrDefault(t.TopicId)?.TopicName,
                    TopicBoardId = places.GetValueOrDefault(t.TopicId)?.BoardId ?? 0,
                    TopicBoardName = places.GetValueOrDefault(t.TopicId)?.BoardName,
                    TopicSyllabusName = places.GetValueOrDefault(t.TopicId)?.SyllabusName,
                    LegacyRelationId = t.LegacyRelationId,
                    CreationDate = t.CreationDate,
                })
                .OrderBy(t => t.TopicBoardName).ThenBy(t => t.TopicName)]);
        });

        public Task<ResultData<int>> AddTopicLinkAsync([NotNull] CurriculumTopicLinkAddDto link) => RunAsync(async uow =>
        {
            var places = await TopicPlacesAsync(uow, [link.MasterTopicId, link.TopicId]);
            if (!places.TryGetValue(link.MasterTopicId, out var master) || !places.TryGetValue(link.TopicId, out var other))
            {
                return NotFound<int>();
            }

            // Links only go through GAMA: one side on the master board, the other on another board.
            if (!master.IsMaster || other.IsMaster)
            {
                return Refused<int>("A link joins a GAMA topic (masterTopicId) and another board's topic (topicId).", "masterTopicId");
            }

            var repository = uow.GetRepository<CurriculumTopicLink, int>();
            if (await repository.AnyAsync(t => t.MasterTopicId == link.MasterTopicId && t.TopicId == link.TopicId))
            {
                return Duplicate<int>("topicId");
            }

            CurriculumTopicLink entity = new()
            {
                MasterTopicId = link.MasterTopicId,
                TopicId = link.TopicId,
                Kind = link.Kind ?? CurriculumLinkKind.Exact,
                CreationDate = DateTimeOffset.UtcNow,
                CreationUserId = link.UserId,
            };
            repository.Add(entity);
            _ = await uow.SaveChangesAsync();
            return Ok(entity.Id);
        });

        public Task<ResultData<bool>> DeleteTopicLinkAsync(int id) => RunAsync(async uow =>
        {
            var repository = uow.GetRepository<CurriculumTopicLink, int>();
            var entity = await repository.GetAsync(id);
            if (entity is null)
            {
                return NotFound<bool>();
            }

            if (Copied<bool>(entity.LegacyRelationId) is { } refused)
            {
                return refused;
            }

            repository.Remove(entity);
            _ = await uow.SaveChangesAsync();
            return Ok(true);
        });

        #endregion

        #region SEO templates

        public Task<ResultData<IReadOnlyList<CurriculumSeoTemplateDto>>> GetSeoTemplatesAsync(int? boardId) => RunAsync<IReadOnlyList<CurriculumSeoTemplateDto>>(async uow =>
            Ok<IReadOnlyList<CurriculumSeoTemplateDto>>([.. (await uow.GetRepository<CurriculumSeoTemplate, int>().GetManyQueryable(t => t.BoardId == boardId).ToListAsync())
                .OrderBy(t => t.Page!.Value).Select(ToDto)]));

        public Task<ResultData<int>> SaveSeoTemplateAsync([NotNull] CurriculumSeoTemplateDto template, long userId) => RunAsync(async uow =>
        {
            if (template.Page is null)
            {
                return Refused<int>("Choose the page the template is for.", "page");
            }

            if (Invalid<int>(Required(template.TitleTemplate, 500, "titleTemplate"), Required(template.DescriptionTemplate, 1000, "descriptionTemplate")) is { } invalid)
            {
                return invalid;
            }

            if (template.BoardId is int board && !await uow.GetRepository<CurriculumBoard, int>().AnyAsync(t => t.Id == board))
            {
                return NotFound<int>();
            }

            var repository = uow.GetRepository<CurriculumSeoTemplate, int>();
            var entity = template.Id == 0 ? new CurriculumSeoTemplate() : await repository.GetAsync(template.Id);
            if (entity is null)
            {
                return NotFound<int>();
            }

            if (await repository.AnyAsync(t => t.BoardId == template.BoardId && t.Page == template.Page && t.Id != template.Id))
            {
                return Duplicate<int>("page");
            }

            entity.BoardId = template.BoardId;
            entity.Page = template.Page;
            entity.TitleTemplate = template.TitleTemplate!.Trim();
            entity.DescriptionTemplate = template.DescriptionTemplate!.Trim();
            entity.LastModifyDate = DateTimeOffset.UtcNow;
            entity.LastModifyUserId = userId;
            if (template.Id == 0)
            {
                repository.Add(entity);
            }

            _ = await uow.SaveChangesAsync();
            return Ok(entity.Id);
        });

        public Task<ResultData<bool>> DeleteSeoTemplateAsync(int id) => RunAsync(async uow =>
        {
            var repository = uow.GetRepository<CurriculumSeoTemplate, int>();
            var entity = await repository.GetAsync(id);
            if (entity is null)
            {
                return NotFound<bool>();
            }

            if (entity.BoardId is null)
            {
                return Refused<bool>("A default template can be edited but not removed: every board's pages use it.", "boardId");
            }

            repository.Remove(entity);
            _ = await uow.SaveChangesAsync();
            return Ok(true);
        });

        #endregion

        #region Helpers

        internal sealed record TopicPlace(string? TopicName, int SyllabusId, string? SyllabusName, int BoardId, string? BoardName, bool IsMaster);

        /// <summary>Each topic's syllabus and board, by topic id.</summary>
        internal static async Task<Dictionary<int, TopicPlace>> TopicPlacesAsync(IUnitOfWork uow, IReadOnlyCollection<int> topicIds)
        {
            List<int> ids = [.. topicIds.Distinct()];
            var topics = await uow.GetRepository<CurriculumTopic, int>().GetManyQueryable(t => ids.Contains(t.Id)).Select(t => new { t.Id, t.Name, t.SyllabusId }).ToListAsync();
            List<int> syllabusIds = [.. topics.Select(t => t.SyllabusId).Distinct()];
            var syllabi = await uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => syllabusIds.Contains(t.Id)).Select(t => new { t.Id, t.Name, t.QualificationId }).ToListAsync();
            List<int> qualificationIds = [.. syllabi.Select(t => t.QualificationId).Distinct()];
            var qualifications = await uow.GetRepository<CurriculumQualification, int>().GetManyQueryable(t => qualificationIds.Contains(t.Id)).Select(t => new { t.Id, t.BoardId }).ToListAsync();
            List<int> boardIds = [.. qualifications.Select(t => t.BoardId).Distinct()];
            var boards = await uow.GetRepository<CurriculumBoard, int>().GetManyQueryable(t => boardIds.Contains(t.Id)).Select(t => new { t.Id, t.Name, t.IsMaster }).ToListAsync();

            Dictionary<int, TopicPlace> places = [];
            foreach (var topic in topics)
            {
                var syllabus = syllabi.Find(t => t.Id == topic.SyllabusId);
                var qualification = syllabus is null ? null : qualifications.Find(t => t.Id == syllabus.QualificationId);
                var board = qualification is null ? null : boards.Find(t => t.Id == qualification.BoardId);
                if (board is not null)
                {
                    places[topic.Id] = new(topic.Name, topic.SyllabusId, syllabus?.Name, board.Id, board.Name, board.IsMaster);
                }
            }

            return places;
        }

        private async Task<ResultData<T>> RunAsync<T>(Func<IUnitOfWork, Task<ResultData<T>>> action)
        {
            try
            {
                return await action(UnitOfWorkProvider.Value.CreateUnitOfWork());
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message }] };
            }
        }

        // The curriculum's writes can't be switched before the lookup lists' (gama-api's copy could bring the ids it makes).
        private ResultData<T>? SwitchRefusal<T>() =>
            migrationRouter.Value.GetWriteRoute(MigrationDomain.Curriculum) == MigrationRoute.Native
                && migrationRouter.Value.GetWriteRoute(MigrationDomain.Types) != MigrationRoute.Native
                ? new(OperationResult.NotValid)
                {
                    Errors = [new() { Message = "Set Migration:WriteRoutes:Types to Native before WriteRoutes:Curriculum: the lists' ids must be made here only.", Reference = "typesNotNative" }],
                }
                : null;

        // After the write switch, what is saved here also gets lookup ids (CurriculumMirror), so content and forms can use it.
        private bool MirrorOn => migrationRouter.Value.GetWriteRoute(MigrationDomain.Curriculum) == MigrationRoute.Native
            && migrationRouter.Value.GetWriteRoute(MigrationDomain.Types) == MigrationRoute.Native;

        // A copied row is refused while the copy still writes it (it would overwrite the edit).
        private ResultData<T>? Copied<T>(int? legacyId) => legacyId is null ? null : CopyRefusal<T>();

        private ResultData<T>? CopyRefusal<T>() =>
            migrationRouter.Value.GetWriteRoute(MigrationDomain.Curriculum) == MigrationRoute.Native
                ? null
                : new(OperationResult.NotValid)
                {
                    Errors = [new() { Message = "This row is copied from the lookup lists; it is edited there until Migration:WriteRoutes:Curriculum is Native.", Reference = "notNative" }],
                };

        private static void Touch<TEntity>(TEntity entity, int id, long userId, Common.DataAccess.Repositories.IRepository<TEntity, int> repository)
            where TEntity : class, Common.DataAccess.Entities.IEntity<TEntity, int>
        {
            (DateTimeOffset? Date, long? User) stamp = id == 0 ? default : (DateTimeOffset.UtcNow, userId);
            switch (entity)
            {
                case CurriculumBoard t:
                    (t.LastModifyDate, t.LastModifyUserId) = stamp;
                    break;
                case CurriculumQualification t:
                    (t.LastModifyDate, t.LastModifyUserId) = stamp;
                    break;
                case CurriculumSyllabus t:
                    (t.LastModifyDate, t.LastModifyUserId) = stamp;
                    break;
                case CurriculumTopic t:
                    (t.LastModifyDate, t.LastModifyUserId) = stamp;
                    break;
                default:
                    break;
            }

            if (id == 0)
            {
                repository.Add(entity);
            }
        }

        private static ResultData<T>? Invalid<T>(params string?[] fields) =>
            fields.FirstOrDefault(t => t is not null) is { } field
                ? new(OperationResult.NotValid) { Errors = [new() { Message = $"Check {field}.", Reference = field }] }
                : null;

        private static string? Required(string? value, int max, string field) => string.IsNullOrWhiteSpace(value) || value.Trim().Length > max ? field : null;

        private static string? Optional(string? value, int max, string field) => value is not null && value.Trim().Length > max ? field : null;

        private static string? Slug(string? value) => value is { Length: <= 100 } && SlugPattern().IsMatch(value) ? null : "slug";

        private static string? Key(string? value, string field = "key") => value is { Length: <= 20 } && KeyPattern().IsMatch(value) ? null : field;

        private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static ResultData<T> Ok<T>(T data) => new(OperationResult.Succeeded) { Data = data };

        private static ResultData<T> NotFound<T>() => new(OperationResult.NotFound) { Errors = [new() { Message = "Not found.", Reference = "notFound" }] };

        private static ResultData<T> Duplicate<T>(string field) => new(OperationResult.Duplicate) { Errors = [new() { Message = $"This {field} is already used.", Reference = field }] };

        private static ResultData<T> InUse<T>() => new(OperationResult.NotValid) { Errors = [new() { Message = "Still in use: remove what points at it first.", Reference = "inUse" }] };

        private static ResultData<T> Refused<T>(string message, string field) => new(OperationResult.NotValid) { Errors = [new() { Message = message, Reference = field }] };

        private static CurriculumBoardDto ToDto(CurriculumBoard t) => new()
        {
            Id = t.Id,
            LegacyId = t.LegacyId,
            Slug = t.Slug,
            Name = t.Name,
            ShortName = t.ShortName,
            CountryCode = t.CountryCode,
            ParentBoardId = t.ParentBoardId,
            Logo = t.Logo,
            Website = t.Website,
            IsMaster = t.IsMaster,
            IsDefault = t.IsDefault,
            IsEnabled = t.IsEnabled,
            ListOrder = t.ListOrder,
            CodeTemplate = t.CodeTemplate,
            LanguageId = t.LanguageId,
        };

        private static CurriculumSeoTemplateDto ToDto(CurriculumSeoTemplate t) => new()
        {
            Id = t.Id,
            BoardId = t.BoardId,
            Page = t.Page,
            TitleTemplate = t.TitleTemplate,
            DescriptionTemplate = t.DescriptionTemplate,
        };

        private static CurriculumTopicDto ToDto(CurriculumTopic t) => new()
        {
            Id = t.Id,
            LegacyId = t.LegacyId,
            SyllabusId = t.SyllabusId,
            ParentTopicId = t.ParentTopicId,
            Name = t.Name,
            IsUnit = t.IsUnit,
            ListOrder = t.ListOrder,
        };

        [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
        private static partial Regex SlugPattern();

        [GeneratedRegex("^[a-z][a-z0-9_]*$")]
        private static partial Regex KeyPattern();

        [GeneratedRegex("^[A-Za-z]{2}$")]
        private static partial Regex CountryCode();

        [GeneratedRegex("^(?:[1-9]|1[0-2])(?:,(?:[1-9]|1[0-2]))*$")]
        private static partial Regex Months();

        #endregion
    }
}
