namespace GamaEdtech.Data.Dto.Game
{
    public sealed class TestTimeRequestDto
    {
        public required long UserId { get; set; }
        public required long TestId { get; set; }
        public required int SubmissionId { get; set; }
    }
}
