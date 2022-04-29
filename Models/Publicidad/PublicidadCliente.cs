using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("PublicidadCliente")]

    public class PublicidadCliente
    {
        [Key]
        [Required(ErrorMessage = "El campo Id es obligatorio")]
        public int idPublicidadCliente { get; set; }
        public virtual Cliente cliente { get; set; }
        public virtual Publicidad publicidad { get; set; }

    }
}