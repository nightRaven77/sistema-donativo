using System;

namespace apiDonativos.Models.Dtos.Reportes
{
    /// <summary>
    /// Parámetros para el reporte de existencias actuales en inventario
    /// ✅ CONVENCIÓN: Propiedades en camelCase para coincidir con parámetros SQL
    /// </summary>
    public class ReporteExistenciasDto
    {
        public int? id_almacen { get; set; }
        public int? id_articulo { get; set; }
        public int? id_categoria { get; set; }
        public decimal? cantidad_minima { get; set; }
        public decimal? cantidad_maxima { get; set; }
        public bool solo_sin_existencia { get; set; } = false;
        public bool solo_stock_bajo { get; set; } = false;
        public string agrupar_por { get; set; }        // "Almacen", "Categoria", "Articulo"
        public string ordenar_por { get; set; } = "nombre_articulo";
        public string orden { get; set; } = "ASC";     // "ASC" o "DESC"
        public DateTime? fecha_corte { get; set; }     // Fecha específica para consultar existencias
        public int pagina { get; set; } = 1;
        public int registros_por_pagina { get; set; } = 100;
    }
}
