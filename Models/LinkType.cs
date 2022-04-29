using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("LinkType")]

    public class LinkType
    {
        [Key]
        public int idLinkType { get; set; }
       

        [Display(Name = "LinkType")]
        public string nombre { get; set; }

        

    }
}