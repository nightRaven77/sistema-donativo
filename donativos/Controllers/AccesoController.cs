using donativos.Models;
using donativos.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace donativos.Controllers
{
    public class AccesoController : Controller
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AccesoController> _logger;

        // La inyección de dependencias se hace automáticamente aquí
        public AccesoController(IAuthenticationService authService, ILogger<AccesoController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(Usuario user)
        {
            var result = await _authService.LoginAsync(user?.username, user?.password);

            if (!result.Success)
            {
                ViewBag.MensajeError = result.Message;
                return View();
            }

            // Guardar datos en sesión
            try
            {
                HttpContext.Session.SetString("usuario", result.User.username);
                HttpContext.Session.SetString("centroAcopio", result.User.centroAcopio ?? "");
                HttpContext.Session.SetString("CentroID", result.User.centroID ?? "0");
                HttpContext.Session.SetString("token", result.User.token);
                HttpContext.Session.SetInt32("userID", result.User.userID);

                return RedirectToAction("Index", "Registros", new { a = result.User.username });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar sesión");
                ViewBag.MensajeError = "Error al inicializar sesión";
                return View();
            }
        }
    }
}
