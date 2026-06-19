namespace GamaEdtech.Presentation.ViewModel.Connection
{
    using GamaEdtech.Common.DataAnnotation;

    public sealed class FollowRequestViewModel
    {
        [Display]
        public bool SubscribeToActivityFeed { get; set; }
    }
}
