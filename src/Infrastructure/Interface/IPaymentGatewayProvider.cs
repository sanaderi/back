namespace GamaEdtech.Infrastructure.Interface
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.Service.Factory;
    using GamaEdtech.Data.Dto.Provider.PaymentGateway;
    using GamaEdtech.Domain.Enumeration;

    [Injectable]
    public interface IPaymentGatewayProvider : IProvider<PaymentGateway>
    {
        Task<ResultData<CreateResponseDto>> CreateAsync([NotNull] CreateRequestDto requestDto);
        Task<ResultData<VerifyResponseDto>> VerifyAsync([NotNull] VerifyRequestDto requestDto);
    }
}
