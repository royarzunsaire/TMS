using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace SGC.Models.Moodle.Feedback
{
    public class FeedbackAnalysisData
    {
        
        public FeedbackItemData item { get; set; }
        public List<string> data { get; set; }
        public List<AnalysisData> dataObject { get {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                return data.Select(x => serializer.Deserialize<AnalysisData>(x)).ToList();
            } }
        //public List<AnalysisData> data { get; set; }
    }
}