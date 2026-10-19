using System;

namespace apiDonativos.Models.Dtos
{
    /// <summary>
    /// DTO para listado de donativos - Response
    /// ✅ CONVENCIÓN: Propiedades en PascalCase (C#) → camelCase automático en JSON
    /// </summary>
    public class DonativoListadoResponse
    {
        public int IdDonativo { get; set; }
        
        /// <summary>
        /// Folio del donativo - JSON: "folio"
        /// </summary>
        public string Folio { get; set; }

        /// <summary>
        /// Nombre del donador - JSON: "nombre"
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Fecha de recepción - JSON: "fechaRecepcion"
        /// </summary>
        public DateTime FechaRecepcion { get; set; }

        /// <summary>
        /// Tipo de donativo - JSON: "tipo"
        /// </summary>
        public string Tipo { get; set; }

        /// <summary>
        /// Estatus del donativo - JSON: "estatus"
        /// </summary>
        public string Estatus { get; set; }

        /// <summary>
        /// Cantidad de artículos - JSON: "cantidadArticulos"
        /// </summary>
        public int CantidadArticulos { get; set; }
    }
}
