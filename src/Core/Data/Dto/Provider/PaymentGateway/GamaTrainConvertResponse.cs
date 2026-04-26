namespace GamaEdtech.Data.Dto.Provider.PaymentGateway
{
    using System.Text.Json.Serialization;

    using GamaEdtech.Common.HttpProvider;

    public sealed class GamaTrainConvertResponse : IHttpResponse
    {
        [JsonPropertyName("outAmount")]
        public decimal Amount { get; set; }
    }
}
