using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apiDonativos.Middleware
{
    /// <summary>
    /// Middleware que agrega un CorrelationId único a cada request
    /// Permite rastrear requests a través de múltiples servicios y logs
    /// ✅ FASE 3: Mejora de observabilidad
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public const string CorrelationIdHeader = "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Obtener CorrelationId del header o generar uno nuevo
            string correlationId = context.Request.Headers[CorrelationIdHeader].ToString();

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            // Agregar a los headers de respuesta
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Response.Headers.Add(CorrelationIdHeader, correlationId);
            }

            // Agregar a HttpContext.Items para acceso en controladores/servicios
            context.Items[CorrelationIdHeader] = correlationId;

            // Agregar al scope del logger para que aparezca en todos los logs
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["RequestPath"] = context.Request.Path,
                ["RequestMethod"] = context.Request.Method
            }))
            {
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error no manejado en request. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
                        correlationId,
                        context.Request.Path,
                        context.Request.Method
                    );
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Extensión para registrar el middleware fácilmente
    /// </summary>
    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
