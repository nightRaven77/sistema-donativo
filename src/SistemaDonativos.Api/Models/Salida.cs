using System;
namespace apiDonativos.Models

{
    public class Salida
    {
        // ✅ CORREGIDO: Cambiar id_Salida a id_salida (consistencia con SQL)
        public int id_salida { get; set; }

        public string folio { get; set; }
        public DateTime fecha_salida { get; set; }
        public int id_almacen_origen { get; set; }

        // ✅ CORREGIDO: Nullable - permite salidas sin beneficiario (ej: internas)
        public int? id_beneficiario { get; set; }

        // ✅ NUEVO: Tipo de salida - solo 3 opciones: donacion, merma, baja
        public string tipo_salida { get; set; }

        // ✅ NUEVO: Campo faltante - registra el motivo de la salida
        public string motivo { get; set; }

        public string observaciones { get; set; }

        // ✅ CORREGIDO: Nullable - puede no tener usuario asignado aún
        public int? id_usuario_entrega { get; set; }

        public string estatus { get; set; }

        // ✅ CORREGIDO: Nullable - puede no tener fecha de entrega aún
        public DateTime? fecha_entrega { get; set; }

        public string recibio { get; set; }

        // ✅ CORREGIDO: Nullable - se establece automáticamente en BD
        public DateTime? fecha_registro { get; set; }

        // ✅ CORREGIDO: Nullable - se actualiza automáticamente
        public DateTime? fecha_actualizacion { get; set; }
    }

    public class SalidaDetalle
    {
        public int id_detalle { get; set; }
        public int id_salida { get; set; }
        public int id_articulo { get; set; }
        public decimal cantidad_entregada { get; set; }

        // ✅ CORREGIDO: Nullable - no siempre se registra valor unitario
        public decimal? valor_unitario { get; set; }

        public string observaciones { get; set; }
        public string unidad_medida { get; set; }

        // Propiedad de navegación
        public Salida salida { get; set; }
    }

    public class SalidasArchivo
    {
        public int id_archivo { get; set; }
        public int id_salida { get; set; }
        public string url { get; set; }
        public string nombre_original { get; set; }

        // ✅ MANTENER: fecha_actualiza (aunque no está en SQL, la agregamos después)
        public DateTime? fecha_actualiza { get; set; }

        // ✅ CORREGIDO: Default true (activo por defecto)
        public bool activo { get; set; } = true;

        // ✅ CORREGIDO: Propiedad de navegación correcta
        public Salida salida { get; set; }
    }
}
