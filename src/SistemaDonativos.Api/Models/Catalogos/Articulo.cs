using System;

namespace apiDonativos.Models.Catalogos
{
    public class Articulo
    {
        public int id_articulo { get; set; }
        public string nombre { get; set; }
        public int id_categoria { get; set; }
        public bool activo { get; set; } = true;
        public DateTime? fecha_creacion { get; set; }
        public DateTime? fecha_actualizacion { get; set; }

        // Propiedad de navegación
        public Categoria categoria { get; set; }
    }
}
