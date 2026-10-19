using System.Text.RegularExpressions;

namespace apiDonativos.Helpers
{
    /// <summary>
    /// Helper para mejorar mensajes de error de base de datos
    /// </summary>
    public static class DatabaseErrorHelper
    {
        /// <summary>
        /// Extrae información útil de mensajes de error de FK constraint
        /// </summary>
        /// <param name="errorMessage">Mensaje de error de SQL Server</param>
        /// <returns>Mensaje mejorado en español</returns>
        public static string MejorarMensajeForeignKey(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                return "Referencia inválida";

            // Patrones comunes en SQL Server:
            // FK_donativos_donadores -> tabla referenciada: "donadores"
            // FK_salidas_beneficiarios -> tabla referenciada: "beneficiarios"
            
            var match = Regex.Match(errorMessage, @"FK_\w+_(\w+)", RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                var tabla = match.Groups[1].Value;
                
                // Convertir nombre de tabla a singular y español
                var entidad = tabla switch
                {
                    "donadores" => "donador",
                    "beneficiarios" => "beneficiario",
                    "articulos" or "Articulos" => "artículo",
                    "almacenes" or "Almacenes" => "almacén",
                    "usuarios" or "Usuarios" => "usuario",
                    "categorias" or "Categorias" => "categoría",
                    _ => tabla.TrimEnd('s') // Por defecto quita la 's'
                };
                
                return $"Referencia inválida: el {entidad} especificado no existe";
            }
            
            // Si no se puede extraer, mensaje genérico
            return "Referencia inválida: uno de los IDs proporcionados no existe";
        }

        /// <summary>
        /// Extrae el nombre de columna de mensajes de NOT NULL
        /// </summary>
        public static string ExtraerNombreColumna(string errorMessage)
        {
            var match = Regex.Match(errorMessage, @"column '([^']+)'", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : "desconocida";
        }
    }
}
