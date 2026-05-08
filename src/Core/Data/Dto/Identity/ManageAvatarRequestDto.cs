namespace GamaEdtech.Data.Dto.Identity
{
    public sealed class ManageAvatarRequestDto
    {
        public required int UserId { get; set; }

        public required string? Avatar { get; set; }
    }
}
