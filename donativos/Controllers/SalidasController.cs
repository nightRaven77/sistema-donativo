using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestSharp;

using donativos.Extensions;

namespace donativos.Controllers
{
    public class SalidasController : Controller
    {
        //const string API_URL = "http://172.16.9.10/apiContingencia2024";
        const string API_URL = "http://localhost/apiContingencia";
        //const string API_URL = "http://localhost:1307";
        const string TOKEN = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InVzdWFyaW8wMSIsIkZpbmdlcnByaW50IjoiYTY5YzdlYjRkM2U0OTlkNGZlZDVhNzM3NTVhMmNhOWIxZjI4N2JhYzEzM2FmZjY1MDdiYjQ4MWVhYmMzOGU3MWVkMzgyMGZkOTk1OGExOTUzYzljYTA4OTM0NzAxNzQxNDYxMTJhMmQwMGQ0NWJkOGM0YWVkZjc2NTU3MTQwNzMiLCJCb2RlZ2FJRCI6IjEiLCJuYmYiOjE3MTk4NjUxMjYsImV4cCI6MTg5MzQ3NzU5OSwiaWF0IjoxNzE5ODY1MTI2LCJpc3MiOiJESUYtSGlkYWxnbyIsImF1ZCI6IkNlbnRyby1hY29waW8ifQ.iAFI9kWiE446hohdWSl_XlO_I37v_w_LlfmYznTK8b0vKMYbO_ZS_jG3S--KMsf8cpjB5D4C56109EQ6ZchU9fIuHVGWdoz7ibb51gkv-zf__mh4yAFi8jkUWE1RXsCCVeOcb_ahX_0JqGzXlvZeH3lkLqvf34__OxgRVHPIVmUfBdA1CTBP4sp4siNSKW97WE7QdJ6E3_1eZXLXOm0RYQB0u-jckZNe9tMYweoy9VdSK6u9GnsHt50H3oJPaZdUQ2N5ILpikBfm8zz6gfXRgWDc_Yzv2mG5czkUfMhhDGINq--qHQPm0myU3pvCcC5182k3jw-YC7FOxsgnSQ6TLevFxrdC3iwPxZzUAIcZylQ_Oi7g5bVTZjm6lP-5pYdBWXyPiXbkQJfQ4LzsBS_mvvk2gi-s2vOsAgb8_zS9dWq3dluiXVz4b5wG5zn0bwnKVR2Xztj7ZS_Ap9gRv1Rk7j6WahJC9YYuzCPC21HkZSpv4GfNwfh_hcsdAMMthbeMy4izVZ45F0x363XEIofXB3_JmkGcBCw4E7E0G6ejVKt6HbN2uiqlmaOdoFE_3G_nhm51FWuhBOfbRSsbwalqnA-9Y8o-EiP9_gmM3bNGRq4muaM0IWfQmil3V4Fg9A_VK-Zq9F8GfEYJQqKAPimDdrjuHxFDERr0f_ygWYjX3yY";

        // Helper que centraliza el uso de RestSharp
        private RestResponse ExecuteApi(string token, string relativePath, string method = "GET", object body = null)
        {
            return method.ToUpper() switch
            {
                "GET" => RestSharpHelper.ExecuteGet(API_URL, token, relativePath),
                "POST" => RestSharpHelper.ExecutePost(API_URL, token, relativePath, body),
                "PUT" => RestSharpHelper.ExecutePut(API_URL, token, relativePath, body),
                "DELETE" => RestSharpHelper.ExecuteDelete(API_URL, token, relativePath),
                _ => RestSharpHelper.ExecuteGet(API_URL, token, relativePath)
            };
        }

        public IActionResult Index(string a)
        {
            var sessionUsuario = HttpContext.Session.GetString("usuario");
            var usuario = sessionUsuario.Replace("\"", "");
            var token = "";
            if (a == usuario)
                token = TOKEN;
            ViewBag.a = token;

            // Conductor
            var responseConductor = ExecuteApi(token, "/catalogos/conductor");
            ViewBag.Conductor = responseConductor.Content;

            // Destino 
            var responseDestino = ExecuteApi(token, "/catalogos/destino");
            ViewBag.Destino = responseDestino.Content;

            //articuloSalidas
            var responseArticulos = ExecuteApi(token, "/catalogos/articulosalida");
            ViewBag.ArticuloSalidas = responseArticulos.Content;

            //registros
            var responseRegistros = ExecuteApi(token, "/registrosalidas");
            ViewBag.RegistrosSalidas = responseRegistros.Content;

            return View();
        }

