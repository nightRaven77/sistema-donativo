using System;
using System.ComponentModel.DataAnnotations;

namespace apiDonativos.Models.Catalogos
{
    public class Donador
    {
        [Key]
        public int id_donador { get; set; }
        public string codigo { get; set; }
        public string tipo { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Especifique el nombre")]
        public string nombre { get; set; }
        public string RFC { get; set; }
        public string telefono { get; set; }
        public string email { get; set; }
        public string direccion { get; set; }
        public string colonia { get; set; }
        public string ciudad { get; set; }
        public string estado { get; set; }
        public string codigo_postal { get; set; }
        public string notas { get; set; }
        
        public bool activo { get; set; }

        // Campos de auditoría
        public DateTime? fecha_creacion { get; set; }
        public DateTime? fecha_actualizacion { get; set; }
    }
}
