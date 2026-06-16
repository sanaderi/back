namespace GamaEdtech.Data.Dto.Subscription
{
    public sealed class ActivateSubscriptionRequestDto
    {
        public required long Id { get; set; }
        public required long UserId { get; set; }
        public required DateTimeOffset StartDate { get; set; }
    }
}
