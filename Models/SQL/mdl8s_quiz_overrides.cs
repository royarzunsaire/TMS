
namespace SGC.Models.SQL
{
    public class mdl8s_quiz_overrides
    {
        public int id { get; set; }
        public int quiz { get; set; }
        public string groupid { get; set; }
        public int userid { get; set; }
        public string timeopen { get; set; }
        public string timeclose { get; set; }
        public string timelimit { get; set; }
        public int attempts { get; set; }
        public string password { get; set; }

   
    }
}