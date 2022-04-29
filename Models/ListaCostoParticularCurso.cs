using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("ListaCostoParticularCurso")]
    public class ListaCostoParticularCurso
    {
        [Key]
        public int idListaCostoParticularCurso { get; set; }

        public string detalle { get; set; }

        public string unidad { get; set; }

        public string categoria { get; set; }

        public int cantidad { get; set; }

        public int costo { get; set; }
    }
}