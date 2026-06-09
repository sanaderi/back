namespace GamaEdtech.Infrastructure.Interface
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.Service.Factory;
    using GamaEdtech.Data.Dto.Provider.CurrencyConverter;
    using GamaEdtech.Domain.Enumeration;

    [Injectable]
    public interface ICurrencyConverterProvider : IProvider<Currency>
    {
        Task<ResultData<ConvertResponseDto>> GetPointsAsync([NotNull] ConvertRequestDto requestDto);
    }
}
