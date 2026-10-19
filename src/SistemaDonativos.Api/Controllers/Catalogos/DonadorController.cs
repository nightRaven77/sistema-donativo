using apiDonativos.Framework;
using apiDonativos.Models.Catalogos;
using apiDonativos.Models.Exceptions;  // ✅ NUEVO: Para DuplicateResourceException
using apiDonativos.Services.Catalogos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;  // ✅ NUEVO
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apiDonativos.Controllers.Catalogos
{
    [ApiController]
    [Authorize]
    [Route("catalogos/[controller]")]
    public class DonadorController : ControllerBase
    {
        private readonly IDonador _donadorService;
        private readonly ILogger<DonadorController> _logger;  // ✅ NUEVO

        public DonadorController(IDonador donadorService, ILogger<DonadorController> logger)  // ✅ MODIFICADO
        {
            _donadorService = donadorService;
            _logger = logger;  // ✅ NUEVO
        }

        /// <summary>
        /// Obtiene el listado de todos los donadores
        /// GET: catalogos/donador
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Donador>>> ObtenerListado()
        {
            try
            {
                var resultado = await _donadorService.GetAllAsync();
                
                if (resultado == null || resultado.Count == 0)
                    return NotFound(new { message = "No se encontraron donadores" });

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Busca donadores por nombre
        /// GET: catalogos/donador/buscar/{nombre}
        /// </summary>
        [HttpGet("buscar/{nombre}")]
        public async Task<ActionResult<List<Donador>>> BuscarPorNombre(string nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                    return BadRequest(new { message = "El nombre no puede estar vacío" });

                var resultado = await _donadorService.GetByNameAsync(nombre);
                
                if (resultado == null || resultado.Count == 0)
                    return NotFound(new { message = $"No se encontraron donadores con el nombre '{nombre}'" });

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Obtiene un donador por su ID
        /// GET: catalogos/donador/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Donador>> Obtener(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID debe ser mayor a 0" });

                var resultado = await _donadorService.GetByIdAsync(id);
                
                if (resultado == null)
                    return NotFound(new { message = "Donador no encontrado" });

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Crea un nuevo donador
        /// POST: catalogos/donador
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Donador>> Insertar([FromBody] Donador nuevoDonador)
        {
            int? id_usuario = null;

            try
            {
                if (nuevoDonador == null)
                    return BadRequest(new { message = "Los datos del donador no fueron proporcionados" });


                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Obtener el ID del usuario autenticado del JWT
                id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                try
                {
                    // Crear con ID del usuario y bitácora
                    var idCreado = await _donadorService.CreateAsync(nuevoDonador, id_usuario.Value);
                    var donadorCreado = await _donadorService.GetByIdAsync(idCreado);

                    return CreatedAtAction(nameof(Obtener), new { id = idCreado }, donadorCreado);
                }
                catch (ArgumentNullException)
                {
                    return BadRequest(new { message = "Donador inválido" });
                }
                catch (DuplicateResourceException ex)
                {
                    // ✅ MEJORA: Manejo específico de recursos duplicados con HTTP 409 Conflict
                    _logger.LogWarning(
                        "Intento de crear donador duplicado. Usuario: {UserId}, Nombre: {Nombre}, ID Existente: {ExistingId}",
                        id_usuario,
                        nuevoDonador?.nombre,
                        ex.ResourceId
                    );

                    return Conflict(new
                    {
                        message = ex.Message,
                        resourceType = ex.ResourceType,
                        conflictingField = ex.ConflictingField,
                        existingResource = new
                        {
                            id_donador = ex.ResourceId,
                            codigo = ex.ResourceCode
                        }
                    });
                }
                catch (InvalidOperationException ex)
                {
                    return StatusCode(500, new { message = ex.Message });
                }
            }
            catch (Exception ex)
            {
                var referenceId = Guid.NewGuid().ToString();
                _logger.LogError(ex, "Error al crear donador. ReferenceId: {ReferenceId}, Usuario: {UserId}", referenceId, id_usuario);

                #if DEBUG
                    return StatusCode(500, new { message = ex.Message, reference_id = referenceId });
                #else
                    return StatusCode(500, new { message = "Error al procesar la solicitud", reference_id = referenceId });
                #endif
            }
        }

        /// <summary>
        /// Actualiza un donador existente
        /// PUT: catalogos/donador
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<Donador>> Actualizar([FromBody] Donador donador)
        {
            try
            {
                if (donador == null || donador.id_donador <= 0)
                    return BadRequest(new { message = "Donador inválido" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                try
                {
                    await _donadorService.UpdateAsync(donador, id_usuario.Value);
                    var donadorActualizado = await _donadorService.GetByIdAsync(donador.id_donador);

                    return Ok(donadorActualizado);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("no existe"))
                {
                    return NotFound(new { message = "Donador no encontrado" });
                }
                catch (ArgumentException)
                {
                    return BadRequest(new { message = "ID de donador inválido" });
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Elimina un donador
        /// DELETE: catalogos/donador/{id}
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<bool>> Eliminar(int id)
        {
            try
            {
                if(id<=0)
                    return BadRequest(new { message = "El ID debe ser mayor a 0" });

                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                try
                {
                    var resultado = await _donadorService.DeleteAsync(id, id_usuario.Value);

                    if (!resultado)
                        return NotFound(new { message = "Donador no encontrado" });

                    return Ok(new { message = "Donador eliminado correctamente" });
                }
                catch (ArgumentException)
                { 
                    return BadRequest(new { message = "ID de donador inválido" });
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }
    }
}
