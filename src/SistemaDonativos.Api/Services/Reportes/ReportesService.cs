using apiDonativos.Models;
using apiDonativos.Models.Dtos.Reportes;
using apiDonativos.Services.Base;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Threading.Tasks;

namespace apiDonativos.Services.Reportes
{
    /// <summary>
    /// Servicio para ejecución de Stored Procedures de reportes
    /// ✅ CONVENCIÓN: Hereda de InventarioServiceBase para logging y configuración
    /// ✅ ARQUITECTURA: Usa ADO.NET (SqlConnection + SqlDataAdapter) para múltiples ResultSets
    /// </summary>
    public class ReportesService : InventarioServiceBase<ReportesService>, IReportesService
    {
        private readonly string _connectionString;

        public ReportesService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<ReportesService> logger)
            : base(context, configuration, logger)
        {
            _connectionString = configuration.GetConnectionString("donativosCn")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' no configurada");
        }

        /// <summary>
        /// Ejecuta sp_Reporte_Entradas y retorna DataSet con 2 tablas:
        /// [0] Resumen (7 columnas)
        /// [1] Detalle (22 columnas)
        /// </summary>
        public async Task<DataSet> ObtenerReporteEntradasAsync(ReporteEntradasDto parametros)
        {
            if (parametros == null)
                throw new ArgumentNullException(nameof(parametros), "Los parámetros del reporte son requeridos");

            _logger.LogInformation(
                "Ejecutando sp_Reporte_Entradas. Fechas: {FechaInicio} - {FechaFin}, Almacén: {AlmacenId}",
                parametros.fecha_inicio,
                parametros.fecha_fin,
                parametros.id_almacen
            );

            try
            {
                return await EjecutarStoredProcedureAsync(
                    "sp_Reporte_Entradas",
                    new SqlParameter[]
                    {
                        new SqlParameter("@fecha_inicio", (object)parametros.fecha_inicio ?? DBNull.Value),
                        new SqlParameter("@fecha_fin", (object)parametros.fecha_fin ?? DBNull.Value),
                        new SqlParameter("@id_almacen", (object)parametros.id_almacen ?? DBNull.Value),
                        new SqlParameter("@id_donador", (object)parametros.id_donador ?? DBNull.Value),
                        new SqlParameter("@id_articulo", (object)parametros.id_articulo ?? DBNull.Value),
                        new SqlParameter("@id_categoria", (object)parametros.id_categoria ?? DBNull.Value),
                        new SqlParameter("@tipo", (object)parametros.tipo ?? DBNull.Value),
                        new SqlParameter("@estatus", (object)parametros.estatus ?? DBNull.Value),
                        new SqlParameter("@agrupar_por", (object)parametros.agrupar_por ?? DBNull.Value),
                        new SqlParameter("@pagina", parametros.pagina),
                        new SqlParameter("@registros_por_pagina", parametros.registros_por_pagina),
                        new SqlParameter("@ordenar_por", parametros.ordenar_por ?? "fecha_recepcion"),
                        new SqlParameter("@orden", parametros.orden ?? "DESC")
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al ejecutar sp_Reporte_Entradas. Parámetros: {@Parametros}",
                    new
                    {
                        parametros.fecha_inicio,
                        parametros.fecha_fin,
                        parametros.id_almacen,
                        parametros.id_donador
                    }
                );
                throw;
            }
        }

        /// <summary>
        /// Ejecuta sp_Reporte_Salidas y retorna DataSet con 2 tablas:
        /// [0] Resumen (7 columnas)
        /// [1] Detalle (22 columnas)
        /// </summary>
        public async Task<DataSet> ObtenerReporteSalidasAsync(ReporteSalidasDto parametros)
        {
            if (parametros == null)
                throw new ArgumentNullException(nameof(parametros), "Los parámetros del reporte son requeridos");

            _logger.LogInformation(
                "Ejecutando sp_Reporte_Salidas. Fechas: {FechaInicio} - {FechaFin}, Almacén: {AlmacenId}",
                parametros.fecha_inicio,
                parametros.fecha_fin,
                parametros.id_almacen
            );

            try
            {
                return await EjecutarStoredProcedureAsync(
                    "sp_Reporte_Salidas",
                    new SqlParameter[]
                    {
                        new SqlParameter("@fecha_inicio", (object)parametros.fecha_inicio ?? DBNull.Value),
                        new SqlParameter("@fecha_fin", (object)parametros.fecha_fin ?? DBNull.Value),
                        new SqlParameter("@id_almacen", (object)parametros.id_almacen ?? DBNull.Value),
                        new SqlParameter("@id_beneficiario", (object)parametros.id_beneficiario ?? DBNull.Value),
                        new SqlParameter("@id_articulo", (object)parametros.id_articulo ?? DBNull.Value),
                        new SqlParameter("@id_categoria", (object)parametros.id_categoria ?? DBNull.Value),
                        new SqlParameter("@tipo_salida", (object)parametros.tipo_salida ?? DBNull.Value),
                        new SqlParameter("@estatus", (object)parametros.estatus ?? DBNull.Value),
                        new SqlParameter("@agrupar_por", (object)parametros.agrupar_por ?? DBNull.Value),
                        new SqlParameter("@pagina", parametros.pagina),
                        new SqlParameter("@registros_por_pagina", parametros.registros_por_pagina)
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al ejecutar sp_Reporte_Salidas. Parámetros: {@Parametros}",
                    new
                    {
                        parametros.fecha_inicio,
                        parametros.fecha_fin,
                        parametros.id_almacen,
                        parametros.id_beneficiario
                    }
                );
                throw;
            }
        }

        /// <summary>
        /// Ejecuta sp_Reporte_Traspasos y retorna DataSet con 2 tablas:
        /// [0] Resumen (7 columnas)
        /// [1] Detalle (19 columnas)
        /// </summary>
        public async Task<DataSet> ObtenerReporteTraspasosAsync(ReporteTraspasosDto parametros)
        {
            if (parametros == null)
                throw new ArgumentNullException(nameof(parametros), "Los parámetros del reporte son requeridos");

            _logger.LogInformation(
                "Ejecutando sp_Reporte_Traspasos. Fechas: {FechaInicio} - {FechaFin}, Origen: {OrigenId}, Destino: {DestinoId}",
                parametros.fecha_inicio,
                parametros.fecha_fin,
                parametros.id_almacen_origen,
                parametros.id_almacen_destino
            );

            try
            {
                return await EjecutarStoredProcedureAsync(
                    "sp_Reporte_Traspasos",
                    new SqlParameter[]
                    {
                        new SqlParameter("@fecha_inicio", (object)parametros.fecha_inicio ?? DBNull.Value),
                        new SqlParameter("@fecha_fin", (object)parametros.fecha_fin ?? DBNull.Value),
                        new SqlParameter("@id_almacen_origen", (object)parametros.id_almacen_origen ?? DBNull.Value),
                        new SqlParameter("@id_almacen_destino", (object)parametros.id_almacen_destino ?? DBNull.Value),
                        new SqlParameter("@id_articulo", (object)parametros.id_articulo ?? DBNull.Value),
                        new SqlParameter("@id_categoria", (object)parametros.id_categoria ?? DBNull.Value),
                        new SqlParameter("@estatus", (object)parametros.estatus ?? DBNull.Value),
                        new SqlParameter("@agrupar_por", (object)parametros.agrupar_por ?? DBNull.Value),
                        new SqlParameter("@pagina", parametros.pagina),
                        new SqlParameter("@registros_por_pagina", parametros.registros_por_pagina)
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al ejecutar sp_Reporte_Traspasos. Parámetros: {@Parametros}",
                    new
                    {
                        parametros.fecha_inicio,
                        parametros.fecha_fin,
                        parametros.id_almacen_origen,
                        parametros.id_almacen_destino
                    }
                );
                throw;
            }
        }

        /// <summary>
        /// Ejecuta sp_Reporte_Existencias y retorna DataSet con 2 tablas:
        /// [0] Resumen (6 columnas)
        /// [1] Detalle (15 columnas)
        /// </summary>
        public async Task<DataSet> ObtenerReporteExistenciasAsync(ReporteExistenciasDto parametros)
        {
            if (parametros == null)
                throw new ArgumentNullException(nameof(parametros), "Los parámetros del reporte son requeridos");

            _logger.LogInformation(
                "Ejecutando sp_Reporte_Existencias. Almacén: {AlmacenId}, FechaCorte: {FechaCorte}, SinExistencia: {SinExistencia}, StockBajo: {StockBajo}",
                parametros.id_almacen,
                parametros.fecha_corte,
                parametros.solo_sin_existencia,
                parametros.solo_stock_bajo
            );

            try
            {
                return await EjecutarStoredProcedureAsync(
                    "sp_Reporte_Existencias",
                    new SqlParameter[]
                    {
                        new SqlParameter("@id_almacen", (object)parametros.id_almacen ?? DBNull.Value),
                        new SqlParameter("@id_articulo", (object)parametros.id_articulo ?? DBNull.Value),
                        new SqlParameter("@id_categoria", (object)parametros.id_categoria ?? DBNull.Value),
                        new SqlParameter("@cantidad_minima", (object)parametros.cantidad_minima ?? DBNull.Value),
                        new SqlParameter("@cantidad_maxima", (object)parametros.cantidad_maxima ?? DBNull.Value),
                        new SqlParameter("@solo_sin_existencia", parametros.solo_sin_existencia),
                        new SqlParameter("@agrupar_por", (object)parametros.agrupar_por ?? DBNull.Value),
                        new SqlParameter("@ordenar_por", parametros.ordenar_por ?? "nombre_articulo"),
                        new SqlParameter("@orden", parametros.orden ?? "ASC"),
                        new SqlParameter("@fecha_corte", (object)parametros.fecha_corte ?? DBNull.Value),
                        new SqlParameter("@pagina", parametros.pagina),
                        new SqlParameter("@registros_por_pagina", parametros.registros_por_pagina)
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al ejecutar sp_Reporte_Existencias. Parámetros: {@Parametros}",
                    new
                    {
                        parametros.id_almacen,
                        parametros.fecha_corte,
                        parametros.solo_sin_existencia
                    }
                );
                throw;
            }
        }

        /// <summary>
        /// Ejecuta sp_Kardex_Inventario y retorna DataSet con 2 tablas:
        /// ⚠️ ORDEN INVERTIDO: [0] Movimientos (7 columnas), [1] Resumen (4 columnas)
        /// </summary>
        public async Task<DataSet> ObtenerKardexInventarioAsync(ReporteKardexDto parametros)
        {
            if (parametros == null)
                throw new ArgumentNullException(nameof(parametros), "Los parámetros del reporte son requeridos");

            if (parametros.id_almacen <= 0)
                throw new ArgumentException("El almacén es requerido", nameof(parametros.id_almacen));

            if (parametros.id_articulo <= 0)
                throw new ArgumentException("El artículo es requerido", nameof(parametros.id_articulo));

            _logger.LogInformation(
                "Ejecutando sp_Kardex_Inventario. Almacén: {AlmacenId}, Artículo: {ArticuloId}, Fechas: {FechaInicio} - {FechaFin}",
                parametros.id_almacen,
                parametros.id_articulo,
                parametros.fecha_inicio,
                parametros.fecha_fin
            );

            try
            {
                return await EjecutarStoredProcedureAsync(
                    "sp_Kardex_Inventario",
                    new SqlParameter[]
                    {
                        new SqlParameter("@id_almacen", parametros.id_almacen),
                        new SqlParameter("@id_articulo", parametros.id_articulo),
                        new SqlParameter("@fecha_inicio", (object)parametros.fecha_inicio ?? DBNull.Value),
                        new SqlParameter("@fecha_fin", (object)parametros.fecha_fin ?? DBNull.Value)
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al ejecutar sp_Kardex_Inventario. Almacén: {AlmacenId}, Artículo: {ArticuloId}",
                    parametros.id_almacen,
                    parametros.id_articulo
                );
                throw;
            }
        }

        /// <summary>
        /// Método genérico para ejecutar Stored Procedures con múltiples ResultSets
        /// ✅ ARQUITECTURA: Usa ADO.NET puro (SqlDataAdapter) para capturar todos los SELECT
        /// </summary>
        /// <param name="spName">Nombre del Stored Procedure</param>
        /// <param name="parameters">Array de SqlParameter</param>
        /// <returns>DataSet con múltiples DataTables (uno por cada SELECT en el SP)</returns>
        private async Task<DataSet> EjecutarStoredProcedureAsync(string spName, SqlParameter[] parameters)
        {
            var dataSet = new DataSet();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(spName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 120; // ✅ SEGURIDAD: Timeout 120s (consultas complejas)

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        await Task.Run(() => adapter.Fill(dataSet));
                    }
                }

                _logger.LogInformation(
                    "SP ejecutado exitosamente: {SpName}. ResultSets: {Cantidad}",
                    spName,
                    dataSet.Tables.Count
                );

                return dataSet;
            }
            catch (SqlException sqlEx)
            {
                var referenceId = Guid.NewGuid().ToString();
                _logger.LogError(
                    sqlEx,
                    "Error SQL al ejecutar {SpName}. ReferenceId: {ReferenceId}, ErrorNumber: {ErrorNumber}",
                    spName,
                    referenceId,
                    sqlEx.Number
                );

                var errorMessage = "Error al ejecutar consulta en la base de datos";

                switch (sqlEx.Number)
                {
                    case -1: // Timeout
                        errorMessage = "La consulta excedió el tiempo de espera. Intente con filtros más específicos.";
                        break;
                    case 2812: // SP no encontrado
                        errorMessage = $"El procedimiento almacenado '{spName}' no existe en la base de datos";
                        break;
                    case 201: // Parámetro faltante
                    case 8144: // Parámetro extra
                        errorMessage = "Error en los parámetros del reporte";
                        break;
                    case 515: // NULL no permitido
                        errorMessage = "Uno o más parámetros requeridos no fueron proporcionados";
                        break;
                    default:
                        errorMessage = $"Error de base de datos: {sqlEx.Message}";
                        break;
                }

                throw new InvalidOperationException(
                    $"{errorMessage} (Ref: {referenceId})",
                    sqlEx
                );
            }
            catch (Exception ex)
            {
                var referenceId = Guid.NewGuid().ToString();
                _logger.LogError(
                    ex,
                    "Error inesperado al ejecutar {SpName}. ReferenceId: {ReferenceId}",
                    spName,
                    referenceId
                );

                throw new InvalidOperationException(
                    $"Error inesperado al generar reporte (Ref: {referenceId})",
                    ex
                );
            }
        }
    }
}
