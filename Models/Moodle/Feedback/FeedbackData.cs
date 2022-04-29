using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGC.Models.Moodle.Feedback
{
    public class FeedbackData
    {
        public int id { get; set; }
        public int course { get; set; }
        public string name { get; set; }
        public string intro { get; set; }
        public int introformat { get; set; }
        public int anonymous { get; set; }
        public bool email_notification { get; set; }
        public bool multiple_submit { get; set; }
        public bool autonumbering { get; set; }
        public string site_after_submit { get; set; }
        public string page_after_submit { get; set; }
        public int page_after_submitformat { get; set; }
        public bool publish_stats { get; set; }
        public int timeopen { get; set; }
        public int timeclose { get; set; }
        public int timemodified { get; set; }
        public bool completionsubmit { get; set; }
        public int coursemodule { get; set; }
        public List<object> introfiles { get; set; }
        public List<object> pageaftersubmitfiles { get; set; }

    }
}