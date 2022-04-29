using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SGC.Models
{
    public class QuizAprendizajeRespuestas
    {
        [Key]
        public virtual int idQuizAprendizajeRespuestas { get; set; }
        public virtual QuizAprendizajePreguntas quizAprendizajePreguntas { get; set; }
        public virtual int codigoRespuesta { get; set; }
        public virtual string respuesta { get; set; }
    }
}