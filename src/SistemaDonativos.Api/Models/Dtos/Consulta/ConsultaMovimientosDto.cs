using System;
using System.Collections.Generic;

namespace apiDonativos.Models.Dtos.Consulta
{
    /// <summary>
    /// DTO para consulta unificada de movimientos (entradas, salidas, traspasos)
    /// ✅ ENDPOINT: GET /api/consulta/movimientos
    /// ✅ CONVENCIÓN: Propiedades en snake_case para coincidir con BD
    /// </summary>
    public class ConsultaMovimientosRequest
    {
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_fin { get; set; }
        public string? tipo_movimiento { get; set; } = "todos"; // entrada, salida, traspaso, todos
        public string? folio { get; set; }
        public int? id_donador { get; set; }
        public int? id_beneficiario { get; set; }
        public int? id_articulo { get; set; }
        public int? id_almacen { get; set; }
        public string? estatus { get; set; }
        public int pagina { get; set; } = 1;
        public int registros_por_pagina { get; set; } = 50;
    }

    /// <summary>
    /// Respuesta de consulta de movimientos con paginación
    /// </summary>
    public class ConsultaMovimientosResponse
    {
        public bool exito { get; set; }
        public string mensaje { get; set; }
        public ConsultaMovimientosDatos data { get; set; }
        public string error { get; set; }
    }

    public class ConsultaMovimientosDatos
    {
        public int total_registros { get; set; }
        public int pagina_actual { get; set; }
        public int total_paginas { get; set; }
        public int registros_por_pagina { get; set; }
        public List<MovimientoItem> movimientos { get; set; }
    }

    public class MovimientoItem
    {
        public int id { get; set; }
        public string tipo { get; set; } // entrada, salida, traspaso
        public string folio { get; set; }
        public DateTime fecha { get; set; }
        public string descripcion { get; set; }
        public string responsable { get; set; }
        public string tipo_responsable { get; set; } // donador, beneficiario, almacen
        public string almacen_origen { get; set; }
        public string almacen_destino { get; set; }
        public int total_articulos { get; set; }
        public decimal cantidad_total { get; set; }
        public string estatus { get; set; }
        public string observaciones { get; set; }
        public List<MovimientoDetalle> detalles { get; set; }
    }

    public class MovimientoDetalle
    {
        public int id_articulo { get; set; }
        public string nombre_articulo { get; set; }
        public decimal cantidad { get; set; }
        public string unidad_medida { get; set; }
    }
}
