using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("EstadoComercial")]
    public partial class EstadoComercial
    {
        public EstadoComercial()
        {
            this.cliente = new HashSet<Cliente>();
        }
        [Key]
        public int idEstadoComercial { get; set; }
        [DisplayName("Nombre")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string nombre { get; set; }
        [DisplayName("Descripción")]
        public string descripcion { get; set; }
        [DisplayName("Usuario Creador")]
        public string usuarioCreador { get; set; }
        [DisplayName("Fecha Creación")]
        public DateTime fechaCreacion { get; set; }

        public virtual ICollection<Cliente> cliente { get; set; }



    }
}