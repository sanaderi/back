namespace GamaEdtech.Domain.Enumeration
{
    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.DataAnnotation;

    public sealed class ProfileVisibility : Enumeration<ProfileVisibility, byte>
    {
        [Display]
        public static readonly ProfileVisibility Private = new(nameof(Private), 0);

        [Display]
        public static readonly ProfileVisibility Public = new(nameof(Public), 1);

        [Display]
        public static readonly ProfileVisibility ConnectionsOnly = new(nameof(ConnectionsOnly), 2);

        public ProfileVisibility()
        {
        }

        public ProfileVisibility(string name, byte value) : base(name, value)
        {
        }
    }
}
