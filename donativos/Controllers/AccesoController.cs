using donativos.Models;
using donativos.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IAuthenticationService = donativos.Services.IAuthenticationService;

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
                // Crear claims para la autenticación por cookie
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, result.User.username),
                    new Claim(ClaimTypes.NameIdentifier, result.User.userID.ToString()),
                    new Claim("NombreCompleto", result.User.nombreCompleto ?? ""),
                    new Claim("CentroAcopio", result.User.centroAcopio ?? ""),
                    new Claim("CentroID", result.User.centroID ?? "0"),
                    new Claim("Token", result.User.token)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Firmar al usuario con la cookie de autenticación
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                // Guardar datos en sesión (mantener compatibilidad con código existente)
                HttpContext.Session.SetString("usuario", result.User.username);
                HttpContext.Session.SetString("centroAcopio", result.User.centroAcopio ?? "");
                HttpContext.Session.SetString("CentroID", result.User.centroID ?? "0");
                HttpContext.Session.SetString("token", result.User.token);
                HttpContext.Session.SetInt32("userID", result.User.userID);
                HttpContext.Session.SetString("nombreCompleto", result.User.nombreCompleto);

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
