using System;
using System.ComponentModel.DataAnnotations;

namespace apiDonativos.Models.Catalogos
{
    public class contactoDonador
    {
        public int id_contacto { get; set; }
        public int id_donador { get; set; }
        public string nombre { get; set; }
        public string puesto { get; set; }
        public string telefono { get; set; }
        public string extension { get; set; }
        public string celular { get; set; }
        public string email { get; set; }
        public string horario_contacto { get; set; }
        public string medio_preferido { get; set; }
        public bool es_principal { get; set; }
        public string notas { get; set; }
        public bool activo { get; set; }
        
        // Campos de auditoría (cambiar nombre para consistencia)
        public DateTime? fecha_creacion { get; set; }
        public DateTime? fecha_actualizacion { get; set; }
    }
}
