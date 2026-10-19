using donativos.Models;
using System.Threading.Tasks;

namespace donativos.Services
{
    public interface IEmailService
    {
        Task<bool> EnviarCorreoAgradecimientoAsync(EmailAgradecimientoViewModel modelo);
    }
}
