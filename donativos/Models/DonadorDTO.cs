namespace donativos.Models
{
    public class DonadorDTO
    {
        public int? id_donador { get; set; }
        public string codigo { get; set; }
        public string tipo { get; set; }
        public string nombre { get; set; }
        public string rfc { get; set; }
        public string telefono { get; set; }
        public string email { get; set; }
        public string direccion { get; set; }
        public string colonia { get; set; }
        public string ciudad { get; set; }
        public string estado { get; set; }
        public string codigo_postal { get; set; }
        public string notas { get; set; }
    }
}