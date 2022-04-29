using System.ComponentModel.DataAnnotations;
namespace SGC.Models

{
    public class RespuestasFormulario
    {
        [Key]
        public int idRespuestasFormulario { get; set; }

        // public int idPreguntasFormulario { get; set; }

        public string respuesta { get; set; }

        public string tipoRespuesta { get; set; }

        public bool respuestaCorrecta { get; set; }

        public virtual PreguntasFormulario preguntasFormulario { get; set; }

        public int puntaje { get; set; }

        public int orden { get; set; }
    }
}