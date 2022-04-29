using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("LinkComercializacion")]

    public class LinkComercializacion
    {
        [Key]
        public int idLinkComercializacion { get; set; }
        [Key]
        [Required]
        public virtual Comercializacion comercializacion { get; set; }
        
        public Link link { get; set; }
        [Key]
        [Required]
        public virtual LinkType linkType { get; set; }
        public string linkManual { get; set; }
        public bool linkAutomatic { get; set; }


    }
}