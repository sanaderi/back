namespace GamaEdtech.Infrastructure.Provider.CurrencyConverter
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.HttpProvider;
    using GamaEdtech.Common.Infrastructure;
    using GamaEdtech.Data.Dto.Provider.CurrencyConverter;
    using GamaEdtech.Data.Dto.Provider.PaymentGateway;
    using GamaEdtech.Domain.Enumeration;
    using GamaEdtech.Infrastructure.Interface;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;

    using static GamaEdtech.Common.Core.Constants;

    public sealed class SolCurrencyConverterProvider(Lazy<IHttpProvider> httpProvider, Lazy<IStringLocalizer<SolCurrencyConverterProvider>> localizer, Lazy<ILogger<SolCurrencyConverterProvider>> logger
        , Lazy<IConfiguration> configuration)
        : InfrastructureBase<SolCurrencyConverterProvider>(httpProvider, localizer, logger), ICurrencyConverterProvider
    {
        public Currency ProviderType => Currency.SOL;

        public async Task<ResultData<ConvertResponseDto>> GetPointsAsync([NotNull] ConvertRequestDto requestDto)
        {
            try
            {
                const long centMetrics = 100;

                var response = await HttpProvider.Value.GetAsync<GamaTrainConvertRequest, GamaTrainConvertResponse, GamaTrainConvertRequest>(new()
                {
                    Uri = configuration.Value.GetValue<string>("PaymentGateway:ConvertUri"),
                    Request = new(),
                    Body = new()
                    {
                        Amount = (long)requestDto.Amount,
                        SourceMint = requestDto.Mint,
                        DestinationMint = configuration.Value.GetValue<string?>("PaymentGateway:$GetMint"),
                    },
                });

                return response is null
                    ? new(OperationResult.Failed) { Errors = [new() { Message = Localizer.Value["GeneralError"], }] }
                    : new(OperationResult.Succeeded)
                    {
                        Data = new()
                        {
                            Point = (long)response.Amount * centMetrics
                        },
                    };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message, }] };
            }
        }
    }
}
