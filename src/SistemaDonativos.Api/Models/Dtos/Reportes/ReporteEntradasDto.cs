using System;

namespace apiDonativos.Models.Dtos.Reportes
{
    /// <summary>
    /// Parámetros para el reporte de entradas al inventario (donativos)
    /// ✅ CONVENCIÓN: Propiedades en camelCase para coincidir con parámetros SQL
    /// </summary>
    public class ReporteEntradasDto
    {
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_fin { get; set; }
        public int? id_almacen { get; set; }
        public int? id_donador { get; set; }
        public int? id_articulo { get; set; }
        public int? id_categoria { get; set; }
        public string tipo { get; set; }              // "especie" o "monetario"
        public string estatus { get; set; }            // "Recibido", "Cancelado", etc.
        public string agrupar_por { get; set; }        // "Fecha", "Donador", "Articulo", "Categoria", "Almacen"
        public int pagina { get; set; } = 1;
        public int registros_por_pagina { get; set; } = 50;
        public string ordenar_por { get; set; } = "fecha_recepcion";
        public string orden { get; set; } = "DESC";    // "ASC" o "DESC"
    }
}
