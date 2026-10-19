using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace apiDonativos.Services.Base
{
    /// <summary>
    /// Clase base para todos los servicios
    /// ✅ FASE 3: Centraliza funcionalidad común
    /// </summary>
    /// <typeparam name="TService">Tipo del servicio que hereda</typeparam>
    public abstract class ServiceBase<TService> where TService : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly ILogger<TService> _logger;
        protected readonly int _commandTimeout;

        /// <summary>
        /// Constructor base con dependencias comunes
        /// </summary>
        protected ServiceBase(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<TService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // ✅ Timeout configurable desde appsettings.json
            _commandTimeout = configuration.GetValue<int>("Database:CommandTimeoutSeconds", 30);
        }

        protected readonly IConfiguration _configuration;

        /// <summary>
        /// Valida que un ID sea válido (mayor a 0)
        /// </summary>
        protected void ValidarId(int id, string nombreParametro)
        {
            if (id <= 0)
                throw new ArgumentException($"ID de {nombreParametro} inválido", nombreParametro);
        }

        /// <summary>
        /// Valida que una cadena no esté vacía
        /// </summary>
        protected void ValidarCadenaRequerida(string valor, string nombreCampo, int? longitudMinima = null, int? longitudMaxima = null)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException($"{nombreCampo} es requerido y no puede estar vacío");

            if (longitudMinima.HasValue && valor.Trim().Length < longitudMinima.Value)
                throw new ArgumentException($"{nombreCampo} debe tener al menos {longitudMinima.Value} caracteres");

            if (longitudMaxima.HasValue && valor.Length > longitudMaxima.Value)
                throw new ArgumentException($"{nombreCampo} no puede exceder {longitudMaxima.Value} caracteres");
        }

        /// <summary>
        /// Valida que un objeto no sea nulo
        /// </summary>
        protected void ValidarObjetoRequerido<T>(T objeto, string nombreParametro) where T : class
        {
            if (objeto == null)
                throw new ArgumentNullException(nombreParametro, $"{nombreParametro} no puede ser nulo");
        }

        /// <summary>
        /// Configura el timeout de comandos en el contexto actual
        /// </summary>
        protected void ConfigurarTimeout()
        {
            _context.Database.SetCommandTimeout(_commandTimeout);
        }

        /// <summary>
        /// Log de inicio de operación con métricas
        /// </summary>
        protected void LogInicioOperacion(string operacion, object parametros = null)
        {
            if (parametros != null)
            {
                _logger.LogInformation(
                    "Iniciando {Operacion}. Parámetros: {@Parametros}",
                    operacion,
                    parametros
                );
            }
            else
            {
                _logger.LogInformation("Iniciando {Operacion}", operacion);
            }
        }

        /// <summary>
        /// Log de éxito de operación con métricas
        /// </summary>
        protected void LogExitoOperacion(string operacion, long duracionMs, object resultado = null)
        {
            if (resultado != null)
            {
                _logger.LogInformation(
                    "Operación {Operacion} completada exitosamente en {DuracionMs}ms. Resultado: {@Resultado}",
                    operacion,
                    duracionMs,
                    resultado
                );
            }
            else
            {
                _logger.LogInformation(
                    "Operación {Operacion} completada exitosamente en {DuracionMs}ms",
                    operacion,
                    duracionMs
                );
            }
        }

        /// <summary>
        /// Log de error de operación
        /// </summary>
        protected void LogErrorOperacion(string operacion, Exception ex, object contexto = null)
        {
            if (contexto != null)
            {
                _logger.LogError(
                    ex,
                    "Error en {Operacion}. Contexto: {@Contexto}",
                    operacion,
                    contexto
                );
            }
            else
            {
                _logger.LogError(ex, "Error en {Operacion}", operacion);
            }
        }
    }
}
