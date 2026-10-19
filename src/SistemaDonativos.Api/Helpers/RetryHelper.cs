using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace apiDonativos.Helpers
{
    /// <summary>
    /// Utilidades para manejar reintentos en operaciones de base de datos
    /// ✅ FASE 1: Protección contra duplicados de folios en alta concurrencia
    /// </summary>
    public static class RetryHelper
    {
        /// <summary>
        /// Ejecuta una operación con lógica de reintento en caso de violación de UNIQUE constraint
        /// </summary>
        /// <typeparam name="T">Tipo de retorno de la operación</typeparam>
        /// <param name="operation">Operación a ejecutar</param>
        /// <param name="maxRetries">Número máximo de reintentos (default: 3)</param>
        /// <param name="delayMs">Delay base en milisegundos entre reintentos (default: 100ms)</param>
        /// <returns>Resultado de la operación</returns>
        public static async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            int maxRetries = 3,
            int delayMs = 100)
        {
            for (int retry = 0; retry <= maxRetries; retry++)
            {
                try
                {
                    return await operation();
                }
                catch (DbUpdateException dbEx) when (
                    retry < maxRetries && 
                    IsUniqueConstraintViolation(dbEx))
                {
                    // Backoff exponencial: 100ms, 200ms, 400ms
                    var delay = delayMs * (int)Math.Pow(2, retry);
                    await Task.Delay(delay);
                    
                    // Continuar con el siguiente intento
                    continue;
                }
                // Si no es una violación de UNIQUE o ya agotamos los reintentos, re-lanzar
                catch
                {
                    throw;
                }
            }

            // Este código nunca debería alcanzarse, pero lo incluimos por completitud
            throw new InvalidOperationException("Operación de reintento completada sin éxito");
        }

        /// <summary>
        /// Determina si una excepción de base de datos es una violación de constraint UNIQUE
        /// </summary>
        private static bool IsUniqueConstraintViolation(DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
            
            // Detectar violaciones de UNIQUE en SQL Server
            return innerMessage.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
                   innerMessage.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
                   innerMessage.Contains("Cannot insert duplicate", StringComparison.OrdinalIgnoreCase);
        }
    }
}
