using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using donativos.Services;
using donativos.Middleware;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazorLight;

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
                    // En desarrollo, permitir HTTP; en producción, solo HTTPS
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.Path = cookiePath;

                    // Configurar dominio solo si está especificado
                    if (!string.IsNullOrWhiteSpace(cookieDomain))
                    {
                        options.Cookie.Domain = cookieDomain;
                    }
                });

            // Código para guardar variables de session
            services.AddDistributedMemoryCache(); // Usar caché en memoria
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(180); // Tiempo de inactividad antes de que la sesión caduque
                options.Cookie.HttpOnly = true; // Configuración del cookie
                options.Cookie.IsEssential = true; // Necesario para la conformidad del GDPR
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.Path = cookiePath;

                // Configurar dominio solo si está especificado
                if (!string.IsNullOrWhiteSpace(cookieDomain))
                {
                    options.Cookie.Domain = cookieDomain;
                }
            });

            // ====== AGREGAR SERVICIOS PERSONALIZADOS ======

            // Registrar RazorLightEngine como Singleton (se reutiliza en toda la aplicación)
            services.AddSingleton<IRazorLightEngine>(sp =>
            {
                return new RazorLightEngineBuilder()
                    .UseMemoryCachingProvider()
                    .Build();
            });

            // Registrar el servicio de autenticación
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            // Registrar el servicio de correo electrónico
            services.AddScoped<IEmailService, EmailService>();

            // Registrar el servicio de mapeo de unidades de red
            services.AddSingleton<INetworkMappingService, NetworkMappingService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, INetworkMappingService networkMappingService, ILogger<Startup> logger)
        {
            // Middleware de manejo de excepciones (debe ir primero)
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Acceso/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession(); // Session debe ir ANTES de Authentication/Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Mapear la unidad de red al iniciar
            _ = MapNetworkDriveOnStartup(networkMappingService, logger);

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
                logger.LogInformation("Mapeando unidad de red en el inicio de la aplicación...");
                var (success, message) = await networkMappingService.MapNetworkDriveAsync();

                if (success)
                {
                    logger.LogInformation("Unidad de red mapeada correctamente: {Message}", message);
                }
                else
                {
                    logger.LogWarning("No se pudo mapear la unidad: {Message}", message);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al mapear unidad en inicio");
            }
        }
    }
}
