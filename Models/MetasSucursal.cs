using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class MetasSucursal
    {
        [Key]
        public int idMetasSucursal { get; set; }

        public virtual ICollection<Meta> metas { get; set; }

        public virtual Sucursal sucursal { get; set; }
    }
}