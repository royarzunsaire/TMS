using System.Collections.Generic;

namespace SGC.Models
{
    public class ViewModelLandingPageCliente
    {
        public ClienteContacto clienteContacto { get; set; }

        public List<Comercializacion> comercializaciones { get; set; }

        public List<Cotizacion_R13> cotizaciones { get; set; }

        public List<SalidaTerreno> salidasTerreno { get; set; }

        public List<ViewModelCurso> cursos { get; set; }

        //public List<Participante> participantes { get; set; }
    }
}