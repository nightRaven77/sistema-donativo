using apiDonativos.Models.Dtos;
using apiDonativos.Services.Catalogos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;  // ✅ NUEVO
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apiDonativos.Controllers.Catalogos
{
    [ApiController]
    [Authorize]
    [Route("catalogos/[controller]")]
    public class AlmacenController : ControllerBase
    {
        private readonly IAlmacen _almacenService;
        private readonly ILogger<AlmacenController> _logger;  // ✅ NUEVO

        public AlmacenController(IAlmacen almacenService, ILogger<AlmacenController> logger)  // ✅ MODIFICADO
        {
            _almacenService = almacenService;
            _logger = logger;  // ✅ NUEVO
        }
        /// <summary>
        /// Obtiene el listado completo de almacenes
        /// GET: catalogos/almacen
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<AlmacenResponse>>> ObtenerListado()  // ✅ Corrijo nombre
        {
            var almacenes = await _almacenService.GetAllAsync();
            return Ok(almacenes);
        }
    }
}
