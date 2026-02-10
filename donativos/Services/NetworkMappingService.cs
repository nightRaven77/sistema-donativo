using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace donativos.Services
{
    /// <summary>
    /// Servicio para mapear unidades de red con credenciales
    /// Ejecuta: net use \\servidor\carpeta contraseña /user:usuario /persistent:yes
    /// </summary>
    public interface INetworkMappingService
    {
        Task<(bool success, string message)> MapNetworkDriveAsync();
        Task<(bool success, string message)> DisconnectNetworkDriveAsync();
    }

    public class NetworkMappingService : INetworkMappingService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NetworkMappingService> _logger;

        public NetworkMappingService(IConfiguration configuration, ILogger<NetworkMappingService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Mapea la unidad de red con credenciales
        /// </summary>
        public async Task<(bool success, string message)> MapNetworkDriveAsync()
        {
            try
            {
                var nasPath = _configuration["FileStorage:NasPath"];
                var username = _configuration["FileStorage:Username"];
                var password = _configuration["FileStorage:Password"];

                if (string.IsNullOrWhiteSpace(nasPath) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    var msg = "Configuración incompleta para mapear unidad de red";
                    _logger.LogWarning(msg);
                    return (false, msg);
                }

                _logger.LogInformation($"?? Mapeando unidad de red: {nasPath}");

                // Extraer solo la ruta compartida (\\servidor\carpeta)
                string sharePath = ExtractSharePath(nasPath);
                
                _logger.LogInformation($"?? Share path: {sharePath}");
                _logger.LogInformation($"?? Usuario: {username}");

                // Comando: net use \\servidor\carpeta /user:usuario contraseña /persistent:yes
                string command = $"net use \"{sharePath}\" /user:\"{username}\" \"{password}\" /persistent:yes";

                _logger.LogInformation("Ejecutando comando: net use [RUTA] /user:[USUARIO] [CONTRASEÑA] /persistent:yes");

                var result = await ExecuteCommandAsync("cmd.exe", $"/c {command}");

                if (result.success)
                {
                    _logger.LogInformation($"? Unidad mapeada exitosamente: {sharePath}");
                    return (true, $"Unidad mapeada: {sharePath}");
                }
                else
                {
                    _logger.LogError($"? Error al mapear unidad: {result.message}");
                    return (false, result.message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Excepción en MapNetworkDriveAsync: {ex.Message}");
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Desconecta la unidad de red
        /// </summary>
        public async Task<(bool success, string message)> DisconnectNetworkDriveAsync()
        {
            try
            {
                var nasPath = _configuration["FileStorage:NasPath"];
                
                if (string.IsNullOrWhiteSpace(nasPath))
                    return (false, "NasPath no configurado");

                string sharePath = ExtractSharePath(nasPath);
                
                _logger.LogInformation($"Desconectando unidad: {sharePath}");

                // Comando: net use \\servidor\carpeta /delete
                string command = $"net use \"{sharePath}\" /delete /yes";
                
                var result = await ExecuteCommandAsync("cmd.exe", $"/c {command}");

                if (result.success)
                {
                    _logger.LogInformation($"? Unidad desconectada: {sharePath}");
                    return (true, $"Unidad desconectada: {sharePath}");
                }
                else
                {
                    _logger.LogWarning($"??  Error al desconectar: {result.message}");
                    return (false, result.message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Excepción en DisconnectNetworkDriveAsync: {ex.Message}");
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta un comando en la línea de comandos
        /// </summary>
        private async Task<(bool success, string message)> ExecuteCommandAsync(string fileName, string arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using (var process = Process.Start(processInfo))
                    {
                        if (process == null)
                            return (false, "No se pudo iniciar el proceso");

                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();

                        process.WaitForExit();

                        if (process.ExitCode == 0)
                        {
                            return (true, output ?? "Comando ejecutado exitosamente");
                        }
                        else
                        {
                            // Error code 67 significa que la unidad ya está mapeada (consideramos exitoso)
                            if (process.ExitCode == 67 && arguments.Contains("/delete"))
                            {
                                return (true, "La unidad no estaba conectada");
                            }
                            
                            return (false, $"Error {process.ExitCode}: {error ?? output}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            });
        }

        /// <summary>
        /// Extrae la ruta del servidor y compartida (ej: \\servidor\compartida)
        /// </summary>
        private string ExtractSharePath(string fullPath)
        {
            try
            {
                fullPath = fullPath.TrimEnd('\\');
                string[] parts = fullPath.Split('\\');
                if (parts.Length >= 4)
                {
                    return $"\\\\{parts[2]}\\{parts[3]}";
                }
                return fullPath;
            }
            catch
            {
                return fullPath;
            }
        }
    }
}
