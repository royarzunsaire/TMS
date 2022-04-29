
namespace SGC.Models.SQL
{
    public class mdl8s_quiz_attempts
    {
        public int id { get; set; }
        public int quiz { get; set; }
        public int userid { get; set; }
        public int attempt { get; set; }
        public int timefinish { get; set; }
        public string sumgrades { get; set; }
        public string totalgrades { get; set; }


    }
}