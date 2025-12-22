using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using System.Data;
using System;
using Newtonsoft.Json;
using donativos.Models;
using Microsoft.Extensions.Configuration;

namespace donativos.Controllers
{
    public class AccesoController(IConfiguration configuration) : Controller
    {
        
        private readonly string _connectionString = configuration.GetConnectionString("donativoCn");
        // GET: AccesoController
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Usuario oUsuario)
        {

            using (SqlConnection connection = new (_connectionString))
            {
                string sql = @"LoginUsuario";
                try
                {
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("usuario",oUsuario.usuario);
                    cmd.Parameters.AddWithValue("pass", oUsuario.password);

                    connection.Open();

                    oUsuario.UsuarioID = Convert.ToInt32(cmd.ExecuteScalar().ToString());

                    if (oUsuario.UsuarioID != 0)
                    {
                        string usuario = oUsuario.usuario;
                        HttpContext.Session.SetString("usuario", JsonConvert.SerializeObject(oUsuario.usuario));
                        ViewBag.MensajeExito = "Login exitoso";
                        return RedirectToAction("Index", "Registros", new { a = usuario });
                    }
                    else
                    {
                        ViewBag.MensajeError = "Usuario o contraseña incorrectos";
                        return View();
                    }

                }
                catch (SqlException sqlEX)
                {
                    ViewBag.MensajeError = "Existe un error: " + sqlEX.Message;
                    return View();
                }
                catch (Exception ex)
                {
                    ViewBag.MensajeError = "Existe un error: " + ex.Message;
                    return View();
                }
                
           }
        }
    }
}
