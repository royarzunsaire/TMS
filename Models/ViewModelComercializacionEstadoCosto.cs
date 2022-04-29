using System.Collections.Generic;

namespace SGC.Models
{
    public class ViewModelComercializacionEstadoCosto
    {
        public Comercializacion comercializacion { get; set; }

        public ComercializacionEstadoComercializacion estado { get; set; }
        public List<Costo> costo { get; set; }
    }
}