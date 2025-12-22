using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using donativos.Models;
using donativos.Extensions;

namespace donativos.Controllers
{
    public class RegistrosController : Controller
    {
        private string ObtenerTokenDelUsuario(string usuario)
        {
            return usuario switch
            {
                "erick" => "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InVzdWFyaW8wMSIsIkZpbmdlcnByaW50IjoiYmQwYzM4NmJkOWYyZDA2Zjk2ZTYyMmYyZDU5NWU2NjQ4MDU3YjhmMWQ5NTMzZTgzYjc1MzNiYTA5YzQ3NTY2YWU0YTdiMzQ2M2IyYzMwMGZhOTgwNWUwNmI4MTVlODE0NzRhZDFmZmU2NTNhNjRjNTg4MWMxMDkwNWZjMjcwY2MiLCJCb2RlZ2FJRCI6IjEiLCJuYmYiOjE2OTg3ODc3NTksImV4cCI6MTg5MzQ3NzU5OSwiaWF0IjoxNjk4Nzg3NzU5LCJpc3MiOiJESUYtSGlkYWxnbyIsImF1ZCI6IkNlbnRyby1hY29waW8ifQ.kSNrQ6QpgDh1zVTLzOInTfxEXL2Qn1K6DGdwZWt6cVtCDzrA0hPubtzSQQUAIuC6ifU3_ZD_o2Wet2B1rJBOgBVwNAwSmVpZWf1VEqRgdNa_89bqziK4gob36_w8Q3v2lQAZJivggWAUgIftcYZXH24xHKwZ8FarhSYuDtrvex1PdUVOxhAOkjyQLj_tNEYTVCkaXq9YGHDZx776LwctDMORJQ485ia-MD6EHglt4fbScD_m7YN-8FtzASily9UkEr41Q_80nMaOz0nw_fo4uuwrfC7CXnldp7bPc8ZpXIHRk1Ntvn1bMmwyKTIkePC00HnbTqZISqpiOBDJ0ZM_GNJhvxEQ0XlGgs298DqcOE7b3QoWu3SXIP6xlJiOi065H9z1jAi7qjPSPCFRxS2vrlKAz4F9e15HK0KdOHL7gV1peOmsoalOHmHwFme6vZiq95FULGZ8JSlzQLoWxBkDcbjJum6qbXwjBoOLH62thLZTN4O1tY7UpEh9eKfWotu8nBDDFGvvBWqaLbumG58muIhyPZsrVDdiPiUims01QAsV9nHtXiIQ80svPHFwpAD0oKMgd_av-5iEnAzaIOlw39H0NNfLDOneHFckGDVQeiznUR87ppc-_3VGGQOqKo454_OQWLpHs_3qzAv9jxbv-qYRJ_O8T0bLKUT4KO2Wzm8",
                _ => ""
            };
        }

        public IActionResult Index(string a)
        {
            var sessionUsuario = HttpContext.Session.GetString("usuario");
            if (sessionUsuario == null) return BadRequest();

            ViewBag.a = a;
            return View();
        }

        public IActionResult GetView(string viewName, string a)
        {
            var sessionUsuario = HttpContext.Session.GetString("usuario");
            if (sessionUsuario == null) return BadRequest();

            return viewName?.ToLower() switch
            {
                "inicio" => PartialView("_Inicio"),
                "registros" => PartialView("_Registros"),
                "consulta" => PartialView("_Consulta"),
                "salidas" => PartialView("_Salidas"),
                "reportes" => PartialView("_Reportes"),
                "dashboard" => PartialView("_Dashboard"),
                _ => PartialView("_Error")
            };
        }

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Acceso");
        }
    }
}
