using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGC.Models
{
    [Table("ListaDetalleCosto")]
    //Es la lista detalle predefinida, no esta ligada a la tabla costos para tener un mayor control de los datos por default 
    //(son los items de la columna detalle de la tabla costos que se muestra en la vista crear cotizacion)
    public class ListaDetalleCosto
    {
        [Key]
        public int idListaDetalleCosto { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Detalle")]
        public string detalle { get; set; }
        public bool activo { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Cantidad")]
        public int cantidad { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Valor")]
        public int valor { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Valor Mínimo")]
        public int valorMinimo { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Valor Máximo")]
        public int valorMaximo { get; set; }
        [Display(Name = "Usuario Creador")]
        public string usuarioCreador { get; set; }
        [Display(Name = "Fecha Creacion")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]

        [Column(TypeName = "datetime2")]
        public DateTime fechaCreacion { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Tipo de Ejecución")]
        public TipoEjecucion tipoEjecucion { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "P/P")]
        public bool porPersona { get; set; }
    }
}