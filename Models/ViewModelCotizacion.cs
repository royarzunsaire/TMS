using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class ViewModelCotizacion
    {


        public Cotizacion_R13 cotizacion { get; set; }
        public virtual IEnumerable<Cliente> clientes { get; set; }
        public IEnumerable<Giro> giro { get; set; }
        public IEnumerable<Contacto> encargadosPagos { get; set; }
        public IEnumerable<Contacto> representantesLegales { get; set; }
        public IEnumerable<Contacto> contactosEmpresa { get; set; }
        public IEnumerable<ListaDetalleCosto> detalleCostos { get; set; }

        [Display(Name = "Tipo de Ejecución")]
        public TipoEjecucion tipoEjecucion { get; set; }

        public ICollection<CotizacionAporteCapacitacion> cotizacionAporteCapacitacion { get; set; }

        public ICollection<CotizacionAporteCapacitacion> cotizacionAporteCapacitacionPresencial { get; set; }

        public ICollection<CotizacionAporteCapacitacion> cotizacionAporteCapacitacionSincronico { get; set; }

        public ICollection<CotizacionAporteCapacitacion> cotizacionAporteCapacitacionAsincronico { get; set; }

        public ICollection<CotizacionAporteCapacitacion> cotizacionAporteCapacitacionRecertificacion { get; set; }

        public ICollection<CotizacionAporteCapacitacion> cotizacionAporteCapacitacionRecertificacionSincrono { get; set; }

        public ICollection<CotizacionAporteCapacitacion> cotizacionAporteCapacitacionRecertificacionAsincronico { get; set; }
    }
}