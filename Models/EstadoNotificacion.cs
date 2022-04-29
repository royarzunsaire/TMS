using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class EstadoNotificacion
    {
        [Key]
        public int idEstadoNotificacion { get; set; }
        public NombreEstadoNotificacion nombre { get; set; }
        public DateTime fecha { get; set; }
    }

    public enum NombreEstadoNotificacion
    {
        [Display(Name = "Anulado")] Anulado,
        [Display(Name = "Enviado")] Enviado,
        [Display(Name = "Visto")] Visto,
        [Display(Name = "Leído")] Leido
    }
}