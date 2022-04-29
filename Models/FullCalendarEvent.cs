using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [NotMapped]
    public class FullCalendarEvent
    {
        public string title { get; set; }
        public object description { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string color { get; set; }
    }
}