using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.ConditionalFormatting;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using donativos.Models;
using donativos.Extensions;

namespace donativos.Controllers
{
    public class RegistrosController : Controller
    {
        //const string API_URL = "http://172.16.9.10/apiContingencia2024";
        const string API_URL = "http://localhost/apiContingencia";
        //const string API_URL = "http://localhost:1307";


        public JsonResult articulos(string text, string a)
        {
            var response = RestSharpHelper.ExecuteGet(API_URL, a, $"/catalogos/articulos?nombre={text}");
            return Json(response.Content);
        }

        ////2a
        /// 

        public JsonResult donador(string text, string a)
        {
            var response = RestSharpHelper.ExecuteGet(API_URL, a, $"/catalogos/donadores?nombre={text}");
            return Json(response.Content);
        }

        ////3a
        /// 

        public JsonResult unidadMedida(string text, string a)
        {
            var response = RestSharpHelper.ExecuteGet(API_URL, a, $"/catalogos/unidadesMedida?nombre={text}");
            return Json(response.Content);
        }

        ////4a
        /// 

        public JsonResult Registro(string text, string a)
        {
            var response = RestSharpHelper.ExecuteGet(API_URL, a, $"/registros?articulo={text}");
            return Json(response.Content);
        }

        ////5a
        /// 

        public JsonResult actualizarRegistro(int don, int id, string a, int art, int uni, double cant, double lt, double kg, double cantArt, string obs, int tel, string cor, int centro)
        {
            int[] unidadesMedida = new int[1];
            unidadesMedida[0] = uni;

            var donadorBody = new
            {
                donadorId = art,
                nombre = "a",
                telefono = tel,
                correoElectronico = cor
            };
            RestSharpHelper.ExecutePut(API_URL, a, "/catalogos/donadores", donadorBody);

            var articuloBody = new
            {
                articuloId = art,
                nombre = "a",
                unidadesMedida = unidadesMedida
            };
            RestSharpHelper.ExecutePut(API_URL, a, "/catalogos/articulos", articuloBody);

            var registroBody = new
            {
                registroId = id,
                DonadorID = don,
                ArticuloID = art,
                UnidadMedidaID = uni,
                cantidad = cant,
                kiloPorPieza = kg,
                litroPorPieza = lt,
                cantidadPorArticulo = cantArt,
                observacion = obs,
                CentroID = centro
            };
            var response = RestSharpHelper.ExecutePut(API_URL, a, "/registros", registroBody);
            return Json(response.Content);
        }

        ////6
        /// 

        public JsonResult eliminar(int id, string a)
        {
            var response = RestSharpHelper.ExecuteDelete(API_URL, a, $"/registros/{id}");
            return Json(response.Content);
        }


        ////7a
        /// 

        public JsonResult registroE(int id, string a)
        {
            var response = RestSharpHelper.ExecuteGet(API_URL, a, $"/registros/{id}");
            return Json(response.Content);
        }


        ////8a
        /// 

        public JsonResult agregarArticulos(string text, int tipoCategoria, string a)
        {
            int[] unidadesMedida = new int[1];
            unidadesMedida[0] = 1;
            var body = new
            {
                nombre = text,
                unidadesMedida = unidadesMedida,
                tipoCategoria = tipoCategoria
            };
            var response = RestSharpHelper.ExecutePost(API_URL, a, "/catalogos/articulos", body);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(BadRequest());
            }
            else
            {
                return Json(response.Content);
            }
        }

        ////9a
        /// 

