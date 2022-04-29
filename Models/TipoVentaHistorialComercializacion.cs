using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class TipoVentaHistorialComercializacion
    {
        [Key]
        public int idTipoVentaHistorialComercializacion { get; set; }
        [Display(Name = "Tipo de Venta")]
        public string tipoVenta { get; set; }
        [Display(Name = "OTIC")]
        public string otic { get; set; }
        [Display(Name = "Documento")]
        public string documento { get; set; }
        [Display(Name = "Número de Serie")]
        public string numeroSerie { get; set; }
        [Display(Name = "Monto")]
        public string monto { get; set; }
        [Display(Name = "Nombre Archivo")]
        public string nombreArchivo { get; set; }
    }
}