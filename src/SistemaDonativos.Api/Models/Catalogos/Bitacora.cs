using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace apiDonativos.Models.Catalogos
{
    public class Bitacora
    {
        [Key]
        public int id_bitacora { get; set; }

        [Required]
        public int id_usuario { get; set; }

        [Required]
        public string modulo { get; set; }

        [Required]
        public string submodulo { get; set; }

        [Required]
        public string accion { get; set; }

        public string descripcion { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime fecha_hora { get; set; }
    }
}