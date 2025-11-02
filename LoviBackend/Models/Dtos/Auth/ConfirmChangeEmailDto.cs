namespace LoviBackend.Models.Dtos.Auth
{
    public class ConfirmChangeEmailDto
    {
        public string UserId { get; set; } = string.Empty;
        public string NewEmail { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
