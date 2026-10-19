using System;
using System.ComponentModel.DataAnnotations;

namespace apiDonativos.Models.Dtos.Reportes
{
    /// <summary>
    /// Parámetros para el reporte Kardex (historial de movimientos de un artículo)
    /// ✅ CONVENCIÓN: Propiedades en camelCase para coincidir con parámetros SQL
    /// ⚠️ NOTA: id_almacen y id_articulo son REQUERIDOS para este reporte
    /// </summary>
    public class ReporteKardexDto
    {
        [Required(ErrorMessage = "El almacén es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "ID de almacén inválido")]
        public int id_almacen { get; set; }

        [Required(ErrorMessage = "El artículo es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "ID de artículo inválido")]
        public int id_articulo { get; set; }

        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_fin { get; set; }
    }
}
