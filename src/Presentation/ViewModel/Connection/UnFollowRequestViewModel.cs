namespace GamaEdtech.Presentation.ViewModel.Connection
{
    using GamaEdtech.Common.DataAnnotation;

    public sealed class UnFollowRequestViewModel
    {
        [Display]
        public bool TwoWayRevoke { get; set; }
    }
}
