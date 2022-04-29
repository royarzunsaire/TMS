using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGC.Models
{
    public class ViewModelAspNetUsersHistorial
    {
        public AspNetUsers Usuarios { get; set; }
        public Contacto Contactos { get; set; }
        public Cliente Clientes { get; set; }
        public ClienteContacto Representantes { get; set; }
    }
}