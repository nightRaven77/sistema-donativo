using System.ComponentModel.DataAnnotations;

namespace apiDonativos.Models.Dtos
{
    /// <summary>
    /// DTO para cancelar un donativo
    /// ✅ CONVENCIÓN: Propiedades en PascalCase (C#) → camelCase automático en JSON
    /// </summary>
    public class CancelarDonativoRequest
    {
        /// <summary>
        /// Motivo de la cancelación (requerido)
        /// JSON: "motivoCancelacion"
        /// </summary>
        [Required(ErrorMessage = "El motivo de cancelación es requerido")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "El motivo debe tener entre 5 y 500 caracteres")]
        public string MotivoCancelacion { get; set; }
    }
}
