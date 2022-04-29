using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class DetalleTitulo
    {
        [Key]
        public int idDetalleTitulo { get; set; }

        [DisplayName("Descripcion")]
        public string nombre { get; set; }

        [DisplayName("Institución")]
        public string institucion { get; set; }

        [DisplayName("Año")]
        public int fecha { get; set; }

        public Storage storage { get; set; }
    }
}