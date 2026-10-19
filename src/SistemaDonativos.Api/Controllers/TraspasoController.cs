using apiDonativos.Framework;
using apiDonativos.Models;
using apiDonativos.Models.Dtos;
using apiDonativos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;  // ✅ NUEVO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apiDonativos.Controllers
{
    /// <summary>
    /// Controlador para gestión de traspasos entre almacenes
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class TraspasoController : ControllerBase
    {
        private readonly ITraspaso _traspasoService;
        private readonly ILogger<TraspasoController> _logger;  // ✅ NUEVO

        public TraspasoController(ITraspaso traspasoService, ILogger<TraspasoController> logger)  // ✅ MODIFICADO
        {
            _traspasoService = traspasoService;
            _logger = logger;  // ✅ NUEVO
        }

        /// <summary>
        /// Crea un nuevo traspaso entre almacenes
        /// POST: /traspaso
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Crear([FromBody] CreateTraspasoRequest request)
        {
            int? id_usuario = null;  // ✅ Declarar fuera del try

            try
            {
                if (request == null)
                    return BadRequest(new { message = "Los datos del traspaso no fueron proporcionados" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                // ✅ CONVENCIÓN: Mapear DTO a entidad
                var traspaso = new Traspaso
                {
                    fecha_traspaso = request.FechaTraspaso,
                    almacen_origen = request.AlmacenOrigen,
                    almacen_destino = request.AlmacenDestino,
                    motivo = request.Motivo
                };

                // ✅ CONVENCIÓN: Mapear detalles
                var detalles = request.Detalles.Select(d => new TraspasoDetalle
                {
                    id_articulo = d.IdArticulo,
                    cantidad = d.Cantidad
                }).ToList();

                // ✅ CONVENCIÓN: Mapear archivos
                var archivos = new List<TraspasoArchivo>();
                if (request.Archivos != null && request.Archivos.Count > 0)
                {
                    foreach (var archivo in request.Archivos)
                    {
                        archivos.Add(new TraspasoArchivo
                        {
                            url = archivo.Url,
                            nombre_original = archivo.NombreOriginal,
                            tipo_archivo = archivo.TipoArchivo,
                            activo = true
                        });
                    }
                }

                // ✅ FASE 2: Crear traspaso con archivos
                var nuevoId = await _traspasoService.CreateAsync(
                    traspaso, 
                    detalles, 
                    archivos,
                    id_usuario.Value, 
                    request.IsInstantaneo
                );

                // Obtener traspaso creado
                var traspasoCreado = await _traspasoService.GetByIdAsync(nuevoId);

                return CreatedAtAction(
                    nameof(ObtenerPorId), 
                    new { id = nuevoId }, 
                    new 
                    { 
                        id_traspaso = nuevoId,
                        folio = traspasoCreado?.Folio,
                        estatus = traspasoCreado?.Estatus,
                        message = request.IsInstantaneo 
                            ? "Traspaso instantáneo creado y recibido exitosamente" 
                            : "Traspaso creado exitosamente en estado Pendiente"
                    }
                );
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
                // ✅ SEGURIDAD: Log y sanitización
                var referenceId = Guid.NewGuid().ToString();

                _logger.LogError(
                    ex,
                    "Error no controlado al crear traspaso. ReferenceId: {ReferenceId}, Usuario: {UserId}",
                    referenceId,
                    id_usuario
                );

                #if DEBUG
                    return StatusCode(500, new { 
                        message = ex.Message,
                        detalle = ex.InnerException?.Message,
                        tipo_error = ex.GetType().Name,
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
        /// Obtiene el listado de todos los traspasos
        /// GET: /traspaso
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<TraspasoListadoResponse>>> ObtenerListado()
        {
            try
            {
                var resultado = await _traspasoService.GetAllAsync();

                if (resultado == null || resultado.Count == 0)
                    return NotFound(new { message = "No se encontraron traspasos" });

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Obtiene el listado de traspasos filtrados por usuario autenticado
        /// GET: /traspaso/miUsuario
        /// </summary>
        [HttpGet("miUsuario")]
        public async Task<ActionResult<List<TraspasoListadoResponse>>> ObtenerMisTraspasos()
        {
            try
            {
                var id_usuario = User.GetUserId();
                var id_almacen = User.GetUserAlmacen();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var resultado = await _traspasoService.GetByIdUsuarioAsync(id_usuario.Value, id_almacen.Value);

                if (resultado == null || resultado.Count == 0)
                    return NotFound(new { message = "No se encontraron traspasos para este usuario" });

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
        /// Obtiene un traspaso completo por su ID
        /// GET: /traspaso/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TraspasoCompleteResponse>> ObtenerPorId(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID del traspaso debe ser mayor a 0" });

                var resultado = await _traspasoService.GetByIdAsync(id);

                if (resultado == null)
                    return NotFound(new { message = "Traspaso no encontrado" });

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
        /// Marca un traspaso como Recibido (solo si está Pendiente)
        /// POST: /traspaso/{id}/recibir
        /// </summary>
        [HttpPost("{id:int}/recibir")]
        public async Task<ActionResult> Recibir(int id, [FromBody] RecibirTraspasoRequest request)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID del traspaso debe ser mayor a 0" });

                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                // ✅ NUEVO: Mapear archivos de recepción
                var archivos = new List<TraspasoArchivo>();
                if (request?.Archivos != null && request.Archivos.Count > 0)
                {
                    foreach (var archivo in request.Archivos)
                    {
                        archivos.Add(new TraspasoArchivo
                        {
                            url = archivo.Url,
                            nombre_original = archivo.NombreOriginal,
                            tipo_archivo = archivo.TipoArchivo,
                            activo = true
                        });
                    }
                }

                var resultado = await _traspasoService.RecibirAsync(id, id_usuario.Value, archivos);

                if (!resultado)
                    return StatusCode(500, new { message = "No se pudo recibir el traspaso" });

                return Ok(new { message = "Traspaso recibido exitosamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Cancela un traspaso (revierte inventario si estaba Recibido)
        /// POST: /traspaso/{id}/cancelar
        /// </summary>
        [HttpPost("{id:int}/cancelar")]
        public async Task<ActionResult> Cancelar(int id, [FromBody] CancelarTraspasoRequest request)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID del traspaso debe ser mayor a 0" });

                if (request == null || string.IsNullOrWhiteSpace(request.MotivoCancelacion))
                    return BadRequest(new { message = "El motivo de cancelación es requerido" });

                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var resultado = await _traspasoService.CancelarAsync(id, id_usuario.Value, request.MotivoCancelacion);

                if (!resultado)
                    return StatusCode(500, new { message = "No se pudo cancelar el traspaso" });

                return Ok(new { message = "Traspaso cancelado exitosamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Elimina un traspaso (soft delete - solo si está Pendiente)
        /// DELETE: /traspaso/{id}
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID del traspaso debe ser mayor a 0" });

                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var resultado = await _traspasoService.DeleteAsync(id, id_usuario.Value);

                if (!resultado)
                    return NotFound(new { message = "Traspaso no encontrado o no se pudo eliminar" });

                return Ok(new { message = "Traspaso eliminado exitosamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }
    }
}
