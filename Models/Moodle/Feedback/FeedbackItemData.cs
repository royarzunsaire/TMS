using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGC.Models.Moodle.Feedback
{
    public class FeedbackItemData
    {
        public int id { get; set; }
        public int feedback { get; set; }
        public int template { get; set; }
        public string name { get; set; }
        public string label { get; set; }
        public string presentation { get; set; }
        public string typ { get; set; }
        public int hasvalue { get; set; }
        public int position { get; set; }
        public bool required { get; set; }
        public int dependitem { get; set; }
        public string dependvalue { get; set; }
        public string options { get; set; }
        public List<object> itemfiles { get; set; }
        public int? itemnumber { get; set; }
        public object otherdata { get; set; }
    }
}