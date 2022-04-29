using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("TipoFormatoDocumento")]

    public class TipoFormatoDocumento
    {
        [Key]
        public int idTipoFormatoDocumento { get; set; }

        [Display(Name = "Tipo Documento")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string tipo { get; set; }

        [Display(Name = "Vigencia")]
        public int vigencia { get; set; }

        [Display(Name = "Utilizado")]
        public int utilizado { get; set; }
    }
}