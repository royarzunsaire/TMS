using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class CotizacionAporteCapacitacion
    {
        [Required(ErrorMessage = "El campo Relator es obligatorio")]
        public virtual int idCotizacion { get; set; }

        [Required(ErrorMessage = "El campo Relator es obligatorio")]
        public virtual int idAporteCapacitacion { get; set; }

        public virtual Cotizacion_R13 cotizacion { get; set; }

        public virtual AporteCapacitacion aporteCapacitacion { get; set; }

        public ACargo? aCargo { get; set; }

        //[DisplayName("Insecap")]
        //public bool insecap { get; set; }

        //[DisplayName("Cliente")]
        //public bool cliente { get; set; }

        //[DisplayName("No Aplica")]
        //public bool noAplica { get; set; }

        //public AspNetUsers usuarioCreador { get; set; }

        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        //public DateTime fechaCreacion { get; set; }
    }

    public enum ACargo
    {
        Insecap,
        Cliente,
        No_Aplica
    }
}