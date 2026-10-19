using System;

namespace apiDonativos.Models.Dtos.Reportes
{
    /// <summary>
    /// Parámetros para el reporte de salidas del inventario (entregas)
    /// ✅ CONVENCIÓN: Propiedades en camelCase para coincidir con parámetros SQL
    /// </summary>
    public class ReporteSalidasDto
    {
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_fin { get; set; }
        public int? id_almacen { get; set; }
        public int? id_beneficiario { get; set; }
        public int? id_articulo { get; set; }
        public int? id_categoria { get; set; }
        public string tipo_salida { get; set; }       // "Donacion", "Baja", "Merma"
        public string estatus { get; set; }            // "Entregado", "Cancelado", etc.
        public string agrupar_por { get; set; }        // "Fecha", "Beneficiario", "Articulo", "Categoria", "Almacen"
        public int pagina { get; set; } = 1;
        public int registros_por_pagina { get; set; } = 50;
        public string ordenar_por { get; set; } = "fecha_salida";
        public string orden { get; set; } = "DESC";    // "ASC" o "DESC"
    }
}
