using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Giro")]
    public partial class Giro
    {
        public Giro()
        {
            this.clienteGiro = new HashSet<ClienteGiro>();
        }

        [Key]
        public int idGiro { get; set; }

        [Display(Name = "Código")]
        [StringLength(25, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string codigo { get; set; }

        [Display(Name = "Descripción")]
        [DataType(DataType.MultilineText)]
        [StringLength(250, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string descripcion { get; set; }

        public string usuarioCreador { get; set; }

        public DateTime fechaCreacion { get; set; }

        public virtual ICollection<ClienteGiro> clienteGiro { get; set; }

        public bool softDelete { get; set; }

    }
}