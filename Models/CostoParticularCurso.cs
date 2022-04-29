using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("CostoParticularCurso")]
    public class CostoParticularCurso
    {
        [Key]
        public int idCostoParticularCurso { get; set; }
        public int idCostoCursoR12 { get; set; }

        public string detalle { get; set; }

        public string unidad { get; set; }

        public int cantidad { get; set; }

        public int costo { get; set; }

        public int subTotal { get; set; }

        public string categoria { get; set; }


        public virtual CostoCursoR12 costoCursoR12 { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public DateTime fechaCreacion { get; set; }

        public bool porPersona { get; set; }
    }
}