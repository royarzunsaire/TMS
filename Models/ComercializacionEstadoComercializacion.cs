using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class ComercializacionEstadoComercializacion
    {
        [Key]
        public int idComercializacionEstadoComercializacion { get; set; }
        public EstadoComercializacion EstadoComercializacion { get; set; }
        public DateTime fechaCreacion { get; set; }
        public string usuarioCreador { get; set; }
        public virtual Comercializacion comercializacion { get; set; }
    }

    public enum EstadoComercializacion
    {
        [Display(Name = "En Proceso")] En_Proceso,
        [Display(Name = "Terminada")] Terminada,
        [Display(Name = "Cancelada")] Cancelada,
        [Display(Name = "Terminada SENCE")] Terminada_SENCE,
        [Display(Name = "Deshabilitada")] Deshabilitada,
        [Display(Name = "Borrador")] Borrador
    }
}