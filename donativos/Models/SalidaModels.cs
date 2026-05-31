using System;
using System.Collections.Generic;

namespace donativos.Models
{
    /// <summary>
    /// DTO para beneficiario (similar a DonadorDTO)
    /// </summary>
    public class BeneficiarioDTO
    {
        public int? id_beneficiario { get; set; }
        public string codigo { get; set; }
        public string tipo { get; set; }
        public string nombre { get; set; }
        public string rfc { get; set; }
        public string telefono { get; set; }
        public string email { get; set; }
        public string direccion { get; set; }
        public string colonia { get; set; }
        public string ciudad { get; set; }
        public string estado { get; set; }
        public string codigo_postal { get; set; }
        public string notas { get; set; }
    }

    /// <summary>
    /// DTO para salida completa
    /// </summary>
    public class SalidaDTO
    {
        public int? id_salida { get; set; }
        public DateTime fecha_salida { get; set; }
        public int id_almacen_origen { get; set; }
        public int id_beneficiario { get; set; }
        public string motivo { get; set; }
        public string observaciones { get; set; }
        public string tipo_salida { get; set; } // "donacion" o "merma"
        public List<DetalleSalidaDTO> detalles { get; set; }
        
        // Propiedades de navegación (si el API las retorna)
        public BeneficiarioDTO beneficiario { get; set; }
        public AlmacenDTO almacen_origen { get; set; }
    }

    /// <summary>
    /// DTO para detalle de salida
    /// </summary>
    public class DetalleSalidaDTO
    {
        public int? id_detalle_salida { get; set; }
        public int id_articulo { get; set; }
        public int cantidad_entregada { get; set; }
        public decimal valor_unitario { get; set; }
        public string unidad_medida { get; set; }
        public string descripcion { get; set; }
        
        // Propiedad de navegación
        public ArticuloDTO articulo { get; set; }
    }

    /// <summary>
    /// DTO para inventario/stock por artículo
    /// Estructura del nuevo endpoint: GET /catalogos/articulo/stock/{idAlmacen}
    /// </summary>
    public class InventarioDTO
    {
        public int idArticulo { get; set; }
        public string nombre { get; set; }
        public int? idCategoria { get; set; }
        public string nombreCategoria { get; set; }
        public decimal stockActual { get; set; }
        public int idAlmacen { get; set; }
        public string nombreAlmacen { get; set; }
        public DateTime? fechaUltimaEntrada { get; set; }
        public DateTime? fechaUltimaSalida { get; set; }
        public bool activo { get; set; }
        public string unidadMedida { get; set; }

        // Para compatibilidad con código existente (snake_case)
        public int id_articulo => idArticulo;
        public int id_almacen => idAlmacen;
        public decimal stock_actual => stockActual;
        public string nombre_articulo => nombre;
        public string nombre_almacen => nombreAlmacen;
        public string unidad_medida => unidadMedida;
    }

    /// <summary>
    /// Respuesta genérica para operaciones de salida
    /// </summary>
    public class SalidaResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public SalidaDTO Data { get; set; }
        public string Error { get; set; }
    }
}
