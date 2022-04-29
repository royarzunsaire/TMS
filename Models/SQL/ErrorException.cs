using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGC.Models.SQL
{
    public class ErrorException
    {
        public int id { get; set; }
        public DateTime fecha { get; set; }
        public string action { get; set; }
        public string message { get; set; }
        public AspNetUsers user { get; set; }
    }
}