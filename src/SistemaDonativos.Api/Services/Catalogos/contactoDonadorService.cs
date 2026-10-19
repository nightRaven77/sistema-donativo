using apiDonativos.Models.Catalogos;
using apiDonativos.Services.Base;  // ✅ FASE 3
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;  // ✅ FASE 3
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apiDonativos.Services.Catalogos
{
    public interface IContactoDonador
    {
        Task<contactoDonador> GetByIdAsync(int id);
        Task<List<contactoDonador>> GetAllAsync();
        Task<contactoDonador> GetByDonadorIdAsync(int id_donador);
        Task<int> CreateAsync(contactoDonador contacto, int id_usuario);
        Task<bool> UpdateAsync(contactoDonador contacto, int id_usuario);
        Task<bool> DeleteAsync(int id, int id_usuario);
    }

    /// <summary>
    /// Servicio para gestión de contactos de donadores
    /// ✅ FASE 3: Heredado de ServiceBase con funcionalidad completa
    /// </summary>
    public class contactoDonadorService : ServiceBase<contactoDonadorService>, IContactoDonador
    {
        public contactoDonadorService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<contactoDonadorService> logger)
            : base(context, configuration, logger)
        {
        }

        /// <summary>
        /// Obtiene el listado de todos los contactos donadores
        /// ✅ FASE 3: Logging agregado
        /// </summary>
        public async Task<List<contactoDonador>> GetAllAsync()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            LogInicioOperacion("GetAllAsync - ContactoDonadores");

            var contactos = await _context.contactos_donadores
                .AsNoTracking()
                .ToListAsync();

            stopwatch.Stop();
            LogExitoOperacion("GetAllAsync - ContactoDonadores", stopwatch.ElapsedMilliseconds, new { count = contactos.Count });

            return contactos;
        }

        /// <summary>
        /// Obtiene un contacto donador por su id
        /// ✅ FASE 3: Validación y logging agregados
        /// </summary>
        public async Task<contactoDonador> GetByIdAsync(int id)
        {
            ValidarId(id, "contacto");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            LogInicioOperacion("GetByIdAsync - ContactoDonador", new { id });

            var contacto = await _context.contactos_donadores
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.id_contacto == id);

            stopwatch.Stop();
            LogExitoOperacion("GetByIdAsync - ContactoDonador", stopwatch.ElapsedMilliseconds, new { encontrado = contacto != null });

            return contacto;
        }


        /// <summary>
        /// Obtiene el contacto donador por el id del donador
        /// </summary>
        /// <param name="id_donador"></param>
        /// <returns></returns>
        public async Task<contactoDonador> GetByDonadorIdAsync(int id_donador)
        {
            return await _context.contactos_donadores
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.id_donador == id_donador);
        }

        /// <summary>
        /// Crea un nuevo contacto donador y registra la acción en la bitácora.
        /// El interceptor manejará automáticamente fecha_creacion y fecha_actualizacion.
        /// </summary>
        public async Task<int> CreateAsync(contactoDonador contacto, int id_usuario)
        {
            if (contacto == null)
                throw new ArgumentNullException(nameof(contacto), "El contacto no puede ser nulo");

            _logger.LogInformation("Iniciando creación de contacto. Usuario: {UserId}, Donador: {DonadorId}", id_usuario, contacto.id_donador);

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);

            try
            {

                var contactoExistente = await _context.contactos_donadores
                    .AsNoTracking().FirstOrDefaultAsync(c => c.id_donador == contacto.id_donador);

                if (contactoExistente != null)
                    return contactoExistente.id_contacto;

                _context.contactos_donadores.Add(contacto);

                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Contactos",
                    submodulo = "Registro",
                    accion = "Crear",
                    descripcion = $"Se registró contacto {contacto.nombre}"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Contacto creado. ID: {Id}, Usuario: {UserId}", contacto.id_contacto, id_usuario);

                return contacto.id_contacto;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "Error BD al crear contacto. Usuario: {UserId}", id_usuario);
                throw new InvalidOperationException("Error al crear contacto: " + dbEx.InnerException?.Message, dbEx);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error inesperado. Usuario: {UserId}", id_usuario);
                throw new InvalidOperationException($"Error inesperado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza un contacto donador y registra la acción en la bitácora.
        /// El interceptor actualizará automáticamente fecha_actualizacion.
        /// </summary>
        public async Task<bool> UpdateAsync(contactoDonador contacto, int id_usuario)
        {
            if (contacto == null)
                throw new ArgumentNullException(nameof(contacto), "El contacto no puede ser nulo");

            if (contacto.id_contacto <= 0)
                throw new ArgumentException("ID de contacto inválido", nameof(contacto));

            _logger.LogInformation("Actualizando contacto. Id: {Id}, Usuario: {UserId}", contacto.id_contacto, id_usuario);

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);

            try
            {
                var contactoExistente = await _context.contactos_donadores
                    .AsNoTracking().FirstOrDefaultAsync(c => c.id_contacto == contacto.id_contacto);

                if (contactoExistente == null)
                    throw new InvalidOperationException($"El contacto con ID {contacto.id_contacto} no existe");

                // Preservar fecha_creacion
                contacto.fecha_creacion = contactoExistente.fecha_creacion;

                _context.contactos_donadores.Update(contacto);

                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Contactos",
                    submodulo = "Registro",
                    accion = "Actualizar",
                    descripcion = $"Se actualizó el contacto '{contacto.nombre}' con ID {contacto.id_contacto}"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Contacto actualizado. Id: {Id}", contacto.id_contacto);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al actualizar contacto. Id: {Id}", contacto.id_contacto);
                throw new InvalidOperationException($"Error al actualizar: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Elimina un contacto donador (marca como inactivo) y registra la acción en la bitácora.
        /// El interceptor actualizará automáticamente fecha_actualizacion.
        /// </summary>
        public async Task<bool> DeleteAsync(int id, int id_usuario)
        {
            if (id <= 0)
                throw new ArgumentException("ID inválido", nameof(id));

            _logger.LogInformation("Eliminando contacto. Id: {Id}, Usuario: {UserId}", id, id_usuario);

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);

            try
            {
                var contacto = await _context.contactos_donadores
                    .AsNoTracking().FirstOrDefaultAsync(c => c.id_contacto == id);

                if (contacto == null)
                    return false;

                _context.contactos_donadores.Remove(contacto);

                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Contactos",
                    submodulo = "Registro",
                    accion = "Eliminar",
                    descripcion = $"Se eliminó el contacto '{contacto.nombre}' con ID {contacto.id_contacto}"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Contacto eliminado. Id: {Id}", id);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al eliminar contacto. Id: {Id}", id);
                throw new InvalidOperationException($"Error al eliminar: {ex.Message}", ex);
            }
        }
    }
}
