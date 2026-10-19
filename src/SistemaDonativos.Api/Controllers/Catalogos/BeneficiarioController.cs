using apiDonativos.Framework;
using apiDonativos.Models.Catalogos;
using apiDonativos.Models.Dtos;
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
    public class BeneficiarioController : ControllerBase
    {
        private readonly IBeneficiarioService _beneficiarioService;
        private readonly ILogger<BeneficiarioController> _logger;  // ✅ NUEVO

        public BeneficiarioController(IBeneficiarioService beneficiarioService, ILogger<BeneficiarioController> logger)  // ✅ MODIFICADO
        {
            _beneficiarioService = beneficiarioService;
            _logger = logger;  // ✅ NUEVO
        }

        /// <summary>
        /// Obtiene el listado de todos los beneficiarios activos
        /// GET: catalogos/beneficiario
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<BeneficiarioResponse>>> Obtener()
        {
            try
            {
                var resultado = await _beneficiarioService.GetAllAsync();
                
                if (resultado == null || resultado.Count == 0)
                    return NotFound(new { message = "No se encontraron beneficiarios" });
                
                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Obtiene un beneficiario por ID con información completa
        /// GET: catalogos/beneficiario/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BeneficiarioCompleteResponse>> ObtenerPorId(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID del beneficiario debe ser mayor a 0" });

                var resultado = await _beneficiarioService.GetCompleteByIdAsync(id);

                if (resultado == null)
                    return NotFound(new { message = "Beneficiario no encontrado" });

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Busca beneficiarios por nombre
        /// GET: catalogos/beneficiario/buscar/{nombre}
        /// </summary>
        [HttpGet("buscar/{nombre}")]
        public async Task<ActionResult<List<BeneficiarioResponse>>> BuscarPorNombre(string nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                    return BadRequest(new { message = "El nombre no puede estar vacío" });

                var resultado = await _beneficiarioService.GetByNameAsync(nombre);

                if (resultado == null || resultado.Count == 0)
                    return NotFound(new { message = "No se encontraron beneficiarios con ese nombre" });

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Crea un nuevo beneficiario
        /// POST: catalogos/beneficiario
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Insertar([FromBody] CreateBeneficiarioRequest request)
        {
            int? id_usuario = null;  // ✅ Declarar fuera del try

            try
            {
                if (request == null)
                    return BadRequest(new { message = "Los datos del beneficiario no fueron proporcionados" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                // Mapear DTO a entidad
                var beneficiario = new Beneficiario
                {
                    nombre = request.nombre,
                    tipo = request.tipo,
                    contacto_principal = request.contacto_principal,
                    telefono = request.telefono,
                    email = request.email,
                    direccion = request.direccion,
                    colonia = request.colonia,
                    ciudad = request.ciudad,
                    estado = request.estado,
                    codigo_postal = request.codigo_postal,
                    curp = request.curp,
                    observaciones = request.observaciones
                };

                var nuevoId = await _beneficiarioService.CreateAsync(beneficiario, id_usuario.Value);
                var beneficiarioCreado = await _beneficiarioService.GetByIdAsync(nuevoId);

                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoId }, 
                    new 
                    { 
                        id_beneficiario = nuevoId,
                        codigo = beneficiarioCreado.codigo,
                        message = "Beneficiario creado exitosamente" 
                    });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DuplicateResourceException ex)
            {
                // ✅ MEJORA: Manejo específico de recursos duplicados con HTTP 409 Conflict
                _logger.LogWarning(
                    "Intento de crear beneficiario duplicado. Usuario: {UserId}, Nombre: {Nombre}, ID Existente: {ExistingId}",
                    id_usuario,
                    request?.nombre,
                    ex.ResourceId
                );

                return Conflict(new
                {
                    message = ex.Message,
                    resourceType = ex.ResourceType,
                    conflictingField = ex.ConflictingField,
                    existingResource = new
                    {
                        id_beneficiario = ex.ResourceId,
                        codigo = ex.ResourceCode
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // ✅ SEGURIDAD: Log y sanitización
                var referenceId = Guid.NewGuid().ToString();

                _logger.LogError(
                    ex,
                    "Error no controlado al crear beneficiario. ReferenceId: {ReferenceId}, Usuario: {UserId}",
                    referenceId,
                    id_usuario
                );

                #if DEBUG
                    return StatusCode(500, new { 
                        message = ex.Message,
                        detalle = ex.InnerException?.Message,
                        reference_id = referenceId
                    });
                #else
                    return StatusCode(500, new { 
                        message = "Error al procesar la solicitud",
                        reference_id = referenceId
                    });
                #endif
            }
        }

        /// <summary>
        /// Actualiza un beneficiario existente
        /// PUT: catalogos/beneficiario/{id}
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Actualizar(int id, [FromBody] UpdateBeneficiarioRequest request)
        {
            int? id_usuario = null;  // ✅ Declarar fuera del try

            try
            {
                if (request == null)
                    return BadRequest(new { message = "Los datos del beneficiario no fueron proporcionados" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var resultado = await _beneficiarioService.UpdateAsync(id, request, id_usuario.Value);

                if (!resultado)
                    return NotFound(new { message = "El beneficiario no fue encontrado" });

                return Ok(new { message = "Beneficiario actualizado exitosamente" });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                var referenceId = Guid.NewGuid().ToString();
                _logger.LogError(ex, "Error al actualizar beneficiario. ReferenceId: {ReferenceId}, Usuario: {UserId}", referenceId, id_usuario);

                #if DEBUG
                    return StatusCode(500, new { message = ex.Message, reference_id = referenceId });
                #else
                    return StatusCode(500, new { message = "Error al procesar la solicitud", reference_id = referenceId });
                #endif
            }
        }

        /// <summary>
        /// Elimina (desactiva) un beneficiario
        /// DELETE: catalogos/beneficiario/{id}
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            int? id_usuario = null;  // ✅ Declarar fuera del try

            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID del beneficiario debe ser mayor a 0" });

                id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var eliminado = await _beneficiarioService.DeleteAsync(id, id_usuario.Value);

                if (!eliminado)
                    return NotFound(new { message = "Beneficiario no encontrado o no se pudo eliminar" });

                return Ok(new { message = "Beneficiario eliminado exitosamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                var referenceId = Guid.NewGuid().ToString();
                _logger.LogError(ex, "Error al eliminar beneficiario. ReferenceId: {ReferenceId}, Usuario: {UserId}", referenceId, id_usuario);

                #if DEBUG
                    return StatusCode(500, new { message = ex.Message, reference_id = referenceId });
                #else
                    return StatusCode(500, new { message = "Error al procesar la solicitud", reference_id = referenceId });
                #endif
            }
        }
    }
}
