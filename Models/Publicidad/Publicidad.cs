using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Publicidad")]

    public class Publicidad
    {
        [Key]
       
        public int idPublicidad { get; set; }
        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string nombre { get; set; }
        [Display(Name = "Titulo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string titulo { get; set; }
        [Display(Name = "Descripcion")]
   
        public string descripcion { get; set; }
        [Display(Name = "Link")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string link { get; set; }
        [Display(Name = "Tipo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string tipo { get; set; }
        [Display(Name = "Foto de fondo")]
        
        public virtual Storage foto { get; set; }
        [Display(Name = "Vigencia")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime vigencia { get; set; }
        public virtual AspNetUsers usuarioCreador { get; set; }
        public DateTime fechaCreacion { get; set; }
        public virtual AspNetUsers usuarioActualizo { get; set; }
        public DateTime fechaActualizacion { get; set; }
        public virtual List<PublicidadCliente> publicidadClientes { get; set; }

    }
}