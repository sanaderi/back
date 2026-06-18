namespace GamaEdtech.Presentation.ViewModel
{
    using GamaEdtech.Common.DataAnnotation;

    public sealed class CoordinateViewModel
    {
        [Display]
        [Required]
        public double? Latitude { get; set; }

        [Display]
        [Required]
        public double? Longitude { get; set; }
    }
}
