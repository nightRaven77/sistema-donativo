using apiDonativos.Framework;
using apiDonativos.Models.Dtos.Consulta;
using apiDonativos.Services.Consulta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace apiDonativos.Controllers
{
    /// <summary>
    /// Controller para consultas de movimientos e inventario en tiempo real
    /// ✅ ARQUITECTURA: REST API con endpoints GET para consultas
    /// ✅ AUTORIZACIÓN: Todos los endpoints requieren autenticación
    /// ✅ CONVENCIÓN: Retorna DTOs estructurados con formato { exito, mensaje, data, error }
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class ConsultaController : ControllerBase
    {
        private readonly IConsultaService _consultaService;
        private readonly ILogger<ConsultaController> _logger;

        public ConsultaController(
            IConsultaService consultaService,
            ILogger<ConsultaController> logger)
        {
            _consultaService = consultaService;
            _logger = logger;
        }

        /// <summary>
        /// Consulta unificada de movimientos (entradas, salidas, traspasos) con filtros dinámicos
        /// GET: consulta/movimientos
        /// </summary>
        /// <param name="request">Filtros de búsqueda y paginación</param>
        /// <returns>Lista paginada de movimientos</returns>
        [HttpGet("movimientos")]
        public async Task<ActionResult<ConsultaMovimientosResponse>> ConsultarMovimientos([FromQuery] ConsultaMovimientosRequest request)
        {
            int? id_usuario = null;

            try
            {
                if (request == null)
                    return BadRequest(new { mensaje = "Parámetros de consulta no proporcionados" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Validar tipo de movimiento
                var tiposPermitidos = new[] { "entrada", "salida", "traspaso", "todos" };
                if (!string.IsNullOrEmpty(request.tipo_movimiento) && 
                    !tiposPermitidos.Contains(request.tipo_movimiento.ToLower()))
                {
                    return BadRequest(new 
                    { 
                        mensaje = $"Tipo de movimiento inválido. Valores permitidos: {string.Join(", ", tiposPermitidos)}" 
                    });
                }

                // Validar paginación
                if (request.pagina < 1)
                    return BadRequest(new { mensaje = "La página debe ser mayor o igual a 1" });

                if (request.registros_por_pagina < 1 || request.registros_por_pagina > 10000)
                    return BadRequest(new { mensaje = "Los registros por página deben estar entre 1 y 10000" });

                // Validar rango de fechas
                if (request.fecha_inicio.HasValue && request.fecha_fin.HasValue && 
                    request.fecha_inicio.Value > request.fecha_fin.Value)
                {
                    return BadRequest(new { mensaje = "La fecha de inicio no puede ser mayor que la fecha de fin" });
                }

                _logger.LogInformation(
                    "Usuario {UserId} consultando movimientos. Tipo: {TipoMovimiento}, Página: {Pagina}",
                    id_usuario,
                    request.tipo_movimiento,
                    request.pagina
                );

                var resultado = await _consultaService.ConsultarMovimientosAsync(request);

                if (!resultado.exito)
                    return BadRequest(resultado);

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación fallida en consulta de movimientos. Usuario: {UserId}", id_usuario);
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                var referenceId = Guid.NewGuid().ToString();
                _logger.LogError(
                    ex,
                    "Error al consultar movimientos. Usuario: {UserId}, ReferenceId: {ReferenceId}",
                    id_usuario,
                    referenceId
                );

                #if DEBUG
                    return StatusCode(500, new { mensaje = ex.Message, reference_id = referenceId });
                #else
                    return StatusCode(500, new { mensaje = "Error al procesar la consulta", reference_id = referenceId });
                #endif
            }
        }

        /// <summary>
        /// Obtiene inventario en tiempo real por almacén con cálculo dinámico de stock
        /// GET: consulta/inventario/almacen/{idAlmacen}
        /// </summary>
        /// <param name="idAlmacen">ID del almacén</param>
        /// <param name="request">Filtros de búsqueda y ordenamiento</param>
        /// <returns>Inventario detallado del almacén</returns>
        [HttpGet("inventario/almacen/{idAlmacen}")]
        public async Task<ActionResult<InventarioAlmacenResponse>> ObtenerInventarioPorAlmacen(
            int idAlmacen,
            [FromQuery] InventarioAlmacenRequest request)
        {
            int? id_usuario = null;

            try
            {
                if (idAlmacen <= 0)
                    return BadRequest(new { mensaje = "ID de almacén inválido" });

                id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Validar ordenamiento
                var ordenesPermitidas = new[] { "nombre", "stock", "categoria" };
                if (!ordenesPermitidas.Contains(request.ordenar_por.ToLower()))
                {
                    return BadRequest(new 
                    { 
                        mensaje = $"Orden inválido. Valores permitidos: {string.Join(", ", ordenesPermitidas)}" 
                    });
                }

                var direccionesPermitidas = new[] { "asc", "desc" };
                if (!direccionesPermitidas.Contains(request.orden_direccion.ToLower()))
                {
                    return BadRequest(new 
                    { 
                        mensaje = $"Dirección de orden inválida. Valores permitidos: {string.Join(", ", direccionesPermitidas)}" 
                    });
                }

                _logger.LogInformation(
                    "Usuario {UserId} consultando inventario del almacén {AlmacenId}",
                    id_usuario,
                    idAlmacen
                );

                var resultado = await _consultaService.ObtenerInventarioPorAlmacenAsync(idAlmacen, request);

                if (!resultado.exito)
                    return BadRequest(resultado);

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación fallida en consulta de inventario. Usuario: {UserId}", id_usuario);
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                var referenceId = Guid.NewGuid().ToString();
                _logger.LogError(
                    ex,
                    "Error al obtener inventario del almacén {AlmacenId}. Usuario: {UserId}, ReferenceId: {ReferenceId}",
                    idAlmacen,
                    id_usuario,
                    referenceId
                );

                #if DEBUG
                    return StatusCode(500, new { mensaje = ex.Message, reference_id = referenceId });
                #else
                    return StatusCode(500, new { mensaje = "Error al obtener el inventario", reference_id = referenceId });
                #endif
            }
        }

        /// <summary>
        /// Obtiene detalle completo de un movimiento específico (entrada, salida o traspaso)
        /// GET: consulta/movimientos/{tipo}/{id}
        /// </summary>
        /// <param name="tipo">Tipo de movimiento: entrada, salida o traspaso</param>
        /// <param name="id">ID del movimiento</param>
        /// <returns>Detalle completo del movimiento con artículos y archivos</returns>
        [HttpGet("movimientos/{tipo}/{id}")]
        public async Task<ActionResult<DetalleMovimientoResponse>> ObtenerDetalleMovimiento(
            string tipo,
            int id)
        {
            int? id_usuario = null;

            try
            {
                if (string.IsNullOrWhiteSpace(tipo))
                    return BadRequest(new { mensaje = "Tipo de movimiento es requerido" });

                if (id <= 0)
                    return BadRequest(new { mensaje = "ID de movimiento inválido" });

                var tiposPermitidos = new[] { "entrada", "salida", "traspaso" };
                if (!tiposPermitidos.Contains(tipo.ToLower()))
                {
                    return BadRequest(new 
                    { 
                        mensaje = $"Tipo de movimiento inválido. Valores permitidos: {string.Join(", ", tiposPermitidos)}" 
                    });
                }

                id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                _logger.LogInformation(
                    "Usuario {UserId} consultando detalle de {Tipo} con ID {Id}",
                    id_usuario,
                    tipo,
                    id
                );

                var resultado = await _consultaService.ObtenerDetalleMovimientoAsync(tipo, id);

                if (!resultado.exito)
                {
                    if (resultado.mensaje.Contains("no encontrado") || resultado.mensaje.Contains("No existe"))
                        return NotFound(resultado);
                    
                    return BadRequest(resultado);
                }

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación fallida en detalle de movimiento. Usuario: {UserId}", id_usuario);
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                var referenceId = Guid.NewGuid().ToString();
                _logger.LogError(
                    ex,
                    "Error al obtener detalle de {Tipo} con ID {Id}. Usuario: {UserId}, ReferenceId: {ReferenceId}",
                    tipo,
                    id,
                    id_usuario,
                    referenceId
                );

                #if DEBUG
                    return StatusCode(500, new { mensaje = ex.Message, reference_id = referenceId });
                #else
                    return StatusCode(500, new { mensaje = "Error al obtener el detalle", reference_id = referenceId });
                #endif
            }
        }

        /// <summary>
        /// Obtiene estadísticas y KPIs de un almacén para dashboard
        /// GET: consulta/inventario/estadisticas/{idAlmacen}
        /// </summary>
        /// <param name="idAlmacen">ID del almacén</param>
        /// <returns>Estadísticas completas del almacén</returns>
        [HttpGet("inventario/estadisticas/{idAlmacen}")]
        public async Task<ActionResult<EstadisticasAlmacenResponse>> ObtenerEstadisticasAlmacen(int idAlmacen)
        {
            int? id_usuario = null;

            try
            {
                if (idAlmacen <= 0)
                    return BadRequest(new { mensaje = "ID de almacén inválido" });

                id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                _logger.LogInformation(
                    "Usuario {UserId} consultando estadísticas del almacén {AlmacenId}",
                    id_usuario,
                    idAlmacen
                );

                var resultado = await _consultaService.ObtenerEstadisticasAlmacenAsync(idAlmacen);

                if (!resultado.exito)
                    return BadRequest(resultado);

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación fallida en estadísticas. Usuario: {UserId}", id_usuario);
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                var referenceId = Guid.NewGuid().ToString();
                _logger.LogError(
                    ex,
                    "Error al obtener estadísticas del almacén {AlmacenId}. Usuario: {UserId}, ReferenceId: {ReferenceId}",
                    idAlmacen,
                    id_usuario,
                    referenceId
                );

                #if DEBUG
                    return StatusCode(500, new { mensaje = ex.Message, reference_id = referenceId });
                #else
                    return StatusCode(500, new { mensaje = "Error al obtener las estadísticas", reference_id = referenceId });
                #endif
            }
        }
    }
}
