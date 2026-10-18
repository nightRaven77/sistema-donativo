using donativos.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Utils;
using RazorLight;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace donativos.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IRazorLightEngine _razorEngine;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailService(
            IConfiguration configuration, 
            ILogger<EmailService> logger, 
            IWebHostEnvironment webHostEnvironment,
            IRazorLightEngine razorEngine)
        {
            _configuration = configuration;
            _logger = logger;
            _razorEngine = razorEngine;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<bool> EnviarCorreoAgradecimientoAsync(EmailAgradecimientoViewModel modelo)
        {
            try
            {
                // Validar configuración
                var emailConfig = _configuration.GetSection("Email");
                if (emailConfig == null || !emailConfig.GetChildren().Any())
                {
                    _logger.LogError("Configuración de correo no encontrada en appsettings.json");
                    return false;
                }

                var smtpServer = emailConfig["SmtpServer"];
                var smtpPort = int.Parse(emailConfig["SmtpPort"] ?? "587");
                var senderEmail = emailConfig["SenderEmail"];
                var senderName = emailConfig["SenderName"];
                var username = emailConfig["Username"];
                var password = emailConfig["Password"];
                var enableSSL = bool.Parse(emailConfig["EnableSSL"] ?? "true");

                // Validar datos del modelo
                if (string.IsNullOrEmpty(modelo?.EmailDestino))
                {
                    _logger.LogWarning("Email de destino no proporcionado");
                    return false;
                }

                // Generar ContentId para la imagen
                string logoContentId = MimeUtils.GenerateMessageId();
                modelo.LogoContentId = logoContentId;

                // Generar HTML de la plantilla
                var templateContent = @"
                    @model donativos.Models.EmailAgradecimientoViewModel
                    <!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <link rel=""preconnect"" href=""https://fonts.googleapis.com"">
<link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
<link href=""https://fonts.googleapis.com/css2?family=Montserrat&display=swap"" rel=""stylesheet"">
    <style>
        body { font-family: 'Montserrat', Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; }
        .header { background: linear-gradient(135deg, #0284c7 0%, #1e3a8a 100%); color: white; padding: 20px; border-radius: 8px 8px 0 0; text-align: center; }
        .logo-container { margin-bottom: 15px; }
        .logo-image { max-width: 200px; height: auto; }
        .content { padding: 20px; }
        .fecha { color: #666; font-size: 14px; margin: 10px 0; }
        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        thead { background-color: #f5f5f5; }
        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
        th { font-weight: bold; color: #333; }
        .footer { background-color: #f9f9f9; padding: 15px; text-align: center; font-size: 12px; color: #999; border-radius: 0 0 8px 8px; }
        .signature { margin-top: 30px; font-weight: bold; color: #0284c7; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""logo-container"">
                        <img src=""cid:@Model.LogoContentId"" alt=""Sistema de Donativos"" class=""logo-image"">
                    </div>
            <h1>¡Gracias por tu donativo!</h1>
            <p>@Model.NombreCentroAcopio</p>
        </div>
        
        <div class=""content"">
            <p>Estimado(a) <strong>@Model.NombreDonante</strong>,</p>
            
            <p>Deseamos expresar nuestra más sincera gratitud por tu generosa donación. Tu contribución es fundamental para ayudar a quienes más lo necesitan.</p>
            
            <p class=""fecha""><strong>Fecha de recepción:</strong> @Model.FechaRecepcion.ToString(""dd/MM/yyyy HH:mm"")</p>
            
            <h3>Resumen de tu donativo:</h3>
            
            <table>
                <thead>
                    <tr>
                        <th>Artículo</th>
                        <th>Cantidad</th>
                        <th>Unidad</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var articulo in Model.Articulos ?? new System.Collections.Generic.List<donativos.Models.ArticuloAgradecimientoDTO>())
                    {
                        <tr>
                            <td>@articulo.Nombre</td>
                            <td style=""text-align: center;"">@articulo.Cantidad</td>
                            <td>@articulo.UnidadMedida</td>
                        </tr>
                    }
                </tbody>
            </table>
            
            <p>Tu donativo nos ayuda a cumplir nuestra misión de solidaridad y apoyo comunitario. Cada contribución, sin importar su tamaño, marca una diferencia positiva.</p>
            
            <div class=""signature"">
                Con gratitud,<br>
                @Model.NombreCentroAcopio
            </div>
        </div>
        
        <div class=""footer"">
            <p>Este es un correo automático. Por favor no responda a esta dirección.</p>
        </div>
    </div>
</body>
</html>";

                // Renderizar la plantilla con el modelo
                var htmlContent = await _razorEngine.CompileRenderStringAsync("email-agradecimiento", templateContent, modelo);

                // Crear mensaje de correo
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress(modelo.NombreDonante, modelo.EmailDestino));
                message.Subject = $"¡Gracias por tu donativo! - {modelo.NombreCentroAcopio}";

                // Crear cuerpo del mensaje con imagen adjunta
                var bodyBuilder = new BodyBuilder { HtmlBody = htmlContent };
                
                // ✅ NUEVA FORMA CORRECTA: Adjuntar la imagen con el ContentId
                string logoPath = ObtenerRutaLogo();
                if (!string.IsNullOrEmpty(logoPath) && System.IO.File.Exists(logoPath))
                {
                    var image = bodyBuilder.LinkedResources.Add(logoPath);
                    image.ContentId = logoContentId;
                    _logger.LogInformation($"Imagen adjuntada correctamente: {logoPath}");
                }
                else
                {
                    _logger.LogWarning($"No se encontró el archivo de logo: {logoPath}");
                }

                message.Body = bodyBuilder.ToMessageBody();

                // Enviar correo
                using (var client = new SmtpClient())
                {
                    try
                    {
                        // Conectar al servidor SMTP
                        await client.ConnectAsync(smtpServer, smtpPort,
                            enableSSL ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                        // Autenticarse
                        await client.AuthenticateAsync(username, password);

                        // Enviar el correo
                        await client.SendAsync(message);

                        _logger.LogInformation($"Correo de agradecimiento enviado exitosamente a {modelo.EmailDestino}");
                        return true;
                    }
                    finally
                    {
                        await client.DisconnectAsync(true);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al enviar correo de agradecimiento: {ex.Message}. Stack: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene la ruta correcta del logo del sistema
        /// </summary>
        private string ObtenerRutaLogo()
        {
            try
            {
                // Use the actual web root path instead of AppContext.BaseDirectory
                string logoPath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "assets", "imagenes", "logo.png");
                
                _logger.LogInformation($"Buscando logo en: {logoPath}");
                
                return logoPath;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener ruta del logo: {ex.Message}");
                return null;
            }
        }
    }
}

