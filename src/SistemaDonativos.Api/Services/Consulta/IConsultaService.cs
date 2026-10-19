using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using apiDonativos.Models.Dtos.Consulta;

namespace apiDonativos.Services.Consulta
{
    /// <summary>
    /// Interfaz para servicios de consulta de movimientos e inventario
    /// ✅ ARQUITECTURA: Usa ADO.NET para consultas complejas con múltiples JOINs
    /// ✅ CONVENCIÓN: Métodos async que retornan DTOs específicos
    /// </summary>
    public interface IConsultaService
    {
        /// <summary>
        /// Consulta unificada de movimientos (entradas, salidas, traspasos) con paginación
        /// </summary>
        Task<ConsultaMovimientosResponse> ConsultarMovimientosAsync(ConsultaMovimientosRequest request);

        /// <summary>
        /// Obtiene inventario en tiempo real por almacén con cálculo dinámico
        /// </summary>
        Task<InventarioAlmacenResponse> ObtenerInventarioPorAlmacenAsync(int idAlmacen, InventarioAlmacenRequest request);

        /// <summary>
        /// Obtiene detalle completo de un movimiento específico (entrada/salida/traspaso)
        /// </summary>
        Task<DetalleMovimientoResponse> ObtenerDetalleMovimientoAsync(string tipo, int id);

        /// <summary>
        /// Obtiene estadísticas y KPIs de un almacén para dashboard
        /// </summary>
        Task<EstadisticasAlmacenResponse> ObtenerEstadisticasAlmacenAsync(int idAlmacen);
    }
}
