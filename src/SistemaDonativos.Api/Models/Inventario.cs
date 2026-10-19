using System;

namespace apiDonativos.Models
{
    public class Inventario
    {
        public int id_inventario { get; set; }
        public int id_almacen { get; set; }
        public int id_articulo { get; set; }
        public decimal cantidad { get; set; }
        public DateTime? fecha_ultima_entrada { get; set; }
        public DateTime? fecha_ultima_salida { get; set; }
        public DateTime fecha_actualizacion { get; set; }
    }
}
