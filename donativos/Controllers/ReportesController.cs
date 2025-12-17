using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using donativos.Extensions;
using donativos.Models;

namespace donativos.Controllers
{
    public class ReportesController(IConfiguration configuration) : Controller
    {
        //Todo la información que se muestra se obtiene de otro proyecto que es una api, se llama apiContingencia. 
        //Si no funciona el consumo de la api, verificar los token
        public JsonResult donador(string text, string a)
        {
            var response = RestSharpHelper.ExecuteGet("http://172.16.9.10/apiContingencia2024", a, $"/catalogos/donadores?nombre={text}");
            return Json(response.Content);
        }

        private readonly string _connectionString = configuration.GetConnectionString("contingenciaCn");

        public IActionResult Index(string a)
        {

            var sessionUsuario = HttpContext.Session.GetString("usuario");
            var usuario = sessionUsuario.Replace("\"", "");
            var token = "";
            if (a == usuario)
                //if (a == "dt10rrsz")
                token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InVzdWFyaW8wMSIsIkZpbmdlcnByaW50IjoiYTY5YzdlYjRkM2U0OTlkNGZlZDVhNzM3NTVhMmNhOWIxZjI4N2JhYzEzM2FmZjY1MDdiYjQ4MWVhYmMzOGU3MWVkMzgyMGZkOTk1OGExOTUzYzljYTA4OTM0NzAxNzQxNDYxMTJhMmQwMGQ0NWJkOGM0YWVkZjc2NTU3MTQwNzMiLCJCb2RlZ2FJRCI6IjEiLCJuYmYiOjE3MTk4NjUxMjYsImV4cCI6MTg5MzQ3NzU5OSwiaWF0IjoxNzE5ODY1MTI2LCJpc3MiOiJESUYtSGlkYWxnbyIsImF1ZCI6IkNlbnRyby1hY29waW8ifQ.iAFI9kWiE446hohdWSl_XlO_I37v_w_LlfmYznTK8b0vKMYbO_ZS_jG3S--KMsf8cpjB5D4C56109EQ6ZchU9fIuHVGWdoz7ibb51gkv-zf__mh4yAFi8jkUWE1RXsCCVeOcb_ahX_0JqGzXlvZeH3lkLqvf34__OxgRVHPIVmUfBdA1CTBP4sp4siNSKW97WE7QdJ6E3_1eZXLXOm0RYQB0u-jckZNe9tMYweoy9VdSK6u9GnsHt50H3oJPaZdUQ2N5ILpikBfm8zz6gfXRgWDc_Yzv2mG5czkUfMhhDGINq--qHQPm0myU3pvCcC5182k3jw-YC7FOxsgnSQ6TLevFxrdC3iwPxZzUAIcZylQ_Oi7g5bVTZjm6lP-5pYdBWXyPiXbkQJfQ4LzsBS_mvvk2gi-s2vOsAgb8_zS9dWq3dluiXVz4b5wG5zn0bwnKVR2Xztj7ZS_Ap9gRv1Rk7j6WahJC9YYuzCPC21HkZSpv4GfNwfh_hcsdAMMthbeMy4izVZ45F0x363XEIofXB3_JmkGcBCw4E7E0G6ejVKt6HbN2uiqlmaOdoFE_3G_nhm51FWuhBOfbRSsbwalqnA-9Y8o-EiP9_gmM3bNGRq4muaM0IWfQmil3V4Fg9A_VK-Zq9F8GfEYJQqKAPimDdrjuHxFDERr0f_ygWYjX3yY";
            var response = RestSharpHelper.ExecuteGet("http://172.16.9.10/apiContingencia2024", token, "/catalogos/donadores");
            ViewBag.Don = response.Content;
            ViewBag.a = token;
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return BadRequest();
            }
            else
            {
                try
                {
                    List<CentroAcopio> centrosAcopio = ObtenerCentrosAcopio();
                    ViewBag.CentrosAcopio = centrosAcopio;

                    return View();
                }
                catch (Exception)
                {

                    throw;
                }

            }
        }
        //Aquí no voy a hacer uso de la api, se obtiene el catalogo de centros de acopio. 
        
