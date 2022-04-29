using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGC.Models
{
    [Table("ItemContenidoEspecificoR11")]

    public class ItemContenidoEspecificoR11
    {
        [Key]
        public int idItemContenidoEspecificoR11 { get; set; }
        public string contenidoEspecifico { get; set; }
        public int idContenidoEspecificoR11 { get; set; }

    }
}