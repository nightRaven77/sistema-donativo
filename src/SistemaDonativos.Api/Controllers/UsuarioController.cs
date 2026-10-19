using apiDonativos.Models.Dtos;
using apiDonativos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;  // ✅ NUEVO
using System;
using System.Threading.Tasks;

namespace apiDonativos.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuario _usuarioService;
        private readonly ILogger<UsuarioController> _logger;  // ✅ NUEVO

        public UsuarioController(IUsuario usuarioService, ILogger<UsuarioController> logger)  // ✅ MODIFICADO
        {
            _usuarioService = usuarioService;
            _logger = logger;  // ✅ NUEVO
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema
        /// POST: usuario/registrar
        /// </summary>
        [HttpPost("registrar")]
        [AllowAnonymous]
        public async Task<ActionResult<RegistrarUsuarioResponse>> Registrar([FromBody] RegistrarUsuarioRequest request)
        {
            if (request == null)
                return BadRequest(new { mensaje = "Los datos del usuario no fueron proporcionados" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var resultado = await _usuarioService.RegistrarAsync(request);

                if (!resultado.exito)
                    return BadRequest(resultado);

                return Ok(resultado);
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (System.Exception ex)
            {
                var referenceId = Guid.NewGuid().ToString();
                _logger.LogError(ex, "Error al registrar usuario. ReferenceId: {ReferenceId}", referenceId);

                #if DEBUG
                    return StatusCode(500, new { mensaje = ex.Message, reference_id = referenceId });
                #else
                    return StatusCode(500, new { mensaje = "Error al procesar la solicitud", reference_id = referenceId });
                #endif
            }
        }
    }
}