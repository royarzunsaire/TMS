using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class FacturaStorage
    {
        [Key]
        public int idFacturaStorage { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime dateUpload { get; set; }
        public virtual Storage file { get; set; }
        public virtual AspNetUsers userUpload { get; set; }
        public virtual Factura factura { get; set; }
    }
}