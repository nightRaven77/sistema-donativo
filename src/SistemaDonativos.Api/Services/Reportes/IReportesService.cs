using apiDonativos.Models.Dtos.Reportes;
using System.Data;
using System.Threading.Tasks;

namespace apiDonativos.Services.Reportes
{
    /// <summary>
    /// Interface para la ejecución de Stored Procedures de reportes
    /// ✅ CONVENCIÓN: Métodos retornan DataSet con múltiples ResultSets
    /// </summary>
    public interface IReportesService
    {
        /// <summary>
        /// Ejecuta sp_Reporte_Entradas
        /// ResultSet 1: Resumen (7 columnas)
        /// ResultSet 2: Detalle (22 columnas)
        /// </summary>
        Task<DataSet> ObtenerReporteEntradasAsync(ReporteEntradasDto parametros);

        /// <summary>
        /// Ejecuta sp_Reporte_Salidas
        /// ResultSet 1: Resumen (7 columnas)
        /// ResultSet 2: Detalle (22 columnas)
        /// </summary>
        Task<DataSet> ObtenerReporteSalidasAsync(ReporteSalidasDto parametros);

        /// <summary>
        /// Ejecuta sp_Reporte_Traspasos
        /// ResultSet 1: Resumen (7 columnas)
        /// ResultSet 2: Detalle (19 columnas)
        /// </summary>
        Task<DataSet> ObtenerReporteTraspasosAsync(ReporteTraspasosDto parametros);

        /// <summary>
        /// Ejecuta sp_Reporte_Existencias
        /// ResultSet 1: Resumen (6 columnas)
        /// ResultSet 2: Detalle (15 columnas)
        /// </summary>
        Task<DataSet> ObtenerReporteExistenciasAsync(ReporteExistenciasDto parametros);

        /// <summary>
        /// Ejecuta sp_Kardex_Inventario
        /// ⚠️ ORDEN INVERTIDO: ResultSet 1 = Movimientos (7 columnas), ResultSet 2 = Resumen (4 columnas)
        /// </summary>
        Task<DataSet> ObtenerKardexInventarioAsync(ReporteKardexDto parametros);
    }
}
