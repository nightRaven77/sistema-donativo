using System;
using System.Collections.Generic;

namespace apiDonativos.Models.Dtos.Consulta
{
    /// <summary>
    /// DTO para obtener detalle completo de un movimiento específico
    /// ✅ ENDPOINT: GET /api/consulta/movimientos/{tipo}/{id}
    /// ✅ CONVENCIÓN: Propiedades en snake_case para coincidir con BD
    /// </summary>
    public class DetalleMovimientoResponse
    {
        public bool exito { get; set; }
        public string mensaje { get; set; }
        public DetalleMovimientoDatos data { get; set; }
        public string error { get; set; }
    }

    public class DetalleMovimientoDatos
    {
        public string tipo { get; set; } // entrada, salida, traspaso
        public int id { get; set; }
        public string folio { get; set; }
        public DateTime fecha { get; set; }
        public AlmacenDetalleInfo almacen_origen { get; set; }
        public AlmacenDetalleInfo almacen_destino { get; set; }
        public ResponsableInfo responsable { get; set; }
        public string estatus { get; set; }
        public string observaciones { get; set; }
        public string tipo_movimiento { get; set; }
        public List<ArticuloDetalle> detalles { get; set; }
        public TotalesMovimiento totales { get; set; }
        public List<ArchivoAdjunto> archivos_adjuntos { get; set; }
        public UsuarioInfo usuario_registro { get; set; }
        public DateTime fecha_registro { get; set; }
    }

    public class AlmacenDetalleInfo
    {
        public int id_almacen { get; set; }
        public string nombre { get; set; }
        public string ubicacion { get; set; }
    }

    public class ResponsableInfo
    {
        public int id { get; set; }
        public string tipo { get; set; } // donador, beneficiario, almacen
        public string codigo { get; set; }
        public string nombre { get; set; }
        public string telefono { get; set; }
        public string email { get; set; }
        public string direccion { get; set; }
    }

    public class ArticuloDetalle
    {
        public int id_detalle { get; set; }
        public int id_articulo { get; set; }
        public string nombre_articulo { get; set; }
        public string categoria { get; set; }
        public decimal cantidad { get; set; }
        public string unidad_medida { get; set; }
        public decimal? valor_unitario { get; set; }
        public decimal? valor_total { get; set; }
        public string descripcion { get; set; }
    }

    public class TotalesMovimiento
    {
        public int total_articulos { get; set; }
        public decimal cantidad_total { get; set; }
        public decimal valor_total { get; set; }
    }

    public class ArchivoAdjunto
    {
        public int id_archivo { get; set; }
        public string nombre_original { get; set; }
        public string url { get; set; }
        public long tamano { get; set; }
    }

    public class UsuarioInfo
    {
        public int id { get; set; }
        public string nombre { get; set; }
    }
}
