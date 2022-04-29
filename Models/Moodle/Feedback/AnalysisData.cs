using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGC.Models.Moodle.Feedback
{
    public class AnalysisData
    {
        public string answertext { get; set; }
        public double answercount { get; set; }
        public double avg { get; set; }
        public string value { get; set; }
        public double quotient { get; set; }
    }
}