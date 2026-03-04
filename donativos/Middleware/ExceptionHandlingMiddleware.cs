using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace donativos.Middleware
{
    /// <summary>
    /// Middleware para manejo centralizado de excepciones
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción no manejada: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            var result = string.Empty;

            switch (exception)
            {
                case ArgumentNullException argumentNullException:
                    code = HttpStatusCode.BadRequest;
                    result = JsonSerializer.Serialize(new
                    {
                        exito = false,
                        mensaje = "Parámetro requerido faltante",
                        error = argumentNullException.ParamName
                    });
                    break;

                case ArgumentException argumentException:
                    code = HttpStatusCode.BadRequest;
                    result = JsonSerializer.Serialize(new
                    {
                        exito = false,
                        mensaje = "Parámetro inválido",
                        error = argumentException.Message
                    });
                    break;

                case UnauthorizedAccessException:
                    code = HttpStatusCode.Unauthorized;
                    result = JsonSerializer.Serialize(new
                    {
                        exito = false,
                        mensaje = "Acceso no autorizado",
                        error = "No tiene permisos para realizar esta operación"
                    });
                    break;

                case KeyNotFoundException keyNotFoundException:
                    code = HttpStatusCode.NotFound;
                    result = JsonSerializer.Serialize(new
                    {
                        exito = false,
                        mensaje = "Recurso no encontrado",
                        error = keyNotFoundException.Message
                    });
                    break;

                default:
                    code = HttpStatusCode.InternalServerError;
                    result = JsonSerializer.Serialize(new
                    {
                        exito = false,
                        mensaje = "Error interno del servidor",
                        error = exception.Message
                    });
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(result);
        }
    }
}
