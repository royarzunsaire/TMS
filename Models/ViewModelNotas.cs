using System.Collections.Generic;

namespace SGC.Models.Feedback
{
    public class ViewModelNotas
    {
        public Comercializacion comercializacion { get; set; }
        public List<Attempts> attempt { get; set; }
        public List<AttemptsQuizUser> attemptsQuizUser { get; set; }
     
    }
}