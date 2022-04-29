using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace SGC.Models
{
    public class Storage
    {
        [Key]
        public int idStorage { get; set; }

        public string nombreArchivo { get; set; }

        public DateTime fechaSubido { get; set; }

        public int tamanioArchivo { get; set; }

        public string tipoArchivo { get; set; }

        public string key { get; set; }

        public string urlArchivo { get; set; }

        [NotMapped]
        public HttpPostedFileBase file { get; set; }
    }
}