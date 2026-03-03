using donativos.Extensions;
using donativos.Models;
using donativos.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace donativos.Controllers
{
    /// <summary>
    /// Helper para verificar y diagnosticar acceso a recursos compartidos UNC
    /// Enfoque simplificado sin impersonación compleja
    /// </summary>
    public static class NetworkShareHelper
    {
        /// <summary>
        /// Verifica si una ruta UNC es accesible
        /// </summary>
        public static (bool success, string error) VerifyNetworkPath(string uncPath)
        {
            try
            {
                Console.WriteLine($"🔍 Verificando acceso a: {uncPath}");

                // Intentar acceso directo
                if (Directory.Exists(uncPath))
                {
                    Console.WriteLine($"✓ Ruta accesible directamente");
                    return (true, null);
                }

                Console.WriteLine($"⚠️  Ruta no accesible en acceso directo");
                
                // Intentar crear un archivo de prueba
                string testFile = Path.Combine(uncPath, $".test_{Guid.NewGuid()}");
                try
                {
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                    Console.WriteLine($"✓ Ruta accesible (prueba de lectura/escritura)");
                    return (true, null);
                }
                catch (DirectoryNotFoundException ex)
                {
                    return (false, $"Directorio no encontrado: {uncPath}. Asegúrese que la carpeta existe en el NAS.");
                }
                catch (UnauthorizedAccessException ex)
                {
                    return (false, $"Acceso denegado a {uncPath}. Verifique permisos.");
                }
                catch (System.Net.Sockets.SocketException ex)
                {
                    return (false, $"No se puede conectar al servidor. El servidor NAS puede no estar disponible.");
                }
                catch (Exception ex)
                {
                    return (false, $"{ex.GetType().Name}: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}

namespace donativos.Controllers
{
    [Authorize]
    public class RegistrosController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<RegistrosController> _logger;

        public RegistrosController(IConfiguration configuration, IEmailService emailService, ILogger<RegistrosController> logger)
        {
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
        }

        private string ApiUrl => _configuration["apiUrl"];

        #region Registro Donativos

        /// <summary>
        /// Inserta un registro completo de donativo en una sola petición
        /// Estructura: {id_donador, fecha_recepcion, tipo, estatus, observaciones, detalles[]}
        /// </summary>
        public async Task<JsonResult> insertaDonativo(string datos)
        {
            var token = HttpContext.Session.GetString("token");

            try
            {
                Console.WriteLine($"Insertando donativo completo: {datos}");

                // Validar que los datos no sean nulos
                if (string.IsNullOrWhiteSpace(datos))
                {
                    return Json(new { exito = false, mensaje = "Datos de donativo requeridos" });
                }

                // Enviar el donativo completo al API
                var response = ExecuteApi(token, "donativo", "POST", datos);

                Console.WriteLine($"Respuesta del API: {response.StatusCode} - {response.Content}");

                // Procesar la respuesta
                if (response.StatusCode == System.Net.HttpStatusCode.OK ||
                    response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    Console.WriteLine("✓ Donativo registrado correctamente");
                    
                    // Intentar enviar correo de agradecimiento
                    try
                    {
                        // Se comenta para evitar el envio hasta la autorización
                        //await EnviarCorreoAgradecimiento(token, datos);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Advertencia: No se pudo enviar correo de agradecimiento: {ex.Message}");
                        // No fallar el registro solo porque no se pudo enviar el correo
                    }
                    
                    return Json(new
                    {
                        exito = true,
                        mensaje = "Donativo registrado correctamente",
                        data = response.Content
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Console.WriteLine("❌ Datos inválidos");
                    return Json(new
                    {
                        exito = false,
                        mensaje = "Datos de donativo inválidos: " + ExtraerMensajeError(response.Content),
                        error = response.Content
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    Console.WriteLine("❌ Error interno del servidor");
                    return Json(new
                    {
                        exito = false,
                        mensaje = "Error interno del servidor: " + ExtraerMensajeError(response.Content),
                        error = response.Content
                    });
                }
                else
                {
                    Console.WriteLine($"❌ Error: {response.StatusCode}");
                    return Json(new
                    {
                        exito = false,
                        mensaje = $"Error {response.StatusCode}",
                        error = response.Content
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
                return Json(new
                {
                    exito = false,
                    mensaje = "Error inesperado al procesar el donativo",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Envía un correo de agradecimiento al donante
        /// </summary>
        private async Task EnviarCorreoAgradecimiento(string token, string datosJson)
        {
            try
            {
                // Parsear datos del donativo
                var datosObj = JsonConvert.DeserializeObject<dynamic>(datosJson);
                
                if (datosObj == null || datosObj["id_donador"] == null)
                {
                    Console.WriteLine("⚠ Datos incompletos para enviar correo");
                    return;
                }

                int idDonador = datosObj["id_donador"];
                
                // Obtener información del donador
                var responseDonador = ExecuteApi(token, $"catalogos/donador/{idDonador}", "GET");
                if (responseDonador.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"⚠ No se pudo obtener datos del donador (ID: {idDonador}). Status: {responseDonador.StatusCode}");
                    return;
                }

                // Deserializar directamente el objeto del donador (no es una lista)
                var donador = JsonConvert.DeserializeObject<dynamic>(responseDonador.Content);

                // Validar email del donador
                string emailDonador = donador["email"]?.ToString();
                if (string.IsNullOrWhiteSpace(emailDonador))
                {
                    Console.WriteLine($"⚠ Donador (ID: {idDonador}) no tiene email registrado");
                    return;
                }

                // Obtener detalles del donativo
                var datosDetalles = datosObj["detalles"];
                var articulos = new List<ArticuloAgradecimientoDTO>();

                if (datosDetalles != null && datosDetalles.Count > 0)
                {
                    await ObtenerArticulosDonativo(token, datosDetalles, articulos);
                }

                // Obtener nombre del centro de acopio de la sesión
                var nombreCentro = HttpContext.Session.GetString("centroAcopio") ?? "Centro de Acopio";

                // Crear modelo de correo
                var modeloCorreo = new EmailAgradecimientoViewModel
                {
                    NombreDonante = donador["nombre"]?.ToString() ?? "Donante",
                    EmailDestino = emailDonador,
                    FechaRecepcion = DateTime.Now,
                    Articulos = articulos,
                    NombreCentroAcopio = nombreCentro
                };

                // Enviar correo
                var enviado = await _emailService.EnviarCorreoAgradecimientoAsync(modeloCorreo);
                
                if (enviado)
                {
                    Console.WriteLine($"✓ Correo enviado exitosamente a {emailDonador} (Donador ID: {idDonador})");
                }
                else
                {
                    Console.WriteLine($"⚠ Fallo al enviar correo a {emailDonador} (Donador ID: {idDonador})");
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"❌ Error al parsear JSON: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error inesperado al enviar correo: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene información de los artículos donados
        /// </summary>
        private async Task ObtenerArticulosDonativo(string token, dynamic datosDetalles, List<ArticuloAgradecimientoDTO> articulos)
        {
            try
            {
                foreach (var detalle in datosDetalles)
                {
                    int? idArticulo = detalle["id_articulo"];
                    int? cantidad = detalle["cantidad_recibida"];

                    if (idArticulo == null || cantidad == null)
                        continue;

                    // Obtener información del artículo
                    var responseArticulo = ExecuteApi(token, $"catalogos/articulo/{idArticulo}", "GET");
                    if (responseArticulo.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        Console.WriteLine($"⚠ No se pudo obtener artículo (ID: {idArticulo})");
                        continue;
                    }

                    var articulo = JsonConvert.DeserializeObject<dynamic>(responseArticulo.Content);
                    if (articulo != null)
                    {
                        articulos.Add(new ArticuloAgradecimientoDTO
                        {
                            Nombre = articulo["nombre"]?.ToString() ?? "Artículo desconocido",
                            Cantidad = (int)cantidad,
                            UnidadMedida = articulo["unidad_medida"]?.ToString() ?? "Unidades"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Error al obtener artículos: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene los detalles de un donativo específico
        /// </summary>
        /// <param name="id_donativo"></param>
        /// <returns></returns>
        public JsonResult obtenerDonativoDetalle(int id_donativo)
        {
            var token = HttpContext.Session.GetString("token");
            var response = ExecuteApi(token, $"donativo/{id_donativo}/detalles", "GET");
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(new { error = "Error al obtener detalles de donativo" });
            }
            return Json(response.Content);
        }

        /// <summary>
        /// Obtiene los datos completos de un donativo específico
        /// </summary>
        /// <param name="id_donativo"></param>
        /// <returns></returns>
        public JsonResult obtenerDonativo(int id_donativo)
        {
            var token = HttpContext.Session.GetString("token");
            var response = ExecuteApi(token, $"donativo/{id_donativo}", "GET");
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(new { error = "Error al obtener detalles de donativo" });
            }
            return Json(response.Content);
        }

        public async Task<JsonResult> actualizaDonativo(int id_donativo, string datos)
            {
            var token = HttpContext.Session.GetString("token");
            try
            {
                Console.WriteLine($"Actualizando donativo ID {id_donativo} con datos: {datos}");
                if (string.IsNullOrWhiteSpace(datos))
                {
                    return Json(new { exito = false, mensaje = "Datos de donativo requeridos" });
                }
                var response = ExecuteApi(token, $"donativo/{id_donativo}", "PUT", datos);
                Console.WriteLine($"Respuesta del API: {response.StatusCode} - {response.Content}");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine("✓ Donativo actualizado correctamente");
                    return Json(new
                    {
                        exito = true,
                        mensaje = "Donativo actualizado correctamente",
                        data = response.Content
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Console.WriteLine("❌ Datos inválidos");
                    return Json(new
                    {
                        exito = false,
                        mensaje = "Datos de donativo inválidos",
                        error = response.Content
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    Console.WriteLine("❌ Error interno del servidor");
                    return Json(new
                    {
                        exito = false,
                        mensaje = "Error interno del servidor",
                        error = response.Content
                    });
                }
                else
                {
                    Console.WriteLine($"❌ Error: {response.StatusCode}");
                    return Json(new
                    {
                        exito = false,
                        mensaje = $"Error {response.StatusCode}",
                        error = response.Content
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
                return Json(new
                {
                    exito = false,
                    mensaje = "Error inesperado al actualizar el donativo",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene la lista de todos los donativos registrados
        /// </summary>
        public IActionResult ObtenerDonativos()
        {
            var token = HttpContext.Session.GetString("token");

            try
            {
                Console.WriteLine("=== Iniciando ObtenerDonativos ===");

                var response = ExecuteApi(token, "donativo/miUsuario", "GET");

                Console.WriteLine($"Status Code: {response.StatusCode}");
                Console.WriteLine($"Response Content (primeros 200 chars): {response.Content?.Substring(0, Math.Min(200, response.Content?.Length ?? 0))}");

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine("✓ Respuesta OK del API");

                    // Validar que el contenido no esté vacío
                    if (string.IsNullOrEmpty(response.Content))
                    {
                        Console.WriteLine("⚠ Respuesta vacía del API");
                        return Content("[]", "application/json");
                    }

                    // ⭐ IMPORTANTE: Retornar el contenido directamente sin serializar nuevamente
                    Console.WriteLine("✓ Retornando respuesta sin serializar adicional");
                    return Content(response.Content, "application/json");
                }
                else
                {
                    Console.WriteLine($"❌ Error al obtener donativos: {response.StatusCode}");
                    return Content($"{{\"error\":\"Error {response.StatusCode}\"}}", "application/json");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Excepción en ObtenerDonativos: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Content($"{{\"error\":\"{ex.Message}\"}}", "application/json");
            }
        }


        /// <summary>
        /// Elimina un donativo
        /// </summary>
        public JsonResult EliminarDonativo(int id_donativo)
        {
            var token = HttpContext.Session.GetString("token");

            try
            {
                var response = ExecuteApi(token, $"donativo/{id_donativo}", "Delete");

                if (response.StatusCode == System.Net.HttpStatusCode.OK ||
                    response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    Console.WriteLine($"Donativo eliminado correctamente");
                    return Json(new { exito = true, mensaje = "Donativo eliminado correctamente" });
                }
                else
                {
                    Console.WriteLine($"Error al eliminar: {response.StatusCode}");
                    return Json(new { exito = false, error = "Error al eliminar el donativo" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en EliminarDonativo: {ex.Message}");
                return Json(new { exito = false, error = ex.Message });
            }
        }
        
        /// <summary>
        /// Guarda los archivos adjuntos al donativo
        /// Genera nombres aleatorios para los archivos y retorna las URLs de acceso
        /// Enfoque simplificado sin complejidad innecesaria
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GuardarArchivos()
        {
            try
            {
                var files = Request.Form.Files;
                var nombresOriginales = Request.Form["nombres_originales"].ToList();

                if (files.Count == 0)
                {
                    return Json(new { exito = false, mensaje = "No hay archivos para guardar" });
                }

                // Obtener ruta del configuration
                string nasPath = _configuration["FileStorage:NasPath"];

                // Validar que la ruta esté configurada
                if (string.IsNullOrWhiteSpace(nasPath))
                {
                    Console.WriteLine("❌ Error: FileStorage:NasPath no configurado");
                    return Json(new { exito = false, mensaje = "Error de configuración: ruta NAS no configurada" });
                }

                Console.WriteLine($"📁 Ruta NAS: {nasPath}");

                // Verificar acceso antes de procesar
                Console.WriteLine($"🔍 Verificando acceso...");
                try
                {
                    if (!Directory.Exists(nasPath))
                    {
                        Console.WriteLine($"⚠️  Creando directorio...");
                        Directory.CreateDirectory(nasPath);
                    }
                }
                catch (DirectoryNotFoundException ex)
                {
                    Console.WriteLine($"❌ Ruta no encontrada: {ex.Message}");
                    return Json(new
                    {
                        exito = false,
                        mensaje = "Error: La ruta del NAS no existe",
                        diagnostico = $"Asegúrese que existe: {nasPath}"
                    });
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"❌ Acceso denegado: {ex.Message}");
                    return Json(new
                    {
                        exito = false,
                        mensaje = "Error: Permisos insuficientes",
                        diagnostico = "Verifique que el usuario tiene permisos de escritura"
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error: {ex.GetType().Name} - {ex.Message}");
                    return Json(new
                    {
                        exito = false,
                        mensaje = "Error al acceder al NAS",
                        error = ex.Message
                    });
                }

                var archivosGuardados = new List<object>();

                // Extensiones permitidas
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
                const long tamañoMaximo = 5 * 1024 * 1024; // 5 MB

                // Guardar archivos
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var nombreOriginal = nombresOriginales.Count > i ? nombresOriginales[i] : file.FileName;

                    // Validar tamaño
                    if (file.Length > tamañoMaximo)
                    {
                        _logger.LogWarning("Archivo {FileName} excede el tamaño máximo permitido: {Size} bytes", nombreOriginal, file.Length);
                        return Json(new
                        {
                            exito = false,
                            mensaje = $"El archivo '{nombreOriginal}' excede el tamaño máximo permitido de 5 MB"
                        });
                    }

                    // Validar extensión
                    string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (string.IsNullOrEmpty(extension) || !extensionesPermitidas.Contains(extension))
                    {
                        _logger.LogWarning("Archivo {FileName} tiene extensión no permitida: {Extension}", nombreOriginal, extension);
                        return Json(new
                        {
                            exito = false,
                            mensaje = $"El archivo '{nombreOriginal}' tiene una extensión no permitida. Solo se aceptan: {string.Join(", ", extensionesPermitidas)}"
                        });
                    }

                    // Validar Content-Type
                    var contentTypesPermitidos = new Dictionary<string, string[]>
                    {
                        { ".jpg", new[] { "image/jpeg" } },
                        { ".jpeg", new[] { "image/jpeg" } },
                        { ".png", new[] { "image/png" } },
                        { ".gif", new[] { "image/gif" } },
                        { ".pdf", new[] { "application/pdf" } },
                        { ".doc", new[] { "application/msword" } },
                        { ".docx", new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" } },
                        { ".xls", new[] { "application/vnd.ms-excel" } },
                        { ".xlsx", new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } }
                    };

                    if (contentTypesPermitidos.TryGetValue(extension, out var tiposEsperados))
                    {
                        if (!tiposEsperados.Contains(file.ContentType))
                        {
                            _logger.LogWarning("Archivo {FileName} tiene Content-Type incorrecto: {ContentType} (esperado: {Expected})", 
                                nombreOriginal, file.ContentType, string.Join(", ", tiposEsperados));
                            return Json(new
                            {
                                exito = false,
                                mensaje = $"El archivo '{nombreOriginal}' no es un archivo válido del tipo esperado"
                            });
                        }
                    }

                    // Generar nombre aleatorio
                    string nombreAleatorio = $"{Guid.NewGuid()}{extension}";
                    string rutaCompleta = Path.Combine(nasPath, nombreAleatorio);

                    try
                    {
                        _logger.LogInformation("Guardando archivo: {FileName} -> {SavedName}", nombreOriginal, nombreAleatorio);

                        // Guardar archivo
                        using (var stream = new FileStream(rutaCompleta, FileMode.Create, FileAccess.Write))
                        {
                            await file.CopyToAsync(stream);
                        }

                        archivosGuardados.Add(new
                        {
                            url = $"/{nombreAleatorio}",
                            nombre_original = nombreOriginal,
                            nombre_guardado = nombreAleatorio
                        });

                        _logger.LogInformation("Archivo guardado exitosamente: {SavedName}", nombreAleatorio);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al guardar archivo {FileName}", nombreOriginal);
                        return Json(new
                        {
                            exito = false,
                            mensaje = $"Error al guardar {nombreOriginal}",
                            error = ex.Message
                        });
                    }
                }

                _logger.LogInformation("{Count} archivos guardados correctamente", archivosGuardados.Count);
                return Json(new { exito = true, archivos = archivosGuardados });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error general: {ex.GetType().Name}");
                return Json(new
                {
                    exito = false,
                    mensaje = "Error al guardar archivos",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Extrae la ruta del servidor y compartida (ej: \\servidor\compartida)
        /// </summary>
        private string GetSharePath(string fullPath)
        {
            // Remove any trailing slashes and get just \\servidor\compartida
            var parts = fullPath.TrimEnd('\\').Split('\\');
            if (parts.Length >= 4)
            {
                return $"\\\\{parts[2]}\\{parts[3]}";
            }
            return fullPath;
        }
        #endregion

        #region Catalogos
        /// <summary>
        /// Creates a new donor record by sending the provided donor data to the API.
        /// </summary>
        /// <param name="datos">A JSON-formatted string containing the donor information to be inserted. Must not be null or empty.</param>
        /// <returns>A JSON result containing the API response content if the operation succeeds; otherwise, a JSON result
        /// indicating a bad request if an internal server error occurs.</returns>
        public JsonResult insertaDonador(string datos)
        {
            var body = new { datos = datos };

            var token = HttpContext.Session.GetString("token");
            var response = ExecuteApi(token, "catalogos/donador", "POST", body.datos);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(BadRequest());
            }
            return Json(response.Content);
        }

        /// <summary>
        /// Creates a new donor contact by sending the provided data to the API and returns the result as a JSON
        /// response.
        /// </summary>
        /// <param name="datos">A JSON-formatted string containing the donor contact information to be inserted. Must not be null or empty.</param>
        /// <returns>A JSON result containing the API response content if the operation succeeds; otherwise, a JSON result
        /// indicating a bad request if an internal server error occurs.</returns>
        public JsonResult insertaContactoDonaodor(string datos)
        {
            var body = new { datos = datos };
            var token = HttpContext.Session.GetString("token");
            var response = ExecuteApi(token, "catalogos/contactodonador", "POST", body.datos);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(BadRequest());
            }
            return Json(response.Content);
        }

        /// <summary>
        /// Creates new donated articles/items by sending the provided data to the API one by one.
        /// Processes each article individually to handle the insertion of multiple articles.
        /// </summary>
        /// <param name="datos">A JSON-formatted string containing an array of article information to be inserted. 
        /// Format: [{"nombre":"...","cantidad":...},{...}]</param>
        /// <returns>A JSON result containing the results of all insertions. Returns success/error status for each article.</returns>
        public JsonResult insertaArticulos(string datos)
        {
            var token = HttpContext.Session.GetString("token");
            var resultados = new List<dynamic>();
            var contador = 0;
            var errores = 0;

            try
            {
                // Parsear el JSON array de artículos
                JArray articulosArray = JArray.Parse(datos);
                contador = articulosArray.Count;

                Console.WriteLine($"Procesando {contador} artículos...");

                // Procesar cada artículo individualmente
                foreach (var articulo in articulosArray)
                {
                    try
                    {
                        // Convertir el artículo individual a string JSON
                        string articuloJson = articulo.ToString();

                        Console.WriteLine($"Insertando artículo: {articulo["nombre"]}");

                        // Enviar cada artículo al API
                        var response = ExecuteApi(token, "catalogos/articulo", "POST", articuloJson);

                        // Procesar la respuesta
                        if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                        {
                            errores++;
                            resultados.Add(new
                            {
                                exito = false,
                                nombre = articulo["nombre"],
                                mensaje = "Error interno del servidor",
                                statusCode = response.StatusCode.ToString()
                            });
                            Console.WriteLine($"  ❌ Error al insertar: {articulo["nombre"]}");
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.OK ||
                                 response.StatusCode == System.Net.HttpStatusCode.Created)
                        {
                            resultados.Add(new
                            {
                                exito = true,
                                nombre = articulo["nombre"],
                                mensaje = "Artículo insertado correctamente",
                                respuesta = response.Content
                            });
                            Console.WriteLine($"  ✓ Insertado: {articulo["nombre"]}");
                        }
                        else
                        {
                            errores++;
                            resultados.Add(new
                            {
                                exito = false,
                                nombre = articulo["nombre"],
                                mensaje = $"Error {response.StatusCode}: {response.Content}",
                                statusCode = response.StatusCode.ToString()
                            });
                            Console.WriteLine($"  ❌ Error {response.StatusCode}: {articulo["nombre"]}");
                        }
                    }
                    catch (Exception ex)
                    {
                        errores++;
                        Console.WriteLine($"  ❌ Excepción: {ex.Message}");
                        resultados.Add(new
                        {
                            exito = false,
                            nombre = articulo["nombre"] ?? "Desconocido",
                            mensaje = ex.Message,
                            error = "Excepción durante la inserción"
                        });
                    }
                }

                // Retornar resumen de resultados
                return Json(new
                {
                    exito = errores == 0,
                    procesados = contador,
                    exitingosos = contador - errores,
                    errores = errores,
                    resultados = resultados
                });
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine($"Error al parsear JSON: {ex.Message}");
                return Json(new
                {
                    exito = false,
                    mensaje = "Error al parsear el JSON",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
                return Json(new
                {
                    exito = false,
                    mensaje = "Error inesperado al procesar artículos",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Retrieves a list of donors from the API, optionally filtering by a search term.
        /// </summary>
        /// <remarks>If the API returns an internal server error, the result will contain an error message
        /// instead of donor data. The search parameter is URL-encoded before being sent to the API.</remarks>
        /// <param name="search">An optional search string used to filter donors by name or other criteria. If empty, all donors are
        /// retrieved.</param>
        /// <returns>A JSON result containing a list of donor data transfer objects. If no donors are found or an error occurs,
        /// the result contains an empty list or an error message.</returns>
        public JsonResult ObtenerDonadores(string search = "")
        {
            var token = HttpContext.Session.GetString("token");

            // Construir la ruta con parámetros
            string relativePath = string.IsNullOrEmpty(search)
                ? "catalogos/donador"
                : $"catalogos/donador/buscar/{Uri.EscapeDataString(search)}";

            var response = ExecuteApi(token, relativePath, "GET");

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(new { error = "Error al obtener donadores" });
            }

            try
            {
                if (string.IsNullOrEmpty(response.Content))
                {
                    return Json(new List<DonadorDTO>());
                }

                Console.WriteLine($"API Raw Response: {response.Content}");

                // Parsear la respuesta como lista de objetos dinámicos primero
                var parsedResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);

                List<DonadorDTO> donadores = new List<DonadorDTO>();

                if (parsedResponse is JArray jArray)
                {
                    // Si es un array directo
                    donadores = jArray
                        .Select(item => ConvertirADonador(item))
                        .Where(d => d != null)
                        .ToList();
                }
                else if (parsedResponse is JObject jObject)
                {
                    // Si es un objeto, buscar una propiedad que contenga el array
                    JArray arrayData = null;

                    if (jObject["data"] is JArray)
                        arrayData = (JArray)jObject["data"];
                    else if (jObject["donadores"] is JArray)
                        arrayData = (JArray)jObject["donadores"];
                    else if (jObject["items"] is JArray)
                        arrayData = (JArray)jObject["items"];

                    if (arrayData != null)
                    {
                        donadores = arrayData
                            .Select(item => ConvertirADonador(item))
                            .Where(d => d != null)
                            .ToList();
                    }
                }

                Console.WriteLine($"Donadores convertidos: {donadores.Count}");
                foreach (var d in donadores)
                {
                    Console.WriteLine($"  - {d.nombre} ({d.id_donador})");
                }

                return Json(donadores);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al parsear donadores: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new List<DonadorDTO>());
            }
        }

        /// <summary>
        /// Retrieves the list of contacts associated with the specified donor.
        /// </summary>
        /// <param name="id_donador">The unique identifier of the donor whose contacts are to be retrieved.</param>
        /// <returns>A JSON result containing the donor's contacts if successful; otherwise, a JSON object with an error message.</returns>
        public JsonResult ObtenerContactos(int id_donador)
        {
            var token = HttpContext.Session.GetString("token");
            var response = ExecuteApi(token, $"catalogos/contactodonador/donador/{id_donador}", "GET");
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(new { error = "Error al obtener contactos de donador" });
            }
            return Json(response.Content);
        }

        /// <summary>
        /// Retrieves a list of articles from the API, optionally filtering by a search term.
        /// </summary>
        /// <remarks>If the API returns an internal server error, the result will contain an error message
        /// instead of article data. The search parameter is URL-encoded before being sent to the API.</remarks>
        /// <param name="search">An optional search string used to filter articles by name or other criteria. If empty, all articles are
        /// retrieved.</param>
        /// <returns>A JSON result containing a list of article data transfer objects. If no articles are found or an error occurs,
        /// the result contains an empty list or an error message.</returns>
        public JsonResult ObtenerArticulos(string search = "")
        {
            var token = HttpContext.Session.GetString("token");

            // Construir la ruta con parámetros
            string relativePath = string.IsNullOrEmpty(search)
                ? "catalogos/articulo"
                : $"catalogos/articulo/buscar/{Uri.EscapeDataString(search)}";

            var response = ExecuteApi(token, relativePath, "GET");

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(new { error = "Error al obtener artículos" });
            }

            try
            {
                if (string.IsNullOrEmpty(response.Content))
                {
                    return Json(new List<ArticuloDTO>());
                }

                Console.WriteLine($"API Raw Response: {response.Content}");

                // Parsear la respuesta como lista de objetos dinámicos primero
                var parsedResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);

                List<ArticuloDTO> articulos = new List<ArticuloDTO>();

                if (parsedResponse is JArray jArray)
                {
                    // Si es un array directo
                    articulos = jArray
                        .Select(item => ConvertirAArticulo(item))
                        .Where(a => a != null)
                        .ToList();
                }
                else if (parsedResponse is JObject jObject)
                {
                    // Si es un objeto, buscar una propiedad que contenga el array
                    JArray arrayData = null;

                    if (jObject["data"] is JArray)
                        arrayData = (JArray)jObject["data"];
                    else if (jObject["articulos"] is JArray)
                        arrayData = (JArray)jObject["articulos"];
                    else if (jObject["items"] is JArray)
                        arrayData = (JArray)jObject["items"];

                    if (arrayData != null)
                    {
                        articulos = arrayData
                            .Select(item => ConvertirAArticulo(item))
                            .Where(a => a != null)
                            .ToList();
                    }
                }

                Console.WriteLine($"Artículos convertidos: {articulos.Count}");
                foreach (var a in articulos)
                {
                    Console.WriteLine($"  - {a.nombre} ({a.id_articulo})");
                }

                return Json(articulos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al parsear artículos: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new List<ArticuloDTO>());
            }
        }

        public JsonResult ObtenerAlmacenes() { 
            var token = HttpContext.Session.GetString("token");
            var response = ExecuteApi(token, "catalogos/almacen", "GET");
            if(response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(new { error = "Error al obtener almacenes" });
            }
            return Json(response.Content);
        }
        #endregion

        #region Utilidades API
        /// <summary>
        /// Executes a REST API request using the specified HTTP method, authentication token, and request parameters.
        /// </summary>
        /// <remarks>If an unsupported HTTP method is specified, the request defaults to a GET operation.
        /// The method does not perform validation on the response; callers should inspect the returned RestResponse for
        /// success or error details.</remarks>
        /// <param name="token">The authentication token to include in the request header. Cannot be null or empty.</param>
        /// <param name="relativePath">The relative path of the API endpoint to which the request is sent. Cannot be null or empty.</param>
        /// <param name="method">The HTTP method to use for the request, such as "GET", "POST", "PUT", or "DELETE". Defaults to "GET" if not
        /// specified. The value is case-insensitive.</param>
        /// <param name="body">The request body to send with the request. Used for methods that support a body, such as "POST" and "PUT".
        /// Can be null if not required.</param>
        /// <returns>A RestResponse object containing the response from the API, including status code, headers, and content.</returns>
        private RestResponse ExecuteApi(string token, string relativePath, string method = "GET", object body = null)
        {
            return method.ToUpper() switch
            {
                "GET" => RestSharpHelper.ExecuteGet(ApiUrl, token, relativePath),
                "POST" => RestSharpHelper.ExecutePost(ApiUrl, token, relativePath, body),
                "PUT" => RestSharpHelper.ExecutePut(ApiUrl, token, relativePath, body),
                "DELETE" => RestSharpHelper.ExecuteDelete(ApiUrl, token, relativePath),
                _ => RestSharpHelper.ExecuteGet(ApiUrl, token, relativePath)
            };
        }

        /// <summary>
        /// Convierte un JToken a un objeto DonadorDTO
        /// </summary>
        private DonadorDTO ConvertirADonador(JToken token)
        {
            try
            {
                if (token == null) return null;

                var donador = new DonadorDTO
                {
                    id_donador = ExtractIntValue(token, "id_donador", "id"),
                    codigo = ExtractStringValue(token, "codigo"),
                    tipo = ExtractStringValue(token, "tipo"),
                    nombre = ExtractStringValue(token, "nombre"),
                    rfc = ExtractStringValue(token, "rfc", "RFC"),
                    telefono = ExtractStringValue(token, "telefono"),
                    email = ExtractStringValue(token, "email"),
                    direccion = ExtractStringValue(token, "direccion"),
                    colonia = ExtractStringValue(token, "colonia"),
                    ciudad = ExtractStringValue(token, "ciudad"),
                    estado = ExtractStringValue(token, "estado"),
                    codigo_postal = ExtractStringValue(token, "codigo_postal"),
                    notas = ExtractStringValue(token, "notas")
                };

                // Validar que tenga al menos un nombre
                if (string.IsNullOrEmpty(donador.nombre))
                    return null;

                return donador;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error convirtiendo donador: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Convierte un JToken a un objeto ArticuloDTO
        /// </summary>
        private ArticuloDTO ConvertirAArticulo(JToken token)
        {
            try
            {
                if (token == null) return null;

                // Extraer la categoría si existe
                Categoria categoria = null;
                var categoriaToken = token["categoria"];
                if (categoriaToken != null && categoriaToken.Type != JTokenType.Null)
                {
                    categoria = new Categoria
                    {
                        id_categoria = ExtractIntValue(categoriaToken, "id_categoria", "id") ?? 0,
                        nombre = ExtractStringValue(categoriaToken, "nombre")
                    };
                }

                var articulo = new ArticuloDTO
                {
                    id_articulo = ExtractIntValue(token, "id_articulo", "id"),
                    nombre = ExtractStringValue(token, "nombre"),
                    descripcion = ExtractStringValue(token, "descripcion"),
                    id_categoria = ExtractIntValue(token, "id_categoria", "categoria_id") ?? 0,
                    unidad_medida = ExtractStringValue(token, "unidad_medida", "unidad"),
                    categoria = categoria
                };

                // Validar que tenga al menos un nombre
                if (string.IsNullOrEmpty(articulo.nombre))
                    return null;

                Console.WriteLine($"Artículo convertido: {articulo.nombre}, Categoría: {articulo.categoria?.nombre}, Unidad: {articulo.unidad_medida}");

                return articulo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error convirtiendo artículo: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extrae un valor string de un JToken, manejando arrays y campos alternativos
        /// </summary>
        private string ExtractStringValue(JToken token, params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                var value = token[fieldName];
                if (value == null) continue;

                // Si es un array, tomar el primer elemento
                if (value is JArray array)
                {
                    if (array.Count > 0)
                        return array[0].ToString();
                }
                else if (value.Type != JTokenType.Null)
                {
                    return value.ToString();
                }
            }
            return null;
        }

        /// <summary>
        /// Extrae un valor int de un JToken, manejando arrays y campos alternativos
        /// </summary>
        private int? ExtractIntValue(JToken token, params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                var value = token[fieldName];
                if (value == null) continue;

                // Si es un array, tomar el primer elemento
                if (value is JArray array)
                {
                    if (array.Count > 0 && int.TryParse(array[0].ToString(), out int result))
                        return result;
                }
                else if (value.Type != JTokenType.Null && int.TryParse(value.ToString(), out int result))
                {
                    return result;
                }
            }
            return null;
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

        #endregion

        #region Vistas y navegación
        public IActionResult Index(string a)
        {
            var sessionUsuario = HttpContext.Session.GetString("usuario");
            var centro = HttpContext.Session.GetString("centroAcopio");
            var usuario = HttpContext.Session.GetString("nombreCompleto");
            if (sessionUsuario == null) return BadRequest();

            ViewBag.a = a;
            ViewBag.centro = centro;
            ViewBag.usuario = usuario;
            return View();

        }

        public IActionResult GetView(string viewName, string a)
        {
            var sessionUsuario = HttpContext.Session.GetString("usuario");
            if (sessionUsuario == null) return BadRequest();

            return viewName?.ToLower() switch
            {
                "inicio" => PartialView("_Inicio"),
                "registros" => PartialView("_Registros"),
                "consulta" => PartialView("_Consulta"),
                "salidas" => PartialView("_Salidas"),
                "reportes" => PartialView("_Reportes"),
                "dashboard" => PartialView("_Dashboard"),
                _ => PartialView("_Error")
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CerrarSesion()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Acceso");
        }
        #endregion

        /// <summary>
        /// Sirve un archivo desde el NAS
        /// </summary>
        [HttpGet]
        public IActionResult ObtenerArchivo(string archivo)
        {
            try
            {
                var nasPath = _configuration["FileStorage:NasPath"];
                
                // El archivo viene como "/guid.extension", quitar la barra inicial
                var nombreArchivo = archivo.TrimStart('/');
                var rutaCompleta = Path.Combine(nasPath, nombreArchivo);
                
                if (!System.IO.File.Exists(rutaCompleta))
                {
                    Console.WriteLine($"❌ Archivo no encontrado: {rutaCompleta}");
                    return NotFound(new { mensaje = "Archivo no encontrado" });
                }
                
                var extension = Path.GetExtension(rutaCompleta).ToLower();
                var contentType = ObtenerContentType(extension);
                
                var bytes = System.IO.File.ReadAllBytes(rutaCompleta);
                var nombreOriginal = Path.GetFileName(rutaCompleta);
                
                Console.WriteLine($"✓ Sirviendo archivo: {nombreOriginal} ({contentType})");
                
                return File(bytes, contentType, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al obtener archivo: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error al obtener el archivo", error = ex.Message });
            }
        }

        private string ObtenerContentType(string extension)
        {
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };
        }
    }
}
