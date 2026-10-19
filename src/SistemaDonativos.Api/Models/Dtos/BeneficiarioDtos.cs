using System;
using System.ComponentModel.DataAnnotations;

namespace apiDonativos.Models.Dtos
{
    // ✅ NOTA: BeneficiarioResponse está definido en BeneficiarioResponse.cs
    // No duplicar aquí para evitar errores de compilación

    /// <summary>
    /// DTO para respuesta completa de beneficiario (detalle)
    /// </summary>
    public class BeneficiarioCompleteResponse
    {
        public int id_beneficiario { get; set; }
        public string codigo { get; set; }
        public string tipo { get; set; }
        public string nombre { get; set; }
        public string contacto_principal { get; set; }
        public string telefono { get; set; }
        public string email { get; set; }
        public string direccion { get; set; }
        public string colonia { get; set; }
        public string ciudad { get; set; }
        public string estado { get; set; }
        public string codigo_postal { get; set; }
        public string curp { get; set; }
        public int total_ayudas { get; set; }
        public decimal monto_total_recibido { get; set; }
        public string estatus { get; set; }
        public string observaciones { get; set; }
        public DateTime? fecha_registro { get; set; }
        public DateTime? fecha_ultima_ayuda { get; set; }
    }

    /// <summary>
    /// DTO para crear un beneficiario
    /// </summary>
    public class CreateBeneficiarioRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string nombre { get; set; }

        public string tipo { get; set; } // Persona Física, Persona Moral, Institución

        public string contacto_principal { get; set; }

        public string telefono { get; set; }

        public string email { get; set; }

        public string direccion { get; set; }
        public string colonia { get; set; }
        public string ciudad { get; set; }
        public string estado { get; set; }
        public string codigo_postal { get; set; }
        public string curp { get; set; }
        public string observaciones { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un beneficiario
    /// </summary>
    public class UpdateBeneficiarioRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string nombre { get; set; }

        [Required(ErrorMessage = "El tipo es requerido")]
        public string tipo { get; set; }

        public string contacto_principal { get; set; }

        [Phone(ErrorMessage = "Teléfono inválido")]
        public string telefono { get; set; }

        [EmailAddress(ErrorMessage = "Email inválido")]
        public string email { get; set; }

        public string direccion { get; set; }
        public string colonia { get; set; }
        public string ciudad { get; set; }
        public string estado { get; set; }
        public string codigo_postal { get; set; }
        public string curp { get; set; }
        public string estatus { get; set; }
        public string observaciones { get; set; }
    }
}