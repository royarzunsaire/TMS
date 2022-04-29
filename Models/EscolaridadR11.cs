using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGC.Models
{
    [Table("EscolaridadR11")]
    public class EscolaridadR11
    {
        [Key]
        public int idEscolaridad { get; set; }
        //public virtual R11 r11 { get; set; }
        public int idR11 { get; set; }
        public string nivel { get; set; }
        public string experiencia { get; set; }
        public bool marca { get; set; }

    }
}