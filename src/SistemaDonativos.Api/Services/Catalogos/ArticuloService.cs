using apiDonativos.Models.Catalogos;
using apiDonativos.Models.Exceptions;  // ✅ NUEVO: Para DuplicateResourceException
using apiDonativos.Services.Base;  // ✅ FASE 3
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;  // ✅ FASE 3
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apiDonativos.Services.Catalogos
{
    public interface IArticulo
    {
        Task<List<Articulo>> GetAllAsync();
        Task<Articulo> GetByIdAsync(int id);
        Task<List<Articulo>> GetByNameAsync(string nombre);
        Task<int> CreateAsync(Articulo articulo, int id_usuario);
        Task<bool> UpdateAsync(Articulo articulo, int id_usuario);
        Task<bool> DeleteAsync(int id, int id_usuario);
        Task<List<Categoria>> GetCategoriasAsync();
        Task<List<apiDonativos.Models.Dtos.ArticuloConStockResponse>> GetConStockPorAlmacenAsync(int idAlmacen, string buscar = null);
    }

    /// <summary>
    /// Servicio para gestión de artículos
    /// ✅ FASE 3: Heredado de ServiceBase con timeout configurable
    /// </summary>
    public class ArticuloService : ServiceBase<ArticuloService>, IArticulo
    {
        public ArticuloService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<ArticuloService> logger) 
            : base(context, configuration, logger)
        {
        }

        /// <summary>
        /// Obtiene el listado de todos los artículos activos.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Articulo>> GetAllAsync()
        {
            return await _context.Articulos
                .Include(a => a.categoria)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene un artículo por su ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Articulo> GetByIdAsync(int id)
        {
            return await _context.Articulos
                .Include(a => a.categoria)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.id_articulo == id);
        }


        public async Task<List<Articulo>> GetByNameAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return new List<Articulo>();

            return await _context.Articulos
                .Where(a => a.nombre.ToLower().Contains(nombre.ToLower()))
                .Include(a => a.categoria)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Crea un nuevo artículo. El interceptor manejará automáticamente fecha_creacion y fecha_actualizacion.
        /// ✅ FASE 3: Usa validaciones y logging estandarizados
        /// </summary>
        public async Task<int> CreateAsync(Articulo articulo, int id_usuario)
        {
            // ✅ FASE 3: Validaciones estandarizadas
            ValidarObjetoRequerido(articulo, nameof(articulo));
            ValidarId(id_usuario, "usuario");
            ValidarCadenaRequerida(articulo.nombre, "nombre del artículo", longitudMinima: 2, longitudMaxima: 200);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            LogInicioOperacion("CreateAsync - Artículo", new { articulo.nombre, id_usuario });

            using var transaction = await _context.Database.BeginTransactionAsync();
            ConfigurarTimeout();  // ✅ FASE 3: Usa método base

            try
            {
                var existeArticulo = await _context.Articulos
                    .Where(a => a.nombre.ToLower() == articulo.nombre.ToLower())
                    .FirstOrDefaultAsync();

                if (existeArticulo != null)
                {
                    // ✅ MEJORA: Lanzar excepción estructurada en lugar de retornar ID silenciosamente
                    throw new DuplicateResourceException(
                        message: $"Ya existe un artículo con el nombre '{articulo.nombre}'",
                        resourceId: existeArticulo.id_articulo,
                        resourceCode: null,  // Los artículos no tienen código
                        resourceType: "Articulo",
                        conflictingField: "nombre"
                    );
                }

                _context.Articulos.Add(articulo);

                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Articulos",
                    submodulo = "Registro",
                    accion = "Crear",
                    descripcion = $"Se registró el artículo '{articulo.nombre}' con ID {articulo.id_articulo}"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                stopwatch.Stop();
                LogExitoOperacion("CreateAsync - Artículo", stopwatch.ElapsedMilliseconds, new { articulo.id_articulo });

                return articulo.id_articulo;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "Error BD al crear artículo. Usuario: {UserId}", id_usuario);
                throw new InvalidOperationException("Error al crear artículo: " + dbEx.InnerException?.Message, dbEx);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error inesperado. Usuario: {UserId}", id_usuario);
                throw new InvalidOperationException($"Error inesperado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza un artículo existente. El interceptor actualizará automáticamente fecha_actualizacion.
        /// </summary>
        public async Task<bool> UpdateAsync(Articulo articulo, int id_usuario)
        {
            if (articulo == null)
                throw new ArgumentNullException(nameof(articulo), "El articulo no puede ser nulo");

            if (articulo.id_articulo <= 0)
                throw new ArgumentException("ID de artículo inválido", nameof(articulo));

            _logger.LogInformation("Actualizando artículo. Id: {Id}, Usuario: {UserId}", articulo.id_articulo, id_usuario);

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);

            try
            {
                var articuloExistente = await _context.Articulos
                    .AsNoTracking().FirstOrDefaultAsync(a => a.id_articulo == articulo.id_articulo);

                if (articuloExistente == null)
                    throw new InvalidOperationException($"El artículo con ID {articulo.id_articulo} no existe");

                // Preservar fecha_creacion
                articulo.fecha_creacion = articuloExistente.fecha_creacion;

                _context.Articulos.Update(articulo);

                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Articulos",
                    submodulo = "Registro",
                    accion = "Actualizar",
                    descripcion = $"Se actualizó el artículo: '{articulo.nombre}' con ID {articulo.id_articulo}"
                };
                _context.Bitacoras.Add(bitacora);

                // El interceptor actualizará automáticamente fecha_actualizacion
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Artículo actualizado. Id: {Id}", articulo.id_articulo);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al actualizar artículo. Id: {Id}", articulo.id_articulo);
                throw new InvalidOperationException($"Error al actualizar: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Marca un artículo como inactivo (soft delete).
        /// El interceptor actualizará automáticamente fecha_actualizacion.
        /// </summary>
        public async Task<bool> DeleteAsync(int id, int id_usuario)
        {
            if (id <= 0)
                throw new ArgumentException("ID inválido", nameof(id));

            _logger.LogInformation("Eliminando artículo. Id: {Id}, Usuario: {UserId}", id, id_usuario);

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);

            try
            {
                var articulo = await _context.Articulos
                    .AsNoTracking().FirstOrDefaultAsync(a => a.id_articulo == id);

                if (articulo == null)
                    return false;

                // El interceptor detectará EntityState.Deleted y convertirá a soft delete
                _context.Articulos.Remove(articulo);

                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Articulos",
                    submodulo = "Registro",
                    accion = "Eliminar",
                    descripcion = $"Se eliminó el artículo '{articulo.nombre}' con ID {articulo.id_articulo}"
                };
                _context.Bitacoras.Add(bitacora);

                // El interceptor manejará el soft delete automáticamente
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Artículo eliminado. Id: {Id}", id);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al eliminar artículo. Id: {Id}", id);
                throw new InvalidOperationException($"Error al eliminar: {ex.Message}", ex);
            }
        }

        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            return await _context.Categorias
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene artículos con su stock actual por almacén
        /// ✅ CONVENCIÓN: Retorna DTOs con PascalCase
        /// </summary>
        /// <param name="idAlmacen">ID del almacén a consultar</param>
        /// <param name="buscar">Término de búsqueda opcional para filtrar por nombre</param>
        /// <returns>Lista de artículos con stock disponible</returns>
        public async Task<List<apiDonativos.Models.Dtos.ArticuloConStockResponse>> GetConStockPorAlmacenAsync(int idAlmacen, string buscar = null)
        {
            ValidarId(idAlmacen, "almacén");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            LogInicioOperacion("GetConStockPorAlmacenAsync", new { idAlmacen, buscar });

            try
            {
                // Validar que el almacén existe
                var almacenExiste = await _context.Almacenes
                    .AnyAsync(a => a.id_almacen == idAlmacen);

                if (!almacenExiste)
                    throw new InvalidOperationException($"El almacén con ID {idAlmacen} no existe");

                var query = _context.inventario
                    .Where(i => i.id_almacen == idAlmacen && i.cantidad > 0)
                    .Join(
                        _context.Articulos,
                        inv => inv.id_articulo,
                        art => art.id_articulo,
                        (inv, art) => new { inventario = inv, articulo = art }
                    )
                    .Join(
                        _context.Categorias,
                        combined => combined.articulo.id_categoria,
                        cat => cat.id_categoria,
                        (combined, cat) => new { combined.inventario, combined.articulo, categoria = cat }
                    )
                    .Join(
                        _context.Almacenes,
                        combined => combined.inventario.id_almacen,
                        alm => alm.id_almacen,
                        (combined, alm) => new
                        {
                            combined.inventario,
                            combined.articulo,
                            combined.categoria,
                            almacen = alm
                        }
                    );

                // Filtrar por nombre si se proporciona término de búsqueda
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    var buscarLower = buscar.ToLower();
                    query = query.Where(x => x.articulo.nombre.ToLower().Contains(buscarLower));
                }

                var resultado = await query
                    .Select(x => new apiDonativos.Models.Dtos.ArticuloConStockResponse
                    {
                        IdArticulo = x.articulo.id_articulo,
                        Nombre = x.articulo.nombre,
                        IdCategoria = x.categoria.id_categoria,
                        NombreCategoria = x.categoria.nombre,
                        StockActual = x.inventario.cantidad,
                        IdAlmacen = x.almacen.id_almacen,
                        NombreAlmacen = x.almacen.nombre,
                        FechaUltimaEntrada = x.inventario.fecha_ultima_entrada,
                        FechaUltimaSalida = x.inventario.fecha_ultima_salida,
                        Activo = x.articulo.activo
                    })
                    .OrderBy(x => x.Nombre)
                    .AsNoTracking()
                    .ToListAsync();

                stopwatch.Stop();
                LogExitoOperacion(
                    "GetConStockPorAlmacenAsync",
                    stopwatch.ElapsedMilliseconds,
                    new { count = resultado.Count, idAlmacen, buscar }
                );

                return resultado;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida en GetConStockPorAlmacenAsync. Almacén: {AlmacenId}", idAlmacen);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener artículos con stock. Almacén: {AlmacenId}", idAlmacen);
                throw new InvalidOperationException($"Error al obtener artículos con stock: {ex.Message}", ex);
            }
        }
    }
}
