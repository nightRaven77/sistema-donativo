using apiDonativos.Models.Catalogos;
using apiDonativos.Models.Exceptions;  // ✅ NUEVO: Para DuplicateResourceException
using apiDonativos.Helpers;
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
    public interface IDonador
    {
        Task<Donador> GetByIdAsync(int id);
        Task<List<Donador>> GetAllAsync();
        Task<List<Donador>> GetByNameAsync(string nombre);
        Task<int> CreateAsync(Donador donador, int id_usuario);
        Task<bool> UpdateAsync(Donador donador, int id_usuario);
        Task<bool> DeleteAsync(int id, int id_usuario);
    }

    /// <summary>
    /// Servicio para gestión de donadores
    /// ✅ FASE 3: Heredado de ServiceBase con timeout configurable
    /// </summary>
    public class DonadorService : ServiceBase<DonadorService>, IDonador
    {
        public DonadorService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<DonadorService> logger)
            : base(context, configuration, logger)
        {
        }

        public async Task<Donador> GetByIdAsync(int id)
        {
            ValidarId(id, "donador");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            LogInicioOperacion("GetByIdAsync - Donador", new { id });

            var donador = await _context.Donadores
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.id_donador == id);

            stopwatch.Stop();
            LogExitoOperacion("GetByIdAsync - Donador", stopwatch.ElapsedMilliseconds, new { encontrado = donador != null });

            return donador;
        }

        public async Task<List<Donador>> GetAllAsync()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            LogInicioOperacion("GetAllAsync - Donadores");

            var donadores = await _context.Donadores
                .AsNoTracking()
                .ToListAsync();

            stopwatch.Stop();
            LogExitoOperacion("GetAllAsync - Donadores", stopwatch.ElapsedMilliseconds, new { count = donadores.Count });

            return donadores;
        }

        public async Task<List<Donador>> GetByNameAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return new List<Donador>();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            LogInicioOperacion("GetByNameAsync - Donadores", new { nombre });

            var donadores = await _context.Donadores
                .Where(d => d.nombre.ToLower().Contains(nombre.ToLower()))
                .AsNoTracking()
                .ToListAsync();

            stopwatch.Stop();
            LogExitoOperacion("GetByNameAsync - Donadores", stopwatch.ElapsedMilliseconds, new { count = donadores.Count });

            return donadores;
        }

        /// <summary>
        /// Crea un nuevo donador con código autogenerado y registra en bitácora.
        /// El interceptor manejará automáticamente fecha_creacion y fecha_actualizacion.
        /// </summary>
        public async Task<int> CreateAsync(Donador donador, int id_usuario)
        {
            // ✅ FASE 3: Validaciones estandarizadas
            ValidarObjetoRequerido(donador, nameof(donador));
            ValidarId(id_usuario, "usuario");
            ValidarCadenaRequerida(donador.nombre, "nombre del donador", longitudMinima: 2, longitudMaxima: 200);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            LogInicioOperacion("CreateAsync - Donador", new { donador.nombre, id_usuario });

            using var transaction = await _context.Database.BeginTransactionAsync();
            ConfigurarTimeout();  // ✅ FASE 3: Usa método base

            try
            {

                var existeDonador = await _context.Donadores
                    .Where(d => d.nombre.ToLower() == donador.nombre.Trim().ToLower()
                    )
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (existeDonador != null)
                {
                    // ✅ MEJORA: Lanzar excepción estructurada en lugar de retornar ID silenciosamente
                    throw new DuplicateResourceException(
                        message: $"Ya existe un donador con el nombre '{donador.nombre}'",
                        resourceId: existeDonador.id_donador,
                        resourceCode: existeDonador.codigo,
                        resourceType: "Donador",
                        conflictingField: "nombre"
                    );
                }

                // Generar código DON-XXXXXX
                string codigo = await GenerarCodigoDonadorAsync();
                donador.codigo = codigo;

                _context.Donadores.Add(donador);

                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Donadores",
                    submodulo = "Registro",
                    accion = "Crear",
                    descripcion = $"Se registró donador {codigo} - {donador.nombre}"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                stopwatch.Stop();
                LogExitoOperacion("CreateAsync - Donador", stopwatch.ElapsedMilliseconds, new { codigo, donador.id_donador });

                return donador.id_donador;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                LogErrorOperacion("CreateAsync - Donador", dbEx, new { id_usuario });

                var errorMessage = "Error al crear donador";
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                if (innerMessage.Contains("UNIQUE")) errorMessage = "Ya existe un donador con esos datos";
                else errorMessage = $"Error de base de datos: {innerMessage}";

                throw new InvalidOperationException(errorMessage, dbEx);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                LogErrorOperacion("CreateAsync - Donador", ex, new { id_usuario });
                throw new InvalidOperationException($"Error inesperado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Genera un código secuencial tipo DON-000001, DON-000002, etc.
        /// ✅ M2: Refactorizado para usar FolioGenerator
        /// </summary>
        private async Task<string> GenerarCodigoDonadorAsync()
        {
            return await FolioGenerator.GenerarCodigoSimpleAsync(
                _context.Donadores,
                "DON",
                d => d.codigo,
                digits: 6
            );
        }

        /// <summary>
        /// Actualiza un donador existente y registra en bitácora.
        /// El interceptor actualizará automáticamente fecha_actualizacion.
        /// ✅ FASE 3: Validaciones y logging estandarizados
        /// </summary>
        public async Task<bool> UpdateAsync(Donador donador, int id_usuario)
        {
            // ✅ FASE 3: Validaciones estandarizadas
            ValidarObjetoRequerido(donador, nameof(donador));
            ValidarId(donador.id_donador, "donador");
            ValidarId(id_usuario, "usuario");
            ValidarCadenaRequerida(donador.nombre, "nombre del donador", longitudMinima: 2, longitudMaxima: 200);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            LogInicioOperacion("UpdateAsync - Donador", new { donador.id_donador, id_usuario });

            using var transaction = await _context.Database.BeginTransactionAsync();
            ConfigurarTimeout();  // ✅ FASE 3: Usa método base

            try
            {
                var donadorExistente = await _context.Donadores
                    .AsNoTracking().FirstOrDefaultAsync(d => d.id_donador == donador.id_donador);

                if (donadorExistente == null)
                    throw new InvalidOperationException($"El donador con ID {donador.id_donador} no existe");

                // Preservar fecha_creacion
                donador.fecha_creacion = donadorExistente.fecha_creacion;
                donador.codigo = donadorExistente.codigo;

                _context.Donadores.Update(donador);

                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Donadores",
                    submodulo = "Registro",
                    accion = "Actualizar",
                    descripcion = $"Se actualizó donador {donador.codigo} - {donador.nombre}"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                stopwatch.Stop();
                LogExitoOperacion("UpdateAsync - Donador", stopwatch.ElapsedMilliseconds, new { donador.id_donador });

                return true;
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                LogErrorOperacion("UpdateAsync - Donador", ex, new { donador.id_donador });
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                LogErrorOperacion("UpdateAsync - Donador", ex, new { donador.id_donador });
                throw new InvalidOperationException($"Error inesperado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Elimina un donador (marca como inactivo si existe el campo, sino lo elimina).
        /// El interceptor actualizará automáticamente fecha_actualizacion.
        /// ✅ FASE 3: Validaciones y logging estandarizados
        /// </summary>
        public async Task<bool> DeleteAsync(int id, int id_usuario)
        {
            // ✅ FASE 3: Validaciones estandarizadas
            ValidarId(id, "donador");
            ValidarId(id_usuario, "usuario");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            LogInicioOperacion("DeleteAsync - Donador", new { id, id_usuario });

            using var transaction = await _context.Database.BeginTransactionAsync();
            ConfigurarTimeout();  // ✅ FASE 3: Usa método base

            try
            {
                var donador = await _context.Donadores
                    .AsNoTracking().FirstOrDefaultAsync(d => d.id_donador == id);

                if (donador == null)
                {
                    stopwatch.Stop();
                    LogExitoOperacion("DeleteAsync - Donador", stopwatch.ElapsedMilliseconds, new { encontrado = false });
                    return false;
                }

                _context.Donadores.Remove(donador);

                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Donadores",
                    submodulo = "Registro",
                    accion = "Eliminar",
                    descripcion = $"Se eliminó donador {donador.codigo} - {donador.nombre}"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                stopwatch.Stop();
                LogExitoOperacion("DeleteAsync - Donador", stopwatch.ElapsedMilliseconds, new { id });

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                LogErrorOperacion("DeleteAsync - Donador", ex, new { id });
                throw new InvalidOperationException($"Error al eliminar donador: {ex.Message}", ex);
            }
        }
    }
}
