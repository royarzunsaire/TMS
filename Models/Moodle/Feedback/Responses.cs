using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGC.Models.Moodle.Feedback
{
    public class Responses
    {
        public int id { get; set; }
        public string name { get; set; }
        public string printval { get; set; }
        public int rawval { get; set; }
    }
}