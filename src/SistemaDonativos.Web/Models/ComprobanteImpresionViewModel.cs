namespace donativos.Models
{
    public class ComprobanteImpresionViewModel
    {
        public string Folio { get; set; } = "S/F";
        public System.DateTime? Fecha { get; set; }
        public string Almacen { get; set; } = "Almacén Central";
        public string AlmacenDestino { get; set; } = "";
        public string Responsable { get; set; } = "N/A";
        public string RFC { get; set; } = "N/A";
        public string Observaciones { get; set; } = "Ninguna";
        public string UsuarioRegistro { get; set; } = "Sistema";
        public string Tipo { get; set; } = "Especie";
        public string Estatus { get; set; } = "Registrado";
        public System.Collections.Generic.List<ComprobanteDetalleItem> Detalles { get; set; } = new();
    }

    public class ComprobanteDetalleItem
    {
        public string Categoria { get; set; } = "General";
        public string Articulo { get; set; } = "Sin descripción";
        public string UnidadMedida { get; set; } = "Pieza";
        public decimal Cantidad { get; set; } = 1;
    }
}
