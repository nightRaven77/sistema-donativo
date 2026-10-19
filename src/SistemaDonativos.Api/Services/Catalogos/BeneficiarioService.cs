        using apiDonativos.Models;
using apiDonativos.Models.Catalogos;
using apiDonativos.Models.Dtos;
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
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace apiDonativos.Services.Catalogos
{
    public interface IBeneficiarioService
    {
        Task<List<BeneficiarioResponse>> GetAllAsync();
        Task<int>CreateAsync(Beneficiario beneficiario, int id_usuario);  
        Task <BeneficiarioResponse> GetByIdAsync(int id);
        Task<BeneficiarioCompleteResponse> GetCompleteByIdAsync(int id);
        Task<List<BeneficiarioResponse>> GetByNameAsync(string nombre);
        Task<bool> UpdateAsync(int id_beneficiario, UpdateBeneficiarioRequest request, int id_usuario);
        Task<bool> DeleteAsync(int id_beneficiario, int id_usuario);
    }

    /// <summary>
    /// Servicio para gestión de beneficiarios
    /// ✅ FASE 3: Heredado de ServiceBase con timeout configurable
    /// </summary>
    public class BeneficiarioService : ServiceBase<BeneficiarioService>, IBeneficiarioService
    {
        public BeneficiarioService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<BeneficiarioService> logger)
            : base(context, configuration, logger)
        {
        }

        /// <summary>
        /// Genera un codigo Secuencial tipo BENE-000001, BENE-000002, etc. para cada nuevo beneficiario registrado
        /// ✅ M2: Refactorizado para usar FolioGenerator
        /// </summary>
        private async Task<string> GenerarCodigoBeneficiarioAsync()
        {
            return await FolioGenerator.GenerarCodigoSimpleAsync(
                _context.Beneficiarios,
                "BENE",
                b => b.codigo,
                digits: 6
            );
        }


        /// <summary>
        /// Obtiene el listado de todos los beneficiarios activos
        /// </summary>
        public async Task<List<BeneficiarioResponse>> GetAllAsync()
        {
            return await _context.Beneficiarios
                .Where(b => b.estatus != "Inactivo")
                .Select(b => new BeneficiarioResponse
                {
                    id_beneficiario = b.id_beneficiario,
                    codigo = b.codigo,
                    nombre = b.nombre,
                    tipo = b.tipo,
                    contacto_principal = b.contacto_principal,
                    telefono = b.telefono,
                    email = b.email,
                    estatus = b.estatus
                })
                .OrderByDescending(b => b.id_beneficiario)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<BeneficiarioResponse> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID de beneficiario inválido", nameof(id));

            return await _context.Beneficiarios
                .Where(b => b.id_beneficiario == id)
                .Select(b => new BeneficiarioResponse
                {
                    id_beneficiario = b.id_beneficiario,
                    codigo = b.codigo,
                    nombre = b.nombre,
                    tipo = b.tipo,
                    contacto_principal = b.contacto_principal,
                    telefono = b.telefono,
                    email = b.email,
                    estatus = b.estatus
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

        }

        public async Task<List<BeneficiarioResponse>> GetByNameAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío", nameof(nombre));

            return await _context.Beneficiarios
                .Where(b => b.nombre.ToLower().Contains(nombre.Trim().ToLower()) 
                    && b.estatus != "Inactivo")
                .Select(b => new BeneficiarioResponse
                {
                    id_beneficiario = b.id_beneficiario,
                    codigo = b.codigo,
                    nombre = b.nombre,
                    tipo = b.tipo,
                    contacto_principal = b.contacto_principal,
                    telefono = b.telefono,
                    email = b.email,
                    estatus = b.estatus
                })
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Crea un nuevo beneficiario en la base de datos, generando un código secuencial único para cada beneficiario registrado.
        /// 
        /// </summary>
        ///<returns></returns>
        public async Task<int> CreateAsync(Beneficiario beneficiario, int id_usuario)
        {
            // ✅ FASE 3: Validaciones estandarizadas
            ValidarObjetoRequerido(beneficiario, nameof(beneficiario));
            ValidarId(id_usuario, "usuario");
            ValidarCadenaRequerida(beneficiario.nombre, "nombre del beneficiario", longitudMinima: 2, longitudMaxima: 200);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            LogInicioOperacion("CreateAsync - Beneficiario", new { beneficiario.nombre, id_usuario });

            using var transaction = await _context.Database.BeginTransactionAsync();
            ConfigurarTimeout();  // ✅ FASE 3: Usa método base

            try
            {
                var existeBeneficiario = await _context.Beneficiarios
                    .Where(b => b.nombre.ToLower() == beneficiario.nombre.Trim().ToLower())
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (existeBeneficiario != null)
                {
                    // ✅ MEJORA: Lanzar excepción estructurada con información completa del recurso duplicado
                    throw new DuplicateResourceException(
                        message: $"Ya existe un beneficiario con el nombre '{beneficiario.nombre}'",
                        resourceId: existeBeneficiario.id_beneficiario,
                        resourceCode: existeBeneficiario.codigo,
                        resourceType: "Beneficiario",
                        conflictingField: "nombre"
                    );
                }

                // ✅ FASE 1: Generar código y crear beneficiario con retry logic
                string codigo = null;
                await RetryHelper.ExecuteWithRetryAsync(async () =>
                {
                    codigo = await GenerarCodigoBeneficiarioAsync();
                    beneficiario.codigo = codigo;
                    beneficiario.estatus = beneficiario.estatus ?? "Activo";
                    beneficiario.fecha_registro = DateTime.Now;
                    beneficiario.total_ayudas = 0;
                    beneficiario.monto_total_recibido = 0;

                    _context.Beneficiarios.Add(beneficiario);

                    var bitacora = new Bitacora
                    {
                        id_usuario = id_usuario,
                        modulo = "Beneficiarios",
                        submodulo = "Registro",
                        accion = "Crear",
                        descripcion = $"Se creó el beneficiario {beneficiario.nombre} con código {codigo}"
                    };

                    _context.Bitacoras.Add(bitacora);
                    await _context.SaveChangesAsync();

                    return beneficiario.id_beneficiario;
                });

                await transaction.CommitAsync();

                // ✅ SEGURIDAD: Log éxito
                _logger.LogInformation(
                    "Beneficiario creado exitosamente. Código: {Codigo}, ID: {BeneficiarioId}, Usuario: {UserId}",
                    codigo,
                    beneficiario.id_beneficiario,
                    id_usuario
                );

                return beneficiario.id_beneficiario;

            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    dbEx,
                    "Error BD al crear beneficiario. Usuario: {UserId}, Nombre: {Nombre}",
                    id_usuario,
                    beneficiario.nombre
                );

                var errorMessage = "Error al crear beneficiario en la base de datos";
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (innerMessage.Contains("Cannot insert the value NULL"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(
                        innerMessage, @"column '([^']+)'");
                    var columnName = match.Success ? match.Groups[1].Value : "desconocida";
                    errorMessage = $"Campo requerido faltante: '{columnName}' no puede ser nulo";
                }
                else if (innerMessage.Contains("UNIQUE") || innerMessage.Contains("duplicate"))
                {
                    errorMessage = "Ya existe un beneficiario con los mismos datos";
                }
                else
                {
                    errorMessage = $"Error de base de datos: {innerMessage}";
                }

                throw new InvalidOperationException(errorMessage, dbEx);
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();

                _logger.LogWarning(
                    ex,
                    "Error de validación al crear beneficiario. Usuario: {UserId}",
                    id_usuario
                );

                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error inesperado al crear beneficiario. Usuario: {UserId}",
                    id_usuario
                );

                throw new InvalidOperationException($"Error inesperado al crear beneficiario: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un beneficiario completo por ID (todos los campos)
        /// </summary>
        public async Task<BeneficiarioCompleteResponse> GetCompleteByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID de beneficiario inválido", nameof(id));

            return await _context.Beneficiarios
                .Where(b => b.id_beneficiario == id)
                .Select(b => new BeneficiarioCompleteResponse
                {
                    id_beneficiario = b.id_beneficiario,
                    codigo = b.codigo,
                    tipo = b.tipo,
                    nombre = b.nombre,
                    contacto_principal = b.contacto_principal,
                    telefono = b.telefono,
                    email = b.email,
                    direccion = b.direccion,
                    colonia = b.colonia,
                    ciudad = b.ciudad,
                    estado = b.estado,
                    codigo_postal = b.codigo_postal,
                    curp = b.curp,
                    total_ayudas = b.total_ayudas,
                    monto_total_recibido = b.monto_total_recibido,
                    estatus = b.estatus,
                    observaciones = b.observaciones,
                    fecha_registro = b.fecha_registro,
                    fecha_ultima_ayuda = b.fecha_ultima_ayuda
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Actualiza un beneficiario existente
        /// </summary>
        public async Task<bool> UpdateAsync(int id_beneficiario, UpdateBeneficiarioRequest request, int id_usuario)
        {
            if (id_beneficiario <= 0)
                throw new ArgumentException("ID de beneficiario inválido", nameof(id_beneficiario));

            if (request == null)
                throw new ArgumentNullException(nameof(request), "La solicitud de actualización no puede ser nula");

            if (string.IsNullOrWhiteSpace(request.nombre))
                throw new ArgumentException("El nombre del beneficiario es requerido");

            _logger.LogInformation("Iniciando actualización de beneficiario. Id: {Id}, Usuario: {UserId}", id_beneficiario, id_usuario);

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);

            try
            {
                var beneficiario = await _context.Beneficiarios.FindAsync(id_beneficiario);

                if (beneficiario == null)
                    throw new InvalidOperationException("Beneficiario no encontrado");

                beneficiario.nombre = request.nombre;
                beneficiario.tipo = request.tipo;
                beneficiario.contacto_principal = request.contacto_principal;
                beneficiario.telefono = request.telefono;
                beneficiario.email = request.email;
                beneficiario.direccion = request.direccion;
                beneficiario.colonia = request.colonia;
                beneficiario.ciudad = request.ciudad;
                beneficiario.estado = request.estado;
                beneficiario.codigo_postal = request.codigo_postal;
                beneficiario.curp = request.curp;
                beneficiario.estatus = request.estatus ?? beneficiario.estatus;
                beneficiario.observaciones = request.observaciones;

                _context.Beneficiarios.Update(beneficiario);

                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Beneficiarios",
                    submodulo = "Actualización",
                    accion = "Actualizar",
                    descripcion = $"Se actualizó el beneficiario {beneficiario.codigo} - {beneficiario.nombre}"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Beneficiario actualizado. Id: {Id}, Usuario: {UserId}", id_beneficiario, id_usuario);

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "Error BD al actualizar beneficiario. Id: {Id}", id_beneficiario);

                var errorMessage = "Error al actualizar beneficiario";
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                if (innerMessage.Contains("UNIQUE")) errorMessage = "Ya existe un beneficiario con esos datos";
                else errorMessage = $"Error de base de datos: {innerMessage}";

                throw new InvalidOperationException(errorMessage, dbEx);
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Error al actualizar beneficiario. Id: {Id}", id_beneficiario);
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error inesperado. Id: {Id}", id_beneficiario);
                throw new InvalidOperationException($"Error inesperado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Elimina (desactiva) un beneficiario mediante soft delete
        /// </summary>
        public async Task<bool> DeleteAsync(int id_beneficiario, int id_usuario)
        {
            if (id_beneficiario <= 0)
                throw new ArgumentException("ID de beneficiario inválido", nameof(id_beneficiario));

            _logger.LogInformation("Iniciando eliminación de beneficiario. Id: {Id}, Usuario: {UserId}", id_beneficiario, id_usuario);

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Database.SetCommandTimeout(30);
            try
            {
                var beneficiario = await _context.Beneficiarios.FindAsync(id_beneficiario);

                if (beneficiario == null)
                    throw new InvalidOperationException("Beneficiario no encontrado");

                var tieneSalidas = await _context.salidas
                    .AnyAsync(s => s.id_beneficiario == id_beneficiario);

                if (tieneSalidas)
                {
                    beneficiario.estatus = "Inactivo";
                    _context.Beneficiarios.Update(beneficiario);
                }
                else
                {
                    _context.Beneficiarios.Remove(beneficiario);
                }

                var bitacora = new Bitacora
                {
                    id_usuario = id_usuario,
                    modulo = "Beneficiarios",
                    submodulo = "Eliminación",
                    accion = "Eliminar",
                    descripcion = $"Se eliminó el beneficiario {beneficiario.codigo} - {beneficiario.nombre}"
                };
                _context.Bitacoras.Add(bitacora);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Beneficiario eliminado. Id: {Id}, Usuario: {UserId}", id_beneficiario, id_usuario);

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "Error BD al eliminar beneficiario. Id: {Id}", id_beneficiario);

                var errorMessage = "Error al eliminar beneficiario";
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                if (innerMessage.Contains("REFERENCE constraint"))
                    errorMessage = "No se puede eliminar: el beneficiario tiene registros dependientes";
                else
                    errorMessage = $"Error de base de datos: {innerMessage}";

                throw new InvalidOperationException(errorMessage, dbEx);
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Error al eliminar beneficiario. Id: {Id}", id_beneficiario);
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error inesperado. Id: {Id}", id_beneficiario);
                throw new InvalidOperationException($"Error inesperado: {ex.Message}", ex);
            }
        }

    }
}