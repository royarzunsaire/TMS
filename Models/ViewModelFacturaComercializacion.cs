namespace SGC.Models
{
    public class ViewModelFacturaComercializacion
    {
        public ViewModelComercializacionEstado comercializacionEstado { get; set; }

        public ViewModelFacturaDocCompromiso facturaDocCompromiso { get; set; }
    }

    public class ViewModelFacturaDocCompromiso
    {
        public DocumentoCompromiso documentoCompromiso { get; set; }

        public ViewModelFacturaEstado facturaEstado { get; set; }
    }

    public class ViewModelFacturaEstado
    {
        public FacturaEstadoFactura estado { get; set; }

        public Factura factura { get; set; }
    }
}