using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.IO;

using Microsoft.IdentityModel.Tokens;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

using System.Security.Cryptography;
using apiDonativos.Services;
using apiDonativos.Services.Catalogos;
using apiDonativos.Middleware;

namespace apiDonativos
{
    public class Startup
    {
        public IConfiguration _configuration { get; }
        private readonly String contentRoot;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            contentRoot = configuration.GetValue<string>(WebHostDefaults.ContentRootKey);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Database configuration
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(_configuration.GetConnectionString("donativosCn")));

            // variable que procesará y generará la llave pública
            string pathKey = Path.Combine(contentRoot, _configuration["Jwt:Asymmetric:PublicKey"]);

            RSA publicRsa = RSA.Create();

            publicRsa.ImportRSAPublicKey(
                    source: File.ReadAllBytes(pathKey),
                    bytesRead: out int _
                );

            // variable que contendrá la llave pública
            RsaSecurityKey publicKey = new RsaSecurityKey(publicRsa);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = publicKey,
                    ValidAudience = "Centro-acopio",
                    ValidIssuer = "Sistema-Donativos",
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateActor = false,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,

                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

            // ? CONVENCIÓN: Configurar serialización JSON
            // PascalCase en C# ? camelCase en JSON (automático)
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Convierte propiedades C# PascalCase a JSON camelCase
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;

                    // Opciones adicionales recomendadas
                    options.JsonSerializerOptions.WriteIndented = false; // Producción: false, Dev: true
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });


            // Servicios con ciclo de vida optimizado
            services.AddSingleton<IAuthService, AuthService>();
            services.AddScoped<IDonador, DonadorService>();
            services.AddScoped<IContactoDonador, contactoDonadorService>();
            services.AddScoped<IArticulo, ArticuloService>();
            services.AddScoped<IDonativo, DonativoService>();
            services.AddScoped<IUsuario, UsuarioService>();
            services.AddScoped<IAlmacen, AlmacenService>();
            services.AddScoped<IBeneficiarioService, BeneficiarioService>();
            services.AddScoped<ITraspaso, TraspasoService>();
            services.AddScoped<ISalida, SalidaService>();

            services.AddScoped<apiDonativos.Services.Reportes.IReportesService, apiDonativos.Services.Reportes.ReportesService>();
            services.AddSingleton<apiDonativos.Services.Reportes.IExcelExportService, apiDonativos.Services.Reportes.ExcelExportService>();

            services.AddScoped<apiDonativos.Services.Consulta.IConsultaService, apiDonativos.Services.Consulta.ConsultaService>();

            // Configuración de Swagger/OpenAPI
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // Health Checks para monitoreo de estado
            services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API Donativos en línea y operativa"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                //Swagger solo en desarrollo (o configurar por appsettings)
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Donativos v1");
                    options.RoutePrefix = "swagger"; // Acceder en /swagger
                    options.DocumentTitle = "API Donativos - Documentación";
                });
            }

            // CorrelationId para tracking de requests
            app.UseCorrelationId();

            // Manejo global de excepciones para estandarizar errores 4xx/5xx
            app.UseGlobalExceptionHandler();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
