using System;

namespace apiDonativos.Models.Catalogos
{
    public class Almacen
    {
        public int id_almacen { get; set; }
        public string codigo { get; set; }
        public string nombre { get; set; }
        public string tipo { get; set; }
        public bool Activo { get; set; }
        public DateTime? fecha_creacion { get; set; }
    }
}
