using jsreport.MVC;
using jsreport.Types;
using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SGC.Controllers
{
    [Authorize]
    [CustomAuthorize(new string[] { "/PanelGerencia/" })]
    public class PanelGerenciaController : Controller
    {
        private InsecapContext db = new InsecapContext();
        private static readonly string directory = ConfigurationManager.AppSettings["directory"] + "Files/";
        private static readonly string directoryImages = ConfigurationManager.AppSettings["directory"] + "Images/";
        private static List<ViewModelComercializacionEstado> ventas;

        private List<ViewModelComercializacionEstado> Ventas()
        {
            if (ventas == null)
            {
                ventas = GetVentas();
            }
            return ventas;
        }

        // -------------------------------------------------------- GENERAR DATA DESDE BASE DE DATOS -------------------------------------------------------- //

        private List<ViewModelComercializacionEstadoCosto> DataVentasPorSucursal(DateTime fechaInicio, DateTime fechaTermino)
        {
            return db.Comercializacion
               .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio.Date)
               .Where(x => DbFunctions.TruncateTime(x.fechaInicio) <= fechaTermino.Date)
               .Where(c => c.softDelete == false)
               .Include(x => x.cotizacion.costo)
               .Join(
                   db.ComercializacionEstadoComercializacion
                       .GroupBy(x => x.comercializacion.idComercializacion)
                       .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                   comercializacion => comercializacion.idComercializacion,
                   estado => estado.comercializacion.idComercializacion,
                   (comercializacion, estado) => new ViewModelComercializacionEstadoCosto()
                   {
                       comercializacion = comercializacion,
                       estado = estado,
                       costo = comercializacion.cotizacion.costo.ToList()

                   }
               )
               .Where(x => x.comercializacion.usuarioCreador != null)
               .Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Borrador)
               .ToList();
        }

        private List<ViewModelComercializacionEficiencia> DataEficacia(DateTime fechaInicio, DateTime fechaTermino)
        {
            List<ViewModelComercializacionEficiencia> data = db.Cotizacion_R13
               .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) >= fechaInicio.Date)
               .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) <= fechaTermino.Date)
               .Where(c => c.softDelete == false).Select(x => new ViewModelComercializacionEficiencia { cotizacion = x }).ToList();
            List<int> cotizaciones = data.Select(x => x.cotizacion.idCotizacion_R13).ToList();
            var comercializaciones = db.Comercializacion.Where(x => cotizaciones.Contains(x.cotizacion.idCotizacion_R13)).ToList();
            foreach (ViewModelComercializacionEficiencia item in data)
            {
                item.comercializacion = comercializaciones.Where(x => x.cotizacion.idCotizacion_R13 == item.cotizacion.idCotizacion_R13).FirstOrDefault();
            }
            return data;
        }

        private List<ViewModelComercializacionEficiencia> DataComercializados(DateTime fechaInicio, DateTime fechaTermino)
        {
            List<ViewModelComercializacionEficiencia> data = db.Cotizacion_R13
               .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) >= fechaInicio.Date)
               .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) <= fechaTermino.Date)
               .Where(c => c.softDelete == false).Select(x => new ViewModelComercializacionEficiencia { cotizacion = x }).ToList();
            List<int> cotizaciones = data.Select(x => x.cotizacion.idCotizacion_R13).ToList();
            var comercializaciones = db.Comercializacion.Where(x => cotizaciones.Contains(x.cotizacion.idCotizacion_R13)).ToList();
            foreach (ViewModelComercializacionEficiencia item in data)
            {
                item.comercializacion = comercializaciones.Where(x => x.cotizacion.idCotizacion_R13 == item.cotizacion.idCotizacion_R13).FirstOrDefault();
            }
            return data;
        }

        private List<Cotizacion_R13> DataKpi(DateTime fechaInicio, DateTime fechaTermino)
        {
            return db.Cotizacion_R13
                .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) >= fechaInicio.Date)
                .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) <= fechaTermino.Date)
                .Where(c => c.softDelete == false && c.curso != null)
                .ToList();
        }

        private List<Comercializacion> DataFacturaRealizada(DateTime fechaInicio, DateTime fechaTermino)
        {
            return db.Comercializacion
                .Where(c => c.softDelete == false)
                .Where(x => x.cotizacion.documentosCompromiso.Any(dc => dc.softDelete == false && dc.factura != null && dc.factura.softDelete == false))
                .Where(x => x.cotizacion.documentosCompromiso.Any(dc => DbFunctions.TruncateTime(dc.factura.fechaFacturacion) >= fechaInicio.Date))
                .Where(x => x.cotizacion.documentosCompromiso.Any(dc => DbFunctions.TruncateTime(dc.factura.fechaFacturacion) <= fechaTermino.Date))
                .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada
                || x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada_SENCE).ToList();
        }

        private List<Comercializacion> DataFacturaRealizadaCerradasFacturadas(DateTime fechaInicio, DateTime fechaTermino)
        {
            return db.Comercializacion
                .Where(c => c.softDelete == false)
                //.Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio.Date)
                //.Where(x => DbFunctions.TruncateTime(x.fechaTermino) <= fechaTermino.Date)
                .Where(x => (x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada
                || x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada_SENCE)
                && (DbFunctions.TruncateTime(x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().fechaCreacion) >= fechaInicio.Date
                && DbFunctions.TruncateTime(x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().fechaCreacion) <= fechaTermino.Date)).ToList();
        }

        private List<ViewModelPostCurso> DataCursosSense(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = db.Comercializacion
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) <= fechaTermino)
                .Where(x => x.cotizacion.tipoCurso == "Curso" || x.cotizacion.tipoCurso == "Recertificación")
                .Where(c => c.softDelete == false)
                .Join(
                    db.ComercializacionEstadoComercializacion
                    .GroupBy(x => x.comercializacion.idComercializacion)
                    .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                    comercializacion => comercializacion.idComercializacion,
                    estado => estado.comercializacion.idComercializacion,
                        (comercializacion, estado) => new ViewModelComercializacionEstado()
                        {
                            comercializacion = comercializacion,
                            estado = estado
                        }
                 )
                .Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Borrador && x.estado.EstadoComercializacion != EstadoComercializacion.Cancelada)
                .ToList();
            var viewModel = ventas.Select(x => new ViewModelPostCurso { comercializacion = x.comercializacion, estado = x.estado, postCurso = null }).ToList();
            var postCurso = db.PostCurso.Where(x => x.comercializacion.softDelete == false)
                .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaInicio) >= fechaInicio)
                .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaInicio) <= fechaTermino)
                .ToList();
            viewModel.ForEach(x => x.postCurso = postCurso.FirstOrDefault(y => y.comercializacion.idComercializacion == x.comercializacion.idComercializacion));
            return viewModel;
        }

        private List<ViewModelPostCurso> DataCursosPresenciales(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = db.Comercializacion
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) <= fechaTermino)
                .Where(x => x.cotizacion.tipoCurso == "Curso" || x.cotizacion.tipoCurso == "Recertificación")
                .Where(x => x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Presencial || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion)
                .Where(c => c.softDelete == false)
                .Join(
                    db.ComercializacionEstadoComercializacion
                    .GroupBy(x => x.comercializacion.idComercializacion)
                    .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                    comercializacion => comercializacion.idComercializacion,
                    estado => estado.comercializacion.idComercializacion,
                        (comercializacion, estado) => new ViewModelComercializacionEstado()
                        {
                            comercializacion = comercializacion,
                            estado = estado
                        }
                 )
                .Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Borrador && x.estado.EstadoComercializacion != EstadoComercializacion.Cancelada)
                .ToList();
            var viewModel = ventas.Select(x => new ViewModelPostCurso { comercializacion = x.comercializacion, estado = x.estado, postCurso = null }).ToList();
            var postCurso = db.PostCurso.Where(x => x.comercializacion.softDelete == false)
                .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaInicio) >= fechaInicio)
                .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaInicio) <= fechaTermino)
                .ToList();
            viewModel.ForEach(x => x.postCurso = postCurso.FirstOrDefault(y => y.comercializacion.idComercializacion == x.comercializacion.idComercializacion));
            return viewModel;
        }

        private List<ViewModelPostCurso> DataCursosSincronicos(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = db.Comercializacion
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) <= fechaTermino)
                .Where(x => x.cotizacion.tipoCurso == "Curso" || x.cotizacion.tipoCurso == "Recertificación")
                .Where(x => x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Sincronica)
                .Where(c => c.softDelete == false)
                .Join(
                    db.ComercializacionEstadoComercializacion
                    .GroupBy(x => x.comercializacion.idComercializacion)
                    .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                    comercializacion => comercializacion.idComercializacion,
                    estado => estado.comercializacion.idComercializacion,
                        (comercializacion, estado) => new ViewModelComercializacionEstado()
                        {
                            comercializacion = comercializacion,
                            estado = estado
                        }
                 )
                .Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Borrador && x.estado.EstadoComercializacion != EstadoComercializacion.Cancelada)
                .ToList();
            var viewModel = ventas.Select(x => new ViewModelPostCurso { comercializacion = x.comercializacion, estado = x.estado, postCurso = null }).ToList();
            var postCurso = db.PostCurso.Where(x => x.comercializacion.softDelete == false)
                .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaInicio) >= fechaInicio)
                .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaInicio) <= fechaTermino)
                .ToList();
            viewModel.ForEach(x => x.postCurso = postCurso.FirstOrDefault(y => y.comercializacion.idComercializacion == x.comercializacion.idComercializacion));
            return viewModel;
        }

        private List<ViewModelPostCurso> DataCursosAsincronicos(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = db.Comercializacion
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) <= fechaTermino)
                .Where(x => x.cotizacion.tipoCurso == "Curso" || x.cotizacion.tipoCurso == "Recertificación")
                .Where(x => x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica)
                .Where(c => c.softDelete == false)
                .Join(
                    db.ComercializacionEstadoComercializacion
                    .GroupBy(x => x.comercializacion.idComercializacion)
                    .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                    comercializacion => comercializacion.idComercializacion,
                    estado => estado.comercializacion.idComercializacion,
                        (comercializacion, estado) => new ViewModelComercializacionEstado()
                        {
                            comercializacion = comercializacion,
                            estado = estado
                        }
                 )
                .Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Borrador && x.estado.EstadoComercializacion != EstadoComercializacion.Cancelada)
                .ToList();
            var viewModel = ventas.Select(x => new ViewModelPostCurso { comercializacion = x.comercializacion, estado = x.estado, postCurso = null }).ToList();
            var postCurso = db.PostCurso.Where(x => x.comercializacion.softDelete == false)
                .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaInicio) >= fechaInicio)
                .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaInicio) <= fechaTermino)
                .ToList();
            viewModel.ForEach(x => x.postCurso = postCurso.FirstOrDefault(y => y.comercializacion.idComercializacion == x.comercializacion.idComercializacion));
            return viewModel;
        }

        private List<ViewModelPostCurso> DataCursosR24Presenciales(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = db.Comercializacion
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) <= fechaTermino)
                .Where(x => x.cotizacion.tipoCurso == "Curso" || x.cotizacion.tipoCurso == "Recertificación")
                .Where(x => x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Presencial || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion)
                .Where(c => c.softDelete == false)
                .Join(
                    db.ComercializacionEstadoComercializacion
                    .GroupBy(x => x.comercializacion.idComercializacion)
                    .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                    comercializacion => comercializacion.idComercializacion,
                    estado => estado.comercializacion.idComercializacion,
                        (comercializacion, estado) => new ViewModelComercializacionEstado()
                        {
                            comercializacion = comercializacion,
                            estado = estado
                        }
                 )
                .Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Borrador && x.estado.EstadoComercializacion != EstadoComercializacion.Cancelada)
                .ToList();
            var viewModel = ventas.Select(x => new ViewModelPostCurso { comercializacion = x.comercializacion, estado = x.estado, postCurso = null }).ToList();
            var postCurso = db.PostCurso.Where(x => x.comercializacion.softDelete == false)
                .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaInicio) >= fechaInicio)
                .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaInicio) <= fechaTermino)
                .ToList();
            viewModel.ForEach(x => x.postCurso = postCurso.FirstOrDefault(y => y.comercializacion.idComercializacion == x.comercializacion.idComercializacion));
            return viewModel;
        }

        private List<ViewModelPostCurso> DataCursosDJOOnline(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = db.Comercializacion
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) <= fechaTermino)
                .Where(x => x.cotizacion.tipoCurso == "Curso" || x.cotizacion.tipoCurso == "Recertificación")
                .Where(x => x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono)
                .Where(x => x.cotizacion.codigoSence != null && x.cotizacion.codigoSence != "" && x.cotizacion.tieneCodigoSence != "on")
                .Where(c => c.softDelete == false)
                .Join(
                    db.ComercializacionEstadoComercializacion
                    .GroupBy(x => x.comercializacion.idComercializacion)
                    .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                    comercializacion => comercializacion.idComercializacion,
                    estado => estado.comercializacion.idComercializacion,
                        (comercializacion, estado) => new ViewModelComercializacionEstado()
                        {
                            comercializacion = comercializacion,
                            estado = estado
                        }
                 )
                .Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Borrador && x.estado.EstadoComercializacion != EstadoComercializacion.Cancelada)
                .ToList();
            var viewModel = ventas.Select(x => new ViewModelPostCurso { comercializacion = x.comercializacion, estado = x.estado, postCurso = null }).ToList();
            var postCurso = db.PostCurso.Where(x => x.comercializacion.softDelete == false)
                .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaInicio) >= fechaInicio)
                .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaInicio) <= fechaTermino)
                .ToList();
            viewModel.ForEach(x => x.postCurso = postCurso.FirstOrDefault(y => y.comercializacion.idComercializacion == x.comercializacion.idComercializacion));
            return viewModel;
        }

        private List<ViewModelAspNetUsersHistorial> DataLogin(DateTime fechaInicio, DateTime fechaTermino)
        {
            var historial = (from usuarios in db.AspNetUsers
                             join contacto in db.Contacto on usuarios.Id equals contacto.usuario.Id into usuarioContacto
                             from contacto in usuarioContacto.DefaultIfEmpty()
                             join clienteContacto in db.ClienteContacto on contacto.idContacto equals clienteContacto.idContacto into clienteContactoContacto
                             from clienteContacto in clienteContactoContacto.DefaultIfEmpty()
                             join cliente in db.Cliente on clienteContacto.idCliente equals cliente.idCliente into clienteClienteContacto
                             from cliente in clienteClienteContacto.DefaultIfEmpty()
                             select new ViewModelAspNetUsersHistorial
                             {
                                 Usuarios = usuarios,
                                 Contactos = contacto,
                                 Representantes = clienteContacto,
                                 Clientes = cliente,
                             }).ToList();

            historial = historial.Where(x => x.Usuarios.AspNetRoles.Any(y => y.Name.Contains("Representante Empresa")))
                .Where(x => x.Clientes != null)
                .OrderByDescending(x => x.Usuarios.AspNetUsersHistorial.Where(y => y.FechaLogin >= fechaInicio && y.FechaLogin <= fechaTermino).Count())
                .ThenBy(x => x.Clientes.nombreEmpresa).ThenBy(x => x.Usuarios.nombreCompleto)
                .ToList();

            return historial;
        }

        private List<ViewModelAspNetUsersHistorial> DataLoginComercializados(DateTime fechaInicio, DateTime fechaTermino)
        {
            var historial = (from comercializaciones in db.Comercializacion
                             join cotizacion in db.Cotizacion_R13 on comercializaciones.cotizacion.idCotizacion_R13 equals cotizacion.idCotizacion_R13 into comerCotizacion
                             from cotizacion in comerCotizacion.DefaultIfEmpty()
                             join cliente in db.Cliente on cotizacion.idCliente equals cliente.idCliente into cotizacionCliente
                             from cliente in cotizacionCliente.DefaultIfEmpty()
                             join clienteContacto in db.ClienteContacto on cliente.idCliente equals clienteContacto.idCliente into clienteClienteContacto
                             from clienteContacto in clienteClienteContacto.DefaultIfEmpty()
                             join contacto in db.Contacto on clienteContacto.idContacto equals contacto.idContacto into contactoCliente
                             from contacto in contactoCliente.DefaultIfEmpty()
                             join usuarios in db.AspNetUsers on contacto.usuario.Id equals usuarios.Id into contactoUsuarios
                             from usuarios in contactoUsuarios.DefaultIfEmpty()
                             where comercializaciones.softDelete == false
                             && (comercializaciones.fechaCreacion >= fechaInicio && comercializaciones.fechaCreacion <= fechaTermino)
                             select new ViewModelAspNetUsersHistorial
                             {
                                 Usuarios = usuarios,
                                 Contactos = contacto,
                                 Representantes = clienteContacto,
                                 Clientes = cliente,
                             }).ToList();

            historial = historial.Where(x => x.Usuarios != null)
                .Where(x => x.Usuarios.AspNetRoles.Any(y => y.Name.Contains("Representante Empresa")))
                .Where(x => x.Clientes != null)
                .OrderByDescending(x => x.Usuarios.AspNetUsersHistorial.Where(y => y.FechaLogin >= fechaInicio && y.FechaLogin <= fechaTermino).Count())
                .ThenBy(x => x.Clientes.nombreEmpresa).ThenBy(x => x.Usuarios.nombreCompleto)
                .ToList();

            return historial;
        }

        private List<AspNetUsers> GetVendedores()
        {
            var permisosRol = db.Permission.Where(x => x.Menu.MenuURL == "/Cotizacion_R13/" || x.Menu.MenuURL == "/Cotizacion_R13/Create/").ToList();
            var permisosUsuario = db.CustomPermission.Where(x => x.Menu.MenuURL == "/Cotizacion_R13/" || x.Menu.MenuURL == "/Cotizacion_R13/Create/").ToList();
            var vendedores = new List<AspNetUsers>();
            foreach (var permiso in permisosRol)
            {
                foreach (var usuario in permiso.AspNetRoles.AspNetUsers)
                {
                    if (!vendedores.Contains(usuario))
                    {
                        vendedores.Add(usuario);
                    }
                }
            }
            foreach (var permiso in permisosUsuario)
            {
                if (!vendedores.Contains(permiso.AspNetUsers))
                {
                    vendedores.Add(permiso.AspNetUsers);
                }
            }
            return vendedores;
        }

        // -------------------------------------------------------- GENERAR VISTA DE REPORTES -------------------------------------------------------- //

        [HttpGet]
        public ActionResult VentasPorSucursalReporte(string fechaInicio, string fechaTermino)
        {
            var ventas = DataVentasPorSucursal(Convert.ToDateTime(fechaInicio), Convert.ToDateTime(fechaTermino));
            ViewBag.sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            ViewBag.vendedores = GetVendedores();
            var jsonResult = Json(new { data = ConvertPartialViewToString(PartialView("VentasPorSucursalReporte", ventas)) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpGet]
        public ActionResult EficaciaReporte(string fechaInicio, string fechaTermino)
        {
            var ventas = DataEficacia(Convert.ToDateTime(fechaInicio), Convert.ToDateTime(fechaTermino));
            ViewBag.vendedores = GetVendedores();
            var jsonResult = Json(new { data = ConvertPartialViewToString(PartialView("EficaciaReporte", ventas)) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpGet]
        public ActionResult ComercializadosReporte(string fechaInicio, string fechaTermino)
        {
            var vigente = DateTime.Now.AddDays(-90);

            var ventas = DataComercializados(Convert.ToDateTime(fechaInicio), Convert.ToDateTime(fechaTermino));
            ViewBag.vendedores = GetVendedores();
            ViewBag.comercializados = db.Comercializacion
               .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) >= vigente)
               .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) <= DateTime.Today)
               .Where(c => c.softDelete == false).ToList();
            ViewBag.comercializaciones = db.Comercializacion.Where(x => x.softDelete == false).ToList();
            var jsonResult = Json(new { data = ConvertPartialViewToString(PartialView("ComercializadosReporte", ventas)) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpGet]
        public ActionResult KpiReporte(string fechaInicio, string fechaTermino)
        {
            var ventas = DataKpi(Convert.ToDateTime(fechaInicio), Convert.ToDateTime(fechaTermino));
            ViewBag.vendedores = GetVendedores();
            var jsonResult = Json(new { data = ConvertPartialViewToString(PartialView("KpiReporte", ventas)) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpGet]
        public ActionResult CursosSenseReporte(string fechaInicio, string fechaTermino)
        {
            var ventas = DataCursosSense(Convert.ToDateTime(fechaInicio), Convert.ToDateTime(fechaTermino));
            ViewBag.Sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            var jsonResult = Json(new { data = ConvertPartialViewToString(PartialView("CursosSenseReporte", ventas)) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpGet]
        public ActionResult CursosPresencialesReporte(string fechaInicio, string fechaTermino)
        {
            var ventas = DataCursosPresenciales(Convert.ToDateTime(fechaInicio), Convert.ToDateTime(fechaTermino));
            ViewBag.Sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            var jsonResult = Json(new { data = ConvertPartialViewToString(PartialView("CursosPresencialesReporte", ventas)) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpGet]
        public ActionResult CursosSincronicosReporte(string fechaInicio, string fechaTermino)
        {
            var ventas = DataCursosSincronicos(Convert.ToDateTime(fechaInicio), Convert.ToDateTime(fechaTermino));
            ViewBag.Sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            var jsonResult = Json(new { data = ConvertPartialViewToString(PartialView("CursosSincronicosReporte", ventas)) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpGet]
        public ActionResult CursosAsincronicosReporte(string fechaInicio, string fechaTermino)
        {
            var ventas = DataCursosAsincronicos(Convert.ToDateTime(fechaInicio), Convert.ToDateTime(fechaTermino));
            ViewBag.Sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            var jsonResult = Json(new { data = ConvertPartialViewToString(PartialView("CursosAsincronicosReporte", ventas)) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpGet]
        public ActionResult CursosR24PresencialesReporte(string fechaInicio, string fechaTermino)
        {
            var ventas = DataCursosR24Presenciales(Convert.ToDateTime(fechaInicio), Convert.ToDateTime(fechaTermino));
            ViewBag.Sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            var jsonResult = Json(new { data = ConvertPartialViewToString(PartialView("CursosR24PresencialesReporte", ventas)) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpGet]
        public ActionResult CursosDJOOnlineReporte(string fechaInicio, string fechaTermino)
        {
            var ventas = DataCursosDJOOnline(Convert.ToDateTime(fechaInicio), Convert.ToDateTime(fechaTermino));
            ViewBag.Sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            var jsonResult = Json(new { data = ConvertPartialViewToString(PartialView("CursosDJOOnlineReporte", ventas)) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpGet]
        public ActionResult LoginReporte(string fechaInicio, string fechaTermino)
        {
            DateTime fecha_Inicio = Convert.ToDateTime(fechaInicio);
            DateTime fecha_Termino = Convert.ToDateTime(fechaTermino);
            var usuarios = DataLogin(fecha_Inicio, fecha_Termino);
            ViewBag.historial = db.AspNetUsersHistorial
                .Where(x => DbFunctions.TruncateTime(x.FechaLogin) >= fecha_Inicio)
                .Where(x => DbFunctions.TruncateTime(x.FechaLogin) <= fecha_Termino)
                .ToList();
            var jsonResult = Json(new { data = ConvertPartialViewToString(PartialView("LoginReporte", usuarios)) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpGet]
        public ActionResult LoginComercializadosReporte(string fechaInicio, string fechaTermino)
        {
            DateTime fecha_Inicio = Convert.ToDateTime(fechaInicio);
            DateTime fecha_Termino = Convert.ToDateTime(fechaTermino);
            var usuarios = DataLoginComercializados(fecha_Inicio, fecha_Termino);
            ViewBag.historial = db.AspNetUsersHistorial
                .Where(x => DbFunctions.TruncateTime(x.FechaLogin) >= fecha_Inicio)
                .Where(x => DbFunctions.TruncateTime(x.FechaLogin) <= fecha_Termino)
                .ToList();
            var jsonResult = Json(new { data = ConvertPartialViewToString(PartialView("LoginComercializadosReporte", usuarios)) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        // -------------------------------------------------------- GENERAR VISTA DE REPORTES A EXCEL -------------------------------------------------------- //

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VentasPorSucursal(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = DataVentasPorSucursal(fechaInicio, fechaTermino);

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"ventas_sucursal_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");

            ViewBag.sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            ViewBag.vendedores = GetVendedores();
            ViewBag.fechaInicio = fechaInicio.ToString("dd-MM-yyyy");
            ViewBag.fechaTermino = fechaTermino.ToString("dd-MM-yyyy");
            return View(ventas);
        }

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eficacia(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = DataEficacia(fechaInicio, fechaTermino);

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Eficacia_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");

            ViewBag.vendedores = GetVendedores();
            ViewBag.fechaInicio = fechaInicio.ToString("dd-MM-yyyy");
            ViewBag.fechaTermino = fechaTermino.ToString("dd-MM-yyyy");
            return View(ventas);
        }

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Comercializados(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = DataComercializados(fechaInicio, fechaTermino);
            var vigente = DateTime.Now.AddDays(-90);

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Clientes_Vigentes_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");

            ViewBag.comercializados = db.Comercializacion
               .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) >= vigente)
               .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) <= DateTime.Today)
               .Where(c => c.softDelete == false).ToList();
            ViewBag.comercializaciones = db.Comercializacion.Where(x => x.softDelete == false).ToList();
            ViewBag.vendedores = GetVendedores();
            ViewBag.fechaInicio = fechaInicio.ToString("dd-MM-yyyy");
            ViewBag.fechaTermino = fechaTermino.ToString("dd-MM-yyyy");
            return View(ventas);
        }

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Kpi(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = DataKpi(fechaInicio, fechaTermino);
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Cotizacion_tipoEjecucion_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");

            ViewBag.vendedores = GetVendedores();
            ViewBag.fechaInicio = fechaInicio.ToString("dd-MM-yyyy"); ;
            ViewBag.fechaTermino = fechaTermino.ToString("dd-MM-yyyy");
            return View(ventas);
        }

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FacturaRealizada(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = DataFacturaRealizada(fechaInicio, fechaTermino);
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Cotizacion_tipoEjecucion_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");

            return View(ventas);
        }

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FacturaRealizadaCerradasFacturadas(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = DataFacturaRealizadaCerradasFacturadas(fechaInicio, fechaTermino);
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Cotizacion_tipoEjecucion_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");

            return View(ventas);
        }

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CursosSense(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = DataCursosSense(fechaInicio, fechaTermino);

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Cursos_Sense_Por_Sucursal_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");

            ViewBag.sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            ViewBag.fechaInicio = fechaInicio.ToString("dd-MM-yyyy");
            ViewBag.fechaTermino = fechaTermino.ToString("dd-MM-yyyy");
            return View(ventas);
        }

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CursosPresenciales(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = DataCursosPresenciales(fechaInicio, fechaTermino);

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Cursos_Presenciales_Sense_Por_Sucursal_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");

            ViewBag.sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            ViewBag.fechaInicio = fechaInicio.ToString("dd-MM-yyyy");
            ViewBag.fechaTermino = fechaTermino.ToString("dd-MM-yyyy");
            return View(ventas);
        }

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CursosSincronicos(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = DataCursosSincronicos(fechaInicio, fechaTermino);

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Cursos_Sincronicos_Sense_Por_Sucursal_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");

            ViewBag.sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            ViewBag.fechaInicio = fechaInicio.ToString("dd-MM-yyyy");
            ViewBag.fechaTermino = fechaTermino.ToString("dd-MM-yyyy");
            return View(ventas);
        }

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CursosAsincronicos(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = DataCursosAsincronicos(fechaInicio, fechaTermino);

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Cursos_Asincronicos_Sense_Por_Sucursal_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");

            ViewBag.sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            ViewBag.fechaInicio = fechaInicio.ToString("dd-MM-yyyy");
            ViewBag.fechaTermino = fechaTermino.ToString("dd-MM-yyyy");
            return View(ventas);
        }

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CursosR24Presenciales(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = DataCursosR24Presenciales(fechaInicio, fechaTermino);

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Cursos_DJO_Presenciales_Por_Sucursal_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");

            ViewBag.sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            ViewBag.fechaInicio = fechaInicio.ToString("dd-MM-yyyy");
            ViewBag.fechaTermino = fechaTermino.ToString("dd-MM-yyyy");
            return View(ventas);
        }

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CursosDJOOnline(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = DataCursosDJOOnline(fechaInicio, fechaTermino);

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Cursos_DJO_Online_Por_Sucursal_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");

            ViewBag.sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            ViewBag.fechaInicio = fechaInicio.ToString("dd-MM-yyyy");
            ViewBag.fechaTermino = fechaTermino.ToString("dd-MM-yyyy");
            return View(ventas);
        }

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(DateTime fechaInicio, DateTime fechaTermino)
        {
            var usuarios = DataLogin(fechaInicio, fechaTermino);

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Control_Clientes_Activos_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");

            ViewBag.historial = db.AspNetUsersHistorial
                .Where(x => DbFunctions.TruncateTime(x.FechaLogin) >= fechaInicio)
                .Where(x => DbFunctions.TruncateTime(x.FechaLogin) <= fechaTermino)
                .ToList();

            return View(usuarios);
        }

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoginComercializados(DateTime fechaInicio, DateTime fechaTermino)
        {
            var usuarios = DataLoginComercializados(fechaInicio, fechaTermino);

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Control_Clientes_Comercialiados_Activos_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");

            ViewBag.historial = db.AspNetUsersHistorial
                .Where(x => DbFunctions.TruncateTime(x.FechaLogin) >= fechaInicio)
                .Where(x => DbFunctions.TruncateTime(x.FechaLogin) <= fechaTermino)
                .ToList();

            return View(usuarios);
        }

        [EnableJsReport()]
        [HttpGet]
        public ActionResult ParticipantesCursosPorVencer()
        {
            //Busca los participantes que se les vencerá el curso en un periodo de 1 mes.
            var participantes = db.Participante.Where(x => DateTime.Today <= DbFunctions.AddMonths(x.comercializacion.fechaTermino, x.comercializacion.vigenciaCredenciales)
            && DbFunctions.AddDays(DateTime.Today, 90) >= DbFunctions.AddMonths(x.comercializacion.fechaTermino, x.comercializacion.vigenciaCredenciales)).ToList();

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Control_Participantes_Cursos_Por_Vencer.xlsx\"");


            return View(participantes);
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //

        public ActionResult VentasReporte()
        {
            var panelGerenciaVM = new ViewModelPanelGerencia();
            var sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            var hoy = DateTime.Now;
            // ------------------------------- ventas mes por sucursal --------------------------------
            panelGerenciaVM.sucursal = new List<ViewModelSucursal>();

            foreach (var sucursal in sucursales)
            {
                List<ViewModelComercializacionEstado> ventasSucursal = Ventas().Where(c => c.comercializacion.fechaInicio.Month == hoy.Month).Where(c => c.comercializacion.cotizacion.sucursal.idSucursal == sucursal.idSucursal).ToList();
                var sucursalVM = new ViewModelSucursal();
                sucursalVM.nombre = sucursal.nombre;
                // Get ventas MOnto
                sucursalVM.VentasCursoMonto = ventasSucursal.Where(c => c.comercializacion.cotizacion.tipoCurso == "Curso").Sum(x => x.comercializacion.valorFinal - x.comercializacion.descuento);
                sucursalVM.VentasRecertificacionMonto = ventasSucursal.Where(c => c.comercializacion.cotizacion.tipoCurso == "Recertificación").Sum(x => x.comercializacion.valorFinal - x.comercializacion.descuento);
                sucursalVM.VentasPrecontratoMonto = ventasSucursal.Where(c => c.comercializacion.cotizacion.tipoCurso == "Pre Contrato").Sum(x => x.comercializacion.valorFinal - x.comercializacion.descuento);
                sucursalVM.VentasComunitarioMonto = ventasSucursal.Where(c => c.comercializacion.cotizacion.tipoCurso == "Social").Sum(x => x.comercializacion.valorFinal - x.comercializacion.descuento);
                sucursalVM.VentasDuplicadoCredencialMonto = ventasSucursal.Where(c => c.comercializacion.cotizacion.tipoCurso == "Duplicado Credencial").Sum(x => x.comercializacion.valorFinal - x.comercializacion.descuento);
                sucursalVM.VentasArriendoMonto = ventasSucursal.Where(c => c.comercializacion.cotizacion.tipoCurso == "Arriendo de Sala").Sum(x => x.comercializacion.valorFinal - x.comercializacion.descuento);
                sucursalVM.VentasArriendoMonto = ventasSucursal.Where(c => c.comercializacion.cotizacion.tipoCurso == "Tramitación Licencia").Sum(x => x.comercializacion.valorFinal - x.comercializacion.descuento);


                // Get cantidad de ventas 
                sucursalVM.VentasCurso = ventasSucursal.Where(c => c.comercializacion.cotizacion.tipoCurso == "Curso").Count();
                sucursalVM.VentasRecertificacion = ventasSucursal.Where(c => c.comercializacion.cotizacion.tipoCurso == "Recertificación").Count();
                sucursalVM.VentasPrecontrato = ventasSucursal.Where(c => c.comercializacion.cotizacion.tipoCurso == "Pre Contrato").Count();
                sucursalVM.VentasComunitario = ventasSucursal.Where(c => c.comercializacion.cotizacion.tipoCurso == "Social").Count();
                sucursalVM.VentasDuplicadoCredencial = ventasSucursal.Where(c => c.comercializacion.cotizacion.tipoCurso == "Duplicado Credencial").Count();
                sucursalVM.VentasArriendo = ventasSucursal.Where(c => c.comercializacion.cotizacion.tipoCurso == "Arriendo de Sala").Count();
                sucursalVM.VentasArriendo = ventasSucursal.Where(c => c.comercializacion.cotizacion.tipoCurso == "Tramitación Licencia").Count();


                sucursalVM.VentasTotalMonto = sucursalVM.VentasCursoMonto + sucursalVM.VentasRecertificacionMonto + sucursalVM.VentasPrecontratoMonto
                    + sucursalVM.VentasComunitarioMonto + sucursalVM.VentasDuplicadoCredencialMonto + sucursalVM.VentasArriendoMonto;
                sucursalVM.VentasTotal = sucursalVM.VentasCurso + sucursalVM.VentasRecertificacion + sucursalVM.VentasPrecontrato
                    + sucursalVM.VentasComunitario + sucursalVM.VentasDuplicadoCredencial + sucursalVM.VentasArriendo;
                panelGerenciaVM.sucursal.Add(sucursalVM);


                //panelGerenciaVM.ventasMesMonto += sucursalVM.VentasTotalMonto.Value;
                //panelGerenciaVM.ventasMes += sucursalVM.VentasTotal;
            }

            List<ViewModelComercializacionEstado> ventasNoTerminadas = Ventas().Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Terminada).ToList();
            // ------------------------------- comercializaciones en proceso --------------------------
            panelGerenciaVM.ventasNoTerminadasSucursal = new List<ViewModelSucursalVentasNoTerminadas>();
            foreach (var sucursal in sucursales)
            {
                var sucursalVM = new ViewModelSucursalVentasNoTerminadas();
                sucursalVM.nombre = sucursal.nombre;

                List<ViewModelComercializacionEstado> ventasNoTerminadasSucursal = ventasNoTerminadas.Where(c => c.comercializacion.cotizacion.sucursal.idSucursal == sucursal.idSucursal).ToList();
                sucursalVM.cantVentasMensuales = ventasNoTerminadasSucursal.Where(c => c.comercializacion.fechaInicio.Month == hoy.Month && c.comercializacion.fechaInicio.Year == hoy.Year).Count();
                sucursalVM.montoMensual = ventasNoTerminadasSucursal.Where(c => c.comercializacion.fechaInicio.Month == hoy.Month && c.comercializacion.fechaInicio.Year == hoy.Year).Sum(x => x.comercializacion.valorFinal - x.comercializacion.descuento);

                sucursalVM.cantVentasAnuales = ventasNoTerminadasSucursal.Where(c => c.comercializacion.fechaInicio.Year == hoy.Year).Count();
                sucursalVM.montoAnual = ventasNoTerminadasSucursal.Where(c => c.comercializacion.fechaInicio.Year == hoy.Year).Sum(x => x.comercializacion.valorFinal - x.comercializacion.descuento);

                panelGerenciaVM.ventasNoTerminadasSucursal.Add(sucursalVM);
            }

            return PartialView("Panel/VentasReporte", panelGerenciaVM);
        }
        // GET: PanelGerencia
        public ActionResult VentasPorVendedor()
        {
            var panelGerenciaVM = new ViewModelPanelGerencia();
            List<ViewModelComercializacionEstado> ventasNoTerminadas = GetVentas().Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Terminada).ToList();
            var hoy = DateTime.Now;
            //-----------------------------------ventas por vendedor --------------------------------
            panelGerenciaVM.vendedores = new List<ViewModelVendedor>();
            var vendedores = GetVendedores();
            List<ViewModelComercializacionEstado> ventasAnuales = Ventas().Where(c => c.comercializacion.cotizacion.tipoCurso != "Pre Contrato" && c.comercializacion.cotizacion.tipoCurso != "Social").ToList();
            foreach (var vendedor in vendedores)
            {
                var vendedorVM = new ViewModelVendedor();
                vendedorVM.nombre = vendedor.UserName;

                vendedorVM.metaMensual = GetMetaVendedor(vendedor, "m");
                vendedorVM.metaAnual = GetMetaVendedor(vendedor, "a");

                List<ViewModelComercializacionEstado> ventasVendedor = ventasAnuales.Where(c => c.comercializacion.usuarioCreador.Id == vendedor.Id).ToList();
                //Cont de ventas de vendedor mensuales y anuales
                vendedorVM.cantVentasMensual = ventasVendedor.Where(c => c.comercializacion.fechaInicio.Month == hoy.Month).Count();
                vendedorVM.cantVentasAnual = ventasVendedor.Count();

                vendedorVM.montoMensual = ventasVendedor.Where(c => c.comercializacion.fechaInicio.Month == hoy.Month).Sum(x => x.comercializacion.valorFinal - x.comercializacion.descuento);
                vendedorVM.montoAnual = ventasVendedor.Sum(x => x.comercializacion.valorFinal - x.comercializacion.descuento);

                panelGerenciaVM.vendedores.Add(vendedorVM);
                panelGerenciaVM.ventasVendedorMontoMes += vendedorVM.montoMensual.Value;
                panelGerenciaVM.ventasVendedorMetaMes += vendedorVM.metaMensual;
                panelGerenciaVM.ventasVendedorMontoAnio += vendedorVM.montoAnual.Value;
                panelGerenciaVM.ventasVendedorMetaAnio += vendedorVM.metaAnual;
            }



            return PartialView("Panel/VentasPorVendedor", panelGerenciaVM);

        }

        // GET: PanelGerencia
        public ActionResult NuevosClientes()
        {
            var panelGerenciaVM = new ViewModelPanelGerencia();
            var hoy = DateTime.Now;

            // ------------------------------------ clientes nuevos ----------------------------------
            panelGerenciaVM.clientesNuevosSucursal = new List<ViewModelClientesNuevos>();
            List<ViewModelComercializacionEstado> comercializacionEstados = GetClientesNuevosAnual();
            panelGerenciaVM.clientesNuevosMes = comercializacionEstados.Where(c => c.comercializacion.fechaInicio.Month == hoy.Month).Count();
            panelGerenciaVM.clientesNuevosAnio = comercializacionEstados.Count();
            // ------------------------------------ clientes  ----------------------------------
            panelGerenciaVM.clientesCountConCompra = comercializacionEstados.Count();
            panelGerenciaVM.clientesCount = db.Cliente.Count();
            panelGerenciaVM.clientesCountContactos = 0;
            panelGerenciaVM.clientesRepresentante = 0;
            return PartialView("Panel/NuevosClientes", panelGerenciaVM);

        }

        // GET: PanelGerencia
        public ActionResult NuevosClientesResumen()
        {
            var panelGerenciaVM = new ViewModelPanelGerencia();
            var sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            var hoy = DateTime.Now;

            //------------------------------------clientes nuevos----------------------------------
            panelGerenciaVM.clientesNuevosSucursal = new List<ViewModelClientesNuevos>();
            List<ViewModelComercializacionEstado> comercializacionEstados = GetClientesNuevosAnual();
            foreach (var sucursal in sucursales)
            {

                var metasAnual = GetMetaClientesNuevosAnual(sucursal);
                var metaAnual = metasAnual.Sum(x => x.monto);
                var metaMensual = metasAnual.Where(x => x.mes.Month == hoy.Month).Sum(x => x.monto);
                var sucursalVM = new ViewModelClientesNuevos();
                sucursalVM.nombre = sucursal.nombre;
                sucursalVM.metaMensual = metaAnual;
                sucursalVM.metaAnual = metaMensual;
                sucursalVM.cantidadMensual = comercializacionEstados.Where(c => c.comercializacion.fechaInicio.Month == hoy.Month).Where(c => c.comercializacion.cotizacion.sucursal.idSucursal == sucursal.idSucursal).Count();
                sucursalVM.cantidadAnual = comercializacionEstados.Where(c => c.comercializacion.cotizacion.sucursal.idSucursal == sucursal.idSucursal).Count();
                panelGerenciaVM.clientesNuevosSucursal.Add(sucursalVM);
            }
            panelGerenciaVM.clientesNuevosMes = panelGerenciaVM.clientesNuevosSucursal.Sum(x => x.cantidadMensual);
            panelGerenciaVM.clientesNuevosMetaMes = panelGerenciaVM.clientesNuevosSucursal.Sum(x => x.metaMensual);
            panelGerenciaVM.clientesNuevosAnio = panelGerenciaVM.clientesNuevosSucursal.Sum(x => x.cantidadAnual);
            panelGerenciaVM.clientesNuevosMetaAnio = panelGerenciaVM.clientesNuevosSucursal.Sum(x => x.metaAnual);

            return PartialView("Panel/NuevosClientesResumen", panelGerenciaVM);

        }
        // GET: PanelGerencia
        public ActionResult FacturasMensuales()
        {
            var panelGerenciaVM = new ViewModelPanelGerencia();
            var sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            var hoy = DateTime.Now;
            // ---------------------------------- ventas facturadas ----------------------------------
            panelGerenciaVM.facturadasSucursal = new List<ViewModelFacturadasSucursal>();
            List<ViewModelFacturaComercializacion> facturaComercializacions = GetFacturadasMes();
            foreach (var sucursal in sucursales)
            {
                var sucursalVM = new ViewModelFacturadasSucursal();
                sucursalVM.nombre = sucursal.nombre;
                sucursalVM.monto = facturaComercializacions.Where(x => x.comercializacionEstado.comercializacion.cotizacion.sucursal.idSucursal == sucursal.idSucursal).Sum(x => x.facturaDocCompromiso.facturaEstado.factura.costo);
                sucursalVM.cantidad = facturaComercializacions.Where(x => x.comercializacionEstado.comercializacion.cotizacion.sucursal.idSucursal == sucursal.idSucursal).Count();
                panelGerenciaVM.facturadasSucursal.Add(sucursalVM);

            }
            panelGerenciaVM.ventasMesMonto = Convert.ToInt32(Ventas().Where(c => c.comercializacion.fechaInicio.Month == hoy.Month && c.comercializacion.fechaInicio.Year == hoy.Year).Sum(x => x.comercializacion.valorFinal - x.comercializacion.descuento));
            panelGerenciaVM.ventasMes += Convert.ToInt32(Ventas().Where(c => c.comercializacion.fechaInicio.Month == hoy.Month && c.comercializacion.fechaInicio.Year == hoy.Year).Count());

            panelGerenciaVM.facturadas = panelGerenciaVM.facturadasSucursal.Where(x => x.cantidad > 0).Count();
            panelGerenciaVM.montoFacturadas = panelGerenciaVM.facturadasSucursal.Sum(x => x.monto);

            List<ViewModelComercializacionEstado> ventasNoTerminadas = Ventas().Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Terminada).ToList();

            panelGerenciaVM.ventasComercializacionesNoTerminadasMes = ventasNoTerminadas.Where(c => c.comercializacion.fechaInicio.Month == hoy.Month && c.comercializacion.fechaInicio.Year == hoy.Year).Count();
            panelGerenciaVM.valorComercializacionesNoTerminadasMes = ventasNoTerminadas.Where(c => c.comercializacion.fechaInicio.Month == hoy.Month && c.comercializacion.fechaInicio.Year == hoy.Year).Sum(x => x.comercializacion.valorFinal - x.comercializacion.descuento);

            panelGerenciaVM.ventasComercializacionesNoTerminadasAnio = ventasNoTerminadas.Where(c => c.comercializacion.fechaInicio.Year == hoy.Year).Count();
            panelGerenciaVM.valorComercializacionesNoTerminadasAnio = ventasNoTerminadas.Where(c => c.comercializacion.fechaInicio.Year == hoy.Year).Sum(x => x.comercializacion.valorFinal - x.comercializacion.descuento);

            return PartialView("Panel/FacturasMensuales", panelGerenciaVM);

        }
        // GET: PanelGerencia
        public ActionResult VisitasTerreno()
        {
            var vendedores = GetVendedores();
            var panelGerenciaVM = new ViewModelPanelGerencia();
            var hoy = DateTime.Now;
            // ---------------------------------- Visitas a terreno ---------------------------------
            // revisar porque es lo mismo
            panelGerenciaVM.visitasTerrenoMensual = GetVisitasTerrenoMensual();
            panelGerenciaVM.visitasTerrenoAnual = GetVisitasTerrenoAnual();
            panelGerenciaVM.visitasTerrenoRealizadasMensual = GetCantVisitasTerrenoMensual().Count();
            panelGerenciaVM.visitasTerrenoRealizadasAnual = GetCantVisitasTerrenoAnual().Count();
            panelGerenciaVM.visitasTerrenoProgramadasMensual = GetVisitasTerrenoProgramadasMensual().Count();
            panelGerenciaVM.visitasTerrenoProgramadasAnual = GetVisitasTerrenoProgramadasAnual().Count();
            // ------------------------------- efectividad visitas a terreno --------------------------
            panelGerenciaVM.efectividadVisitasVendedor = new List<ViewModelEfectividadVisitasVendedor>();
            List<ViewModelComercializacionEstadoSalidaTerreno> comercializacionEstadoSalidaTerrenos = GetEfectividadVisitasAnual();
            foreach (var vendedor in vendedores)
            {
                var vendedorVM = new ViewModelEfectividadVisitasVendedor();
                vendedorVM.nombre = vendedor.UserName;
                vendedorVM.cantidadMensual = comercializacionEstadoSalidaTerrenos.Where(c => c.comercializacionEstado.comercializacion.fechaInicio.Month == hoy.Month).Where(x => x.comercializacionEstado.comercializacion.usuarioCreador.Id == vendedor.Id).Count();
                vendedorVM.montoMensual = comercializacionEstadoSalidaTerrenos.Where(c => c.comercializacionEstado.comercializacion.fechaInicio.Month == hoy.Month).Where(x => x.comercializacionEstado.comercializacion.usuarioCreador.Id == vendedor.Id)
                    .Sum(x => x.comercializacionEstado.comercializacion.valorFinal - x.comercializacionEstado.comercializacion.descuento).Value;

                vendedorVM.cantidadAnual = comercializacionEstadoSalidaTerrenos.Where(x => x.comercializacionEstado.comercializacion.usuarioCreador.Id == vendedor.Id).Count();
                vendedorVM.montoAnual = comercializacionEstadoSalidaTerrenos.Where(x => x.comercializacionEstado.comercializacion.usuarioCreador.Id == vendedor.Id)
                    .Sum(x => x.comercializacionEstado.comercializacion.valorFinal - x.comercializacionEstado.comercializacion.descuento).Value;
                panelGerenciaVM.efectividadVisitasVendedor.Add(vendedorVM);
            }
            List<ViewModelComercializacionEstadoSalidaTerreno> efectividadVisitasAnual = GetEfectividadVisitasAnual();
            panelGerenciaVM.cantEfectividadVisitasMensual = efectividadVisitasAnual.Where(c => c.comercializacionEstado.comercializacion.fechaInicio.Month == hoy.Month).Count();
            panelGerenciaVM.montoEfectividadVisitasMensual = efectividadVisitasAnual.Where(c => c.comercializacionEstado.comercializacion.fechaInicio.Month == hoy.Month)
                .Sum(x => x.comercializacionEstado.comercializacion.valorFinal - x.comercializacionEstado.comercializacion.descuento).Value;
            panelGerenciaVM.cantEfectividadVisitasAnual = efectividadVisitasAnual.Count();
            panelGerenciaVM.montoEfectividadVisitasAnual = efectividadVisitasAnual
                .Sum(x => x.comercializacionEstado.comercializacion.valorFinal - x.comercializacionEstado.comercializacion.descuento).Value;


            return PartialView("Panel/VisitasTerreno", panelGerenciaVM);

        }
        // GET: PanelGerencia
        public ActionResult NuevosRelatores()
        {
            var hoy = DateTime.Now;
            var panelGerenciaVM = new ViewModelPanelGerencia();
            //// ---------------------------------- Relatores Nuevos ------------------------------
            panelGerenciaVM.nuevosRelatores = GetNuevosRelatores();
            //// -------------------------------- nuevos relatores sence --------------------------
            var relatoresSence = GetNuevosRelatoresSenceAnual();
            panelGerenciaVM.nuevosRelatoresSenceMensual = relatoresSence.Where(x => x.fechaValidoSence.Value.Month == hoy.Month).ToList();
            panelGerenciaVM.nuevosRelatoresSenceAnual = relatoresSence;
            // ------------------------------------- nuevos r11 ---------------------------------
            panelGerenciaVM.cantR11Mensual = GetR11Mensual().Count();
            // ------------------------------- nuevos cursos completos --------------------------
            panelGerenciaVM.cantNuevosCursosCmpletosMensual = GetNuevosCursosCompletosMensual().Count();

            return PartialView("Panel/NuevosRelatores", panelGerenciaVM);

        }
        // GET: PanelGerencia
        public ActionResult NuevosRelatoresResumen()
        {
            var hoy = DateTime.Now;
            var panelGerenciaVM = new ViewModelPanelGerencia();
            //// ---------------------------------- Relatores Nuevos ------------------------------
            panelGerenciaVM.nuevosRelatores = GetNuevosRelatores();
            //// -------------------------------- nuevos relatores sence --------------------------
            var relatoresSence = GetNuevosRelatoresSenceAnual();
            panelGerenciaVM.nuevosRelatoresSenceMensual = relatoresSence.Where(x => x.fechaValidoSence.Value.Month == hoy.Month).ToList();
            panelGerenciaVM.nuevosRelatoresSenceAnual = relatoresSence;
            return PartialView("Panel/NuevosRelatoresResumen", panelGerenciaVM);

        }
        // GET: PanelGerencia
        public ActionResult VisitasTerrenoResumen()
        {
            var vendedores = GetVendedores();
            var panelGerenciaVM = new ViewModelPanelGerencia();
            var hoy = DateTime.Now;
            // ---------------------------------- Visitas a terreno ---------------------------------
            // revisar porque es lo mismo
            panelGerenciaVM.visitasTerrenoMensual = GetVisitasTerrenoMensual();
            panelGerenciaVM.visitasTerrenoAnual = GetVisitasTerrenoAnual();
            panelGerenciaVM.visitasTerrenoRealizadasMensual = GetCantVisitasTerrenoMensual().Count();
            panelGerenciaVM.visitasTerrenoRealizadasAnual = GetCantVisitasTerrenoAnual().Count();
            panelGerenciaVM.visitasTerrenoProgramadasMensual = GetVisitasTerrenoProgramadasMensual().Count();
            panelGerenciaVM.visitasTerrenoProgramadasAnual = GetVisitasTerrenoProgramadasAnual().Count();
            // ------------------------------- efectividad visitas a terreno --------------------------
            panelGerenciaVM.efectividadVisitasVendedor = new List<ViewModelEfectividadVisitasVendedor>();
            List<ViewModelComercializacionEstadoSalidaTerreno> comercializacionEstadoSalidaTerrenos = GetEfectividadVisitasAnual();
            foreach (var vendedor in vendedores)
            {
                var vendedorVM = new ViewModelEfectividadVisitasVendedor();
                vendedorVM.nombre = vendedor.UserName;
                vendedorVM.cantidadMensual = comercializacionEstadoSalidaTerrenos.Where(c => c.comercializacionEstado.comercializacion.fechaInicio.Month == hoy.Month).Where(x => x.comercializacionEstado.comercializacion.usuarioCreador.Id == vendedor.Id).Count();
                vendedorVM.montoMensual = comercializacionEstadoSalidaTerrenos.Where(c => c.comercializacionEstado.comercializacion.fechaInicio.Month == hoy.Month).Where(x => x.comercializacionEstado.comercializacion.usuarioCreador.Id == vendedor.Id)
                    .Sum(x => x.comercializacionEstado.comercializacion.valorFinal - x.comercializacionEstado.comercializacion.descuento).Value;

                vendedorVM.cantidadAnual = comercializacionEstadoSalidaTerrenos.Where(x => x.comercializacionEstado.comercializacion.usuarioCreador.Id == vendedor.Id).Count();
                vendedorVM.montoAnual = comercializacionEstadoSalidaTerrenos.Where(x => x.comercializacionEstado.comercializacion.usuarioCreador.Id == vendedor.Id)
                    .Sum(x => x.comercializacionEstado.comercializacion.valorFinal - x.comercializacionEstado.comercializacion.descuento).Value;
                panelGerenciaVM.efectividadVisitasVendedor.Add(vendedorVM);
            }
            List<ViewModelComercializacionEstadoSalidaTerreno> efectividadVisitasAnual = GetEfectividadVisitasAnual();
            panelGerenciaVM.cantEfectividadVisitasMensual = efectividadVisitasAnual.Where(c => c.comercializacionEstado.comercializacion.fechaInicio.Month == hoy.Month).Count();
            panelGerenciaVM.montoEfectividadVisitasMensual = efectividadVisitasAnual.Where(c => c.comercializacionEstado.comercializacion.fechaInicio.Month == hoy.Month)
                .Sum(x => x.comercializacionEstado.comercializacion.valorFinal - x.comercializacionEstado.comercializacion.descuento).Value;
            panelGerenciaVM.cantEfectividadVisitasAnual = efectividadVisitasAnual.Count();
            panelGerenciaVM.montoEfectividadVisitasAnual = efectividadVisitasAnual
                .Sum(x => x.comercializacionEstado.comercializacion.valorFinal - x.comercializacionEstado.comercializacion.descuento).Value;


            return PartialView("Panel/VisitasTerrenoResumen", panelGerenciaVM);

        }

        // GET: PanelGerencia
        public ActionResult Index()
        {
            ventas = GetVentas().Select(x => new ViewModelComercializacionEstado
            {
                comercializacion = new Comercializacion
                {
                    fechaInicio = x.comercializacion.fechaInicio,
                    fechaTermino = x.comercializacion.fechaTermino,
                    valorFinal = x.comercializacion.valorFinal,
                    descuento = x.comercializacion.descuento,
                    fechaCreacion = x.comercializacion.fechaCreacion,
                    usuarioCreador = new AspNetUsers { Id = x.comercializacion.usuarioCreador.Id, nombres = x.comercializacion.usuarioCreador.nombres, apellidoMaterno = x.comercializacion.usuarioCreador.apellidoMaterno, apellidoPaterno = x.comercializacion.usuarioCreador.apellidoPaterno },
                    cotizacion = new Cotizacion_R13 { tipoCurso = x.comercializacion.cotizacion.tipoCurso, sucursal = new Sucursal { idSucursal = x.comercializacion.cotizacion.sucursal.idSucursal }, cliente = new Cliente { idCliente = x.comercializacion.cotizacion.cliente.idCliente } },


                },
                estado = new ComercializacionEstadoComercializacion { EstadoComercializacion = x.estado.EstadoComercializacion }
            }).ToList();

            ViewBag.clientes = GetClientes();
            ViewBag.sucursales = GetSucursales();
            return View("Panel/Index");
        }

        // ------------------------------- ventas mes por sucursal --------------------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VentasMesSucursal(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = db.Comercializacion
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio.Date)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) <= fechaTermino.Date)
                .Where(c => c.softDelete == false)
                .Join(
                    db.ComercializacionEstadoComercializacion
                        .GroupBy(x => x.comercializacion.idComercializacion)
                        .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                    comercializacion => comercializacion.idComercializacion,
                    estado => estado.comercializacion.idComercializacion,
                    (comercializacion, estado) => new ViewModelComercializacionEstado()
                    {
                        comercializacion = comercializacion,
                        estado = estado
                    }
                )
                .Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Borrador
                    && x.estado.EstadoComercializacion != EstadoComercializacion.Cancelada)
                .ToList();
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"ventas_mes_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");
            return View(ventas);
        }

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostCurso(DateTime fechaInicio, DateTime fechaTermino)
        {
            var viewModel = db.Comercializacion
                .Include(x => x.cotizacion.sucursal)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio.Date)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) <= fechaTermino.Date)
                .Where(c => c.softDelete == false)
                .Where(x => x.cotizacion.tipoCurso == "Curso" || x.cotizacion.tipoCurso == "Recertificación")
                    .Join(
                    db.ComercializacionEstadoComercializacion
                        .GroupBy(x => x.comercializacion.idComercializacion)
                        .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                    comercializacion => comercializacion.idComercializacion,
                    estado => estado.comercializacion.idComercializacion,
                    (comercializacion, estado) => new ViewModelPostCurso()
                    {
                        comercializacion = comercializacion,
                        estado = estado,
                        postCurso = null
                    }
                )
                .Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Borrador
                    && x.estado.EstadoComercializacion != EstadoComercializacion.Cancelada)
                 .Where(x => x.comercializacion.cotizacion.sucursal.nombre != "Distancia")
                 .Where(x => x.comercializacion.cotizacion.sucursal.nombre != "SPD")
                .ToList();

            var postCurso = db.PostCurso.Where(x => x.comercializacion.softDelete == false)
                .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaInicio) >= fechaInicio.Date)
                .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaInicio) <= fechaTermino.Date)
                .ToList();

            viewModel.ForEach(x => x.postCurso = postCurso.FirstOrDefault(y => y.comercializacion.idComercializacion == x.comercializacion.idComercializacion));

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Jdo_Por_Sucursal" + fechaInicio + "_" + fechaTermino + ".xlsx\"");
            return View(viewModel);
        }

        public ActionResult Resumen()
        {
            var fechaInicio = new DateTime(2021, 01, 01);
            var fechaTermino = new DateTime(2021, 03, 01);
            ViewBag.vendedores = GetVendedores();
            ViewBag.fechaInicio = fechaInicio.ToString("dd-MM-yyyy"); ;
            ViewBag.fechaTermino = fechaTermino.ToString("dd-MM-yyyy");
            return View();
        }

        public ActionResult KPIPostCurso()
        {
            var fechaInicio = new DateTime(2021, 01, 01);
            var fechaTermino = new DateTime(2021, 03, 01);
            ViewBag.vendedores = GetVendedores();
            ViewBag.fechaInicio = fechaInicio.ToString("dd-MM-yyyy"); ;
            ViewBag.fechaTermino = fechaTermino.ToString("dd-MM-yyyy");
            return View();
        }
        public ActionResult KPITica()
        {
            var fechaInicio = new DateTime(2021, 01, 01);
            var fechaTermino = new DateTime(2021, 03, 01);
            ViewBag.fechaInicio = fechaInicio.ToString("dd-MM-yyyy"); ;
            ViewBag.fechaTermino = fechaTermino.ToString("dd-MM-yyyy");
            return View();
        }

        public string ConvertPartialViewToString(PartialViewResult partialView)
        {
            using (var sw = new StringWriter())
            {
                partialView.View = ViewEngines.Engines
                  .FindPartialView(ControllerContext, partialView.ViewName).View;

                var vc = new ViewContext(
                  ControllerContext, partialView.View, partialView.ViewData, partialView.TempData, sw);
                partialView.View.Render(vc, sw);

                var partialViewString = sw.GetStringBuilder().ToString();

                return partialViewString;
            }
        }

        private List<ViewModelComercializacionEstado> GetVentas()
        {
            var hoy = DateTime.Now;
            List<ViewModelComercializacionEstado> ventas = db.Comercializacion
                .Where(c => c.fechaInicio.Year == hoy.Year)
                .Where(c => c.softDelete == false)
                .Include(x => x.cotizacion)
                .Include(x => x.usuarioCreador)
                .Include(x => x.cotizacion.sucursal)
                .Join(
                    db.ComercializacionEstadoComercializacion
                        .GroupBy(x => x.comercializacion.idComercializacion)
                        .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                    comercializacion => comercializacion.idComercializacion,
                    estado => estado.comercializacion.idComercializacion,
                    (comercializacion, estado) => new ViewModelComercializacionEstado()
                    {
                        comercializacion = comercializacion,
                        estado = estado
                    }
                )
                .Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Borrador
                    && x.estado.EstadoComercializacion != EstadoComercializacion.Cancelada)
                 .Where(x => x.comercializacion.cotizacion.sucursal.nombre != "Distancia")
                 .Where(x => x.comercializacion.cotizacion.sucursal.nombre != "SPD")
                .ToList();
            return ventas;
        }


        // ----------------------------------- ventas por vendedor --------------------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VentasVendedor(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = new List<ViewModelComercializacionEstado>();
            ventas = db.Comercializacion
                .Include(x => x.cotizacion.sucursal)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio.Date)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) <= fechaTermino.Date)
                .Where(c => c.softDelete == false)
                    .Join(
                    db.ComercializacionEstadoComercializacion
                        .GroupBy(x => x.comercializacion.idComercializacion)
                        .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                    comercializacion => comercializacion.idComercializacion,
                    estado => estado.comercializacion.idComercializacion,
                    (comercializacion, estado) => new ViewModelComercializacionEstado()
                    {
                        comercializacion = comercializacion,
                        estado = estado
                    }
                )
                .Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Borrador
                    && x.estado.EstadoComercializacion != EstadoComercializacion.Cancelada)
                 .Where(x => x.comercializacion.cotizacion.sucursal.nombre != "Distancia")
                 .Where(x => x.comercializacion.cotizacion.sucursal.nombre != "SPD")
                .ToList();
            var nombreArchivo = fechaInicio + "_" + fechaTermino;
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"ventas_vendedor_" + nombreArchivo + ".xlsx\"");
            return View(ventas);
        }


        private int GetMetaVendedor(AspNetUsers vendedor, String periodo)
        {
            var hoy = DateTime.Now;
            Meta meta = new Meta();
            if (periodo == "m")
            {
                meta = db.Meta
               .Where(x => x.metasVendedor.vendedor.Id == vendedor.Id)
               .Where(x => x.mes.Month == hoy.Month && x.mes.Year == hoy.Year)
               .FirstOrDefault();
                if (meta != null)
                {
                    return meta.monto;
                }
            }
            else if (periodo == "a")
            {
                return db.Meta
                  .Where(x => x.metasVendedor.vendedor.Id == vendedor.Id)
                  .Where(x => x.mes.Year == hoy.Year)
                  .GroupBy(x => x.metasVendedor.vendedor.Id)
                  .Select(x => x.Sum(y => y.monto))
                  .FirstOrDefault();
            }


            return 0;
        }

        // ------------------------------- comercializaciones en proceso --------------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ComercializacionesProceso(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = new List<ViewModelDocCompromisoComercializacionEstado>();
            ventas = db.DocumentoCompromiso
                .Where(c => c.softDelete == false)
                .Join(
                    db.Comercializacion
                        .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio.Date)
                        .Where(x => DbFunctions.TruncateTime(x.fechaInicio) <= fechaTermino.Date)
                        .Where(c => c.softDelete == false),
                    documentoCompromiso => documentoCompromiso.cotizacion.idCotizacion_R13,
                    comercializacion => comercializacion.cotizacion.idCotizacion_R13,
                    (documentoCompromiso, comercializacion) => new ViewModelDocCompromisoComercializacion()
                    {
                        documentoCompromiso = documentoCompromiso,
                        comercializacion = comercializacion
                    }
                )
                .Join(
                    db.ComercializacionEstadoComercializacion
                        .GroupBy(x => x.comercializacion.idComercializacion)
                        .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                    docCompromisoComercializacion => docCompromisoComercializacion.comercializacion.idComercializacion,
                    estado => estado.comercializacion.idComercializacion,
                    (docCompromisoComercializacion, estado) => new ViewModelDocCompromisoComercializacionEstado()
                    {
                        docCompromisoComercializacion = docCompromisoComercializacion,
                        estado = estado
                    }
                )
                .Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Borrador
                    && x.estado.EstadoComercializacion != EstadoComercializacion.Cancelada
                    && x.estado.EstadoComercializacion != EstadoComercializacion.Terminada)
                .ToList();
            var nombreArchivo = fechaInicio + "_" + fechaTermino;
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"comercializaciones_en_proceso_" + nombreArchivo + ".xlsx\"");
            return View(ventas);
        }



        // ---------------------------------- ventas facturadas ----------------------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FacturadoMes(DateTime fechaInicio, DateTime fechaTermino)
        {
            List<ViewModelFacturaComercializacion> facturas = GetFacturadas(fechaInicio, fechaTermino);
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"facturado_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");
            return View(facturas);
        }


        private List<ViewModelFacturaComercializacion> GetFacturadasMes()
        {
            var hoy = DateTime.Now;
            return db.Factura
                .Where(x => x.fechaCreacion.Month == hoy.Month && x.fechaCreacion.Year == hoy.Year)
                .Where(x => x.softDelete == false)
                .Join(
                    db.FacturaEstadoFactura
                        .GroupBy(x => x.factura.idFactura)
                        .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                    factura => factura.idFactura,
                    estado => estado.factura.idFactura,
                    (factura, estado) => new ViewModelFacturaEstado()
                    {
                        factura = factura,
                        estado = estado
                    }
                )
                //.Where(x => x.estado.estado == EstadoFactura.Pagado)
                .Join(
                    db.DocumentoCompromiso,
                    factura => factura.factura.idFactura,
                    documentoCompromiso => documentoCompromiso.factura.idFactura,
                    (factura, documentoCompromiso) => new ViewModelFacturaDocCompromiso()
                    {
                        documentoCompromiso = documentoCompromiso,
                        facturaEstado = factura
                    }
                )
                .Join(
                    db.Comercializacion
                        .Join(
                            db.ComercializacionEstadoComercializacion
                                .GroupBy(x => x.comercializacion.idComercializacion)
                                .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                            comercializacion => comercializacion.idComercializacion,
                            estado => estado.comercializacion.idComercializacion,
                            (comercializacion, estado) => new ViewModelComercializacionEstado()
                            {
                                comercializacion = comercializacion,
                                estado = estado
                            }
                        ),
                    factura => factura.documentoCompromiso.cotizacion.idCotizacion_R13,
                    comercializacionEstado => comercializacionEstado.comercializacion.cotizacion.idCotizacion_R13,
                    (factura, comercializacionEstado) => new ViewModelFacturaComercializacion()
                    {
                        facturaDocCompromiso = factura,
                        comercializacionEstado = comercializacionEstado
                    }
                )
                .ToList();
        }

        private List<ViewModelFacturaComercializacion> GetFacturadas(DateTime fechaInicio, DateTime fechaTermino)
        {
            return db.Factura
                .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) >= fechaInicio.Date)
                .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) <= fechaTermino.Date)
                .Where(x => x.softDelete == false)
                .Join(
                    db.FacturaEstadoFactura
                        .GroupBy(x => x.factura.idFactura)
                        .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                    factura => factura.idFactura,
                    estado => estado.factura.idFactura,
                    (factura, estado) => new ViewModelFacturaEstado()
                    {
                        factura = factura,
                        estado = estado
                    }
                )
                .Where(x => x.estado.estado == EstadoFactura.Pagado)
                .Join(
                    db.DocumentoCompromiso,
                    factura => factura.factura.idFactura,
                    documentoCompromiso => documentoCompromiso.factura.idFactura,
                    (factura, documentoCompromiso) => new ViewModelFacturaDocCompromiso()
                    {
                        documentoCompromiso = documentoCompromiso,
                        facturaEstado = factura
                    }
                )
                .Join(
                    db.Comercializacion
                        .Join(
                            db.ComercializacionEstadoComercializacion
                                .GroupBy(x => x.comercializacion.idComercializacion)
                                .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                            comercializacion => comercializacion.idComercializacion,
                            estado => estado.comercializacion.idComercializacion,
                            (comercializacion, estado) => new ViewModelComercializacionEstado()
                            {
                                comercializacion = comercializacion,
                                estado = estado
                            }
                        ),
                    factura => factura.documentoCompromiso.cotizacion.idCotizacion_R13,
                    comercializacionEstado => comercializacionEstado.comercializacion.cotizacion.idCotizacion_R13,
                    (factura, comercializacionEstado) => new ViewModelFacturaComercializacion()
                    {
                        facturaDocCompromiso = factura,
                        comercializacionEstado = comercializacionEstado
                    }
                )
                .ToList();
        }

        // ------------------------------------ clientes nuevos ----------------------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClientesNuevos(DateTime fechaInicio, DateTime fechaTermino)
        {
            var ventas = new List<ViewModelComercializacionEstado>();
            var sucursales = db.Sucursal.Where(s => s.nombre != "Distancia").Where(s => s.nombre != "SPD").ToList();
            foreach (var sucursal in sucursales)
            {
                ventas.AddRange(GetClientesNuevos(sucursal, fechaInicio, fechaTermino));
            }
            var nombreArchivo = fechaInicio + "_" + fechaTermino;
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"clientes_nuevos_" + nombreArchivo + ".xlsx\"");
            return View(ventas);
        }



        private List<Meta> GetMetaClientesNuevosAnual(Sucursal sucursal)
        {
            var hoy = DateTime.Now;
            var meta = db.Meta
                .Where(x => x.metasSucursal.sucursal.idSucursal == sucursal.idSucursal)
                .Where(x => x.mes.Year == hoy.Year)
                .ToList();

            return meta;


        }




        private List<ViewModelComercializacionEstado> GetClientesNuevosAnual()
        {
            var hoy = DateTime.Now;
            return Ventas().GroupBy(x => x.comercializacion.cotizacion.cliente.idCliente)
                .Select(x => x.OrderBy(y => y.comercializacion.fechaInicio).FirstOrDefault())
                .ToList();
        }



        private List<ViewModelComercializacionEstado> GetClientesNuevos(Sucursal sucursal, DateTime fechaInicio, DateTime fechaTermino)
        {
            var hoy = DateTime.Now;
            return db.Comercializacion
                .Where(c => c.softDelete == false)
                .Where(c => c.cotizacion.sucursal.idSucursal == sucursal.idSucursal)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio.Date)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) <= fechaTermino.Date)
                .Join(
                    db.ComercializacionEstadoComercializacion
                        .GroupBy(x => x.comercializacion.idComercializacion)
                        .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                    comercializacion => comercializacion.idComercializacion,
                    estado => estado.comercializacion.idComercializacion,
                    (comercializacion, estado) => new ViewModelComercializacionEstado()
                    {
                        comercializacion = comercializacion,
                        estado = estado
                    }
                )
                .Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Borrador
                    && x.estado.EstadoComercializacion != EstadoComercializacion.Cancelada)
                .GroupBy(x => x.comercializacion.cotizacion.cliente.idCliente)
                .Select(x => x.OrderBy(y => y.comercializacion.fechaCreacion).FirstOrDefault())
                .ToList();
        }

        // ---------------------------------- Visitas a terreno ---------------------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VisitasTerreno(DateTime fechaInicio, DateTime fechaTermino)
        {
            var vistasTerreno = new List<SalidaTerreno>();
            vistasTerreno = GetCantVisitasTerreno(fechaInicio, fechaTermino);
            var nombreArchivo = fechaInicio + "_" + fechaTermino;
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"visitas_terreno_" + nombreArchivo + ".xlsx\"");
            return View(vistasTerreno);
        }

        private List<SalidaTerreno> GetVisitasTerrenoMensual()
        {
            var hoy = DateTime.Now;
            var salidas = new List<SalidaTerreno>();
            foreach (var salida in db.SalidaTerreno
               .Where(c => c.fecha.Month == hoy.Month && c.fecha.Year == hoy.Year)
               .Where(c => c.estado == EstadoSalidaTerreno.Realizado)
               .ToList())
            {
                var ingresada = false;
                foreach (var salidaUnica in salidas)
                {
                    if (salida.cliente != null && salidaUnica.cliente != null)
                    {
                        if (salida.cliente.idCliente == salidaUnica.cliente.idCliente)
                        {
                            ingresada = true;
                        }
                    }
                    else
                    {
                        if (salida.posibleCliente == salidaUnica.posibleCliente)
                        {
                            ingresada = true;
                        }
                    }
                }
                if (!ingresada)
                {
                    salidas.Add(salida);
                }
            }
            return salidas;
        }

        private List<SalidaTerreno> GetCantVisitasTerrenoMensual()
        {
            var hoy = DateTime.Now;
            return db.SalidaTerreno
               .Where(c => c.fecha.Month == hoy.Month && c.fecha.Year == hoy.Year)
               .Where(c => c.estado == EstadoSalidaTerreno.Realizado)
               .ToList();
        }

        private List<SalidaTerreno> GetVisitasTerrenoProgramadasMensual()
        {
            var hoy = DateTime.Now;
            return db.SalidaTerreno
               .Where(c => c.fecha.Month == hoy.Month && c.fecha.Year == hoy.Year)
               .ToList();
        }

        private List<SalidaTerreno> GetVisitasTerrenoAnual()
        {
            var hoy = DateTime.Now;
            var salidas = new List<SalidaTerreno>();
            foreach (var salida in db.SalidaTerreno
               .Where(c => c.fecha.Year == hoy.Year)
               .Where(c => c.estado == EstadoSalidaTerreno.Realizado)
               .ToList())
            {
                var ingresada = false;
                foreach (var salidaUnica in salidas)
                {
                    if (salida.cliente != null && salidaUnica.cliente != null)
                    {
                        if (salida.cliente.idCliente == salidaUnica.cliente.idCliente)
                        {
                            ingresada = true;
                        }
                    }
                    else
                    {
                        if (salida.posibleCliente == salidaUnica.posibleCliente)
                        {
                            ingresada = true;
                        }
                    }
                }
                if (!ingresada)
                {
                    salidas.Add(salida);
                }
            }
            return salidas;
        }

        private List<SalidaTerreno> GetCantVisitasTerrenoAnual()
        {
            var hoy = DateTime.Now;
            return db.SalidaTerreno
               .Where(c => c.fecha.Year == hoy.Year)
               .Where(c => c.estado == EstadoSalidaTerreno.Realizado)
               .ToList();
        }

        private List<SalidaTerreno> GetVisitasTerrenoProgramadasAnual()
        {
            var hoy = DateTime.Now;
            return db.SalidaTerreno
               .Where(c => c.fecha.Year == hoy.Year)
               .ToList();
        }

        private List<SalidaTerreno> GetCantVisitasTerreno(DateTime fechaInicio, DateTime fechaTermino)
        {
            return db.SalidaTerreno
                .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) >= fechaInicio.Date)
                .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) <= fechaTermino.Date)
                .Where(c => c.estado == EstadoSalidaTerreno.Realizado)
                .ToList();
        }

        // ------------------------------- efectividad visitas a terreno --------------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EfectividadVisitasTerreno(DateTime fechaInicio, DateTime fechaTermino)
        {
            var comercializaciones = new List<ViewModelComercializacionEstadoSalidaTerreno>();
            comercializaciones = GetEfectividadVisitas(fechaInicio, fechaTermino);
            var nombreArchivo = fechaInicio + "_" + fechaTermino;
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"efectividad_visitas_terreno_" + nombreArchivo + ".xlsx\"");
            return View(comercializaciones);
        }



        private List<ViewModelComercializacionEstadoSalidaTerreno> GetEfectividadVisitasAnual()
        {
            var hoy = DateTime.Now;
            return db.Comercializacion
                .Where(c => c.fechaInicio.Year == hoy.Year)
                .Where(c => c.softDelete == false)
                .Include(x => x.cotizacion.sucursal)
                .Join(
                     db.ComercializacionEstadoComercializacion
                         .GroupBy(x => x.comercializacion.idComercializacion)
                         .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                     comercializacion => comercializacion.idComercializacion,
                     estado => estado.comercializacion.idComercializacion,
                     (comercializacion, estado) => new ViewModelComercializacionEstado()
                     {
                         comercializacion = comercializacion,
                         estado = estado
                     }
                 )
                 .Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Borrador
                     && x.estado.EstadoComercializacion != EstadoComercializacion.Cancelada)
                 .Join(
                     db.SalidaTerreno,
                     comercializacionEstado => comercializacionEstado.comercializacion.cotizacion.cliente.idCliente,
                     salidaTerreno => salidaTerreno.cliente.idCliente,
                     (comercializacionEstado, salidaTerreno) => new ViewModelComercializacionEstadoSalidaTerreno()
                     {
                         comercializacionEstado = comercializacionEstado,
                         salidaTerreno = salidaTerreno
                     }
                 )
                 .Where(x => x.salidaTerreno.estado == EstadoSalidaTerreno.Realizado)
                 .ToList()
                 .Where(x => x.salidaTerreno.fecha >= x.comercializacionEstado.comercializacion.fechaCreacion.AddMonths(-3))
                 .GroupBy(x => x.comercializacionEstado.comercializacion.idComercializacion)
                 .Select(x => x.FirstOrDefault())
                 .Where(x => x.comercializacionEstado.comercializacion.cotizacion.sucursal.nombre != "Distancia")
                 .Where(x => x.comercializacionEstado.comercializacion.cotizacion.sucursal.nombre != "SPD")
                 .ToList();
        }

        private List<ViewModelComercializacionEstadoSalidaTerreno> GetEfectividadVisitas(DateTime fechaInicio, DateTime fechaTermino)
        {
            return db.Comercializacion
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio.Date)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) <= fechaTermino.Date)
                .Where(c => c.softDelete == false)
                .Join(
                    db.ComercializacionEstadoComercializacion
                        .GroupBy(x => x.comercializacion.idComercializacion)
                        .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                    comercializacion => comercializacion.idComercializacion,
                    estado => estado.comercializacion.idComercializacion,
                    (comercializacion, estado) => new ViewModelComercializacionEstado()
                    {
                        comercializacion = comercializacion,
                        estado = estado
                    }
                )
                .Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Borrador
                    && x.estado.EstadoComercializacion != EstadoComercializacion.Cancelada)
                .Join(
                    db.SalidaTerreno,
                    comercializacionEstado => comercializacionEstado.comercializacion.cotizacion.cliente.idCliente,
                    salidaTerreno => salidaTerreno.cliente.idCliente,
                    (comercializacionEstado, salidaTerreno) => new ViewModelComercializacionEstadoSalidaTerreno()
                    {
                        comercializacionEstado = comercializacionEstado,
                        salidaTerreno = salidaTerreno
                    }
                )
                .Where(x => x.salidaTerreno.estado == EstadoSalidaTerreno.Realizado)
                .ToList()
                .Where(x => x.salidaTerreno.fecha >= x.comercializacionEstado.comercializacion.fechaCreacion.AddMonths(-3))
                .GroupBy(x => x.comercializacionEstado.comercializacion.idComercializacion)
                .Select(x => x.FirstOrDefault())
                .ToList();
        }

        // ------------------------------- Relatores Nuevos --------------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NuevosRelatores(DateTime fechaInicio, DateTime fechaTermino)
        {
            var relatores = GetNuevosRelatoresPorFecha(fechaInicio, fechaTermino);

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"nuevos_relatores_" + fechaInicio + "_" + fechaTermino + ".xlsx\"");
            return View(relatores);
        }

        private List<Relator> GetNuevosRelatores()
        {
            var hoy = DateTime.Now;
            return db.Relators
                    .Where(c => c.softDelete == false)
                    .ToList()
                    .Where(x => x.fechaCreacion >= hoy.AddMonths(-6))
                    .ToList();
        }

        private List<Relator> GetNuevosRelatoresPorFecha(DateTime fechaInicio, DateTime fechaTermino)
        {
            return db.Relators
                    .Where(c => c.softDelete == false)
                    .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) >= fechaInicio.Date)
                    .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) <= fechaTermino.Date)
                    .ToList();
        }

        // ------------------------------- nuevos r11 --------------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NuevosR11(DateTime fechaInicio, DateTime fechaTermino)
        {
            var r11s = new List<ViewModelCursoR11>();
            r11s = GetR11(fechaInicio, fechaTermino);
            var nombreArchivo = fechaInicio + "_" + fechaTermino;
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"nuevos_r11_" + nombreArchivo + ".xlsx\"");
            return View(r11s);
        }

        private List<ViewModelCursoR11> GetR11Mensual()
        {
            var hoy = DateTime.Now;
            return db.R11
                .Where(c => c.fechaCreacion.Month == hoy.Month && c.fechaCreacion.Year == hoy.Year)
                .Where(c => c.softDelete == false)
                .Join(
                    db.Curso,
                    r11 => r11.idCurso,
                    curso => curso.idCurso,
                    (r11, curso) => new ViewModelCursoR11()
                    {
                        r11 = r11,
                        curso = curso
                    }
                ).ToList();
        }

        //private List<ViewModelCursoR11> GetR11Anual()
        //{
        //    var hoy = DateTime.Now;
        //    return db.R11
        //        .Where(c => c.fechaCreacion.Year == hoy.Year)
        //        .Where(c => c.softDelete == false)
        //        .Join(
        //            db.Curso,
        //            r11 => r11.idCurso,
        //            curso => curso.idCurso,
        //            (r11, curso) => new ViewModelCursoR11()
        //            {
        //                r11 = r11,
        //                curso = curso
        //            }
        //        ).ToList();
        //}

        private List<ViewModelCursoR11> GetR11(DateTime fechaInicio, DateTime fechaTermino)
        {
            return db.R11
                .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) >= fechaInicio.Date)
                .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) <= fechaTermino.Date)
                .Where(c => c.softDelete == false)
                .Join(
                    db.Curso,
                    r11 => r11.idCurso,
                    curso => curso.idCurso,
                    (r11, curso) => new ViewModelCursoR11()
                    {
                        r11 = r11,
                        curso = curso
                    }
                ).ToList();
        }

        // ------------------------------- nuevos cursos completos --------------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NuevosCursosCompletos(DateTime fechaInicio, DateTime fechaTermino)
        {
            var cursos = new List<ViewModelCursoR51>();
            cursos = GetNuevosCursosCompletosMensual();
            var nombreArchivo = fechaInicio + "_" + fechaTermino;
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"nuevos_cursos_completos_" + nombreArchivo + ".xlsx\"");
            return View(cursos);
        }

        private List<ViewModelCursoR51> GetNuevosCursosCompletosMensual()
        {
            var hoy = DateTime.Now;
            return db.Curso
                .Where(c => c.softDelete == false)
                .Where(c => c.materialCompleto)
                .Join(
                   db.R51,
                   curso => curso.idCurso,
                   r51 => r51.idCurso,
                   (curso, r51) => new ViewModelCursoR51()
                   {
                       curso = curso,
                       r51 = r51
                   }
                ).Where(c => c.curso.fechaValidacionMaterial.Value.Month == hoy.Month && c.curso.fechaValidacionMaterial.Value.Year == hoy.Year)
                .ToList();
        }

        //private List<ViewModelCursoR51> GetNuevosCursosCompletos(DateTime fechaInicio, DateTime fechaTermino)
        //{
        //    return db.Curso
        //        .Where(c => c.softDelete == false)
        //        .Where(c => c.materialCompleto)
        //        .Join(
        //           db.R51,
        //           curso => curso.idCurso,
        //           r51 => r51.idCurso,
        //           (curso, r51) => new ViewModelCursoR51()
        //           {
        //               curso = curso,
        //               r51 = r51
        //           }
        //        )
        //        .Where(x => DbFunctions.TruncateTime(x.curso.fechaValidacionMaterial.Value) >= fechaInicio.Date)
        //        .Where(x => DbFunctions.TruncateTime(x.curso.fechaValidacionMaterial.Value) <= fechaTermino.Date)
        //        .ToList();
        //}

        // ------------------------------- nuevos relatores sence --------------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NuevosRelatoresSence(DateTime fechaInicio, DateTime fechaTermino)
        {
            var relatorCurso = new List<RelatorCurso>();
            relatorCurso = GetNuevosRelatoresSence(fechaInicio, fechaTermino);
            var nombreArchivo = fechaInicio + "_" + fechaTermino;
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"nuevos_relatores_sence_" + nombreArchivo + ".xlsx\"");
            return View(relatorCurso);
        }

        //private List<RelatorCurso> GetNuevosRelatoresSenceMensual()
        //{
        //    var hoy = DateTime.Now;
        //    return db.RelatorCurso
        //        .Where(c => c.softDelete == false)
        //        .Where(c => c.validoSence)
        //        .Where(c => c.curso.softDelete == false)
        //        .Where(c => c.relator.softDelete == false)
        //        .Where(c => c.fechaValidoSence.Value.Month == hoy.Month && c.fechaValidoSence.Value.Year == hoy.Year)
        //        .ToList();
        //}

        private List<RelatorCurso> GetNuevosRelatoresSenceAnual()
        {
            var hoy = DateTime.Now;
            return db.RelatorCurso
                .Where(c => c.softDelete == false)
                .Where(c => c.validoSence)
                .Where(c => c.curso.softDelete == false)
                .Where(c => c.relator.softDelete == false)
                .Where(c => c.fechaValidoSence.Value.Year == hoy.Year)
                .ToList();
        }

        private List<RelatorCurso> GetNuevosRelatoresSence(DateTime fechaInicio, DateTime fechaTermino)
        {
            return db.RelatorCurso
                .Where(c => c.softDelete == false)
                .Where(c => c.validoSence)
                .Where(c => c.curso.softDelete == false)
                .Where(c => c.relator.softDelete == false)
                .Where(x => DbFunctions.TruncateTime(x.fechaValidoSence.Value) >= fechaInicio.Date)
                .Where(x => DbFunctions.TruncateTime(x.fechaValidoSence.Value) <= fechaTermino.Date)
                .ToList();
        }

        // ------------------------------- R04 Contactos de Clientes -----------------------
        [EnableJsReport()]
        public ActionResult DescargarR04Excel()
        {
            var contactos = GetContactosClientes();
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"R04.xlsx\"");
            return View(contactos);
        }

        private ViewModelContactosClientes GetContactosClientes()
        {
            var contactos = new ViewModelContactosClientes();
            contactos.clienteContacto = db.ClienteContacto
              .Where(x => x.contacto.softDelete == false)
              .Where(x => x.cliente.softDelete == false)
              .Where(x => x.cliente.mandante.nombreMandante.Contains("Particular") != true && x.cliente.mandante.nombreMandante.Contains("particular") != true && x.cliente.mandante.nombreMandante.Contains("PARTICULAR") != true && x.cliente.mandante.nombreMandante.Contains("mandante") != true)
              .Where(x => x.cliente.nombreEmpresa.Contains("Particular") != true && x.cliente.nombreEmpresa.Contains("particular") != true && x.cliente.nombreEmpresa.Contains("PARTICULAR") != true && x.cliente.nombreEmpresa.Contains("mandante") != true)
              .OrderBy(x => x.cliente.nombreEmpresa)
              .ThenByDescending(x => x.contacto.nombres).ThenByDescending(x => x.contacto.apellidoPaterno).ThenByDescending(x => x.contacto.correo)
              .ToList();

            return contactos;
        }
        // ------------------------------- Lista de Cotizaciones -----------------------
        [EnableJsReport()]
        [HttpGet]
        public ActionResult ReporteCotizaciones()
        {
            var cotizaciones = db.Cotizacion_R13.Where(x => x.softDelete == false).ToList();


            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Cotizaciones.xlsx\"");
            return View(cotizaciones);
        }
        // ------------------------------- Participantes de Cliente -----------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ParticipantesClienteExcel(int idCliente)
        {
            var cliente = db.Cliente.Find(idCliente);
            var participantes = GetParticipantesCliente(idCliente);
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Participantes_" + cliente.nombreEmpresa + ".xlsx\"");
            return View(participantes);
        }

        private List<Participante> GetParticipantesCliente(int idCliente)
        {
            return db.Participante
                .Where(x => x.comercializacion.cotizacion.cliente.idCliente == idCliente)
                .ToList();
        }

        // ------------------------------- Comercializaciones en Farcturacion -----------------------
        [EnableJsReport()]
        public ActionResult ComercializacionesFacturacionExcel()
        {
            var comercializaciones = GetComercializacionesFacturacion();
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Comercializaciones_Facturacion.xlsx\"");
            return View(comercializaciones);
        }

        private List<ViewModelComercializacionDocComromiso> GetComercializacionesFacturacion()
        {
            return db.Comercializacion
                .Where(x => x.softDelete == false)
                .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada
                || x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada_SENCE)
                .Join(
                   db.DocumentoCompromiso,
                   comercializacion => comercializacion.cotizacion.idCotizacion_R13,
                   documentoCompromiso => documentoCompromiso.cotizacion.idCotizacion_R13,
                   (comercializacion, documentoCompromiso) => new ViewModelComercializacionDocComromiso()
                   {
                       comercializacion = comercializacion,
                       documentoCompromiso = documentoCompromiso
                   }
                ).ToList();
        }

        // ------------------------------------- Reporte R25 --------------------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReporteR25(DateTime fechaInicio, DateTime fechaTermino)
        {
            var comercializaciones = db.Comercializacion
                .Where(x => x.softDelete == false)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio.Date)
                .Where(x => DbFunctions.TruncateTime(x.fechaTermino) <= fechaTermino.Date)
                .Where(x => x.r19.Count() != 0)
                .ToList();
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Reporte_R25.xlsx\"");
            return View(comercializaciones);
        }
        // ------------------------------------- Reporte R43 --------------------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClienteEncuesta(DateTime fechaInicio, DateTime fechaTermino, string typeEncuesta)
        {
            List<Cliente> clientes = new List<Cliente>();
            var tipo = TipoFormulario.R43_E;
            if (typeEncuesta.Equals("p"))
            {
                tipo = TipoFormulario.R43;
            }

            clientes = db.Comercializacion
                .Where(x => x.softDelete == false)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio && DbFunctions.TruncateTime(x.fechaInicio) <= fechaTermino)
                .Where(x => !x.cotizacion.cliente.encuestaSatisfaccion)
                .Where(x => x.cotizacion.cliente.r43.Any(y => y.encuesta.seccionEncuesta.Any(z => z.formulario.tipoFormulario == tipo)))
                .Select(x => x.cotizacion.cliente)
                .Distinct()
                .ToList();
            clientes.ForEach(r => r.r43 = r.r43.Where(y => y.encuesta.seccionEncuesta.Any(z => z.formulario.tipoFormulario == tipo)).ToList());
            clientes = clientes.Where(x => x.r43.Where(y => y.encuesta.seccionEncuesta.Any(z => z.formulario.tipoFormulario == tipo)).ToList().Count() > 0).ToList();
            var r43s = new List<R43>();
            var preguntas = new List<PreguntasFormulario>();

            foreach (var cliente in clientes)
            {
                foreach (var r43 in cliente.r43)
                {
                    r43s.Add(r43);
                    foreach (var seccionEncuesta in r43.encuesta.seccionEncuesta.OrderBy(f => f.posicion))
                    {
                        foreach (var preguntasFormularios in seccionEncuesta.formulario.preguntasFormularios.OrderBy(p => p.orden))
                        {
                            preguntas.Add(preguntasFormularios);
                        }
                    }
                }
            }
            ViewBag.preguntas = preguntas.Distinct().ToList();

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Reporte_R43_Encuesta_Cliente_" + tipo + ".xlsx\"");
            return View(r43s);
        }
        // GET: Participante encuesta
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Encuesta(DateTime fechaInicio, DateTime fechaTermino)
        {
            var feedbackMoodle = db.Comercializacion
                 .Where(x => x.softDelete == false)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio && DbFunctions.TruncateTime(x.fechaInicio) <= fechaTermino)
                .Join(
                    db.FeedbackMoodle,
                    comercializacion => comercializacion.idComercializacion,
                    feedback => feedback.comercializacion.idComercializacion,
                    (comercializacion, feedback) => new ViewModelComercializacionFeedback()
                    {
                        comercializacion = comercializacion,
                        feedback = feedback
                    }
                ).ToList();



            HttpContext
             .JsReportFeature()
             .Recipe(Recipe.HtmlToXlsx)
             .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Encuesta_Participantes.xlsx\"");

            return View(feedbackMoodle);
        }

        // GET: Participante encuesta
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EncuestaComentarios(DateTime fechaInicio, DateTime fechaTermino)
        {
            var feedbackMoodle = db.Comercializacion
                 .Where(x => x.softDelete == false)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio && DbFunctions.TruncateTime(x.fechaInicio) <= fechaTermino)
                .Join(
                    db.FeedbackMoodle,
                    comercializacion => comercializacion.idComercializacion,
                    feedback => feedback.comercializacion.idComercializacion,
                    (comercializacion, feedback) => new ViewModelComercializacionFeedback()
                    {
                        comercializacion = comercializacion,
                        feedback = feedback
                    }
                ).ToList();



            HttpContext
             .JsReportFeature()
             .Recipe(Recipe.HtmlToXlsx)
             .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Encuesta_Participantes.xlsx\"");

            return View(feedbackMoodle);
        }
        // ------------------------------------- Reporte relatores cursos --------------------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RelatorComercializacion(DateTime fechaInicio, DateTime fechaTermino, string typeEncuesta)
        {
            List<RelatorCurso> relatorCurso = db.RelatorCurso.ToList();
            var comercializaciones = new List<Comercializacion>();
            foreach (var item in relatorCurso)
            {
                foreach (var itemC in item.comercializaciones.Where(x => x.softDelete == false)
                .Where(x => x.fechaInicio >= fechaInicio && x.fechaInicio <= fechaTermino)
               .ToList())
                {
                    itemC.cotizacion.costo = db.Costo.Where(x => x.cotizacion.idCotizacion_R13 == itemC.cotizacion.idCotizacion_R13).ToList();
                    comercializaciones.Add(itemC);
                }
            }

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Reporte_Relatores_Confirmados.xlsx\"");
            return View(comercializaciones);
        }

        // ------------------------------------------ R56 -----------------------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult R56(DateTime fechaInicio, DateTime fechaTermino)
        {
            var comercializaciones = db.Comercializacion
                .Where(x => x.softDelete == false)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio.Date)
                .Where(x => DbFunctions.TruncateTime(x.fechaTermino) <= fechaTermino.Date)
                .Where(x => x.cotizacion.calendarizacionAbierta != null)
                .ToList();
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"R56.xlsx\"");
            return View(comercializaciones);
        }

        // ------------------------------------------ R47 -----------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public FileResult R47(DateTime fechaInicio, DateTime fechaTermino, int? idSucursal)
        {
            var sucursal = db.Sucursal.Find(idSucursal);

            var relatores = GetRelatoresR47(fechaInicio, fechaTermino, sucursal);

            if ((((fechaTermino.Year - fechaInicio.Year) * 12) + fechaTermino.Month - fechaInicio.Month + 1) > 12)
            {
                return null;
            }

            return CrearExcelR47(relatores, fechaInicio, fechaTermino, sucursal);
        }

        private FileContentResult CrearExcelR47(List<Relator> relatores, DateTime fechaInicio, DateTime fechaTermino, Sucursal sucursal)
        {
            var cantMesesPeriodo = ((fechaTermino.Year - fechaInicio.Year) * 12) + fechaTermino.Month - fechaInicio.Month + 1;
            var cantPreguntasR19 = GetCantPreguntasR19(relatores, fechaInicio, fechaTermino, sucursal);
            var cantPreguntasR52 = GetCantPreguntasR52(relatores, fechaInicio, fechaTermino, sucursal);

            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Worksheets.Add("RESUMEN");

                foreach (var relator in relatores)
                {
                    excel.Workbook.Worksheets.Add(relator.contacto.nombreCompleto);
                }

                GenerarHojaResumen(excel.Workbook.Worksheets["RESUMEN"], relatores, fechaInicio, fechaTermino, sucursal);

                foreach (var relator in relatores)
                {
                    GenerarHojaRelator(excel.Workbook.Worksheets[relator.contacto.nombreCompleto], relator, fechaInicio, fechaTermino, sucursal, cantMesesPeriodo, cantPreguntasR19, cantPreguntasR52);
                }

                excel.Workbook.Properties.Title = "R47";
                //FileInfo excelFile = new FileInfo(directory + "R47.xlsx");
                return File(excel.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                //excel.SaveAs(excelFile);
            }
        }

        private void GenerarHojaRelator(ExcelWorksheet worksheet, Relator relator, DateTime fechaInicio, DateTime fechaTermino, Sucursal sucursal, int cantMesesPeriodo, int cantPreguntasR19, int cantPreguntasR52)
        {
            GenerarEstilosRelator(worksheet, cantMesesPeriodo, cantPreguntasR19, cantPreguntasR52);
            GenerarHeaderRelator(worksheet);
            GenerarDataRelator(worksheet, relator, fechaInicio, fechaTermino, sucursal, cantMesesPeriodo, cantPreguntasR19, cantPreguntasR52);
        }

        private void GenerarDataRelator(ExcelWorksheet worksheet, Relator relator, DateTime fechaInicio, DateTime fechaTermino, Sucursal sucursal, int cantMesesPeriodo, int cantPreguntasR19, int cantPreguntasR52)
        {
            var primerMes = fechaInicio.Month;
            // ---- fila ----
            var row = 3;
            // data
            var cellData = new List<object[]>();
            cellData.Add(new object[] {
                        "IDENTIFICACIÓN INSTRUCTOR"
                    });
            worksheet.Cells[row, 2].LoadFromArrays(cellData);
            // ---- fila ----
            row++;
            cellData = new List<object[]>();
            cellData.Add(new object[] {
                        "NOMBRE",
                        relator.contacto.nombreCompleto.ToUpper()
                    });
            worksheet.Cells[row, 2].LoadFromArrays(cellData);
            // ---- fila ----
            row++;
            cellData = new List<object[]>();
            cellData.Add(new object[] {
                        "CARGO / PROFESION",
                        "RELATOR " + GetProfesionRelator(relator).ToUpper()
                    });
            worksheet.Cells[row, 2].LoadFromArrays(cellData);
            // ---- fila ----
            row++;
            cellData = new List<object[]>();
            cellData.Add(new object[] {
                        "CANTIDAD DE CURSOS",
                        "EL TOTAL DE CURSOS REALIZADOS POR " + relator.contacto.nombreCompleto.ToUpper() + " EN EL AÑO O PERIODO DE EVALUACION SON " + GetCantCursosRelator(relator, fechaInicio, fechaTermino, sucursal)
                    });
            worksheet.Cells[row, 2].LoadFromArrays(cellData);
            // ---- fila ----
            row++;
            cellData = new List<object[]>();
            cellData.Add(new object[] {
                        "PERIODO DE EVALUACION",
                        fechaInicio.ToString("MMMM yyyy").ToUpper() + " - " + fechaTermino.ToString("MMMM yyyy").ToUpper()
                    });
            worksheet.Cells[row, 2].LoadFromArrays(cellData);
            // ---- fila ----
            row++;
            cellData = new List<object[]>();
            cellData.Add(new object[] {
                        "ITEMS A EVALUAR"
                    });
            worksheet.Cells[row, 2].LoadFromArrays(cellData);
            for (int j = 4; j <= cantMesesPeriodo + 3; j++)
            {
                cellData = new List<object[]>();
                cellData.Add(new object[] {
                        CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(primerMes + j - 4).ToUpper()
                    });
                worksheet.Cells[row, j].LoadFromArrays(cellData);
            }
            // ---- fila ----
            row++;
            cellData = new List<object[]>();
            cellData.Add(new object[] {
                        "RESPECTO AL RELATOR EN DESEMPEÑO DE CAPACITACION SEGÚN EVALUACION ALUMNO"
                    });
            worksheet.Cells[row, 2].LoadFromArrays(cellData);
            // ---- fila ----
            row++;
            cellData = new List<object[]>();
            cellData.Add(new object[] {
                        "Resultados Obtenidos de Encuesta del Alumno R19"
                    });
            worksheet.Cells[row, 1].LoadFromArrays(cellData);
            cellData = new List<object[]>();
            var sumRespuestasR19 = 0;
            var contRespuestasR19 = 0;
            foreach (var pregunta in GetPreguntasR19(relator, fechaInicio, fechaTermino, sucursal))
            {
                var fila = new object[2 + cantMesesPeriodo];
                fila[0] = pregunta.pregunta;
                for (int j = 2; j <= cantMesesPeriodo + 1; j++)
                {
                    var promedioRespuestas = GetPromedioMesPreguntaR19(relator, pregunta, primerMes + j - 2, fechaInicio, fechaTermino, sucursal);
                    fila[j] = promedioRespuestas;
                    sumRespuestasR19 += promedioRespuestas;
                    contRespuestasR19 += promedioRespuestas > 0 ? 1 : 0;
                }
                cellData.Add(fila);
            }
            worksheet.Cells[row, 2].LoadFromArrays(cellData);
            worksheet.Cells[row, cantMesesPeriodo + 4].Value = contRespuestasR19 > 0 ? sumRespuestasR19 / contRespuestasR19 : 0;
            // ---- fila ----
            row += cantPreguntasR19;
            cellData = new List<object[]>();
            cellData.Add(new object[] {
                        "RESPECTO AL RELATOR EN FORMA CUALITATIVA FRENTE AL DESARROLLO DE LA CLASE"
                    });
            worksheet.Cells[row, 2].LoadFromArrays(cellData);
            for (int j = 4; j <= cantMesesPeriodo + 3; j++)
            {
                cellData = new List<object[]>();
                cellData.Add(new object[] {
                        CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(primerMes + j - 4).ToUpper()
                    });
                worksheet.Cells[row, j].LoadFromArrays(cellData);
            }
            // ---- fila ----
            row++;
            cellData = new List<object[]>();
            cellData.Add(new object[] {
                        "Resultados obtenidos de Evaluación Cualitativa de Diseño y Desarrollo"
                    });
            worksheet.Cells[row, 1].LoadFromArrays(cellData);
            cellData = new List<object[]>();
            var sumRespuestasR52 = 0;
            var contRespuestasR52 = 0;
            foreach (var pregunta in GetPreguntasR52(relator, fechaInicio, fechaTermino, sucursal))
            {
                var fila = new object[2 + cantMesesPeriodo];
                fila[0] = pregunta.pregunta;
                for (int j = 2; j <= cantMesesPeriodo + 1; j++)
                {
                    var promedioRespuestas = GetPromedioMesPreguntaR52(relator, pregunta, primerMes + j - 2, fechaInicio, fechaTermino, sucursal);
                    fila[j] = promedioRespuestas;
                    sumRespuestasR52 += promedioRespuestas;
                    contRespuestasR52 += promedioRespuestas > 0 ? 1 : 0;
                }
                cellData.Add(fila);
            }
            worksheet.Cells[row, 2].LoadFromArrays(cellData);
            worksheet.Cells[row, cantMesesPeriodo + 4].Value = contRespuestasR52 > 0 ? sumRespuestasR52 / contRespuestasR52 : 0;
            // ---- fila ----
            row += cantPreguntasR52;
            cellData = new List<object[]>();
            cellData.Add(new object[] {
                        "% ESPERADO FINAL IGUAL O SUPERIOR A 95"
                    });
            worksheet.Cells[row, 2].LoadFromArrays(cellData);
            cellData = new List<object[]>();
            var promedio19 = contRespuestasR19 > 0 ? sumRespuestasR19 / contRespuestasR19 : 0;
            var promedio52 = contRespuestasR52 > 0 ? sumRespuestasR52 / contRespuestasR52 : 0;
            cellData.Add(new object[] {
                        (promedio19 + promedio52) / 2
                    });
            worksheet.Cells[row, cantMesesPeriodo + 4].LoadFromArrays(cellData);
        }

        private void GenerarEstilosRelator(ExcelWorksheet worksheet, int cantMesesPeriodo, int cantPreguntasR19, int cantPreguntasR52)
        {
            Color colorFromHexAzul = System.Drawing.ColorTranslator.FromHtml("#000090"); // 000090 0066cc
            Color colorFromHexRojo = System.Drawing.ColorTranslator.FromHtml("#f2dcdb");
            Color colorFromHexVerde = System.Drawing.ColorTranslator.FromHtml("#d8e4bc");
            Color colorFromHexAmarillo = System.Drawing.ColorTranslator.FromHtml("#f9fdd2");
            Color colorFromHexAmarillo2 = System.Drawing.ColorTranslator.FromHtml("#ffff00");
            Color colorFromHexCeleste = System.Drawing.ColorTranslator.FromHtml("#dce6f1");
            // ---- columnas ----
            // largo
            worksheet.Column(2).Width = 32;
            worksheet.Column(3).Width = 77;
            for (int i = 4; i <= cantMesesPeriodo + 3; i++)
            {
                worksheet.Column(i).Width = 12;
            }
            // ---- fila ----
            var row = 2;
            // alto
            worksheet.Row(row).Height = 50;
            // estilos
            worksheet.Cells["C" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Font.Name = "Bookman Old Style";
            worksheet.Cells["C" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.ShrinkToFit = true;
            worksheet.Cells["C" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["C" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Font.Bold = true;
            worksheet.Cells["C" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["C" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Font.Size = 16;
            worksheet.Cells["C" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Font.Color.SetColor(System.Drawing.Color.White);
            worksheet.Cells["C" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["C" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Fill.BackgroundColor.SetColor(colorFromHexAzul);
            worksheet.Cells["B" + row + ":B" + row].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells["C" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            // merge
            worksheet.Cells["C" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Merge = true;
            // ---- fila ----
            row++;
            // alto
            worksheet.Row(row).Height = 22;
            // merge
            worksheet.Cells["B" + row + ":C" + row].Merge = true;
            worksheet.Cells["D" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Merge = true;
            // estilos
            worksheet.Cells["B" + row + ":C" + row].Style.Font.Name = "Bookman Old Style";
            worksheet.Cells["B" + row + ":C" + row].Style.ShrinkToFit = true;
            worksheet.Cells["B" + row + ":C" + row].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["B" + row + ":C" + row].Style.Font.Bold = true;
            worksheet.Cells["B" + row + ":C" + row].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["B" + row + ":C" + row].Style.Font.Size = 11;
            worksheet.Cells["B" + row + ":C" + row].Style.Font.Color.SetColor(System.Drawing.Color.White);
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Fill.BackgroundColor.SetColor(colorFromHexAzul);
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            // ---- fila ----
            row++;
            // alto
            for (int i = row; i < row + 4; i++)
            {
                worksheet.Row(i).Height = 18;
                // merge
                worksheet.Cells["C" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Merge = true;
                // estilos
                worksheet.Cells["B" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.Font.Name = "Bookman Old Style";
                worksheet.Cells["B" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.ShrinkToFit = true;
                worksheet.Cells["B" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["B" + i + ":B" + i].Style.Font.Bold = true;
                worksheet.Cells["B" + i + ":B" + i].Style.Font.Size = 9;
                worksheet.Cells["C" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.Font.Size = 10;
                worksheet.Cells["B" + i + ":C" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells["C" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            // ---- fila ----
            row += 4;
            // alto
            worksheet.Row(row).Height = 22;
            // merge
            worksheet.Cells["B" + row + ":C" + row].Merge = true;
            // estilos
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Font.Name = "Bookman Old Style";
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.ShrinkToFit = true;
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Font.Bold = true;
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["B" + row + ":C" + row].Style.Font.Size = 11;
            worksheet.Cells["D" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Font.Size = 9;
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Font.Color.SetColor(System.Drawing.Color.White);
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Fill.BackgroundColor.SetColor(colorFromHexAzul);
            worksheet.Cells["B" + row + ":C" + row].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            // otros
            for (int j = 4; j <= cantMesesPeriodo + 3; j++)
            {
                // merge
                worksheet.Cells[LetraColumna(j) + row + ":" + LetraColumna(j) + (row + 1)].Merge = true;
                // estilos
                worksheet.Cells[LetraColumna(j) + row + ":" + LetraColumna(j) + (row + 1)].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            // ---- fila ----
            row++;
            // alto
            worksheet.Row(row).Height = 22;
            // merge
            worksheet.Cells["B" + row + ":C" + row].Merge = true;
            // estilos
            worksheet.Cells["B" + row + ":C" + row].Style.Font.Name = "Bookman Old Style";
            worksheet.Cells["B" + row + ":C" + row].Style.ShrinkToFit = true;
            worksheet.Cells["B" + row + ":C" + row].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["B" + row + ":C" + row].Style.Font.Bold = true;
            worksheet.Cells["B" + row + ":C" + row].Style.Font.Size = 11;
            worksheet.Cells["B" + row + ":C" + row].Style.Font.Color.SetColor(System.Drawing.Color.White);
            worksheet.Cells["B" + row + ":C" + row].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["B" + row + ":C" + row].Style.Fill.BackgroundColor.SetColor(colorFromHexAzul);
            worksheet.Cells["B" + row + ":C" + row].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            // ---- fila ----
            row++;
            for (int i = row; i < row + cantPreguntasR19; i++)
            {
                // alto
                worksheet.Row(i).Height = 14;
                // merge
                worksheet.Cells["B" + i + ":C" + i].Merge = true;
                // estilos
                worksheet.Cells["B" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.Font.Name = "Bookman Old Style";
                worksheet.Cells["B" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.ShrinkToFit = true;
                worksheet.Cells["B" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["D" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.Font.Bold = true;
                worksheet.Cells["D" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["B" + i + ":C" + i].Style.Font.Size = 12;
                worksheet.Cells["D" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.Font.Size = 10;
                worksheet.Cells["B" + i + ":C" + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["B" + i + ":C" + i].Style.Fill.BackgroundColor.SetColor(colorFromHexRojo);
                worksheet.Cells["B" + i + ":C" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                // otros
                for (int j = 4; j <= cantMesesPeriodo + 3; j++)
                {
                    // estilos
                    worksheet.Cells[LetraColumna(j) + i + ":" + LetraColumna(j) + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }
            // merge
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR19 - 1)].Merge = true;
            // estilos
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR19 - 1)].Style.Font.Name = "Bookman Old Style";
            //worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR19 - 1)].Style.ShrinkToFit = true;
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR19 - 1)].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR19 - 1)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR19 - 1)].Style.Font.Size = 9;
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR52 - 1)].Style.TextRotation = 90;
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR52 - 1)].Style.WrapText = true;
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR19 - 1)].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR19 - 1)].Style.Fill.BackgroundColor.SetColor(colorFromHexRojo);
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR19 - 1)].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            // merge
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR19 - 1)].Merge = true;
            // estilos
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR19 - 1)].Style.Font.Name = "Bookman Old Style";
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR19 - 1)].Style.ShrinkToFit = true;
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR19 - 1)].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR19 - 1)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR19 - 1)].Style.Font.Size = 10;
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR19 - 1)].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR19 - 1)].Style.Fill.BackgroundColor.SetColor(colorFromHexCeleste);
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR19 - 1)].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            // ---- fila ----
            row += cantPreguntasR19;
            // alto
            worksheet.Row(row).Height = 22;
            // merge
            worksheet.Cells["B" + row + ":C" + row].Merge = true;
            // estilos
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Font.Name = "Bookman Old Style";
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.ShrinkToFit = true;
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Font.Bold = true;
            worksheet.Cells["D" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["B" + row + ":C" + row].Style.Font.Size = 11;
            worksheet.Cells["D" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Font.Size = 9;
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Font.Color.SetColor(System.Drawing.Color.White);
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Fill.BackgroundColor.SetColor(colorFromHexAzul);
            worksheet.Cells["B" + row + ":C" + row].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            // otros
            for (int j = 4; j <= cantMesesPeriodo + 3; j++)
            {
                // estilos
                worksheet.Cells[LetraColumna(j) + row + ":" + LetraColumna(j) + row].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            // ---- fila ----
            row++;
            for (int i = row; i < row + cantPreguntasR52; i++)
            {
                // alto
                worksheet.Row(i).Height = 14;
                // merge
                worksheet.Cells["B" + i + ":C" + i].Merge = true;
                // estilos
                worksheet.Cells["B" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.Font.Name = "Bookman Old Style";
                worksheet.Cells["B" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.ShrinkToFit = true;
                worksheet.Cells["B" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["D" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.Font.Bold = true;
                worksheet.Cells["D" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["B" + i + ":C" + i].Style.Font.Size = 12;
                worksheet.Cells["D" + i + ":" + LetraColumna(cantMesesPeriodo + 3) + i].Style.Font.Size = 10;
                worksheet.Cells["B" + i + ":C" + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["B" + i + ":C" + i].Style.Fill.BackgroundColor.SetColor(colorFromHexAmarillo);
                worksheet.Cells["B" + i + ":C" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                // otros
                for (int j = 4; j <= cantMesesPeriodo + 3; j++)
                {
                    // estilos
                    worksheet.Cells[LetraColumna(j) + i + ":" + LetraColumna(j) + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }
            // merge
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR52 - 1)].Merge = true;
            // estilos
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR52 - 1)].Style.Font.Name = "Bookman Old Style";
            //worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR52 - 1)].Style.ShrinkToFit = true;
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR52 - 1)].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR52 - 1)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR52 - 1)].Style.Font.Size = 9;
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR52 - 1)].Style.TextRotation = 90;
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR52 - 1)].Style.WrapText = true;
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR52 - 1)].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR52 - 1)].Style.Fill.BackgroundColor.SetColor(colorFromHexAmarillo);
            worksheet.Cells["A" + row + ":A" + (row + cantPreguntasR52 - 1)].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            // merge
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR52 - 1)].Merge = true;
            // estilos
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR52 - 1)].Style.Font.Name = "Bookman Old Style";
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR52 - 1)].Style.ShrinkToFit = true;
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR52 - 1)].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR52 - 1)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR52 - 1)].Style.Font.Size = 10;
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR52 - 1)].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR52 - 1)].Style.Fill.BackgroundColor.SetColor(colorFromHexCeleste);
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + (row + cantPreguntasR52 - 1)].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            //// ---- fila + cantPreguntasR19 + 1 + cantPreguntasR52 ----
            //row += cantPreguntasR52;
            //// alto
            //worksheet.Row(row).Height = 24;
            //// merge
            //worksheet.Cells["B" + row + ":C" + row].Merge = true;
            //// estilos
            //worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Font.Name = "Bookman Old Style";
            //worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.ShrinkToFit = true;
            //worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //worksheet.Cells["D" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Font.Bold = true;
            //worksheet.Cells["D" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            //worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Font.Size = 12;
            //worksheet.Cells["B" + row + ":C" + row].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            //worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + row].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            //worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + row].Style.Fill.PatternType = ExcelFillStyle.Solid;
            //worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + row].Style.Fill.BackgroundColor.SetColor(colorFromHexAmarillo2);
            // ---- fila ----
            row += cantPreguntasR52;
            // alto
            worksheet.Row(row).Height = 22;
            // merge
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Merge = true;
            // estilos
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 4) + row].Style.Font.Name = "Bookman Old Style";
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 4) + row].Style.ShrinkToFit = true;
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 4) + row].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 4) + row].Style.Font.Bold = true;
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 4) + row].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Font.Size = 16;
            worksheet.Cells["B" + row + ":" + LetraColumna(cantMesesPeriodo + 3) + row].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + row].Style.Font.Size = 10;
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + row].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + row].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[LetraColumna(cantMesesPeriodo + 4) + row + ":" + LetraColumna(cantMesesPeriodo + 4) + row].Style.Fill.BackgroundColor.SetColor(colorFromHexAmarillo2);
        }

        private string LetraColumna(int posicion)
        {
            return ((char)(posicion + 64)).ToString();
        }

        private void GenerarHeaderRelator(ExcelWorksheet worksheet)
        {
            var headerRow = new List<string[]>()
                {
                    new string[]
                    {
                        "",
                        "EVALUACION DESEMPEÑO INSTRUCTORES                                         R47 - V3"
                    }
                };

            // Determine the header range (e.g. A1:D1)

            // Popular header row data
            worksheet.Cells["B2:AA2"].LoadFromArrays(headerRow);

            GenerarImagen(worksheet);
        }

        private void GenerarImagen(ExcelWorksheet worksheet)
        {//get the image from disk
            using (System.Drawing.Image image = System.Drawing.Image.FromFile(directoryImages + "logo.png"))
            {
                var excelImage = worksheet.Drawings.AddPicture("Logo", image);

                //add the image to row 20, column E
                excelImage.SetPosition(1, 0, 1, 0);
                excelImage.SetSize(183, 66);
            }
        }

        private void GenerarHojaResumen(ExcelWorksheet worksheet, List<Relator> relatores, DateTime fechaInicio, DateTime fechaTermino, Sucursal sucursal)
        {
            GenerarHeaderResumen(worksheet);
            GenerarEstilosResumen(worksheet);
            GenerarDataResumen(worksheet, relatores, fechaInicio, fechaTermino, sucursal);
            GenerarGraficoResumen(worksheet, relatores, fechaInicio, fechaTermino);
        }

        private void GenerarGraficoResumen(ExcelWorksheet worksheet, List<Relator> relatores, DateTime fechaInicio, DateTime fechaTermino)
        {
            // create chart
            ExcelChart chart = worksheet.Drawings.AddChart("Resumen", eChartType.ColumnClustered); //To add chart to added sheet of type column clustered chart  
            chart.XAxis.Title.Text = ""; //give label to x-axis of chart  
            chart.XAxis.Title.Font.Size = 10;
            chart.YAxis.Title.Text = ""; //give label to Y-axis of chart  
            chart.YAxis.Title.Font.Size = 10;
            chart.SetSize(1200, 400);
            chart.SetPosition(1, 0, 5, 0);
            chart.Title.Text = "Evaluación Instructores PERIODO " + fechaInicio.ToString("MMMyyyy") + " - " + fechaTermino.ToString("MMMyyyy") + " - ANTOFAGASTA";

            // add series
            var row = 1;
            var finalAlumnos = chart.Series.Add(("B" + (row + 1) + ":" + "B" + (relatores.Count + 1)), ("A" + (row + 1) + ":" + "A" + (relatores.Count + 1)));
            finalAlumnos.Header = "% Final Según Alumnos";
            var finalAdministracion = chart.Series.Add(("C" + (row + 1) + ":" + "C" + (relatores.Count + 1)), ("A" + (row + 1) + ":" + "A" + (relatores.Count + 1)));
            finalAdministracion.Header = "% Final Según Administración";
        }

        private void GenerarDataResumen(ExcelWorksheet worksheet, List<Relator> relatores, DateTime fechaInicio, DateTime fechaTermino, Sucursal sucursal)
        {
            var cellData = new List<object[]>();

            foreach (var relator in relatores)
            {
                cellData.Add(new object[] {
                        relator.contacto.nombreCompleto,
                        GetFinalAlumnosR47(relator, fechaInicio, fechaTermino, sucursal),
                        GetFinalAdministracionR47(relator, fechaInicio, fechaTermino, sucursal),
                        GetFinalR47(relator, fechaInicio, fechaTermino, sucursal)
                    });
            }

            worksheet.Cells[2, 1].LoadFromArrays(cellData);
        }

        private void GenerarEstilosResumen(ExcelWorksheet worksheet)
        {
            worksheet.Column(1).Width = 25;
            worksheet.Column(2).Width = 20;
            worksheet.Column(3).Width = 25;
            worksheet.Column(4).Width = 7;
        }

        private void GenerarHeaderResumen(ExcelWorksheet worksheet)
        {
            var headerRow = new List<string[]>()
                {
                    new string[]
                    {
                        "Nombre Instructor",
                        "% Final Según Alumnos",
                        "% Final Según Administración",
                        "% Final"
                    }
                };

            // Determine the header range (e.g. A1:D1)
            string headerRange = "A1:" + Char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

            // Popular header row data
            worksheet.Cells[headerRange].LoadFromArrays(headerRow);

            GenerarEstilosHeaderResumen(worksheet, headerRange);
        }

        private void GenerarEstilosHeaderResumen(ExcelWorksheet worksheet, string headerRange)
        {
            worksheet.Cells[headerRange].Style.Font.Bold = true;
            worksheet.Cells[headerRange].Style.Font.Size = 10;
            worksheet.Cells[headerRange].Style.Font.Color.SetColor(System.Drawing.Color.White);

            Color colorFromHex = System.Drawing.ColorTranslator.FromHtml("#0066CC");
            worksheet.Cells[headerRange].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[headerRange].Style.Fill.BackgroundColor.SetColor(colorFromHex);
        }

        private List<Relator> GetRelatoresR47(DateTime fechaInicio, DateTime fechaTermino, Sucursal sucursal)
        {
            if (sucursal == null)
            {
                return new List<Relator>();
            }
            var relatores = db.Relators
                .Where(x => x.softDelete == false)
                .Where(x => x.r19
                    .Where(y => y.comercializacion.cotizacion.sucursal.idSucursal == sucursal.idSucursal)
                    .Where(y => DbFunctions.TruncateTime(y.comercializacion.fechaInicio) >= fechaInicio.Date)
                    .Where(y => DbFunctions.TruncateTime(y.comercializacion.fechaInicio) <= fechaTermino.Date)
                    .Where(y => y.encuesta.respuestas
                        .Where(z => z.encuesta.idEncuesta == y.encuesta.idEncuesta)
                        .Where(z => z.pregunta.tipo == TipoPregunta.Alternativa)
                        .Where(z => z.pregunta.formulario.idFormulario == y.idFormularioRelator)
                        .ToList().Count() > 0).ToList().Count() > 0
                    || x.r52
                        .Where(y => y.comercializacion.cotizacion.sucursal.idSucursal == sucursal.idSucursal)
                        .Where(y => DbFunctions.TruncateTime(y.comercializacion.fechaInicio) >= fechaInicio.Date)
                        .Where(y => DbFunctions.TruncateTime(y.comercializacion.fechaInicio) <= fechaTermino.Date)
                        .Where(y => y.encuesta.respuestas
                            .Where(z => z.encuesta.idEncuesta == y.encuesta.idEncuesta)
                            .Where(z => z.pregunta.tipo == TipoPregunta.Alternativa)
                            .ToList().Count() > 0).ToList().Count() > 0)
                .ToList()
                .Where(x => x.r52.Count() >= 2)
                .ToList();
            return relatores;
        }

        private int GetCantPreguntasR52(List<Relator> relatores, DateTime fechaInicio, DateTime fechaTermino, Sucursal sucursal)
        {
            // cantida de preguntas
            var cont = 0;
            // lista de formularios utilizados en el periodo
            var listaFormularios = new List<Formulario>();
            foreach (var relator in relatores)
            {
                foreach (var r52 in relator.r52.Where(x => x.comercializacion.cotizacion.sucursal.idSucursal == sucursal.idSucursal))
                {
                    var formulario = db.Formulario
                        .Where(x => x.idFormulario == r52.idFormularioCualitativa)
                        .FirstOrDefault();
                    if (!listaFormularios.Contains(formulario) && formulario != null)
                    {
                        listaFormularios.Add(formulario);
                        cont += formulario.preguntasFormularios.Count();
                    }
                }
            }
            return cont;
        }

        private int GetCantPreguntasR19(List<Relator> relatores, DateTime fechaInicio, DateTime fechaTermino, Sucursal sucursal)
        {
            // cantida de preguntas
            var cont = 0;
            // lista de formularios utilizados en el periodo
            var listaFormularios = new List<Formulario>();
            foreach (var relator in relatores)
            {
                foreach (var r19 in relator.r19.Where(x => x.comercializacion.cotizacion.sucursal.idSucursal == sucursal.idSucursal))
                {
                    var formulario = db.Formulario
                        .Where(x => x.idFormulario == r19.idFormularioRelator)
                        .FirstOrDefault();
                    if (!listaFormularios.Contains(formulario) && formulario != null)
                    {
                        listaFormularios.Add(formulario);
                        cont += formulario.preguntasFormularios.Count();
                    }
                }
            }
            return cont;
        }

        private double GetFinalR47(Relator relator, DateTime fechaInicio, DateTime fechaTermino, Sucursal sucursal)
        {
            var finalAlumnos = GetFinalAlumnosR47(relator, fechaInicio, fechaTermino, sucursal);
            var finalAdministracion = GetFinalAdministracionR47(relator, fechaInicio, fechaTermino, sucursal);
            return (finalAlumnos + finalAdministracion) / 2;
        }

        private double GetFinalAdministracionR47(Relator relator, DateTime fechaInicio, DateTime fechaTermino, Sucursal sucursal)
        {
            var cont = 0;
            var sum = 0;
            foreach (var r52 in relator.r52.Where(x => x.comercializacion.cotizacion.sucursal.idSucursal == sucursal.idSucursal))
            {
                var respuestas = db.RespuestasContestadasFormulario
                    .Where(x => x.encuesta.idEncuesta == r52.encuesta.idEncuesta)
                    .Where(x => x.pregunta.tipo == TipoPregunta.Alternativa)
                    .Where(x => x.pregunta.formulario.idFormulario == r52.idFormularioCualitativa)
                    .Where(x => DbFunctions.TruncateTime(x.fecha) >= fechaInicio.Date)
                    .Where(x => DbFunctions.TruncateTime(x.fecha) <= fechaTermino.Date)
                    .ToList();
                sum += respuestas.Sum(x => int.Parse(x.respuesta));
                cont += respuestas.Count();
            }
            return cont > 0 ? sum / cont : 0;
        }

        private double GetFinalAlumnosR47(Relator relator, DateTime fechaInicio, DateTime fechaTermino, Sucursal sucursal)
        {
            var cont = 0;
            var sum = 0;
            foreach (var r19 in relator.r19.Where(x => x.comercializacion.cotizacion.sucursal.idSucursal == sucursal.idSucursal))
            {
                var respuestas = db.RespuestasContestadasFormulario
                    .Where(x => x.encuesta.idEncuesta == r19.encuesta.idEncuesta)
                    .Where(x => x.pregunta.tipo == TipoPregunta.Alternativa)
                    .Where(x => x.pregunta.formulario.idFormulario == r19.idFormularioRelator)
                    .Where(x => DbFunctions.TruncateTime(x.fecha) >= fechaInicio.Date)
                    .Where(x => DbFunctions.TruncateTime(x.fecha) <= fechaTermino.Date)
                    .ToList();
                sum += respuestas.Sum(x => int.Parse(x.respuesta));
                cont += respuestas.Count();
            }
            return cont > 0 ? sum / cont : 0;
        }

        private int GetCantCursosRelator(Relator relator, DateTime fechaInicio, DateTime fechaTermino, Sucursal sucursal)
        {
            return db.RelatorCurso.Where(x => x.relator.idRelator == relator.idRelator)
                .Sum(x => x.comercializaciones
                    .Where(y => y.cotizacion.sucursal.idSucursal == sucursal.idSucursal)
                    .Where(y => DbFunctions.TruncateTime(y.fechaInicio) >= fechaInicio.Date)
                    .Where(y => DbFunctions.TruncateTime(y.fechaInicio) <= fechaTermino.Date).Count());
        }

        private string GetProfesionRelator(Relator relator)
        {
            var profesiones = "";
            foreach (var profesion in relator.tituloCurricular)
            {
                profesiones += " - ";
                profesiones += profesion.descripcion;
            }
            return profesiones;
        }

        private int GetPromedioMesPreguntaR19(Relator relator, PreguntasFormulario pregunta, int mes, DateTime fechaInicio, DateTime fechaTermino, Sucursal sucursal)
        {
            var sumRespuestas = 0;
            int contRespuestas = 0;
            foreach (var r19 in relator.r19
                .Where(x => x.comercializacion.cotizacion.sucursal.idSucursal == sucursal.idSucursal)
                .Where(x => DateTime.Compare(x.comercializacion.fechaInicio, fechaInicio) > 0)
                .Where(x => DateTime.Compare(x.comercializacion.fechaInicio, fechaTermino) < 0)
                .Where(x => x.comercializacion.fechaCreacion.Month == mes))
            {
                var respuestas = db.RespuestasContestadasFormulario
                    .Where(x => x.pregunta.idPreguntasFormulario == pregunta.idPreguntasFormulario)
                    .Where(x => x.encuesta.idEncuesta == r19.encuesta.idEncuesta)
                    .Where(x => x.pregunta.tipo == TipoPregunta.Alternativa)
                    .Where(x => DbFunctions.TruncateTime(x.fecha) >= fechaInicio.Date)
                    .Where(x => DbFunctions.TruncateTime(x.fecha) <= fechaTermino.Date)
                    .ToList();
                var valorRespuesta = 0;
                sumRespuestas += respuestas.Sum(x => int.TryParse(x.respuesta, out valorRespuesta) ? int.Parse(x.respuesta) : 0);
                contRespuestas += respuestas.Where(x => int.TryParse(x.respuesta, out valorRespuesta)).Count();
            }
            return contRespuestas > 0 ? sumRespuestas / contRespuestas : 0;
        }

        private List<PreguntasFormulario> GetPreguntasR19(Relator relator, DateTime fechaInicio, DateTime fechaTermino, Sucursal sucursal)
        {
            var preguntas = new List<PreguntasFormulario>();
            foreach (var r19 in relator.r19.Where(x => x.comercializacion.cotizacion.sucursal.idSucursal == sucursal.idSucursal))
            {
                if (DateTime.Compare(r19.comercializacion.fechaCreacion, fechaInicio) > 0
                    && DateTime.Compare(r19.comercializacion.fechaCreacion, fechaTermino) < 0)
                {
                    var seccionEncuesta = r19.encuesta.seccionEncuesta
                        .Where(x => x.formulario.idFormulario == r19.idFormularioRelator)
                        .FirstOrDefault();
                    if (seccionEncuesta != null)
                    {
                        foreach (var pregunta in seccionEncuesta.formulario.preguntasFormularios)
                        {
                            if (!preguntas.Contains(pregunta))
                            {
                                preguntas.Add(pregunta);
                            }
                        }
                    }
                }
            }
            return preguntas;
        }

        private int GetPromedioMesPreguntaR52(Relator relator, PreguntasFormulario pregunta, int mes, DateTime fechaInicio, DateTime fechaTermino, Sucursal sucursal)
        {
            var sumRespuestas = 0;
            int contRespuestas = 0;
            foreach (var r52 in relator.r52
                .Where(x => x.comercializacion.cotizacion.sucursal.idSucursal == sucursal.idSucursal)
                .Where(x => DateTime.Compare(x.comercializacion.fechaInicio, fechaInicio) > 0)
                .Where(x => DateTime.Compare(x.comercializacion.fechaInicio, fechaTermino) < 0)
                .Where(x => x.comercializacion.fechaCreacion.Month == mes))
            {
                var respuestas = db.RespuestasContestadasFormulario
                    .Where(x => x.pregunta.idPreguntasFormulario == pregunta.idPreguntasFormulario)
                    .Where(x => x.encuesta.idEncuesta == r52.encuesta.idEncuesta)
                    .Where(x => x.pregunta.tipo == TipoPregunta.Alternativa)
                    .Where(x => DbFunctions.TruncateTime(x.fecha) >= fechaInicio.Date)
                    .Where(x => DbFunctions.TruncateTime(x.fecha) <= fechaTermino.Date)
                    .ToList();
                var valorRespuesta = 0;
                sumRespuestas += respuestas.Sum(x => int.TryParse(x.respuesta, out valorRespuesta) ? int.Parse(x.respuesta) : 0);
                contRespuestas += respuestas.Where(x => int.TryParse(x.respuesta, out valorRespuesta)).Count();
            }
            return contRespuestas > 0 ? sumRespuestas / contRespuestas : 0;
        }

        private List<PreguntasFormulario> GetPreguntasR52(Relator relator, DateTime fechaInicio, DateTime fechaTermino, Sucursal sucursal)
        {
            var preguntas = new List<PreguntasFormulario>();
            foreach (var r52 in relator.r52.Where(x => x.comercializacion.cotizacion.sucursal.idSucursal == sucursal.idSucursal))
            {
                if (DateTime.Compare(r52.comercializacion.fechaInicio, fechaInicio) > 0
                    && DateTime.Compare(r52.comercializacion.fechaInicio, fechaTermino) < 0)
                {
                    var seccionEncuesta = r52.encuesta.seccionEncuesta
                        .Where(x => x.formulario.idFormulario == r52.idFormularioCualitativa)
                        .FirstOrDefault();
                    if (seccionEncuesta != null)
                    {
                        foreach (var pregunta in seccionEncuesta.formulario.preguntasFormularios)
                        {
                            if (!preguntas.Contains(pregunta))
                            {
                                preguntas.Add(pregunta);
                            }
                        }
                    }
                }
            }
            return preguntas;
        }

        // ------------------------------------------ R53 -----------------------------------
        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult R53(DateTime fechaInicio, DateTime fechaTermino)
        {
            var r53 = db.R53
                .Where(x => x.comercializacion.softDelete == false)
                .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaInicio) >= fechaInicio.Date)
                .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaTermino) <= fechaTermino.Date)
                .ToList();
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"R53.xlsx\"");
            return View(r53);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public SelectList GetClientes()
        {
            return new SelectList(db.Cliente.Where(x => x.softDelete == false).Select(c => new SelectListItem
            {
                Text = "[" + c.rut + "] " + c.nombreEmpresa,
                Value = c.idCliente.ToString()
            }).ToList(), "Value", "Text");
        }

        public SelectList GetSucursales()
        {
            return new SelectList(db.Sucursal.Select(x => new SelectListItem
            {
                Text = x.nombre,
                Value = x.idSucursal.ToString()
            }).ToList(), "Value", "Text");
        }
    }
}