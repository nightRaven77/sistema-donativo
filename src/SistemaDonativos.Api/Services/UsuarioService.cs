using apiDonativos.Models;
using apiDonativos.Models.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;  // ? NUEVO
using System;
using System.Linq;
using System.Threading.Tasks;

namespace apiDonativos.Services
{
    public interface IUsuario
    {
        Task<RegistrarUsuarioResponse> RegistrarAsync(RegistrarUsuarioRequest request);
        //Task<ActualizarPasswordResponse> ActualizarPasswordAsync(ActualizarPasswordRequest request);
    }

    public class UsuarioService : IUsuario
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsuarioService> _logger;  // ? NUEVO

        public UsuarioService(ApplicationDbContext context, ILogger<UsuarioService> logger)  // ? MODIFICADO
        {
            _context = context;
            _logger = logger;  // ? NUEVO
        }

        public async Task<RegistrarUsuarioResponse> RegistrarAsync(RegistrarUsuarioRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "La solicitud no puede ser nula");

            if (string.IsNullOrWhiteSpace(request.usuario) || string.IsNullOrWhiteSpace(request.password))
                throw new ArgumentException("Usuario y contraseña son requeridos");

            // ? SEGURIDAD: Log inicio (sin contraseña)
            _logger.LogInformation(
                "Iniciando registro de usuario. Usuario: {Username}, Centro: {CentroId}",
                request.usuario,
                request.centroID
            );

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@usuario", request.usuario ?? (object)DBNull.Value),
                    new SqlParameter("@password", request.password ?? (object)DBNull.Value),
                    new SqlParameter("@centroAcopio", request.centroAcopio ?? (object)DBNull.Value),
                    new SqlParameter("@centroID", request.centroID),
                    new SqlParameter("@nombreCompleto", request.nombreCompleto ?? (object)DBNull.Value),
                    new SqlParameter("@correoElectronico", request.correoElectronico ?? (object)DBNull.Value),
                    new SqlParameter("@telefono", request.telefono ?? (object)DBNull.Value)
                };

                // Ejecutar el SP y capturar los resultados
                var resultado = await _context.RegistrarUsuarioResultados
                    .FromSqlRaw("EXEC RegistrarUsuario @usuario, @password, @centroAcopio, @centroID, @nombreCompleto, @correoElectronico, @telefono",
                        parameters)
                    .AsNoTracking()
                    .ToListAsync();

                if (resultado == null || resultado.Count == 0)
                {
                    return new RegistrarUsuarioResponse
                    {
                        exito = false,
                        mensaje = "No se recibió respuesta del servidor"
                    };
                }

                var item = resultado.FirstOrDefault();

                if (item.UsuarioID > 0)
                {
                    _logger.LogInformation(
                        "Usuario registrado exitosamente. Usuario: {Username}, ID: {UserId}",
                        request.usuario,
                        item.UsuarioID
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "Registro fallido. Usuario: {Username}, Mensaje: {Mensaje}",
                        request.usuario,
                        item.Mensaje
                    );
                }

                return new RegistrarUsuarioResponse
                {
                    exito = item.UsuarioID > 0,
                    mensaje = item.Mensaje,
                    idUsuario = item.UsuarioID > 0 ? item.UsuarioID : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al registrar usuario. Usuario: {Username}",
                    request.usuario
                );

                return new RegistrarUsuarioResponse
                {
                    exito = false,
                    mensaje = "Error al registrar usuario"
                };
            }
        }

        //public async Task<ActualizarPasswordResponse> ActualizarPasswordAsync(ActualizarPasswordRequest request)
        //{
        //    if (request == null)
        //        throw new ArgumentNullException(nameof(request), "La solicitud no puede ser nula");
        //    if (string.IsNullOrWhiteSpace(request.usuario) || string.IsNullOrWhiteSpace(request.passwordActual) || string.IsNullOrWhiteSpace(request.nuevaPassword))
        //        throw new ArgumentException("Usuario, contraseña actual y nueva contraseña son requeridos");
        //    try
        //    {
        //        var parameters = new[]
        //        {
        //            new SqlParameter("@usuario", request.usuario ?? (object)DBNull.Value),
        //            new SqlParameter("@passwordActual", request.passwordActual ?? (object)DBNull.Value),
        //            new SqlParameter("@nuevaPassword", request.nuevaPassword ?? (object)DBNull.Value)
        //        };
        //        var resultado = await _context.ActualizarPassResultados
        //            .FromSqlRaw("EXEC ActualizarPassword @usuario, @passwordActual, @nuevaPassword", parameters)
        //            .AsNoTracking()
        //            .ToListAsync();
        //        if (resultado == null || resultado.Count == 0)
        //        {
        //            return new ActualizarPasswordResponse
        //            {
        //                exito = false,
        //                mensaje = "No se recibió respuesta del servidor"
        //            };
        //        }
        //        var item = resultado.FirstOrDefault();
        //        return new ActualizarPasswordResponse
        //        {
        //            exito = item.Exito,
        //            mensaje = item.Resultado
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(
        //            ex,
        //            "Error al actualizar contraseña. Usuario: {Username}",
        //            request.usuario
        //        );
        //        return new ActualizarPasswordResponse
        //        {
        //            exito = false,
        //            mensaje = "Error al actualizar contraseña"
        //        };
        //    }
        //}
    }
}