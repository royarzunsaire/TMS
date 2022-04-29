using System.Collections.Generic;

namespace SGC.Models
{
    public class ViewModelR19
    {
        public int idComercializacion { get; set; }

        public List<ViewModelFormularioR19> formularios { get; set; }
    }

    public class ViewModelFormularioR19
    {
        public int idFormulario { get; set; }

        public int posicion { get; set; }

        public bool encuestaRelator { get; set; }
    }
}