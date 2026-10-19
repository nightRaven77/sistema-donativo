
using System;

namespace apiDonativos.Models.Catalogos
{
    public class Beneficiario
    {
        public int id_beneficiario { get; set; }
        public string codigo { get; set; }
        public string tipo { get; set; }
        public string nombre { get; set; }
        public string contacto_principal { get; set; }
        public string telefono { get; set; }
        public string email { get; set; }
        public string direccion { get; set; }
        public string colonia { get; set; }
        public string ciudad { get; set; }
        public string estado { get; set; }
        public string codigo_postal { get; set; }
        public string curp { get; set; }
        public int total_ayudas { get; set; }
        public decimal monto_total_recibido { get; set; }
        public string estatus { get; set; }
        public string observaciones { get; set; }

        public DateTime? fecha_registro { get; set; }
        public DateTime? fecha_ultima_ayuda { get; set; }

    }
}
