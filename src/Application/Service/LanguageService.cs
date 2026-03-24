namespace GamaEdtech.Application.Service
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using EntityFramework.Exceptions.Common;

    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Core.Extensions.Linq;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Common.DataAccess.UnitOfWork;
    using GamaEdtech.Common.Localization;
    using GamaEdtech.Common.Service;
    using GamaEdtech.Data.Dto.Language;

    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;

    using static GamaEdtech.Common.Core.Constants;

    public class LanguageService(Lazy<IUnitOfWorkProvider> unitOfWorkProvider, Lazy<IHttpContextAccessor> httpContextAccessor, Lazy<IStringLocalizer<LanguageService>> localizer, Lazy<ILogger<LanguageService>> logger)
        : LocalizableServiceBase<LanguageService>(unitOfWorkProvider, httpContextAccessor, localizer, logger), Interface.ILanguageService
    {
        public async Task<ResultData<ListDataSource<LanguageDto>>> GetLanguagesAsync(ListRequestDto<Language>? requestDto = null)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var result = await uow.GetRepository<Language, int>().GetManyQueryable(requestDto?.Specification).FilterListAsync(requestDto?.PagingDto);
                var lst = await result.List.Select(t => new LanguageDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Code = t.Code,
                    Icon = t.Icon,
                    IsEnable = t.IsEnable,
                    IsDefault = t.IsDefault,
                }).ToListAsync();
                return new(OperationResult.Succeeded) { Data = new() { List = lst, TotalRecordsCount = result.TotalRecordsCount } };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message },] };
            }
        }

        public async Task<ResultData<List<LanguageDto>>> GetActiveLanguagesAsync()
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var lst = await uow.GetRepository<Language, int>().GetManyQueryable(t => t.IsEnable).Select(t => new LanguageDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Code = t.Code,
                    Icon = t.Icon,
                    IsEnable = t.IsEnable,
                    IsDefault = t.IsDefault,
                }).ToListAsync();
                return new(OperationResult.Succeeded) { Data = lst };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message },] };
            }
        }

        public async Task<ResultData<LanguageDto>> GetLanguageAsync([NotNull] ISpecification<Language> specification)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var language = await uow.GetRepository<Language, int>().GetManyQueryable(specification).Select(t => new LanguageDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Code = t.Code,
                    Icon = t.Icon,
                    IsEnable = t.IsEnable,
                    IsDefault = t.IsDefault,
                }).FirstOrDefaultAsync();

                return language is null
                    ? new(OperationResult.NotFound)
                    {
                        Errors = [new() { Message = Localizer.Value["LanguageNotFound"] },],
                    }
                    : new(OperationResult.Succeeded) { Data = language };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message },] };
            }
        }

        public async Task<ResultData<int>> ManageLanguageAsync([NotNull] ManageLanguageRequestDto requestDto)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                using var trn = uow.CreateTransactionScope();

                var repository = uow.GetRepository<Language, int>();
                Language? language = null;

                if (requestDto.IsDefault)
                {
                    _ = await repository.GetManyQueryable().ExecuteUpdateAsync(t => t.SetProperty(p => p.IsDefault, false));
                }

                if (requestDto.Id.HasValue)
                {
                    language = await repository.GetAsync(requestDto.Id.Value);
                    if (language is null)
                    {
                        return new(OperationResult.NotFound)
                        {
                            Errors = [new() { Message = Localizer.Value["LanguageNotFound"] },],
                        };
                    }

                    language.Name = requestDto.Name;
                    language.Code = requestDto.Code;
                    language.Icon = requestDto.Icon;
                    language.IsEnable = requestDto.IsEnable;
                    language.IsDefault = requestDto.IsDefault;

                    _ = repository.Update(language);
                }
                else
                {
                    language = new Language
                    {
                        Name = requestDto.Name,
                        Code = requestDto.Code,
                        Icon = requestDto.Icon,
                        IsEnable = requestDto.IsEnable,
                        IsDefault = requestDto.IsDefault,
                    };
                    repository.Add(language);
                }

                _ = await uow.SaveChangesAsync();
                trn.Complete();

                return new(OperationResult.Succeeded) { Data = language.Id };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message, }] };
            }
        }

        public async Task<ResultData<bool>> RemoveLanguageAsync([NotNull] ISpecification<Language> specification)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var repository = uow.GetRepository<Language, int>();
                var language = await repository.GetAsync(specification);
                if (language is null)
                {
                    return new(OperationResult.NotFound)
                    {
                        Data = false,
                        Errors = [new() { Message = Localizer.Value["LanguageNotFound"] },],
                    };
                }

                repository.Remove(language);
                _ = await uow.SaveChangesAsync();
                return new(OperationResult.Succeeded) { Data = true };
            }
            catch (ReferenceConstraintException)
            {
                return new(OperationResult.NotValid) { Errors = [new() { Message = Localizer.Value["LanguageCantBeRemoved"], },] };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message, },] };
            }
        }
    }
}
