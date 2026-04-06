using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace donativos.Models
{
    // ============================================================
    // MODELOS PARA CONSULTA UNIFICADA DE MOVIMIENTOS
    // ============================================================

    /// <summary>
    /// DTO para un movimiento individual en la consulta unificada
    /// </summary>
    public class MovimientoConsultaDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("tipo")]
        public string Tipo { get; set; }  // "entrada", "salida", "traspaso"

        [JsonProperty("folio")]
        public string Folio { get; set; }

        [JsonProperty("fecha")]
        public DateTime Fecha { get; set; }

        [JsonProperty("descripcion")]
        public string Descripcion { get; set; }

        [JsonProperty("responsable")]
        public string Responsable { get; set; }

        [JsonProperty("tipo_responsable")]
        public string TipoResponsable { get; set; }  // "donador", "beneficiario", "almacen"

        [JsonProperty("almacen_origen")]
        public string AlmacenOrigen { get; set; }

        [JsonProperty("almacen_destino")]
        public string AlmacenDestino { get; set; }

        [JsonProperty("total_articulos")]
        public int TotalArticulos { get; set; }

        [JsonProperty("cantidad_total")]
        public decimal CantidadTotal { get; set; }

        [JsonProperty("estatus")]
        public string Estatus { get; set; }

        [JsonProperty("observaciones")]
        public string Observaciones { get; set; }

        [JsonProperty("detalles")]
        public List<DetalleMovimientoDTO> Detalles { get; set; }
    }

    /// <summary>
    /// Detalle de artículos en un movimiento
    /// </summary>
    public class DetalleMovimientoDTO
    {
        [JsonProperty("id_articulo")]
        public int IdArticulo { get; set; }

        [JsonProperty("nombre_articulo")]
        public string NombreArticulo { get; set; }

        [JsonProperty("cantidad")]
        public decimal Cantidad { get; set; }

        [JsonProperty("unidad_medida")]
        public string UnidadMedida { get; set; }
    }

    /// <summary>
    /// Respuesta paginada para consulta de movimientos
    /// </summary>
    public class ConsultaMovimientosResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public ConsultaMovimientosData Data { get; set; }
        public string Error { get; set; }
    }

    public class ConsultaMovimientosData
    {
        [JsonProperty("total_registros")]
        public int TotalRegistros { get; set; }

        [JsonProperty("pagina_actual")]
        public int PaginaActual { get; set; }

        [JsonProperty("total_paginas")]
        public int TotalPaginas { get; set; }

        [JsonProperty("registros_por_pagina")]
        public int RegistrosPorPagina { get; set; }

        [JsonProperty("movimientos")]
        public List<MovimientoConsultaDTO> Movimientos { get; set; }
    }

    // ============================================================
    // MODELOS PARA INVENTARIO EN TIEMPO REAL
    // ============================================================

    /// <summary>
    /// Artículo con información de stock en un almacén específico
    /// </summary>
    public class ArticuloInventarioDTO
    {
        [JsonProperty("id_articulo")]
        public int IdArticulo { get; set; }

        [JsonProperty("codigo")]
        public string Codigo { get; set; }

        [JsonProperty("nombre")]
        public string Nombre { get; set; }

        [JsonProperty("descripcion")]
        public string Descripcion { get; set; }

        [JsonProperty("categoria")]
        public CategoriaSimpleDTO Categoria { get; set; }

        [JsonProperty("unidad_medida")]
        public string UnidadMedida { get; set; }

        [JsonProperty("stock_actual")]
        public decimal StockActual { get; set; }

        [JsonProperty("entradas_totales")]
        public decimal EntradasTotales { get; set; }

        [JsonProperty("salidas_totales")]
        public decimal SalidasTotales { get; set; }

        [JsonProperty("fecha_ultima_entrada")]
        public DateTime? FechaUltimaEntrada { get; set; }

        [JsonProperty("fecha_ultima_salida")]
        public DateTime? FechaUltimaSalida { get; set; }

        [JsonProperty("ultima_actualizacion")]
        public DateTime? UltimaActualizacion { get; set; }

        [JsonProperty("movimientos_recientes")]
        public int MovimientosRecientes { get; set; }

        [JsonProperty("alerta_stock_bajo")]
        public bool AlertaStockBajo { get; set; }

        [JsonProperty("activo")]
        public bool Activo { get; set; }
    }

    public class CategoriaSimpleDTO
    {
        [JsonProperty("id")]
        public int IdCategoria { get; set; }

        [JsonProperty("nombre")]
        public string Nombre { get; set; }
    }

    /// <summary>
    /// Respuesta de inventario por almacén
    /// </summary>
    public class InventarioAlmacenResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public InventarioAlmacenData Data { get; set; }
        public string Error { get; set; }
    }

    public class InventarioAlmacenData
    {
        [JsonProperty("almacen")]
        public AlmacenInfoDTO Almacen { get; set; }

        [JsonProperty("articulos")]
        public List<ArticuloInventarioDTO> Articulos { get; set; }

        [JsonProperty("resumen_por_categoria")]
        public List<ResumenCategoriaDTO> ResumenPorCategoria { get; set; }

        // Propiedades calculadas
        public int TotalArticulos => Articulos?.Count ?? 0;
        public int TotalArticulosConStock => Articulos?.Where(a => a.StockActual > 0).Count() ?? 0;
    }

    public class AlmacenInfoDTO
    {
        [JsonProperty("id_almacen")]
        public int IdAlmacen { get; set; }

        [JsonProperty("nombre")]
        public string Nombre { get; set; }

        [JsonProperty("ubicacion")]
        public string Ubicacion { get; set; }

        [JsonProperty("responsable")]
        public string Responsable { get; set; }
    }

    public class ResumenCategoriaDTO
    {
        [JsonProperty("categoria")]
        public string Categoria { get; set; }

        [JsonProperty("total_articulos")]
        public int TotalArticulos { get; set; }

        [JsonProperty("total_stock")]
        public decimal TotalStock { get; set; }

        // Para compatibilidad
        public int IdCategoria { get; set; }
        public string NombreCategoria => Categoria;
        public int ArticulosConStock { get; set; }
        public decimal StockTotal => TotalStock;
    }

    // ============================================================
    // MODELOS PARA DETALLE DE MOVIMIENTO ESPECÍFICO
    // ============================================================

    /// <summary>
    /// Detalle completo de un movimiento específico
    /// </summary>
    public class DetalleMovimientoResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public DetalleMovimientoCompleto Data { get; set; }
        public string Error { get; set; }
    }

    public class DetalleMovimientoCompleto
    {
        [JsonProperty("tipo")]
        public string Tipo { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("folio")]
        public string Folio { get; set; }

        [JsonProperty("fecha")]
        public DateTime Fecha { get; set; }

        [JsonProperty("almacen_origen")]
        public AlmacenBasicoDTO AlmacenOrigen { get; set; }

        [JsonProperty("almacen_destino")]
        public AlmacenBasicoDTO AlmacenDestino { get; set; }

        [JsonProperty("responsable")]
        public ResponsableDTO Responsable { get; set; }

        [JsonProperty("estatus")]
        public string Estatus { get; set; }

        [JsonProperty("observaciones")]
        public string Observaciones { get; set; }

        [JsonProperty("tipo_movimiento")]
        public string TipoMovimiento { get; set; }

        [JsonProperty("detalles")]
        public List<ArticuloDetalleDTO> Detalles { get; set; }

        [JsonProperty("totales")]
        public TotalesDTO Totales { get; set; }

        [JsonProperty("archivos_adjuntos")]
        public List<ArchivoAdjuntoDTO> ArchivosAdjuntos { get; set; }

        [JsonProperty("usuario_registro")]
        public UsuarioSimpleDTO UsuarioRegistro { get; set; }

        [JsonProperty("fecha_registro")]
        public DateTime FechaRegistro { get; set; }
    }

    public class AlmacenBasicoDTO
    {
        [JsonProperty("id_almacen")]
        public int IdAlmacen { get; set; }

        [JsonProperty("nombre")]
        public string Nombre { get; set; }

        [JsonProperty("ubicacion")]
        public string Ubicacion { get; set; }
    }

    public class ResponsableDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("tipo")]
        public string Tipo { get; set; }  // "donador", "beneficiario"

        [JsonProperty("codigo")]
        public string Codigo { get; set; }

        [JsonProperty("nombre")]
        public string Nombre { get; set; }

        [JsonProperty("telefono")]
        public string Telefono { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("direccion")]
        public string Direccion { get; set; }
    }

    public class ArticuloDetalleDTO
    {
        [JsonProperty("id_detalle")]
        public int IdDetalle { get; set; }

        [JsonProperty("id_articulo")]
        public int IdArticulo { get; set; }

        [JsonProperty("nombre_articulo")]
        public string NombreArticulo { get; set; }

        [JsonProperty("categoria")]
        public string Categoria { get; set; }

        [JsonProperty("cantidad")]
        public decimal Cantidad { get; set; }

        [JsonProperty("unidad_medida")]
        public string UnidadMedida { get; set; }

        [JsonProperty("valor_unitario")]
        public decimal ValorUnitario { get; set; }

        [JsonProperty("valor_total")]
        public decimal ValorTotal { get; set; }

        [JsonProperty("descripcion")]
        public string Descripcion { get; set; }
    }

    public class TotalesDTO
    {
        [JsonProperty("total_articulos")]
        public int TotalArticulos { get; set; }

        [JsonProperty("cantidad_total")]
        public decimal CantidadTotal { get; set; }

        [JsonProperty("valor_total")]
        public decimal ValorTotal { get; set; }
    }

    public class ArchivoAdjuntoDTO
    {
        [JsonProperty("id_archivo")]
        public int IdArchivo { get; set; }

        [JsonProperty("nombre_original")]
        public string NombreOriginal { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("ruta_archivo")]
        public string RutaArchivo { get; set; }

        [JsonProperty("tipo_mime")]
        public string TipoMime { get; set; }

        [JsonProperty("tamano")]
        public long Tamano { get; set; }

        [JsonProperty("fecha_subida")]
        public DateTime? FechaSubida { get; set; }
    }

    public class UsuarioSimpleDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nombre")]
        public string Nombre { get; set; }
    }

    // ============================================================
    // MODELOS PARA ESTADÍSTICAS DE DASHBOARD
    // ============================================================

    /// <summary>
    /// Estadísticas del inventario para el dashboard
    /// </summary>
    public class EstadisticasInventarioResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public EstadisticasInventarioData Data { get; set; }
        public string Error { get; set; }
    }

    public class EstadisticasInventarioData
    {
        [JsonProperty("almacen")]
        public AlmacenBasicoSimpleDTO Almacen { get; set; }

        [JsonProperty("resumen")]
        public ResumenInventarioDTO Resumen { get; set; }

        [JsonProperty("movimientos_hoy")]
        public MovimientosDelDiaDTO MovimientosHoy { get; set; }

        [JsonProperty("movimientos_este_mes")]
        public MovimientosDelMesDTO MovimientosEsteMes { get; set; }

        [JsonProperty("top_5_articulos_mas_stock")]
        public List<ArticuloStockDTO> Top5ArticulosMasStock { get; set; }

        [JsonProperty("top_5_articulos_menos_stock")]
        public List<ArticuloStockDTO> Top5ArticulosMenosStock { get; set; }

        [JsonProperty("categorias_mas_movimiento")]
        public List<CategoriaMovimientoDTO> CategoriasMasMovimiento { get; set; }
    }

    public class AlmacenBasicoSimpleDTO
    {
        [JsonProperty("id_almacen")]
        public int IdAlmacen { get; set; }

        [JsonProperty("nombre")]
        public string Nombre { get; set; }
    }

    public class ResumenInventarioDTO
    {
        [JsonProperty("total_articulos_catalogo")]
        public int TotalArticulosCatalogo { get; set; }

        [JsonProperty("articulos_con_stock")]
        public int ArticulosConStock { get; set; }

        [JsonProperty("articulos_sin_stock")]
        public int ArticulosSinStock { get; set; }

        [JsonProperty("stock_total_general")]
        public decimal StockTotalGeneral { get; set; }

        [JsonProperty("alertas_stock_bajo")]
        public int AlertasStockBajo { get; set; }
    }

    public class MovimientosDelDiaDTO
    {
        [JsonProperty("entradas")]
        public int Entradas { get; set; }

        [JsonProperty("salidas")]
        public int Salidas { get; set; }

        [JsonProperty("traspasos")]
        public int Traspasos { get; set; }
    }

    public class MovimientosDelMesDTO
    {
        [JsonProperty("entradas")]
        public int Entradas { get; set; }

        [JsonProperty("salidas")]
        public int Salidas { get; set; }

        [JsonProperty("traspasos")]
        public int Traspasos { get; set; }

        [JsonProperty("traspasos_recibidos")]
        public int TraspasosRecibidos { get; set; }

        [JsonProperty("traspasos_pendientes")]
        public int TraspasosPendientes { get; set; }
    }

    public class ArticuloStockDTO
    {
        [JsonProperty("id_articulo")]
        public int IdArticulo { get; set; }

        [JsonProperty("nombre")]
        public string Nombre { get; set; }

        [JsonProperty("stock")]
        public decimal Stock { get; set; }

        [JsonProperty("unidad")]
        public string Unidad { get; set; }
    }

    public class CategoriaMovimientoDTO
    {
        [JsonProperty("categoria")]
        public string Categoria { get; set; }

        [JsonProperty("entradas")]
        public int Entradas { get; set; }

        [JsonProperty("salidas")]
        public int Salidas { get; set; }
    }
}
