using donativos.Extensions;
using donativos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace donativos.Controllers
{
    [Authorize]
    public class ConsultaController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConsultaController> _logger;

        public ConsultaController(IConfiguration configuration, ILogger<ConsultaController> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // ⭐ Configurar licencia de EPPlus 8+ una sola vez
            // En EPPlus 8.5, se debe usar ExcelPackage.LicenseContext (obsoleto pero funcional)
            // o configurar en Program.cs con services.AddSingleton<IExcelPackage>()
            #pragma warning disable CS0618 // Type or member is obsolete
            if (OfficeOpenXml.ExcelPackage.LicenseContext == OfficeOpenXml.LicenseContext.Commercial)
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            }
            #pragma warning restore CS0618 // Type or member is obsolete
        }

        private string ApiUrl => _configuration["apiUrl"];

        // ============================================================
        // CONSULTA UNIFICADA DE MOVIMIENTOS
        // ============================================================

        /// <summary>
        /// Consulta movimientos (entradas, salidas, traspasos) con filtros
        /// GET: /Consulta/ObtenerMovimientos
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> ObtenerMovimientos(
            string fechaInicio = null,
            string fechaFin = null,
            string tipoMovimiento = "todos",
            string folio = null,
            int? idDonador = null,
            int? idBeneficiario = null,
            int? idArticulo = null,
            int? idAlmacen = null,
            string estatus = null,
            int pagina = 1,
            int registrosPorPagina = 50)
        {
            var token = HttpContext.Session.GetString("token");

            try
            {
                // Construir query string con parámetros en snake_case (formato que espera la API)
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(fechaInicio))
                    queryParams.Add($"fecha_inicio={Uri.EscapeDataString(fechaInicio)}");

                if (!string.IsNullOrEmpty(fechaFin))
                    queryParams.Add($"fecha_fin={Uri.EscapeDataString(fechaFin)}");

                if (!string.IsNullOrEmpty(tipoMovimiento) && tipoMovimiento != "todos")
                    queryParams.Add($"tipo_movimiento={Uri.EscapeDataString(tipoMovimiento)}");

                if (!string.IsNullOrEmpty(folio))
                    queryParams.Add($"folio={Uri.EscapeDataString(folio)}");

                if (idDonador.HasValue)
                    queryParams.Add($"id_donador={idDonador.Value}");

                if (idBeneficiario.HasValue)
                    queryParams.Add($"id_beneficiario={idBeneficiario.Value}");

                if (idArticulo.HasValue)
                    queryParams.Add($"id_articulo={idArticulo.Value}");

                if (idAlmacen.HasValue)
                    queryParams.Add($"id_almacen={idAlmacen.Value}");

                if (!string.IsNullOrEmpty(estatus))
                    queryParams.Add($"estatus={Uri.EscapeDataString(estatus)}");

                queryParams.Add($"pagina={pagina}");
                queryParams.Add($"registros_por_pagina={registrosPorPagina}");

                var queryString = string.Join("&", queryParams);
                var endpoint = $"consulta/movimientos?{queryString}";

                _logger.LogInformation($"Consultando movimientos: {ApiUrl}{endpoint}");

                var response = await RestSharpHelper.ExecuteGetAsync(ApiUrl, token, endpoint);

                if (response.IsSuccessful)
                {
                    _logger.LogInformation($"✅ Respuesta exitosa de la API");
                    var data = JsonConvert.DeserializeObject<ConsultaMovimientosResponse>(response.Content);
                    return Json(data);
                }
                else
                {
                    _logger.LogError($"❌ Error al consultar movimientos: {response.StatusCode} - {response.Content}");
                    return Json(new ConsultaMovimientosResponse
                    {
                        Exito = false,
                        Mensaje = "Error al consultar movimientos",
                        Error = $"Status: {response.StatusCode}, Contenido: {response.Content}"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar movimientos");
                return Json(new ConsultaMovimientosResponse
                {
                    Exito = false,
                    Mensaje = "Error al consultar movimientos",
                    Error = ex.Message
                });
            }
        }

        // ============================================================
        // INVENTARIO EN TIEMPO REAL
        // ============================================================

        /// <summary>
        /// Obtiene el inventario en tiempo real de un almacén
        /// GET: /Consulta/ObtenerInventario/{idAlmacen}
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> ObtenerInventario(
            int idAlmacen,
            string busqueda = null,
            int? idCategoria = null,
            bool soloConStock = false,
            string ordenarPor = "nombre",
            string ordenDireccion = "asc")
        {
            var token = HttpContext.Session.GetString("token");

            try
            {
                // Construir query string con parámetros en snake_case
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(busqueda))
                    queryParams.Add($"busqueda={Uri.EscapeDataString(busqueda)}");

                if (idCategoria.HasValue)
                    queryParams.Add($"id_categoria={idCategoria.Value}");

                queryParams.Add($"solo_con_stock={soloConStock.ToString().ToLower()}");
                queryParams.Add($"ordenar_por={Uri.EscapeDataString(ordenarPor)}");
                queryParams.Add($"orden={Uri.EscapeDataString(ordenDireccion)}");

                var queryString = string.Join("&", queryParams);
                var endpoint = $"consulta/inventario/almacen/{idAlmacen}?{queryString}";

                _logger.LogInformation($"Consultando inventario: {endpoint}");

                var response = await RestSharpHelper.ExecuteGetAsync(ApiUrl, token, endpoint);

                if (response.IsSuccessful)
                {
                    var data = JsonConvert.DeserializeObject<InventarioAlmacenResponse>(response.Content);
                    return Json(data);
                }
                else
                {
                    _logger.LogError($"Error al consultar inventario: {response.StatusCode} - {response.Content}");
                    return Json(new InventarioAlmacenResponse
                    {
                        Exito = false,
                        Mensaje = "Error al consultar inventario",
                        Error = response.Content
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar inventario");
                return Json(new InventarioAlmacenResponse
                {
                    Exito = false,
                    Mensaje = "Error al consultar inventario",
                    Error = ex.Message
                });
            }
        }

        // ============================================================
        // DETALLE DE MOVIMIENTO ESPECÍFICO
        // ============================================================

        /// <summary>
        /// Obtiene el detalle completo de un movimiento específico
        /// GET: /Consulta/ObtenerDetalleMovimiento/{tipo}/{id}
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> ObtenerDetalleMovimiento(string tipo, int id)
        {
            var token = HttpContext.Session.GetString("token");

            try
            {
                var endpoint = $"consulta/movimientos/{tipo}/{id}";

                _logger.LogInformation($"Consultando detalle de movimiento: {ApiUrl}{endpoint}");

                var response = await RestSharpHelper.ExecuteGetAsync(ApiUrl, token, endpoint);

                if (response.IsSuccessful)
                {
                    var data = JsonConvert.DeserializeObject<DetalleMovimientoResponse>(response.Content);
                    return Json(data);
                }
                else
                {
                    _logger.LogError($"Error al consultar detalle: {response.StatusCode} - {response.Content}");
                    return Json(new DetalleMovimientoResponse
                    {
                        Exito = false,
                        Mensaje = "Error al consultar detalle del movimiento",
                        Error = response.Content
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar detalle del movimiento");
                return Json(new DetalleMovimientoResponse
                {
                    Exito = false,
                    Mensaje = "Error al consultar detalle del movimiento",
                    Error = ex.Message
                });
            }
        }

        // ============================================================
        // ESTADÍSTICAS PARA DASHBOARD
        // ============================================================

        /// <summary>
        /// Obtiene estadísticas del inventario para el dashboard
        /// GET: /Consulta/ObtenerEstadisticas/{idAlmacen}
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> ObtenerEstadisticas(int idAlmacen)
        {
            var token = HttpContext.Session.GetString("token");

            try
            {
                var endpoint = $"consulta/inventario/estadisticas/{idAlmacen}";

                _logger.LogInformation($"Consultando estadísticas: {ApiUrl}{endpoint}");

                var response = await RestSharpHelper.ExecuteGetAsync(ApiUrl, token, endpoint);

                if (response.IsSuccessful)
                {
                    var data = JsonConvert.DeserializeObject<EstadisticasInventarioResponse>(response.Content);
                    return Json(data);
                }
                else
                {
                    _logger.LogError($"Error al consultar estadísticas: {response.StatusCode} - {response.Content}");
                    return Json(new EstadisticasInventarioResponse
                    {
                        Exito = false,
                        Mensaje = "Error al consultar estadísticas",
                        Error = response.Content
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar estadísticas");
                return Json(new EstadisticasInventarioResponse
                {
                    Exito = false,
                    Mensaje = "Error al consultar estadísticas",
                    Error = ex.Message
                });
            }
        }

        // ============================================================
        // MÉTODOS AUXILIARES PARA CATÁLOGOS
        // ============================================================

        /// <summary>
        /// Obtiene lista de categorías para filtros
        /// GET: /Consulta/ObtenerCategorias
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> ObtenerCategorias()
        {
            var token = HttpContext.Session.GetString("token");

            try
            {
                var endpoint = "catalogos/articulo/categoria";
                _logger.LogInformation($"🔍 Consultando categorías: {ApiUrl}{endpoint}");

                var response = await RestSharpHelper.ExecuteGetAsync(ApiUrl, token, endpoint);

                if (response.IsSuccessful)
                {
                    _logger.LogInformation($"✅ Respuesta categorías: {response.Content}");

                    // La API puede retornar directamente un array o un objeto con data
                    var content = response.Content?.Trim();

                    // Si la respuesta ya es un array JSON, deserializar directamente
                    if (content.StartsWith("["))
                    {
                        var categorias = JsonConvert.DeserializeObject<List<Categoria>>(content);
                        return Json(new { exito = true, data = categorias });
                    }
                    // Si es un objeto con estructura {data: [...]} o {exito: true, data: [...]}
                    else if (content.StartsWith("{"))
                    {
                        var responseObj = JsonConvert.DeserializeObject<dynamic>(content);

                        // Intentar obtener el array de datos desde diferentes propiedades
                        if (responseObj.data != null)
                        {
                            var categorias = JsonConvert.DeserializeObject<List<Categoria>>(JsonConvert.SerializeObject(responseObj.data));
                            return Json(new { exito = true, data = categorias });
                        }
                        else if (responseObj.categorias != null)
                        {
                            var categorias = JsonConvert.DeserializeObject<List<Categoria>>(JsonConvert.SerializeObject(responseObj.categorias));
                            return Json(new { exito = true, data = categorias });
                        }
                        else
                        {
                            _logger.LogWarning($"⚠ Estructura de respuesta no reconocida: {content}");
                            return Json(new { exito = false, mensaje = "Estructura de respuesta no reconocida" });
                        }
                    }
                    else
                    {
                        return Json(new { exito = false, mensaje = "Formato de respuesta inválido" });
                    }
                }
                else
                {
                    _logger.LogError($"❌ Error al obtener categorías: {response.StatusCode} - {response.Content}");
                    return Json(new { exito = false, mensaje = "Error al obtener categorías", error = response.Content });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
                return Json(new { exito = false, mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene lista de artículos para filtros
        /// GET: /Consulta/ObtenerArticulos
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> ObtenerArticulos(string busqueda = null)
        {
            var token = HttpContext.Session.GetString("token");

            try
            {
                var endpoint = "catalogos/articulo";
                if (!string.IsNullOrEmpty(busqueda))
                {
                    endpoint += $"?busqueda={Uri.EscapeDataString(busqueda)}";
                }

                _logger.LogInformation($"🔍 Consultando artículos: {ApiUrl}{endpoint}");

                var response = await RestSharpHelper.ExecuteGetAsync(ApiUrl, token, endpoint);

                if (response.IsSuccessful)
                {
                    _logger.LogInformation($"✅ Respuesta artículos: {response.Content}");

                    // La API puede retornar directamente un array o un objeto con data
                    var content = response.Content?.Trim();

                    // Si la respuesta ya es un array JSON, deserializar directamente
                    if (content.StartsWith("["))
                    {
                        var articulos = JsonConvert.DeserializeObject<List<ArticuloDTO>>(content);
                        return Json(new { exito = true, data = articulos });
                    }
                    // Si es un objeto con estructura {data: [...]} o {exito: true, data: [...]}
                    else if (content.StartsWith("{"))
                    {
                        var responseObj = JsonConvert.DeserializeObject<dynamic>(content);

                        // Intentar obtener el array de datos desde diferentes propiedades
                        if (responseObj.data != null)
                        {
                            var articulos = JsonConvert.DeserializeObject<List<ArticuloDTO>>(JsonConvert.SerializeObject(responseObj.data));
                            return Json(new { exito = true, data = articulos });
                        }
                        else if (responseObj.articulos != null)
                        {
                            var articulos = JsonConvert.DeserializeObject<List<ArticuloDTO>>(JsonConvert.SerializeObject(responseObj.articulos));
                            return Json(new { exito = true, data = articulos });
                        }
                        else
                        {
                            _logger.LogWarning($"⚠ Estructura de respuesta no reconocida: {content}");
                            return Json(new { exito = false, mensaje = "Estructura de respuesta no reconocida" });
                        }
                    }
                    else
                    {
                        return Json(new { exito = false, mensaje = "Formato de respuesta inválido" });
                    }
                }
                else
                {
                    _logger.LogError($"❌ Error al obtener artículos: {response.StatusCode} - {response.Content}");
                    return Json(new { exito = false, mensaje = "Error al obtener artículos", error = response.Content });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener artículos");
                return Json(new { exito = false, mensaje = ex.Message });
            }
        }

        // ============================================================
        // EXPORTACIÓN A EXCEL
        // ============================================================

        /// <summary>
        /// Exporta los movimientos a un archivo Excel
        /// GET: /Consulta/ExportarMovimientos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportarMovimientos(
            string fechaInicio = null,
            string fechaFin = null,
            string tipoMovimiento = "todos",
            string folio = null,
            int? idArticulo = null,
            int? idAlmacen = null,
            string estatus = null)
        {
            var token = HttpContext.Session.GetString("token");

            try
            {
                // Construir query string con parámetros en snake_case
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(fechaInicio))
                    queryParams.Add($"fecha_inicio={Uri.EscapeDataString(fechaInicio)}");

                if (!string.IsNullOrEmpty(fechaFin))
                    queryParams.Add($"fecha_fin={Uri.EscapeDataString(fechaFin)}");

                // ⭐ Siempre enviar tipo_movimiento (la API podría requerirlo)
                if (!string.IsNullOrEmpty(tipoMovimiento))
                    queryParams.Add($"tipo_movimiento={Uri.EscapeDataString(tipoMovimiento)}");

                if (!string.IsNullOrEmpty(folio))
                    queryParams.Add($"folio={Uri.EscapeDataString(folio)}");

                if (idArticulo.HasValue)
                    queryParams.Add($"id_articulo={idArticulo.Value}");

                if (idAlmacen.HasValue)
                    queryParams.Add($"id_almacen={idAlmacen.Value}");

                if (!string.IsNullOrEmpty(estatus))
                    queryParams.Add($"estatus={Uri.EscapeDataString(estatus)}");

                // Solicitar TODOS los registros para exportación
                queryParams.Add($"pagina=1");
                queryParams.Add($"registros_por_pagina=10000"); // Límite alto para obtener todos

                var queryString = string.Join("&", queryParams);
                var endpoint = $"consulta/movimientos?{queryString}";

                _logger.LogInformation($"📊 EXPORTAR EXCEL - Endpoint: {ApiUrl}{endpoint}");
                _logger.LogInformation($"📊 EXPORTAR EXCEL - Parámetros recibidos: fechaInicio={fechaInicio}, fechaFin={fechaFin}, tipo={tipoMovimiento}, idAlmacen={idAlmacen}");

                var response = await RestSharpHelper.ExecuteGetAsync(ApiUrl, token, endpoint);

                _logger.LogInformation($"📊 EXPORTAR EXCEL - Response Status: {response.StatusCode}, IsSuccessful: {response.IsSuccessful}");

                if (response.IsSuccessful)
                {
                    _logger.LogInformation($"📊 EXPORTAR EXCEL - Response Content Length: {response.Content?.Length ?? 0}");
                    _logger.LogInformation($"📊 EXPORTAR EXCEL - Response Content Preview: {response.Content?.Substring(0, Math.Min(500, response.Content?.Length ?? 0))}");

                    var data = JsonConvert.DeserializeObject<ConsultaMovimientosResponse>(response.Content);

                    _logger.LogInformation($"📊 EXPORTAR EXCEL - Deserialización: Exito={data?.Exito}, Mensaje={data?.Mensaje}");
                    _logger.LogInformation($"📊 EXPORTAR EXCEL - Data != null: {data?.Data != null}");
                    _logger.LogInformation($"📊 EXPORTAR EXCEL - Movimientos != null: {data?.Data?.Movimientos != null}");
                    _logger.LogInformation($"📊 EXPORTAR EXCEL - Movimientos Count: {data?.Data?.Movimientos?.Count ?? 0}");

                    if (data?.Data?.Movimientos != null && data.Data.Movimientos.Count > 0)
                    {
                        // Generar archivo Excel
                        var excelBytes = GenerarExcelMovimientos(data.Data.Movimientos);

                        // Nombre del archivo con fecha
                        var nombreArchivo = $"Movimientos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                        return File(
                            excelBytes,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            nombreArchivo
                        );
                    }
                    else
                    {
                        // ⭐ Devolver una página HTML simple con mensaje de error
                        _logger.LogWarning("No hay movimientos para exportar con los filtros aplicados");
                        var errorHtml = @"
<!DOCTYPE html>
<html>
<head>
    <title>Sin Datos para Exportar</title>
    <style>
        body { font-family: Arial, sans-serif; text-align: center; padding: 50px; background-color: #f5f5f5; }
        .message { background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); display: inline-block; }
        .icon { font-size: 48px; color: #ff9800; }
    </style>
</head>
<body>
    <div class='message'>
        <div class='icon'>⚠️</div>
        <h2>Sin Datos para Exportar</h2>
        <p>No hay movimientos que coincidan con los filtros aplicados.</p>
        <p><small>Puede cerrar esta ventana.</small></p>
    </div>
</body>
</html>";
                        return Content(errorHtml, "text/html");
                    }
                }
                else
                {
                    _logger.LogError($"Error al obtener movimientos para exportar: {response.StatusCode}");
                    _logger.LogError($"📊 EXPORTAR EXCEL - Response Content (ERROR): {response.Content}");
                    _logger.LogError($"📊 EXPORTAR EXCEL - Response Error Message: {response.ErrorMessage}");
                    var errorHtml = @"
<!DOCTYPE html>
<html>
<head>
    <title>Error de Exportación</title>
    <style>
        body { font-family: Arial, sans-serif; text-align: center; padding: 50px; background-color: #f5f5f5; }
        .message { background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); display: inline-block; }
        .icon { font-size: 48px; color: #d32f2f; }
    </style>
</head>
<body>
    <div class='message'>
        <div class='icon'>❌</div>
        <h2>Error al Exportar</h2>
        <p>No se pudieron obtener los datos para la exportación.</p>
        <p><small>Por favor, intente nuevamente. Si el problema persiste, contacte al administrador.</small></p>
    </div>
</body>
</html>";
                    return Content(errorHtml, "text/html");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar movimientos a Excel");
                var errorHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Error de Exportación</title>
    <style>
        body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; background-color: #f5f5f5; }}
        .message {{ background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); display: inline-block; }}
        .icon {{ font-size: 48px; color: #d32f2f; }}
        .error-details {{ margin-top: 20px; padding: 10px; background: #ffebee; border-radius: 4px; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='message'>
        <div class='icon'>❌</div>
        <h2>Error al Exportar</h2>
        <p>Ocurrió un error al generar el archivo Excel.</p>
        <div class='error-details'>{ex.Message}</div>
        <p><small>Por favor, intente nuevamente o contacte al administrador.</small></p>
    </div>
</body>
</html>";
                return Content(errorHtml, "text/html");
            }
        }

        /// <summary>
        /// Genera un archivo Excel con los movimientos
        /// </summary>
        private byte[] GenerarExcelMovimientos(List<MovimientoConsultaDTO> movimientos)
        {
            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Movimientos");

            // ============================================
            // ENCABEZADOS
            // ============================================
            worksheet.Cells[1, 1].Value = "Folio";
            worksheet.Cells[1, 2].Value = "Tipo";
            worksheet.Cells[1, 3].Value = "Fecha";
            worksheet.Cells[1, 4].Value = "Descripción";
            worksheet.Cells[1, 5].Value = "Responsable";
            worksheet.Cells[1, 6].Value = "Tipo Responsable";
            worksheet.Cells[1, 7].Value = "Almacén Origen";
            worksheet.Cells[1, 8].Value = "Almacén Destino";
            worksheet.Cells[1, 9].Value = "Total Artículos";
            worksheet.Cells[1, 10].Value = "Cantidad Total";
            worksheet.Cells[1, 11].Value = "Estatus";
            worksheet.Cells[1, 12].Value = "Observaciones";

            // Estilo de encabezados
            using (var range = worksheet.Cells[1, 1, 1, 12])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(114, 47, 55)); // Color guinda
                range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }

            // ============================================
            // DATOS
            // ============================================
            int row = 2;
            foreach (var mov in movimientos)
            {
                worksheet.Cells[row, 1].Value = mov.Folio;
                worksheet.Cells[row, 2].Value = (mov.Tipo ?? "").ToUpper();
                worksheet.Cells[row, 3].Value = mov.Fecha.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cells[row, 4].Value = mov.Descripcion ?? "";
                worksheet.Cells[row, 5].Value = mov.Responsable ?? "";
                worksheet.Cells[row, 6].Value = mov.TipoResponsable ?? "";
                worksheet.Cells[row, 7].Value = mov.AlmacenOrigen ?? "";
                worksheet.Cells[row, 8].Value = mov.AlmacenDestino ?? "";
                worksheet.Cells[row, 9].Value = mov.TotalArticulos;
                worksheet.Cells[row, 10].Value = mov.CantidadTotal;
                worksheet.Cells[row, 11].Value = mov.Estatus ?? "";
                worksheet.Cells[row, 12].Value = mov.Observaciones ?? "";

                row++;
            }

            // ============================================
            // FORMATO
            // ============================================
            // Auto-ajustar columnas
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Bordes
            using (var range = worksheet.Cells[1, 1, row - 1, 12])
            {
                range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }

            // Formato de números
            worksheet.Column(9).Style.Numberformat.Format = "#,##0";  // Total Artículos
            worksheet.Column(10).Style.Numberformat.Format = "#,##0.00";  // Cantidad Total

            // Congelar primera fila
            worksheet.View.FreezePanes(2, 1);

            // ============================================
            // METADATOS
            // ============================================
            package.Workbook.Properties.Title = "Reporte de Movimientos";
            package.Workbook.Properties.Author = "Sistema de Donativos DIF Hidalgo";
            package.Workbook.Properties.Created = DateTime.Now;

            return package.GetAsByteArray();
        }
    }
}
