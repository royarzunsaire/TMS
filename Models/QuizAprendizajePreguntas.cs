using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SGC.Models
{
    public class QuizAprendizajePreguntas
    {
        [Key]
        public virtual int idQuizAprendizajePreguntas { get; set; }
        public virtual string pregunta { get; set; }
    }
}