using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Template
    {
        [Key]
        public int idTemplate { get; set; }

        [Display(Name = "Nombre")]
        [DataType(DataType.Text)]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string nombre { get; set; }

        [Display(Name = "Tipo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public TipoTemplate tipo { get; set; }

        [Display(Name = "Template")]
        public virtual Storage template { get; set; }

        [Display(Name = "Fecha Última Modificación")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime fechaUltimaModificacion { get; set; }

        public virtual AspNetUsers usuarioUltimaModificacion { get; set; }
    }

    public enum TipoTemplate
    {
        word,
        excel,
        xml
        //pptx,
        //pdf
    }
}