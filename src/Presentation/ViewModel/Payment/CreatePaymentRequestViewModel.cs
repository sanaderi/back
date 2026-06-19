namespace GamaEdtech.Presentation.ViewModel.Payment
{
    using System.Text.Json.Serialization;

    using GamaEdtech.Common.Converter;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Domain.Enumeration;

    public sealed class CreatePaymentRequestViewModel
    {
        public static decimal Zero => decimal.Zero;

        [Display]
        [Required]
        [Compare(nameof(Zero), OperandType = Common.Core.Constants.OperandType.GreaterThan)]
        public decimal? Amount { get; set; }

        [Display]
        [Required]
        [JsonConverter(typeof(EnumerationConverter<Currency, byte>))]
        public Currency? Currency { get; set; }

        [Display]
        [Required]
        [JsonConverter(typeof(EnumerationConverter<PaymentGateway, byte>))]
        public PaymentGateway? Gateway { get; set; }

        [Display]
        public string? Title { get; set; }

        [Display]
        public string? Description { get; set; }
    }
}
