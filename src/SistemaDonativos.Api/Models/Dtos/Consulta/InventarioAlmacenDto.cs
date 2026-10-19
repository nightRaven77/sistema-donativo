using System;
using System.Collections.Generic;

namespace apiDonativos.Models.Dtos.Consulta
{
    /// <summary>
    /// DTO para consulta de inventario en tiempo real por almacén
    /// ✅ ENDPOINT: GET /api/consulta/inventario/almacen/{idAlmacen}
    /// ✅ CONVENCIÓN: Propiedades en snake_case para coincidir con BD
    /// </summary>
    public class InventarioAlmacenRequest
    {
        public string? busqueda { get; set; }
        public int? id_categoria { get; set; }
        public bool solo_con_stock { get; set; } = false;
        public string ordenar_por { get; set; } = "nombre"; // nombre, stock, categoria
        public string orden_direccion { get; set; } = "asc"; // asc, desc
    }

    /// <summary>
    /// Respuesta de inventario por almacén
    /// </summary>
    public class InventarioAlmacenResponse
    {
        public bool exito { get; set; }
        public string mensaje { get; set; }
        public InventarioAlmacenDatos data { get; set; }
        public string error { get; set; }
    }

    public class InventarioAlmacenDatos
    {
        public AlmacenInfo almacen { get; set; }
        public int total_articulos { get; set; }
        public int total_articulos_con_stock { get; set; }
        public List<ArticuloInventario> articulos { get; set; }
        public List<ResumenCategoria> resumen_por_categoria { get; set; }
    }

    public class AlmacenInfo
    {
        public int id_almacen { get; set; }
        public string nombre { get; set; }
        public string ubicacion { get; set; }
        public string responsable { get; set; }
    }

    public class ArticuloInventario
    {
        public int id_articulo { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public CategoriaInfo categoria { get; set; }
        public string unidad_medida { get; set; }
        public decimal stock_actual { get; set; }
        public decimal entradas_totales { get; set; }
        public decimal salidas_totales { get; set; }
        public DateTime? fecha_ultima_entrada { get; set; }
        public DateTime? fecha_ultima_salida { get; set; }
        public int movimientos_recientes { get; set; } // últimos 30 días
        public bool alerta_stock_bajo { get; set; }
        public bool activo { get; set; }
    }

    public class CategoriaInfo
    {
        public int id_categoria { get; set; }
        public string nombre { get; set; }
    }

    public class ResumenCategoria
    {
        public int id_categoria { get; set; }
        public string nombre_categoria { get; set; }
        public int total_articulos { get; set; }
        public int articulos_con_stock { get; set; }
        public decimal stock_total { get; set; }
    }
}
