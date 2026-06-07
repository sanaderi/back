namespace GamaEdtech.Data.Dto.Identity
{
    public sealed class ResetPasswordRequestDto
    {
        public required long UserId { get; set; }

        public required string NewPassword { get; set; }
    }
}
