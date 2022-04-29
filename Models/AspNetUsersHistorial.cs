using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SGC.Models
{
    public class AspNetUsersHistorial
    {
        [Key]
        public virtual int idAspNetUsersHistorial { get; set; }
        public virtual AspNetUsers usuario { get; set; }
        public virtual DateTime FechaLogin { get; set; }
    }
}