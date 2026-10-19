using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace apiDonativos.Validation
{
    /// <summary>
    /// Atributo de validación para el tipo de salida
    /// Solo permite: "donacion", "merma", "baja"
    /// </summary>
    public class TipoSalidaAttribute : ValidationAttribute
    {
        private static readonly string[] ValoresPermitidos = { "donacion", "merma", "baja" };

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("El tipo de salida es requerido");
            }

            string tipo = value.ToString().ToLower();

            if (!ValoresPermitidos.Contains(tipo))
            {
                return new ValidationResult(
                    $"El tipo de salida debe ser uno de los siguientes: {string.Join(", ", ValoresPermitidos)}"
                );
            }

            return ValidationResult.Success;
        }
    }
}
