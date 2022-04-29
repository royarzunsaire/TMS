using Quartz;
using SGC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace SGC.Utils.ScheduledTask
{
    public class JobNoche : IJob
    {
        private InsecapContext db = new InsecapContext();

        public Task Execute(IJobExecutionContext context)
        {
            var task = Task.Run(() =>
            {
                AlertaPostVenta();
                AlertaEncuestaSatisfaccion();
            });

            return task;
        }

        // alerta post venta en 3 meses sin ventas
        private void AlertaPostVenta()
        {
            var clientes = db.Cliente
                .Where(x => x.softDelete == false)
                .Join(
                    db.Comercializacion
                        .Where(x => x.softDelete == false),
                    cliente => cliente.idCliente,
                    comercializacion => comercializacion.cotizacion.cliente.idCliente,
                    (cliente, comercializacion) => new
                    {
                        cliente,
                        comercializacion
                    }
                )
                .GroupBy(x => x.cliente.idCliente)
                .Select(x => x.OrderByDescending(y => y.comercializacion.fechaTermino).FirstOrDefault())
                .ToList();
            foreach (var cliente in clientes)
            {
                if (DateTime.Compare(cliente.comercializacion.fechaTermino.AddMonths(3).Date, DateTime.Now.Date) == 0)
                {
                    // notificacion cliente post venta
                    //var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta de Cliente Post Venta").FirstOrDefault();
                    //if (notificacionConfig != null)
                    //{
                    //    notificacionConfig.CrearNotificacion(db, cliente.cliente.nombreEmpresa, cliente.cliente.idCliente.ToString(), "");
                    //}
                    cliente.cliente.postVenta = true;
                    db.Entry(cliente.cliente).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        // alerta encuesta satisfaccion cada 6 meses
        private void AlertaEncuestaSatisfaccion()
        {
            var soloClientes = db.Cliente
                .Where(x => x.softDelete == false)
                .ToList();
            foreach (var cliente in soloClientes)
            {
                if (DateTime.Compare(cliente.fechaAlertaEncuestaSatisfaccion.AddMonths(6).Date, DateTime.Now.Date) == 0)
                {
                    cliente.fechaAlertaEncuestaSatisfaccion = DateTime.Now;
                    if (CienteTieneComercializacionPresencial(cliente))
                    {
                        cliente.encuestaSatisfaccion = true;
                        // notificacion cliente post venta
                        //var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta de Encuesta de Satisfacción Cliente").FirstOrDefault();
                        //if (notificacionConfig != null)
                        //{
                        //    notificacionConfig.CrearNotificacion(db, cliente.nombreEmpresa, cliente.idCliente.ToString(), "");
                        //}
                    }
                    if (CienteTieneComercializacionElerning(cliente))
                    {
                        cliente.encuestaSatisfaccionElerning = true;
                        // notificacion cliente post venta
                        //var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta de Encuesta de Satisfacción Cliente").FirstOrDefault();
                        //if (notificacionConfig != null)
                        //{
                        //    notificacionConfig.CrearNotificacion(db, cliente.nombreEmpresa, cliente.idCliente.ToString(), "");
                        //}
                    }
                    db.Entry(cliente).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        private bool CienteTieneComercializacionElerning(Cliente cliente)
        {
            var hoy = DateTime.Now.Date.AddMonths(-6);
            var tieneComercializacionElerning = db.Comercializacion
                .Where(x => x.softDelete == false)
                .Where(x => x.cotizacion.cliente.idCliente == cliente.idCliente)
                .Where(x => x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono
                || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono
                || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica
                || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Sincronica)
                .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) >= hoy)
                .Count();
            if (tieneComercializacionElerning > 0)
            {
                return true;
            }
            return false;
        }

        private bool CienteTieneComercializacionPresencial(Cliente cliente)
        {
            var hoy = DateTime.Now.Date.AddMonths(-6);
            var tieneComercializacionElerning = db.Comercializacion
                .Where(x => x.softDelete == false)
                .Where(x => x.cotizacion.cliente.idCliente == cliente.idCliente)
                .Where(x => x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Presencial
                || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion)
                .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) >= hoy)
                .Count();
            if (tieneComercializacionElerning > 0)
            {
                return true;
            }
            return false;
        }
    }
}