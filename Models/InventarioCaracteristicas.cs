using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("InventarioCaracteristicas")]
    public class InventarioCaracteristicas
    {
        [Key]
        public int idInventarioCaracteristica { get; set; }
        [StringLength(250, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Detalle { get; set; }
        public string Caracteristica1 { get; set; }
        public string Caracteristica2 { get; set; }
        public string Caracteristica3 { get; set; }
        public string Caracteristica4 { get; set; }
        public string Caracteristica5 { get; set; }
        public bool softDelete { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int idCategoria { get; set; }
        public virtual Categoria categoria { get; set; }
    }
}