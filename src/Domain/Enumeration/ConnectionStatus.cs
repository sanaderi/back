namespace GamaEdtech.Domain.Enumeration
{
    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.DataAnnotation;

    public sealed class ConnectionStatus : Enumeration<ConnectionStatus, byte>
    {
        [Display]
        public static readonly ConnectionStatus Requested = new(nameof(Requested), 0);

        [Display]
        public static readonly ConnectionStatus Confirmed = new(nameof(Confirmed), 1);

        [Display]
        public static readonly ConnectionStatus Rejected = new(nameof(Rejected), 2);

        public ConnectionStatus()
        {
        }

        public ConnectionStatus(string name, byte value) : base(name, value)
        {
        }
    }
}
