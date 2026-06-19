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
    using GamaEdtech.Data.Dto.Experience;
    using GamaEdtech.Domain.Entity;

    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;

    using static GamaEdtech.Common.Core.Constants;

    public class ExperienceService(Lazy<IUnitOfWorkProvider> unitOfWorkProvider, Lazy<IHttpContextAccessor> httpContextAccessor, Lazy<IStringLocalizer<ExperienceService>> localizer, Lazy<ILogger<ExperienceService>> logger)
        : LocalizableServiceBase<ExperienceService>(unitOfWorkProvider, httpContextAccessor, localizer, logger), IExperienceService
    {
        public async Task<ResultData<ListDataSource<ExperienceDto>>> GetExperiencesAsync(ListRequestDto<Experience>? requestDto = null)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var result = await uow.GetRepository<Experience>().GetManyQueryable(requestDto?.Specification).FilterListAsync(requestDto?.PagingDto);
                var lst = await result.List.Select(t => new ExperienceDto
                {
                    Id = t.Id,
                    SchoolId = t.SchoolId,
                    SchoolTitle = t.School.Name,
                    Description = t.Description,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                }).ToListAsync();
                return new(OperationResult.Succeeded) { Data = new() { List = lst, TotalRecordsCount = result.TotalRecordsCount } };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message },] };
            }
        }

        public async Task<ResultData<ExperienceDto>> GetExperienceAsync([NotNull] ISpecification<Experience> specification)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var experience = await uow.GetRepository<Experience>().GetManyQueryable(specification).Select(t => new ExperienceDto
                {
                    Id = t.Id,
                    SchoolId = t.SchoolId,
                    SchoolTitle = t.School.Name,
                    Description = t.Description,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                }).FirstOrDefaultAsync();

                return experience is null
                    ? new(OperationResult.NotFound)
                    {
                        Errors = [new() { Message = Localizer.Value["ExperienceNotFound"] },],
                    }
                    : new(OperationResult.Succeeded) { Data = experience };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message },] };
            }
        }

        public async Task<ResultData<long>> ManageExperienceAsync([NotNull] ManageExperienceRequestDto requestDto)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var repository = uow.GetRepository<Experience>();
                Experience? experience = null;

                if (requestDto.Id.HasValue)
                {
                    experience = await repository.GetAsync(t => t.Id == requestDto.Id.Value && t.UserId == requestDto.UserId);
                    if (experience is null)
                    {
                        return new(OperationResult.NotFound)
                        {
                            Errors = [new() { Message = Localizer.Value["ExperienceNotFound"] },],
                        };
                    }

                    experience.SchoolId = requestDto.SchoolId;
                    experience.Description = requestDto.Description;
                    experience.StartDate = requestDto.StartDate;
                    experience.EndDate = requestDto.EndDate;

                    _ = repository.Update(experience);
                }
                else
                {
                    experience = new Experience
                    {
                        UserId = requestDto.UserId,
                        SchoolId = requestDto.SchoolId,
                        Description = requestDto.Description,
                        StartDate = requestDto.StartDate,
                        EndDate = requestDto.EndDate,
                    };
                    repository.Add(experience);
                }

                _ = await uow.SaveChangesAsync();

                return new(OperationResult.Succeeded) { Data = experience.Id };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message, }] };
            }
        }

        public async Task<ResultData<bool>> RemoveExperienceAsync([NotNull] ISpecification<Experience> specification)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var repository = uow.GetRepository<Experience>();
                var experience = await repository.GetAsync(specification);
                if (experience is null)
                {
                    return new(OperationResult.NotFound)
                    {
                        Data = false,
                        Errors = [new() { Message = Localizer.Value["ExperienceNotFound"] },],
                    };
                }

                repository.Remove(experience);
                _ = await uow.SaveChangesAsync();
                return new(OperationResult.Succeeded) { Data = true };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message, },] };
            }
        }
    }
}
