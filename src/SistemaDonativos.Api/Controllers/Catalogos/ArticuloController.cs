using apiDonativos.Models.Catalogos;
using apiDonativos.Models.Exceptions;  // ✅ NUEVO: Para DuplicateResourceException
using apiDonativos.Services.Catalogos;
using apiDonativos.Framework;
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
    public class ArticuloController : ControllerBase
    {
        private readonly IArticulo _articuloService;
        private readonly ILogger<ArticuloController> _logger;  // ✅ NUEVO

        public ArticuloController(IArticulo articuloService, ILogger<ArticuloController> logger)  // ✅ MODIFICADO
        {
            _articuloService = articuloService;
            _logger = logger;  // ✅ NUEVO
        }

        /// <summary>
        /// Obtiene el listado completo de artículos.
        /// GET: catalogos/articulo
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Articulo>>> ObtenerListado()
        {
            try
            {
                var resultado = await _articuloService.GetAllAsync();

                if (resultado == null || resultado.Count == 0)
                    return NotFound(new { message = "No se encontraron artículos" });

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Busca artículos por nombre.
        /// GET: catalogos/articulo/buscar/{nombre}
        /// </summary>
        [HttpGet("buscar/{nombre}")]
        public async Task<ActionResult<List<Articulo>>> BuscarPorNombre(string nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                    return BadRequest(new { message = "El nombre no puede estar vacío" });

                var resultado = await _articuloService.GetByNameAsync(nombre);

                if (resultado == null || resultado.Count == 0)
                    return NotFound(new { message = $"No se encontraron artículos con el nombre '{nombre}'" });

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Obtiene un artículo por su ID.
        /// GET: catalogos/articulo/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Articulo>> Obtener(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID debe ser mayor a 0" });

                var resultado = await _articuloService.GetByIdAsync(id);

                if (resultado == null)
                    return NotFound(new { message = "Artículo no encontrado" });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error en el servidor: {ex.Message}" });
            }
        }

        /// <summary>
        /// Crea un nuevo artículo.
        /// POST: catalogos/articulo
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Articulo>> Crear([FromBody] Articulo articulo)
        {
            try
            {
                if (articulo == null)
                    return BadRequest(new { message = "Los datos del artículo no fueron proporcionados" });

                if (string.IsNullOrWhiteSpace(articulo.nombre))
                    return BadRequest(new { message = "El nombre del artículo es requerido" });

                if (articulo.id_categoria <= 0)
                    return BadRequest(new { message = "Debe seleccionar una categoría válida" });

                //Validar el modelo recibido
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                //Valda si el usuario esta autenticado
                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                try
                {
                    var nuevoId = await _articuloService.CreateAsync(articulo, id_usuario.Value);
                    var articuloCreado = await _articuloService.GetByIdAsync(nuevoId);

                    return CreatedAtAction(nameof(Obtener), new { id = nuevoId }, articuloCreado);
                }
                catch (ArgumentNullException)
                {
                    return BadRequest(new { message = "Artículo inválido" });
                }
                catch (DuplicateResourceException ex)
                {
                    // ✅ MEJORA: Manejo específico de recursos duplicados con HTTP 409 Conflict
                    _logger.LogWarning(
                        "Intento de crear artículo duplicado. Usuario: {UserId}, Nombre: {Nombre}, ID Existente: {ExistingId}",
                        id_usuario,
                        articulo?.nombre,
                        ex.ResourceId
                    );

                    return Conflict(new
                    {
                        message = ex.Message,
                        resourceType = ex.ResourceType,
                        conflictingField = ex.ConflictingField,
                        existingResource = new
                        {
                            id_articulo = ex.ResourceId
                        }
                    });
                }
                catch (InvalidOperationException)
                {
                    return StatusCode(500, new { message = "Error al crear el artículo" });
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Actualiza un artículo existente.
        /// PUT: catalogos/articulo
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<Articulo>> Actualizar([FromBody] Articulo articulo)
        {
            try
            {
                if (articulo == null || articulo.id_articulo <= 0)
                    return BadRequest(new { message = "Artículo inválido" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                try
                {
                    await _articuloService.UpdateAsync(articulo, id_usuario.Value);
                    var articuloActualizado = await _articuloService.GetByIdAsync(articulo.id_articulo);

                    return Ok(articuloActualizado);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("no existe"))
                {
                    return NotFound(new { message = "Artículo no encontrado" });
                }
                catch (ArgumentException)
                {
                    return BadRequest(new { message = "ID de artículo inválido" });
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Elimina un artículo por su ID.
        /// DELETE: catalogos/articulo/{id}
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "El ID debe ser mayor a 0" });

                var id_usuario = User.GetUserId();
                if (id_usuario == null)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                try
                {
                    var resultado = await _articuloService.DeleteAsync(id, id_usuario.Value);

                    if (!resultado)
                        return NotFound(new { message = "Artículo no encontrado" });

                    return Ok(new { message = "Artículo eliminado correctamente" });
                }
                catch (ArgumentException)
                {
                    return BadRequest(new { message = "ID inválido" });
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Lista las categorías disponibles para los artículos.
        /// GET: catalogos/articulo/categoria
        /// </summary>
        /// <returns></returns>
        [HttpGet("categoria")]
        public async Task<ActionResult<List<Categoria>>> ObtenerCategorias()
        {
            try
            {
                var resultado = await _articuloService.GetCategoriasAsync();
                if (resultado == null || resultado.Count == 0)
                    return NotFound(new { message = "No se encontraron categorías" });
                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error en el servidor" });
            }
        }

        /// <summary>
        /// Obtiene artículos con su stock actual por almacén
        /// ✅ Endpoint para Salidas y Traspasos
        /// GET: catalogos/articulo/stock/{idAlmacen}?buscar=nombre
        /// </summary>
        /// <param name="idAlmacen">ID del almacén a consultar</param>
        /// <param name="buscar">Término de búsqueda opcional para filtrar por nombre</param>
        /// <returns>Lista de artículos con stock disponible en formato camelCase</returns>
        [HttpGet("stock/{idAlmacen:int}")]
        public async Task<ActionResult<List<apiDonativos.Models.Dtos.ArticuloConStockResponse>>> ObtenerStockPorAlmacen(
            int idAlmacen, 
            [FromQuery] string buscar = null)
        {
            var referenceId = Guid.NewGuid().ToString();

            try
            {
                if (idAlmacen <= 0)
                    return BadRequest(new { message = "El ID del almacén debe ser mayor a 0" });

                _logger.LogInformation(
                    "Consultando stock de artículos. ReferenceId: {ReferenceId}, Almacén: {AlmacenId}, Búsqueda: {Buscar}",
                    referenceId,
                    idAlmacen,
                    buscar ?? "todos"
                );

                var resultado = await _articuloService.GetConStockPorAlmacenAsync(idAlmacen, buscar);

                if (resultado == null || resultado.Count == 0)
                {
                    var mensaje = string.IsNullOrWhiteSpace(buscar)
                        ? $"No hay artículos con stock disponible en el almacén {idAlmacen}"
                        : $"No se encontraron artículos con '{buscar}' en el almacén {idAlmacen}";

                    _logger.LogWarning(
                        "Stock vacío. ReferenceId: {ReferenceId}, Mensaje: {Mensaje}",
                        referenceId,
                        mensaje
                    );

                    return NotFound(new { message = mensaje });
                }

                _logger.LogInformation(
                    "Stock consultado exitosamente. ReferenceId: {ReferenceId}, Cantidad: {Count}",
                    referenceId,
                    resultado.Count
                );

                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Validación fallida en stock. ReferenceId: {ReferenceId}, Almacén: {AlmacenId}",
                    referenceId,
                    idAlmacen
                );

                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar stock. ReferenceId: {ReferenceId}, Almacén: {AlmacenId}",
                    referenceId,
                    idAlmacen
                );

                #if DEBUG
                    return StatusCode(500, new
                    {
                        message = ex.Message,
                        detalle = ex.InnerException?.Message,
                        tipo_error = ex.GetType().Name,
                        reference_id = referenceId
                    });
                #else
                    return StatusCode(500, new
                    {
                        message = "Error al consultar stock de artículos",
                        reference_id = referenceId
                    });
                #endif
            }
        }
    }
}
