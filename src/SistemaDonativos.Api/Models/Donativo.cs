 using System;

namespace apiDonativos.Models
{
    public class Donativo
    {
        public int id_donativo { get; set; }
        public string folio { get; set; }
        public int id_donador { get; set; }
        public int id_almacen { get; set; }
        public DateTime fecha_recepcion { get; set; }
        public string tipo { get; set; }
        public string estatus { get; set; }
        public string observaciones { get; set; }
        public int id_usuario_registro { get; set; }
        public DateTime? fecha_registro { get; set; }
        public DateTime? fecha_actualizacion { get; set; }       
    }

    public class DonativoDetalle
    {
        public int id_donativo_detalle { get; set; }
        public int id_donativo { get; set; }
        public int id_articulo { get; set; }
        public decimal cantidad_recibida { get; set; }
        public decimal valor_unitario { get; set; }
        public string estado { get; set; }
        public string descripcion { get; set; }
        public string unidad_medida { get; set; }

        public Donativo donativo { get; set; }
    }

    public class DonativoArchivo
    {
        public int id_archivo { get; set; }
        public int id_donativo { get; set; }
        public string url { get; set; }
        public string nombre_original { get; set; }
        public DateTime? fecha_actualiza { get; set; }
        public bool activo { get; set; } = true;

        // Propiedad de navegación
        public Donativo donativo { get; set; }
    }
}
