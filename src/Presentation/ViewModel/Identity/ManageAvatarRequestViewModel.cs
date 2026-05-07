namespace GamaEdtech.Presentation.ViewModel.Identity
{
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.DataAnnotation;

    using Microsoft.AspNetCore.Http;

    public sealed class ManageAvatarRequestViewModel
    {
        [Display]
        [FileSize(300 * 1024)] // 300KB
        [FileExtensions(Constants.ValidImageExtensions)]
        [Required]
        public IFormFile? Avatar { get; set; }
    }
}
