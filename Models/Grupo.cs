namespace SGC.Models
{

    public class Grupo
    {
        public string id { get; set; }
        public int courseid { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int descriptionformat { get; set; }
        public object enrolmentkey { get; set; }
        public string idnumber { get; set; }
    }

}