using System.Collections.Generic;

namespace SGC.Models
{
    public class ViewModelClienteContacto
    {
        public ClienteContacto clienteContacto { get; set; }
        public Cliente cliente { get; set; }
        public IEnumerable<Contacto> contactos { get; set; }
        public IEnumerable<clienteContactos_result> clienteContactos { get; set; }
    }
    public class clienteContactos_result
    {
        public ClienteContacto _ClienteContacto { get; set; }
        public Contacto _Contacto { get; set; }
    }
}