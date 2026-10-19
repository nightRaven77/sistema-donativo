using apiDonativos.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;  // ? NUEVO
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace apiDonativos.Services
{
    public interface IAuthService
    {
        string GenerateToken(string username, int bodegaID, int id_usuario);
        Task<AuthResult> ValidateCredentialsAsync(string username, string password, System.Threading.CancellationToken cancellationToken = default);
        AuthResult ValidateCredentials(string username, string password);
    }

    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public int UserId { get; set; }
        public string CentroAcopio { get; set; }
        public int CentroId { get; set; }
        public string nombreCompleto { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class AuthService : IAuthService, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly string _contentRoot;
        private readonly RSA _privateRsa;
        private readonly SigningCredentials _signingCredentials;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _contentRoot = configuration.GetValue<string>(WebHostDefaults.ContentRootKey);
            _logger = logger;
            _privateRsa = LoadPrivateKey();
            var privateKey = new RsaSecurityKey(_privateRsa);
            _signingCredentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);
        }

        private RSA LoadPrivateKey()
        {
            RSA privateRsa = RSA.Create();
            string privateKeyPath = Path.Combine(_contentRoot, _configuration["Jwt:Asymmetric:PrivateKey"] ?? "llaves/donativoPrivada.der");

            if (!File.Exists(privateKeyPath))
            {
                throw new FileNotFoundException($"Clave privada no encontrada en: {privateKeyPath}");
            }

            privateRsa.ImportRSAPrivateKey(
                source: File.ReadAllBytes(privateKeyPath),
                bytesRead: out int _
            );

            return privateRsa;
        }

        public string GenerateToken(string username, int bodegaID, int id_usuario)
        {
            // ? SEGURIDAD: Log generación de token (sin datos sensibles)
            _logger.LogInformation(
                "Generando token JWT. Usuario: {Username}, BodegaID: {BodegaId}, UserId: {UserId}",
                username,
                bodegaID,
                id_usuario
            );

            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim("BodegaID", bodegaID.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("id_usuario", id_usuario.ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: "Sistema-Donativos",
                    audience: "Centro-acopio",
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(10),
                    signingCredentials: _signingCredentials
                );

                _logger.LogInformation("Token JWT generado exitosamente para usuario: {Username}", username);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                // ? SEGURIDAD: Log error sin exponer detalles sensibles
                _logger.LogError(
                    ex,
                    "Error al generar token JWT. Usuario: {Username}",
                    username
                );

                throw new Exception($"Error al generar el token de autenticación", ex);
            }
        }

        public void Dispose()
        {
            _privateRsa?.Dispose();
        }

        public async Task<AuthResult> ValidateCredentialsAsync(string username, string password, System.Threading.CancellationToken cancellationToken = default)
        {
            // ? SEGURIDAD: NO loguear la contraseña
            _logger.LogInformation(
                "Intentando validar credenciales de forma asíncrona. Usuario: {Username}",
                username
            );

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Intento de login con credenciales vacías");

                return new AuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Usuario y contraseña son requeridos"
                };
            }

            try
            {
                using (var connection = new SqlConnection(_configuration["ConnectionStrings:donativosCn"]))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new SqlCommand("LoginUsuario", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@usuario", username);
                        command.Parameters.AddWithValue("@pass", password);

                        using (var dataReader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            if (await dataReader.ReadAsync(cancellationToken))
                            {
                                int userId = Convert.ToInt32(dataReader["id_usuario"]);

                                if (userId <= 0)
                                {
                                    // ? SEGURIDAD: Log intento fallido (sin revelar detalles)
                                    _logger.LogWarning(
                                        "Intento de login fallido. Usuario: {Username}",
                                        username
                                    );

                                    return new AuthResult
                                    {
                                        IsSuccess = false,
                                        ErrorMessage = "Usuario o contraseña incorrectos"
                                    };
                                }

                                string centroAcopio = dataReader["centro_acopio"]?.ToString() ?? string.Empty;
                                int centroId = Convert.ToInt32(dataReader["centro_id"]);
                                string nombreCompleto = dataReader["nombre_completo"]?.ToString() ?? string.Empty;

                                if (string.IsNullOrWhiteSpace(centroAcopio) || centroId <= 0)
                                {
                                    _logger.LogError(
                                        "Usuario autenticado pero datos de centro inválidos. UserId: {UserId}",
                                        userId
                                    );

                                    return new AuthResult
                                    {
                                        IsSuccess = false,
                                        ErrorMessage = "Datos del centro de acopio inválidos"
                                    };
                                }

                                // ? SEGURIDAD: Log login exitoso
                                _logger.LogInformation(
                                    "Login exitoso. Usuario: {Username}, UserId: {UserId}, Centro: {CentroId}",
                                    username,
                                    userId,
                                    centroId
                                );

                                return new AuthResult
                                {
                                    IsSuccess = true,
                                    UserId = userId,
                                    CentroAcopio = centroAcopio,
                                    CentroId = centroId,
                                    nombreCompleto = nombreCompleto
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // ? SEGURIDAD: Log error BD sin exponer estructura
                _logger.LogError(
                    sqlEx,
                    "Error de base de datos en validación de credenciales. Usuario: {Username}",
                    username
                );

                return new AuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Error al conectar con la base de datos"
                };
            }
            catch (Exception ex)
            {
                // ? SEGURIDAD: Log error inesperado
                _logger.LogError(
                    ex,
                    "Error inesperado en validación. Usuario: {Username}",
                    username
                );

                return new AuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Error inesperado durante la validación"
                };
            }

            // ? SEGURIDAD: Log intento fallido sin detalles
            _logger.LogWarning(
                "Credenciales no válidas. Usuario: {Username}",
                username
            );

            return new AuthResult
            {
                IsSuccess = false,
                ErrorMessage = "Credenciales no válidas"
            };
        }

        public AuthResult ValidateCredentials(string username, string password)
        {
            return ValidateCredentialsAsync(username, password).GetAwaiter().GetResult();
        }
    }
}
