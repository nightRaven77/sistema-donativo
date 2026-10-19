using System.Collections.Generic;

namespace apiDonativos.Models.Dtos
{
    /// <summary>
    /// DTO para listado de detalles de donativo - Response
    /// ✅ CONVENCIÓN: Propiedades en PascalCase
    /// </summary>
    public class DonativoDetalleListadoResponse
    {
        public int IdArticulo { get; set; }
        public string NombreArticulo { get; set; }
        public int IdCategoria { get; set; }
        public string CategoriaArticulo { get; set; }
        public string UnidadMedida { get; set; }
        public string Estado { get; set; }
        public decimal CantidadRecibida { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal ValorTotal { get; set; }
    }

    /// <summary>
    /// DTO para archivo de detalle de donativo - Response
    /// </summary>
    public class DonativoDetalleArchivoResponse
    {
        public int IdArchivo { get; set; }
        public int IdDonativo { get; set; }
        public string NombreArchivo { get; set; }
        public string UrlArchivo { get; set; }
    }

    /// <summary>
    /// DTO para detalle completo de donativo - Response
    /// </summary>
    public class DonativoDetalleCompleteResponse
    {
        public int IdArticulo { get; set; }
        public string NombreArticulo { get; set; }
        public int IdCategoria { get; set; }
        public string CategoriaArticulo { get; set; }
        public string UnidadMedida { get; set; }
        public string Estado { get; set; }
        public decimal CantidadRecibida { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal ValorTotal { get; set; }
        public List<DonativoDetalleArchivoResponse> Archivos { get; set; }
    }

    /// <summary>
    /// DTO para detalles con archivos - Response
    /// </summary>
    public class DonativoDetalleConArchivosResponse
    {
        public List<DonativoDetalleListadoResponse> Detalles { get; set; }
        public List<DonativoDetalleArchivoResponse> Archivos { get; set; }
    }
}
