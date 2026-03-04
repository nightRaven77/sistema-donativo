using donativos.Extensions;
using donativos.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;

namespace donativos.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly string _apiUrl;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(IConfiguration configuration, ILogger<AuthenticationService> logger)
        {
            _apiUrl = configuration["apiUrl"] ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthenticationResult> LoginAsync(string username, string password)
        {
            // Validar entrada
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Usuario y contraseña son requeridos",
                    ErrorCode = "EMPTY_CREDENTIALS"
                };
            }

            try
            {
                var body = new { username = username.Trim(), password };
                var response = await RestSharpHelper.ExecutePostAsync(_apiUrl, "", "auth/login", body);

                return ProcessAuthenticationResponse(response, username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante autenticación de {Usuario}", username);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Error inesperado durante la autenticación",
                    ErrorCode = "AUTHENTICATION_ERROR"
                };
            }
        }

        private AuthenticationResult ProcessAuthenticationResponse(RestResponse response, string username)
        {
            return response.StatusCode switch
            {
                HttpStatusCode.OK or HttpStatusCode.Created => 
                    DeserializeAndValidateUser(response, username),

                HttpStatusCode.BadRequest => 
                    CreateErrorResult("Credenciales inválidas", "INVALID_CREDENTIALS", username),

                HttpStatusCode.Unauthorized or HttpStatusCode.NotFound => 
                    CreateErrorResult("Usuario o contraseña incorrectos", "INVALID_LOGIN", username),

                HttpStatusCode.Forbidden => 
                    CreateErrorResult("Cuenta desactivada o sin permisos", "ACCOUNT_DISABLED", username),

                HttpStatusCode.Gone => 
                    CreateErrorResult("Cuenta eliminada", "ACCOUNT_DELETED", username),

                HttpStatusCode.InternalServerError => 
                    CreateErrorResult("Error en el servidor", "SERVER_ERROR", username),

                HttpStatusCode.ServiceUnavailable => 
                    CreateErrorResult("Servicio no disponible", "SERVICE_UNAVAILABLE", username),

                HttpStatusCode.GatewayTimeout => 
                    CreateErrorResult("Tiempo de espera agotado", "TIMEOUT", username),

                _ => CreateErrorResult($"Error desconocido ({response.StatusCode})", "UNKNOWN_ERROR", username)
            };
        }

        private AuthenticationResult DeserializeAndValidateUser(RestResponse response, string username)
        {
            try
            {
                if (string.IsNullOrEmpty(response.Content))
                {
                    _logger.LogError("Respuesta vacía del API para {Usuario}", username);
                    return CreateErrorResult("Respuesta inválida del servidor", "EMPTY_RESPONSE", username);
                }

                var user = JsonConvert.DeserializeObject<Usuario>(response.Content);

                if (user == null || string.IsNullOrWhiteSpace(user.username) || string.IsNullOrWhiteSpace(user.token))
                {
                    _logger.LogError("Datos incompletos en respuesta del API para {Usuario}", username);
                    return CreateErrorResult("Datos de usuario incompletos", "INCOMPLETE_DATA", username);
                }

                _logger.LogInformation("Autenticación exitosa para {Usuario}", username);

                return new AuthenticationResult
                {
                    Success = true,
                    Message = "Autenticación exitosa",
                    User = user
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al deserializar respuesta del API para {Usuario}", username);
                return CreateErrorResult("Error al procesar respuesta del servidor", "JSON_ERROR", username);
            }
        }

        private AuthenticationResult CreateErrorResult(string message, string errorCode, string username)
        {
            _logger.LogWarning("Fallo de autenticación para {Usuario}: {ErrorCode} - {Message}", 
                username, errorCode, message);

            return new AuthenticationResult
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }
}