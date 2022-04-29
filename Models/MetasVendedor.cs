using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class MetasVendedor
    {
        [Key]
        public int idMetasVendedor { get; set; }

        public virtual ICollection<Meta> metas { get; set; }

        public virtual AspNetUsers vendedor { get; set; }
    }
}