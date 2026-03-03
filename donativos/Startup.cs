using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using donativos.Services;
using System;
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
            services.AddControllersWithViews();

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
                });

            // Código para guardar variables de session
            services.AddDistributedMemoryCache(); // Usar caché en memoria
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(180); // Tiempo de inactividad antes de que la sesión caduque
                options.Cookie.HttpOnly = true; // Configuración del cookie
                options.Cookie.IsEssential = true; // Necesario para la conformidad del GDPR
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            // ====== AGREGAR SERVICIOS PERSONALIZADOS ======
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
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession(); // Esto DEBE estar antes de UseEndpoints

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
