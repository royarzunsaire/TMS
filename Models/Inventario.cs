using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Inventario")]
    public class Inventario
    {
        [Key]
        public int idInventario { get; set; }
        [Display(Name = "Código")]
        [StringLength(25, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Codigo { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime FechaCreacion { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Display(Name = "Fecha Compra")]
        public DateTime FechaCompra { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Solo numero positivos")]
        public int Stock { get; set; }
        [Display(Name = "Periodo Mantención")]
        [Range(1, int.MaxValue, ErrorMessage = "Solo numero positivos")]
        public int PeriodoMantencion { get; set; }
        public bool softDelete { get; set; }
        [Display(Name = "Categoria")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int idCategoria { get; set; }
        public virtual Categoria categoria { get; set; }
        public string usuarioCreador { get; set; }
        public virtual AspNetUsers usuario { get; set; }
    }
}