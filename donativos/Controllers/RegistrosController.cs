using donativos.Extensions;
using donativos.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace donativos.Controllers
{
    public class RegistrosController : Controller
    {
        private readonly IConfiguration _configuration;

        public RegistrosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string ApiUrl => _configuration["apiUrl"];
       
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
        /// Creates new donated articles/items by sending the provided data to the API.
        /// </summary>
        /// <param name="datos">A JSON-formatted string containing the article information to be inserted. Must not be null or empty.</param>
        /// <returns>A JSON result containing the API response content if the operation succeeds; otherwise, a JSON result
        /// indicating a bad request if an internal server error occurs.</returns>
        public JsonResult insertaArticulos(string datos)
        {
            var body = new { datos = datos };
            var token = HttpContext.Session.GetString("token");
            var response = ExecuteApi(token, "catalogos/articulos", "POST", body.datos);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(BadRequest());
            }
            return Json(response.Content);
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

        public IActionResult Index(string a)
        {
            var sessionUsuario = HttpContext.Session.GetString("usuario");
            if (sessionUsuario == null) return BadRequest();

            ViewBag.a = a;
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

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Acceso");
        }
    }
}
