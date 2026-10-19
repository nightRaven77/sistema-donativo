using apiDonativos.Models.Dtos.Reportes;
using apiDonativos.Services.Reportes;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace apiDonativos.Controllers
{
    /// <summary>
    /// Controlador para generación de reportes en formato Excel
    /// ✅ ARQUITECTURA: API-First, retorna archivos binarios (.xlsx)
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly IReportesService _reportesService;
        private readonly IExcelExportService _excelService;
        private readonly ILogger<ReportesController> _logger;
        private readonly IConfiguration _configuration;

        public ReportesController(
            IReportesService reportesService,
            IExcelExportService excelService,
            ILogger<ReportesController> logger,
            IConfiguration configuration)
        {
            _reportesService = reportesService;
            _excelService = excelService;
            _logger = logger;
            _configuration = configuration;

        }

        /// <summary>
        /// Genera reporte de entradas (donativos) en formato Excel
        /// POST: reportes/entradas
        /// </summary>
        /// <param name="parametros">Filtros del reporte</param>
        /// <returns>Archivo Excel (.xlsx)</returns>
        [HttpPost("entradas")]
        public async Task<IActionResult> GenerarReporteEntradas([FromBody] ReporteEntradasDto parametros)
        {
            if (parametros == null)
                return BadRequest(new { mensaje = "Parámetros no proporcionados" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation(
                    "Generando reporte de entradas. Fechas: {FechaInicio} - {FechaFin}",
                    parametros.fecha_inicio,
                    parametros.fecha_fin
                );

                var dataSet = await _reportesService.ObtenerReporteEntradasAsync(parametros);

                if (dataSet == null || dataSet.Tables.Count == 0)
                {
                    return NotFound(new { mensaje = "No se encontraron datos para el reporte" });
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var nombreArchivo = $"Reporte_Entradas_{timestamp}";

                var archivoExcel = await _excelService.GenerarExcelEntradasAsync(dataSet, nombreArchivo);

                _logger.LogInformation(
                    "Reporte de entradas generado exitosamente. Archivo: {NombreArchivo}.xlsx",
                    nombreArchivo
                );

                return File(
                    archivoExcel,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{nombreArchivo}.xlsx"
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación fallida en reporte de entradas");
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                var referenceId = Guid.NewGuid().ToString();
                _logger.LogError(
                    ex,
                    "Error al generar reporte de entradas. ReferenceId: {ReferenceId}",
                    referenceId
                );

                #if DEBUG
                    return StatusCode(500, new { mensaje = ex.Message, reference_id = referenceId });
                #else
                    return StatusCode(500, new { mensaje = "Error al generar el reporte", reference_id = referenceId });
                #endif
            }
        }

        /// <summary>
        /// Genera reporte de salidas (entregas) en formato Excel
        /// POST: reportes/salidas
        /// </summary>
        /// <param name="parametros">Filtros del reporte</param>
        /// <returns>Archivo Excel (.xlsx)</returns>
        [HttpPost("salidas")]
        public async Task<IActionResult> GenerarReporteSalidas([FromBody] ReporteSalidasDto parametros)
        {
            if (parametros == null)
                return BadRequest(new { mensaje = "Parámetros no proporcionados" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation(
                    "Generando reporte de salidas. Fechas: {FechaInicio} - {FechaFin}",
                    parametros.fecha_inicio,
                    parametros.fecha_fin
                );

                var dataSet = await _reportesService.ObtenerReporteSalidasAsync(parametros);

                if (dataSet == null || dataSet.Tables.Count == 0)
                {
                    return NotFound(new { mensaje = "No se encontraron datos para el reporte" });
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var nombreArchivo = $"Reporte_Salidas_{timestamp}";

                var archivoExcel = await _excelService.GenerarExcelSalidasAsync(dataSet, nombreArchivo);

                _logger.LogInformation(
                    "Reporte de salidas generado exitosamente. Archivo: {NombreArchivo}.xlsx",
                    nombreArchivo
                );

                return File(
                    archivoExcel,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{nombreArchivo}.xlsx"
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación fallida en reporte de salidas");
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                var referenceId = Guid.NewGuid().ToString();
                _logger.LogError(
                    ex,
                    "Error al generar reporte de salidas. ReferenceId: {ReferenceId}",
                    referenceId
                );

                #if DEBUG
                    return StatusCode(500, new { mensaje = ex.Message, reference_id = referenceId });
                #else
                    return StatusCode(500, new { mensaje = "Error al generar el reporte", reference_id = referenceId });
                #endif
            }
        }

        /// <summary>
        /// Genera reporte de traspasos entre almacenes en formato Excel
        /// POST: reportes/traspasos
        /// </summary>
        /// <param name="parametros">Filtros del reporte</param>
        /// <returns>Archivo Excel (.xlsx)</returns>
        [HttpPost("traspasos")]
        public async Task<IActionResult> GenerarReporteTraspasos([FromBody] ReporteTraspasosDto parametros)
        {
            if (parametros == null)
                return BadRequest(new { mensaje = "Parámetros no proporcionados" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation(
                    "Generando reporte de traspasos. Fechas: {FechaInicio} - {FechaFin}",
                    parametros.fecha_inicio,
                    parametros.fecha_fin
                );

                var dataSet = await _reportesService.ObtenerReporteTraspasosAsync(parametros);

                if (dataSet == null || dataSet.Tables.Count == 0)
                {
                    return NotFound(new { mensaje = "No se encontraron datos para el reporte" });
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var nombreArchivo = $"Reporte_Traspasos_{timestamp}";

                var archivoExcel = await _excelService.GenerarExcelTraspasosAsync(dataSet, nombreArchivo);

                _logger.LogInformation(
                    "Reporte de traspasos generado exitosamente. Archivo: {NombreArchivo}.xlsx",
                    nombreArchivo
                );

                return File(
                    archivoExcel,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{nombreArchivo}.xlsx"
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación fallida en reporte de traspasos");
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                var referenceId = Guid.NewGuid().ToString();
                _logger.LogError(
                    ex,
                    "Error al generar reporte de traspasos. ReferenceId: {ReferenceId}",
                    referenceId
                );

                #if DEBUG
                    return StatusCode(500, new { mensaje = ex.Message, reference_id = referenceId });
                #else
                    return StatusCode(500, new { mensaje = "Error al generar el reporte", reference_id = referenceId });
                #endif
            }
        }

        /// <summary>
        /// Genera reporte de existencias actuales en formato Excel
        /// POST: reportes/existencias
        /// </summary>
        /// <param name="parametros">Filtros del reporte</param>
        /// <returns>Archivo Excel (.xlsx)</returns>
        [HttpPost("existencias")]
        public async Task<IActionResult> GenerarReporteExistencias([FromBody] ReporteExistenciasDto parametros)
        {
            if (parametros == null)
                return BadRequest(new { mensaje = "Parámetros no proporcionados" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation(
                    "Generando reporte de existencias. Almacén: {AlmacenId}, FechaCorte: {FechaCorte}",
                    parametros.id_almacen,
                    parametros.fecha_corte
                );

                var dataSet = await _reportesService.ObtenerReporteExistenciasAsync(parametros);

                if (dataSet == null || dataSet.Tables.Count == 0)
                {
                    return NotFound(new { mensaje = "No se encontraron datos para el reporte" });
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var nombreArchivo = $"Reporte_Existencias_{timestamp}";

                var archivoExcel = await _excelService.GenerarExcelExistenciasAsync(dataSet, nombreArchivo);

                _logger.LogInformation(
                    "Reporte de existencias generado exitosamente. Archivo: {NombreArchivo}.xlsx",
                    nombreArchivo
                );

                return File(
                    archivoExcel,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{nombreArchivo}.xlsx"
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación fallida en reporte de existencias");
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                var referenceId = Guid.NewGuid().ToString();
                _logger.LogError(
                    ex,
                    "Error al generar reporte de existencias. ReferenceId: {ReferenceId}",
                    referenceId
                );

                #if DEBUG
                    return StatusCode(500, new { mensaje = ex.Message, reference_id = referenceId });
                #else
                    return StatusCode(500, new { mensaje = "Error al generar el reporte", reference_id = referenceId });
                #endif
            }
        }

        /// <summary>
        /// Genera reporte Kardex (historial de movimientos) en formato Excel
        /// POST: reportes/kardex
        /// ⚠️ Requiere id_almacen y id_articulo obligatorios
        /// </summary>
        /// <param name="parametros">Parámetros del reporte (almacén y artículo requeridos)</param>
        /// <returns>Archivo Excel (.xlsx)</returns>
        [HttpPost("kardex")]
        public async Task<IActionResult> GenerarKardexInventario([FromBody] ReporteKardexDto parametros)
        {
            if (parametros == null)
                return BadRequest(new { mensaje = "Parámetros no proporcionados" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation(
                    "Generando Kardex. Almacén: {AlmacenId}, Artículo: {ArticuloId}",
                    parametros.id_almacen,
                    parametros.id_articulo
                );

                var dataSet = await _reportesService.ObtenerKardexInventarioAsync(parametros);

                if (dataSet == null || dataSet.Tables.Count == 0)
                {
                    return NotFound(new { mensaje = "No se encontraron movimientos para el artículo especificado" });
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var nombreArchivo = $"Kardex_Almacen{parametros.id_almacen}_Articulo{parametros.id_articulo}_{timestamp}";

                var archivoExcel = await _excelService.GenerarExcelKardexAsync(dataSet, nombreArchivo);

                _logger.LogInformation(
                    "Kardex generado exitosamente. Archivo: {NombreArchivo}.xlsx",
                    nombreArchivo
                );

                return File(
                    archivoExcel,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{nombreArchivo}.xlsx"
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Validación fallida en Kardex. Almacén: {AlmacenId}, Artículo: {ArticuloId}",
                    parametros?.id_almacen,
                    parametros?.id_articulo
                );
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                var referenceId = Guid.NewGuid().ToString();
                _logger.LogError(
                    ex,
                    "Error al generar Kardex. ReferenceId: {ReferenceId}",
                    referenceId
                );

                #if DEBUG
                    return StatusCode(500, new { mensaje = ex.Message, reference_id = referenceId });
                #else
                    return StatusCode(500, new { mensaje = "Error al generar el reporte", reference_id = referenceId });
                #endif
            }
        }

        ///<summary>
        /// Genera la impresión de los distintos formatos:
        /// 1 -> Entrada
        /// 2 -> Salida
        /// 3 -> Traspasos
        /// </summary>
        /// 
        [HttpGet("{id:int}/ssrs")]
        public async Task<IActionResult> GetReport(int id,[FromQuery] int tipoReporte)
        {
            _logger.LogInformation("Generando impresión de tipo={tipoReporte}, para el id={id}", tipoReporte, id);
            
                        var ssrsUrl = _configuration["SsrsSettings:Url"];
            var reportPath = _configuration["SsrsSettings:ReportPath"];
            var username = _configuration["SsrsSettings:Username"];
            var password = _configuration["SsrsSettings:Password"];
            var domain = _configuration["SsrsSettings:Domain"];
            var report = "";

            report = tipoReporte switch
            {
                1 => "entrada",
                2 => "salida",
                3 => "traspaso",
                _ => null
            };

            var finalUrl = $"{ssrsUrl.TrimEnd('/')}?{Uri.EscapeDataString(reportPath+report)}&rs:Command=Render&rs:Format=PDF&id={id}";

            try
            {
                var handler = new System.Net.Http.HttpClientHandler();
                if (!string.IsNullOrWhiteSpace(username))
                {
                    handler.Credentials = string.IsNullOrWhiteSpace(domain)
                        ? new System.Net.NetworkCredential(username, password)
                        : new System.Net.NetworkCredential(username, password, domain);
                }

                using var client = new System.Net.Http.HttpClient(handler);
                var response = await client.GetAsync(finalUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al obtener reporte de SSRS. Código: {StatusCode}, Detalles: {Details}", response.StatusCode, errorContent);
                    return Problem(
                        detail: $"SSRS returned {response.StatusCode}: {errorContent}",
                        statusCode: StatusCodes.Status502BadGateway,
                        title: "SSRS Report Server error");
                }

                var pdfStream = await response.Content.ReadAsStreamAsync();
                var fileName = $"{report ?? id.ToString()}.pdf";
                return File(pdfStream, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción ocurrida al generar el reporte de SSRS para la solicitud. Id={RequestId}", id);
                return Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error generating report");
            }
        }
    }
}
