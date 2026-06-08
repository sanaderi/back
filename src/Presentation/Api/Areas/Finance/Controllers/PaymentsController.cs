namespace GamaEdtech.Presentation.Api.Areas.Finance.Controllers
{
    using System.Diagnostics.CodeAnalysis;

    using Asp.Versioning;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Common.Identity;
    using GamaEdtech.Domain.Entity;
    using GamaEdtech.Domain.Enumeration;
    using GamaEdtech.Domain.Specification;
    using GamaEdtech.Domain.Specification.Payment;
    using GamaEdtech.Presentation.ViewModel.Payment;

    using Microsoft.AspNetCore.Mvc;

    [Common.DataAnnotation.Area(nameof(Finance), "Finance")]
    [Route("api/v{version:apiVersion}/[area]/[controller]")]
    [ApiVersion("1.0")]
    [Permission(Roles = [nameof(Role.Finance)])]
    public class PaymentsController(Lazy<ILogger<PaymentsController>> logger, Lazy<IPaymentService> paymentService)
        : ApiControllerBase<PaymentsController>(logger)
    {
        [HttpGet("summary"), Produces<ApiResponse<IEnumerable<PaymentsSummaryResponseViewModel>>>()]
        public async Task<IActionResult<IEnumerable<PaymentsSummaryResponseViewModel>>> GetPaymentsSummary([NotNull, FromQuery] PaymentsSummaryRequestViewModel request)
        {
            try
            {
                ISpecification<Payment>? specification = null;

                if (request.StartDate.HasValue || request.EndDate.HasValue)
                {
                    specification = new CreationDateBetweenSpecification(request.StartDate, request.EndDate);
                }

                if (request.UserId.HasValue)
                {
                    var spec = new UserIdEqualsSpecification<Payment, long>(request.UserId.Value);
                    specification = specification is null ? spec : specification.And(spec);
                }

                if (request.Gateway is not null)
                {
                    var spec = new GatewayEqualsSpecification(request.Gateway);
                    specification = specification is null ? spec : specification.And(spec);
                }

                if (request.Status is not null)
                {
                    var spec = new StatusEqualsSpecification(request.Status);
                    specification = specification is null ? spec : specification.And(spec);
                }

                if (request.Currency is not null)
                {
                    var spec = new CurrencyEqualsSpecification(request.Currency);
                    specification = specification is null ? spec : specification.And(spec);
                }

                var lst = await paymentService.Value.GetPaymentsSummaryAsync(specification);
                if (lst.OperationResult is not Constants.OperationResult.Succeeded)
                {
                    return Ok<IEnumerable<PaymentsSummaryResponseViewModel>>(new() { Errors = lst.Errors });
                }

                var start = request.StartDate.HasValue ? request.StartDate.Value.ToDateTime(TimeOnly.MinValue) : lst.Data![0].Date;
                var end = request.EndDate.HasValue ? request.EndDate.Value.ToDateTime(TimeOnly.MinValue) : lst.Data![^1].Date;

                List<PaymentsSummaryResponseViewModel> result = [];
                while (start <= end)
                {
                    var failed = lst.Data!.Find(t => t.Status == PaymentStatus.Failed && t.Date == start);
                    var paid = lst.Data!.Find(t => t.Status == PaymentStatus.Paid && t.Date == start);
                    var pending = lst.Data!.Find(t => t.Status == PaymentStatus.Pending && t.Date == start);
                    result.Add(new()
                    {
                        Date = DateOnly.FromDateTime(start),
                        FailedAmount = failed?.Amount ?? 0,
                        FailedCount = failed?.Count ?? 0,
                        PaidAmount = paid?.Amount ?? 0,
                        PaidCount = paid?.Count ?? 0,
                        PendingAmount = pending?.Amount ?? 0,
                        PendingCount = pending?.Count ?? 0,
                    });

                    start = start.AddDays(1);
                }

                return Ok<IEnumerable<PaymentsSummaryResponseViewModel>>(new()
                {
                    Data = result,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<IEnumerable<PaymentsSummaryResponseViewModel>>(new() { Errors = [new() { Message = exc.Message }] });
            }
        }
    }
}
