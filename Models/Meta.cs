using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Meta
    {
        [Key]
        public int idMeta { get; set; }

        [Display(Name = "Mes")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime mes { get; set; }

        [Display(Name = "Monto")]
        [DataType(DataType.Currency)]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int monto { get; set; }

        public virtual MetasVendedor metasVendedor { get; set; }

        public virtual MetasSucursal metasSucursal { get; set; }

        public DateTime fechaCreacion { get; set; }

        public virtual AspNetUsers usuarioCreador { get; set; }
    }
}