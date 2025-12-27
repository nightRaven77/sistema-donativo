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
            services.AddMvc();

            // Código para guardar variables de session
            services.AddDistributedMemoryCache(); // Usar caché en memoria
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(180); // Tiempo de inactividad antes de que la sesión caduque
                options.Cookie.HttpOnly = true; // Configuración del cookie
                options.Cookie.IsEssential = true; // Necesario para la conformidad del GDPR
            });

            services.AddControllersWithViews(); // O cualquier otro servicio que necesites

            // ====== AGREGAR SERVICIOS PERSONALIZADOS ======
            // Registrar el servicio de autenticación
            services.AddScoped<IAuthenticationService, AuthenticationService>();

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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();
            app.UseSession(); // Esto DEBE estar antes de UseEndpoints
            
            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllerRoute("default", "{controller=Registros}/{action=Index}");
                endpoints.MapControllerRoute("default", "{controller=Acceso}/{action=Login}");
            });
        }
    }
}
