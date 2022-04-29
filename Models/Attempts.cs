
using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Attempts
    {
        [Key]
        public int idAttempts { get; set; }
        public int idAttemptsMoodle { get; set; }
        public int quiz { get; set; }
        public int userid { get; set; }
        public int attempt { get; set; }
        public virtual DateTime timeFinish { get; set; }
      
        public string sumGrades { get; set; }

        public string totalGrades { get; set; }

        public virtual Participante participante { get; set; }
        public virtual Evaluacion evaluacion { get; set; }
    }
}