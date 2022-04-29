using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SGC.Models
{
    public class QuizAprendizajeParticipantesResultados
    {
        [Key]
        public virtual int idQuizAprendizajeParticipantesResultados { get; set; }
        public virtual QuizAprendizajeResultados quizAprendizajeResultados { get; set; }
        public virtual DateTime fecha { get; set; }
        public virtual bool enviado { get; set; }
        public virtual Participante participante { get; set; }
        public virtual Comercializacion comercializacion { get; set; }
    }
}