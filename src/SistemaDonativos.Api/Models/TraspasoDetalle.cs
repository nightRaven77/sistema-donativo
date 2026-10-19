using System.ComponentModel.DataAnnotations.Schema;

namespace apiDonativos.Models
{
    public class TraspasoDetalle
    {
        public int id_detalle { get; set; }
        public int id_traspaso { get; set; }
        public int? id_articulo { get; set; }
        public decimal? cantidad { get; set; }

        // ✅ FIX: Especificar FK explícitamente
        [ForeignKey("id_traspaso")]
        public Traspaso Traspaso { get; set; }
    }
}