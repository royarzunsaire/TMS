using System.Collections.Generic;

namespace SGC.Models
{
    public class ViewModelR11
    {
        public IList<ContenidoEspecificoR11> contenidoEspecifico { get; set; }
        public IList<EscolaridadR11> escolaridadR11 { get; set; }
        public IList<ItemContenidoEspecificoR11> itemContenidoEspecificoR11 { get; set; }
        public IList<R11> r11 { get; set; }
        public IList<CategoriaR11> categoriaR11 { get; set; }
        public IEnumerable<R51> r51 { get; set; }
        public IList<Curso> curso { get; set; }
        public R11 r11Entity { get; set; }

        public List<Contacto> instructores { get; set; }

    }
}