using System;

namespace apiDonativos.Models
{
    public class Traspaso
    {
        public int id_traspaso { get; set; }
        public string folio { get; set; }
        public DateTime fecha_traspaso { get; set; }
        public DateTime? fecha_registro { get; set; }
        public DateTime? fecha_actualiza { get; set; }
        public DateTime? fecha_recibido { get; set; }
        
        public int? id_usuario_envia { get; set; }
        public int? id_usuario_recibe { get; set; }
        
        public int? almacen_origen { get; set; }
        public int? almacen_destino { get; set; }
        
        public string motivo { get; set; }
        public string estatus { get; set; } // Pendiente, Recibido, Cancelado
    }
}