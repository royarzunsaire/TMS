
using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class AttemptsQuizUser
    {
        [Key]
        public int idAttemptsQuizUser { get; set; }
      
        public int quiz { get; set; }
        public int userid { get; set; }
        public int attempt { get; set; }
        public virtual Participante participante { get; set; }
        public virtual Evaluacion evaluacion { get; set; }

    }
}