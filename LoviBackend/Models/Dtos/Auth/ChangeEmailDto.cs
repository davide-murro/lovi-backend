namespace LoviBackend.Models.Dtos.Auth
{
    public class ChangeEmailDto
    {
        public string Password { get; set; } = string.Empty;
        public string NewEmail { get; set; } = string.Empty;
    }
}
