using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;  // ? NUEVO
using System;
using System.Threading.Tasks;
using apiDonativos.Models;
using apiDonativos.Services;

namespace apiDonativos.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;  // ? NUEVO

        public AuthController(IAuthService authService, ILogger<AuthController> logger)  // ? MODIFICADO
        {
            _authService = authService;
            _logger = logger;  // ? NUEVO
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.Username) || string.IsNullOrEmpty(request?.Password))
                {
                    return BadRequest(new { message = "Usuario y contraseña son requeridos" });
                }

                // Validar credenciales de forma asíncrona
                var authResult = await _authService.ValidateCredentialsAsync(request.Username, request.Password, HttpContext.RequestAborted);
                
                if (!authResult.IsSuccess)
                {
                    return Unauthorized(new { message = authResult.ErrorMessage });
                }

                // Generar token con el centroID obtenido de la BD
                string token = _authService.GenerateToken(request.Username, authResult.CentroId, authResult.UserId);

                return Ok(new LoginResponse
                {
                    Token = token,
                    Message = "Login exitoso",
                    userId= authResult.UserId,
                    Username = request.Username,
                    centroAcopio = authResult.CentroAcopio,
                    centroID = authResult.CentroId,
                    nombreCompleto = authResult.nombreCompleto
                });
            }
            catch (Exception ex)
            {
                // ? SEGURIDAD: Log error sin exponer información sensible
                var referenceId = Guid.NewGuid().ToString();

                _logger.LogError(
                    ex,
                    "Error no controlado en login. ReferenceId: {ReferenceId}, Usuario: {Username}",
                    referenceId,
                    request?.Username ?? "desconocido"
                );

                #if DEBUG
                    return StatusCode(500, new { 
                        message = "Error en el proceso de autenticación",
                        detalle = ex.Message,
                        reference_id = referenceId
                    });
                #else
                    return StatusCode(500, new { 
                        message = "Error en el proceso de autenticación",
                        reference_id = referenceId
                    });
                #endif
            }
        }
    }
}
