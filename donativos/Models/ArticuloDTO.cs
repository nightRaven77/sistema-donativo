using System;

namespace donativos.Models
{
    public class ArticuloDTO
    {
        public int? id_articulo { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public int id_categoria { get; set; }
        public string unidad_medida { get; set; }
        // Propiedad de navegación
        public Categoria categoria { get; set; }
    }

    public class Categoria
    {
        public int id_categoria { get; set; }
        public string nombre { get; set; }
    }
}

