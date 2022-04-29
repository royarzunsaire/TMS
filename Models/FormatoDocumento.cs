using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("FormatoDocumento")]

    public class FormatoDocumento
    {
        [Key]
        public int idFormatoDocumento { get; set; }
        [Display(Name = "Nombre Archivo")]
        public string nombreDocumento { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Tipo Archivo")]
        public string tipoArchivo { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public DateTime fechaSubida { get; set; }
    }
}