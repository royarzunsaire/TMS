using System.Collections.Generic;

namespace SGC.Models
{
    public class ViewModelPanelGerencia
    {
        // ------------------------------- ventas mes por sucursal --------------------------------
        public List<ViewModelSucursal> sucursal { get; set; }
        public int ventasMesMonto { get; set; }
        public int ventasMes { get; set; }
        // ----------------------------------- ventas por vendedor --------------------------------
        public List<ViewModelVendedor> vendedores { get; set; }
        public int ventasVendedorMontoMes { get; set; }
        public int ventasVendedorMetaMes { get; set; }
        public int ventasVendedorMontoAnio { get; set; }
        public int ventasVendedorMetaAnio { get; set; }
        // ------------------------------- comercializaciones en proceso ---------------------------
        public List<ViewModelSucursalVentasNoTerminadas> ventasNoTerminadasSucursal { get; set; }
        public int? ventasComercializacionesNoTerminadasMes { get; set; }
        public int? valorComercializacionesNoTerminadasMes { get; set; }
        public int? ventasComercializacionesNoTerminadasAnio { get; set; }
        public int? valorComercializacionesNoTerminadasAnio { get; set; }
        // ------------------------------------ ventas facturadas ----------------------------------
        public List<ViewModelFacturadasSucursal> facturadasSucursal { get; set; }
        public int facturadas { get; set; }
        public int? montoFacturadas { get; set; }
        // ------------------------------------ clientes nuevos ----------------------------------
        public List<ViewModelClientesNuevos> clientesNuevosSucursal { get; set; }
        public int clientesNuevosMes { get; set; }
        public int clientesNuevosMetaMes { get; set; }
        public int clientesNuevosAnio { get; set; }
        public int clientesNuevosMetaAnio { get; set; }
        // ------------------------------------ clientes nuevos ----------------------------------

        public int clientesCount { get; set; }
        public int clientesCountConCompra { get; set; }
        public int clientesCountContactos { get; set; }
        public int clientesRepresentante { get; set; }
    
        // ---------------------------------- Visitas a terreno ---------------------------------
        public List<SalidaTerreno> visitasTerrenoMensual { get; set; }
        public List<SalidaTerreno> visitasTerrenoAnual { get; set; }
        public int visitasTerrenoRealizadasMensual { get; set; }
        public int visitasTerrenoRealizadasAnual { get; set; }
        public int visitasTerrenoProgramadasMensual { get; set; }
        public int visitasTerrenoProgramadasAnual { get; set; }
        // ------------------------------- efectividad visitas a terreno --------------------------
        public List<ViewModelEfectividadVisitasVendedor> efectividadVisitasVendedor { get; set; }
        public int cantEfectividadVisitasMensual { get; set; }
        public int montoEfectividadVisitasMensual { get; set; }
        public int cantEfectividadVisitasAnual { get; set; }
        public int montoEfectividadVisitasAnual { get; set; }
        // ------------------------------------- Relatores Nuevos ---------------------------------
        public List<Relator> nuevosRelatores { get; set; }
        // ---------------------------------------- nuevos r11 ------------------------------------
        public int cantR11Mensual { get; set; }
        // ---------------------------------- nuevos cursos completos -----------------------------
        public int cantNuevosCursosCmpletosMensual { get; set; }
        // ----------------------------------- nuevos relatores sence -----------------------------
        public List<RelatorCurso> nuevosRelatoresSenceMensual { get; set; }
        public List<RelatorCurso> nuevosRelatoresSenceAnual { get; set; }
    }

    public class ViewModelSucursal
    {
        public string nombre { get; set; }
        public int? VentasCursoMonto { get; set; }
        public int VentasCurso { get; set; }
        public int? VentasRecertificacionMonto { get; set; }
        public int VentasRecertificacion { get; set; }
        public int? VentasPrecontratoMonto { get; set; }
        public int VentasPrecontrato { get; set; }
        public int? VentasComunitarioMonto { get; set; }
        public int VentasComunitario { get; set; }
        public int? VentasDuplicadoCredencialMonto { get; set; }
        public int VentasDuplicadoCredencial { get; set; }
        public int? VentasArriendoMonto { get; set; }
        public int VentasArriendo { get; set; }
        public int? VentasTotalMonto { get; set; }
        public int VentasTotal { get; set; }
    }

    public class ViewModelVendedor
    {
        public string nombre { get; set; }
        public int metaMensual { get; set; }
        public int metaAnual { get; set; }
        public int cantVentasMensual { get; set; }
        public int cantVentasAnual { get; set; }
        public int? montoMensual { get; set; }
        public int? montoAnual { get; set; }
    }

    public class ViewModelSucursalVentasNoTerminadas
    {
        public string nombre { get; set; }
        public int? montoMensual { get; set; }
        public int? cantVentasMensuales { get; set; }
        public int? montoAnual { get; set; }
        public int? cantVentasAnuales { get; set; }
    }

    public class ViewModelFacturadasSucursal
    {
        public string nombre { get; set; }
        public int? monto { get; set; }
        public int cantidad { get; set; }
    }

    public class ViewModelClientesNuevos
    {
        public string nombre { get; set; }
        public int metaMensual { get; set; }
        public int metaAnual { get; set; }
        public int cantidadMensual { get; set; }
        public int cantidadAnual { get; set; }
    }

    public class ViewModelEfectividadVisitasVendedor
    {
        public string nombre { get; set; }
        public int montoMensual { get; set; }
        public int cantidadMensual { get; set; }
        public int montoAnual { get; set; }
        public int cantidadAnual { get; set; }
    }

    public class ViewModelContactosClientes
    {
        public IEnumerable<ClienteContacto> clienteContacto { get; set; }
        public IEnumerable<RepresentanteLegal> representanteLegal { get; set; }
        public IEnumerable<EncargadoPago> encargadoPago { get; set; }
    }

    public class ViewModelComercializacionDocComromiso
    {
        public Comercializacion comercializacion { get; set; }
        public DocumentoCompromiso documentoCompromiso { get; set; }
    }

    //public class ViewModelRelatorR52
    //{
    //    public Relator relator { get; set; }
    //    public R52 r52 { get; set; }
    //}

    //public class ViewModelRelatorR52R19
    //{
    //    public ViewModelRelatorR52 relatorR52 { get; set; }
    //    public R52 r19 { get; set; }
    //}
}