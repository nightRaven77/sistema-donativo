using System;
using System.Collections.Generic;

namespace donativos.Models
{
    /// <summary>
    /// DTO para traspaso entre almacenes
    /// </summary>
    public class TraspasoDTO
    {
        public int? id_traspaso { get; set; }
        public DateTime fecha_traspaso { get; set; }
        public int almacen_origen { get; set; }
        public int almacen_destino { get; set; }
        public string motivo { get; set; }
        public string observaciones { get; set; }
        public bool is_instantaneo { get; set; }
        public string estado { get; set; } // "pendiente", "aprobado", "cancelado"
        public int? usuario_envia { get; set; }
        public int? usuario_recibe { get; set; }
        public DateTime? fecha_recepcion { get; set; }
        public string motivo_cancelacion { get; set; }
        public List<DetalleTraspasoDTO> detalles { get; set; }
        
        // Propiedades de navegación
        public AlmacenDTO almacen_origen_info { get; set; }
        public AlmacenDTO almacen_destino_info { get; set; }
    }

    /// <summary>
    /// DTO para detalle de traspaso
    /// </summary>
    public class DetalleTraspasoDTO
    {
        public int? id_detalle_traspaso { get; set; }
        public int id_articulo { get; set; }
        public int cantidad { get; set; }
        public string unidad_medida { get; set; }
        public string descripcion { get; set; }
        
        // Propiedad de navegación
        public ArticuloDTO articulo { get; set; }
    }

    /// <summary>
    /// DTO para almacén
    /// </summary>
    public class AlmacenDTO
    {
        public int id_almacen { get; set; }
        public string nombre { get; set; }
        public string ubicacion { get; set; }
        public string responsable { get; set; }
        public string telefono { get; set; }
    }

    /// <summary>
    /// Respuesta para operaciones de traspaso
    /// </summary>
    public class TraspasoResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public TraspasoDTO Data { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// Request para recibir traspaso
    /// </summary>
    public class RecibirTraspasoRequest
    {
        public string observaciones { get; set; }
    }

    /// <summary>
    /// Request para cancelar traspaso
    /// </summary>
    public class CancelarTraspasoRequest
    {
        public string motivo_cancelacion { get; set; }
    }
}
