using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace apiDonativos.Models.Dtos
{
    /// <summary>
    /// Request para actualizar un donativo existente
    /// ✅ CONVENCIÓN: Propiedades en PascalCase (C#) → camelCase automático en JSON
    /// </summary>
    public class UpdateDonativoRequest
    {
        /// <summary>
        /// Fecha en que se recibe el donativo - JSON: "fechaRecepcion"
        /// </summary>
        [Required(ErrorMessage = "La fecha de recepción es requerida")]
        public DateTime FechaRecepcion { get; set; }

        /// <summary>
        /// Tipo de donativo - JSON: "tipo"
        /// </summary>
        [Required(ErrorMessage = "El tipo de donativo es requerido")]
        [StringLength(50, ErrorMessage = "El tipo no puede exceder 50 caracteres")]
        public string Tipo { get; set; }

        /// <summary>
        /// Estatus del donativo - JSON: "estatus"
        /// </summary>
        [Required(ErrorMessage = "El estatus es requerido")]
        [StringLength(50, ErrorMessage = "El estatus no puede exceder 50 caracteres")]
        public string Estatus { get; set; }

        /// <summary>
        /// Observaciones adicionales - JSON: "observaciones"
        /// </summary>
        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string Observaciones { get; set; }

        /// <summary>
        /// Lista de detalles del donativo - JSON: "detalles"
        /// </summary>
        public List<UpdateDonativoDetalleRequest> Detalles { get; set; }

        /// <summary>
        /// Lista de archivos asociados - JSON: "archivos"
        /// </summary>
        [MaxLength(10, ErrorMessage = "Máximo 10 archivos permitidos")]
        public List<UpdateDonativoArchivoRequest> Archivos { get; set; }
    }

    public class UpdateDonativoDetalleRequest
    {
        /// <summary>
        /// ID del detalle (null si es nuevo) - JSON: "idDonativoDetalle"
        /// </summary>
        public int? IdDonativoDetalle { get; set; }

        public int IdDonativo { get; set; }

        /// <summary>
        /// ID del artículo donado - JSON: "idArticulo"
        /// </summary>
        [Required(ErrorMessage = "El ID del artículo es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del artículo debe ser mayor a 0")]
        public int IdArticulo { get; set; }

        /// <summary>
        /// Cantidad de artículos recibidos - JSON: "cantidadRecibida"
        /// </summary>
        [Required(ErrorMessage = "La cantidad recibida es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int CantidadRecibida { get; set; }

        /// <summary>
        /// Valor unitario del artículo - JSON: "valorUnitario"
        /// </summary>
        [Required(ErrorMessage = "El valor unitario es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El valor unitario debe ser mayor o igual a 0")]
        public decimal ValorUnitario { get; set; }

        /// <summary>
        /// Estado del artículo - JSON: "estado"
        /// </summary>
        [Required(ErrorMessage = "El estado del artículo es requerido")]
        [StringLength(50, ErrorMessage = "El estado no puede exceder 50 caracteres")]
        public string Estado { get; set; }

        /// <summary>
        /// Descripción específica - JSON: "descripcion"
        /// </summary>
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string Descripcion { get; set; }

        /// <summary>
        /// Unidad de medida - JSON: "unidadMedida"
        /// </summary>
        [Required(ErrorMessage = "La unidad de medida es requerida")]
        [StringLength(50, ErrorMessage = "La unidad de medida no puede exceder 50 caracteres")]
        public string UnidadMedida { get; set; }
    }

    /// <summary>
    /// Request para archivo de donativo en actualización
    /// ✅ CONVENCIÓN: Propiedades en PascalCase
    /// </summary>
    public class UpdateDonativoArchivoRequest : IValidatableObject
    {
        /// <summary>
        /// ID del archivo (null si es nuevo) - JSON: "idArchivo"
        /// </summary>
        public int? IdArchivo { get; set; }

        public int IdDonativo { get; set; }

        /// <summary>
        /// URL del archivo almacenado - JSON: "url"
        /// </summary>
        [Required(ErrorMessage = "La URL del archivo es requerida")]
        [StringLength(2048, ErrorMessage = "La URL no puede exceder 2048 caracteres")]
        public string Url { get; set; }

        /// <summary>
        /// Nombre original del archivo - JSON: "nombreOriginal"
        /// </summary>
        [Required(ErrorMessage = "El nombre del archivo es requerido")]
        [StringLength(255, ErrorMessage = "El nombre del archivo no puede exceder 255 caracteres")]
        public string NombreOriginal { get; set; }

        /// <summary>
        /// Validación personalizada usando FileValidator
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Helpers.FileValidator.ValidarNombreArchivo(NombreOriginal, out string errorNombre))
            {
                yield return new ValidationResult(errorNombre, new[] { nameof(NombreOriginal) });
            }

            if (!Helpers.FileValidator.ValidarUrlArchivo(Url, out string errorUrl))
            {
                yield return new ValidationResult(errorUrl, new[] { nameof(Url) });
            }
        }
    }
}
