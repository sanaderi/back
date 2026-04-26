namespace GamaEdtech.Presentation.Api.Controllers
{
    using System.Diagnostics.CodeAnalysis;

    using Asp.Versioning;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.Identity;
    using GamaEdtech.Presentation.ViewModel.Payment;

    using Microsoft.AspNetCore.Mvc;

    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Permission(policy: null)]
    public class PaymentsController(Lazy<ILogger<PaymentsController>> logger, Lazy<IPaymentService> paymentService)
        : ApiControllerBase<PaymentsController>(logger)
    {
        [HttpPost, Produces(typeof(ApiResponse<CreatePaymentResponseViewModel>))]
        [Permission(policy: null)]
        public async Task<IActionResult<CreatePaymentResponseViewModel>> CreatePayment([NotNull] CreatePaymentRequestViewModel request)
        {
            try
            {
                var result = await paymentService.Value.CreatePaymentAsync(new()
                {
                    UserId = User.UserId(),
                    Amount = request.Amount.GetValueOrDefault(),
                    Currency = request.Currency!,
                    Gateway = request.Gateway!,
                    Title = request.Title,
                    Description = request.Description,
                });

                return Ok<CreatePaymentResponseViewModel>(new(result.Errors)
                {
                    Data = result.Data is null
                    ? null
                    : new()
                    {
                        PaymentId = result.Data.PaymentId,
                        Url = result.Data.Url,
                    },
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return Ok<CreatePaymentResponseViewModel>(new(new Error { Message = exc.Message }));
            }
        }

        [HttpPost("{id:long}/verify"), Produces(typeof(ApiResponse<bool>))]
        [Permission(policy: null)]
        public async Task<IActionResult<bool>> VerifyPayment([FromRoute] long id, string? transactionId)
        {
            try
            {
                var result = await paymentService.Value.VerifyPaymentAsync(new()
                {
                    Id = id,
                    TransactionId = transactionId,
                });

                return Ok<bool>(new(result.Errors)
                {
                    Data = result.OperationResult is Constants.OperationResult.Succeeded,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return Ok<bool>(new(new Error { Message = exc.Message }));
            }
        }
    }
}
