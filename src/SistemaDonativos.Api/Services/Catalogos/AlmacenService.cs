using apiDonativos.Models.Dtos;
using apiDonativos.Services.Base;  // ✅ FASE 3
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;  // ✅ FASE 3
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apiDonativos.Services.Catalogos
{
    public interface IAlmacen
    {
        Task<List<AlmacenResponse>> GetAllAsync();
    }

    /// <summary>
    /// Servicio para gestión de almacenes
    /// ✅ FASE 3: Heredado de ServiceBase con timeout configurable
    /// </summary>
    public class AlmacenService : ServiceBase<AlmacenService>, IAlmacen
    {
        public AlmacenService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AlmacenService> logger)
            : base(context, configuration, logger)
        {
        }

        public async Task<List<AlmacenResponse>> GetAllAsync()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            LogInicioOperacion("GetAllAsync - Almacenes");

            var almacenes = await _context.Almacenes
                .AsNoTracking()
                .ToListAsync();

            var almacenResponses = almacenes.Select(a => new AlmacenResponse
            {
                id_almacen = a.id_almacen,
                codigo = a.codigo,
                nombre = a.nombre
            }).ToList();

            stopwatch.Stop();
            LogExitoOperacion("GetAllAsync - Almacenes", stopwatch.ElapsedMilliseconds, new { count = almacenResponses.Count });

            return almacenResponses;
        }
    }
}
