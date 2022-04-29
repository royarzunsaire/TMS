using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGC.Models.Moodle.Feedback
{
    public class Feedback
    {
        public List<FeedbackData> feedbacks { get; set; }
        public List<object> warnings { get; set; }
    }
}