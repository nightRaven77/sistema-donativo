using System;
using System.Collections.Generic;

namespace apiDonativos.Models.Dtos
{
    /// <summary>
    /// DTO para archivos de donativo - Response
    /// ✅ CONVENCIÓN: Propiedades en PascalCase
    /// </summary>
    public class DonativoArchivosResponse
    {
        public int IdArchivo { get; set; }
        public int IdDonativo { get; set; }
        public string NombreArchivo { get; set; }
        public string UrlArchivo { get; set; }
    }

    /// <summary>
    /// DTO para detalle de donativo - Response
    /// </summary>
    public class DonativoDetalleResponse
    {
        public int IdDonativoDetalle { get; set; }
        public int IdArticulo { get; set; }
        public string NombreArticulo { get; set; }
        public decimal CantidadRecibida { get; set; }
        public decimal ValorUnitario { get; set; }
        public string Estado { get; set; }
        public int Categoria { get; set; }
        public string UnidadMedida { get; set; }
    }

    /// <summary>
    /// DTO para donador - Response
    /// </summary>
    public class DonadorResponse
    {
        public int IdDonador { get; set; }
        public string Tipo { get; set; }
        public string Nombre { get; set; }
        public string RFC { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public string Colonia { get; set; }
        public string Ciudad { get; set; }
        public string Estado { get; set; }
        public string CodigoPostal { get; set; }
        public string Notas { get; set; }
    }

    /// <summary>
    /// DTO para contacto - Response
    /// </summary>
    public class ContactoResponse
    {
        public int IdContacto { get; set; }
        public string Nombre { get; set; }
        public string Puesto { get; set; }
        public string Telefono { get; set; }
        public string Extension { get; set; }
        public string Celular { get; set; }
        public string Email { get; set; }
        public string HorarioContacto { get; set; }
        public string MedioPreferido { get; set; }
        public bool EsPrincipal { get; set; }
        public string Notas { get; set; }
        public bool Activo { get; set; }
    }

    /// <summary>
    /// DTO para respuesta completa de donativo - Response
    /// ✅ CONVENCIÓN: Propiedades en PascalCase
    /// </summary>
    public class DonativoCompleteResponse
    {
        public int IdDonativo { get; set; }
        public int IdDonador { get; set; }
        public string NombreDonador { get; set; }
        public int IdAlmacen { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public string Tipo { get; set; }
        public string Estatus { get; set; }
        public string Observaciones { get; set; }
        public List<DonativoDetalleResponse> Detalles { get; set; }
        public DonadorResponse Donador { get; set; }
        public ContactoResponse Contacto { get; set; }
        public List<DonativoArchivosResponse> Archivos { get; set; } = null;
    }
}
