using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using donativos.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace donativos
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
<<<<<<< Updated upstream
            services.AddMvc();
=======
            services.AddControllersWithViews();

            // Obtener configuración de cookies
            var cookieDomain = Configuration["AppSettings:CookieDomain"];
            var cookiePath = Configuration["AppSettings:CookiePath"] ?? "/";

            // Autenticación por cookie
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Acceso/Login";
                    options.LogoutPath = "/Registros/CerrarSesion";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(180);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.Path = cookiePath;

                    // Configurar dominio solo si está especificado
                    if (!string.IsNullOrWhiteSpace(cookieDomain))
                    {
                        options.Cookie.Domain = cookieDomain;
                    }
                });
>>>>>>> Stashed changes

            // Código para guardar variables de session
            services.AddDistributedMemoryCache(); // Usar caché en memoria
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(180); // Tiempo de inactividad antes de que la sesión caduque
                options.Cookie.HttpOnly = true; // Configuración del cookie
                options.Cookie.IsEssential = true; // Necesario para la conformidad del GDPR
<<<<<<< Updated upstream
=======
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.Path = cookiePath;

                // Configurar dominio solo si está especificado
                if (!string.IsNullOrWhiteSpace(cookieDomain))
                {
                    options.Cookie.Domain = cookieDomain;
                }
>>>>>>> Stashed changes
            });

            services.AddControllersWithViews(); // O cualquier otro servicio que necesites

            // ====== AGREGAR SERVICIOS PERSONALIZADOS ======
            // Registrar el servicio de autenticación
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            // Registrar el servicio de correo electrónico
            services.AddScoped<IEmailService, EmailService>();

            // Registrar el servicio de mapeo de unidades de red
            services.AddSingleton<INetworkMappingService, NetworkMappingService>();

            // Opcionales pero recomendados:
            // Agregar logging
            services.AddLogging(config =>
            {
                config.AddConsole();
                config.AddDebug();
            });

            // CORS si lo necesitas (comentado por si no lo usas aún)
            // services.AddCors(options =>
            // {
            //     options.AddDefaultPolicy(builder =>
            //     {
            //         builder.AllowAnyOrigin()
            //                .AllowAnyMethod()
            //                .AllowAnyHeader();
            //     });
            // });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, INetworkMappingService networkMappingService, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Mapear la unidad de red al iniciar
            _ = MapNetworkDriveOnStartup(networkMappingService, logger);

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();
            app.UseSession(); // Esto DEBE estar antes de UseEndpoints
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Acceso}/{action=Login}");
            });
        }

        /// <summary>
        /// Mapea la unidad de red al iniciar la aplicación
        /// </summary>
        private async Task MapNetworkDriveOnStartup(INetworkMappingService networkMappingService, ILogger<Startup> logger)
        {
            try
            {
                logger.LogInformation("?? Mapeando unidad de red en el inicio de la aplicación...");
                var (success, message) = await networkMappingService.MapNetworkDriveAsync();
                
                if (success)
                {
                    logger.LogInformation($"? {message}");
                }
                else
                {
                    logger.LogWarning($"??  No se pudo mapear la unidad: {message}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"? Error al mapear unidad en inicio: {ex.Message}");
            }
        }
    }
}
