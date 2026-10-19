using System;
using System.Collections.Generic;

namespace apiDonativos.Models.Dtos.Consulta
{
    /// <summary>
    /// DTO para estadísticas y KPIs de un almacén
    /// ✅ ENDPOINT: GET /api/consulta/inventario/estadisticas/{idAlmacen}
    /// ✅ CONVENCIÓN: Propiedades en snake_case para coincidir con BD
    /// </summary>
    public class EstadisticasAlmacenResponse
    {
        public bool exito { get; set; }
        public string mensaje { get; set; }
        public EstadisticasAlmacenDatos data { get; set; }
        public string error { get; set; }
    }

    public class EstadisticasAlmacenDatos
    {
        public AlmacenInfo almacen { get; set; }
        public ResumenEstadisticas resumen { get; set; }
        public MovimientosEstadisticas movimientos_hoy { get; set; }
        public MovimientosEstadisticas movimientos_este_mes { get; set; }
        public List<ArticuloRanking> top5_articulos_mas_stock { get; set; }
        public List<ArticuloRanking> top5_articulos_menos_stock { get; set; }
        public List<CategoriaMovimiento> categorias_mas_movimiento { get; set; }
    }

    public class ResumenEstadisticas
    {
        public int total_articulos_catalogo { get; set; }
        public int articulos_con_stock { get; set; }
        public int articulos_sin_stock { get; set; }
        public decimal stock_total_general { get; set; }
        public int alertas_stock_bajo { get; set; }
    }

    public class MovimientosEstadisticas
    {
        public int entradas { get; set; }
        public int salidas { get; set; }
        public int traspasos { get; set; }
        public int traspasos_recibidos { get; set; }
        public int traspasos_pendientes { get; set; }
    }

    public class ArticuloRanking
    {
        public int id_articulo { get; set; }
        public string nombre { get; set; }
        public decimal stock { get; set; }
        public string unidad { get; set; }
    }

    public class CategoriaMovimiento
    {
        public string categoria { get; set; }
        public int entradas { get; set; }
        public int salidas { get; set; }
    }
}
