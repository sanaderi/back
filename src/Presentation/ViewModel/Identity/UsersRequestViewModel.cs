namespace GamaEdtech.Presentation.ViewModel.Identity
{
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAnnotation;

    public sealed class UserListRequestViewModel
    {
        [Display]
        public PagingDto? PagingDto { get; set; } = new() { PageFilter = new(), };

        [Display]
        public bool? HasReferral { get; set; }

        [Display]
        public string? FirstName { get; set; }

        [Display]
        public string? LastName { get; set; }

        [Display]
        [EmailAddress]
        public string? Email { get; set; }

        [Display]
        public string? ReferralId { get; set; }
    }
}
