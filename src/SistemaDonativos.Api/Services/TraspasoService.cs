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
    public interface ITraspaso
    {
        // ✅ FASE 2: Agregado parámetro de archivos
        Task<int> CreateAsync(Traspaso traspaso, List<TraspasoDetalle> detalles, List<TraspasoArchivo> archivos, int id_usuario, bool is_instantaneo = false);
        Task<List<TraspasoListadoResponse>> GetAllAsync();
        Task<List<TraspasoListadoResponse>> GetByIdUsuarioAsync(int id_usuario, int id_almacen);
        Task<TraspasoCompleteResponse> GetByIdAsync(int id_traspaso);
        Task<bool> RecibirAsync(int id_traspaso, int id_usuario_recibe, List<TraspasoArchivo> archivos);  // ✅ ACTUALIZADO
        Task<bool> CancelarAsync(int id_traspaso, int id_usuario, string motivo_cancelacion);
        Task<bool> DeleteAsync(int id_traspaso, int id_usuario);
    }

    /// <summary>
    /// Servicio para gestión de traspasos
    /// ✅ FASE 3: Heredado de InventarioServiceBase con funcionalidad completa
    /// </summary>
    public class TraspasoService : InventarioServiceBase<TraspasoService>, ITraspaso
    {
        public TraspasoService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<TraspasoService> logger)
            : base(context, configuration, logger)
        {
        }

        // ✅ FASE 3: Métodos de validación, logging y registro de movimientos heredados

        // ✅ M2: Usar FolioGenerator centralizado
        private async Task<string> GenerateFolioAsync(int year)
        {
            return await FolioGenerator.GenerarFolioConAnioAsync(
                _context.Traspasos,
                "TRAS",
                year,
                t => t.folio,
                digits: 5
            );
        }

        /// <summary>
        /// Crea un traspaso entre almacenes
        /// Si is_instantaneo = true, mueve inventario inmediatamente y marca como Recibido
        /// Si is_instantaneo = false, crea en estado Pendiente sin mover inventario
        /// ✅ FASE 2: Soporte de archivos agregado
        /// </summary>
        public async Task<int> CreateAsync(Traspaso traspaso, List<TraspasoDetalle> detalles, List<TraspasoArchivo> archivos, int id_usuario, bool is_instantaneo = false)
        {
            if (traspaso == null)
                throw new ArgumentNullException(nameof(traspaso), "El traspaso no puede ser nulo");

            if (detalles == null || !detalles.Any())
                throw new ArgumentException("Al menos un detalle es requerido", nameof(detalles));

            if (!traspaso.almacen_origen.HasValue || traspaso.almacen_origen <= 0)
                throw new ArgumentException("Almacén de origen inválido");

            if (!traspaso.almacen_destino.HasValue || traspaso.almacen_destino <= 0)
                throw new ArgumentException("Almacén de destino inválido");

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

                    // Validar tipo de archivo
                    if (archivo.tipo_archivo != "Envio" && archivo.tipo_archivo != "Recepcion")
                    {
                        throw new ArgumentException($"Tipo de archivo inválido: '{archivo.tipo_archivo}'. Debe ser 'Envio' o 'Recepcion'", nameof(archivos));
                    }
                }
            }

            if (traspaso.almacen_origen == traspaso.almacen_destino)
                throw new InvalidOperationException("El almacén de origen no puede ser igual al de destino");

            // ✅ SEGURIDAD: Log inicio de operación
            _logger.LogInformation(
                "Iniciando traspaso. Usuario: {UserId}, Origen: {OrigenId}, Destino: {DestinoId}, Instantáneo: {Instantaneo}",
                id_usuario,
                traspaso.almacen_origen,
                traspaso.almacen_destino,
                is_instantaneo
            );

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);  // ✅ SEGURIDAD: Timeout de 30s

            try
            {
                // Validar que ambos almacenes existen
                var almacenOrigenExiste = await _context.Almacenes.AnyAsync(a => a.id_almacen == traspaso.almacen_origen);
                var almacenDestinoExiste = await _context.Almacenes.AnyAsync(a => a.id_almacen == traspaso.almacen_destino);

                if (!almacenOrigenExiste)
                    throw new InvalidOperationException($"El almacén de origen {traspaso.almacen_origen} no existe");

                if (!almacenDestinoExiste)
                    throw new InvalidOperationException($"El almacén de destino {traspaso.almacen_destino} no existe");

                // ✅ FASE 1: Generar folio y crear traspaso con retry logic
                string folio = null;
                await RetryHelper.ExecuteWithRetryAsync(async () =>
                {
                    folio = await GenerateFolioAsync(traspaso.fecha_traspaso.Year);
                    traspaso.folio = folio;
                    traspaso.id_usuario_envia = id_usuario;

                    // Determinar estatus según tipo de traspaso
                    if (is_instantaneo)
                    {
                        traspaso.estatus = "Recibido";
                        traspaso.fecha_recibido = DateTime.Now;
                        traspaso.id_usuario_recibe = id_usuario; // Mismo usuario en instantáneo
                    }
                    else
                    {
                        traspaso.estatus = "Pendiente";
                    }

                    _context.Traspasos.Add(traspaso);
                    await _context.SaveChangesAsync();

                    return traspaso.id_traspaso;
                });

                // Agregar detalles
                foreach (var detalle in detalles)
                {
                    detalle.id_traspaso = traspaso.id_traspaso;
                }
                _context.traspasos_detalle.AddRange(detalles);
                await _context.SaveChangesAsync();

                // ✅ FASE 2: Agregar archivos si se proporcionan
                if (archivos != null && archivos.Any())
                {
                    foreach (var archivo in archivos)
                    {
                        archivo.id_traspaso = traspaso.id_traspaso;
                        archivo.activo = true;
                    }
                    _context.traspasos_archivos.AddRange(archivos);
                    await _context.SaveChangesAsync();
                }

                // Agrupar detalles por artículo
                var detallesAgrupados = detalles
                    .GroupBy(d => d.id_articulo)
                    .Select(g => new
                    {
                        id_articulo = g.Key,
                        cantidad_total = g.Sum(d => d.cantidad ?? 0)
                    })
                    .ToList();

                var ahora = DateTime.Now;

                var idsArticulos = detallesAgrupados
                    .Where(d => d.id_articulo.HasValue)
                    .Select(d => d.id_articulo.Value)
                    .ToList();

                var inventariosOrigen = await _context.inventario
                    .Where(i => i.id_almacen == traspaso.almacen_origen && idsArticulos.Contains(i.id_articulo))
                    .ToDictionaryAsync(i => i.id_articulo);

                // Validar inventario en origen SIEMPRE (antes de crear)
                foreach (var detalle in detallesAgrupados)
                {
                    if (!detalle.id_articulo.HasValue || !inventariosOrigen.TryGetValue(detalle.id_articulo.Value, out var inventarioOrigen))
                    {
                        throw new InvalidOperationException(
                            $"El artículo {detalle.id_articulo} no existe en el inventario del almacén origen {traspaso.almacen_origen}");
                    }

                    if (inventarioOrigen.cantidad < detalle.cantidad_total)
                    {
                        throw new InvalidOperationException(
                            $"Inventario insuficiente en almacén origen para artículo {detalle.id_articulo}. " +
                            $"Disponible: {inventarioOrigen.cantidad}, Solicitado: {detalle.cantidad_total}");
                    }
                }

                // Si es instantáneo, mover inventario inmediatamente
                if (is_instantaneo)
                {
                    var inventariosDestino = await _context.inventario
                        .Where(i => i.id_almacen == traspaso.almacen_destino && idsArticulos.Contains(i.id_articulo))
                        .ToDictionaryAsync(i => i.id_articulo);

                    foreach (var detalle in detallesAgrupados)
                    {
                        var idArticulo = detalle.id_articulo.Value;

                        // Restar de origen
                        var inventarioOrigen = inventariosOrigen[idArticulo];
                        inventarioOrigen.cantidad -= detalle.cantidad_total;
                        inventarioOrigen.fecha_ultima_salida = ahora;
                        inventarioOrigen.fecha_actualizacion = ahora;
                        _context.inventario.Update(inventarioOrigen);

                        // Registrar movimiento de salida
                        RegistrarMovimiento(
                            traspaso.almacen_origen.Value, 
                            idArticulo, 
                            -detalle.cantidad_total, // Negativo para salida
                            "Traspaso-Salida", 
                            ahora, 
                            id_usuario, 
                            id_traspaso: traspaso.id_traspaso
                        );

                        // Sumar a destino
                        if (inventariosDestino.TryGetValue(idArticulo, out var inventarioDestino))
                        {
                            inventarioDestino.cantidad += detalle.cantidad_total;
                            inventarioDestino.fecha_ultima_entrada = ahora;
                            inventarioDestino.fecha_actualizacion = ahora;
                            _context.inventario.Update(inventarioDestino);
                        }
                        else
                        {
                            var nuevoInventario = new Inventario
                            {
                                id_almacen = traspaso.almacen_destino.Value,
                                id_articulo = idArticulo,
                                cantidad = detalle.cantidad_total,
                                fecha_ultima_entrada = ahora,
                                fecha_actualizacion = ahora
                            };
                            _context.inventario.Add(nuevoInventario);
                            inventariosDestino[idArticulo] = nuevoInventario;
                        }

                        // Registrar movimiento de entrada (usando clase base)
                        RegistrarMovimiento(
                            traspaso.almacen_destino.Value, 
                            idArticulo, 
                            detalle.cantidad_total, // Positivo para entrada
                            "Traspaso-Entrada", 
                            ahora, 
                            id_usuario, 
                            id_traspaso: traspaso.id_traspaso  // ✅ M1: Parámetro con nombre
                        );
                    }
                }

                // Registrar en bitácora
                var accion = is_instantaneo ? "Crear Instantáneo" : "Crear";
                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Traspaso",
                    submodulo = "Registro",
                    accion = accion,
                    descripcion = $"Se registró traspaso {folio} de almacén {traspaso.almacen_origen} a {traspaso.almacen_destino} con {detalles.Count} artículos"
                };
                _context.Bitacoras.Add(bitacora);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // ✅ SEGURIDAD: Log éxito
                _logger.LogInformation(
                    "Traspaso creado exitosamente. Folio: {Folio}, ID: {TraspasoId}, Usuario: {UserId}",
                    folio,
                    traspaso.id_traspaso,
                    id_usuario
                );

                return traspaso.id_traspaso;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();

                // ✅ SEGURIDAD: Log error BD
                _logger.LogError(
                    dbEx,
                    "Error de BD al crear traspaso. Usuario: {UserId}, Origen: {Origen}, Destino: {Destino}",
                    id_usuario,
                    traspaso.almacen_origen,
                    traspaso.almacen_destino
                );

                // ✅ MEJORADO: Mensajes claros
                var errorMessage = "Error al crear traspaso en la base de datos";
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
                    if (innerMessage.Contains("almacen"))
                        errorMessage = "Uno de los almacenes especificados no existe";
                    else if (innerMessage.Contains("articulo"))
                        errorMessage = "Uno o más artículos especificados no existen";
                    else
                        errorMessage = "Referencia inválida: uno de los IDs proporcionados no existe";
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
                    "Error de validación al crear traspaso. Usuario: {UserId}",
                    id_usuario
                );

                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error inesperado al crear traspaso. Usuario: {UserId}",
                    id_usuario
                );

                throw new InvalidOperationException($"Error inesperado al crear traspaso: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Marca un traspaso como Recibido y mueve el inventario
        /// Solo puede recibirse si está en estado Pendiente
        /// ✅ ACTUALIZADO: Soporte para archivos de recepción
        /// </summary>
        public async Task<bool> RecibirAsync(int id_traspaso, int id_usuario_recibe, List<TraspasoArchivo> archivos)
        {
            if (id_traspaso <= 0)
                throw new ArgumentException("ID de traspaso inválido", nameof(id_traspaso));

            // ✅ NUEVO: Validar archivos de recepción si se proporcionan
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

                    // Validar tipo de archivo - en recepción solo "Recepcion"
                    if (archivo.tipo_archivo != "Recepcion")
                    {
                        throw new ArgumentException(
                            $"En la recepción, el tipo de archivo debe ser 'Recepcion', no '{archivo.tipo_archivo}'", 
                            nameof(archivos));
                    }
                }
            }

            // ✅ SEGURIDAD: Log inicio
            _logger.LogInformation(
                "Iniciando recepción de traspaso. TraspasoId: {TraspasoId}, Usuario: {UserId}, Archivos: {CantArchivos}",
                id_traspaso,
                id_usuario_recibe,
                archivos?.Count ?? 0
            );

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);  // ✅ SEGURIDAD: Timeout 30s

            try
            {
                var traspaso = await _context.Traspasos.FindAsync(id_traspaso);

                if (traspaso == null)
                    throw new InvalidOperationException("Traspaso no encontrado");

                if (traspaso.estatus != "Pendiente")
                    throw new InvalidOperationException($"No se puede recibir un traspaso con estatus '{traspaso.estatus}'");

                // Obtener detalles
                var detalles = await _context.traspasos_detalle
                    .Where(td => td.id_traspaso == id_traspaso)
                    .ToListAsync();

                // Agrupar detalles
                var detallesAgrupados = detalles
                    .GroupBy(d => d.id_articulo)
                    .Select(g => new
                    {
                        id_articulo = g.Key,
                        cantidad_total = g.Sum(d => d.cantidad ?? 0)
                    })
                    .ToList();

                var ahora = DateTime.Now;

                var idsArticulos = detallesAgrupados
                    .Where(d => d.id_articulo.HasValue)
                    .Select(d => d.id_articulo.Value)
                    .ToList();

                var inventariosOrigen = await _context.inventario
                    .Where(i => i.id_almacen == traspaso.almacen_origen && idsArticulos.Contains(i.id_articulo))
                    .ToDictionaryAsync(i => i.id_articulo);

                // Validar inventario en origen (por si cambió desde que se creó)
                foreach (var detalle in detallesAgrupados)
                {
                    if (!detalle.id_articulo.HasValue || !inventariosOrigen.TryGetValue(detalle.id_articulo.Value, out var inventarioOrigen))
                    {
                        throw new InvalidOperationException(
                            $"El artículo {detalle.id_articulo} ya no existe en el inventario del almacén origen");
                    }

                    if (inventarioOrigen.cantidad < detalle.cantidad_total)
                    {
                        throw new InvalidOperationException(
                            $"Inventario insuficiente en almacén origen para artículo {detalle.id_articulo}. " +
                            $"Disponible: {inventarioOrigen.cantidad}, Requerido: {detalle.cantidad_total}");
                    }
                }

                var inventariosDestino = await _context.inventario
                    .Where(i => i.id_almacen == traspaso.almacen_destino && idsArticulos.Contains(i.id_articulo))
                    .ToDictionaryAsync(i => i.id_articulo);

                // Mover inventario
                foreach (var detalle in detallesAgrupados)
                {
                    var idArticulo = detalle.id_articulo.Value;

                    // Restar de origen
                    var inventarioOrigen = inventariosOrigen[idArticulo];
                    inventarioOrigen.cantidad -= detalle.cantidad_total;
                    inventarioOrigen.fecha_ultima_salida = ahora;
                    inventarioOrigen.fecha_actualizacion = ahora;
                    _context.inventario.Update(inventarioOrigen);

                    // Registrar movimiento de salida
                    RegistrarMovimiento(
                        traspaso.almacen_origen.Value, 
                        idArticulo, 
                        -detalle.cantidad_total, 
                        "Traspaso-Salida", 
                        ahora, 
                        id_usuario_recibe, 
                        id_traspaso: id_traspaso
                    );

                    // Sumar a destino
                    if (inventariosDestino.TryGetValue(idArticulo, out var inventarioDestino))
                    {
                        inventarioDestino.cantidad += detalle.cantidad_total;
                        inventarioDestino.fecha_ultima_entrada = ahora;
                        inventarioDestino.fecha_actualizacion = ahora;
                        _context.inventario.Update(inventarioDestino);
                    }
                    else
                    {
                        var nuevoInventario = new Inventario
                        {
                            id_almacen = traspaso.almacen_destino.Value,
                            id_articulo = idArticulo,
                            cantidad = detalle.cantidad_total,
                            fecha_ultima_entrada = ahora,
                            fecha_actualizacion = ahora
                        };
                        _context.inventario.Add(nuevoInventario);
                        inventariosDestino[idArticulo] = nuevoInventario;
                    }

                    // Registrar movimiento de entrada
                    RegistrarMovimiento(
                        traspaso.almacen_destino.Value, 
                        idArticulo, 
                        detalle.cantidad_total, 
                        "Traspaso-Entrada", 
                        ahora, 
                        id_usuario_recibe, 
                        id_traspaso: id_traspaso
                    );
                }

                // Actualizar traspaso
                traspaso.estatus = "Aprobado";
                traspaso.fecha_recibido = ahora;
                traspaso.id_usuario_recibe = id_usuario_recibe;
                _context.Traspasos.Update(traspaso);

                // ✅ NUEVO: Agregar archivos de recepción si se proporcionan
                if (archivos != null && archivos.Any())
                {
                    foreach (var archivo in archivos)
                    {
                        archivo.id_traspaso = id_traspaso;
                        archivo.activo = true;
                    }
                    _context.traspasos_archivos.AddRange(archivos);
                }

                // Bitácora
                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario_recibe,
                    modulo = "Traspaso",
                    submodulo = "Recepción",
                    accion = "Recibir",
                    descripcion = $"Se recibió traspaso {traspaso.folio}"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // ✅ SEGURIDAD: Log éxito
                _logger.LogInformation(
                    "Traspaso recibido exitosamente. Folio: {Folio}, Usuario: {UserId}, Archivos recepción: {CantArchivos}",
                    traspaso.folio,
                    id_usuario_recibe,
                    archivos?.Count ?? 0
                );

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    dbEx,
                    "Error BD al recibir traspaso. TraspasoId: {TraspasoId}",
                    id_traspaso
                );

                var errorMessage = "Error al recibir traspaso en la base de datos";
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
                    errorMessage = "Referencia inválida en recepción de traspaso";
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
                    "Error al recibir traspaso. TraspasoId: {TraspasoId}",
                    id_traspaso
                );

                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error inesperado al recibir traspaso. TraspasoId: {TraspasoId}",
                    id_traspaso
                );

                throw new InvalidOperationException($"Error inesperado al recibir traspaso: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cancela un traspaso
        /// Si está Pendiente: solo cambia estatus
        /// Si está Recibido: revierte el inventario y cambia estatus
        /// </summary>
        public async Task<bool> CancelarAsync(int id_traspaso, int id_usuario, string motivo_cancelacion)
        {
            if (id_traspaso <= 0)
                throw new ArgumentException("ID de traspaso inválido", nameof(id_traspaso));

            // ✅ FIX W2: Validar motivo de cancelación
            if (string.IsNullOrWhiteSpace(motivo_cancelacion))
                throw new ArgumentException("El motivo de cancelación es requerido", nameof(motivo_cancelacion));

            // ✅ SEGURIDAD: Log inicio
            _logger.LogInformation(
                "Iniciando cancelación de traspaso. TraspasoId: {TraspasoId}, Usuario: {UserId}, Motivo: {Motivo}",
                id_traspaso,
                id_usuario,
                motivo_cancelacion
            );

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);  // ✅ SEGURIDAD: Timeout 30s

            try
            {
                var traspaso = await _context.Traspasos.FindAsync(id_traspaso);

                if (traspaso == null)
                    throw new InvalidOperationException("Traspaso no encontrado");

                if (traspaso.estatus == "Cancelado")
                    throw new InvalidOperationException("El traspaso ya está cancelado");

                var ahora = DateTime.Now;

                // Si está Recibido o Aprobado, revertir inventario
                if (traspaso.estatus == "Recibido" || traspaso.estatus == "Aprobado")
                {
                    var detalles = await _context.traspasos_detalle
                        .Where(td => td.id_traspaso == id_traspaso)
                        .AsNoTracking()
                        .ToListAsync();

                    var detallesAgrupados = detalles
                        .GroupBy(d => d.id_articulo)
                        .Select(g => new
                        {
                            id_articulo = g.Key,
                            cantidad_total = g.Sum(d => d.cantidad ?? 0)
                        })
                        .ToList();

                    var idsArticulos = detallesAgrupados
                        .Where(d => d.id_articulo.HasValue)
                        .Select(d => d.id_articulo.Value)
                        .ToList();

                    var inventariosOrigen = await _context.inventario
                        .Where(i => i.id_almacen == traspaso.almacen_origen && idsArticulos.Contains(i.id_articulo))
                        .ToDictionaryAsync(i => i.id_articulo);

                    var inventariosDestino = await _context.inventario
                        .Where(i => i.id_almacen == traspaso.almacen_destino && idsArticulos.Contains(i.id_articulo))
                        .ToDictionaryAsync(i => i.id_articulo);

                    foreach (var detalle in detallesAgrupados)
                    {
                        if (!detalle.id_articulo.HasValue) continue;
                        var idArticulo = detalle.id_articulo.Value;

                        // Sumar de vuelta a origen
                        if (inventariosOrigen.TryGetValue(idArticulo, out var inventarioOrigen))
                        {
                            inventarioOrigen.cantidad += detalle.cantidad_total;
                            inventarioOrigen.fecha_actualizacion = ahora;
                            _context.inventario.Update(inventarioOrigen);

                            // Registrar reversión como entrada
                            RegistrarMovimiento(
                                traspaso.almacen_origen.Value, 
                                idArticulo, 
                                detalle.cantidad_total, 
                                "Traspaso-Cancelado-Entrada", 
                                ahora, 
                                id_usuario, 
                                id_traspaso: id_traspaso
                            );
                        }

                        // Restar de destino
                        if (inventariosDestino.TryGetValue(idArticulo, out var inventarioDestino))
                        {
                            inventarioDestino.cantidad -= detalle.cantidad_total;
                            inventarioDestino.fecha_actualizacion = ahora;
                            _context.inventario.Update(inventarioDestino);

                            // Registrar reversión como salida
                            RegistrarMovimiento(
                                traspaso.almacen_destino.Value, 
                                idArticulo, 
                                -detalle.cantidad_total, 
                                "Traspaso-Cancelado-Salida", 
                                ahora, 
                                id_usuario, 
                                id_traspaso: id_traspaso
                            );
                        }
                    }
                }

                // Actualizar traspaso
                traspaso.estatus = "Cancelado";
                traspaso.motivo = string.IsNullOrEmpty(traspaso.motivo) 
                    ? $"CANCELADO: {motivo_cancelacion}" 
                    : $"{traspaso.motivo} | CANCELADO: {motivo_cancelacion}";
                _context.Traspasos.Update(traspaso);

                // Bitácora
                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Traspaso",
                    submodulo = "Cancelación",
                    accion = "Cancelar",
                    descripcion = $"Se canceló traspaso {traspaso.folio}. Motivo: {motivo_cancelacion}"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // ✅ SEGURIDAD: Log éxito
                _logger.LogInformation(
                    "Traspaso cancelado exitosamente. Folio: {Folio}, Usuario: {UserId}",
                    traspaso.folio,
                    id_usuario
                );

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    dbEx,
                    "Error BD al cancelar traspaso. TraspasoId: {TraspasoId}",
                    id_traspaso
                );

                var errorMessage = "Error al cancelar traspaso en la base de datos";
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
                    "Error al cancelar traspaso. TraspasoId: {TraspasoId}",
                    id_traspaso
                );

                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error inesperado al cancelar traspaso. TraspasoId: {TraspasoId}",
                    id_traspaso
                );

                throw new InvalidOperationException($"Error inesperado al cancelar traspaso: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Elimina (soft delete) un traspaso
        /// Solo permite eliminar si está en estado Pendiente
        /// </summary>
        public async Task<bool> DeleteAsync(int id_traspaso, int id_usuario)
        {
            if (id_traspaso <= 0)
                throw new ArgumentException("ID de traspaso inválido", nameof(id_traspaso));

            // ✅ SEGURIDAD: Log inicio
            _logger.LogInformation(
                "Iniciando eliminación de traspaso. TraspasoId: {TraspasoId}, Usuario: {UserId}",
                id_traspaso,
                id_usuario
            );

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);  // ✅ SEGURIDAD: Timeout 30s

            try
            {
                var traspaso = await _context.Traspasos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.id_traspaso == id_traspaso);

                if (traspaso == null)
                    throw new InvalidOperationException("Traspaso no encontrado");

                if (traspaso.estatus == "Recibido" || traspaso.estatus == "Aprobado")
                    throw new InvalidOperationException("No se puede eliminar un traspaso que ya fue recibido. Use cancelar en su lugar.");

                // Soft delete: marcar como cancelado
                var traspasoTracked = await _context.Traspasos.FindAsync(id_traspaso);
                traspasoTracked.estatus = "Cancelado";
                traspasoTracked.motivo = string.IsNullOrEmpty(traspasoTracked.motivo)
                    ? "ELIMINADO"
                    : $"{traspasoTracked.motivo} | ELIMINADO";
                _context.Traspasos.Update(traspasoTracked);

                // Bitácora
                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Traspaso",
                    submodulo = "Eliminación",
                    accion = "Eliminar",
                    descripcion = $"Se eliminó traspaso {traspaso.folio}"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // ✅ SEGURIDAD: Log éxito
                _logger.LogInformation(
                    "Traspaso eliminado exitosamente. Folio: {Folio}, Usuario: {UserId}",
                    traspaso.folio,
                    id_usuario
                );

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    dbEx,
                    "Error BD al eliminar traspaso. TraspasoId: {TraspasoId}",
                    id_traspaso
                );

                var errorMessage = "Error al eliminar traspaso en la base de datos";
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                errorMessage = $"Error de base de datos: {innerMessage}";

                throw new InvalidOperationException(errorMessage, dbEx);
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();

                _logger.LogWarning(
                    ex,
                    "Error al eliminar traspaso. TraspasoId: {TraspasoId}",
                    id_traspaso
                );

                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error inesperado al eliminar traspaso. TraspasoId: {TraspasoId}",
                    id_traspaso
                );

                throw new InvalidOperationException($"Error inesperado al eliminar traspaso: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el listado de todos los traspasos con información de almacenes
        /// </summary>
        public async Task<List<TraspasoListadoResponse>> GetAllAsync()
        {
            return await _context.Traspasos
                .Join(
                    _context.Almacenes,
                    t => t.almacen_origen,
                    ao => ao.id_almacen,
                    (t, ao) => new { traspaso = t, almacenOrigen = ao }
                )
                .Join(
                    _context.Almacenes,
                    combined => combined.traspaso.almacen_destino,
                    ad => ad.id_almacen,
                    (combined, ad) => new { combined.traspaso, combined.almacenOrigen, almacenDestino = ad }
                )
                .Select(x => new TraspasoListadoResponse
                {
                    IdTraspaso = x.traspaso.id_traspaso,
                    Folio = x.traspaso.folio,
                    FechaTraspaso = x.traspaso.fecha_traspaso,
                    AlmacenOrigenNombre = x.almacenOrigen.nombre,
                    AlmacenDestinoNombre = x.almacenDestino.nombre,
                    Estatus = x.traspaso.estatus,
                    FechaRecibido = x.traspaso.fecha_recibido,
                    CantidadArticulos = _context.traspasos_detalle
                        .Where(td => td.id_traspaso == x.traspaso.id_traspaso)
                        .Count()
                })
                .OrderByDescending(x => x.Folio)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene el listado de traspasos filtrados por usuario y almacén
        /// Muestra: traspasos que el usuario envía O traspasos que llegan al almacén especificado
        /// </summary>
        public async Task<List<TraspasoListadoResponse>> GetByIdUsuarioAsync(int id_usuario, int id_almacen)
        {
            if (id_usuario <= 0)
                throw new ArgumentException("ID de usuario inválido", nameof(id_usuario));

            if (id_almacen <= 0)
                throw new ArgumentException("ID de almacén inválido", nameof(id_almacen));

            return await _context.Traspasos
                .Where(t => t.id_usuario_envia == id_usuario || t.almacen_destino == id_almacen)
                .Join(
                    _context.Almacenes,
                    t => t.almacen_origen,
                    ao => ao.id_almacen,
                    (t, ao) => new { traspaso = t, almacenOrigen = ao }
                )
                .Join(
                    _context.Almacenes,
                    combined => combined.traspaso.almacen_destino,
                    ad => ad.id_almacen,
                    (combined, ad) => new { combined.traspaso, combined.almacenOrigen, almacenDestino = ad }
                )
                .Select(x => new TraspasoListadoResponse
                {
                    IdTraspaso = x.traspaso.id_traspaso,
                    Folio = x.traspaso.folio,
                    FechaTraspaso = x.traspaso.fecha_traspaso,
                    AlmacenOrigenNombre = x.almacenOrigen.nombre,
                    AlmacenDestinoNombre = x.almacenDestino.nombre,
                    Estatus = x.traspaso.estatus,
                    FechaRecibido = x.traspaso.fecha_recibido,
                    CantidadArticulos = _context.traspasos_detalle
                        .Where(td => td.id_traspaso == x.traspaso.id_traspaso)
                        .Count()
                })
                .OrderByDescending(x => x.Folio)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene un traspaso completo con todos sus detalles
        /// </summary>
        public async Task<TraspasoCompleteResponse> GetByIdAsync(int id_traspaso)
        {
            if (id_traspaso <= 0)
                throw new ArgumentException("ID de traspaso inválido", nameof(id_traspaso));

            var traspaso = await _context.Traspasos
                .Where(t => t.id_traspaso == id_traspaso)
                .Join(
                    _context.Almacenes,
                    t => t.almacen_origen,
                    ao => ao.id_almacen,
                    (t, ao) => new { traspaso = t, almacenOrigen = ao }
                )
                .Join(
                    _context.Almacenes,
                    combined => combined.traspaso.almacen_destino,
                    ad => ad.id_almacen,
                    (combined, ad) => new
                    {
                        combined.traspaso,
                        combined.almacenOrigen,
                        almacenDestino = ad
                    }
                )
                .Select(x => new
                {
                    x.traspaso.id_traspaso,
                    x.traspaso.folio,
                    x.traspaso.fecha_traspaso,
                    x.traspaso.fecha_registro,
                    x.traspaso.fecha_recibido,
                    x.traspaso.id_usuario_envia,
                    x.traspaso.id_usuario_recibe,
                    almacen_origen = x.traspaso.almacen_origen.Value,
                    almacen_origen_nombre = x.almacenOrigen.nombre,
                    almacen_destino = x.traspaso.almacen_destino.Value,
                    almacen_destino_nombre = x.almacenDestino.nombre,
                    x.traspaso.motivo,
                    x.traspaso.estatus
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (traspaso == null)
                return null;

            var detalles = await _context.traspasos_detalle
                .Where(td => td.id_traspaso == id_traspaso)
                .Join(
                    _context.Articulos,
                    td => td.id_articulo,
                    art => art.id_articulo,
                    (td, art) => new { detalle = td, articulo = art }
                )
                .Join(
                    _context.Categorias,
                    combined => combined.articulo.id_categoria,
                    cat => cat.id_categoria,
                    (combined, cat) => new TraspasoDetalleResponse
                    {
                        IdDetalle = combined.detalle.id_detalle,
                        IdArticulo = combined.detalle.id_articulo.Value,
                        NombreArticulo = combined.articulo.nombre,
                        Categoria = combined.articulo.id_categoria,
                        CategoriaNombre = cat.nombre,
                        Cantidad = combined.detalle.cantidad.Value
                    }
                )
                .AsNoTracking()
                .ToListAsync();

            // ✅ NUEVO: Consultar archivos del traspaso
            var archivos = await _context.traspasos_archivos
                .Where(ta => ta.id_traspaso == id_traspaso && ta.activo == true)
                .Select(ta => new TraspasoArchivoResponse
                {
                    IdArchivo = ta.id_archivo,
                    NombreArchivo = ta.nombre_original,
                    UrlArchivo = ta.url,
                    TipoArchivo = ta.tipo_archivo
                })
                .AsNoTracking()
                .ToListAsync();

            return new TraspasoCompleteResponse
            {
                IdTraspaso = traspaso.id_traspaso,
                Folio = traspaso.folio,
                FechaTraspaso = traspaso.fecha_traspaso,
                FechaRegistro = traspaso.fecha_registro,
                FechaRecibido = traspaso.fecha_recibido,
                IdUsuarioEnvia = traspaso.id_usuario_envia,
                IdUsuarioRecibe = traspaso.id_usuario_recibe,
                AlmacenOrigen = traspaso.almacen_origen,
                AlmacenOrigenNombre = traspaso.almacen_origen_nombre,
                AlmacenDestino = traspaso.almacen_destino,
                AlmacenDestinoNombre = traspaso.almacen_destino_nombre,
                Motivo = traspaso.motivo,
                Estatus = traspaso.estatus,
                Detalles = detalles,
                Archivos = archivos  // ✅ NUEVO
            };
        }
    }
}
