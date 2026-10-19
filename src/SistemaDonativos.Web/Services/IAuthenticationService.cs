using donativos.Models;
using System.Threading.Tasks;

namespace donativos.Services
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> LoginAsync(string username, string password);
    }

    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Usuario User { get; set; }
        public string ErrorCode { get; set; }
    }
}