        public JsonResult agregarUniMed(string text, string a)
        {
            var body = new
            {
                nombre = text
            };
            var response = RestSharpHelper.ExecutePost(API_URL, a, "/catalogos/unidadesMedida", body);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(BadRequest());
            }
            else
            {
                return Json(response.Content);
            }
        }

        ////10a
        /// 

        public JsonResult agregarDonador(string text, string numero, string correo, int tipoDonador, string a)
        {
            var body = new
            {
                nombre = text,
                telefono = numero,
                correoElectronico = correo,
                tipoDonadorID = tipoDonador
            };
            var response = RestSharpHelper.ExecutePost(API_URL, a, "/catalogos/donadores", body);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return Json(BadRequest());
            }
            else
            {
                return Json(response.Content);
            }
        }

        ////11a 
        /// 

        public JsonResult agregarRegistro(int don, int art, string a, int uni, double cant, double lt, double kg, double cantArt, string obs, long tel, string cor)
        {
            int[] unidadesMedida = new int[1];
            unidadesMedida[0] = uni;

            var donadorBody = new
            {
                donadorId = don,
                nombre = "a",
                telefono = tel.ToString(),
                correoElectronico = cor
            };
            RestSharpHelper.ExecutePut(API_URL, a, "/catalogos/donadores", donadorBody);

            var articuloBody = new
            {
                articuloId = art,
                nombre = "a",
                unidadesMedida = unidadesMedida
            };
            RestSharpHelper.ExecutePut(API_URL, a, "/catalogos/articulos", articuloBody);


            var CentroSession = HttpContext.Session.GetString("CentroID");
            var Centro = CentroSession.Replace("\"", "");
            int Cen = Convert.ToInt32(Centro);

            var registroBody = new
            {
                DonadorID = don,
                ArticuloID = art,
                UnidadMedidaID = uni,
                cantidad = cant,
                kiloPorPieza = kg,
                litroPorPieza = lt,
                cantidadPorArticulo = cantArt,
                observacion = obs,
                CentroID = Cen
            };
            var response = RestSharpHelper.ExecutePost(API_URL, a, "/registros", registroBody);
            return Json(response.Content);


        }

        ////
        /// 12
        public JsonResult buscarArt(string text, string a)
        {
            var response = RestSharpHelper.ExecuteGet(API_URL, a, $"/registros?articulo={text}");
            return Json(response.Content);
        }

        ////
        /// 13
        public JsonResult buscarDon(string text, string a)
        {
            var response = RestSharpHelper.ExecuteGet(API_URL, a, $"/registros?donador={text}");
            return Json(response.Content);
        }
        public JsonResult buscarDonE(int id, string a)
        {
            var response = RestSharpHelper.ExecuteGet(API_URL, a, $"/catalogos/donadores/{id}");
            return Json(response.Content);
        }

        
        public IActionResult Index(string a)
        {
            var sessionUsuario = HttpContext.Session.GetString("usuario");

            ViewBag.centro = HttpContext.Session.GetString("centroAcopio");

            if (sessionUsuario != null)
            {

                string acceso = a;
                string tokenAcceso = "";


                switch (acceso)
                {
                    //Token vigencia 31 de Dic del 2029 - Usuarios oficinas centrales
                    case "admin":
                        tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InVzdWFyaW8wMSIsIkZpbmdlcnByaW50IjoiYmQwYzM4NmJkOWYyZDA2Zjk2ZTYyMmYyZDU5NWU2NjQ4MDU3YjhmMWQ5NTMzZTgzYjc1MzNiYTA5YzQ3NTY2YWU0YTdiMzQ2M2IyYzMwMGZhOTgwNWUwNmI4MTVlODE0NzRhZDFmZmU2NTNhNjRjNTg4MWMxMDkwNWZjMjcwY2MiLCJCb2RlZ2FJRCI6IjEiLCJuYmYiOjE2OTg3ODc3NTksImV4cCI6MTg5MzQ3NzU5OSwiaWF0IjoxNjk4Nzg3NzU5LCJpc3MiOiJESUYtSGlkYWxnbyIsImF1ZCI6IkNlbnRyby1hY29waW8ifQ.kSNrQ6QpgDh1zVTLzOInTfxEXL2Qn1K6DGdwZWt6cVtCDzrA0hPubtzSQQUAIuC6ifU3_ZD_o2Wet2B1rJBOgBVwNAwSmVpZWf1VEqRgdNa_89bqziK4gob36_w8Q3v2lQAZJivggWAUgIftcYZXH24xHKwZ8FarhSYuDtrvex1PdUVOxhAOkjyQLj_tNEYTVCkaXq9YGHDZx776LwctDMORJQ485ia-MD6EHglt4fbScD_m7YN-8FtzASily9UkEr41Q_80nMaOz0nw_fo4uuwrfC7CXnldp7bPc8ZpXIHRk1Ntvn1bMmwyKTIkePC00HnbTqZISqpiOBDJ0ZM_GNJhvxEQ0XlGgs298DqcOE7b3QoWu3SXIP6xlJiOi065H9z1jAi7qjPSPCFRxS2vrlKAz4F9e15HK0KdOHL7gV1peOmsoalOHmHwFme6vZiq95FULGZ8JSlzQLoWxBkDcbjJum6qbXwjBoOLH62thLZTN4O1tY7UpEh9eKfWotu8nBDDFGvvBWqaLbumG58muIhyPZsrVDdiPiUims01QAsV9nHtXiIQ80svPHFwpAD0oKMgd_av-5iEnAzaIOlw39H0NNfLDOneHFckGDVQeiznUR87ppc-_3VGGQOqKo454_OQWLpHs_3qzAv9jxbv-qYRJ_O8T0bLKUT4KO2Wzm8";
                        break;
                    case "usuario1":
                        tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InVzdWFyaW8wMiIsIkZpbmdlcnByaW50IjoiNmRhYWI0YjdmYmMyYWRlY2FmZjc1ZWI5MjQyYTA2OWU5YmMyYmJjNzk3MDA4MzI5MDhjNTA1MzRlY2MzYzc0YTAxNjE3OTJiNDkyNDNiM2I4MGUyZTUwZDAyODkzYjExYjg5NWZlZDI2MWYwMGRlOGZkZTNhZGU1NmFlNGU2ZWMiLCJCb2RlZ2FJRCI6IjEiLCJuYmYiOjE2OTg3ODc3NzIsImV4cCI6MTg5MzQ3NzU5OSwiaWF0IjoxNjk4Nzg3NzcyLCJpc3MiOiJESUYtSGlkYWxnbyIsImF1ZCI6IkNlbnRyby1hY29waW8ifQ.ULA_Y_NiT5Eb6tVlLBYPl8uStLBAa0T7HYq5KG6FxXkIC3EuvyjSPKI80bQ_4skdJ3SMR6sZVyEfMf-hjPVrcOD4H5yCHNvgFHgIetWkrktGxm8Awbo7Zu3PQ1mGYrYiv0tT5-apqFzjqNGzxKrW6CvlRpp6z1QFEdW9tT4hBgbJ_T3Y-LNPtwludPFXRel3_9DXM1zZx89KYujVm2xinRXrWYGvutGhAYfILnLKh97uOeVK2mpy0cERr9Q6hulj0ZEKD2D_w3bTBCM0byuo6E3i0iLXPgToui2lao4GlMh9qtvva8Bx1PregsWXnQc3ys1EPSJDPLa6YYi2ptxSna4GKDEp0XQU9ym-q2eLEd9mldHv-BvWdQISK1srB9CqwLNd2rXn0GHt96pOhOem5kfamCNDLHfPS4tx_iOBi2mGpb_pjEQkiBF5aXSdAITjG_KxAUJVjuOkUrpp9hg_Ut_ReoOi6p-iX1-ElcWk1e4isg_wfJD1jMognAf4q-dKpqbGvJ54Ipf5hbA2AmrMm9r0-pWjbOK3Czw_fjbM6seDg8CgJZ4J3Vwwo4PoG1MGsNXL7DprRDgjtcsF9hfxJj37xKEsUmLQYubOVPICPDuw1svFxLFbWZ-rlfD4I7aR5MmPnHvLccbi2ocRn2f07xU7RXjwXriCW-N0TRJw8-E";
                        break;

                    case "usuario2":
                        tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InVzdWFyaW8wMyIsIkZpbmdlcnByaW50IjoiODQwMTAwN2NjZmUzMzk2N2MxMjYyYjNhOGI2ZGFjOGQ1NTdmNjg0Nzk3OGIxN2NhNjQ1MDhkMjQwMDk5NGRlMTk1NWRjZjA4NzAyNjRmMzFmZmFjYjdkYzQ5NGE3YmM1OWNkZTVhN2YzZjk1ZTU0ZGNkNzYzNzk5ZTk4YzVkZjUiLCJCb2RlZ2FJRCI6IjEiLCJuYmYiOjE2OTg3ODc3OTQsImV4cCI6MTg5MzQ3NzU5OSwiaWF0IjoxNjk4Nzg3Nzk0LCJpc3MiOiJESUYtSGlkYWxnbyIsImF1ZCI6IkNlbnRyby1hY29waW8ifQ.ElEAhitSAYrRV_eeckKO0UiOp9L7uTWF6V8tFZVH0GByOkddSkIooExssTriY5XnJ-nvxC27fdJxb3asNFcfSS-BQj8l2JPdwnkXMYIZltnpGzxu4mC1NChpEBdZ86TquaG-Qpodj_imKPeUHQE4MDP6RVZFd3qXLVxQy123P_zXwEU0OQGk86qcSJ_hONaF47arwBhj7gqdi5DFiaolKjqljQHwgFUZ1dfy2f4ye_frxiyOSvotKpEYCvl0Nhr8qT48HEouDFpniNr-19xkyuB4U_RgKiI7HHb-lz5new5skd7ZG8V_ly-Czu9wgpKru1GcaLYWawezIOdxTxd8sslyIlIfElGhLVfX59GvGdWgvxY6fWtjs4wzlyoa73nsAeXEK8vQxuMWeB7nGO6bV92bFs1ZubDmcaH57H-JuXdDUDTYh4E1h7xn8DfARmQ8n7EqrnARNI_SNog6me4gwYi-ZdlMUJaYhBbYKrvRcGFoXO3dqOz_L-cZ_m6SYT2IbzmeMPMSGkD7azGNgyGMTzo1-1DS7Dhtb6TMdgcWYTN81TE-Vx_Us88WPhBhSUwwVifXzY6j6fxru1cqfMjlRRn3xlXqK2_-S4SaLXCRvX94jhNyXFiMVp3cUTPFmnUJdAkDosk1MRE_JQe_S_v85CEljGOt0LsKBzdFN1TkVX4";
                        break;

                    case "bodega1":
                        tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InVzdWFyaW8wNCIsIkZpbmdlcnByaW50IjoiYThiYWUyZjJkNzNkYzIzMTViYjIxZGNkNTBjMmY1MjE3YzExNWQ0ZTEyYmQ5MTM0ODFjMzM3NDk0MDBjODAwYmUyNWIxNDNmYzk0M2MxNmUwYjUzNzE5NjEwMGIzNGUwNGQ3MzAzOGUyYWI1MGJkN2RhMTNjN2M1MWU2MzA0OTUiLCJCb2RlZ2FJRCI6IjEiLCJuYmYiOjE2OTg3ODc4MDcsImV4cCI6MTg5MzQ3NzU5OSwiaWF0IjoxNjk4Nzg3ODA3LCJpc3MiOiJESUYtSGlkYWxnbyIsImF1ZCI6IkNlbnRyby1hY29waW8ifQ.HjGOA1zNkgs1OWKYPxAo5sBrxFsfo2W07ugRHrCJ2FwnZBK6jdnc6c8ZGn7TQL_s39Mbqz2RjjKvBydm4fGIFsEohrLCTU1OMhZ50fYjKBAOd8tvmZ6ZGFzxCdpZbc826QUrkreO-ZP65Pdx3L87Cl8yd9EC62zSEwlDzgMj00gQwAXCxEmbStdT2VRdcHoXk82SWIjDe6GiNMY_mf50_gkrgPwcutJpVSyxDJm0jFo4biPkRkdh_Jv9xOulWNpKxbh6RAw2gHrBdlSiFStnJGI3f3ZIACfVaxc1GBFClbN2gAFGOtU5xKNe2uEC-J4bLNgK1OO-FcKEAtoRzAaB3UuQGd4dmSNwKcb1phnJduYAy7zCvs83VFo4Qn4ERoRpz6zEJdx77adEcPfi5hZ8T_tqt7UZpNgFRXxE7_jbQ0Y2VofmvLaP58-W8QgzcuwJFgPvpRL9p3jbUnqsLYSgDX2g8bpAuTo0Y6oEvidhuTDGgzjFMciRGm5CqqO6G-qsvqcaikKi37HEOjvIDjl_o7TqFti28k8Vm9reV3U9qz6myqgdBPksrX5qbVUYqFTlZyFCdzFP9mQoxJbHd13rnIr85-ceeayRJE1OvCD-ZN0wjYXviU6aH83N5NUb-rkcmgNFUaIaBEbvRJlWeYcfsLBAwLWPEHJTJOA3Aw8AhQo";
                        break;

                    case "bodega2":
                        tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InVzdWFyaW8wNSIsIkZpbmdlcnByaW50IjoiZjlkYmIyMGVlMzA0NTMyZDcwODQxYmExYmRlOGY3ZjhlN2M2ZWUwYzljNDc0ZjVkMDYwZmFhMjY3ODg5ODI2YzUxNzAzN2Q5ODRiY2ViMDQ3MWYyNWU0ZTdlYWM0ZWRlNDAxZmIyNTFjMGMyY2RlYjNjYWYwNDVlZDVmMDBkMWEiLCJCb2RlZ2FJRCI6IjEiLCJuYmYiOjE2OTg3ODc4MjEsImV4cCI6MTg5MzQ3NzU5OSwiaWF0IjoxNjk4Nzg3ODIxLCJpc3MiOiJESUYtSGlkYWxnbyIsImF1ZCI6IkNlbnRyby1hY29waW8ifQ.CGwJmV__e4GHoUo89Gk7zeuZB8b-6mDBxEpPQLP1r0WwekUGqn0Ue-KaEds8cWwrp8z_2KUX-3zhZies4llXrDZmwobZxu1Ve_rYWMljrgSjtrNB_9OBdY4uP690qB37v499DEW6n3KYIKFpW9VjcVWyAhkX-JInYhJuu84mCP-i7sqbipQ-ySaq9M5F_dMf_UCpWe6ZJNcwfTYWSHFw15m5o6b3OxnA96uZOvh4OpFRx3LOzGWDu8VJsbTzPhWGnU0mq2BVkwfBxS_FrsiwneqjmAJkfyzJrmv7CgdOQD9x5uwiXXV-MI_KTkCuAbAgcH4GR9YarkCLQKgqa93BM7Ct5E9lgRr4flmMO_ea-RXbKylrKxOoGy5KETqnjuI46jcoKpUl8YG-KI--uaPjWKHqN5-KOptIfU_ZNnkKNEFlbZxhef5DRoHblwfsLJ3x3PWL4ePZk4TGrc6HA27CTmu1Hl9xNhgrulovAekhz4Kg-XIMPN3MRy6BzZGS7Sxo8_0z4wiIeL3_bZZX6Zd008BRQJ0NMZmgvCDiuSqUxydybLvkbpvAitBJ3FGPQQsOzJ-FxisdN6JJL6dAVtnUPzGcMqhb-Ux5Y0XlmVECi3Njuj4mp2MkZmoHTneb_tntCDDG4BWoYcYJCpl_e-pTUxItmPkjaz3MAYGd_7oEGwQ";
                        break;

                    case "erick":
                        tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InVzdWFyaW8wNiIsIkZpbmdlcnByaW50IjoiMDNlZGYwNDAzYTM3NTNmZmQxYjFmN2UyMjcxODViOWVkMWY1N2E0ZDEyOWE4YWI2ZjJkZjFkYWEwYWUyYTJlMDNkZGRiMjg0NDIwMDU5NDg3YWZiNGQ0ZTRkYjk1NmU0NDYwZDJlOWQ1YTUxMzljY2NkZDUxY2EyNTU4ODNkYmIiLCJCb2RlZ2FJRCI6IjEiLCJuYmYiOjE2OTg3ODc4MzMsImV4cCI6MTg5MzQ3NzU5OSwiaWF0IjoxNjk4Nzg3ODMzLCJpc3MiOiJESUYtSGlkYWxnbyIsImF1ZCI6IkNlbnRyby1hY29waW8ifQ.mQVMZW9tLEeDyF8U3kdU2mxRjaajLiy4uNbn4CQMFxkFimyo4ehoeRauuCf2eAZyjukUrQF6fO0Bvw5EoiQKG07nP9eo-mJeF9Zwm-aEDZ0uNZ6n88YxpTXFRE03m9SWt2Rh_pfKnTDu8cgiUGgi-WG7ujPDcPgJpbUVNhC5A6fDF42FUSCaPE4LG_b7oyItBD0nyu2u0ZAQ76juBltPq2vdHjUrLHFS13UJk9f6Qyg-62DnCubx0xrojtT-q1zHcTIuBsT2qtfeeJTuwIXe4CYVP7zuivXTFf-KpuM5IguL0n80ZfQPTtdW3n_1lQsyf8yElPTsVoZxiOxIFsbUdoFM7RcTe3alJNp0B3d_VNz_ELoHM8nGKrJ9MfQy5wXsw0QKa9mb1O0jEqwJ-AwSQ5r30Elss1lgC8XkhnYcsXTBY87-Te9065JO_7N8M0dtOoRjWaZDlWkWubO7y9ith97wuVIMTDUFuwSJ4rlbd4h01OEe7EOhmvvR8oOjzKvhPBP2paQku6JXzjDMImBHpapDGi3BS4lJXfBl-SqaLNjKhR0hxlEiXBPMrnv97EKKK8J53INPaeYZx4GFRDwlj7w_zQxm3emRMiqua7oYcqaEp4QI9WUw3oT2i45Z9YdUJjulXUEqL7_UmanPov5pe6sCJIqkB3AMoh8h5L4_rMg";
                        break;

                    case "prueba":
                        tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InVzdWFyaW8wNyIsIkZpbmdlcnByaW50IjoiZmNjYWFkNTRhZTZlYjkzZmIzYTU5MGQyMDYyMTI1NmExNGMwZGJjMDM3YTAzNmQ4NGFjMzg1ZmE5Nzg4YTcwNmU3ZmE4MTM0NDgyOTNmYmFiNGVkYzE2YzBmODFlNGZiNTIxMTUzZmY3MjAwNGI3NjViZDVkZTU3M2Y3ZGY3MTEiLCJCb2RlZ2FJRCI6IjEiLCJuYmYiOjE2OTg3ODc4NDUsImV4cCI6MTg5MzQ3NzU5OSwiaWF0IjoxNjk4Nzg3ODQ1LCJpc3MiOiJESUYtSGlkYWxnbyIsImF1ZCI6IkNlbnRyby1hY29waW8ifQ.KqdE9tJFc36ywy3UH-d5BaZtRLy5BpAygtvgTGFXJeSTnWbW3XmqIcl8hMmY3-70VI8fpVxZLMPK-64SgL5vMLs2pwpooH2DUiZ3W9vrWzsi3ow3851W0qL6bKkRULw8AfhpZBKbJPtzqefVTNc8mfU_l4nMCt2dGcBGJ2Tjhgvx8Wk8zq_jnHfEN1AbrO_B4FTllIM1rntu01wV3mbEcmV0PsBBV4hrRsrGxtxfCeB1825CPtADjX1eQAjHM1PtENMayB3tZwws3K06PoQpIOdPU34Gl2LB4R1ZEHRQ1iR1_bJNe5OavsqWpyy9W-QqTnHUJnUVB-yqXW4IivVQLjnDcYywaL0tnHmfzdF5YGJCDGGlppH2o5Lg2Goq1Rvof7d6S11xyuIW9DEJO_wtEK9aIMNo5c0z5vj1kLPismjlMaJYHQwVnvAixJOk4pDSbDeB_N-fFsJh2cZi4Rq8tMidZh1cXuRzfodVafIKLaNHu2V-qRsM3gn-ondch2v7_TccY7WSkTj4aTSIDV1AtiDIGucYPAycFU1tgd_u2lRXomX0sgsp7Zp_8WXKF6-LFodOdc31ASbZDDpOQG_Z62_z8tHldxNQMjkFoIN1fVsMU2GrI77EgGJMaZZvaFWqgfVqutcUtgR2wczQDicYXm8zWEMp39croFRFSMA8Pqk";
                        break;

                    case "usuario08":
                        tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InVzdWFyaW8wOCIsIkZpbmdlcnByaW50IjoiNDRjMmFiZTMwOTljODJhZWU2MzcxY2UxOTc0M2MyNTFjMDMwMzIzNDlkZWFjOGYwMDU4OGZkM2Q0ODJkZDY5NWIxZWUwZTkxZmUzYmFjY2VhN2E3NjkyYWU0NDAzNGU5ZTkyZTU5OTk1NGNiODZiM2M3MGYzODFlMDRmMjdhYmMiLCJCb2RlZ2FJRCI6IjEiLCJuYmYiOjE2OTg3ODc4NTgsImV4cCI6MTg5MzQ3NzU5OSwiaWF0IjoxNjk4Nzg3ODU4LCJpc3MiOiJESUYtSGlkYWxnbyIsImF1ZCI6IkNlbnRyby1hY29waW8ifQ.hv8v-5MEjLk_RK_4sPAy1ClwEqqrWnj9SZDAofbEDbNMFO9PqNYG2o5D6ZWGIX1BPxTmKDqaKPxShj-hwc_kXO07LNgBmtIezAT5jsl9ox4GVRRWLqe5ZR149tqJ9r3bLnvTkAK98uA590KEl2sWlKswnjdHEbkhr_33jqdHd6FBPg-idRhqXohQA-otYREDKGX-JmdRSvW0MbmwVDeuMdUaoGYl3--0Sj7dnGy18UyWRm6dFJqy_NZDUSYXU8aPluxEwcgEEJdb89OM5ogZlSxFBUJl6WgGbDAwESx4qxaE5hit7NJZ_n1zJ9283qGBBS681Y9wZubpKPRjiIkmLZvqmkwXeyTjNJP6xkzMLgwV5pcnxQW2OSbyJzvGyHdTpUiMPTTl4DxH1jasA1h_yJyli-XTzd8YI4iJLZeXDhDsBQjGX4CLDL7Gjtft__O-j-TL6zug5wO34VlopUW6BCbyMf-pJJdLbaRx3vFo96qTE136bZxUcuu8_LxsodohK-WuFlUAt2XRid6nfvGSBZiOmzQXwr1cPDdUvgL8zB6Xx81h91HHhYZmP8WZVmU_zUG7Ohhxn3WlKeL0zn4GKgKNEsQwObBuB5gZx6iQwbk1rvoTMewcmZ9c5bi0kzZD6Xt6MNasJT3n45xXruwyGePBHWr6Jecr8o09la589do";
                        break;

                    case "usuario09":
                        tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InVzdWFyaW8wOSIsIkZpbmdlcnByaW50IjoiMWMyNWI3YTNmNzY0ZjNjNmFhMzE1ZjRiOTZiODhiZTkwN2QwZDAxNGE5ZTkzZjlkNjI1ZThmYjc1ZTgyMDUxY2U5ZDBlZjhiNTZmNDYxOWFjNTcyZjY3Mzk5M2IxMmI1NDFmMDM3NDg0YzdjZmY3YjViOTllYjAxMTYwNmQ3Y2EiLCJCb2RlZ2FJRCI6IjEiLCJuYmYiOjE2OTg3ODc4NzMsImV4cCI6MTg5MzQ3NzU5OSwiaWF0IjoxNjk4Nzg3ODczLCJpc3MiOiJESUYtSGlkYWxnbyIsImF1ZCI6IkNlbnRyby1hY29waW8ifQ.WJ3Z7B-EVUhJQuPA7VG97AXKuWDHdUfyWwiQFEuYW0ySAgn3MUPlkaumovJ3F2cnj-FuaOK4gXJv7KpshXexMk0tEx7pakK8EHwAPcUqdBm663p_8pfwUS846gICpQJSR6pYzDg2FJ9vr8mdVUIsg0prkje9zhP02yj25xRbLfwHUV6GSxvIlRjQH3Gw6foCC9SBAqGLet65Gbm-z8wxxYrJims_MxKXyhOLC6Jncw1vLWb9b1jtfJKbuT-Kqn9Xgzt15zFDssZMTbP_IkYr4ZRAreRsHrsD7JS29oMVqyqhKF-OBnpdHPD07p5gz_dePlzC0j2e5CPd_nVrFkjlyGqpYbjURLbNRoHuuaPfMow1hi1T8PUJ-y0jxiJS-AdDVrpsQq477RCih4sKi-PbwBBVlFet8jbloYaSnPPUPkKiXgYDA2NrbokT9g8rYC2jZKQdOUV_S4To0DdgG6qv3AXFHxViiY3KbWZ-hsewPAIUHrMCILuH1RH77n8ijhL1rAgvEJwQzXCsMT7JiD5-w6UmsKSAYEtZgFMH688ZcyAoh3ewo79tQlK9oOs0cWeNTGIekgTuOSW_aUidyzXIJexo11m1mgwYN3JKuCJnmXiM2I33Vi7fipkvAUhgHM7M1bo3TEhqcWDWNRxKTGWRRjU7moft7C8uurjpqCVm0iY";
                        break;
                    case "usuario10":
                        tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InVzdWFyaW8xMCIsIkZpbmdlcnByaW50IjoiZTIxNjYzNjM3NzgyZWNjZmNkZWQ2NDVmZjRjYTcxZTkxOGQ5ZjYyYWY0NjIwZDY3NmQ1NTg3NjU1OTI0ODA3ZThiNzA1MzcwMDZlMzJjM2ExMTJiNWIxODU1ODk1ZjNmNDdmMmQwNTY1ZTYxMjNlYzhlNmYzYTRkZDk4NWQ3ZjEiLCJCb2RlZ2FJRCI6IjEiLCJuYmYiOjE2OTg3ODc4ODUsImV4cCI6MTg5MzQ3NzU5OSwiaWF0IjoxNjk4Nzg3ODg1LCJpc3MiOiJESUYtSGlkYWxnbyIsImF1ZCI6IkNlbnRyby1hY29waW8ifQ.jhNSdG8yajXxwDoFh3hxugGjXmpxNAJMGXhvJgBmuDWMr4e9xUyQVG12PYwEXUPf2wEB_DXRJe0tlzKYRx-A_slqcJjuGUeIKG4eqAOkiNpn4-iWcmm7WDh536U4em3iJwV1GG2Q0VLnMf8RhDotem4hBZi9_Owamtx08BSTGYtrIPPCd694XVDfkMDErm9c1_cnAXuvX2QWlAQXbYnvQgAqyc9UDH9LMszMNBXazTwVz1HFzoV3m38ncVbzJDCcppC3wq2We_uwJvBWl8GFLcVicM_YwjtxKIvflMBdoK-6owhcmSDOANpBPN1wwh54KgPfJyq7dcZMxO1l-4fkomYT_PEx3yLdc8C4RkQs3_9SjCproAyPTPED_1Z4sU0aQXV5fsSRsL60PkvdNqQj-QhcIwS471tY6Vp72WoQOz1N6PPsktKzsL-PTqrObzqHYFcNMfGCw12A1lexKBOpEZ8YSqDTrRRF7cNmrOLc6a8n_71DLmBSRatV_yRd3h_qUwgui6MpyLnMwFTtOARXLLj0lofkWfmW5ReyMxFk0fKJuRu-0Txbk1A65zj5UdkS7B9RXWn7MdKGUSKtH1QwQSyoekNrOSpQEYbIK9NbOGwP0zvhl_yMY_8YiNMJY7kToB1DQDCK8X0LcWGlYoLLOecD3-M_8Qt7VZXg_8XgO8g";
                        break;

                    //PACHUCA  31 de Dic 2021
                    //        case "d01qemz":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvMDEiLCJGaW5nZXJwcmludCI6IjIxMTNhNzY4M2VmM2Y3MDMxOGUwYzMwMmMxYmRiMzU4ZjIyOTA6ZmQ1NGY2YjBhMTgyNzQ0NGIzYWVmMDI0MTdjZTA5Y2U0ODRjMGJiY2ZjMWE2ZWM2ZjY3NWE4NTljMmEyOTYwMGUzZmNhNTlkM2Q1OWJkOTM4ZjQ3Y2U4OWY5NGY1ZTA5IiwiQm9kZWdhSUQiOiIyIiwibmJmIjoxNjMxNTYxNzY4LCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU2MTc2OCwiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.WEW0r1WP-HkFymK1AoyIBstKw-yluqKXf_5BYkmyPbFnXGO6wqiTT9KZOQ5fykuon5xLR68fF5zwdBN7piVd4wPwxk141C1RO1L6tO17rxQOrNkYyPlF_UEbtCkv7y4zA81Xn03ozRXHBJJUvT8al_DzRNnkyqGmyTg2rZIir3nbH_jzt4MUb4Z_NXvwghUmXntqESEAA_Nfyre2aNPixU2QmwMm2bhHIqStMKeH-QoUBW91Rhl3clPEt5Kyo_pQ06-WRgTMtsTRNNJFI8MhKG3z088YT18HT9aR-BCLZY8XrEvAV5uUQ5MH2UG2oWIXNSihHyCV8dHI9wbBP67k6rT88pWn2v6TUnZAmqNlgb-9TWZj9UmFYsZpDXqHnIHBV8fMFONGrExzuO8OJwYu8pZcpcKAVgPeuAOxqvwPF408SuMPe4d0o6NzUA6I28mH0Cz_rD2Fi9LJJkIG0gP6pfHNHG7qhWrnHdB-dKTbR6lAA0qUD59O9K2aKSJRwCss7ihwhDMLbdv7zJtIV--iTSXFJnsfQLI-hR0mB7_V28_vT-e67DBQh81w5tHM_PRUW0SBLL8NpW6xRFZaq3H3IqBWYjiJfaLA2MnN_rrGlY8hIzA0yY_QDE8dFKf3pDxrQ7xlNyvp2nySD_kbxFHTgHCrI2YL0MO_qk5C6KgsAEE";
                    //break;
                    //        case "d02zdvg":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvMDIiLCJGaW5nZXJwcmludCI6IjEyOWQzZDNkMmFjOTNiZGQ5NTAwNjY3YmNhNTA4MWRmMDYzNzg2MWZjZjM1ZDhkYmM4ZDljOGI2MjI4YzA1NzU2YjRkMzg0NTM2MWQ3MTZkYWM3M2UyYWNjYjhmYWQ3ZTYzZGY3MjViYTA4YjllYWIzN2Q0YTA0YTYyZmE3YTc2IiwiQm9kZWdhSUQiOiIyIiwibmJmIjoxNjMxNTYxODQ3LCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU2MTg0NywiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.mzzdXFFaEjSlxgtafJxYtUPaDzBxZoPkpr2T2vyZ62M8tyKrsbBC6zPwje1-y3L1qhhckSpBJkpqQE9pYQpdBD01vm6PkCWr4WRZ9MA8oTVEX3lA4g5pBcZOOCBejtQsSVCbZ1Z0LiF0e-uXt1vRpVAt9vnloQvxSv4JhKmTbZ5btwDA5b2ULmY0WycF9blNHbP8lMu15fIB_QG8HdpdysgfgFPy_2sL0kMuvrUoz1Sqthytiy54grMpuMRKQIi4ey_wg-9GYTAEG7Gl75jkM4YQswuI1t7tb9BpcoKdwL7QkMBtvnfvvvz7r-NoZiK7b_d3vvfbDKHAYddayZ45AH1_9ADteoXvSozoppA8-1ieLY5yM4iO3xWpV11K21nqAL4KleoJameEJ-FTzv_1bd8mrM7fNCTrQAIzS_wixArXbIkHCRUyzBN0is0-A1nYSkHE1-zIlENUKm5fg4-zCZYAZsZ3WDSDwjRF8JwmNjD4X-5DoSj504ZS8rbQhwoNYidAXBkMhXDidl-6IJVIWe9iihG8SyWyDw_OPKL-4hSUC5cmgWNnoKn2mL9f9EDQaoBnqQVIPtoMTByhm0yfiHnilEo5WbuIWy6F7malo4Iz_cJOe8DD9wTofcTh40n_BX_Y-jFrH3uZTM-pj1H3TbDn2M7suycmZmcw2VpxOpQ";
                    //            break;

                    //        case "d03rwqi":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvMDMiLCJGaW5nZXJwcmludCI6IjQzMWY1YTNkOWJkY2UwNGQ1YjJkNGM2OTFiOTIxZWViNjMyZjA2MDk0ZjkwYThiMTc0YzJjZTM0ZWY1ODNjZjQwYmYwMWZmZGMzOGY4NDYxYzhmMmFjZGIxODE2NDgwNWM4Mjc3Nzk1ZTk2ZWNiM2FlMmUxN2M1OTc0ZjA4ZmMxIiwiQm9kZWdhSUQiOiIyIiwibmJmIjoxNjMxNTYxODkzLCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU2MTg5MywiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.Sfkcl7BLo9kzNqo39vpgoZZY_xAGn27J4HVYgg3_vDAJXVCX44omaqhhltVR_emFNLW3HxIXIYoWIIgbyqJ_dVyU2h6c6mC56sH_kp3lBz7SQJwpJXVu2wYXiZxcB0yHoggYO4gusDUnIFmLxFZGZ11sgfHAnT-lryVCQ6Qu4XIQ979RiiK8GAWutKCT0JbSqS0N4kFkp6IQt3t_NEz7i08HDTkvitE39PmJrknD6Tu18iPa7hWQQtKRswYeHNlPtAmk-VcijmxMeZxcgZ4rlm8TleQSm9w2_y7sIsYztb5PgV9RLNsUGUXDnacUXGs2dsNZLPBRmHmYZVwe6lmPUlLQrXCaH8F8P4X18_vRcNOsS0lg5jy89TJgx7bMVugomp10z8jNDQ_FzR3kUUUP6z3iCZLJMzbX7q1CZ6YuHCGDZgssfsK0iUPumMPXEPKTENLldP8kGj0xuY1XzB--cSDGkT5r4jI9QgJgEaJCj-3oc7sZPTlftAqUeBEXfItSwxP6le_wtVaczWia2u8OXVRZadcylzyx68ILxZWZGcoeoVHhQT1KlCP9AORgQASbll16PoNkuhXLKdqN83kfAWl_wAXgrYIq1vfacsyNTXcsMVmj-UoEG8NeMo22J4r8XAECgrTb6DhDwTbJfyYIuVYtmTJimcrUSWz2zpS7auo";
                    //            break;

                    //        case "d04yuni":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvMDQiLCJGaW5nZXJwcmludCI6IjU2NzhiNzljZDU1YzE3NjU4YTZlM2NmYmEyYTNiMTU0NGJmNDk0ZGYxYzk0Y2IzNjUyNTIwNThkNjIzZTA5ZDNlMWNjOWJkMGUxNWQ3Yjk1ODkwNTAwNmRiNzY1MTAxYzU4ODIxOTYwMzJjMzM1MWE2MjkwYWYyNmUwMTU1YzI4IiwiQm9kZWdhSUQiOiIyIiwibmJmIjoxNjMxNTYxOTMxLCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU2MTkzMSwiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.b2ZnXvFYBTocndp3BbKEJhj8cpF0CvDXHWjVY-P5ZymC1U1riExIDGIjsFQClJ7ScXNOvTG43ya5-Aj4loL1vx6sldLiPfb-vm_sfHrU5cXD9b9oI0Cz8TqkCqbdf1JxgHVCcm4mJ71J9U6K36anwE3XrUhvBwYdw-XQYXTh_7LaJRonTKxFXjmoTO_9F1hJ7AC1ip_k_ZKPvdy89haNF8ujQJq4SrRafroyZxzh5iwKaCF7JD4JTj2CPobqIaZilDjqOBcztTWirOcOr2FFANhIgN5-_ab8kc6JWjm9ctVK_RJB1voEIIYTTvFOzXhIg9jxpua7ayBaZyR_Vd9MufKl0F1lZ7RO1vCO0dDEx_oDZLgpdRVL5EBu8GlMtyUeutC-5QZyL9lu_ZAm_ONCuGDVkohjJNslmLugkARYj-ESBcrr4AzOYAxrhhLn9bCG7FUcaznZ3tGKWYLSY_LJk8gKa3XXLHOMenbxqTkI-dZd5yuPt8cZ9SkdmTwdFKsjI7F6-Im8FlxN0T7fjRyNrxdiVacd_15my8wt7OYLU7X2LFC2uuYHxwA_di-MX8syEj-6UqLuYQCsatblQ1f7ej5sS1Wyj0fdLf25OqtqL1XoA8m1UCuajumnMxqUFn6dSYs-a-nn3jROxhO1YuVUYklp8WNRWGEaCO7UfC88w4";
                    //            break;

                    //        case "d05amgz":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvMDUiLCJGaW5nZXJwcmludCI6IjFhYWVhNmNkY2NkNDM4MzljYzM3YjBmMDk0YmY5MTUyYzdlYjJjYWQyYzRlNjlhY2M5MDBlNWQwMzc0Y2QzODNmMzY4OTFkNTQyMTBiNTk5Njg3ZTk2NzM3Y2U0NmNiMTJiYWNiYmJhZWVjYzMzNGEwYTI2NDg2NjhmYTgyOWQyIiwiQm9kZWdhSUQiOiIyIiwibmJmIjoxNjMxNTYxOTYzLCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU2MTk2MywiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.VB8_1zSDZGmPZEhg1ngvdtk6jNiY81F8lDil7htHXd1DRqZW_AJtjzKbEL5Q_8EvEZKFwFKs1HasTHypkCezrFT3YeHolDVB4RvWigMzxOoPyIqaZo5ZvGdEqcxVzRAQzyhEnitPbehOXGGF7SyAHGa4CRgVV983qxo5n7n_47yIrqAa8csDZ0hhxvlCaLHgoieshFIUoT5rILgPFRtzg1e6ENbi2Ywf0UHRyhU29o051MXFzSnbS2GBaowCm7Iq75UjYOZKfU3WuauwGye0pjEcKGiCoE30NeRtPIqzwgeva5zuZQ3-zcpx_GlvZPQj2SgnuGrhkj1pUwc7sWCkWF9VzA7f6mbDEIy73K23wLWj7ILuMjVUQnRCALIpgxPVldApkxHgp7xESr4F-TX4gHu9l-AUol-Kbu2Fx8gNlZl1e-MoCy_SO9t0jVpe7l1v6yZNYYywxuVK0f0coEVV8tGBNW2oSfoP4O0m6-Yk9Hpp4hwL59akKtWlen-gDO7GoSnggAv0RCygu9B3_watJ2EiCwYPIvChQvBtqWJ1uWlzY29R8FxWAVzP8AqI_r1-oWtXuYDRVVbtEZOzALzfSV6vs9uE02uMGIXi-HtyyyFGdNw_SD42FkxZp4MVwptvP-w8hKMUZzInNP9OTLMzC9eX_Zm2LZQtwOU-8QHpyxs";
                    //            break;

                    //        case "d06eycg":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvMDYiLCJGaW5nZXJwcmludCI6ImMyNDBjMzM3OTZjOTA5MjBiOGNjMDFiNjJmZGFlNWRkMmE1NjM1MmViMTQyMWJmNjYwMjgyYmRmODQ3YTBiNzY1NzAxMzI4Nzg2NDFjYjI4MjU0NzEwYmE2NDdjNjIxMTg4ZTUzM2I4YWQwMDJjNjQwMjY1NGZlMmFkMTM4Y2RjIiwiQm9kZWdhSUQiOiIyIiwibmJmIjoxNjMxNTYyMDA0LCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU2MjAwNCwiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.uH-lctxg7IT-33Cmff8DnCzcqvqSJI1mpeUEDe1MEmCgZZFF92kqXZVzqZMEQxZK2s2n8R75JYqDg04BzvsI6ENiMsNA-wXqOB_pP-m_zBJLvSDmvRFcJY5uY63nUqyWAvLgXcXh-cO4CFDj-rXjJv4F0Qd9Nl24UtyHncQOyx4_pi3sj3BKj1Gvxl9IL3fg5LqHFAI1tzJ44Z3C2s3gglVZB09-tuiSULmrCdEkvoZ_zX8e4P7UMkwCoT110BfqX2FiHlv5XSR1ZCGlIbH2LWeEBplkLAUoV7A5_dAVwtJUTMjRvT3426tx2wVtM9iUZzElbhJqAHPWiUtvC09WuLgR7tUg3CggP3GRPwRPkxno8C-vrLPCitBFkhsEUs6A1UrX9TfU4hoYFrQsxM_izIt2bVdqHtaYH7-KQ3Lx2gDOsmstpS4iXBXsxW0PYdU7S3Q0wy-CWcI4v_6KSrzPcJSiJQMHtZbf2gbPUTceG546BMTaawW56j-qQviRMfC1SpPzdADfq-UN9jlSk11R2xYgx_Tabgl9s_4qMfUG01rjHwK6kEMNH9cHEhuhSdYKbH-fH8g9GA1-6CKa3aoV27wZ_NmapbEPldy7iQeAvwFlWadoLDmqHx2fAjDeykVWajJK_yAft85EccMtrRwL_-ox3eqeP2lFaaqIKFGQHJY";
                    //            break;

                    //        case "d07xbui":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvMDciLCJGaW5nZXJwcmludCI6IjNhYzMzMGRkNDgyZWI5YjkzYzU0OTZmZDg2OTUwZjJjNjViMzcwNmZkNjIyZDEyYTU1ODI2MDk0YTA4ZDI0MzUzOGNiOGUzNTlmNmU3NzdjNjEwYWMxNzBlODhlZDk0M2U3MDUyMTc0Y2I1OTA4YTczMWY4MzJkNzAzNzMwZmI0IiwiQm9kZWdhSUQiOiIyIiwibmJmIjoxNjMxNTYyMDM0LCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU2MjAzNCwiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.DzHucweHDXrBALqZPW0SBD7IG9yy2TgOo-TWtBPKH1pBxznhBk0y0ZS3RgkZcwzpzCV8ZtWhhP1AAvFbyfiz0VDOVf4tmZD8c9anK5zK2i4RzAX6RlwluvqKBeqKH7W0xUNCBXt7gkjrkHnEgA5ai0HWEsLU2A6J1j0QD56kk0E-LS4LBu3oK7Nna0-CVQFmXDnvTENAu-GzPWy2ltfqjzXeSL3x7FmD9RJNgItw4itkUtf1aFAdHhBnb37ZzJ3MCQgaaiduyEFRAGeaZc3oqh2VB_fsnSy2L2yP4CF6HES-b6hJcTsFisiz-yOEdNY3kE-BupXS36KHJTfUU6dLScqYCgXpDNLmFqKOxiWoYo167z4Av87jNpFmLEo6pBBrlc82Z-FRBcCGWxINB5UTxcL16C84OywpSL6J3gnKyyzeaNnZA16uWHhDeG-QHhmp3jGvYWiTvrKJhn7lFFpm1N-sxNE9rpruECAW-P0oGBkCftayL8BHB3fB_b7g6nymSbrc0VlX9X5GTSxvHVYWdTLiapxXpah4BVdbHD2guyHpf80Jg2gLCFTtD5_rWSvOORbA3z87gn1Aw_R6X58ZcidhnIf92MV2j4oL26e2TVF5Fca0YLYhpS9ikIezftJ-GxfidWkgQUW6Z-41guBCv9Mo-iKJl_madCp1-rXl828";
                    //            break;

                    //        case "dt08bgum":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvMDgiLCJGaW5nZXJwcmludCI6Ijc4ZDRjMGJjM2Y4ZGE1MTRmNTJjZWUzZWFjMjZkODFmY2VjOWFiZjM2YTkwNjJjNzFhMzM5ODMxN2RkZWQxOWU3NjMwMmFmNzBkNDM5MjdlY2Y4NWQ2NzUyY2UyMDA0YzQ3ZDUwYjI1OWZmY2MwN2I4ZTc5MzYzNDgxYWU2NTJkIiwiQm9kZWdhSUQiOiIyIiwibmJmIjoxNjMxNTYyMDYzLCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU2MjA2MywiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.rG1me7xPq4xJnVF7R_BE105-Pq6s50Q3iOQi_wHF12yp01i-1CBs6AHk5Y7_Dd5wpfOEmSnGlkih6e913Fj72uqcDou5QcBS5YEL9_HClp8pRnR1qSBv3XjRjBTpN13zzwZOQyWYqgR2apSvoNYg_u9ubpqvumir6GA_sYIoFtuTQ8W53cxl5epJPqRVRJ6HCn-gkojhLZexcvuFTkUd7UhSyeDgu7Hqcy6KWbs4crgRFsmhTQ_D79MyyIfYvcj2soGdnUwvwgElIighSNVn4bZ3NxnWxub_OuhVRMAo2fs0fABram8ceTyCSoMvyTKEz3XWzZk4cDlacBpbTTlB3I2h2QKYQ2fCWuphUyfZiIcjj4WjYazNgNetKUGYloQRD9_ps4sQqa3Z8Rrp5h2FvYUWHDYx1OwmRCUWjM5vnHQEcypK22p_hJ8WTEO3-kCDjQfC21R4KbR-vdU_AKHrg1PLT4oP9GVQj-V3tddKlNHeXdirnnCC3wndB4HCr4xL67_5rv-9MiegiaAh--MlBddN6loquDs5Mn1a8el1v6fpa_LtI4YnVFI_d1klD9VhkWzSJwS-_bPmdXPBmxW7grkswY-N2dwO7069jZDWE8JTNzrmHJA0ytJgCotgjccCIbwLjusNT7qr_Bp3Rqsv88wGsjv61jVYYF15zosmU";
                    //            break;

                    //        case "dt09estg":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvMDkiLCJGaW5nZXJwcmludCI6ImVkZTNiNTc3ZDE1ZDQzNzdlMGFiZDQ4YjlhYzI2YmQ5MjQwZTFjN2MwOWM3NTNlNjJiNTliNTM4MzEzYjQ2NmFmYTM4MTgwNzEyMTNkMDMzMWE0ZmNlOTE0ZWQ4MTA4NDdlNjBlOTc1NzA0MTk4MmUwNWFlMzE1MmY2NzhkODFmIiwiQm9kZWdhSUQiOiIyIiwibmJmIjoxNjMxNTYyMTAzLCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU2MjEwMywiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.tQM-gpXPoCrO_U2sZHd8WJSY2j2WTAxsfAcAt_Nv58h00FMeNgU1-q5PYJhB3fFKoutEM28YxVY6VbCHYRrk0l5Hld_6L-IWGJqN0g9IgZG0ibViEy8CpnoXw0bPRvtwLqzDa6s5iLwl0cyl1Fibtp6G4mAH-fFy7W0_MEjDix7ng3N6334XXbwe1OXEdulwknUp_3ItPm8zKYbUnClcQBluptYNpycW2-gAhufeVaDEy2d-dEZDURG6h6q7Wja3_tvpzQBiLIlSZOf-N2UeNfzQGdcCCQi00cqd5HIzqtLfylqjNyALBP9gizVoWOv074Qbeet-qkQAK_f5lzBpi4-JudR8wZbEm8of51D4_mXbpa6mFfgTnr6pwopMRW2ZNaAzqWxeYPjYPBC57l0iMJ-0uNal7ZaxVUGYgDyk-WYtmKJUuVhU0XxBTUdWYpRj5WCPaLo5JCJeYaDnjx6gsAK3h77iNkUNv72RznZBiXpNPDRERqqXcoKHHstS18HhFQdYuCa9M98Jje2yK7XIqXieO6EFvtziKuksBWnHjWFAcx2tO07YeTRKdkKXl6x6fxTMTcz8OeA2q32AMFcAL3LB3zxFngqDEtE5bPe0d-MdFpYsAghGScqe-A4zshlBy9MWbE7M0tcmSUcS6RnnTG47HbAM8CQ9UJn4S-1IPjY";
                    //            break;
                    //        //TULA 31 de Dic 2021

                    //        case "dt01ayaj":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvIFR1bGUgMDEiLCJGaW5nZXJwcmludCI6IjQ0YWY3NjgwNjVhZTgxZWZjYzg4NjAzMGU4YWM1NTQ0OGQ1MzM3OGM3MTFiNTI5M2IwNmU4YTFmYzA4YTQ2ODI0YjBmNjM4ZjdmNzQxODc5ZWM0MmRjNDA0ZGYyMjBjYmUwZDlhOTEyYjc2NzhkMTYyNjkyMTdkNjgyNTY0MjY1IiwiQm9kZWdhSUQiOiIxIiwibmJmIjoxNjMxNTcyMjExLCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU3MjIxMSwiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.S5WnXWvya1LEaCssRHnY_GEglk7eai_HYfbZ7erm7FeqOLb3ULyCLyh_tMPi8wsh4h1El-87Y7tkA71PosQazjTqtK7gCMRot7TLLKUcZrqqSdwjxJjPrQ2zgX-cLWVaVlgmeiiVdHHajBaEhoVzcb41mQZvNr3Zj0MHvOOtMx715mHlWrZ3_x5QafJ1glIs4I9yJnSLAaFBroiZtCnQVm3uP22JuTP1DsHT0TpHE_5oQ5iErFboAAtkxmemPa4CRZgJrZ7XKKvei7358ol3c562RiOa9StCa49nm7DkvVC4On03PHdGEO71OTNrjvxet8ram7Z4mugMLrFiMzb6X295Xf6XUob75DoTICkv0OLst9KThtglK190DjM7_z4_bV2F8BwovbAwhJrcOHPZUbP6wO0aJNDOBWqsTDJ2SXcwxNBBavjRdvxyzHJlnaF0dFPh-Ns7lenZ6l3VeMxVelp9pN7AJ5iP-2sUFqERs7dNnh5_ledQnjzKU0kymVwfacE0tihf9McK6xyy8OQCSl4O9WxIRXoEw42ojZigRM2OSfYqUwY4iwDpII1DLWcx5SByGI7hpPXnsIjAN5drhLTqfZHwlaL5zsuxeKthH3ryLurQ0CALaOJ1wEb58Knu3y2hZfKRwvHH6863pNCvFXW9rFIfSfBcHm5uTxpLPXk";

                        break;

                    case "dt02ygqp":
                        tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvIFR1bGUgMDIiLCJGaW5nZXJwcmludCI6ImZjZjBhNTg3NGNjNzk4NzZhYWQ3MTliODQwZGYyMGRmODUwNDYyZjc1NzZjMDZkYmRlNjQ0ZmYzMjk3MmM3MzIyNzZlNTQ5ZDJhZWQwNjc0NmY0NmFkMDlhNzc1NDQzN2Q4ZjFkZTNhODAwODk4MGI0MDFmN2JhZDdjYjBjMTI5IiwiQm9kZWdhSUQiOiIxIiwibmJmIjoxNjMxNTcyMjU0LCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU3MjI1NCwiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.udB_ayYvW9Ke2Hq96qZlLhN09QW9kh6I4e8peO2qGJw_eD1IwChcX1kOioZkFDTsPBkctrv2V0ZnSChr0pZfsqazB3r_xZje6jSkSER26xlsa26f_IMP_TTA0lN_oPKKDRTTIWX-JEPn02UCv-9k_67uFA3h9G5h4TEKAakAnzc5ZXmykjlnIfs1GF6Y26h5Vx-FknzNPQ2SqhfvKvlt8goNazXFTcC96Z80d8LzloGJWj7-8Al81tiXSWPnN-9kJBsPJZ_NhaJAWY8wjJlofc16-YTcnffEbylhsuGYkUZhjMEQcN_SJ6UhaaB83Mjy_s3xXBh4LPMqPXnjLbuXppN0CQoGMJviQ3eq58vYfCvna4-6KbO0UlEI-j3kykzPlO3STBz7KGnP8fUYz5gS4mEM7tQL1uFKUOOQL7CVO1WkxPqepZ0HRNvwxsbkgfBH6gV2rNIET_-7UL0UAcjFU5pXr-xv5YP2yESx_ruCinwx8bHggfyE1_cADdppcasAf2jDpvBYuTnRywhBvB_TwP89GoO_gA-ggQtX1ygZz4HOHUOKG7Rm8VdsEDQ6BJ-HU9MGF88nVRHemHCeKgc7pncWRM3A0LBlTRnGi9IfG0bO01rATcV7_XMii98KiiNzpckQXh2l_Qs0jZnSscNIfobkamu9vvQ6OfzSm0WBIQg";
                        break;
                    //        case "dt03zuet":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvIFR1bGUgMDMiLCJGaW5nZXJwcmludCI6IjY5MGZlMzQ4ZDU0N2RjNDNiODg4MjMzNjJlMTljYTkyZGRiYmQ0OTQ3OGUwYTY0YzAyMWVlNWU3MTc3ZDEyMzMzYTRjODUyYmU2ZjAwM2Y1OTU5YTc2YmJiNWI3MTUyODE1YmMzOWNlMGM1MTAwZGRhMDZjOGYyOWMwNzBkNWY0IiwiQm9kZWdhSUQiOiIxIiwibmJmIjoxNjMxNTcyMjgxLCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU3MjI4MSwiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.i9lIzwFTvrlsZ50XyzwhP63u0N_DtUMHTKCAcriobMDw38CX0yXtZEYgZfNTpORmufIT6PqRmP9ILlBmMcHuzSRMDne9Dflp2dQQyEeyt4lZWPHExoKIbbO-dyYACBzM7TqXrWndmKx96qx1B0o8EiCx1Vr95cNPxDjZzu6BtKYLaRw6VRXOxS_U1VYHgqagu9-0A0faFSs1p4aCmqC9JaGphUJDKVtoYhneGBThxaF2B1hap4xLjAaPItJXJt7-yPet7Dh24SZQVEvTMFxKLzfeSVHUuf37uN6XAV__FNMm57dg-Rh9iYt-_hrsFJy9m7NmtcLvyQqZ9U7X9CVk3WvQikyzaN6sduv6f9F_Q5wG-fj7w19DoH9hHTyA5XjRQIhC9V37C7n8DJo0ItBS6zvKsfcYFkgUkLxHEMO1bQKd_xCGKke5QDCcKW3NrJhwJW8mn2Vr1jg4zjdbqDIzemJl-0xOciyixDT4g0WXO2b3KnNHc5DRQXUJr4SMdgBf0ripdH6ZgGVyeaV4IHjwMLD4GCn2cxrG0G3_Hdn2WBgSvzcqXeQ7BtE0atdX2JJQ8w5u6jjNyZS6N0AlZbGcTQiREFLCWvXVU0TGL6xdG62B--ixJVQgbEoQdFrV4bYFHBGxAt9g3paEtq6ppGkVaLX6eFWPji_RH9ZMUP74UpY";
                    //            break;
                    //        case "dt04qihc":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvIFR1bGUgMDQiLCJGaW5nZXJwcmludCI6ImM5MDE1NTE0NDg3NGExZDMyNzRiNmMxMDU3ZGJhMjg2ZjQ2NmEyZDZjZTRkY2JkZWM2NzJiZDBlOGY4NWE1MzdmZDJlZWFjOTY4M2RhZDA2OGU5Y2M1N2Q2OWQ0ODRlOGFiMDI0YjYyMzVkMDliYjFjYzNmM2JmMGQ5NWE1M2ZiIiwiQm9kZWdhSUQiOiIxIiwibmJmIjoxNjMxNTcyMzIyLCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU3MjMyMiwiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.BfSWYamT8QyBCLH8IbRdTtl34PCbh6VfypUZYJZXC51aJRxpWg3cWz_lqr8VgC24qtlsT4EUFnLTG9psm2ZlDw1j2dLyrOROLr7iuZ6G0hNv2qRXxKgb6AvE9AFGwO_wJKQcncKRChjDxpew56PLd2BO5GAcrdmzJLkA5Lr7JINzS3xJkW_ToZk6gBXT3daUxNuzVY-9_xtjEnZ5jWU6KID4eBf6c3Xsfg4Kc2tvsbNqWZqEu-_p6BgUy7-s9_m12ch-bju3jCSxWINfnqehYP8OnfYfntlgjPstgDirPxEZaAZIvW7hG_o-z-4Tsd8oWCsqwLOneBNGOzBm3ffokXMd-2QIT2pgzZTWkzaNnD2bqFb9fjewxIJc3dKTnndrbglnsYklWozCyMno7tJCYxKZT1wf4ZcNt5cgM1UoaJwPN3FNjiTy2o3ErapUJpQ15uItmxPivhe38N4JP6VElVOTAkeSWcMxLrn50mldgpoLg1DRR34ZHXT_1sS4Dfbbyb9lg3v04Ki5HZrOdrbAMRRHxDbTkzSuHz6Zf-PxprBP3tjf99T2Jv5giBh6a7rfbwg5SatdSqclY1BdPJK-fjgkv-Wjdvc1nWIlTWAa7Sq6_JASugQVhHRcmQ0Lcce1FTe04SgREb9uh8yHAmkMdxc9ilrsFkI4WThTeGTS1WI";
                    //            break;

                    //        case "dt05vsvf":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvIFR1bGUgMDUiLCJGaW5nZXJwcmludCI6ImQ1NGI2MmM4YmRmNWM5NDIyZWViOWI4OGE4NDIwM2ExYmUyNzc4Y2FkMWU2YTIwMDExNTU1ZDBhNzNjMGIwMGNlY2ZlNGExYTQyNmJmZTU4MTAyODdkYjcwMTYxMjM1MDlhMzIwYzE0NGI5OTVkZWEzYzIxNWYyYjc4YjQyMjIyIiwiQm9kZWdhSUQiOiIxIiwibmJmIjoxNjMxNTcyODI1LCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU3MjgyNSwiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.U3xBC_fqU5UaQ6ueyI5TM7XRZ95Uy35NmWLQK3_KyUSWDqPk8YU6FClme1CYDXIz_PQDxxo-ieB0-0PETwVz0tbM8YJOt3J4EcxGiVU3MLMF03BODywHJhR_4fcxtw5Nm5mmRgUKbcCbFUNIRMzVGwieyJX99ycv1c3lsqZCFZ9JxDWoywZMxuvB25TbreXejxjRrldzNix3MwAvtWH9Fq0XKxsKizrK4Wh501TBf8MfmCwn_im-C_MiIUNkuT3Wmto568KMvK0X4rYXTb4cSSuqfC-VgbB6Zmc7pO_0yFuGtVyXG9Ql8sAtTGihI6ZgVLFqedDUhBbp3PS2Tmkqa9hy9drPy_0dQciNZRJWskInivj2DzsUtifbhYCLpRfiHpKqklRf48KSvETOQqPelEfC0-b33D93ZvlIi1I1eyu5-zpJvESdhazQpqS_SwXzA5O2eE7B9a63q2OmiYRshUZo7cbPkR5FjDdkuwiXlWRUdktP7eFwn0L02TY41xiebmxtsgErFRzzyiH29dZvXB2LrgIS0LR8fcp68dENOlGP1hqL1DMU82xxxs5H57CC3Bw-vkNqKqT7ccbV0EA2xbEnsuFeBandLE0E_HI37BW7y8QoiY3mmJxGWxP27oslYL5u-PpJLU7nIgUcww6FkxShxJ-nXTiM1dO-Nsl5kbM";
                    //            break;

                    //        case "dt06nqim":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvIFR1bGUgMDYiLCJGaW5nZXJwcmludCI6IjRiOGIwYTFkY2I2NGUyZmZmMTY2NzE4NWZjMTNjMzVkYjJjMDk5YmVjMDQ4ZmJiZmE3ZTg1MTY3ZjhjNTIxYjk1NGQ5NmZkMGVlMzIyZGRiMDc3NTViYzYyNjYzZmNiM2U5Y2E0ZDRjNjg1ZGQ5OGZiNGMxYzE2NGVlOTAxM2Y1IiwiQm9kZWdhSUQiOiIxIiwibmJmIjoxNjMxNTcyNDk4LCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU2MjQ5OCwiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.Jid7s2S_pf73Qtt6dFsZw1tvKiXbv7UUzDYy2MjXqCOIStg-uyvH6V8i-_z9QF6oBIV704ogQK1uk7XEIRu9wnGj3kIqoiVjpQBL4TwNBJ-OcfGLZwFHxvqKyDuifbUz5t_Z8i2DFZJpx6jTwfnCdbYOuShSzjiHatCujx9AFLHgUK13JtY0tN21jTJhG1pt_BDJF_cP-p130_db5ga2soIjcpqvJQJcRZkKzza-27oQsyW9fHZU-dwAcJOgl6k11PCVoioGW7DFHrMWsaxWbTRCimK1wXuIhulC3kfogE1893NSgNibAQ5gce2gMWO049zhAGinXeVWNp5ScoYnl9Yjq25UKxSYtWauaACrtyw8JVe40t9RHN5uguN6OskSmgiMO1LcFV5Ni1O7vQAkqiAQtB6rZT_gnkN-Gue_aREIYIUkZnR_tSlHhtAFolsCkBROn0kO5rv8gxwNAMWAtYGE5ZdgK9jluXYjLRXW75TcCLoSii-IFNem7bTpmgmitiXULAnPAUI_11MY5O0mHgJzj8RQhK5UG7c2G2Q-RQqI1T7KdfkIR-wEH2XqB93pxOJVk_eR3OE_yZ7mg3rkCf5AT4J-1chgQX4D7HWb2rij_Lq9jothLXzIy8OA0KhgiDPe53sW9NMAUX4bSvfy0OV17_P0OeJAn9LDfZvr6_c";
                    //            break;

                    //        case "dt07trie":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvIFR1bGUgMDciLCJGaW5nZXJwcmludCI6ImQ0NWU0NDVmMDMyOWQ2YjEyMzUyMTI2ZWE2MDRiYjA4OWRkZjJjNzBhM2E2YTU3MTRiYjFhNDA1MDVmYmJiYzgwMjY1Mzk1ODc5ZWRmYTkyMzJhYzkxMTczNDk3N2E5N2YxYTM2YjU5OTgyYTUzZTljYjk0NGJiNzU5NTIxNDIzIiwiQm9kZWdhSUQiOiIxIiwibmJmIjoxNjMxNTcyNTMxLCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU3MjUzMSwiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.m750SGDjhI6cNDy7ZRNLHxVPCt7YMxVJNPmpNPDkVcslLl07B-zdjZr0v4JBFBsZGQiY9MvGF2VEFEuGvGFvvHqk0amkAmSyIEtXj95hpT9T5firqgiKg7vSQirr1ccGleEOJLqmiejwtdkSXg47Q_qqmiW5WhAIWLQZYjPwoXBMyWBZC1DnpEP3Zi-J52eT6VvH8PAY_jyx-H3EPjAne2JGexW8TUZg7Fjuu-E_A21DPAltgcQQ3GlHNzAo1tCujMtJrrW2zRdtZoxpAXuBW2P-gkdxn1YlraqBQnKjv_BiwO0ztbcluW_GYrmoKXBhppsNf3watg3YRZu6QZ-brYtmdTTfL4kqTFdE5W5HA8y_OJo-7_odg8L9JWfEL_mENYn5V5Kq-0dy5H5ARtQR-xitixY61tH68-vQ6q6ddKwGP66Ln7HYtf1tqRoCwiRJHKU8lZ-cVAxPqoN988Ia-6XlrOf0Dz4sj0wPFhzDhtyK4enN728zTtV-IGBoR_gbaCdDmwSqkxIaVGiHWWtVl1Dx0m2yrFplJVwgjJVmc8Buo5lHA1pVxo79jFMxvv9wVkq7dZ50DBQNacrMHrsAEhFeqk9zEQRPii8ir9TWmXL5orEBwuBQgCkLLPrZp6jTDBhIzbS47xiFKPshGA_1tTFjP4Z7iQZPpMFzUHYmwdE";
                    //            break;

                    //        case "dt08wsqt":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvIFR1bGUgMDgiLCJGaW5nZXJwcmludCI6ImM2MmQ4YmE1ODMxMjkzZThiNzM0NzhlYjI1ODg1MGI1NjFjNDFhNzBkNTg1MWEyZDMxMGYzYjZkMDBkMGQzYTg4MGQ4Njc4OTNjYTc3OTQ4YTM5MTc0YzBiNzVjZmVhMTI3ZWQ4NjhmNWZjOWJiYWE0ZmM4OTVmNjYyOWZjZTlmIiwiQm9kZWdhSUQiOiIxIiwibmJmIjoxNjMxNTcyNTY2LCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU3MjU2NiwiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.ArGE33iSKwfxlBc-MyxkkglQFC9Ti7Y1-SmZsXHueUjlN346pXYR9PSLY3DPu89QDOGmO4H7qXhCq66dL0NEklGbAuAG_60NSQ5HALPo4K54Bn_ChLa7pPXPv0GWvzHq2rGQD9mNGin_pwHOLney_EriY1xhrlvYNjmPMgetWeu9fORDLZ_a1ubOBB0Bdj9iwV5haVO2yIMXT4WPjlQVpUuXGYFKOA4cTg14_IpdNn9bZrMw79aYhQNr3U5zDgLFIFD4Loedz9kPU9ZLvKWNliQLRj2vSCprt9M8_ZdyOkE-tM70Nfdl-JwqPtqor3bgU9W9Q4uqbymcoHtx3iuitIBpMZ5Kwv_99MRGeR4TPKUImyWF-vlAiMv21LgQO9aZnsTWVLvzAZATTxIUFXiX3S7bXT_rgSFPIRf6fzDNildBDxvMZysraezr6hT3zD9Yohb8MxJick1-IUXbWY0ZJLE9G7Dd8eD17lnkkq2SzbvgYFYqtASrsTelfN1H7S0k0ZLJsL1BWyN-J-H15EPtWY3HbhKqohFbsexKoquAfcOYdKksKRoH2tNRnWcDfTWAbbV_nwpqRxXIsq7AF63TRsBsNq5dbOp7_0a7MzSyaEHuaoa1DZVkvxGc_Uyz_2w0Gx7yXySeN_yBxpjZiTenr9SD80sATIgFhYofHDRlZ_k";
                    //            break;

                    //        case "dt09bucg":
                    //            tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvIFR1bGUgMDkiLCJGaW5nZXJwcmludCI6ImFjODc1OTRjZDkyYzUwMzc1NzQzNTU2YzBmMTMwOWY5YmFjNjAyMWI0NzVmYjcwNjhmMDhhOWNmNWMzZTY3M2YxZDY4NGFiY2E4M2MzZmM4Zjg0NGZjODE0ZjZhMmY5YTQ5MmZiOGFmY2I2MjRjMTZhZTU3M2VjMjRiNGI1ZjQxIiwiQm9kZWdhSUQiOiIxIiwibmJmIjoxNjMxNTYyNjAzLCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU3MjYwMywiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.usoMnzWxNo4Scu9HRi5qcJYta1O-qDrwBgcFHm9hMm_i2hRRVJvKbvM7WulmzUSOipRf7HsjkHfBHxq9fAU1hVhz0UsDxfdFjJ6O3bi-0BbKLeECbRZPwmBouxhQVfmw34173VQ_AbLow3PjvljUCoPvAZQe_KQT3-6ZF7bBjB1IxtDl4UsUok5et91n4c61XgJlGuLB9lWmanGTt1-2_oLEfwwi1kDz91FIFKapML1NuK1RzUsACnhKYc5pNBTsp356psxkLe10W132k1s9j01rPHcH1GMjMMc4SkE8G3T1RPxoDwn4QBdBSs-v8R05R8p51NfelKKmP1Kp7cWMpp7U79g4P8j7p6O0ThxlfLhLSap7af-cRpuBqlpRLEG0pt3Kca58G4PO4TQTcsMmdvk923Qkx8a1kA5gGd6gsqh3wsP1mTCLhFDjZtbMri64lY1BoCK_948R6AiYUupoeiNA9pGSqGuRGBEOc1eu0XFvRKqhMFQk37QLBOhFCGJuhTXN5FPMEcpi7NqQqCRjAthSVTrAoD9NV9T3FNdT_9Ke8py7yHpwa96PAzXZRKukj6-axZOiCmf7vAeladW9-W_vf73hYCapwy_K8oupzhLVbwBkb1s3zL1BAMGmNEAANFgIJOFcA_VfYLW9RlfQIUch5O5RUtxnxMHhpIqs4V0";
                    //            break;
                    //case "dt10rrsz":
                    //    tokenAcceso = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRpc3Bvc2l0aXZvIFR1bGUgMTAiLCJGaW5nZXJwcmludCI6IjI2NTM1NDQ5NTY0NmE2MjkzODIwYmZhNTVjZGU0ZmM5NTllYzk2NGFmODk4ZDI3MmNjYTk3NjNhZDFmNDVhMTRlMWUxMGExODAyODBlNTJjMzAzOTI1YzI5MmNmMGFmMTcyZTAyZjFiNDNiYWVjOWVhYmFlMTgwYjNlNDUyNTM2IiwiQm9kZWdhSUQiOiIxIiwibmJmIjoxNjMxNTcyNjM0LCJleHAiOjE2NDEwMTY3OTksImlhdCI6MTYzMTU3MjYzNCwiaXNzIjoiRElGLUhpZGFsZ28iLCJhdWQiOiJDZW50cm8tYWNvcGlvIn0.In9iP_8qy3-WiM-657vDfzsm213fSY1H1MS9NZ03AUNfxvD4xnaT6tNVyEwmf2H129v3lTsXcfXQouzP9kJAGXwZU6qf17aDIzYODW7Z1czIBzhaE7iQHmDkq1aDptwE_bhQ0tCaUzDA4mY2QvC60YmY1tjWto1hWXWMeTzOKI5EAquMELxfRVZtJfdDR4aICs9_EKEvNLlvKdBVRuU6lhOfudjmjs1d8Hv4TnN6g2Ojlr8ejk7YXuqPPbYEa-tpW2Sx-Ua_6xJ54N0nVmyYiAV3Ty_CqFK0a7d9gZR2JzVw92rNpDP8_a2V04KPu-WfiJO7ms1wg8m8JJyjzEvox2DMPQ6TeS_s2LpCLp2bIy7-kLTTrA05lfqYcRro1wrC5Pf8O1MmyCmlLVEyISINWNOgLTMhSwT281vjycPZ2vzmQm4GLAafvI0VBwQ06W2a4GvbJG2y2aI0YChAOnXlwTlH7wG6ZzzDNw8hIDBd4f3282Bm08dydKtEk1rUeUbb-Z7d2UDdAwBxZC0apS-ujSZRdYoZESLdrXQTOV7i2be1zmjkyoqhlX8sAgEj7jnKsE2Ka7mhKmdGCwzHkBuNc0La-pv1DeO0dJ1SzN0Vz5Z9QPguEBMK4d0lVN3xjrMtPEDO1UwfS7pCpqcsxPk1a-uEAYB5NTFl5izKFMbsG6o";
                    //    break;

                    default:
                        return BadRequest();
                        //mensaje no puede accesar

                }
                var response = RestSharpHelper.ExecuteGet(API_URL, tokenAcceso, "/catalogos/articulos");
                ViewBag.Art = response.Content;

                ///// 2
                ///
                response = RestSharpHelper.ExecuteGet(API_URL, tokenAcceso, "/catalogos/donadores");
                ViewBag.Don = response.Content;

                ////3
                ///
                response = RestSharpHelper.ExecuteGet(API_URL, tokenAcceso, "/catalogos/unidadesMedida");
                ViewBag.Med = response.Content;

                ////4
                ///
                response = RestSharpHelper.ExecuteGet(API_URL, tokenAcceso, "/registros");
                ViewBag.json = response.Content;
                ViewBag.a = tokenAcceso;

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return BadRequest();
                }
                else
                {
                    return View();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        public IActionResult ReportesRed()
        {
            var sessionUsuario = HttpContext.Session.GetString("usuario");
            var usuario = sessionUsuario.Replace("\"", "");
            return RedirectToAction("Index", "Reportes", new { a = usuario });
        }

        public IActionResult SalidasRed()
        {
            var sessionUsuario = HttpContext.Session.GetString("usuario");
            var usuario = sessionUsuario.Replace("\"", "");
            return RedirectToAction("Index", "Salidas", new { a = usuario });
        }

        public IActionResult CerrarSesion()
        {
            //elimin variables de sesion
            HttpContext.Session.Clear();
            //Cerrar sesion del usuario
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //Redirigir al usuario a la pagina de inicio
            return RedirectToAction("Login", "Acceso");
        }
    }
}
