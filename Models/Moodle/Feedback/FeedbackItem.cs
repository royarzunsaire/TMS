using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGC.Models.Moodle.Feedback
{
    public class FeedbackItem
    {
        public List<FeedbackItemData> items { get; set; }
        public List<object> warnings { get; set; }
    }
}