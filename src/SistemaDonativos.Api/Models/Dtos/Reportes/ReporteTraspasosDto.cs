using System;

namespace apiDonativos.Models.Dtos.Reportes
{
    /// <summary>
    /// Parámetros para el reporte de traspasos entre almacenes
    /// ✅ CONVENCIÓN: Propiedades en camelCase para coincidir con parámetros SQL
    /// </summary>
    public class ReporteTraspasosDto
    {
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_fin { get; set; }
        public int? id_almacen_origen { get; set; }
        public int? id_almacen_destino { get; set; }
        public int? id_articulo { get; set; }
        public int? id_categoria { get; set; }
        public string estatus { get; set; }            // "Completado", "Cancelado", etc.
        public string agrupar_por { get; set; }        // "Fecha", "AlmacenOrigen", "AlmacenDestino", "Articulo", "Categoria"
        public int pagina { get; set; } = 1;
        public int registros_por_pagina { get; set; } = 50;
        public string ordenar_por { get; set; } = "fecha_traspaso";
        public string orden { get; set; } = "DESC";    // "ASC" o "DESC"
    }
}
