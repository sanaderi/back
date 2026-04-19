namespace GamaEdtech.Application.Service
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Core.Extensions.Linq;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Common.DataAccess.UnitOfWork;
    using GamaEdtech.Common.Service;
    using GamaEdtech.Data.Dto.ContentLocalization;
    using GamaEdtech.Domain.Entity;

    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;

    using static GamaEdtech.Common.Core.Constants;

    public class ContentLocalizationService(Lazy<IUnitOfWorkProvider> unitOfWorkProvider, Lazy<IHttpContextAccessor> httpContextAccessor, Lazy<IStringLocalizer<ContentLocalizationService>> localizer
        , Lazy<ILogger<ContentLocalizationService>> logger)
        : LocalizableServiceBase<ContentLocalizationService>(unitOfWorkProvider, httpContextAccessor, localizer, logger), IContentLocalizationService
    {
        public async Task<ResultData<ListDataSource<ContentLocalizationsDto>>> GetContentLocalizationsAsync(ListRequestDto<ContentLocalization>? requestDto = null)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var result = await uow.GetRepository<ContentLocalization>().GetManyQueryable(requestDto?.Specification).FilterListAsync(requestDto?.PagingDto);
                var lst = await result.List.Select(t => new ContentLocalizationsDto
                {
                    Id = t.Id,
                    ContentId = t.ContentId,
                    ContentType = t.ContentType,
                    LanguageId = t.LanguageId,
                    Name = t.Name,
                }).ToListAsync();
                return new(OperationResult.Succeeded) { Data = new() { List = lst, TotalRecordsCount = result.TotalRecordsCount } };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message },] };
            }
        }

        public async Task<ResultData<ContentLocalizationDto>> GetContentLocalizationAsync([NotNull] ISpecification<ContentLocalization> specification)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var contentLocalization = await uow.GetRepository<ContentLocalization>().GetManyQueryable(specification).Select(t => new ContentLocalizationDto
                {
                    Id = t.Id,
                    ContentId = t.ContentId,
                    ContentType = t.ContentType,
                    LanguageId = t.LanguageId,
                    Name = t.Name,
                    Value = t.Value,
                }).FirstOrDefaultAsync();

                return contentLocalization is null
                    ? new(OperationResult.NotFound)
                    {
                        Errors = [new() { Message = Localizer.Value["ContentLocalizationNotFound"] },],
                    }
                    : new(OperationResult.Succeeded) { Data = contentLocalization };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message },] };
            }
        }

        public async Task<ResultData<long>> ManageContentLocalizationAsync([NotNull] ManageContentLocalizationRequestDto requestDto)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var repository = uow.GetRepository<ContentLocalization>();
                ContentLocalization? contentLocalization = null;

                if (requestDto.Id.HasValue)
                {
                    contentLocalization = await repository.GetAsync(requestDto.Id.Value);
                    if (contentLocalization is null)
                    {
                        return new(OperationResult.NotFound)
                        {
                            Errors = [new() { Message = Localizer.Value["ContentLocalizationNotFound"] },],
                        };
                    }

                    contentLocalization.ContentId = requestDto.ContentId;
                    contentLocalization.ContentType = requestDto.ContentType;
                    contentLocalization.LanguageId = requestDto.LanguageId;
                    contentLocalization.Name = requestDto.Name;
                    contentLocalization.Value = requestDto.Value;

                    _ = repository.Update(contentLocalization);
                }
                else
                {
                    contentLocalization = await repository.GetAsync(t => t.LanguageId == requestDto.LanguageId && t.ContentType == requestDto.ContentType && t.ContentId == requestDto.ContentId && t.Name == requestDto.Name);
                    if (contentLocalization is null)
                    {

                        contentLocalization = new ContentLocalization
                        {
                            ContentId = requestDto.ContentId,
                            ContentType = requestDto.ContentType,
                            LanguageId = requestDto.LanguageId,
                            Name = requestDto.Name,
                            Value = requestDto.Value,
                        };
                        repository.Add(contentLocalization);
                    }
                    else
                    {
                        contentLocalization.Value = requestDto.Value;

                        _ = repository.Update(contentLocalization);
                    }
                }

                _ = await uow.SaveChangesAsync();

                return new(OperationResult.Succeeded) { Data = contentLocalization.Id };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message, }] };
            }
        }

        public async Task<ResultData<bool>> RemoveContentLocalizationAsync([NotNull] ISpecification<ContentLocalization> specification)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var repository = uow.GetRepository<ContentLocalization>();
                var contentLocalization = await repository.GetAsync(specification);
                if (contentLocalization is null)
                {
                    return new(OperationResult.NotFound)
                    {
                        Data = false,
                        Errors = [new() { Message = Localizer.Value["ContentLocalizationNotFound"] },],
                    };
                }

                repository.Remove(contentLocalization);
                _ = await uow.SaveChangesAsync();
                return new(OperationResult.Succeeded) { Data = true };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message, },] };
            }
        }

        public async Task<ResultData<List<LocalizedValueDto>>> GetLocalizedValuesAsync([NotNull] LocalizedValuesRequestDto requestDto)
        {
            try
            {
                var languageId = HttpContextAccessor.Value.HttpContext?.Request.Headers.AcceptLanguage.ToString().ValueOf<int?>();
                if (!languageId.HasValue)
                {
                    return new(OperationResult.NotFound);
                }

                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var lst = await uow.GetRepository<ContentLocalization>().GetManyQueryable(t => t.LanguageId == languageId.Value && t.ContentType == requestDto.ContentType && requestDto.ContentIds.Contains(t.ContentId)).Select(t => new LocalizedValueDto
                {
                    ContentId = t.ContentId,
                    Name = t.Name,
                    Value = t.Value,
                }).ToListAsync();

                return new(OperationResult.Succeeded) { Data = lst };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message },] };
            }
        }
    }
}
