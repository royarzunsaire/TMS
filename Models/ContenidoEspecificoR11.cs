using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGC.Models
{
    [Table("ContenidoEspecificoR11")]
    public class ContenidoEspecificoR11
    {
        [Key]
        public int idContenidoEspecificoR11 { get; set; }
        public string nombre { get; set; }
        public double horasT { get; set; }
        public double horasP { get; set; }
        public int idR11 { get; set; }
        public virtual ICollection<ItemContenidoEspecificoR11> itemConteidoEspecificoR11 { get; set; }

    }
}