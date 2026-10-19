using apiDonativos.Models;
using apiDonativos.Models.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace apiDonativos.Data.Interceptors
{
    /// <summary>
    /// Interceptor que maneja automáticamente las fechas de auditoría (creación y actualización)
    /// para todas las entidades que lo requieran: Articulo, Donador, contactoDonador.
    /// 
    /// Comportamiento:
    /// - CREATE: fecha_creacion = NOW, fecha_actualizacion = NOW
    /// - UPDATE: fecha_creacion = preservada, fecha_actualizacion = NOW
    /// - DELETE (soft delete): fecha_creacion = preservada, fecha_actualizacion = NOW, activo = false
    /// </summary>
    public class AuditInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var dbContext = eventData.Context;
            if (dbContext == null)
                return base.SavingChangesAsync(eventData, result, cancellationToken);

            var entries = dbContext.ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                switch (entry.Entity)
                {
                    case Articulo articulo:
                        HandleArticuloAudit(entry, articulo);
                        break;

                    case Donador donador:
                        HandleDonadorAudit(entry, donador);
                        break;

                    case contactoDonador contacto:
                        HandleContactoDonadorAudit(entry, contacto);
                        break;

                    case Donativo donativo:
                        HandleDonativoAudit(entry, donativo);
                        break;

                    case Salida salida:
                        HandleSalidaAudit(entry, salida);
                        break;

                    case Traspaso traspaso:
                        HandleTraspasoAudit(entry, traspaso);
                        break;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        /// <summary>
        /// Maneja auditoría para la entidad Articulo.
        /// </summary>
        private static void HandleArticuloAudit(EntityEntry entry, Articulo articulo)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    articulo.fecha_creacion = DateTime.Now;
                    articulo.fecha_actualizacion = DateTime.Now;
                    break;

                case EntityState.Modified:
                    articulo.fecha_actualizacion = DateTime.Now;
                    break;

                case EntityState.Deleted:
                    // Soft delete: no permitir eliminación física
                    entry.State = EntityState.Modified;
                    articulo.activo = false;
                    articulo.fecha_actualizacion = DateTime.Now;
                    break;
            }
        }

        /// <summary>
        /// Maneja auditoría para la entidad Donador.
        /// </summary>
        private static void HandleDonadorAudit(EntityEntry entry, Donador donador)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    donador.fecha_creacion = DateTime.Now;
                    donador.fecha_actualizacion = DateTime.Now;
                    donador.activo = true;
                    break;

                case EntityState.Modified:
                    donador.fecha_actualizacion = DateTime.Now;
                    break;

                case EntityState.Deleted:
                    // Soft delete: marcar como inactivo
                    entry.State = EntityState.Modified;
                    donador.activo = false; 
                    donador.fecha_actualizacion = DateTime.Now;

                    break;
            }
        }

        /// <summary>
        /// Maneja auditoría para la entidad contactoDonador.
        /// </summary>
        private static void HandleContactoDonadorAudit(EntityEntry entry, contactoDonador contacto)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    contacto.fecha_creacion = DateTime.Now;
                    contacto.fecha_actualizacion = DateTime.Now;
                    break;

                case EntityState.Modified:
                    contacto.fecha_actualizacion = DateTime.Now;
                    break;

                case EntityState.Deleted:
                    // Soft delete: marcar como inactivo
                    entry.State = EntityState.Modified;
                    contacto.activo = false;
                    contacto.fecha_actualizacion = DateTime.Now;
                    break;
            }
        }

        /// <summary>
        /// Maneja auditoría para la entidad Donativo.
        /// ? ACTUALIZADO: NO modifica el folio al hacer soft delete
        /// </summary>
        private static void HandleDonativoAudit(EntityEntry entry, Donativo donativo)
        {
         switch(entry.State)
         {
            case EntityState.Added:
                donativo.fecha_registro = DateTime.Now;
                donativo.fecha_actualizacion = DateTime.Now;
                break;
            case EntityState.Modified:
                donativo.fecha_actualizacion = DateTime.Now;
                break;
            case EntityState.Deleted:
                // ? Soft delete: Solo cambia estatus, NO el folio
                entry.State = EntityState.Modified;
                donativo.estatus = "Cancelado"; // Cambiar estatus
                donativo.fecha_actualizacion = DateTime.Now;

                // ? NO HACER ESTO:
                // donativo.folio = $"{donativo.folio}-CANCELADO";
                break;
            }
        }

        /// <summary>
        /// Maneja auditoría para la entidad Salida.
        /// ? ACTUALIZADO: NO modifica el folio al hacer soft delete
        /// </summary>
        private static void HandleSalidaAudit(EntityEntry entry, Salida salida)
        {
            switch(entry.State)
            {
                case EntityState.Added:
                    salida.fecha_registro = DateTime.Now;
                    salida.fecha_actualizacion = DateTime.Now;
                    break;

                case EntityState.Modified:
                    salida.fecha_actualizacion = DateTime.Now;
                    break;

                case EntityState.Deleted:
                    // ? Soft delete: Solo cambia estatus, NO el folio
                    entry.State = EntityState.Modified;
                    salida.estatus = "Cancelada";
                    salida.fecha_actualizacion = DateTime.Now;

                    // ? NO HACER ESTO:
                    // salida.folio = $"{salida.folio}-CANCELADO";
                    break;
            }
        }

        /// <summary>
        /// Maneja auditoría para la entidad Traspaso.
        /// ? ACTUALIZADO: NO modifica el folio al hacer soft delete
        /// </summary>
        private static void HandleTraspasoAudit(EntityEntry entry, Traspaso traspaso)
        {
            switch(entry.State)
            {
                case EntityState.Added:
                    traspaso.fecha_registro = DateTime.Now;
                    traspaso.fecha_actualiza = DateTime.Now;
                    break;

                case EntityState.Modified:
                    traspaso.fecha_actualiza = DateTime.Now;
                    break;

                case EntityState.Deleted:
                    // ? Soft delete: Solo cambia estatus, NO el folio
                    entry.State = EntityState.Modified;
                    traspaso.estatus = "Cancelado";
                    traspaso.fecha_actualiza = DateTime.Now;

                    // ? NO HACER ESTO:
                    // traspaso.folio = $"{traspaso.folio}-CANCELADO";
                    break;
            }
        }
    }
}