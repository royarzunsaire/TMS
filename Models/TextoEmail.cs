using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class TextoEmail
    {
        [Key]
        public int idTextoEmail { get; set; }

        public string email { get; set; }

        public string motivo { get; set; }

        public string texto { get; set; }
    }
}