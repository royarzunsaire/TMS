using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("TipoDocumentosPago")]
    public partial class TiposDocumentosPago
    {
        public TiposDocumentosPago()
        {
            this.clienteTipoDocumentosPago = new HashSet<ClienteTipoDocumentosPago>();
        }

        [Key]
        public int idTipoDocumentosPago { get; set; }

        [Display(Name = "Nombre")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string nombre { get; set; }

        [Display(Name = "Descripción")]
        [DataType(DataType.MultilineText)]
        [StringLength(250, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string descripcion { get; set; }

        public string habilitado { get; set; }

        public string usuarioCreador { get; set; }

        public DateTime fechaCreacion { get; set; }

        public virtual ICollection<ClienteTipoDocumentosPago> clienteTipoDocumentosPago { get; set; }

        public bool softDelete { get; set; }
    }
}