namespace GamaEdtech.Infrastructure.Provider.PaymentGateway
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.HttpProvider;
    using GamaEdtech.Common.Infrastructure;
    using GamaEdtech.Data.Dto.Payment;
    using GamaEdtech.Data.Dto.Provider.PaymentGateway;
    using GamaEdtech.Domain.Enumeration;
    using GamaEdtech.Infrastructure.Interface;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;

    using static GamaEdtech.Common.Core.Constants;

    public sealed class GamaTrainPaymentGatewayProvider(Lazy<IConfiguration> configuration, Lazy<IHttpProvider> httpProvider, Lazy<IStringLocalizer<GamaTrainPaymentGatewayProvider>> localizer
        , Lazy<ILogger<GamaTrainPaymentGatewayProvider>> logger)
        : InfrastructureBase<GamaTrainPaymentGatewayProvider>(httpProvider, localizer, logger), IPaymentGatewayProvider
    {
        public PaymentGateway ProviderType => PaymentGateway.GamaTrain;

        public async Task<ResultData<CreateResponseDto>> CreateAsync([NotNull] CreateRequestDto requestDto) => await Task.FromResult(new ResultData<CreateResponseDto>(OperationResult.Succeeded));

        public async Task<ResultData<VerifyResponseDto>> VerifyAsync([NotNull] VerifyRequestDto requestDto)
        {
            try
            {
                var details = await GetTransactionDetailsAsync(requestDto.TransactionId);
                if (details.OperationResult is not OperationResult.Succeeded)
                {
                    return new(details.OperationResult) { Errors = details.Errors };
                }

                if (details.Data!.Memo != requestDto.PaymentId.ToString())
                {
                    return new(OperationResult.Failed) { Errors = [new() { Message = Localizer.Value["PaymentMissMatch"], }] };
                }

                var wallet = configuration.Value.GetValue<string>("PaymentGateway:Wallet");
                if (details.Data.DestinationWallet != wallet)
                {
                    return new(OperationResult.Failed) { Errors = [new() { Message = Localizer.Value["DestinationWalletMissMatch"].Value, }] };
                }

                if (details.Data.Currency != requestDto.Currency)
                {
                    return new(OperationResult.Failed) { Errors = [new() { Message = Localizer.Value["CurrencyMissMatch"].Value, }] };
                }

                if (details.Data.Amount.GetValueOrDefault() < requestDto.Amount)
                {
                    var message = Localizer.Value["AmountMissMatch"].Value;
                    return new(OperationResult.Failed) { Errors = [new() { Message = message, }] };
                }


                return new(OperationResult.Succeeded)
                {
                    Data = new()
                    {
                        Mint = details.Data.Mint,
                        SourceWallet = details.Data.SourceWallet,
                    }
                };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message, }] };
            }
        }

        private async Task<ResultData<PaymentInfoResponseDto>> GetTransactionDetailsAsync(string? transactionId)
        {
            try
            {
                var response = await HttpProvider.Value.PostAsync<GamaTrainTransactionDetailsRequest, GamaTrainTransactionDetailsResponse, GamaTrainTransactionDetailsRequest>(new()
                {
                    Uri = configuration.Value.GetValue<string>("PaymentGateway:GamaTrain:Uri"),
                    Request = new(),
                    Body = new()
                    {
                        ApiKey = configuration.Value.GetValue<string>("PaymentGateway:GamaTrain:ApiKey"),
                        TransactionId = transactionId,
                    },
                });
                if (response is null)
                {
                    return new(OperationResult.Failed) { Errors = [new() { Message = Localizer.Value["GeneralError"], }] };
                }

                if (response.Error)
                {
                    return new(OperationResult.Failed) { Errors = [new() { Message = response.ErrorMessage, }] };
                }

                if (response.Transfer is null)
                {
                    return new(OperationResult.Failed) { Errors = [new() { Message = Localizer.Value["NullTransferInfo"], }] };
                }

                var data = new PaymentInfoResponseDto
                {
                    Amount = response.Transfer.Amount,
                    Currency = response.Transfer.Currency,
                    DestinationWallet = response.Transfer.DestinationWallet,
                    SourceWallet = response.Transfer.SourceWallet,
                    Mint = response.Transfer.Mint,
                    Memo = response.Memo,
                };
                return new(OperationResult.Succeeded)
                {
                    Data = data,
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
