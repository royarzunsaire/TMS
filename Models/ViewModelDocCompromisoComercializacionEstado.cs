namespace SGC.Models
{
    public class ViewModelDocCompromisoComercializacionEstado
    {
        public ViewModelDocCompromisoComercializacion docCompromisoComercializacion { get; set; }

        public ComercializacionEstadoComercializacion estado { get; set; }
    }

    public class ViewModelDocCompromisoComercializacion
    {
        public DocumentoCompromiso documentoCompromiso { get; set; }

        public Comercializacion comercializacion { get; set; }
    }
}