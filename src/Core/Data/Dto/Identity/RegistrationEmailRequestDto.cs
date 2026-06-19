namespace GamaEdtech.Data.Dto.Identity
{
    public sealed class RegistrationEmailRequestDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
