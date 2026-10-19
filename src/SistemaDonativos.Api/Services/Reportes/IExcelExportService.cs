using System.Data;
using System.Threading.Tasks;

namespace apiDonativos.Services.Reportes
{
    /// <summary>
    /// Interface para generación de archivos Excel con EPPlus
    /// ✅ CONVENCIÓN: Métodos retornan byte[] (archivo Excel en memoria)
    /// </summary>
    public interface IExcelExportService
    {
        /// <summary>
        /// Genera archivo Excel para Reporte de Entradas
        /// Hoja 1: Resumen | Hoja 2: Detalle
        /// </summary>
        /// <param name="dataSet">DataSet con 2 tablas (Resumen + Detalle)</param>
        /// <param name="nombreArchivo">Nombre base del archivo sin extensión</param>
        Task<byte[]> GenerarExcelEntradasAsync(DataSet dataSet, string nombreArchivo);

        /// <summary>
        /// Genera archivo Excel para Reporte de Salidas
        /// Hoja 1: Resumen | Hoja 2: Detalle
        /// </summary>
        /// <param name="dataSet">DataSet con 2 tablas (Resumen + Detalle)</param>
        /// <param name="nombreArchivo">Nombre base del archivo sin extensión</param>
        Task<byte[]> GenerarExcelSalidasAsync(DataSet dataSet, string nombreArchivo);

        /// <summary>
        /// Genera archivo Excel para Reporte de Traspasos
        /// Hoja 1: Resumen | Hoja 2: Detalle
        /// </summary>
        /// <param name="dataSet">DataSet con 2 tablas (Resumen + Detalle)</param>
        /// <param name="nombreArchivo">Nombre base del archivo sin extensión</param>
        Task<byte[]> GenerarExcelTraspasosAsync(DataSet dataSet, string nombreArchivo);

        /// <summary>
        /// Genera archivo Excel para Reporte de Existencias
        /// Hoja 1: Resumen | Hoja 2: Detalle
        /// </summary>
        /// <param name="dataSet">DataSet con 2 tablas (Resumen + Detalle)</param>
        /// <param name="nombreArchivo">Nombre base del archivo sin extensión</param>
        Task<byte[]> GenerarExcelExistenciasAsync(DataSet dataSet, string nombreArchivo);

        /// <summary>
        /// Genera archivo Excel para Kardex de Inventario
        /// ⚠️ ORDEN INVERTIDO: Hoja 1 = Movimientos | Hoja 2 = Resumen
        /// </summary>
        /// <param name="dataSet">DataSet con 2 tablas (Movimientos + Resumen)</param>
        /// <param name="nombreArchivo">Nombre base del archivo sin extensión</param>
        Task<byte[]> GenerarExcelKardexAsync(DataSet dataSet, string nombreArchivo);
    }
}
