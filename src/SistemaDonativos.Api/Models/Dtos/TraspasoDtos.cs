using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace apiDonativos.Models.Dtos
{
    /// <summary>
    /// DTO para crear un traspaso
    /// ✅ CONVENCIÓN: Propiedades en PascalCase (C#) → camelCase automático en JSON
    /// </summary>
    public class CreateTraspasoRequest
    {
        /// <summary>
        /// Fecha del traspaso - JSON: "fechaTraspaso"
        /// </summary>
        [Required(ErrorMessage = "La fecha de traspaso es requerida")]
        public DateTime FechaTraspaso { get; set; }

        /// <summary>
        /// ID del almacén de origen - JSON: "almacenOrigen"
        /// </summary>
        [Required(ErrorMessage = "El almacén de origen es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El almacén de origen debe ser válido")]
        public int AlmacenOrigen { get; set; }

        /// <summary>
        /// ID del almacén de destino - JSON: "almacenDestino"
        /// </summary>
        [Required(ErrorMessage = "El almacén de destino es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El almacén de destino debe ser válido")]
        public int AlmacenDestino { get; set; }

        /// <summary>
        /// Motivo del traspaso - JSON: "motivo"
        /// </summary>
        [StringLength(2000, ErrorMessage = "El motivo no puede exceder 2000 caracteres")]
        public string Motivo { get; set; }

        /// <summary>
        /// Lista de detalles - JSON: "detalles"
        /// </summary>
        [Required(ErrorMessage = "Los detalles son requeridos")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un detalle")]
        public List<TraspasoDetalleRequest> Detalles { get; set; }

        /// <summary>
        /// Si es instantáneo, se marca como Recibido inmediatamente - JSON: "isInstantaneo"
        /// </summary>
        public bool IsInstantaneo { get; set; } = false;

        /// <summary>
        /// Lista de archivos de envío - JSON: "archivos"
        /// </summary>
        [MaxLength(10, ErrorMessage = "Máximo 10 archivos permitidos")]
        public List<CreateTraspasoArchivoRequest> Archivos { get; set; }
    }

    /// <summary>
    /// DTO para detalle de traspaso en creación
    /// </summary>
    public class TraspasoDetalleRequest
    {
        [Required(ErrorMessage = "El artículo es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del artículo debe ser válido")]
        public int IdArticulo { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }
    }

    /// <summary>
    /// DTO para marcar un traspaso como recibido
    /// </summary>
    public class RecibirTraspasoRequest
    {
        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string Observaciones { get; set; }

        /// <summary>
        /// Lista de archivos de recepción - JSON: "archivos"
        /// </summary>
        [MaxLength(10, ErrorMessage = "Máximo 10 archivos permitidos")]
        public List<CreateTraspasoArchivoRequest> Archivos { get; set; }
    }

    /// <summary>
    /// DTO para listado de traspasos - Response
    /// </summary>
    public class TraspasoListadoResponse
    {
        public int IdTraspaso { get; set; }
        public string Folio { get; set; }
        public DateTime FechaTraspaso { get; set; }
        public string AlmacenOrigenNombre { get; set; }
        public string AlmacenDestinoNombre { get; set; }
        public string Estatus { get; set; }
        public int CantidadArticulos { get; set; }
        public DateTime? FechaRecibido { get; set; }
    }

    /// <summary>
    /// DTO para detalle completo de un traspaso - Response
    /// </summary>
    public class TraspasoCompleteResponse
    {
        public int IdTraspaso { get; set; }
        public string Folio { get; set; }
        public DateTime FechaTraspaso { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public DateTime? FechaRecibido { get; set; }

        public int? IdUsuarioEnvia { get; set; }
        public string UsuarioEnviaNombre { get; set; }

        public int? IdUsuarioRecibe { get; set; }
        public string UsuarioRecibeNombre { get; set; }

        public int AlmacenOrigen { get; set; }
        public string AlmacenOrigenNombre { get; set; }

        public int AlmacenDestino { get; set; }
        public string AlmacenDestinoNombre { get; set; }

        public string Motivo { get; set; }
        public string Estatus { get; set; }

        public List<TraspasoDetalleResponse> Detalles { get; set; }
        public List<TraspasoArchivoResponse> Archivos { get; set; }  // ✅ NUEVO
    }

    /// <summary>
    /// DTO para respuesta de detalle individual de traspaso
    /// </summary>
    public class TraspasoDetalleResponse
    {
        public int IdDetalle { get; set; }
        public int IdArticulo { get; set; }
        public string NombreArticulo { get; set; }
        public int Categoria { get; set; }
        public string CategoriaNombre { get; set; }
        public decimal Cantidad { get; set; }
    }

    /// <summary>
    /// DTO para listado de detalles con información completa de artículos
    /// </summary>
    public class TraspasoDetalleListadoResponse
    {
        public int IdArticulo { get; set; }
        public string NombreArticulo { get; set; }
        public int IdCategoria { get; set; }
        public string CategoriaArticulo { get; set; }
        public decimal Cantidad { get; set; }
    }

    /// <summary>
    /// DTO para cancelar un traspaso
    /// </summary>
    public class CancelarTraspasoRequest
    {
        [Required(ErrorMessage = "El motivo de cancelación es requerido")]
        [StringLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
        public string MotivoCancelacion { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un traspaso
    /// </summary>
    public class UpdateTraspasoRequest
    {
        [Required(ErrorMessage = "La fecha de traspaso es requerida")]
        public DateTime FechaTraspaso { get; set; }

        [StringLength(2000, ErrorMessage = "El motivo no puede exceder 2000 caracteres")]
        public string Motivo { get; set; }

        public List<TraspasoDetalleUpdateRequest> Detalles { get; set; }
    }

    /// <summary>
    /// DTO para actualizar detalle de traspaso
    /// </summary>
    public class TraspasoDetalleUpdateRequest
    {
        public int? IdDetalle { get; set; }

        [Required(ErrorMessage = "El artículo es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del artículo debe ser válido")]
        public int IdArticulo { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }
    }

    /// <summary>
    /// DTO para crear archivo de traspaso
    /// ✅ CONVENCIÓN: Propiedades en PascalCase
    /// </summary>
    public class CreateTraspasoArchivoRequest : IValidatableObject
    {
        [Required(ErrorMessage = "La URL del archivo es requerida")]
        [StringLength(2048, ErrorMessage = "La URL no puede exceder 2048 caracteres")]
        public string Url { get; set; }

        [Required(ErrorMessage = "El nombre del archivo es requerido")]
        [StringLength(255, ErrorMessage = "El nombre del archivo no puede exceder 255 caracteres")]
        public string NombreOriginal { get; set; }

        /// <summary>
        /// Tipo: "Envio" o "Recepcion"
        /// </summary>
        [Required(ErrorMessage = "El tipo de archivo es requerido")]
        [RegularExpression("^(Envio|Recepcion)$", ErrorMessage = "El tipo debe ser 'Envio' o 'Recepcion'")]
        public string TipoArchivo { get; set; }

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
    /// DTO para respuesta de archivos de traspaso
    /// </summary>
    public class TraspasoArchivoResponse
    {
        public int IdArchivo { get; set; }
        public string NombreArchivo { get; set; }
        public string UrlArchivo { get; set; }
        public string TipoArchivo { get; set; }  // "Envio" o "Recepcion"
    }
}
