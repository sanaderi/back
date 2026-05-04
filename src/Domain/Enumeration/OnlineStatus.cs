namespace GamaEdtech.Domain.Enumeration
{
    using System;

    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.DataAnnotation;

    public sealed class OnlineStatus : Enumeration<OnlineStatus, byte>
    {
        [Display]
        public static readonly OnlineStatus Online = new(nameof(Online), 0);

        [Display]
        public static readonly OnlineStatus ActiveRecently = new(nameof(ActiveRecently), 1);

        [Display]
        public static readonly OnlineStatus OnlineToday = new(nameof(OnlineToday), 2);

        [Display]
        public static readonly OnlineStatus ActiveThisWeek = new(nameof(ActiveThisWeek), 3);

        [Display]
        public static readonly OnlineStatus ActiveThisMonth = new(nameof(ActiveThisMonth), 4);

        [Display]
        public static readonly OnlineStatus ActiveLongTimeAgo = new(nameof(ActiveLongTimeAgo), 5);

        [Display]
        public static readonly OnlineStatus NewUser = new(nameof(NewUser), 6);

        public OnlineStatus()
        {
        }

        public OnlineStatus(string name, byte value) : base(name, value)
        {
        }

        public static OnlineStatus Parse(DateTimeOffset? loginDate)
        {
            if (!loginDate.HasValue)
            {
                return NewUser;
            }

            var diff = DateTimeOffset.UtcNow.Subtract(loginDate.Value);
            if (diff <= TimeSpan.FromMinutes(5))
            {
                return Online;
            }
            if (diff <= TimeSpan.FromHours(1))
            {
                return ActiveRecently;
            }
            if (diff <= TimeSpan.FromHours(24))
            {
                return OnlineToday;
            }
            if (diff <= TimeSpan.FromDays(7))
            {
                return OnlineToday;
            }
            if (diff <= TimeSpan.FromDays(30))
            {
                return OnlineToday;
            }

            _ = diff;   //bypass analyzer
            return ActiveLongTimeAgo;
        }
    }
}
