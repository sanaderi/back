namespace GamaEdtech.Domain.Enumeration
{
    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.DataAnnotation;

    public sealed class PaymentGateway : Enumeration<PaymentGateway, byte>
    {
        [Display]
        public static readonly PaymentGateway GamaTrain = new(nameof(GamaTrain), 0);

        [Display]
        public static readonly PaymentGateway Stripe = new(nameof(Stripe), 1);

        public PaymentGateway()
        {
        }

        public PaymentGateway(string name, byte value) : base(name, value)
        {
        }
    }
}
