using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Sucursal
    {
        [Key]
        [Display(Name = "Sucursal")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int idSucursal { get; set; }

        public string nombre { get; set; }

        public string prefijoCodigo { get; set; }

        public virtual List<MetasSucursal> metasSucursal { get; set; }

        public virtual Storage firmaAdministrador { get; set; }

        [Display(Name = "Nombre")]
        public string nombreAdministrador { get; set; }

        [Display(Name = "RUN")]
        public string runAdministrador { get; set; }

        [Display(Name = "Dirección")]
        public string direccionAdministrador { get; set; }
    }
}