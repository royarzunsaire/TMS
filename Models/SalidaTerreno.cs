using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class SalidaTerreno
    {
        [Key]
        public int idSalidaTerreno { get; set; }

        [Display(Name = "Fecha")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime fecha { get; set; }

        [Display(Name = "Hora")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime hora { get; set; }

        [Display(Name = "Motivo")]
        [DataType(DataType.MultilineText)]
        [StringLength(999, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string motivo { get; set; }

        [Display(Name = "Estado")]
        public EstadoSalidaTerreno estado { get; set; }

        [Display(Name = "Resumen Visita")]
        [DataType(DataType.MultilineText)]
        [StringLength(999, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        public string observacion { get; set; }

        [Display(Name = "Posible Cliente")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        public string posibleCliente { get; set; }

        public DateTime fechaCreacion { get; set; }

        public string usuarioCreador { get; set; }

        public virtual AspNetUsers vendedor { get; set; }

        public virtual Cliente cliente { get; set; }

        public virtual Sucursal sucursal { get; set; }
        public bool softdelete { get; set; }
    }

    public enum EstadoSalidaTerreno
    {
        [Display(Name = "Programado")] Programado,
        [Display(Name = "Realizado")] Realizado,
        [Display(Name = "Cancelado")] Cancelado,
        [Display(Name = "Reprogramado")] Reprogramado
    }
}