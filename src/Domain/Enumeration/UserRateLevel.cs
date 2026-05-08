namespace GamaEdtech.Domain.Enumeration
{
    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.DataAnnotation;

    public sealed class UserRateLevel : Enumeration<UserRateLevel, byte>
    {
        [Display]
        public static readonly UserRateLevel Beginner = new(nameof(Beginner), 0);

        [Display]
        public static readonly UserRateLevel Basic = new(nameof(Basic), 1);

        [Display]
        public static readonly UserRateLevel Intermediate = new(nameof(Intermediate), 2);

        [Display]
        public static readonly UserRateLevel Advanced = new(nameof(Advanced), 3);

        [Display]
        public static readonly UserRateLevel Complete = new(nameof(Complete), 4);

        public UserRateLevel()
        {
        }

        public UserRateLevel(string name, byte value) : base(name, value)
        {
        }

        public static UserRateLevel Calculate(string? avatar, string? firstName, string? lastName, string? currentStatusSentence, string? biography, IEnumerable<string?>? skills, IEnumerable<string?>? experiences)
        {
            var rate = 0;
            if (string.IsNullOrEmpty(avatar))
            {
                rate += 15;
            }
            if (string.IsNullOrEmpty(firstName))
            {
                rate += 5;
            }
            if (string.IsNullOrEmpty(lastName))
            {
                rate += 5;
            }
            if (currentStatusSentence?.Length > 9)
            {
                rate += 10;
            }
            if (biography?.Length > 49)
            {
                rate += 15;
            }
            if (skills?.Any() == true)
            {
                rate += 20;
            }
            if (experiences?.Any() == true)
            {
                rate += 30;
            }

            if (rate < 20)
            {
                return Beginner;
            }

            if (rate < 40)
            {
                return Basic;
            }

            if (rate < 60)
            {
                return Intermediate;
            }

            if (rate < 80)
            {
                return Intermediate;
            }

            _ = rate;   //bypass analyzer
            return Complete;
        }
    }
}
