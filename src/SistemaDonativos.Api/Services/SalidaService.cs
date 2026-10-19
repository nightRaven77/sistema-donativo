using apiDonativos.Models;
using apiDonativos.Models.Catalogos;
using apiDonativos.Models.Dtos;
using apiDonativos.Services.Base;  // ✅ NUEVO
using apiDonativos.Helpers;  // ✅ M2: NUEVO
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;  // ✅ M3: NUEVO
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apiDonativos.Services 
{
    public interface ISalida
    {
        // ✅ FASE 2: Agregado parámetro de archivos
        Task<int> CreateAsync(Salida salida, List<SalidaDetalle> detalles, List<SalidasArchivo> archivos, int id_usuario);
        Task<List<SalidaListadoResponse>> GetAllAsync();
        Task<List<SalidaListadoResponse>> GetByIdUsuarioAsync(int id_usuario);
        Task<SalidaDetalleConArchivosResponse> GetDetailById(int id_salida);
        Task<SalidaCompleteResponse> GetByIdAsync(int id_salida);
        Task<bool> DeleteAsync(int id_salida, int id_usuario);
        Task<bool> UpdateAsync(int id_salida, UpdateSalidaRequest request, int id_usuario);
    }

    /// <summary>
    /// Servicio para gestión de salidas
    /// ✅ FASE 3: Heredado de InventarioServiceBase con funcionalidad completa
    /// </summary>
    public class SalidaService : InventarioServiceBase<SalidaService>, ISalida
    {
        public SalidaService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<SalidaService> logger)
            : base(context, configuration, logger)
        {
        }

        // ✅ FASE 3: Métodos de validación, logging y registro de movimientos heredados

        // ✅ M2: Usar FolioGenerator centralizado
        private async Task<string> GenerateFolioAsync(int year)
        {
            return await FolioGenerator.GenerarFolioConAnioAsync(
                _context.salidas,
                "SAL",
                year,
                s => s.folio,
                digits: 5
            );
        }

        /// <summary>
        /// Crea una salida de inventario
        /// ✅ REFACTORIZADO: Valida inventario ANTES de crear registros
        /// ✅ FASE 2: Soporte de archivos agregado
        /// </summary>
        public async Task<int> CreateAsync(Salida salida, List<SalidaDetalle> detalles, List<SalidasArchivo> archivos, int id_usuario)
        {
            // ============================================
            // VALIDACIONES DE PARÁMETROS
            // ============================================
            if (salida == null)
                throw new ArgumentNullException(nameof(salida), "La salida no puede ser nula");

            if (detalles == null || !detalles.Any())
                throw new ArgumentException("Al menos un detalle es requerido", nameof(detalles));

            // ✅ NUEVO: Validar que el almacén de origen esté presente
            if (salida.id_almacen_origen <= 0)
                throw new ArgumentException("ID de almacén de origen inválido", nameof(salida.id_almacen_origen));

            if (id_usuario <= 0)
                throw new ArgumentException("ID de usuario inválido", nameof(id_usuario));

            // ✅ VALIDACIÓN: Motivo es requerido (campo NOT NULL en SQL)
            if (string.IsNullOrWhiteSpace(salida.motivo))
                throw new ArgumentException("El motivo de la salida es requerido", nameof(salida.motivo));

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

            // ✅ M5: Iniciar medición de performance
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // ✅ SEGURIDAD: Log inicio de operación crítica
            _logger.LogInformation(
                "Iniciando creación de salida. Usuario: {UserId}, Almacén: {AlmacenId}, Artículos: {CantidadArticulos}",
                id_usuario,
                salida.id_almacen_origen,
                detalles.Count
            );

            // ✅ SEGURIDAD: Transacción con isolation level por defecto (ReadCommitted)
            using var transaction = await _context.Database.BeginTransactionAsync();

            // ✅ M3: Configurar timeout desde configuración
            _context.Database.SetCommandTimeout(_commandTimeout);
            try
            {
                var ahora = DateTime.Now;

                // ✅ USAR: Almacén del objeto salida (no parámetro separado)
                var id_almacen = salida.id_almacen_origen;

                // ============================================
                // ✅ PASO 1: AGRUPAR Y PREPARAR DETALLES
                // ============================================
                var detallesAgrupados = detalles
                    .GroupBy(d => d.id_articulo)
                    .Select(g => new 
                    {
                        id_articulo = g.Key,
                        cantidad_total = g.Sum(d => d.cantidad_entregada)
                    })
                    .ToList();

                // ============================================
                // ✅ PASO 2: VALIDAR INVENTARIO (ANTES DE CREAR)
                // ============================================
                var idsArticulos = detallesAgrupados.Select(d => d.id_articulo).ToList();
                var inventariosExistentes = await _context.inventario
                    .Where(i => i.id_almacen == id_almacen && idsArticulos.Contains(i.id_articulo))
                    .ToDictionaryAsync(i => i.id_articulo);

                foreach (var detalle in detallesAgrupados)
                {
                    // Validar existencia
                    if (!inventariosExistentes.TryGetValue(detalle.id_articulo, out var inventarioExistente))
                    {
                        // Obtener nombre del artículo para mensaje más claro
                        var articulo = await _context.Articulos
                            .AsNoTracking()
                            .FirstOrDefaultAsync(a => a.id_articulo == detalle.id_articulo);

                        var nombreArticulo = articulo?.nombre ?? $"ID {detalle.id_articulo}";

                        throw new InvalidOperationException(
                            $"El artículo '{nombreArticulo}' no existe en el inventario del almacén {id_almacen}");
                    }

                    // Validar cantidad suficiente
                    if (inventarioExistente.cantidad < detalle.cantidad_total)
                    {
                        // Obtener nombre del artículo para mensaje más claro
                        var articulo = await _context.Articulos
                            .AsNoTracking()
                            .FirstOrDefaultAsync(a => a.id_articulo == detalle.id_articulo);

                        var nombreArticulo = articulo?.nombre ?? $"ID {detalle.id_articulo}";

                        throw new InvalidOperationException(
                            $"Inventario insuficiente para el artículo '{nombreArticulo}'. " +
                            $"Disponible: {inventarioExistente.cantidad}, Solicitado: {detalle.cantidad_total}");
                    }
                }

                // ============================================
                // ✅ PASO 3: CREAR SALIDA (Ya validado) + FASE 1: Retry logic
                // ============================================
                string folioSalida = null;
                await RetryHelper.ExecuteWithRetryAsync(async () =>
                {
                    folioSalida = await GenerateFolioAsync(salida.fecha_salida.Year);
                    salida.folio = folioSalida;
                    // ✅ id_almacen_origen ya viene asignado en el objeto salida
                    salida.id_usuario_entrega = id_usuario;
                    salida.fecha_registro = ahora;

                    _context.salidas.Add(salida);
                    await _context.SaveChangesAsync();

                    return salida.id_salida;
                });

                // ============================================
                // ✅ PASO 4: AGREGAR DETALLES
                // ============================================
                foreach (var detalle in detalles)
                {
                    detalle.id_salida = salida.id_salida;
                }
                _context.salidas_detalle.AddRange(detalles);

                // ============================================
                // ✅ PASO 4.5: AGREGAR ARCHIVOS (FASE 2)
                // ============================================
                if (archivos != null && archivos.Any())
                {
                    foreach (var archivo in archivos)
                    {
                        archivo.id_salida = salida.id_salida;
                        archivo.fecha_actualiza = ahora;
                        archivo.activo = true;
                    }
                    _context.salidas_archivos.AddRange(archivos);
                }

                // ============================================
                // ✅ PASO 5: ACTUALIZAR INVENTARIO
                // ============================================
                foreach (var detalle in detallesAgrupados)
                {
                    var inventarioExistente = inventariosExistentes[detalle.id_articulo];

                    // Actualizar la tabla inventario (reducir cantidad)
                    inventarioExistente.cantidad -= detalle.cantidad_total;
                    inventarioExistente.fecha_ultima_salida = ahora;
                    inventarioExistente.fecha_actualizacion = ahora;
                    _context.inventario.Update(inventarioExistente);

                    // ✅ PASO 6: Registrar movimiento de salida (usando clase base)
                    RegistrarMovimiento(
                        id_almacen,
                        detalle.id_articulo,
                        -detalle.cantidad_total, // Negativo para salida
                        "Salida",
                        ahora,
                        id_usuario,
                        id_salida: salida.id_salida  // ✅ M1: Parámetro con nombre
                    );
                }

                // Registrar en bitácora
                var bitacora = new Bitacora 
                {
                    id_usuario = id_usuario,
                    modulo = "Salida",
                    submodulo = "Registro",
                    accion = "Crear",
                    descripcion = $"Se registró salida {folioSalida} con {detalles.Count} artículos"
                };
                _context.Bitacoras.Add(bitacora);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // ✅ M5: Detener medición
                stopwatch.Stop();

                // ✅ SEGURIDAD: Log éxito de operación con métricas
                _logger.LogInformation(
                    "Salida creada exitosamente en {ElapsedMs}ms. Folio: {Folio}, ID: {SalidaId}, Usuario: {UserId}",
                    stopwatch.ElapsedMilliseconds,
                    folioSalida,
                    salida.id_salida,
                    id_usuario
                );

                return salida.id_salida;  // ✅ CORREGIDO
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();

                // ✅ SEGURIDAD: Log error de BD (sin exponer estructura en producción)
                _logger.LogError(
                    dbEx,
                    "Error de base de datos al crear salida. Usuario: {UserId}, Almacén: {AlmacenId}",
                    id_usuario,
                    salida.id_almacen_origen
                );

                // ✅ MEJORADO: Extraer información detallada del error SQL
                var errorMessage = "Error al crear Salida en la base de datos";
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                // Detectar tipos comunes de errores SQL
                if (innerMessage.Contains("Cannot insert the value NULL"))
                {
                    // Extraer nombre de columna del mensaje
                    var match = System.Text.RegularExpressions.Regex.Match(
                        innerMessage, @"column '([^']+)'");
                    var columnName = match.Success ? match.Groups[1].Value : "desconocida";

                    errorMessage = $"Campo requerido faltante: '{columnName}' no puede ser nulo";
                }
                else if (innerMessage.Contains("FOREIGN KEY constraint"))
                {
                    // Error de llave foránea
                    if (innerMessage.Contains("id_almacen"))
                        errorMessage = "El almacén especificado no existe";
                    else if (innerMessage.Contains("id_beneficiario"))
                        errorMessage = "El beneficiario especificado no existe";
                    else if (innerMessage.Contains("id_articulo"))
                        errorMessage = "Uno o más artículos especificados no existen";
                    else
                        errorMessage = "Referencia inválida: uno de los IDs proporcionados no existe en el sistema";
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
                    // Error genérico pero con el mensaje real de SQL
                    errorMessage = $"Error de base de datos: {innerMessage}";
                }

                throw new InvalidOperationException(errorMessage, dbEx);
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();

                // ✅ SEGURIDAD: Log error de validación
                _logger.LogWarning(
                    ex,
                    "Error de validación al crear salida. Usuario: {UserId}, Error: {ErrorMessage}",
                    id_usuario,
                    ex.Message
                );

                throw;  // Re-lanzar tal cual (ya tiene mensaje descriptivo)
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // ✅ SEGURIDAD: Log error inesperado
                _logger.LogError(
                    ex,
                    "Error inesperado al crear salida. Usuario: {UserId}",
                    id_usuario
                );

                throw new InvalidOperationException(
                    $"Error inesperado al crear salida: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el listado de salidas con información del beneficiario, almacén y cantidad de artículos
        /// ✅ ACTUALIZADO: Incluye nombre del almacén, tipo de salida y motivo
        /// ✅ LEFT JOIN: Incluye salidas sin beneficiario (merma/baja)
        /// </summary>
        public async Task<List<SalidaListadoResponse>> GetAllAsync()
        {
            return await _context.salidas
                // ✅ LEFT JOIN con Beneficiarios (para incluir salidas de merma/baja sin beneficiario)
                .GroupJoin(
                    _context.Beneficiarios,
                    s => s.id_beneficiario,
                    b => b.id_beneficiario,
                    (s, bs) => new { salida = s, beneficiarios = bs }
                )
                .SelectMany(
                    x => x.beneficiarios.DefaultIfEmpty(),
                    (x, b) => new { x.salida, beneficiario = b }
                )
                // INNER JOIN con Almacenes (siempre debe tener almacén)
                .Join(
                    _context.Almacenes,
                    x => x.salida.id_almacen_origen,
                    a => a.id_almacen,
                    (x, a) => new { x.salida, x.beneficiario, almacen = a }
                )
                .GroupBy(x => new
                {
                    x.salida.id_salida,
                    x.salida.folio,
                    x.almacen.nombre,
                    nombreBeneficiario = x.beneficiario != null ? x.beneficiario.nombre : null,  // ✅ NULL-safe
                    x.salida.tipo_salida,
                    x.salida.motivo,
                    x.salida.fecha_salida,
                    x.salida.estatus
                })
                .Select(g => new SalidaListadoResponse
                {
                    IdSalida = g.Key.id_salida,
                    Folio = g.Key.folio,
                    NombreAlmacen = g.Key.nombre,
                    NombreBeneficiario = g.Key.nombreBeneficiario ?? "N/A",  // ✅ Default para merma/baja
                    TipoSalida = g.Key.tipo_salida,
                    Motivo = g.Key.motivo,
                    FechaSalida = g.Key.fecha_salida,
                    Estatus = g.Key.estatus,
                    CantidadArticulos = _context.salidas_detalle
                        .Where(sd => sd.id_salida == g.Key.id_salida)
                        .Count()
                })
                .OrderByDescending(x => x.Folio)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene el listado de salidas filtradas por el usuario que las registró
        /// ✅ ACTUALIZADO: Incluye nombre del almacén, tipo de salida y motivo
        /// ✅ LEFT JOIN: Incluye salidas sin beneficiario (merma/baja)
        /// </summary>
        public async Task<List<SalidaListadoResponse>> GetByIdUsuarioAsync(int id_usuario)
        {
            if (id_usuario <= 0)
                throw new ArgumentException("ID de usuario inválido", nameof(id_usuario));

            return await _context.salidas
                .Where(s => s.id_usuario_entrega == id_usuario)
                // ✅ LEFT JOIN con Beneficiarios (para incluir salidas de merma/baja sin beneficiario)
                .GroupJoin(
                    _context.Beneficiarios,
                    s => s.id_beneficiario,
                    b => b.id_beneficiario,
                    (s, bs) => new { salida = s, beneficiarios = bs }
                )
                .SelectMany(
                    x => x.beneficiarios.DefaultIfEmpty(),
                    (x, b) => new { x.salida, beneficiario = b }
                )
                // INNER JOIN con Almacenes (siempre debe tener almacén)
                .Join(
                    _context.Almacenes,
                    x => x.salida.id_almacen_origen,
                    a => a.id_almacen,
                    (x, a) => new { x.salida, x.beneficiario, almacen = a }
                )
                .GroupBy(x => new
                {
                    x.salida.id_salida,
                    x.salida.folio,
                    x.almacen.nombre,
                    nombreBeneficiario = x.beneficiario != null ? x.beneficiario.nombre : null,  // ✅ NULL-safe
                    x.salida.tipo_salida,
                    x.salida.motivo,
                    x.salida.fecha_salida,
                    x.salida.estatus
                })
                .Select(g => new SalidaListadoResponse
                {
                    IdSalida = g.Key.id_salida,
                    Folio = g.Key.folio,
                    NombreAlmacen = g.Key.nombre,
                    NombreBeneficiario = g.Key.nombreBeneficiario ?? "N/A",  // ✅ Default para merma/baja
                    TipoSalida = g.Key.tipo_salida,
                    Motivo = g.Key.motivo,
                    FechaSalida = g.Key.fecha_salida,
                    Estatus = g.Key.estatus,
                    CantidadArticulos = _context.salidas_detalle
                        .Where(sd => sd.id_salida == g.Key.id_salida)
                        .Count()
                })
                .OrderByDescending(x => x.Folio)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene los detalles de una salida con información de artículos
        /// </summary>
        public async Task<SalidaDetalleConArchivosResponse> GetDetailById(int id_salida)
        {
            if (id_salida <= 0)
                throw new ArgumentException("ID de salida inválido", nameof(id_salida));

            var detalles = await _context.salidas
                .Where(s => s.id_salida == id_salida)  // ✅ CORREGIDO
                .Join(_context.salidas_detalle,
                      salida => salida.id_salida,  // ✅ CORREGIDO
                      detalle => detalle.id_salida,
                      (salida, detalle) => new { salida, detalle })
                .Join(_context.Articulos,
                      combined => combined.detalle.id_articulo,
                      articulo => articulo.id_articulo,
                      (combined, articulo) => new { combined.salida, combined.detalle, articulo })
                .Join(_context.Categorias,
                      combined => combined.articulo.id_categoria,
                      categoria => categoria.id_categoria,
                      (combined, categoria) => new { combined.salida, combined.detalle, combined.articulo, categoria })
                .Select(result => new SalidaDetalleListadoResponse
                {
                    IdArticulo = result.articulo.id_articulo,
                    NombreArticulo = result.articulo.nombre,
                    IdCategoria = result.categoria.id_categoria,
                    CategoriaArticulo = result.categoria.nombre,
                    UnidadMedida = result.detalle.unidad_medida,
                    CantidadEntregada = result.detalle.cantidad_entregada,
                    ValorUnitario = result.detalle.valor_unitario,
                    ValorTotal = result.detalle.cantidad_entregada * result.detalle.valor_unitario
                })
                .AsNoTracking()
                .ToListAsync();

            return new SalidaDetalleConArchivosResponse
            {
                Detalles = detalles
            };
        }

        /// <summary>
        /// Obtiene una salida completa con todos sus detalles
        /// ✅ LEFT JOIN: Incluye salidas sin beneficiario (merma/baja)
        /// </summary>
        public async Task<SalidaCompleteResponse> GetByIdAsync(int id_salida)
        {
            if (id_salida <= 0)
                throw new ArgumentException("ID de salida inválido", nameof(id_salida));

            var salida = await _context.salidas
                .Where(s => s.id_salida == id_salida)
                // ✅ LEFT JOIN con Beneficiarios (para incluir salidas de merma/baja sin beneficiario)
                .GroupJoin(
                    _context.Beneficiarios,
                    s => s.id_beneficiario,
                    b => b.id_beneficiario,
                    (s, bs) => new { salida = s, beneficiarios = bs }
                )
                .SelectMany(
                    x => x.beneficiarios.DefaultIfEmpty(),
                    (x, b) => new { x.salida, beneficiario = b }
                )
                // INNER JOIN con Almacenes (siempre debe tener almacén)
                .Join(
                    _context.Almacenes,
                    x => x.salida.id_almacen_origen,
                    a => a.id_almacen,
                    (x, a) => new { x.salida, x.beneficiario, almacen = a }
                )
                .Select(x => new
                {
                    x.salida.id_salida,
                    x.salida.folio,
                    x.salida.id_beneficiario,
                    x.salida.id_almacen_origen,
                    nombre = x.beneficiario != null ? x.beneficiario.nombre : null,  // ✅ NULL-safe
                    nombre_almacen = x.almacen.nombre,
                    x.salida.tipo_salida,
                    x.salida.motivo,
                    x.salida.fecha_salida,
                    x.salida.estatus,
                    x.salida.observaciones,
                    x.salida.fecha_entrega,
                    x.salida.recibio
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (salida == null)
                return null;

            var detalles = await _context.salidas_detalle
                .Where(sd => sd.id_salida == id_salida)
                .Join(
                    _context.Articulos,
                    sd => sd.id_articulo,
                    art => art.id_articulo,
                    (sd, art) => new { detalle = sd, articulo = art }
                )
                .Select(sd => new SalidaDetalleResponse
                {
                    IdDetalle = sd.detalle.id_detalle,
                    IdArticulo = sd.detalle.id_articulo,
                    NombreArticulo = sd.articulo.nombre,
                    Categoria = sd.articulo.id_categoria,
                    CantidadEntregada = sd.detalle.cantidad_entregada,
                    ValorUnitario = sd.detalle.valor_unitario,
                    UnidadMedida = sd.detalle.unidad_medida,
                    Observaciones = sd.detalle.observaciones
                })
                .AsNoTracking()
                .ToListAsync();

            // ✅ NUEVO: Consultar archivos de la salida
            var archivos = await _context.salidas_archivos
                .Where(sa => sa.id_salida == id_salida && sa.activo)
                .Select(sa => new SalidaArchivoResponse
                {
                    IdArchivo = sa.id_archivo,
                    NombreArchivo = sa.nombre_original,
                    UrlArchivo = sa.url
                })
                .AsNoTracking()
                .ToListAsync();

            return new SalidaCompleteResponse
            {
                IdSalida = salida.id_salida,
                Folio = salida.folio,
                IdBeneficiario = salida.id_beneficiario ?? 0,
                NombreBeneficiario = salida.nombre ?? "N/A",  // ✅ NULL-safe para merma/baja
                IdAlmacenOrigen = salida.id_almacen_origen,
                NombreAlmacen = salida.nombre_almacen,
                TipoSalida = salida.tipo_salida,
                Motivo = salida.motivo,
                FechaSalida = salida.fecha_salida,
                Estatus = salida.estatus,
                Observaciones = salida.observaciones,
                FechaEntrega = salida.fecha_entrega,
                Recibio = salida.recibio,
                Detalles = detalles,
                Archivos = archivos
            };
        }

        /// <summary>
        /// Elimina una salida y revierte el inventario
        /// </summary>
        public async Task<bool> DeleteAsync(int id_salida, int id_usuario)
        {
            if (id_salida <= 0)
                throw new ArgumentException("ID de salida inválido", nameof(id_salida));

            _logger.LogInformation(
                "Iniciando eliminación de salida. SalidaId: {SalidaId}, Usuario: {UserId}",
                id_salida,
                id_usuario
            );

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);  // ✅ Timeout de 30s
            try
            {
                var salida = await _context.salidas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.id_salida == id_salida);  // ✅ CORREGIDO

                if (salida == null)
                    throw new InvalidOperationException("Salida no encontrada");

                // Obtener detalles para revertir inventario
                var detalles = await _context.salidas_detalle
                    .Where(sd => sd.id_salida == id_salida)
                    .AsNoTracking()
                    .ToListAsync();

                // Revertir las cantidades al inventario (sumar de vuelta)
                var ahora = DateTime.Now;
                var idsArticulos = detalles.Select(d => d.id_articulo).Distinct().ToList();
                var inventariosExistentes = await _context.inventario
                    .Where(i => i.id_almacen == salida.id_almacen_origen && idsArticulos.Contains(i.id_articulo))
                    .ToDictionaryAsync(i => i.id_articulo);

                foreach (var detalle in detalles)
                {
                    if (inventariosExistentes.TryGetValue(detalle.id_articulo, out var inventarioExistente))
                    {
                        // Sumar la cantidad que se había restado
                        inventarioExistente.cantidad += detalle.cantidad_entregada;
                        inventarioExistente.fecha_actualizacion = ahora;
                        _context.inventario.Update(inventarioExistente);

                        // ✅ Registrar reversión de movimiento (entrada de vuelta)
                        RegistrarMovimiento(
                            salida.id_almacen_origen,
                            detalle.id_articulo,
                            detalle.cantidad_entregada, // Positivo para entrada
                            "Salida-Eliminada",
                            ahora,
                            id_usuario,
                            salida.id_salida  // ✅ CORREGIDO
                        );
                    }
                }

                _context.salidas.Remove(salida);

                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Salida",
                    submodulo = "Eliminación",
                    accion = "Eliminar",
                    descripcion = $"Se eliminó salida {salida.folio} y se revirtió el inventario"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Salida eliminada exitosamente. Folio: {Folio}, Usuario: {UserId}",
                    salida.folio,
                    id_usuario
                );

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    dbEx,
                    "Error de BD al eliminar salida. SalidaId: {SalidaId}, Usuario: {UserId}",
                    id_salida,
                    id_usuario
                );

                var errorMessage = "Error al eliminar salida en la base de datos";
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                // Detectar errores comunes
                if (innerMessage.Contains("REFERENCE constraint"))
                {
                    errorMessage = "No se puede eliminar: la salida tiene registros dependientes (archivos, movimientos)";
                }
                else
                {
                    errorMessage = $"Error de base de datos al eliminar: {innerMessage}";
                }

                throw new InvalidOperationException(errorMessage, dbEx);
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();

                _logger.LogWarning(
                    ex,
                    "Error al eliminar salida. SalidaId: {SalidaId}, Usuario: {UserId}",
                    id_salida,
                    id_usuario
                );

                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error inesperado al eliminar salida. SalidaId: {SalidaId}, Usuario: {UserId}",
                    id_salida,
                    id_usuario
                );

                throw new InvalidOperationException($"Error inesperado al eliminar salida: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza una salida existente con sus detalles
        /// </summary>
        public async Task<bool> UpdateAsync(int id_salida, UpdateSalidaRequest request, int id_usuario)
        {
            if (id_salida <= 0)
                throw new ArgumentException("ID de salida inválido", nameof(id_salida));

            if (request == null)
                throw new ArgumentNullException(nameof(request), "La solicitud de actualización no puede ser nula");

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);  // ✅ Timeout de 30s

            try
            {
                var salida = await _context.salidas.FindAsync(id_salida);
                if (salida == null)
                    throw new InvalidOperationException("Salida no encontrada");

                // Actualizar propiedades simples
                salida.fecha_salida = request.FechaSalida;
                salida.id_beneficiario = request.IdBeneficiario;
                salida.tipo_salida = request.TipoSalida.ToLower();  // ✅ NUEVO
                salida.motivo = request.Motivo;  // ✅ AGREGADO
                salida.estatus = request.Estatus;
                salida.observaciones = request.Observaciones;
                salida.fecha_entrega = request.FechaEntrega ?? salida.fecha_entrega;
                salida.recibio = request.Recibio;

                _context.salidas.Update(salida);

                // Manejo de detalles
                if (request.Detalles != null && request.Detalles.Any())
                {
                    var detallesActuales = await _context.salidas_detalle
                        .Where(sd => sd.id_salida == id_salida)
                        .ToListAsync();

                    var idsDetallesEnRequest = request.Detalles
                        .Where(d => d.IdDetalle.HasValue && d.IdDetalle.Value > 0)
                        .Select(d => d.IdDetalle.Value)
                        .ToList();

                    // Detalles a eliminar
                    var detallesAEliminar = detallesActuales
                        .Where(sd => !idsDetallesEnRequest.Contains(sd.id_detalle))
                        .ToList();

                    // Pre-cargar inventario de todos los artículos involucrados en una sola consulta
                    var idsArticulosInvolucrados = detallesActuales.Select(d => d.id_articulo)
                        .Union(request.Detalles.Select(d => d.IdArticulo))
                        .Distinct()
                        .ToList();
                    var inventariosDict = await _context.inventario
                        .Where(i => i.id_almacen == salida.id_almacen_origen && idsArticulosInvolucrados.Contains(i.id_articulo))
                        .ToDictionaryAsync(i => i.id_articulo);

                    // Revertir inventario para detalles eliminados
                    foreach (var detalleEliminado in detallesAEliminar)
                    {
                        if (inventariosDict.TryGetValue(detalleEliminado.id_articulo, out var inventario))
                        {
                            inventario.cantidad += detalleEliminado.cantidad_entregada; // Sumar de vuelta
                            inventario.fecha_actualizacion = DateTime.Now;
                            _context.inventario.Update(inventario);
                        }
                    }

                    _context.salidas_detalle.RemoveRange(detallesAEliminar);

                    // Procesar detalles del request
                    foreach (var detalle in request.Detalles)
                    {
                        if (detalle.IdDetalle.HasValue && detalle.IdDetalle.Value > 0)
                        {
                            // Actualizar detalle existente
                            var detalleExistente = detallesActuales
                                .FirstOrDefault(d => d.id_detalle == detalle.IdDetalle);

                            if (detalleExistente != null)
                            {
                                // Verificar si cambió el artículo
                                if (detalleExistente.id_articulo != detalle.IdArticulo)
                                {
                                    // Revertir inventario del artículo viejo
                                    if (inventariosDict.TryGetValue(detalleExistente.id_articulo, out var inventarioViejo))
                                    {
                                        inventarioViejo.cantidad += detalleExistente.cantidad_entregada;
                                        inventarioViejo.fecha_actualizacion = DateTime.Now;
                                        _context.inventario.Update(inventarioViejo);
                                    }

                                    // Validar y descontar del artículo nuevo
                                    if (!inventariosDict.TryGetValue(detalle.IdArticulo, out var inventarioNuevo))
                                        throw new InvalidOperationException($"El artículo {detalle.IdArticulo} no existe en inventario");

                                    if (inventarioNuevo.cantidad < detalle.CantidadEntregada)
                                        throw new InvalidOperationException($"Inventario insuficiente para artículo {detalle.IdArticulo}");

                                    inventarioNuevo.cantidad -= detalle.CantidadEntregada;
                                    inventarioNuevo.fecha_ultima_salida = DateTime.Now;
                                    inventarioNuevo.fecha_actualizacion = DateTime.Now;
                                    _context.inventario.Update(inventarioNuevo);
                                }
                                else
                                {
                                    // Mismo artículo, solo ajustar por diferencia de cantidad
                                    var diferenciaCantidad = detalle.CantidadEntregada - detalleExistente.cantidad_entregada;

                                    if (diferenciaCantidad != 0)
                                    {
                                        if (inventariosDict.TryGetValue(detalle.IdArticulo, out var inventario))
                                        {
                                            inventario.cantidad -= diferenciaCantidad;

                                            if (inventario.cantidad < 0)
                                            {
                                                throw new InvalidOperationException(
                                                    $"Inventario insuficiente para el artículo ID {detalle.IdArticulo}");
                                            }

                                            inventario.fecha_actualizacion = DateTime.Now;
                                            _context.inventario.Update(inventario);
                                        }
                                    }
                                }

                                // Actualizar detalle
                                detalleExistente.id_articulo = detalle.IdArticulo;
                                detalleExistente.cantidad_entregada = detalle.CantidadEntregada;
                                detalleExistente.valor_unitario = detalle.ValorUnitario;
                                detalleExistente.observaciones = detalle.Observaciones;
                                detalleExistente.unidad_medida = detalle.UnidadMedida;

                                _context.salidas_detalle.Update(detalleExistente);
                            }
                        }
                        else
                        {
                            // Agregar nuevo detalle
                            var nuevoDetalle = new SalidaDetalle
                            {
                                id_salida = id_salida,
                                id_articulo = detalle.IdArticulo,
                                cantidad_entregada = detalle.CantidadEntregada,
                                valor_unitario = detalle.ValorUnitario,
                                observaciones = detalle.Observaciones,
                                unidad_medida = detalle.UnidadMedida
                            };
                            _context.salidas_detalle.Add(nuevoDetalle);

                            // Reducir inventario
                            if (inventariosDict.TryGetValue(detalle.IdArticulo, out var inventarioExistente))
                            {
                                if (inventarioExistente.cantidad < detalle.CantidadEntregada)
                                {
                                    throw new InvalidOperationException(
                                        $"Inventario insuficiente para el artículo ID {detalle.IdArticulo}");
                                }

                                inventarioExistente.cantidad -= detalle.CantidadEntregada;
                                inventarioExistente.fecha_ultima_salida = DateTime.Now;
                                inventarioExistente.fecha_actualizacion = DateTime.Now;
                                _context.inventario.Update(inventarioExistente);
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    $"El artículo ID {detalle.IdArticulo} no existe en el inventario");
                            }
                        }
                    }
                }

                // ✅ NUEVO: Manejo de archivos
                if (request.Archivos != null)
                {
                    // Obtener archivos actuales
                    var archivosActuales = await _context.salidas_archivos
                        .Where(sa => sa.id_salida == id_salida)
                        .ToListAsync();

                    // Identificar IDs de archivos en el request
                    var idsArchivosEnRequest = request.Archivos
                        .Where(a => a.IdArchivo.HasValue && a.IdArchivo.Value > 0)
                        .Select(a => a.IdArchivo.Value)
                        .ToList();

                    // Archivos a eliminar (soft delete)
                    var archivosAEliminar = archivosActuales
                        .Where(sa => !idsArchivosEnRequest.Contains(sa.id_archivo))
                        .ToList();

                    foreach (var archivoEliminar in archivosAEliminar)
                    {
                        archivoEliminar.activo = false;
                        archivoEliminar.fecha_actualiza = DateTime.Now;
                        _context.salidas_archivos.Update(archivoEliminar);
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
                                archivoExistente.fecha_actualiza = DateTime.Now;
                                _context.salidas_archivos.Update(archivoExistente);
                            }
                        }
                        else
                        {
                            // Agregar nuevo archivo
                            var nuevoArchivo = new SalidasArchivo
                            {
                                id_salida = id_salida,
                                url = archivo.Url,
                                nombre_original = archivo.NombreOriginal,
                                activo = true,
                                fecha_actualiza = DateTime.Now
                            };
                            _context.salidas_archivos.Add(nuevoArchivo);
                        }
                    }
                }

                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Salida",
                    submodulo = "Actualización",
                    accion = "Actualizar",
                    descripcion = $"Se actualizó salida {salida.folio}"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();

                var errorMessage = "Error al actualizar salida en la base de datos";
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (innerMessage.Contains("Cannot insert the value NULL"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(
                        innerMessage, @"column '([^']+)'");
                    var columnName = match.Success ? match.Groups[1].Value : "desconocida";
                    errorMessage = $"Campo requerido faltante en actualización: '{columnName}' no puede ser nulo";
                }
                else if (innerMessage.Contains("FOREIGN KEY constraint"))
                {
                    errorMessage = "Referencia inválida: uno de los IDs proporcionados no existe";
                }
                else
                {
                    errorMessage = $"Error de base de datos al actualizar: {innerMessage}";
                }

                throw new InvalidOperationException(errorMessage, dbEx);
            }
            catch (InvalidOperationException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Error inesperado al actualizar salida: {ex.Message}", ex);
            }
        }
    }
}
