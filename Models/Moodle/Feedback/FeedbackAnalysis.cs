using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGC.Models.Moodle.Feedback
{
    public class FeedbackAnalysis
    {
        public int completedcount { get; set; }
        public int itemscount { get; set; }
        public List<FeedbackAnalysisData> itemsdata { get; set; }
        public List<object> warnings { get; set; }
    }
}