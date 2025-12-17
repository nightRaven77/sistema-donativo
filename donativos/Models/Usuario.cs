namespace donativos.Models
{
    public class Usuario
    {
        public int UsuarioID { get; set; }
        public string usuario { get; set; }
        public string password { get; set; }
        public bool activo { get; set; }
        public string centroAcopio { get; set; }
        public string CentroID { get; set; }

    }
}
