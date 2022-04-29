using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Categoria")]
    public class Categoria
    {
        [Key]
        public int idCategoria { get; set; }
        [Display(Name = "Nombre")]
        [StringLength(25, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Nombre { get; set; }
        public bool softDelete { get; set; }
        public virtual ICollection<Inventario> inventario { get; set; }
        public virtual ICollection<InventarioCaracteristicas> InventarioCaracteristicas { get; set; }
    }
}