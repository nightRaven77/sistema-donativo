using Microsoft.AspNetCore.Http;
using System;

namespace apiDonativos.Helpers
{
    /// <summary>
    /// Helper para obtener el CorrelationId del request actual
    /// ✅ FASE 3: Tracking de requests
    /// </summary>
    public static class CorrelationIdHelper
    {
        private const string CorrelationIdHeader = "X-Correlation-Id";

        /// <summary>
        /// Obtiene el CorrelationId del HttpContext actual
        /// </summary>
        public static string GetCorrelationId(HttpContext httpContext)
        {
            if (httpContext?.Items.TryGetValue(CorrelationIdHeader, out var correlationId) == true)
            {
                return correlationId?.ToString();
            }

            return null;
        }

        /// <summary>
        /// Obtiene el CorrelationId o genera uno nuevo si no existe
        /// </summary>
        public static string GetOrCreateCorrelationId(HttpContext httpContext)
        {
            return GetCorrelationId(httpContext) ?? Guid.NewGuid().ToString();
        }
    }
}
