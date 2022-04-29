using System.Collections.Generic;

namespace SGC.Models
{
    public class ViewModelContacto
    {
        public Contacto _contacto { get; set; }
        public Cliente _cliente { get; set; }
        public string _idUsuario { get; set; }
        public string _idCliente { get; set; }
        public IEnumerable<AspNetUsers> _aspnetusers { get; set; }
    }


}