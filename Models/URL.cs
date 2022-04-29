using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Url
    {
        [Key]
        public int idUrl { get; set; }

        [StringLength(100)]
        public string url { get; set; }
    }
}