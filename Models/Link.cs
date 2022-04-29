using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Link")]

    public class Link
    {
        [Key]
        [Required(ErrorMessage = "El campo Link es obligatorio")]
        public int idLink { get; set; }
       
        [Required(ErrorMessage = "El campo Link es obligatorio")]
        [Display(Name = "Link")]
        public string url { get; set; }
        [Display(Name = "Tipo")]
        public virtual LinkType type { get; set; }

    }
}