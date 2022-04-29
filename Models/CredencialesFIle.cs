using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class CredencialesFile
    {
      
        [Key]
        public int idCredencialesFile { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime dateUpload { get; set; }
        public virtual string description { get; set; }

        public virtual Storage file { get; set; }
        public virtual AspNetUsers userUpload { get; set; }
        public virtual Comercializacion comercializacion { get; set; }
    }
}