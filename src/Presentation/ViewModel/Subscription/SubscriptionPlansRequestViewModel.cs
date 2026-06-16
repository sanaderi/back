namespace GamaEdtech.Presentation.ViewModel.Subscription
{
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAnnotation;

    public sealed class SubscriptionPlansRequestViewModel
    {
        [Display]
        public PagingDto? PagingDto { get; set; }
    }
}
