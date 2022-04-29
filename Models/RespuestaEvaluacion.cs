using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class RespuestaEvaluacion
    {
        [Key]
        public int idRespuestaEvaluacion { get; set; }

        public string respuesta { get; set; }

        public int puntaje { get; set; }

        public virtual PreguntasFormulario pregunta { get; set; }

        public virtual RespuestasFormulario respuestaFormulario { get; set; }

        public virtual Notas nota { get; set; }
    }
}