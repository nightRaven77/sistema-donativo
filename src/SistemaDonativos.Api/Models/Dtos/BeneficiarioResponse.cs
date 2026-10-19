namespace apiDonativos.Models.Dtos
{
    public class BeneficiarioResponse
    {
        public int id_beneficiario { get; set; }
        public string codigo { get; set; }
        public string nombre { get; set; }
        public string tipo { get; set; }
        public string contacto_principal { get; set; }
        public string telefono { get; set; }
        public string email { get; set; }
        public string estatus { get; set; }
    }
}
