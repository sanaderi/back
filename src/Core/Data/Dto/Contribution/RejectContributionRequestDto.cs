namespace GamaEdtech.Data.Dto.Contribution
{
    public sealed class RejectContributionRequestDto
    {
        public required long Id { get; set; }
        public required int UserId { get; set; }
        public string? Comment { get; set; }
    }
}