        #region Conductores        
        public JsonResult buscaConductor(string a, int conductorID)
        {
            var response = ExecuteApi(a, $"/catalogos/conductor/{conductorID}");
            return Json(response.Content);
        }
        public JsonResult buscaConductoList(string a, string conductorText)
        {
            var response = ExecuteApi(a, $"/catalogos/conductor?nombre={conductorText}");
            return Json(response.Content);
        }
        public JsonResult insertaConductor(string a, string nombreConductor, string telefonoConductor)
        {
            var body = new
            {
                nombre = nombreConductor,
                telefono = telefonoConductor
            };
            var response = ExecuteApi(a, "/catalogos/conductor", "POST", body);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(BadRequest());
            }
            return Json(response.Content);
        }
        public JsonResult actualizaConductor(string a, string nombreConductor, string telefonoConductor)
        {
            var body = new
            {
                nombre = nombreConductor,
                telefono = telefonoConductor
            };
            var response = ExecuteApi(a, "/catalogos/conductor", "PUT", body);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(BadRequest());
            }
            return Json(response.Content);
        }


        #endregion

        #region Destinos
        public JsonResult buscaDestino(string a, int destinoID)
        {
            var response = ExecuteApi(a, $"/catalogos/destino/{destinoID}");
            return Json(response.Content);
        }
        public JsonResult buscaDestinosList(string a, string destinoText)
        {
            var response = ExecuteApi(a, $"/catalogos/destino?nombre={destinoText}");
            return Json(response.Content);
        }
        public JsonResult insertaDestinos(string a, string nombreDestino)
        {
            var body = new
            {
                nombre = nombreDestino
            };
            var response = ExecuteApi(a, "/catalogos/destino", "POST", body);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(BadRequest());
            }
            return Json(response.Content);
        }
        #endregion

        #region Articulos Salidas
        public JsonResult buscaArticuloSalida(string a, int articuloSalidaID)
        {
            var response = ExecuteApi(a, $"/catalogos/articulosalida/{articuloSalidaID}");
            return Json(response.Content);
        }
        public JsonResult buscaArticuloSalidaList(string a, string articuloSalidaText)
        {
            var response = ExecuteApi(a, $"/catalogos/articulosalida?nombre={articuloSalidaText}");
            return Json(response.Content);
        }
        public JsonResult insertaArticuloSalida(string a, string nombreArticuloSalida)
        {
            var body = new
            {
                nombre = nombreArticuloSalida
            };
            var response = ExecuteApi(a, "/catalogos/articulosalida", "POST", body);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(BadRequest());
            }
            return Json(response.Content);
        }
        public JsonResult actualizaArticuloSalida(string a, int articuloSalidaID, string nombreArticuloSalida)
        {
            var body = new
            {
                ArticuloSalidaID = articuloSalidaID,
                nombre = nombreArticuloSalida
            };
            var response = ExecuteApi(a, "/catalogos/articulosalida", "PUT", body);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(BadRequest());
            }
            return Json(response.Content);
        }
        #endregion

        #region Registro Salidas
        public JsonResult buscarResgistroSalidas(string a, int registroSalidaID)
        {
            var response = ExecuteApi(a, $"/registrosalidas/{registroSalidaID}");
            return Json(response.Content);
        }
        public JsonResult buscarResgistroSalidasList(string a, string resgistroSalidaText)
        {
            var response = ExecuteApi(a, $"/registrosalidas?articulo={resgistroSalidaText}");
            return Json(response.Content);
        }
        public JsonResult insertaRegistroSalida(string a, int conductorInt, int articuloInt, double cantidadInt, int destinoInt, string placaVehiculoStr, string localidadStr, string observacionesStr, string dependeciaStr, string salidaStr)
        {
            var body = new
            {
                conductorID = conductorInt,
                articuloID = articuloInt,
                cantidad = cantidadInt,
                destinoID = destinoInt,
                placaVehiculo = placaVehiculoStr,
                localidad = localidadStr,
                observaciones = observacionesStr,
                dependencia = dependeciaStr,
                salida = salidaStr
            };
            var response = ExecuteApi(a, "/registrosalidas", "POST", body);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(BadRequest());
            }
            return Json(response.Content);
        }
        public JsonResult actualizaRegistroSalida(string a, int registroSalidaInt, int conductorInt, int articuloInt, double cantidadInt, int destinoInt, string placaVehiculoStr, string localidadStr, string observacionesStr, string dependeciaStr, string salidaStr)
        {
            var body = new
            {
                RegistroSalidaID = registroSalidaInt,
                conductorID = conductorInt,
                articuloID = articuloInt,
                cantidad = cantidadInt,
                destinoID = destinoInt,
                placaVehiculo = placaVehiculoStr,
                localidad = localidadStr,
                observaciones = observacionesStr,
                dependencia = dependeciaStr,
                salida = salidaStr
            };
            var response = ExecuteApi(a, "/registrosalidas", "PUT", body);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(BadRequest());
            }
            return Json(response.Content);
        }
        public JsonResult eliminaRegistroSalida(string a, int registroSalidaID)
        {
            var response = ExecuteApi(a, $"/registrosalidas/{registroSalidaID}", "DELETE");
            return Json(response.Content);
        }
        #endregion
    }


}
