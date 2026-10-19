using apiDonativos.Models;
using apiDonativos.Models.Catalogos;
using apiDonativos.Models.Dtos;
using apiDonativos.Services.Base;  // ✅ NUEVO
using apiDonativos.Helpers;  // ✅ M2: NUEVO
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;  // ✅ M3: NUEVO
using Microsoft.Extensions.Logging;  // ✅ NUEVO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apiDonativos.Services
{
    public interface IDonativo
    {
        Task<int> CreateAsync(Donativo donativo, List<DonativoDetalle> detalles, List<DonativoArchivo> archivos, int id_almacen, int id_usuario);
        Task<List<DonativoListadoResponse>> GetAllAsync();
        Task<List<DonativoListadoResponse>> GetByIdUsuarioAsync(int id_usuario);
        Task<DonativoDetalleConArchivosResponse> GetDetailById(int id_donativo); 
        Task<DonativoCompleteResponse> GetByIdAsync(int id_donativo);
        Task<bool> DeleteAsync(int id_donativo, int id_usuario);
        Task<bool> UpdateAsync(int id_donativo, UpdateDonativoRequest request, int id_usuario);
        Task<bool> CancelarAsync(int id_donativo, int id_usuario, string motivo_cancelacion);
    }

    /// <summary>
    /// Servicio para gestión de donativos
    /// ✅ FASE 3: Heredado de InventarioServiceBase con funcionalidad completa
    /// </summary>
    public class DonativoService : InventarioServiceBase<DonativoService>, IDonativo
    {
        public DonativoService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<DonativoService> logger)
            : base(context, configuration, logger)
        {
        }

        // ✅ FASE 3: Métodos de validación y logging heredados de ServiceBase

        /// <summary>
        /// Crea un donativo con sus detalles y archivos en una única transacción.
        /// También registra cada detalle en la tabla de inventario.
        /// ✅ FASE 2: Validaciones de archivos agregadas
        /// </summary>
        public async Task<int> CreateAsync(Donativo donativo, List<DonativoDetalle> detalles, List<DonativoArchivo> archivos, int id_almacen, int id_usuario)
        {
            if (donativo == null)
                throw new ArgumentNullException(nameof(donativo), "El donativo no puede ser nulo");

            if (detalles == null || !detalles.Any())
                throw new ArgumentException("Al menos un detalle es requerido", nameof(detalles));

            if (donativo.id_donador <= 0)
                throw new ArgumentException("El Donador no puede ser nulo");

            // ✅ FASE 2: Validar archivos si se proporcionan
            if (archivos != null && archivos.Any())
            {
                // Validar cantidad máxima
                if (!FileValidator.ValidarListaArchivos(
                    archivos.Select(a => a.nombre_original), 
                    out string errorArchivos))
                {
                    throw new ArgumentException(errorArchivos, nameof(archivos));
                }

                // Validar cada archivo individual
                foreach (var archivo in archivos)
                {
                    if (!FileValidator.ValidarNombreArchivo(archivo.nombre_original, out string errorNombre))
                    {
                        throw new ArgumentException($"Archivo '{archivo.nombre_original}': {errorNombre}", nameof(archivos));
                    }

                    if (!FileValidator.ValidarUrlArchivo(archivo.url, out string errorUrl))
                    {
                        throw new ArgumentException($"Archivo '{archivo.nombre_original}': {errorUrl}", nameof(archivos));
                    }
                }
            }

            // ✅ SEGURIDAD: Log inicio
            _logger.LogInformation(
                "Iniciando creación de donativo. Usuario: {UserId}, Donador: {DonadorId}, Almacén: {AlmacenId}, Artículos: {CantidadArticulos}",
                id_usuario,
                donativo.id_donador,
                id_almacen,
                detalles.Count
            );

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);  // ✅ SEGURIDAD: Timeout 30s

            try
            {
                // ✅ FASE 1: Generar folio y crear donativo con retry logic para manejar colisiones de UNIQUE
                string folioGenerado = null;
                await RetryHelper.ExecuteWithRetryAsync(async () =>
                {
                    folioGenerado = await GenerarFolioDonativoAsync(donativo.fecha_recepcion.Year);
                    donativo.folio = folioGenerado;
                    donativo.id_usuario_registro = id_usuario;
                    donativo.id_almacen = id_almacen;

                    _context.donativos.Add(donativo);
                    await _context.SaveChangesAsync();

                    return donativo.id_donativo;
                });

                // Agregar detalles con el ID del donativo recién creado
                foreach (var detalle in detalles)
                {
                    detalle.id_donativo = donativo.id_donativo;
                }
                _context.donativos_detalle.AddRange(detalles);

                // Agregar archivos con el ID del donativo recién creado
                if (archivos != null && archivos.Any())
                {
                    foreach (var archivo in archivos)
                    {
                        archivo.id_donativo = donativo.id_donativo;
                    }
                    _context.donativos_archivos.AddRange(archivos);
                }

                // Insertar en la tabla de inventario para cada detalle
                var ahora = DateTime.Now;

                // ✅ CAMBIO: Agrupar por artículo para evitar duplicados
                var detallesAgrupados = detalles
                    .GroupBy(d => d.id_articulo)
                    .Select(g => new
                    {
                        id_articulo = g.Key,
                        cantidad_total = g.Sum(d => d.cantidad_recibida)
                    })
                    .ToList();

                var idsArticulos = detallesAgrupados.Select(d => d.id_articulo).ToList();
                var inventariosExistentes = await _context.inventario
                    .Where(i => i.id_almacen == id_almacen && idsArticulos.Contains(i.id_articulo))
                    .ToDictionaryAsync(i => i.id_articulo);

                foreach (var detalleAgrupado in detallesAgrupados)
                {
                    if (inventariosExistentes.TryGetValue(detalleAgrupado.id_articulo, out var inventarioExistente))
                    {
                        inventarioExistente.cantidad += detalleAgrupado.cantidad_total;
                        inventarioExistente.fecha_ultima_entrada = ahora;
                        inventarioExistente.fecha_actualizacion = ahora;
                        _context.inventario.Update(inventarioExistente);

                        // ✅ Registrar movimiento de entrada (usando clase base)
                        RegistrarMovimiento(
                            id_almacen,
                            detalleAgrupado.id_articulo,
                            detalleAgrupado.cantidad_total, // Positivo para entrada
                            "Donativo",
                            ahora,
                            id_usuario,
                            id_donativo: donativo.id_donativo  // ✅ M1: Parámetro con nombre
                        );
                    }
                    else
                    {
                        var nuevoInventario = new Inventario
                        {
                            id_almacen = id_almacen,
                            id_articulo = detalleAgrupado.id_articulo,
                            cantidad = detalleAgrupado.cantidad_total,
                            fecha_ultima_entrada = ahora,
                            fecha_actualizacion = ahora
                        };
                        _context.inventario.Add(nuevoInventario);
                        inventariosExistentes[detalleAgrupado.id_articulo] = nuevoInventario;

                        // ✅ Registrar movimiento de entrada para inventario nuevo
                        RegistrarMovimiento(
                            id_almacen,
                            detalleAgrupado.id_articulo,
                            detalleAgrupado.cantidad_total,
                            "Donativo",
                            ahora,
                            id_usuario,
                            id_donativo: donativo.id_donativo
                        );
                    }
                }

                // Registrar en bitácora
                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Donativo",
                    submodulo = "Registro",
                    accion = "Crear",
                    descripcion = $"Se registró donativo {folioGenerado} con {detalles.Count} artículos"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // ✅ SEGURIDAD: Log éxito
                _logger.LogInformation(
                    "Donativo creado exitosamente. Folio: {Folio}, ID: {DonativoId}, Usuario: {UserId}",
                    folioGenerado,
                    donativo.id_donativo,
                    id_usuario
                );

                return donativo.id_donativo;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();

                // ✅ SEGURIDAD: Log error BD
                _logger.LogError(
                    dbEx,
                    "Error de BD al crear donativo. Usuario: {UserId}, Donador: {DonadorId}",
                    id_usuario,
                    donativo.id_donador
                );

                // ✅ MEJORADO: Mensajes claros
                var errorMessage = "Error al crear donativo en la base de datos";
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (innerMessage.Contains("Cannot insert the value NULL"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(
                        innerMessage, @"column '([^']+)'");
                    var columnName = match.Success ? match.Groups[1].Value : "desconocida";
                    errorMessage = $"Campo requerido faltante: '{columnName}' no puede ser nulo";
                }
                else if (innerMessage.Contains("FOREIGN KEY constraint"))
                {
                    if (innerMessage.Contains("id_donador"))
                        errorMessage = "El donador especificado no existe";
                    else if (innerMessage.Contains("id_almacen"))
                        errorMessage = "El almacén especificado no existe";
                    else if (innerMessage.Contains("id_articulo"))
                        errorMessage = "Uno o más artículos especificados no existen";
                    else
                        errorMessage = "Referencia inválida: uno de los IDs proporcionados no existe";
                }
                else if (innerMessage.Contains("PRIMARY KEY") || innerMessage.Contains("UNIQUE"))
                {
                    errorMessage = "Ya existe un registro con ese identificador";
                }
                else if (innerMessage.Contains("String or binary data would be truncated"))
                {
                    errorMessage = "Uno de los campos excede la longitud máxima permitida";
                }
                else
                {
                    errorMessage = $"Error de base de datos: {innerMessage}";
                }

                throw new InvalidOperationException(errorMessage, dbEx);
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();

                _logger.LogWarning(
                    ex,
                    "Error de validación al crear donativo. Usuario: {UserId}",
                    id_usuario
                );

                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error inesperado al crear donativo. Usuario: {UserId}",
                    id_usuario
                );

                throw new InvalidOperationException($"Error inesperado al crear donativo: {ex.Message}", ex);
            }
        }

        // ✅ M2: Usar FolioGenerator centralizado
        private async Task<string> GenerarFolioDonativoAsync(int year)
        {
            return await FolioGenerator.GenerarFolioAnioSimpleAsync(
                _context.donativos,
                year,
                d => d.folio,
                digits: 6
            );
        }

        /// <summary>
        /// Obtiene el listado de donativos con información del donador y cantidad de artículos.
        /// Equivalente a:
        /// SELECT folio, nombre, fecha_recepcion, donativos.tipo, estatus, 
        ///        COUNT(id_donativo_detalle) as cantidad_articulos
        /// FROM donativos 
        /// JOIN donativos_detalle ON donativos.id_donativo = donativos_detalle.id_donativo
        /// JOIN donadores ON donadores.id_donador = donativos.id_donador
        /// GROUP BY folio, nombre, fecha_recepcion, donativos.tipo, estatus
        /// ORDER BY folio ASC
        /// </summary>
        public async Task<List<DonativoListadoResponse>> GetAllAsync()
        {
            return await _context.donativos
                //.Where()
                .Join(
                    _context.Donadores,
                    d => d.id_donador,
                    don => don.id_donador,
                    (d, don) => new { donativo = d, donador = don }
                )
                .GroupBy(x => new
                {
                    x.donativo.folio,
                    x.donador.nombre,
                    x.donativo.fecha_recepcion,
                    x.donativo.tipo,
                    x.donativo.estatus,
                    x.donativo.id_donativo
                })
                .Select(g => new DonativoListadoResponse
                {
                    IdDonativo = g.Key.id_donativo,
                    Folio = g.Key.folio,
                    Nombre = g.Key.nombre,
                    FechaRecepcion = g.Key.fecha_recepcion,
                    Tipo = g.Key.tipo,
                    Estatus = g.Key.estatus,
                    CantidadArticulos = _context.donativos_detalle
                        .Where(dd => dd.id_donativo == g.Key.id_donativo)
                        .Count()
                })
                .OrderBy(x => x.Folio)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene el listado de donativos filtrados por el usuario que los registró.
        /// </summary>
        public async Task<List<DonativoListadoResponse>> GetByIdUsuarioAsync(int id_usuario)
        {
            if (id_usuario <= 0)
                throw new ArgumentException("ID de usuario inválido", nameof(id_usuario));

            return await _context.donativos
                .Where(d => d.id_usuario_registro == id_usuario)
                .Join(
                    _context.Donadores,
                    d => d.id_donador,
                    don => don.id_donador,
                    (d, don) => new { donativo = d, donador = don }
                )
                .GroupBy(x => new
                {
                    x.donativo.folio,
                    x.donador.nombre,
                    x.donativo.fecha_recepcion,
                    x.donativo.tipo,
                    x.donativo.estatus,
                    x.donativo.id_donativo
                })
                .Select(g => new DonativoListadoResponse
                {
                    IdDonativo = g.Key.id_donativo,
                    Folio = g.Key.folio,
                    Nombre = g.Key.nombre,
                    FechaRecepcion = g.Key.fecha_recepcion,
                    Tipo = g.Key.tipo,
                    Estatus = g.Key.estatus,
                    CantidadArticulos = _context.donativos_detalle
                        .Where(dd => dd.id_donativo == g.Key.id_donativo)
                        .Count()
                })
                .OrderBy(x => x.Folio)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<DonativoDetalleConArchivosResponse> GetDetailById(int id_donativo)
        {

            if (id_donativo <= 0)
                throw new ArgumentException("ID de donativo inválido", nameof(id_donativo));

            var archivos = await _context.donativos_archivos
                .Where(da => da.id_donativo == id_donativo && da.activo)
                .Select(da => new DonativoDetalleArchivoResponse
                {
                    IdArchivo = da.id_archivo,
                    IdDonativo = da.id_donativo,
                    NombreArchivo = da.nombre_original,
                    UrlArchivo = da.url
                })
                .AsNoTracking()
                .ToListAsync();

            var detalles = await _context.donativos
                .Where(d => d.id_donativo == id_donativo)
                .Join(_context.donativos_detalle,
                      donativo => donativo.id_donativo,
                      detalle => detalle.id_donativo,
                      (donativo, detalle) => new { donativo, detalle })
                .Join(_context.Articulos
                      , combined => combined.detalle.id_articulo
                      , articulo => articulo.id_articulo
                      , (combined, articulo) => new { combined.donativo, combined.detalle, articulo })
                .Join(_context.Categorias,
                      combined => combined.articulo.id_categoria,
                      categoria => categoria.id_categoria,
                      (combined, categoria) => new { combined.donativo, combined.detalle, combined.articulo, categoria })
                .Select(result => new DonativoDetalleListadoResponse
                {
                    IdArticulo = result.articulo.id_articulo,
                    NombreArticulo = result.articulo.nombre,
                    IdCategoria = result.categoria.id_categoria,
                    CategoriaArticulo = result.categoria.nombre,
                    UnidadMedida = result.detalle.unidad_medida,
                    Estado = result.detalle.estado,
                    CantidadRecibida = result.detalle.cantidad_recibida,
                    ValorUnitario = result.detalle.valor_unitario,
                    ValorTotal = result.detalle.cantidad_recibida * result.detalle.valor_unitario
                })
                .AsNoTracking()
                .ToListAsync();

            return new DonativoDetalleConArchivosResponse
            {
                Detalles = detalles,
                Archivos = archivos
            };
        }

        /// <summary>
        /// Obtiene un donativo completo con todos sus detalles para edición o visualización.
        /// Retorna la estructura: { id_donador, fecha_recepcion, tipo, estatus, observaciones, detalles [...] }
        /// </summary>
        public async Task<DonativoCompleteResponse> GetByIdAsync(int id_donativo)
        {
            if (id_donativo <= 0)
                throw new ArgumentException("ID de donativo inválido", nameof(id_donativo));

            var donativo = await _context.donativos
                .Where(d => d.id_donativo == id_donativo)
                .Join(_context.Donadores,
                      d => d.id_donador,
                      don => don.id_donador,
                      (d, don) => new { donativo = d, donador = don }
                )
                .Select(x => new
                {
                    x.donativo.id_donativo,
                    x.donativo.id_donador,
                    x.donativo.id_almacen,
                    x.donador.nombre,
                    x.donativo.fecha_recepcion,
                    x.donativo.tipo,
                    x.donativo.estatus,
                    x.donativo.observaciones
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (donativo == null)
                return null;

            var donador = await _context.Donadores
                .Where(d => d.id_donador == donativo.id_donador)
                .Select(d => new DonadorResponse
                {
                    IdDonador = d.id_donador,
                    Nombre = d.nombre,
                    Tipo = d.tipo,
                    RFC = d.RFC,
                    Telefono = d.telefono,
                    Email = d.email,
                    Direccion = d.direccion,
                    Colonia = d.colonia,
                    Ciudad = d.ciudad,
                    Estado = d.estado,
                    CodigoPostal = d.codigo_postal,
                    Notas = d.notas
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var contacto = await _context.contactos_donadores
                .Where(dc => dc.id_donador == donativo.id_donador)
                .Select(ContactoResponse => new ContactoResponse
                {
                    IdContacto = ContactoResponse.id_contacto,
                    Nombre = ContactoResponse.nombre,
                    Puesto = ContactoResponse.puesto,
                    Telefono = ContactoResponse.telefono,
                    Extension = ContactoResponse.extension,
                    Celular = ContactoResponse.celular,
                    Email = ContactoResponse.email,
                    HorarioContacto = ContactoResponse.horario_contacto,
                    MedioPreferido = ContactoResponse.medio_preferido,
                    EsPrincipal = ContactoResponse.es_principal,
                    Activo = ContactoResponse.activo,
                    Notas = ContactoResponse.notas
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var detalles = await _context.donativos_detalle
                .Where(dd => dd.id_donativo == id_donativo)
                .Join(
                    _context.Articulos,
                    dd => dd.id_articulo,
                    art => art.id_articulo,
                    (dd, art) => new { donativo = dd, articulo = art }
                )
                .AsNoTracking()
                .Select(dd => new DonativoDetalleResponse
                {
                    IdDonativoDetalle = dd.donativo.id_donativo_detalle,
                    IdArticulo = dd.donativo.id_articulo,
                    CantidadRecibida = dd.donativo.cantidad_recibida,
                    ValorUnitario = dd.donativo.valor_unitario,
                    Estado = dd.donativo.estado,
                    Categoria = dd.articulo.id_categoria,
                    UnidadMedida = dd.donativo.unidad_medida,
                    NombreArticulo = dd.articulo.nombre
                })
                .ToListAsync();

            var archivos = await _context.donativos_archivos
                .Where(da => da.id_donativo == id_donativo && da.activo)
                .Select(da => new DonativoArchivosResponse
                {
                    IdArchivo = da.id_archivo,
                    NombreArchivo = da.nombre_original,
                    UrlArchivo = da.url
                })
                .AsNoTracking()
                .ToListAsync();

            return new DonativoCompleteResponse
            {
                IdDonativo = donativo.id_donativo,
                IdDonador = donativo.id_donador,
                IdAlmacen = donativo.id_almacen,
                NombreDonador = donativo.nombre,
                FechaRecepcion = donativo.fecha_recepcion,
                Tipo = donativo.tipo,
                Estatus = donativo.estatus,
                Observaciones = donativo.observaciones,
                Contacto = contacto,
                Donador = donador,
                Detalles = detalles,
                Archivos = archivos
            };
        }

        public async Task<bool> DeleteAsync(int id_donativo, int id_usuario)
        {
            if (id_donativo <= 0)
                throw new ArgumentException("ID de donativo inválido", nameof(id_donativo));

            // ✅ SEGURIDAD: Log inicio
            _logger.LogInformation(
                "Iniciando eliminación de donativo. DonativoId: {DonativoId}, Usuario: {UserId}",
                id_donativo,
                id_usuario
            );

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);  // ✅ SEGURIDAD: Timeout 30s

            try
            {
                var donativo = await _context.donativos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.id_donativo == id_donativo);

                if (donativo == null)
                    throw new InvalidOperationException("Donativo no encontrado");

                // Obtener todos los detalles del donativo para actualizar el inventario
                var detalles = await _context.donativos_detalle
                    .Where(dd => dd.id_donativo == id_donativo)
                    .AsNoTracking()
                    .ToListAsync();

                // Restar las cantidades del inventario
                var ahora = DateTime.Now;

                // ✅ Agrupar detalles por artículo para evitar duplicados
                var detallesAgrupados = detalles
                    .GroupBy(d => d.id_articulo)
                    .Select(g => new
                    {
                        id_articulo = g.Key,
                        cantidad_total = g.Sum(d => d.cantidad_recibida)
                    })
                    .ToList();

                var idsArticulos = detallesAgrupados.Select(d => d.id_articulo).ToList();
                var inventariosExistentes = await _context.inventario
                    .Where(i => i.id_almacen == donativo.id_almacen && idsArticulos.Contains(i.id_articulo))
                    .ToDictionaryAsync(i => i.id_articulo);

                foreach (var detalle in detallesAgrupados)
                {
                    if (inventariosExistentes.TryGetValue(detalle.id_articulo, out var inventarioExistente))
                    {
                        // Restar la cantidad recibida
                        inventarioExistente.cantidad -= detalle.cantidad_total;
                        inventarioExistente.fecha_ultima_salida = ahora;
                        inventarioExistente.fecha_actualizacion = ahora;
                        _context.inventario.Update(inventarioExistente);

                        // ✅ Registrar movimiento de reversión
                        RegistrarMovimiento(
                            donativo.id_almacen,
                            detalle.id_articulo,
                            -detalle.cantidad_total,  // Negativo para salida
                            "Donativo-Eliminado",
                            ahora,
                            id_usuario,
                            id_donativo: id_donativo
                        );
                    }
                }

                _context.donativos.Remove(donativo);

                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Donativo",
                    submodulo = "Eliminación",
                    accion = "Eliminar",
                    descripcion = $"Se eliminó donativo {donativo.folio} y sus detalles"
                };
                _context.Bitacoras.Add(bitacora);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // ✅ SEGURIDAD: Log éxito
                _logger.LogInformation(
                    "Donativo eliminado exitosamente. Folio: {Folio}, Usuario: {UserId}",
                    donativo.folio,
                    id_usuario
                );

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    dbEx,
                    "Error BD al eliminar donativo. DonativoId: {DonativoId}",
                    id_donativo
                );

                var errorMessage = "Error al eliminar donativo en la base de datos";
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (innerMessage.Contains("REFERENCE constraint"))
                {
                    errorMessage = "No se puede eliminar: el donativo tiene registros dependientes";
                }
                else
                {
                    errorMessage = $"Error de base de datos: {innerMessage}";
                }

                throw new InvalidOperationException(errorMessage, dbEx);
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();

                _logger.LogWarning(
                    ex,
                    "Error al eliminar donativo. DonativoId: {DonativoId}",
                    id_donativo
                );

                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error inesperado al eliminar donativo. DonativoId: {DonativoId}",
                    id_donativo
                );

                throw new InvalidOperationException($"Error inesperado al eliminar donativo: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateAsync(int id_donativo, UpdateDonativoRequest request, int id_usuario)
        {
            if (id_donativo <= 0)
                throw new ArgumentException("ID de donativo inválido", nameof(id_donativo));

            if (request == null)
                throw new ArgumentNullException(nameof(request), "La solicitud de actualización no puede ser nula");

            // ✅ CONVENCIÓN: Validar archivos si se proporcionan
            if (request.Archivos != null && request.Archivos.Any())
            {
                // Validar cantidad máxima
                if (!FileValidator.ValidarListaArchivos(
                    request.Archivos.Select(a => a.NombreOriginal), 
                    out string errorArchivos))
                {
                    throw new ArgumentException(errorArchivos, nameof(request.Archivos));
                }

                // Validar cada archivo individual
                foreach (var archivo in request.Archivos)
                {
                    if (!FileValidator.ValidarNombreArchivo(archivo.NombreOriginal, out string errorNombre))
                    {
                        throw new ArgumentException($"Archivo '{archivo.NombreOriginal}': {errorNombre}", nameof(request.Archivos));
                    }

                    if (!FileValidator.ValidarUrlArchivo(archivo.Url, out string errorUrl))
                    {
                        throw new ArgumentException($"Archivo '{archivo.NombreOriginal}': {errorUrl}", nameof(request.Archivos));
                    }
                }
            }

            // ✅ SEGURIDAD: Log inicio
            _logger.LogInformation(
                "Iniciando actualización de donativo. DonativoId: {DonativoId}, Usuario: {UserId}",
                id_donativo,
                id_usuario
            );

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);  // ✅ SEGURIDAD: Timeout 30s

            try
            {
                var donativo = await _context.donativos.FindAsync(id_donativo);
                if (donativo == null)
                    throw new InvalidOperationException("Donativo no encontrado");

                // Actualizar propiedades simples
                donativo.fecha_recepcion = request.FechaRecepcion;
                donativo.tipo = request.Tipo;
                donativo.estatus = request.Estatus;
                donativo.observaciones = request.Observaciones;
                donativo.id_usuario_registro = id_usuario;
                donativo.fecha_actualizacion = DateTime.Now;

                _context.donativos.Update(donativo);

                // Manejo de detalles
                if (request.Detalles != null && request.Detalles.Any())
                {
                    // Obtener detalles actuales
                    var detallesActuales = await _context.donativos_detalle
                        .Where(dd => dd.id_donativo == id_donativo)
                        .ToListAsync();

                    // Identificar los IDs de detalles que vienen en el request
                    var idsDetallesEnRequest = request.Detalles
                        .Where(d => d.IdDonativoDetalle.HasValue && d.IdDonativoDetalle.Value > 0)
                        .Select(d => d.IdDonativoDetalle.Value)
                        .ToList();

                    // Detalles a eliminar: los que existen en BD pero NO vienen en el request
                    var detallesAEliminar = detallesActuales
                        .Where(dd => !idsDetallesEnRequest.Contains(dd.id_donativo_detalle))
                        .ToList();

                    // Pre-cargar inventario de todos los artículos involucrados en una sola consulta
                    var idsArticulosInvolucrados = detallesActuales.Select(d => d.id_articulo)
                        .Union(request.Detalles.Select(d => d.IdArticulo))
                        .Distinct()
                        .ToList();
                    var inventariosDict = await _context.inventario
                        .Where(i => i.id_almacen == donativo.id_almacen && idsArticulosInvolucrados.Contains(i.id_articulo))
                        .ToDictionaryAsync(i => i.id_articulo);

                    // Ajustar inventario para detalles eliminados
                    foreach (var detalleEliminado in detallesAEliminar)
                    {
                        if (inventariosDict.TryGetValue(detalleEliminado.id_articulo, out var inventario))
                        {
                            inventario.cantidad -= detalleEliminado.cantidad_recibida;
                            inventario.fecha_actualizacion = DateTime.Now;
                            _context.inventario.Update(inventario);
                        }
                    }

                    _context.donativos_detalle.RemoveRange(detallesAEliminar);

                    // Procesar detalles del request
                    foreach (var detalle in request.Detalles)
                    {
                        if (detalle.IdDonativoDetalle.HasValue && detalle.IdDonativoDetalle.Value > 0)
                        {
                            // Actualizar detalle existente
                            var detalleExistente = detallesActuales
                                .FirstOrDefault(d => d.id_donativo_detalle == detalle.IdDonativoDetalle);

                            if (detalleExistente != null)
                            {
                                // Calcular diferencia para ajustar inventario
                                var diferenciaCantidad = detalle.CantidadRecibida - detalleExistente.cantidad_recibida;

                                // Actualizar detalle
                                detalleExistente.id_articulo = detalle.IdArticulo;
                                detalleExistente.cantidad_recibida = detalle.CantidadRecibida;
                                detalleExistente.valor_unitario = detalle.ValorUnitario;
                                detalleExistente.estado = detalle.Estado;
                                detalleExistente.descripcion = detalle.Descripcion;
                                detalleExistente.unidad_medida = detalle.UnidadMedida;

                                _context.donativos_detalle.Update(detalleExistente);

                                // Ajustar inventario
                                if (diferenciaCantidad != 0)
                                {
                                    if (inventariosDict.TryGetValue(detalle.IdArticulo, out var inventario))
                                    {
                                        inventario.cantidad += diferenciaCantidad;
                                        inventario.fecha_actualizacion = DateTime.Now;
                                        _context.inventario.Update(inventario);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Agregar nuevo detalle
                            var nuevoDetalle = new DonativoDetalle
                            {
                                id_donativo = id_donativo,
                                id_articulo = detalle.IdArticulo,
                                cantidad_recibida = detalle.CantidadRecibida,
                                valor_unitario = detalle.ValorUnitario,
                                estado = detalle.Estado,
                                descripcion = detalle.Descripcion,
                                unidad_medida = detalle.UnidadMedida
                            };
                            _context.donativos_detalle.Add(nuevoDetalle);

                            // Actualizar inventario
                            if (inventariosDict.TryGetValue(detalle.IdArticulo, out var inventarioExistente))
                            {
                                inventarioExistente.cantidad += detalle.CantidadRecibida;
                                inventarioExistente.fecha_ultima_entrada = DateTime.Now;
                                inventarioExistente.fecha_actualizacion = DateTime.Now;
                                _context.inventario.Update(inventarioExistente);
                            }
                            else
                            {
                                var nuevoInventario = new Inventario
                                {
                                    id_almacen = donativo.id_almacen,
                                    id_articulo = detalle.IdArticulo,
                                    cantidad = detalle.CantidadRecibida,
                                    fecha_ultima_entrada = DateTime.Now,
                                    fecha_actualizacion = DateTime.Now
                                };
                                _context.inventario.Add(nuevoInventario);
                                inventariosDict[detalle.IdArticulo] = nuevoInventario;
                            }
                        }
                    }
                }

                // ✅ CONVENCIÓN: Manejo de archivos
                if (request.Archivos != null)
                {
                    // Obtener archivos actuales
                    var archivosActuales = await _context.donativos_archivos
                        .Where(da => da.id_donativo == id_donativo)
                        .ToListAsync();

                    // Identificar los IDs de archivos que vienen en el request
                    var idsArchivosEnRequest = request.Archivos
                        .Where(a => a.IdArchivo.HasValue && a.IdArchivo.Value > 0)
                        .Select(a => a.IdArchivo.Value)
                        .ToList();

                    // Archivos a eliminar: los que existen en BD pero NO vienen en el request
                    var archivosAEliminar = archivosActuales
                        .Where(da => !idsArchivosEnRequest.Contains(da.id_archivo))
                        .ToList();

                    // Marcar archivos como inactivos (soft delete)
                    foreach (var archivoEliminar in archivosAEliminar)
                    {
                        archivoEliminar.activo = false;
                        _context.donativos_archivos.Update(archivoEliminar);
                    }

                    // Procesar archivos del request
                    foreach (var archivo in request.Archivos)
                    {
                        if (archivo.IdArchivo.HasValue && archivo.IdArchivo.Value > 0)
                        {
                            // Actualizar archivo existente
                            var archivoExistente = archivosActuales
                                .FirstOrDefault(a => a.id_archivo == archivo.IdArchivo);

                            if (archivoExistente != null)
                            {
                                archivoExistente.url = archivo.Url;
                                archivoExistente.nombre_original = archivo.NombreOriginal;
                                archivoExistente.activo = true;
                                _context.donativos_archivos.Update(archivoExistente);
                            }
                        }
                        else
                        {
                            // Agregar nuevo archivo
                            var nuevoArchivo = new DonativoArchivo
                            {
                                id_donativo = id_donativo,
                                url = archivo.Url,
                                nombre_original = archivo.NombreOriginal,
                                activo = true
                            };
                            _context.donativos_archivos.Add(nuevoArchivo);
                        }
                    }
                }

                // Registrar en bitácora
                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Donativo",
                    submodulo = "Actualización",
                    accion = "Actualizar",
                    descripcion = $"Se actualizó donativo {donativo.folio}"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // ✅ SEGURIDAD: Log éxito
                _logger.LogInformation(
                    "Donativo actualizado exitosamente. Folio: {Folio}, Usuario: {UserId}",
                    donativo.folio,
                    id_usuario
                );

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    dbEx,
                    "Error BD al actualizar donativo. DonativoId: {DonativoId}",
                    id_donativo
                );

                var errorMessage = "Error al actualizar donativo en la base de datos";
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (innerMessage.Contains("Cannot insert the value NULL"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(
                        innerMessage, @"column '([^']+)'");
                    var columnName = match.Success ? match.Groups[1].Value : "desconocida";
                    errorMessage = $"Campo requerido faltante: '{columnName}' no puede ser nulo";
                }
                else if (innerMessage.Contains("FOREIGN KEY constraint"))
                {
                    errorMessage = "Referencia inválida: uno de los IDs proporcionados no existe";
                }
                else
                {
                    errorMessage = $"Error de base de datos: {innerMessage}";
                }

                throw new InvalidOperationException(errorMessage, dbEx);
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();

                _logger.LogWarning(
                    ex,
                    "Error al actualizar donativo. DonativoId: {DonativoId}",
                    id_donativo
                );

                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error inesperado al actualizar donativo. DonativoId: {DonativoId}",
                    id_donativo
                );

                throw new InvalidOperationException($"Error inesperado al actualizar donativo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cancela un donativo sin modificar el folio
        /// ✅ SOLUCIÓN: Solo cambia el estatus, NO el folio (evita duplicados)
        /// </summary>
        /// <param name="id_donativo">ID del donativo a cancelar</param>
        /// <param name="id_usuario">ID del usuario que cancela</param>
        /// <param name="motivo_cancelacion">Razón de la cancelación</param>
        /// <returns>True si se canceló exitosamente</returns>
        public async Task<bool> CancelarAsync(int id_donativo, int id_usuario, string motivo_cancelacion)
        {
            if (id_donativo <= 0)
                throw new ArgumentException("ID de donativo inválido", nameof(id_donativo));

            if (string.IsNullOrWhiteSpace(motivo_cancelacion))
                throw new ArgumentException("El motivo de cancelación es requerido", nameof(motivo_cancelacion));

            // ✅ SEGURIDAD: Log inicio
            _logger.LogInformation(
                "Iniciando cancelación de donativo. DonativoId: {DonativoId}, Usuario: {UserId}, Motivo: {Motivo}",
                id_donativo,
                id_usuario,
                motivo_cancelacion
            );

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);  // ✅ SEGURIDAD: Timeout 30s

            try
            {
                var donativo = await _context.donativos.FindAsync(id_donativo);
                if (donativo == null)
                    throw new InvalidOperationException("Donativo no encontrado");

                if (donativo.estatus == "Cancelado")
                    throw new InvalidOperationException("El donativo ya está cancelado");

                // ✅ SOLUCIÓN: NO modificar el folio, solo el estatus
                // ❌ ANTES: donativo.folio = donativo.folio + "-CANCELADO";
                // ✅ AHORA: El folio se mantiene igual

                var folioOriginal = donativo.folio;  // Guardar para logs
                donativo.estatus = "Cancelado";
                donativo.observaciones = string.IsNullOrEmpty(donativo.observaciones)
                    ? $"CANCELADO: {motivo_cancelacion}"
                    : $"{donativo.observaciones} | CANCELADO: {motivo_cancelacion}";
                donativo.fecha_actualizacion = DateTime.Now;

                _context.donativos.Update(donativo);

                // Obtener detalles y revertir inventario
                var detalles = await _context.donativos_detalle
                    .Where(dd => dd.id_donativo == id_donativo)
                    .ToListAsync();

                var ahora = DateTime.Now;

                // Agrupar detalles por artículo
                var detallesAgrupados = detalles
                    .GroupBy(d => d.id_articulo)
                    .Select(g => new
                    {
                        id_articulo = g.Key,
                        cantidad_total = g.Sum(d => d.cantidad_recibida)
                    })
                    .ToList();

                var idsArticulos = detallesAgrupados.Select(d => d.id_articulo).ToList();
                var inventariosExistentes = await _context.inventario
                    .Where(i => i.id_almacen == donativo.id_almacen && idsArticulos.Contains(i.id_articulo))
                    .ToDictionaryAsync(i => i.id_articulo);

                // Revertir inventario (restar las cantidades)
                foreach (var detalle in detallesAgrupados)
                {
                    if (inventariosExistentes.TryGetValue(detalle.id_articulo, out var inventarioExistente))
                    {
                        inventarioExistente.cantidad -= detalle.cantidad_total;
                        inventarioExistente.fecha_ultima_salida = ahora;
                        inventarioExistente.fecha_actualizacion = ahora;
                        _context.inventario.Update(inventarioExistente);

                        // ✅ Registrar movimiento de salida (reversión)
                        RegistrarMovimiento(
                            donativo.id_almacen,
                            detalle.id_articulo,
                            -detalle.cantidad_total,  // Negativo para salida
                            "Donativo-Cancelado",
                            ahora,
                            id_usuario,
                            id_donativo: id_donativo
                        );
                    }
                }

                // Registrar en bitácora
                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Donativo",
                    submodulo = "Cancelación",
                    accion = "Cancelar",
                    descripcion = $"Se canceló donativo {folioOriginal}. Motivo: {motivo_cancelacion}"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // ✅ SEGURIDAD: Log éxito
                _logger.LogInformation(
                    "Donativo cancelado exitosamente. Folio: {Folio} (SIN MODIFICAR), Usuario: {UserId}",
                    folioOriginal,
                    id_usuario
                );

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    dbEx,
                    "Error BD al cancelar donativo. DonativoId: {DonativoId}",
                    id_donativo
                );

                var errorMessage = "Error al cancelar donativo en la base de datos";
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (innerMessage.Contains("String or binary data would be truncated"))
                {
                    errorMessage = "El motivo de cancelación es demasiado largo";
                }
                else
                {
                    errorMessage = $"Error de base de datos: {innerMessage}";
                }

                throw new InvalidOperationException(errorMessage, dbEx);
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();

                _logger.LogWarning(
                    ex,
                    "Error al cancelar donativo. DonativoId: {DonativoId}",
                    id_donativo
                );

                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error inesperado al cancelar donativo. DonativoId: {DonativoId}",
                    id_donativo
                );

                throw new InvalidOperationException($"Error inesperado al cancelar donativo: {ex.Message}", ex);
            }
        }
    }
}