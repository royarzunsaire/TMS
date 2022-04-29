using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("FormatoDocumentoR50")]
    public partial class FormatoDocumentoR50
    {
        public FormatoDocumentoR50()
        {

            this.cliente = new HashSet<Cliente>();
        }
        [Key]
        public int idFormatoDocumentoR50 { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string nombre { get; set; }

        public string descripcion { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string rutaArchivo { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string usuarioCreador { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime fechaCreacion { get; set; }

        public virtual ICollection<Cliente> cliente { get; set; }

    }
}