using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGC.Models.Moodle.Feedback
{
    public class FeedbackResponses
    {
        public List<FeedbackResponsesData> attempts { get; set; }
        public List<object> warnings { get; set; }
    }
}