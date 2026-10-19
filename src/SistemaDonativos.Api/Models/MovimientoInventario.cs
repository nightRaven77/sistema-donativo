using System;

namespace apiDonativos.Models
{
    /// <summary>
    /// Registra todos los movimientos de inventario (entradas y salidas)
    /// Cantidad: Positivo para entradas, Negativo para salidas
    /// </summary>
    public class MovimientoInventario
    {
        public int id_movimiento { get; set; }
        public int id_almacen { get; set; }
        public int id_articulo { get; set; }
        public decimal cantidad { get; set; } // Positivo = Entrada, Negativo = Salida
        public string tipo_movimiento { get; set; } // 'Donativo', 'Salida', 'Traspaso-Entrada', 'Traspaso-Salida', 'Ajuste'
        public DateTime fecha_movimiento { get; set; }
        public int? id_usuario { get; set; }
        public int? id_donativo { get; set; }
        public int? id_salida { get; set; }
        public int? id_traspaso { get; set; }
        public string observaciones { get; set; }
    }
}
