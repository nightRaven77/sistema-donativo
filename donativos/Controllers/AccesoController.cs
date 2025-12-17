using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Data;
using System;
using Microsoft.AspNetCore.Session;
using Newtonsoft.Json;
using donativos.Models;

namespace donativos.Controllers
{
    public class AccesoController : Controller
    {
        //static string cadena = "Server=172.16.9.10;Database=contingencia2024;User Id=usContingencia;Password=d52wa1AAw2zfPDwv115V";
        static string cadena = "Server=172.16.9.10;Database=contingencia2025;User Id=yaneth;Password=xPwzspGGdm";

        // GET: AccesoController
        public ActionResult Login()
        {
            return View();
            //return RedirectToAction("Login", "Acceso");
        }

        [HttpPost]
        public ActionResult Login(Usuario oUsuario)
        {
            try
            {
                oUsuario.password = ConvertirSha256(oUsuario.password);
                using (SqlConnection cn = new SqlConnection(cadena))
                {

                    SqlCommand cmd = new SqlCommand("sp_ValidarUsuario", cn);
                    cmd.Parameters.AddWithValue("usuario", oUsuario.usuario);
                    cmd.Parameters.AddWithValue("pw", oUsuario.password);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cn.Open();

                    oUsuario.UsuarioID = Convert.ToInt32(cmd.ExecuteScalar().ToString());

                }
                using (SqlConnection cn = new SqlConnection(cadena))
                {

                    SqlCommand cmd2 = new SqlCommand("SELECT centroAcopio from usuarios where usuario='"+ oUsuario.usuario + "'", cn);
                    cmd2.CommandType = CommandType.Text;

                    SqlCommand cmd3 = new SqlCommand("SELECT CentroID from usuarios where usuario='" + oUsuario.usuario + "'", cn);
                   cmd3.CommandType = CommandType.Text;

                    cn.Open();
                    oUsuario.centroAcopio    = cmd2.ExecuteScalar().ToString();
                    oUsuario.CentroID = cmd3.ExecuteScalar().ToString();

                }

                if (oUsuario.UsuarioID != 0)
                {
                    HttpContext.Session.SetString("usuario", JsonConvert.SerializeObject(oUsuario.usuario));
                    HttpContext.Session.SetString("centroAcopio", JsonConvert.SerializeObject(oUsuario.centroAcopio));
                    HttpContext.Session.SetString("CentroID", JsonConvert.SerializeObject(oUsuario.CentroID));

                    //string usuario = "usuario01" ;

                    string usuario = oUsuario.usuario;
                    return RedirectToAction("Index", "Registros", new {a = usuario });
                    //return RedirectToAction("Index", "Registros", new { a = oUsuario.usuario });

                }
                else {
                    return View();
                }

                //return RedirectToAction(nameof(Index));
                
            }
            catch
            {
                return View();
            }
        }



        public static string ConvertirSha256(string texto)
        {
            //using System.Text;
            //USAR LA REFERENCIA DE "System.Security.Cryptography"

            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(texto));

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
    }
}
