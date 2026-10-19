using System.Globalization;

namespace apiDonativos.Models
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string Message { get; set; }
        public string Username { get; set; }
        public string centroAcopio { get; set; }
        public int centroID { get; set; }
        public int userId { get; set; }
        public string nombreCompleto { get; set; }
    }
}
