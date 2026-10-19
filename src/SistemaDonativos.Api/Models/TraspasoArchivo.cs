using System;

namespace apiDonativos.Models
{
    public class TraspasoArchivo
    {
        public int id_archivo { get; set; }
        public int? id_traspaso { get; set; }
        public string url { get; set; }
        public string nombre_original { get; set; }
        public string tipo_archivo { get; set; } // "Envio" o "Recepcion"
        public bool? activo { get; set; }
        
        // Propiedad de navegación
        public Traspaso Traspaso { get; set; }
    }
}