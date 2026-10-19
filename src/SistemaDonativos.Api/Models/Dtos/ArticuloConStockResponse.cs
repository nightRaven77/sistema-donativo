using System;

namespace apiDonativos.Models.Dtos
{
    /// <summary>
    /// DTO para artículo con información de stock actual por almacén
    /// ✅ CONVENCIÓN: Propiedades en PascalCase (C#) → camelCase automático en JSON
    /// </summary>
    public class ArticuloConStockResponse
    {
        /// <summary>
        /// ID del artículo - JSON: "idArticulo"
        /// </summary>
        public int IdArticulo { get; set; }

        /// <summary>
        /// Nombre del artículo - JSON: "nombre"
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// ID de categoría - JSON: "idCategoria"
        /// </summary>
        public int IdCategoria { get; set; }

        /// <summary>
        /// Nombre de la categoría - JSON: "nombreCategoria"
        /// </summary>
        public string NombreCategoria { get; set; }

        /// <summary>
        /// Stock actual en el almacén - JSON: "stockActual"
        /// </summary>
        public decimal StockActual { get; set; }

        /// <summary>
        /// ID del almacén - JSON: "idAlmacen"
        /// </summary>
        public int IdAlmacen { get; set; }

        /// <summary>
        /// Nombre del almacén - JSON: "nombreAlmacen"
        /// </summary>
        public string NombreAlmacen { get; set; }

        /// <summary>
        /// Fecha última entrada - JSON: "fechaUltimaEntrada"
        /// </summary>
        public DateTime? FechaUltimaEntrada { get; set; }

        /// <summary>
        /// Fecha última salida - JSON: "fechaUltimaSalida"
        /// </summary>
        public DateTime? FechaUltimaSalida { get; set; }

        /// <summary>
        /// Indica si el artículo está activo - JSON: "activo"
        /// </summary>
        public bool Activo { get; set; }
    }
}
