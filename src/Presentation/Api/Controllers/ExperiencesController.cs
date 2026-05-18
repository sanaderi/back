namespace GamaEdtech.Presentation.Api.Controllers
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Asp.Versioning;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Core.Extensions.Linq;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Common.DataAccess.Specification.Impl;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.Identity;
    using GamaEdtech.Domain.Entity;
    using GamaEdtech.Domain.Specification;
    using GamaEdtech.Presentation.ViewModel.Experience;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Permission(policy: null)]
    public class ExperiencesController(Lazy<ILogger<ExperiencesController>> logger, Lazy<IExperienceService> experienceService)
        : ApiControllerBase<ExperiencesController>(logger)
    {
        [HttpGet(), Produces(typeof(ApiResponse<ListDataSource<ExperienceResponseViewModel>>))]
        [Display(Name = "Get Experience List")]
        public async Task<IActionResult<ListDataSource<ExperienceResponseViewModel>>> GetExperiences([NotNull, FromQuery] ExperiencesRequestViewModel request)
        {
            try
            {
                var result = await experienceService.Value.GetExperiencesAsync(new()
                {
                    PagingDto = request.PagingDto,
                    Specification = new UserIdEqualsSpecification<Experience, int>(User.UserId()),
                });

                return Ok<ListDataSource<ExperienceResponseViewModel>>(new(result.Errors)
                {
                    Data = result.Data.List is null ? new() : new()
                    {
                        List = result.Data.List.Select(t => new ExperienceResponseViewModel
                        {
                            Id = t.Id,
                            SchoolId = t.SchoolId,
                            SchoolTitle = t.SchoolTitle,
                            Description = t.Description,
                            StartDate = t.StartDate,
                            EndDate = t.EndDate,
                        }),
                        TotalRecordsCount = result.Data.TotalRecordsCount,
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<ListDataSource<ExperienceResponseViewModel>>(new(new Error { Message = exc.Message }));
            }
        }

        [HttpGet("{id:long}"), Produces(typeof(ApiResponse<ExperienceResponseViewModel>))]
        public async Task<IActionResult<ExperienceResponseViewModel>> GetExperience([FromRoute] long id)
        {
            try
            {
                var specification = new IdEqualsSpecification<Experience, long>(id)
                    .And(new UserIdEqualsSpecification<Experience, int>(User.UserId()));
                var result = await experienceService.Value.GetExperienceAsync(specification);

                return Ok<ExperienceResponseViewModel>(new(result.Errors)
                {
                    Data = result.Data is null ? null : new()
                    {
                        Id = result.Data.Id,
                        SchoolTitle = result.Data.SchoolTitle,
                        SchoolId = result.Data.SchoolId,
                        Description = result.Data.Description,
                        StartDate = result.Data.StartDate,
                        EndDate = result.Data.EndDate,
                    },
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<ExperienceResponseViewModel>(new(new Error { Message = exc.Message }));
            }
        }

        [HttpPost, Produces<ApiResponse<ManageExperienceResponseViewModel>>()]
        public async Task<IActionResult<ManageExperienceResponseViewModel>> CreateExperience([NotNull] ManageExperienceRequestViewModel request)
        {
            try
            {
                var result = await experienceService.Value.ManageExperienceAsync(new()
                {
                    UserId = User.UserId(),
                    SchoolId = request.SchoolId.GetValueOrDefault(),
                    Description = request.Description,
                    StartDate = request.StartDate.GetValueOrDefault(),
                    EndDate = request.EndDate,
                });
                return Ok<ManageExperienceResponseViewModel>(new(result.Errors)
                {
                    Data = new() { Id = result.Data, },
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<ManageExperienceResponseViewModel>(new() { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpPut("{id:long}"), Produces<ApiResponse<ManageExperienceResponseViewModel>>()]
        public async Task<IActionResult<ManageExperienceResponseViewModel>> UpdateExperience([FromRoute] long id, [NotNull, FromBody] ManageExperienceRequestViewModel request)
        {
            try
            {
                var result = await experienceService.Value.ManageExperienceAsync(new()
                {
                    Id = id,
                    UserId = User.UserId(),
                    SchoolId = request.SchoolId.GetValueOrDefault(),
                    Description = request.Description,
                    StartDate = request.StartDate.GetValueOrDefault(),
                    EndDate = request.EndDate,
                });
                return Ok<ManageExperienceResponseViewModel>(new(result.Errors)
                {
                    Data = new() { Id = result.Data, },
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<ManageExperienceResponseViewModel>(new() { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpDelete("{id:long}"), Produces<ApiResponse<bool>>()]
        public async Task<IActionResult<bool>> RemoveExperience([FromRoute] long id)
        {
            try
            {
                var specification = new IdEqualsSpecification<Experience, long>(id)
                    .And(new UserIdEqualsSpecification<Experience, int>(User.UserId()));
                var result = await experienceService.Value.RemoveExperienceAsync(specification);
                return Ok<bool>(new(result.Errors)
                {
                    Data = result.Data
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<bool>(new() { Errors = [new() { Message = exc.Message }] });
            }
        }
    }
}