        private List<CentroAcopio> ObtenerCentrosAcopio()
        {
            List<CentroAcopio> centrosAcopio = new List<CentroAcopio>();

            using (Microsoft.Data.SqlClient.SqlConnection cn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString))
            {
                Microsoft.Data.SqlClient.SqlCommand cmd2 = new Microsoft.Data.SqlClient.SqlCommand("SELECT distinct CatCentrosAcopio.CentroID, CatCentrosAcopio.centroAcopio from usuarios  inner join CatCentrosAcopio on CatCentrosAcopio.CentroID = Usuarios.CentroID where usuarios.activo = 1", cn);
                cmd2.CommandType = CommandType.Text;
                cn.Open();

                using (SqlDataReader reader = cmd2.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        centrosAcopio.Add(new CentroAcopio
                        {
                            CentroID = Convert.ToInt32(reader["CentroID"]),
                            centroAcopio = reader["centroAcopio"].ToString()
                        });

                    }
                }
            }

            return centrosAcopio;
        }

        public IActionResult generaReporte(int opcion, string fechaInicio, string fechaFinal, int donador, int centro)
        {

            DataTable dt = obtieneDatos(opcion, fechaInicio, fechaFinal, donador, centro);
            string excelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            switch (opcion)
            {
                case 1:
                case 2:
                case 3:
                    using (var libro = new ExcelPackage())
                    {
                        ExcelWorksheet ws;

                        ws = libro.Workbook.Worksheets.Add("Reporte");
                        ws.Cells["A1"].LoadFromDataTable(dt, true);

                        using (ExcelRange rng = ws.Cells[1, 1, 1, dt.Columns.Count])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            rng.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                            rng.AutoFilter = true;
                            rng.AutoFitColumns();
                        }
                        return File(libro.GetAsByteArray(), excelContentType, "ReporteDonativos.xlsx");
                    }
                case 4:
                case 5:
                case 6:
                    string titulo = opcion switch
                    {
                        4 => "Reporte de Donativos por Categoria y Donador",
                        5 => "Reporte de Donativos por Fecha y Categoria",
                        6 => "Reporte de Donativos por Centro de Acopio y Categoria",
                        _ => "Reporte",
                    };
                    using (var libro = new ExcelPackage())
                    {

                        ExcelWorksheet ws;

                        ws = libro.Workbook.Worksheets.Add("Reporte");

                        ws.Cells["A8"].LoadFromDataTable(dt, true, OfficeOpenXml.Table.TableStyles.Light1);

                        string imagePath = "wwwroot/assets/imagenes/logodif.png"; // Replace with your image path
                        var picture = ws.Drawings.AddPicture("LogoDIF", new FileInfo(imagePath));

                        // Set position (row, column, offsets)
                        picture.SetPosition(0, 0, 0, 0); // Row 2, Column 1

                        // Optionally set size
                        picture.SetSize(288, 80); // Width: 150px, Height: 100px

                        using (ExcelRange titleRange = ws.Cells[5, 1, 5, 8])
                        {
                            titleRange.Merge = true;
                            titleRange.Value = titulo;
                            titleRange.Style.Font.Size = 14;
                            titleRange.Style.Font.Bold = true;
                            titleRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                            titleRange.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        }
                        using (ExcelRange horaRange = ws.Cells[6, 1, 6, 8])
                        {
                            horaRange.Merge = true;
                            horaRange.Value = "Fecha Corte: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                            horaRange.Style.Font.Size = 14;
                            horaRange.Style.Font.Bold = true;
                            horaRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                            horaRange.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        }


                        using (ExcelRange rng = ws.Cells[8, 1, ws.Dimension.End.Row, ws.Dimension.End.Column])
                        {
                            rng.AutoFitColumns();
                        }

                        if (opcion != 6)
                        {
                            using (ExcelRange rng = ws.Cells[ws.Dimension.End.Row, 1, ws.Dimension.End.Row, ws.Dimension.End.Column])
                            {
                                rng.Style.Font.Bold = true;
                                rng.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                rng.Style.Border.Top.Style = ExcelBorderStyle.Medium;
                                rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                rng.Style.Fill.BackgroundColor.SetColor(Color.LightSlateGray);
                            }
                        }

                        ExcelWorksheet ws2;

                        DataTable dt2 = obtieneDatos(1, fechaInicio, fechaFinal, donador, centro);

                        ws2 = libro.Workbook.Worksheets.Add("Detalle");
                        ws2.Cells["A1"].LoadFromDataTable(dt2, true);

                        using (ExcelRange rng = ws2.Cells[1, 1, 1, dt2.Columns.Count])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            rng.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                            rng.AutoFilter = true;
                            rng.AutoFitColumns();
                        }


                        return File(libro.GetAsByteArray(), excelContentType, "ReporteDonativos.xlsx");
                    }
                default:
                    using (var libro = new ExcelPackage())
                    {
                        ExcelWorksheet ws;

                        ws = libro.Workbook.Worksheets.Add("Reporte");
                        ws.Cells["A1"].LoadFromDataTable(dt, true);

                        using (ExcelRange rng = ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            rng.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                            rng.AutoFitColumns();
                        }
                        return File(libro.GetAsByteArray(), excelContentType, "ReporteDonativos.xlsx");
                    }
            }
        }

        public JsonResult generaMensaje(int opcion, string fechaInicio, string fechaFinal, int donador, int centro)
                {
                    DataTable dt = obtieneDatos(opcion, fechaInicio, fechaFinal, donador, centro);
                    string mensaje = "";
                    foreach (DataRow row in dt.Rows)
                    {
                        mensaje += row["mensaje"].ToString() + "\n";
                    }
                    return Json(mensaje);
                }

        public IActionResult Regresar()
        {
            var sessionUsuario = HttpContext.Session.GetString("usuario");
            var usuario = sessionUsuario.Replace("\"", "");
            return RedirectToAction("Index", "Registros", new { a = usuario });
        }

        private DataTable obtieneDatos(int opcion, string fechaInicio, string fechaFinal, int donador, int centro)
        {
            DataTable dt = new DataTable();
            using (Microsoft.Data.SqlClient.SqlConnection connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString))
            {
                string sql = @"generaReporteTotales";
                try
                {
                    Microsoft.Data.SqlClient.SqlCommand command = new Microsoft.Data.SqlClient.SqlCommand(sql, connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@opcion", SqlDbType.Int).Value = opcion;
                    connection.Open();

                    if ((fechaInicio == null || fechaInicio == "") && (fechaFinal == null || fechaFinal == "") && donador == 0 && centro == 0)
                    {
                        command.Parameters.Add("@Parametros", SqlDbType.VarChar).Value = "";
                    }
                    else if (!string.IsNullOrEmpty(fechaInicio) && !string.IsNullOrEmpty(fechaFinal) && donador != 0 && centro != 0)
                    {
                        command.Parameters.Add("@Parametros", SqlDbType.VarChar).Value = "where convert(date,fecha,103) between '" + fechaInicio.ToString() + "' and '" + fechaFinal.ToString() + "' and Donadores.donadorID = '" + donador + "' and Registros.CentroID = " + centro;
                    }
                    else if (!string.IsNullOrEmpty(fechaInicio) && !string.IsNullOrEmpty(fechaFinal) && donador != 0)
                    {
                        command.Parameters.Add("@Parametros", SqlDbType.VarChar).Value = "where convert(date,fecha,103) between '" + fechaInicio.ToString() + "' and '" + fechaFinal.ToString() + "' and " + "Donadores.donadorID = " + donador;
                    }
                    else if (!string.IsNullOrEmpty(fechaInicio) && !string.IsNullOrEmpty(fechaFinal) && centro != 0)
                    {
                        command.Parameters.Add("@Parametros", SqlDbType.VarChar).Value = "where convert(date,fecha,103) between '" + fechaInicio.ToString() + "' and '" + fechaFinal.ToString() + "' and Registros.CentroID = " + centro;
                    }
                    else if (donador != 0 && centro != 0)
                    {
                        command.Parameters.Add("@Parametros", SqlDbType.VarChar).Value = "Where Donadores.donadorID = " + donador + "and Registros.CentroID = " + centro;
                    }
                    else if (!string.IsNullOrEmpty(fechaInicio) && !string.IsNullOrEmpty(fechaFinal))
                    {
                        command.Parameters.Add("@Parametros", SqlDbType.VarChar).Value = "where convert(date,fecha,103) between '" + fechaInicio.ToString() + "' and '" + fechaFinal.ToString() + "'";
                    }
                    else if (donador != 0)
                    {
                        command.Parameters.Add("@Parametros", SqlDbType.VarChar).Value = "Where Donadores.donadorID = " + donador;
                    }
                    else if (centro != 0)
                    {
                        command.Parameters.Add("@Parametros", SqlDbType.VarChar).Value = "Where Registros.CentroID = " + centro;
                    }
                    else
                    {
                        command.Parameters.Add("@Parametros", SqlDbType.VarChar).Value = "";
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        dt = new DataTable();
                        dt.Load(reader);
                    }
                    dt.TableName = "Registros";
                    dt.AcceptChanges();


                    return dt;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error: " + ex.Message);
                }
            }
        }
    }
}
