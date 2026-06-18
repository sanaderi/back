namespace GamaEdtech.Data.Dto.Subscription
{
    public sealed class InitializeSubscriptionRequestDto
    {
        public required long UserId { get; set; }
        public required long SubscriptionPlanId { get; set; }
        public required DateTimeOffset StartDate { get; set; }
    }
}
