using System.Collections.Generic;

namespace SGC.Models
{
    public class ViewModelRespuestaFormulario
    {
        public int idContacto { get; set; }

        public Formulario formulario { get; set; }

        public List<RespuestasFormulario> respuestas { get; set; }

        public List<RespuestasContestadasFormulario> respuestasContestadas { get; set; }
    }
}