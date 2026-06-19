namespace GamaEdtech.Presentation.ViewModel.Identity
{
    public sealed class UserResponseViewModel
    {
        public long Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTimeOffset? RegistrationDate { get; set; }
        public bool Enabled { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ReferralId { get; set; }
    }
}
