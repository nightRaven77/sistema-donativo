using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;

namespace donativos.Extensions
{
    /// <summary>
    /// Helper para ejecutar llamadas HTTP con RestSharp de forma eficiente.
    /// Usa un RestClient singleton para evitar socket exhaustion.
    /// </summary>
    public static class RestSharpHelper
    {
        private static readonly Lazy<RestClient> _client = new Lazy<RestClient>(() => new RestClient());

        /// <summary>
        /// Ejecuta una solicitud GET de forma asíncrona
        /// </summary>
        public static async Task<RestResponse> ExecuteGetAsync(string baseUrl, string token, string resourcePath)
        {
            var request = new RestRequest(baseUrl + resourcePath, Method.Get);

            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", "Bearer " + token);
            }

            return await _client.Value.ExecuteAsync(request);
        }

        /// <summary>
        /// Ejecuta una solicitud POST con body JSON de forma asíncrona
        /// </summary>
        public static async Task<RestResponse> ExecutePostAsync(string baseUrl, string token, string resourcePath, object body)
        {
            var request = new RestRequest(baseUrl + resourcePath, Method.Post);

            if (body != null)
            {
                request.AddJsonBody(body);
            }

            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", "Bearer " + token);
            }

            return await _client.Value.ExecuteAsync(request);
        }

        /// <summary>
        /// Ejecuta una solicitud PUT con body JSON de forma asíncrona
        /// </summary>
        public static async Task<RestResponse> ExecutePutAsync(string baseUrl, string token, string resourcePath, object body)
        {
            var request = new RestRequest(baseUrl + resourcePath, Method.Put);

            if (body != null)
            {
                request.AddJsonBody(body);
            }

            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", "Bearer " + token);
            }

            return await _client.Value.ExecuteAsync(request);
        }

        /// <summary>
        /// Ejecuta una solicitud DELETE de forma asíncrona
        /// </summary>
        public static async Task<RestResponse> ExecuteDeleteAsync(string baseUrl, string token, string resourcePath)
        {
            var request = new RestRequest(baseUrl + resourcePath, Method.Delete);

            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", "Bearer " + token);
            }

            return await _client.Value.ExecuteAsync(request);
        }
    }
}
