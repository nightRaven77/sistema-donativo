using apiDonativos.Framework;
using apiDonativos.Models;
using apiDonativos.Models.Dtos;
using apiDonativos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;  // ? NUEVO
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apiDonativos.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class DonativoController : ControllerBase
    {
        private readonly IDonativo _donativoService;
        private readonly ILogger<DonativoController> _logger;  // ? NUEVO

        public DonativoController(IDonativo donativoService, ILogger<DonativoController> logger)  // ? MODIFICADO
        {
            _donativoService = donativoService;
            _logger = logger;  // ? NUEVO
        }

        /// <summary>
        /// Crea un nuevo donativo con sus detalles
        /// POST: donativo
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Donativo>> Crear([FromBody] CreateDonativoRequest request)
        {
            int? id_usuario = null;  // ? Declarar fuera del try

            try
            {
                if (request == null)
                    return BadRequest(new { message = "Los datos del donativo no fueron proporcionados" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                // Mapear DTO a entidad
                // ? CONVENCIÓN: DTO usa PascalCase, Modelo usa snake_case
                var donativo = new Donativo
                {
                    id_donador = request.IdDonador,
                    id_almacen = request.IdAlmacen,
                    fecha_recepcion = request.FechaRecepcion,
                    tipo = request.Tipo,
                    estatus = request.Estatus,
                    observaciones = request.Observaciones
                };


                // Mapear almacen a variable individual
                var id_almacen = request.IdAlmacen;

                // Mapear detalles
                var detalles = new List<DonativoDetalle>();
                foreach (var detalle in request.Detalles)
                {
                    detalles.Add(new DonativoDetalle
                    {
                        id_articulo = detalle.IdArticulo,
                        cantidad_recibida = detalle.CantidadRecibida,
                        valor_unitario = detalle.ValorUnitario,
                        estado = detalle.Estado,
                        descripcion = detalle.Descripcion,
                        unidad_medida = detalle.UnidadMedida
                    });
                }

                // Mapear archivos
                var archivos = new List<DonativoArchivo>();
                if (request.Archivos != null && request.Archivos.Count > 0)
                {
                    foreach (var archivo in request.Archivos)
                    {
                        archivos.Add(new DonativoArchivo
                        {
                            url = archivo.Url,
                            nombre_original = archivo.NombreOriginal,
                            fecha_actualiza = DateTime.UtcNow,
                        });
                    }
                }

                var idCreado = await _donativoService.CreateAsync(donativo, detalles, archivos, id_almacen, id_usuario.Value);

                return CreatedAtAction(nameof(Crear), new { id = idCreado }, 
                    new { 
                        id_donativo = idCreado, 
                        folio = donativo.folio,
                        message = "Donativo creado exitosamente"
                    });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = "Datos inválidos", detalle = ex.Message });
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
                // ? SEGURIDAD: Log y sanitización
                var referenceId = Guid.NewGuid().ToString();

                _logger.LogError(
                    ex,
                    "Error no controlado al crear donativo. ReferenceId: {ReferenceId}, Usuario: {UserId}",
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
        /// Obtiene el listado de donativos con información del donador
        /// GET: donativo
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<DonativoListadoResponse>>> ObtenerListado()
        {
            try
            {
                var resultado = await _donativoService.GetAllAsync();

                if (resultado == null || resultado.Count == 0)
                    return NotFound(new { message = "No se encontraron donativos" });

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Obtiene el listado de donativos registrados por el usuario autenticado
        /// GET: donativo/miusuario
        /// </summary>
        [HttpGet("miUsuario")]
        public async Task<ActionResult<List<DonativoListadoResponse>>> ObtenerMisDonativos()
        {
            try
            {
                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var resultado = await _donativoService.GetByIdUsuarioAsync(id_usuario.Value);

                if (resultado == null || resultado.Count == 0)
                    return NotFound(new { message = "No se encontraron donativos registrados por este usuario" });

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
        /// Obtiene los detalles de un donativo por su id
        /// get: donativo/{id}/detalles
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}/detalles")]
        public async Task<ActionResult<List<DonativoDetalleListadoResponse>>> ObtenerDetalles(int id)
        {
            try
            {
                if (id<= 0)
                    return BadRequest(new { message = "El id del donativo es inválido" });

                var resultado = await _donativoService.GetDetailById(id);

                if (resultado == null || resultado.Detalles.Count == 0)
                    return NotFound(new { message = "No se encontraron detalles para este donativo" });
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
        /// Obtiene un donativo completo por su ID con todos sus detalles
        /// GET: donativo/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DonativoCompleteResponse>> ObtenerPorId(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID del donativo debe ser mayor a 0" });

                var resultado = await _donativoService.GetByIdAsync(id);

                if (resultado == null)
                    return NotFound(new { message = "Donativo no encontrado" });

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

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID del donativo debe ser mayor a 0" });
                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });
                var eliminado = await _donativoService.DeleteAsync(id, id_usuario.Value);
                if (!eliminado)
                    return NotFound(new { message = "Donativo no encontrado o no se pudo eliminar" });
                return Ok(new { message = "Donativo eliminado exitosamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { message = "Error al eliminar el donativo" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Actualiza un donativo existente con sus detalles y archivos
        /// PUT: donativo/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> Actualizar(int id, [FromBody] UpdateDonativoRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Los datos del donativo no fueron proporcionados" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                try
                {
                    var resultado = await _donativoService.UpdateAsync(id, request, id_usuario.Value);

                    if (!resultado)
                        return NotFound(new { message = "El donativo no fue encontrado" });

                    return Ok(new { message = "Donativo actualizado exitosamente" });
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
                    return StatusCode(500, new { message = "Error al actualizar el donativo", details = ex.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error en el servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Cancela un donativo sin modificar el folio
        /// ? SOLUCIÓN: Solo cambia estatus, NO el folio
        /// POST: donativo/{id}/cancelar
        /// </summary>
        [HttpPost("{id}/cancelar")]
        public async Task<ActionResult> Cancelar(int id, [FromBody] CancelarDonativoRequest request)
        {
            var referenceId = Guid.NewGuid().ToString();

            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID del donativo debe ser mayor a 0" });

                if (request == null || string.IsNullOrWhiteSpace(request.MotivoCancelacion))
                    return BadRequest(new { message = "El motivo de cancelación es requerido" });

                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                _logger.LogInformation(
                    "Solicitud de cancelación de donativo. ReferenceId: {ReferenceId}, DonativoId: {DonativoId}, Usuario: {UserId}",
                    referenceId,
                    id,
                    id_usuario
                );

                var resultado = await _donativoService.CancelarAsync(id, id_usuario.Value, request.MotivoCancelacion);

                if (!resultado)
                    return NotFound(new { message = "Donativo no encontrado" });

                _logger.LogInformation(
                    "Donativo cancelado exitosamente. ReferenceId: {ReferenceId}, DonativoId: {DonativoId}",
                    referenceId,
                    id
                );

                return Ok(new 
                { 
                    message = "Donativo cancelado exitosamente",
                    info = "El folio se mantiene sin cambios, solo se actualiza el estatus"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Validación fallida al cancelar donativo. ReferenceId: {ReferenceId}, DonativoId: {DonativoId}",
                    referenceId,
                    id
                );

                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Error de operación al cancelar donativo. ReferenceId: {ReferenceId}, DonativoId: {DonativoId}",
                    referenceId,
                    id
                );

                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error inesperado al cancelar donativo. ReferenceId: {ReferenceId}, DonativoId: {DonativoId}",
                    referenceId,
                    id
                );

                #if DEBUG
                    return StatusCode(500, new
                    {
                        message = "Error al cancelar el donativo",
                        details = ex.Message,
                        reference_id = referenceId
                    });
                #else
                    return StatusCode(500, new
                    {
                        message = "Error al cancelar el donativo",
                        reference_id = referenceId
                    });
                #endif
            }
        }
    }
}