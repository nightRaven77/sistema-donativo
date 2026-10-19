using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace apiDonativos.Models.Dtos
{
    /// <summary>
    /// DTO para crear una salida
    /// ✅ CONVENCIÓN: Propiedades en PascalCase (C#) → camelCase automático en JSON
    /// </summary>
    public class CreateSalidaRequest
    {
        /// <summary>
        /// Fecha de salida - JSON: "fechaSalida"
        /// </summary>
        [Required(ErrorMessage = "La fecha de salida es requerida")]
        public DateTime FechaSalida { get; set; }

        /// <summary>
        /// ID del almacén de origen - JSON: "idAlmacenOrigen"
        /// </summary>
        [Required(ErrorMessage = "El almacén de origen es requerido")]
        public int IdAlmacenOrigen { get; set; }

        /// <summary>
        /// ID del beneficiario (opcional) - JSON: "idBeneficiario"
        /// </summary>
        public int? IdBeneficiario { get; set; }

        /// <summary>
        /// Tipo de salida: donacion, merma o baja - JSON: "tipoSalida"
        /// </summary>
        [Required(ErrorMessage = "El tipo de salida es requerido")]
        [RegularExpression("^(donacion|merma|baja)$", 
            ErrorMessage = "El tipo de salida debe ser: donacion, merma o baja")]
        public string TipoSalida { get; set; }

        /// <summary>
        /// Motivo de la salida - JSON: "motivo"
        /// </summary>
        [Required(ErrorMessage = "El motivo de la salida es requerido")]
        [MinLength(5, ErrorMessage = "El motivo debe tener al menos 5 caracteres")]
        [MaxLength(1000, ErrorMessage = "El motivo no puede exceder 1000 caracteres")]
        public string Motivo { get; set; }

        /// <summary>
        /// Observaciones - JSON: "observaciones"
        /// </summary>
        public string Observaciones { get; set; }

        /// <summary>
        /// Estatus - JSON: "estatus"
        /// </summary>
        public string Estatus { get; set; }

        /// <summary>
        /// Fecha de entrega - JSON: "fechaEntrega"
        /// </summary>
        public DateTime? FechaEntrega { get; set; }

        /// <summary>
        /// Nombre de quien recibió - JSON: "recibio"
        /// </summary>
        public string Recibio { get; set; }

        /// <summary>
        /// Lista de detalles - JSON: "detalles"
        /// </summary>
        [Required(ErrorMessage = "Los detalles son requeridos")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un detalle")]
        public List<SalidaDetalleRequest> Detalles { get; set; }

        /// <summary>
        /// Lista de archivos - JSON: "archivos"
        /// </summary>
        [MaxLength(10, ErrorMessage = "Máximo 10 archivos permitidos")]
        public List<CreateSalidaArchivoRequest> Archivos { get; set; }
    }

    /// <summary>
    /// DTO para actualizar una salida
    /// ✅ CONVENCIÓN: Propiedades en PascalCase
    /// </summary>
    public class UpdateSalidaRequest
    {
        [Required(ErrorMessage = "La fecha de salida es requerida")]
        public DateTime FechaSalida { get; set; }

        public int? IdBeneficiario { get; set; }

        [Required(ErrorMessage = "El tipo de salida es requerido")]
        [RegularExpression("^(donacion|merma|baja)$", 
            ErrorMessage = "El tipo de salida debe ser: donacion, merma o baja")]
        public string TipoSalida { get; set; }

        [Required(ErrorMessage = "El motivo de la salida es requerido")]
        [MinLength(5, ErrorMessage = "El motivo debe tener al menos 5 caracteres")]
        [MaxLength(1000, ErrorMessage = "El motivo no puede exceder 1000 caracteres")]
        public string Motivo { get; set; }

        public string Observaciones { get; set; }

        public string Estatus { get; set; }

        public DateTime? FechaEntrega { get; set; }

        public string Recibio { get; set; }

        public List<SalidaDetalleUpdateRequest> Detalles { get; set; }

        /// <summary>
        /// Lista de archivos para actualizar - JSON: "archivos"
        /// </summary>
        [MaxLength(10, ErrorMessage = "Máximo 10 archivos permitidos")]
        public List<UpdateSalidaArchivoRequest> Archivos { get; set; }
    }

    /// <summary>
    /// DTO para detalle de salida en creación
    /// </summary>
    public class SalidaDetalleRequest
    {
        [Required(ErrorMessage = "El artículo es requerido")]
        public int IdArticulo { get; set; }

        [Required(ErrorMessage = "La cantidad entregada es requerida")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal CantidadEntregada { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El valor unitario debe ser mayor o igual a 0")]
        public decimal? ValorUnitario { get; set; }

        public string Observaciones { get; set; }

        [Required(ErrorMessage = "La unidad de medida es requerida")]
        public string UnidadMedida { get; set; }
    }

    /// <summary>
    /// DTO para actualizar detalle de salida
    /// </summary>
    public class SalidaDetalleUpdateRequest
    {
        public int? IdDetalle { get; set; }

        [Required(ErrorMessage = "El artículo es requerido")]
        public int IdArticulo { get; set; }

        [Required(ErrorMessage = "La cantidad entregada es requerida")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal CantidadEntregada { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El valor unitario debe ser mayor o igual a 0")]
        public decimal? ValorUnitario { get; set; }

        public string Observaciones { get; set; }

        [Required(ErrorMessage = "La unidad de medida es requerida")]
        public string UnidadMedida { get; set; }
    }

    /// <summary>
    /// DTO para listado de salidas - Response
    /// </summary>
    public class SalidaListadoResponse
    {
        public int IdSalida { get; set; }
        public string Folio { get; set; }
        public string NombreAlmacen { get; set; }
        public string NombreBeneficiario { get; set; }
        public string TipoSalida { get; set; }  // ✅ NUEVO - donacion, merma o baja
        public string Motivo { get; set; }
        public DateTime FechaSalida { get; set; }
        public string Estatus { get; set; }
        public int CantidadArticulos { get; set; }
    }

    /// <summary>
    /// DTO para detalle completo de una salida - Response
    /// </summary>
    public class SalidaCompleteResponse
    {
        public int IdSalida { get; set; }
        public string Folio { get; set; }
        public int IdBeneficiario { get; set; }
        public string NombreBeneficiario { get; set; }
        public int IdAlmacenOrigen { get; set; }
        public string NombreAlmacen { get; set; }  // ✅ NUEVO
        public string TipoSalida { get; set; }
        public string Motivo { get; set; }
        public DateTime FechaSalida { get; set; }
        public string Estatus { get; set; }
        public string Observaciones { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string Recibio { get; set; }
        public List<SalidaDetalleResponse> Detalles { get; set; }
        public List<SalidaArchivoResponse> Archivos { get; set; }  // ✅ NUEVO
    }

    /// <summary>
    /// DTO para respuesta de detalle individual
    /// </summary>
    public class SalidaDetalleResponse
    {
        public int IdDetalle { get; set; }
        public int IdArticulo { get; set; }
        public string NombreArticulo { get; set; }
        public int Categoria { get; set; }
        public decimal CantidadEntregada { get; set; }
        public decimal? ValorUnitario { get; set; }
        public string UnidadMedida { get; set; }
        public string Observaciones { get; set; }
    }

    /// <summary>
    /// DTO para listado de detalles con información de artículos
    /// </summary>
    public class SalidaDetalleListadoResponse
    {
        public int IdArticulo { get; set; }
        public string NombreArticulo { get; set; }
        public int IdCategoria { get; set; }
        public string CategoriaArticulo { get; set; }
        public string UnidadMedida { get; set; }
        public decimal CantidadEntregada { get; set; }
        public decimal? ValorUnitario { get; set; }
        public decimal? ValorTotal { get; set; }
    }

    /// <summary>
    /// DTO para detalles con archivos
    /// </summary>
    public class SalidaDetalleConArchivosResponse
    {
        public List<SalidaDetalleListadoResponse> Detalles { get; set; }
    }

    /// <summary>
    /// DTO para crear archivo de salida
    /// ✅ CONVENCIÓN: Propiedades en PascalCase
    /// </summary>
    public class CreateSalidaArchivoRequest : IValidatableObject
    {
        [Required(ErrorMessage = "La URL del archivo es requerida")]
        [StringLength(2048, ErrorMessage = "La URL no puede exceder 2048 caracteres")]
        public string Url { get; set; }

        [Required(ErrorMessage = "El nombre del archivo es requerido")]
        [StringLength(255, ErrorMessage = "El nombre del archivo no puede exceder 255 caracteres")]
        public string NombreOriginal { get; set; }

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

    /// <summary>
    /// DTO para actualizar archivo de salida
    /// </summary>
    public class UpdateSalidaArchivoRequest
    {
        /// <summary>
        /// ID del archivo (null si es nuevo)
        /// </summary>
        public int? IdArchivo { get; set; }

        [Required(ErrorMessage = "La URL del archivo es requerida")]
        [StringLength(2048, ErrorMessage = "La URL no puede exceder 2048 caracteres")]
        public string Url { get; set; }

        [Required(ErrorMessage = "El nombre del archivo es requerido")]
        [StringLength(255, ErrorMessage = "El nombre del archivo no puede exceder 255 caracteres")]
        public string NombreOriginal { get; set; }
    }

    /// <summary>
    /// DTO para respuesta de archivos de salida
    /// </summary>
    public class SalidaArchivoResponse
    {
        public int IdArchivo { get; set; }
        public string NombreArchivo { get; set; }
        public string UrlArchivo { get; set; }
    }
}
