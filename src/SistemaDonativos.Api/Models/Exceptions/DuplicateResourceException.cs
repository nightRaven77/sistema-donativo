using System;

namespace apiDonativos.Models.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando se intenta crear un recurso que ya existe en el sistema.
    /// Proporciona información estructurada sobre el recurso duplicado para respuestas API consistentes.
    /// </summary>
    public class DuplicateResourceException : InvalidOperationException
    {
        /// <summary>
        /// ID del recurso existente
        /// </summary>
        public int? ResourceId { get; set; }

        /// <summary>
        /// Código del recurso existente (ej: BENE-000012, DON-000005)
        /// </summary>
        public string ResourceCode { get; set; }

        /// <summary>
        /// Tipo de recurso (ej: "Beneficiario", "Donador", "Articulo")
        /// </summary>
        public string ResourceType { get; set; }

        /// <summary>
        /// Campo que causó el conflicto (ej: "nombre", "codigo", "email")
        /// </summary>
        public string ConflictingField { get; set; }

        /// <summary>
        /// Crea una nueva instancia de DuplicateResourceException
        /// </summary>
        /// <param name="message">Mensaje descriptivo del error</param>
        /// <param name="resourceId">ID del recurso existente</param>
        /// <param name="resourceCode">Código del recurso existente (opcional)</param>
        /// <param name="resourceType">Tipo de recurso</param>
        /// <param name="conflictingField">Campo que causó el conflicto</param>
        public DuplicateResourceException(
            string message, 
            int? resourceId, 
            string resourceCode = null,
            string resourceType = null,
            string conflictingField = null)
            : base(message)
        {
            ResourceId = resourceId;
            ResourceCode = resourceCode;
            ResourceType = resourceType;
            ConflictingField = conflictingField;
        }
    }
}
