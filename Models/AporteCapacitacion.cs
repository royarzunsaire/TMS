using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class AporteCapacitacion
    {
        [Key]
        public int idAporteCapacitacion { get; set; }

        [DisplayName("Nombre")]
        [Required(ErrorMessage = "El campo Relator es obligatorio")]
        [DataType(DataType.Text)]
        [StringLength(250, ErrorMessage = "El campo {0} puede tener entre {2} y {1} caracteres", MinimumLength = 1)]
        public string nombre { get; set; }

        [DisplayName("Tipo")]
        [Required(ErrorMessage = "El campo Relator es obligatorio")]
        public TipoEjecucion tipo { get; set; }

        public virtual ICollection<CotizacionAporteCapacitacion> cotizacionAporteCapacitacion { get; set; }

        public virtual AspNetUsers usuarioCreador { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime fechaCreacion { get; set; }

        public bool softDelete { get; set; }
    }
}