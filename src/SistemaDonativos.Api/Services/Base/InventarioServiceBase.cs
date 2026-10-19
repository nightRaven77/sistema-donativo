using apiDonativos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace apiDonativos.Services.Base
{
    /// <summary>
    /// Clase base para servicios que manejan inventario
    /// Centraliza el registro de movimientos de inventario
    /// ✅ FASE 3: Ahora hereda de ServiceBase para reutilizar funcionalidad común
    /// </summary>
    public abstract class InventarioServiceBase<TService> : ServiceBase<TService> where TService : class
    {
        protected InventarioServiceBase(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<TService> logger) 
            : base(context, configuration, logger)
        {
        }

        /// <summary>
        /// Registra un movimiento de inventario
        /// </summary>
        /// <param name="id_almacen">ID del almacén</param>
        /// <param name="id_articulo">ID del artículo</param>
        /// <param name="cantidad">Cantidad del movimiento (positivo=entrada, negativo=salida)</param>
        /// <param name="tipo">Tipo de movimiento (Salida, Traspaso, Donativo, etc.)</param>
        /// <param name="fecha">Fecha del movimiento</param>
        /// <param name="id_usuario">ID del usuario que realiza el movimiento</param>
        /// <param name="id_salida">ID de salida (opcional)</param>
        /// <param name="id_traspaso">ID de traspaso (opcional)</param>
        /// <param name="id_donativo">ID de donativo (opcional)</param>
        protected void RegistrarMovimiento(
            int id_almacen, 
            int id_articulo, 
            decimal cantidad, 
            string tipo, 
            DateTime fecha, 
            int id_usuario,
            int? id_salida = null,
            int? id_traspaso = null,
            int? id_donativo = null)
        {
            var movimiento = new MovimientoInventario
            {
                id_almacen = id_almacen,
                id_articulo = id_articulo,
                cantidad = cantidad,
                tipo_movimiento = tipo,
                fecha_movimiento = fecha,
                id_usuario = id_usuario,
                id_salida = id_salida,
                id_traspaso = id_traspaso,
                id_donativo = id_donativo
            };
            
            _context.movimientos_inventario.Add(movimiento);
        }
    }
}
