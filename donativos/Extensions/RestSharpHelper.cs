using RestSharp;
using System.Net;
using System.Threading.Tasks;

namespace donativos.Extensions
{
    /// <summary>
    /// Helper para facilitar la migración de RestSharp v106 a v107+
    /// Proporciona métodos sincronos que envuelven las llamadas asincronivas
    /// </summary>
    public static class RestSharpHelper
    {
        /// <summary>
        /// Ejecuta una solicitud GET
        /// </summary>
        public static RestResponse ExecuteGet(string baseUrl, string token, string resourcePath)
        {
            var options = new RestClientOptions(baseUrl + resourcePath);
            var client = new RestClient(options);
            var request = new RestRequest();

            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", "Bearer " + token);
            }

            // GET es el método por defecto
            return client.ExecuteAsync(request).Result;
        }

        /// <summary>
        /// Ejecuta una solicitud POST con body JSON
        /// </summary>
        public static RestResponse ExecutePost(string baseUrl, string token, string resourcePath, object body)
        {
            var options = new RestClientOptions(baseUrl + resourcePath);
            var client = new RestClient(options);
            var request = new RestRequest();

            if (body != null)
            {
                request.AddJsonBody(body);
            }

            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", "Bearer " + token);
            }

            // Especificar el método HTTP
            return client.ExecuteAsync(request, Method.Post).Result;
        }

        /// <summary>
        /// Ejecuta una solicitud PUT con body JSON
        /// </summary>
        public static RestResponse ExecutePut(string baseUrl, string token, string resourcePath, object body)
        {
            var options = new RestClientOptions(baseUrl + resourcePath);
            var client = new RestClient(options);
            var request = new RestRequest();

            if (body != null)
            {
                request.AddJsonBody(body);
            }

            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", "Bearer " + token);
            }

            return client.ExecuteAsync(request, Method.Put).Result;
        }

        /// <summary>
        /// Ejecuta una solicitud DELETE
        /// </summary>
        public static RestResponse ExecuteDelete(string baseUrl, string token, string resourcePath)
        {
            var options = new RestClientOptions(baseUrl + resourcePath);
            var client = new RestClient(options);
            var request = new RestRequest();

            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", "Bearer " + token);
            }

            return client.ExecuteAsync(request, Method.Delete).Result;
        }
    }
}
