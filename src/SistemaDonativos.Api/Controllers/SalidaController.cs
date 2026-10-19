using apiDonativos.Framework;
using apiDonativos.Models;
using apiDonativos.Models.Dtos;
using apiDonativos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;  // ✅ NUEVO
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apiDonativos.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    // ✅ NOTA: Rate limiting comentado temporalmente (implementar con AspNetCoreRateLimit)
    // [EnableRateLimiting("api")]
    public class SalidaController : ControllerBase
    {
        private readonly ISalida _salidaService;
        private readonly ILogger<SalidaController> _logger;

        public SalidaController(ISalida salidaService, ILogger<SalidaController> logger)
        {
            _salidaService = salidaService;
            _logger = logger;
        }

        /// <summary>
        /// Crea una nueva salida con sus detalles
        /// POST: salida
        /// </summary>
        [HttpPost]
        // [EnableRateLimiting("critico")]  // ✅ TODO: Habilitar cuando se implemente
        public async Task<ActionResult<Salida>> Crear([FromBody] CreateSalidaRequest request)
        {
            int? id_usuario = null;  // ✅ Declarar fuera del try para usar en catch externo

            try
            {
                if (request == null)
                    return BadRequest(new { message = "Los datos de la salida no fueron proporcionados" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                try
                {
                    // ✅ CONVENCIÓN: Mapear DTO a entidad
                    var salida = new Salida
                    {
                        fecha_salida = request.FechaSalida,
                        id_almacen_origen = request.IdAlmacenOrigen,
                        id_beneficiario = request.IdBeneficiario,
                        tipo_salida = request.TipoSalida.ToLower(),  // ✅ NUEVO
                        motivo = request.Motivo,
                        observaciones = request.Observaciones,
                        estatus = request.Estatus ?? "Pendiente",
                        fecha_entrega = request.FechaEntrega ?? DateTime.Now,
                        recibio = request.Recibio,
                        id_usuario_entrega = id_usuario.Value,
                        fecha_registro = DateTime.Now
                    };

                    // ✅ CONVENCIÓN: Mapear detalles
                    var detalles = new List<SalidaDetalle>();
                    foreach (var detalle in request.Detalles)
                    {
                        detalles.Add(new SalidaDetalle
                        {
                            id_articulo = detalle.IdArticulo,
                            cantidad_entregada = detalle.CantidadEntregada,
                            valor_unitario = detalle.ValorUnitario,
                            observaciones = detalle.Observaciones,
                            unidad_medida = detalle.UnidadMedida
                        });
                    }

                    // ✅ CONVENCIÓN: Mapear archivos
                    var archivos = new List<SalidasArchivo>();
                    if (request.Archivos != null && request.Archivos.Count > 0)
                    {
                        foreach (var archivo in request.Archivos)
                        {
                            archivos.Add(new SalidasArchivo
                            {
                                url = archivo.Url,
                                nombre_original = archivo.NombreOriginal,
                                fecha_actualiza = DateTime.Now,
                                activo = true
                            });
                        }
                    }

                    // ✅ FASE 2: Llamada actualizada con archivos
                    var idCreado = await _salidaService.CreateAsync(salida, detalles, archivos, id_usuario.Value);

                    return CreatedAtAction(nameof(Crear), new { id = idCreado },
                        new { 
                            id_salida = idCreado,
                            folio = salida.folio,
                            message = "Salida creada exitosamente"
                        });
                }
                catch (ArgumentNullException ex)
                {
                    return BadRequest(new { 
                        message = "Datos inválidos proporcionados",
                        error = ex.Message 
                    });
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(new { 
                        message = ex.Message,
                        campo = ex.ParamName 
                    });
                }
                catch (InvalidOperationException ex)
                {
                    return StatusCode(500, new { 
                        message = ex.Message,
                        tipo_error = "Operación inválida"
                    });
                }
            }
            catch (Exception ex)
            {
                // ✅ SEGURIDAD: Log completo en servidor, mensaje sanitizado al cliente
                var referenceId = Guid.NewGuid().ToString();

                _logger.LogError(
                    ex,
                    "Error no controlado en creación de salida. ReferenceId: {ReferenceId}, Usuario: {UserId}",
                    referenceId,
                    id_usuario
                );

                #if DEBUG
                    // En desarrollo: mensaje detallado
                    return StatusCode(500, new { 
                        message = ex.Message,
                        detalle = ex.InnerException?.Message,
                        tipo_error = ex.GetType().Name,
                        reference_id = referenceId
                    });
                #else
                    // En producción: mensaje sanitizado
                    return StatusCode(500, new { 
                        message = "Error al procesar la solicitud",
                        reference_id = referenceId
                    });
                #endif
            }
        }

        /// <summary>
        /// Obtiene el listado de todas las salidas
        /// GET: salida
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<SalidaListadoResponse>>> ObtenerListado()
        {
            try
            {
                var resultado = await _salidaService.GetAllAsync();

                if (resultado == null || resultado.Count == 0)
                    return NotFound(new { message = "No se encontraron salidas" });

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Obtiene el listado de salidas registradas por el usuario autenticado
        /// GET: salida/miUsuario
        /// </summary>
        [HttpGet("miUsuario")]
        public async Task<ActionResult<List<SalidaListadoResponse>>> ObtenerMisSalidas()
        {
            try
            {
                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var resultado = await _salidaService.GetByIdUsuarioAsync(id_usuario.Value);

                if (resultado == null || resultado.Count == 0)
                    return NotFound(new { message = "No se encontraron salidas registradas por este usuario" });

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
        /// Obtiene los detalles de una salida por su ID
        /// GET: salida/{id}/detalles
        /// </summary>
        [HttpGet("{id:int}/detalles")]
        public async Task<ActionResult<SalidaDetalleConArchivosResponse>> ObtenerDetalles(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID de la salida es inválido" });

                var resultado = await _salidaService.GetDetailById(id);

                if (resultado == null || resultado.Detalles.Count == 0)
                    return NotFound(new { message = "No se encontraron detalles para esta salida" });

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
        /// Obtiene una salida completa por su ID con todos sus detalles
        /// GET: salida/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SalidaCompleteResponse>> ObtenerPorId(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID de la salida debe ser mayor a 0" });

                var resultado = await _salidaService.GetByIdAsync(id);

                if (resultado == null)
                    return NotFound(new { message = "Salida no encontrada" });

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
        /// Elimina una salida y revierte el inventario
        /// DELETE: salida/{id}
        /// </summary>
        [HttpDelete("{id:int}")]
        // [EnableRateLimiting("critico")]  // ✅ TODO: Habilitar cuando se implemente
        public async Task<ActionResult> Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID de la salida debe ser mayor a 0" });

                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var eliminado = await _salidaService.DeleteAsync(id, id_usuario.Value);

                if (!eliminado)
                    return NotFound(new { message = "Salida no encontrada o no se pudo eliminar" });

                return Ok(new { message = "Salida eliminada exitosamente" });
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
        /// Actualiza una salida existente con sus detalles
        /// PUT: salida/{id}
        /// </summary>
        [HttpPut("{id:int}")]
        // [EnableRateLimiting("critico")]  // ✅ TODO: Habilitar cuando se implemente
        public async Task<ActionResult> Actualizar(int id, [FromBody] UpdateSalidaRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Los datos de la salida no fueron proporcionados" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                try
                {
                    var resultado = await _salidaService.UpdateAsync(id, request, id_usuario.Value);

                    if (!resultado)
                        return NotFound(new { message = "La salida no fue encontrada" });

                    return Ok(new { message = "Salida actualizada exitosamente" });
                }
                catch (ArgumentNullException)
                {
                    return BadRequest(new { message = "Datos inválidos" });
                }
                catch (ArgumentException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return StatusCode(500, new { message = ex.Message });
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }
    }
}