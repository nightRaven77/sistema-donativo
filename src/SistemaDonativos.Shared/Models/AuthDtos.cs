namespace SistemaDonativos.Shared.Models
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string CentroAcopio { get; set; } = string.Empty;
        public int CentroId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
    }
}
