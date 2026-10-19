using System.Security.Claims;

namespace apiDonativos.Framework
{
    /// <summary>
    /// Extensiones para trabajar con Claims del usuario autenticado.
    /// </summary>
    public static class ClaimsExtensions
    {
        /// <summary>
        /// Obtiene el ID del usuario autenticado desde los claims.
        /// Intenta obtenerlo primero del claim "id_usuario", si no existe, del NameIdentifier.
        /// </summary>
        /// <param name="user">ClaimsPrincipal del usuario autenticado</param>
        /// <returns>ID del usuario o null si no se puede obtener</returns>
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            if (user == null)
                return null;

            var id_usuarioStr = user.FindFirst("id_usuario")?.Value
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(id_usuarioStr) || !int.TryParse(id_usuarioStr, out int id_usuario))
                return null;

            return id_usuario;
        }

        /// <summary>
        /// Obtiene el id_almacen del usaurio autenticado
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int? GetUserAlmacen(this ClaimsPrincipal user)
        {
            if (user== null)
                return null;

            var id_almacenStr = user.FindFirst("BodegaID")?.Value
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(id_almacenStr) || !int.TryParse(id_almacenStr, out int id_almacen))
                return null;

            return id_almacen;
        }

        /// <summary>
        /// Obtiene el nombre del usuario autenticado.
        /// </summary>
        /// <param name="user">ClaimsPrincipal del usuario autenticado</param>
        /// <returns>Nombre del usuario o null si no existe</returns>
        public static string GetUserName(this ClaimsPrincipal user)
        {
            if (user == null)
                return null;

            return user.FindFirst(ClaimTypes.Name)?.Value
                ?? user.FindFirst("nombre")?.Value;
        }

        /// <summary>
        /// Verifica si el usuario está autenticado y tiene un ID válido.
        /// </summary>
        /// <param name="user">ClaimsPrincipal del usuario autenticado</param>
        /// <returns>True si el usuario está autenticado correctamente</returns>
        public static bool IsAuthenticatedWithValidId(this ClaimsPrincipal user)
        {
            return user?.GetUserId().HasValue ?? false;
        }
    }
}