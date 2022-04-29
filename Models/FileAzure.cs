using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class FileAzure
    {
        [Key]
        public int idStorage { get; set; }
        [DisplayName("Nombre Archivo")]
        public string nombreArchivo { get; set; }
        [DisplayName("Fecha Subida")]
        public DateTime fechaSubida { get; set; }
        [DisplayName("Tamaño de Archivo")]
        public string tamañoArchivo { get; set; }
        public int Vigencia { get; set; }

        public string UriBlob { get; set; }

    }

}