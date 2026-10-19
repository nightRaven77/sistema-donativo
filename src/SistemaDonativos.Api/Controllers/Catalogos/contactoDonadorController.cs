using apiDonativos.Framework;
using apiDonativos.Models.Catalogos;
using apiDonativos.Services.Catalogos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;  // ✅ NUEVO
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace apiDonativos.Controllers.Catalogos
{
    [ApiController]
    [Authorize]
    [Route("catalogos/[controller]")]
    public class contactoDonadorController : ControllerBase
    {
        private readonly IContactoDonador _contactoDonadorService;
        private readonly ILogger<contactoDonadorController> _logger;  // ✅ NUEVO

        public contactoDonadorController(IContactoDonador contactoDonadorService, ILogger<contactoDonadorController> logger)  // ✅ MODIFICADO
        {
            _contactoDonadorService = contactoDonadorService;
            _logger = logger;  // ✅ NUEVO
        }

        /// <summary>
        /// Obtiene el listado de los contactos de los donadores
        /// GET: catalogos/contactoDonador
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<contactoDonador>> ObtenerListado()
        {
            try
            {
                var resultado = await _contactoDonadorService.GetAllAsync();

                if (resultado == null || resultado.Count == 0)
                {
                    return NotFound(new { message = "No se encontraron Contactos" });
                }
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error en el servidor: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene un contacto del donador por su ID
        /// GET: catalogos/contactoDonador/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<contactoDonador>> Obtener(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID debe ser mayor a 0" });

                var resultado = await _contactoDonadorService.GetByIdAsync(id);

                if (resultado == null)
                    return NotFound(new { message = "Contacto del donador no encontrado" });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error en el servidor: {ex.Message}" });
            }
        }


        /// <summary>
        /// Obtiene un contacto del donador por el ID del donador
        /// get: catalogos/contactoDonador/donador/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("donador/{id:int}")]
        public async Task<ActionResult<contactoDonador>> ObtenerPorIdDonador(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID debe ser mayor a 0" });
                var resultado = await _contactoDonadorService.GetByDonadorIdAsync(id);
                if (resultado == null)
                    return NotFound(new { message = "Contacto del donador no encontrado" });
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error en el servidor: {ex.Message}" });
            }
        }

        /// <summary>
        ///     Crea un nuevo contacto para un donador
        ///     POST: catalogos/contactoDonador
        /// </summary>
        /// <param name="nuevoContacto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<contactoDonador>> Crear([FromBody] contactoDonador nuevoContacto)
        {
            try
            {
                if (nuevoContacto == null)
                    return BadRequest(new { message = "Los datos del contacto no fueron proporcionados" });

                //Validar el modelo recibido
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                //Valda si el usuario esta autenticado
                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });
                try
                {
                    // Crear el nuevo contacto del donador
                    var nuevoContactoId = await _contactoDonadorService.CreateAsync(nuevoContacto, id_usuario.Value);

                    // Obtener el contacto creado para devolverlo en la respuesta
                    var contactoCreado = await _contactoDonadorService.GetByIdAsync(nuevoContactoId);

                    //Regresa el contacto creado con el estatus 201 y la ruta para obtenerlo nuevamente, así como el objeto creado
                    return CreatedAtAction(nameof(Obtener), new { id = nuevoContactoId }, contactoCreado);
                }
                catch (ArgumentNullException)
                {
                    return BadRequest(new { message = "Contacto inválido" });
                }
                catch (InvalidOperationException)
                {
                    return StatusCode(500, new { message = "Error al crear el contacto" });
                }

            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Actualiza un contacto de donador existente
        /// PUT: catalogos/contactoDonador
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<contactoDonador>> Actualizar([FromBody] contactoDonador contacto)
        {
            try
            {
                // Validaciones de entrada (responsabilidad del Controller)
                if (contacto == null || contacto.id_contacto <= 0)
                    return BadRequest(new { message = "Contacto inválido" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                // El Service maneja la lógica y validaciones de negocio
                try
                {
                    await _contactoDonadorService.UpdateAsync(contacto, id_usuario.Value);
                    var contactoActualizado = await _contactoDonadorService.GetByIdAsync(contacto.id_contacto);

                    return Ok(contactoActualizado);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("no existe"))
                {
                    return NotFound(new { message = "Contacto no encontrado" });
                }
                catch (ArgumentException)
                {
                    return BadRequest(new { message = "ID de contacto inválido" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error en el servidor: {ex.Message}" });
            }
        }

        /// <summary>
        /// Elimina un contacto de donador por su ID
        /// delete: catalogos/contactoDonador/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<bool>> Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID debe ser mayor a 0" });

                // Obtener el ID del usuario autenticado del JWT
                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                try
                {
                    var resultado = await _contactoDonadorService.DeleteAsync(id, id_usuario.Value);

                    if (!resultado)
                        return NotFound(new { message = "Contacto no encontrado" });

                    return Ok(new { message = "Contacto eliminado exitosamente" });
                }
                catch (ArgumentException)
                {
                    return BadRequest(new { message = "ID inválido" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error en el servidor: {ex.Message}" });
            }
        }
    }
}
