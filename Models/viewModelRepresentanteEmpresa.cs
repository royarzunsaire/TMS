using System.Collections.Generic;

namespace SGC.Models
{
    public class viewModelRepresentanteEmpresa
    {
        public List<Comercializacion> comercializaciones { get; set; }
        public List<Cotizacion_R13> cotizaciones { get; set; }
        public ClienteContacto representate { get; set; }

        public Participante participante { get; set; }

    }
}