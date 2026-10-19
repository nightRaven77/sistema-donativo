using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;
using donativos.Extensions;

namespace donativos.Controllers
{
    /// <summary>
    /// Controlador dedicado exclusivamente a la generación de reportes en formato Excel.
    /// Implementa el patrón: Frontend → ReportesController → API Externa (con token) → Blob Excel
    /// </summary>
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(IConfiguration configuration, ILogger<ReportesController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string ApiUrl => _configuration["apiUrl"];

        #region Generación de Reportes

        /// <summary>
        /// Genera el reporte de entradas en formato Excel
        /// Parámetros: { fecha_Inicio, fecha_Fin, id_Almacen?, id_donador?, id_articulo?, id_categoria?, tipo?, estatus?, agrupar_por?, ordenar_Por?, orden?, pagina?, registros_Por_Pagina? }
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GenerarReporteEntradas(string datos)
        {
            var token = HttpContext.Session.GetString("token");

            try
            {
                _logger.LogInformation("📊 Generando reporte de Entradas");
                _logger.LogDebug("Parámetros recibidos: {Datos}", datos);

                // Validar que los datos no sean nulos
                if (string.IsNullOrWhiteSpace(datos))
                {
                    _logger.LogWarning("❌ Datos de reporte requeridos");
                    return BadRequest(new { mensaje = "Parámetros de reporte requeridos" });
                }

                // Parsear y validar parámetros
                var parametros = JsonConvert.DeserializeObject<JObject>(datos);
                if (parametros == null)
                {
                    _logger.LogWarning("❌ Error al parsear parámetros");
                    return BadRequest(new { mensaje = "Error al parsear parámetros" });
                }

                // Validar fechas requeridas (con el nuevo formato: fecha_Inicio y fecha_Fin)
                if (parametros["fecha_Inicio"] == null || parametros["fecha_Fin"] == null)
                {
                    _logger.LogWarning("❌ Fechas requeridas");
                    return BadRequest(new { mensaje = "Las fechas de inicio y fin son requeridas" });
                }

                _logger.LogInformation("✓ Parámetros validados correctamente");
                _logger.LogDebug("Parámetros completos: {Parametros}", parametros.ToString());

                // Llamar a la API externa
                var response = await ExecuteApiAsync(token, "reportes/entradas", "POST", datos);

                // Procesar respuesta
                return await ProcesarRespuestaReporte(response, "Reporte_Entradas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar reporte de Entradas");
                return StatusCode(500, new { mensaje = "Error al generar el reporte", error = ex.Message });
            }
        }

        /// <summary>
        /// Genera el reporte de salidas en formato Excel
        /// Parámetros: { fecha_Inicio, fecha_Fin, id_Almacen?, id_beneficiario?, id_articulo?, id_categoria?, tipo_Salida?, estatus?, agrupar_por?, pagina?, registros_Por_Pagina? }
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GenerarReporteSalidas(string datos)
        {
            var token = HttpContext.Session.GetString("token");

            try
            {
                _logger.LogInformation("📊 Generando reporte de Salidas");
                _logger.LogDebug("Parámetros recibidos: {Datos}", datos);

                if (string.IsNullOrWhiteSpace(datos))
                {
                    _logger.LogWarning("❌ Datos de reporte requeridos");
                    return BadRequest(new { mensaje = "Parámetros de reporte requeridos" });
                }

                var parametros = JsonConvert.DeserializeObject<JObject>(datos);
                if (parametros == null)
                {
                    _logger.LogWarning("❌ Error al parsear parámetros");
                    return BadRequest(new { mensaje = "Error al parsear parámetros" });
                }

                // Validar fechas requeridas (con el nuevo formato: fecha_Inicio y fecha_Fin)
                if (parametros["fecha_Inicio"] == null || parametros["fecha_Fin"] == null)
                {
                    _logger.LogWarning("❌ Fechas requeridas");
                    return BadRequest(new { mensaje = "Las fechas de inicio y fin son requeridas" });
                }

                _logger.LogInformation("✓ Parámetros validados correctamente");
                _logger.LogDebug("Parámetros completos: {Parametros}", parametros.ToString());

                var response = await ExecuteApiAsync(token, "reportes/salidas", "POST", datos);
                return await ProcesarRespuestaReporte(response, "Reporte_Salidas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar reporte de Salidas");
                return StatusCode(500, new { mensaje = "Error al generar el reporte", error = ex.Message });
            }
        }

        /// <summary>
        /// Genera el reporte de traspasos en formato Excel
        /// Parámetros: { fecha_inicio, fecha_fin, id_almacen_origen?, id_almacen_destino?, id_articulo?, id_categoria?, estatus?, agrupar_por?, pagina?, registros_Por_Pagina? }
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GenerarReporteTraspasos(string datos)
        {
            var token = HttpContext.Session.GetString("token");

            try
            {
                _logger.LogInformation("📊 Generando reporte de Traspasos");
                _logger.LogDebug("Parámetros recibidos: {Datos}", datos);

                if (string.IsNullOrWhiteSpace(datos))
                {
                    _logger.LogWarning("❌ Datos de reporte requeridos");
                    return BadRequest(new { mensaje = "Parámetros de reporte requeridos" });
                }

                var parametros = JsonConvert.DeserializeObject<JObject>(datos);
                if (parametros == null)
                {
                    _logger.LogWarning("❌ Error al parsear parámetros");
                    return BadRequest(new { mensaje = "Error al parsear parámetros" });
                }

                // Validar fechas requeridas (formato: fecha_inicio y fecha_fin con minúsculas)
                if (parametros["fecha_inicio"] == null || parametros["fecha_fin"] == null)
                {
                    _logger.LogWarning("❌ Fechas requeridas");
                    return BadRequest(new { mensaje = "Las fechas de inicio y fin son requeridas" });
                }

                _logger.LogInformation("✓ Parámetros validados correctamente");
                _logger.LogDebug("Parámetros completos: {Parametros}", parametros.ToString());

                var response = await ExecuteApiAsync(token, "reportes/traspasos", "POST", datos);
                return await ProcesarRespuestaReporte(response, "Reporte_Traspasos");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar reporte de Traspasos");
                return StatusCode(500, new { mensaje = "Error al generar el reporte", error = ex.Message });
            }
        }

        /// <summary>
        /// Genera el reporte de existencias en formato Excel
        /// NOTA: Todos los parámetros son opcionales (a diferencia de otros reportes que requieren fechas)
        /// Parámetros: { fecha_corte?, id_almacen?, id_articulo?, id_categoria?, cantidad_minima?, cantidad_maxima?, solo_sin_existencia?, agrupar_por?, ordenar_por?, orden?, pagina?, registros_por_pagina? }
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GenerarReporteExistencias(string datos)
        {
            var token = HttpContext.Session.GetString("token");

            try
            {
                _logger.LogInformation("📊 Generando reporte de Existencias");
                _logger.LogDebug("Parámetros recibidos: {Datos}", datos);

                if (string.IsNullOrWhiteSpace(datos))
                {
                    _logger.LogWarning("❌ Datos de reporte requeridos");
                    return BadRequest(new { mensaje = "Parámetros de reporte requeridos" });
                }

                var parametros = JsonConvert.DeserializeObject<dynamic>(datos);
                if (parametros == null)
                {
                    _logger.LogWarning("❌ Error al parsear parámetros");
                    return BadRequest(new { mensaje = "Error al parsear parámetros" });
                }

                // NOTA: No se validan fechas porque son opcionales en Existencias
                // El reporte puede generar con solo filtros de cantidad, categoría, etc.

                _logger.LogInformation("✓ Parámetros validados correctamente");

                var response = await ExecuteApiAsync(token, "reportes/existencias", "POST", datos);
                return await ProcesarRespuestaReporte(response, "Reporte_Existencias");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar reporte de Existencias");
                return StatusCode(500, new { mensaje = "Error al generar el reporte", error = ex.Message });
            }
        }

        /// <summary>
        /// Genera el reporte Kardex de inventario en formato Excel
        /// Kardex = Registro detallado de movimientos de un artículo específico (entradas/salidas/existencias)
        /// Parámetros: { fecha_Inicio, fecha_Fin, id_Articulo (requerido), id_Almacen (requerido) }
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GenerarReporteKardex(string datos)
        {
            var token = HttpContext.Session.GetString("token");

            try
            {
                _logger.LogInformation("📊 Generando reporte Kardex");
                _logger.LogDebug("Parámetros recibidos: {Datos}", datos);

                if (string.IsNullOrWhiteSpace(datos))
                {
                    _logger.LogWarning("❌ Datos de reporte requeridos");
                    return BadRequest(new { mensaje = "Parámetros de reporte requeridos" });
                }

                var parametros = JsonConvert.DeserializeObject<dynamic>(datos);
                if (parametros == null)
                {
                    _logger.LogWarning("❌ Error al parsear parámetros");
                    return BadRequest(new { mensaje = "Error al parsear parámetros" });
                }

                // Validar fechas requeridas (capitalizadas con underscore como Entradas/Salidas)
                if (parametros.fecha_Inicio == null || parametros.fecha_Fin == null)
                {
                    _logger.LogWarning("❌ Fechas requeridas");
                    return BadRequest(new { mensaje = "Las fechas de inicio y fin son requeridas" });
                }

                // Validar artículo requerido (único reporte que lo requiere obligatoriamente)
                if (parametros.id_Articulo == null)
                {
                    _logger.LogWarning("❌ ID de artículo requerido");
                    return BadRequest(new { mensaje = "El ID del artículo es requerido para generar el Kardex" });
                }

                // Validar almacén requerido (Kardex necesita contexto de almacén específico)
                if (parametros.id_Almacen == null)
                {
                    _logger.LogWarning("❌ ID de almacén requerido");
                    return BadRequest(new { mensaje = "El almacén es requerido para generar el Kardex" });
                }

                _logger.LogInformation("✓ Parámetros validados correctamente");

                var response = await ExecuteApiAsync(token, "reportes/kardex", "POST", datos);
                return await ProcesarRespuestaReporte(response, "Reporte_Kardex");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar reporte Kardex");
                return StatusCode(500, new { mensaje = "Error al generar el reporte", error = ex.Message });
            }
        }

        #endregion

        #region Métodos Auxiliares

        /// <summary>
        /// Procesa la respuesta de la API y retorna el archivo Excel como blob
        /// </summary>
        private async Task<IActionResult> ProcesarRespuestaReporte(RestResponse response, string nombreReporte)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("✓ {NombreReporte} generado correctamente", nombreReporte);

                // Retornar el archivo Excel como blob
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var nombreArchivo = $"{nombreReporte}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(response.RawBytes, contentType, nombreArchivo);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogWarning("❌ Parámetros inválidos para {NombreReporte}", nombreReporte);

                // Intentar extraer mensaje de error
                var mensaje = ExtraerMensajeError(response.Content);
                return BadRequest(new { mensaje = $"Parámetros inválidos: {mensaje}" });
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError("❌ Error interno del servidor al generar {NombreReporte}", nombreReporte);

                var mensaje = ExtraerMensajeError(response.Content);
                return StatusCode(500, new { mensaje = $"Error al generar el reporte: {mensaje}" });
            }
            else
            {
                _logger.LogError("❌ Error desconocido al generar {NombreReporte}: {StatusCode}", nombreReporte, response.StatusCode);
                return StatusCode((int)response.StatusCode, new { mensaje = $"Error {response.StatusCode}" });
            }
        }

        /// <summary>
        /// Extrae el mensaje de error de un response JSON
        /// </summary>
        private string ExtraerMensajeError(string jsonContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonContent))
                    return "Error desconocido";

                var obj = JObject.Parse(jsonContent);

                // Intentar diferentes nombres de campo
                var mensaje = obj["message"]?.ToString()
                           ?? obj["mensaje"]?.ToString()
                           ?? obj["error"]?.ToString()
                           ?? jsonContent;

                return mensaje;
            }
            catch
            {
                // Si falla el parseo, devolver el contenido completo
                return jsonContent;
            }
        }

        /// <summary>
        /// Ejecuta una petición a la API externa con el token de sesión
        /// </summary>
        private async Task<RestResponse> ExecuteApiAsync(string token, string relativePath, string method = "GET", object body = null)
        {
            return method.ToUpper() switch
            {
                "GET" => await RestSharpHelper.ExecuteGetAsync(ApiUrl, token, relativePath),
                "POST" => await RestSharpHelper.ExecutePostAsync(ApiUrl, token, relativePath, body),
                "PUT" => await RestSharpHelper.ExecutePutAsync(ApiUrl, token, relativePath, body),
                "DELETE" => await RestSharpHelper.ExecuteDeleteAsync(ApiUrl, token, relativePath),
                _ => await RestSharpHelper.ExecuteGetAsync(ApiUrl, token, relativePath)
            };
        }

        #endregion
    }
}
