namespace GamaEdtech.Data.Dto.Identity
{
    public sealed class ManageAvatarRequestDto
    {
        public required long UserId { get; set; }

        public required string? Avatar { get; set; }
    }
}
