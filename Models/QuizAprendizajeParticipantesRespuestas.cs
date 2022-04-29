using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SGC.Models
{
    public class QuizAprendizajeParticipantesRespuestas
    {
        [Key]
        public virtual int idQuizAprendizajeParticipantesRespuestas { get; set; }
        public virtual QuizAprendizajePreguntas quizAprendizajePreguntas { get; set; }
        public virtual QuizAprendizajeRespuestas quizAprendizajeRespuestas { get; set; }
        public virtual DateTime fecha { get; set; }
        public virtual Participante participante { get; set; }
        public virtual Comercializacion comercializacion { get; set; }
    }
}