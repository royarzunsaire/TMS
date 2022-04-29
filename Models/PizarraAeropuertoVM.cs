using System.Collections.Generic;

namespace SGC.Models
{
    public class PizarraAeropuertoVM
    {
        public Bloque bloque { get; set; }
        public Curso curso { get; set; }
        public Cliente cliente { get; set; }
        public int cantParticipantes { get; set; }
        public List<Costo> costo { get; set; }

    }
}