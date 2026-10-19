using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace apiDonativos.Services.Reportes
{
    /// <summary>
    /// Servicio para generación de archivos Excel con EPPlus 7.5.2
    /// ✅ CONVENCIÓN: Formato profesional con headers, bordes y estilos consistentes
    /// </summary>
    public class ExcelExportService : IExcelExportService
    {
        private readonly ILogger<ExcelExportService> _logger;

        public ExcelExportService(ILogger<ExcelExportService> logger)
        {
            _logger = logger;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Genera Excel para Reporte de Entradas (Donativos)
        /// Hoja 1: Resumen (7 columnas) | Hoja 2: Detalle (22 columnas)
        /// </summary>
        public async Task<byte[]> GenerarExcelEntradasAsync(DataSet dataSet, string nombreArchivo)
        {
            if (dataSet == null || dataSet.Tables.Count < 2)
                throw new ArgumentException("El DataSet debe contener al menos 2 tablas (Resumen y Detalle)", nameof(dataSet));

            _logger.LogInformation("Generando Excel para Reporte de Entradas: {NombreArchivo}", nombreArchivo);

            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();

                var tablaResumen = dataSet.Tables[0];
                var tablaDetalle = dataSet.Tables[1];

                CrearHojaResumen(package, tablaResumen, "Resumen Entradas");
                CrearHojaDetalle(package, tablaDetalle, "Detalle Entradas", new[]
                {
                    "fecha_recepcion",
                    "fecha_creacion"
                }, new[]
                {
                    "valor_unitario",
                    "valor_total"
                }, new[]
                {
                    "cantidad_recibida"
                });

                _logger.LogInformation(
                    "Excel generado. Resumen: {Filas1} filas, Detalle: {Filas2} filas",
                    tablaResumen.Rows.Count,
                    tablaDetalle.Rows.Count
                );

                return package.GetAsByteArray();
            });
        }

        /// <summary>
        /// Genera Excel para Reporte de Salidas (Entregas)
        /// Hoja 1: Resumen (7 columnas) | Hoja 2: Detalle (22 columnas)
        /// </summary>
        public async Task<byte[]> GenerarExcelSalidasAsync(DataSet dataSet, string nombreArchivo)
        {
            if (dataSet == null || dataSet.Tables.Count < 2)
                throw new ArgumentException("El DataSet debe contener al menos 2 tablas (Resumen y Detalle)", nameof(dataSet));

            _logger.LogInformation("Generando Excel para Reporte de Salidas: {NombreArchivo}", nombreArchivo);

            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();

                var tablaResumen = dataSet.Tables[0];
                var tablaDetalle = dataSet.Tables[1];

                CrearHojaResumen(package, tablaResumen, "Resumen Salidas");
                CrearHojaDetalle(package, tablaDetalle, "Detalle Salidas", new[]
                {
                    "fecha_salida",
                    "fecha_creacion"
                }, new[]
                {
                    "valor_unitario",
                    "valor_total"
                }, new[]
                {
                    "cantidad_entregada"
                });

                _logger.LogInformation(
                    "Excel generado. Resumen: {Filas1} filas, Detalle: {Filas2} filas",
                    tablaResumen.Rows.Count,
                    tablaDetalle.Rows.Count
                );

                return package.GetAsByteArray();
            });
        }

        /// <summary>
        /// Genera Excel para Reporte de Traspasos
        /// Hoja 1: Resumen (7 columnas) | Hoja 2: Detalle (19 columnas)
        /// </summary>
        public async Task<byte[]> GenerarExcelTraspasosAsync(DataSet dataSet, string nombreArchivo)
        {
            if (dataSet == null || dataSet.Tables.Count < 2)
                throw new ArgumentException("El DataSet debe contener al menos 2 tablas (Resumen y Detalle)", nameof(dataSet));

            _logger.LogInformation("Generando Excel para Reporte de Traspasos: {NombreArchivo}", nombreArchivo);

            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();

                var tablaResumen = dataSet.Tables[0];
                var tablaDetalle = dataSet.Tables[1];

                CrearHojaResumen(package, tablaResumen, "Resumen Traspasos");
                CrearHojaDetalle(package, tablaDetalle, "Detalle Traspasos", new[]
                {
                    "fecha_traspaso",
                    "fecha_creacion"
                }, new string[] { }, new[]
                {
                    "cantidad"
                });

                _logger.LogInformation(
                    "Excel generado. Resumen: {Filas1} filas, Detalle: {Filas2} filas",
                    tablaResumen.Rows.Count,
                    tablaDetalle.Rows.Count
                );

                return package.GetAsByteArray();
            });
        }

        /// <summary>
        /// Genera Excel para Reporte de Existencias
        /// Hoja 1: Resumen (6 columnas) | Hoja 2: Detalle (15 columnas)
        /// </summary>
        public async Task<byte[]> GenerarExcelExistenciasAsync(DataSet dataSet, string nombreArchivo)
        {
            if (dataSet == null || dataSet.Tables.Count < 2)
                throw new ArgumentException("El DataSet debe contener al menos 2 tablas (Resumen y Detalle)", nameof(dataSet));

            _logger.LogInformation("Generando Excel para Reporte de Existencias: {NombreArchivo}", nombreArchivo);

            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();

                var tablaResumen = dataSet.Tables[0];
                var tablaDetalle = dataSet.Tables[1];

                CrearHojaResumen(package, tablaResumen, "Resumen Existencias");
                CrearHojaDetalle(package, tablaDetalle, "Detalle Existencias", new[]
                {
                    "fecha_ultima_entrada",
                    "fecha_ultima_salida",
                    "fecha_actualizacion"
                }, new[]
                {
                    "valor_unitario_promedio",
                    "valor_total"
                }, new[]
                {
                    "cantidad",
                    "cantidad_minima",
                    "cantidad_maxima"
                });

                _logger.LogInformation(
                    "Excel generado. Resumen: {Filas1} filas, Detalle: {Filas2} filas",
                    tablaResumen.Rows.Count,
                    tablaDetalle.Rows.Count
                );

                return package.GetAsByteArray();
            });
        }

        /// <summary>
        /// Genera Excel para Kardex de Inventario
        /// ⚠️ ORDEN INVERTIDO: Hoja 1 = Movimientos (7 columnas) | Hoja 2 = Resumen (4 columnas)
        /// </summary>
        public async Task<byte[]> GenerarExcelKardexAsync(DataSet dataSet, string nombreArchivo)
        {
            if (dataSet == null || dataSet.Tables.Count < 2)
                throw new ArgumentException("El DataSet debe contener al menos 2 tablas (Movimientos y Resumen)", nameof(dataSet));

            _logger.LogInformation("Generando Excel para Kardex: {NombreArchivo}", nombreArchivo);

            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();

                var tablaMovimientos = dataSet.Tables[0];
                var tablaResumen = dataSet.Tables[1];

                CrearHojaDetalle(package, tablaMovimientos, "Movimientos", new[]
                {
                    "fecha_movimiento"
                }, new string[] { }, new[]
                {
                    "cantidad",
                    "entradas",
                    "salidas",
                    "saldo"
                });

                CrearHojaResumen(package, tablaResumen, "Resumen Kardex");

                _logger.LogInformation(
                    "Kardex Excel generado. Movimientos: {Filas1} filas, Resumen: {Filas2} filas",
                    tablaMovimientos.Rows.Count,
                    tablaResumen.Rows.Count
                );

                return package.GetAsByteArray();
            });
        }

        /// <summary>
        /// Crea hoja de Resumen con formato estándar
        /// </summary>
        private void CrearHojaResumen(ExcelPackage package, DataTable tabla, string nombreHoja)
        {
            var worksheet = package.Workbook.Worksheets.Add(nombreHoja);

            if (tabla.Rows.Count == 0)
            {
                worksheet.Cells[1, 1].Value = "No hay datos disponibles";
                return;
            }

            worksheet.Cells["A1"].LoadFromDataTable(tabla, true);

            var headerRange = worksheet.Cells[1, 1, 1, tabla.Columns.Count];
            AplicarEstiloHeader(headerRange);

            var dataRange = worksheet.Cells[2, 1, tabla.Rows.Count + 1, tabla.Columns.Count];
            AplicarBordes(dataRange);

            AplicarFormatoColumnas(worksheet, tabla, new[] { "fecha_recepcion", "fecha_salida", "fecha_traspaso", "fecha_corte" }, new[] { "monto_total", "valor_total" }, new[] { "cantidad_total", "total_registros" });

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        /// <summary>
        /// Crea hoja de Detalle con formato estándar
        /// </summary>
        private void CrearHojaDetalle(ExcelPackage package, DataTable tabla, string nombreHoja, string[] columnasFecha, string[] columnasMoneda, string[] columnasCantidad)
        {
            var worksheet = package.Workbook.Worksheets.Add(nombreHoja);

            if (tabla.Rows.Count == 0)
            {
                worksheet.Cells[1, 1].Value = "No hay datos disponibles";
                return;
            }

            worksheet.Cells["A1"].LoadFromDataTable(tabla, true);

            var headerRange = worksheet.Cells[1, 1, 1, tabla.Columns.Count];
            AplicarEstiloHeader(headerRange);

            var dataRange = worksheet.Cells[2, 1, tabla.Rows.Count + 1, tabla.Columns.Count];
            AplicarBordes(dataRange);

            AplicarFormatoColumnas(worksheet, tabla, columnasFecha, columnasMoneda, columnasCantidad);

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        /// <summary>
        /// Aplica estilo profesional al header (fondo azul, texto blanco, negrita)
        /// </summary>
        private void AplicarEstiloHeader(ExcelRange headerRange)
        {
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(68, 114, 196));
            headerRange.Style.Font.Color.SetColor(Color.White);
            headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            AplicarBordes(headerRange);
        }

        /// <summary>
        /// Aplica bordes a un rango de celdas
        /// </summary>
        private void AplicarBordes(ExcelRange range)
        {
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        /// <summary>
        /// Aplica formato específico a columnas (fechas, moneda, cantidad)
        /// </summary>
        private void AplicarFormatoColumnas(ExcelWorksheet worksheet, DataTable tabla, string[] columnasFecha, string[] columnasMoneda, string[] columnasCantidad)
        {
            for (int colIndex = 0; colIndex < tabla.Columns.Count; colIndex++)
            {
                var columnName = tabla.Columns[colIndex].ColumnName.ToLower();
                var excelColumn = worksheet.Cells[2, colIndex + 1, tabla.Rows.Count + 1, colIndex + 1];

                if (Array.Exists(columnasFecha, c => columnName.Contains(c.ToLower())))
                {
                    excelColumn.Style.Numberformat.Format = "dd/MM/yyyy";
                }
                else if (Array.Exists(columnasMoneda, c => columnName.Contains(c.ToLower())))
                {
                    excelColumn.Style.Numberformat.Format = "$#,##0.00";
                }
                else if (Array.Exists(columnasCantidad, c => columnName.Contains(c.ToLower())))
                {
                    excelColumn.Style.Numberformat.Format = "#,##0.00";
                }
            }
        }
    }
}
