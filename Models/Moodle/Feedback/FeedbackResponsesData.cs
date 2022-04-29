using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGC.Models.Moodle.Feedback
{
    public class FeedbackResponsesData
    {
        public int id { get; set; }
        public int courseid { get; set; }
        public int userid { get; set; }
        public int timemodified { get; set; }
        public string fullname { get; set; }
        public List<Responses> responses { get; set; }
    }
}