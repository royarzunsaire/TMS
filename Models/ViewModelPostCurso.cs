using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class ViewModelPostCurso
    {

        public Comercializacion comercializacion { get; set; }
        public PostCurso postCurso { get; set; }
        public ComercializacionEstadoComercializacion estado { get; set; }
    }
}