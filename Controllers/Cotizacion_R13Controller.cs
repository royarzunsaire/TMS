using jsreport.MVC;
using jsreport.Types;
using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using SGC.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SGC.Controllers
{
    public class Cotizacion_R13Controller : Controller
    {
        private static readonly string directory = ConfigurationManager.AppSettings["directory"] + "Files/";
        private InsecapContext db = new InsecapContext();

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/" })]
        // GET: Cotizacion_R13
        public ActionResult Index()
        {

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
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        // GET: Comercializacions
        public ActionResult IndexData()
        {

            int start = Convert.ToInt32(Request["start"]);
            int draw = Convert.ToInt32(Request["draw"]);
            String search = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];
            int recordsTotal = 0;
            int count = Convert.ToInt32(Request["length"]);
            string view = Request["view"];

            var dataDb = db.Cotizacion_R13
                .Where(c => c.softDelete == false)
                .Include(c => c.cliente)
                .Include(c => c.curso)
                .Include(c => c.costo);

            if (view.Contains("MyIndex"))
            {
                string idUser = User.Identity.GetUserId();
                dataDb = dataDb.Where(x => x.usuarioCreador.Id == idUser);
            }

            DateTime dateSearch = DateTime.MinValue;
            DateTime.TryParse(search, out dateSearch);


            if (string.IsNullOrEmpty(search))
            {
                recordsTotal = dataDb.Count();
            }
            else
            {
                dataDb = dataDb.Where(x => x.codigoCotizacion.ToLower().Contains(search)
              || x.cliente.nombreEmpresa.ToLower().Contains(search)
              || x.curso.nombreCurso.ToLower().Contains(search)
              || x.tipoCurso.ToLower().Contains(search)
              || x.curso.tipoEjecucion.ToString().ToLower().Contains(search)
              || x.usuarioCreador.nombres.ToLower().Contains(search)
               || x.usuarioCreador.apellidoMaterno.ToLower().Contains(search)
                || x.usuarioCreador.apellidoPaterno.ToLower().Contains(search)
                || DateTime.Compare(x.fechaCreacion, dateSearch) == 0

              );
                recordsTotal = dataDb.Count();
            }

            if (count == -1)
            {
                count = recordsTotal;
            }

            var data = dataDb.OrderByDescending(x => x.fechaCreacion)
                .Skip(start)
                .Take(count)
                .ToList();

            List<object> resultset = new List<object>();
            foreach (Cotizacion_R13 cotizacion in data)
            {
                var curso = "";
                if (cotizacion.tipoCurso != "Duplicado Credencial" && cotizacion.tipoCurso != "Arriendo de Sala" && cotizacion.tipoCurso != "Tramitación Licencia")
                {
                    curso = cotizacion.curso.nombreCurso;
                }
                else
                {
                    curso = cotizacion.tipoCurso;

                }

                string costo = "0";

                if (cotizacion.isValorUnico)
                {
                    costo = string.Format("{0:C}", cotizacion.valorUnico);
                }
                else
                {
                    if (cotizacion.costo != null && cotizacion.costo.Count() != 0)
                        costo = string.Format("{0:C}", cotizacion.costo.Where(x => x.idCotizacion == cotizacion.idCotizacion_R13).Select(y => y.total).Sum());

                }

                resultset.Add(
                    new
                    {
                        cotizacion.codigoCotizacion,
                        creacionFecha = String.Format("{0:dd/MM/yyyy}", cotizacion.fechaCreacion),
                        curso,
                        cliente = cotizacion.cliente.nombreEmpresa,
                        tipo = cotizacion.tipoCurso,
                        tipoEjecucion = cotizacion.curso != null ? cotizacion.curso.tipoEjecucion.ToString() : " ",
                        comercial = cotizacion.usuarioCreador.nombres + " " + cotizacion.usuarioCreador.apellidoPaterno,
                        costo,
                        menu = ConvertPartialViewToString(PartialView("IndexMenu", cotizacion))
                    }
                    );



            }


            var jsonResult = Json(new { draw, recordsTotal, recordsFiltered = recordsTotal, data = resultset }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }



        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/" })]
        // GET: Cotizacion_R13
        public ActionResult MyIndex()
        {

            String idUser = User.Identity.GetUserId();
            var cotizacion_R13 = db.Cotizacion_R13
                .Where(c => c.softDelete == false)
                 .Where(x => x.usuarioCreador.Id == idUser)
                .Include(c => c.cliente)
                .Include(c => c.curso)
                .Include(c => c.costo);
            if (TempData["PosseComercializacion"] != null)
            {
                ViewBag.PosseComercializacion = TempData["PosseComercializacion"];
            }
            var tienenComercializacion = new List<Cotizacion_R13>(); ;
            var a = cotizacion_R13.ToList();
            foreach (var item in cotizacion_R13.ToList())
            {
                var dc = db.Comercializacion.Where(c => c.cotizacion.idCotizacion_R13 == item.idCotizacion_R13).ToList();
                if (db.Comercializacion
                    .Where(c => c.softDelete == false)
                    .Where(c => c.cotizacion.idCotizacion_R13 == item.idCotizacion_R13)
                    .FirstOrDefault() != null)
                {
                    tienenComercializacion.Add(item);
                }
            }
            ViewBag.tienenComercializacion = tienenComercializacion;
            return View("Index", cotizacion_R13.ToList());
        }


        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/" })]
        // GET: Cotizacion_R13/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cotizacion_R13 cotizacion_R13 = db.Cotizacion_R13
                .Where(c => c.idCotizacion_R13 == id)
                .Include(x => x.cotizacionAporteCapacitacion.Select(y => y.aporteCapacitacion))
                .Include(x => x.faena)
                .FirstOrDefault();
            if (cotizacion_R13 == null)
            {
                return HttpNotFound();
            }
            db.Configuration.ProxyCreationEnabled = false;
            ViewModelCotizacion mymodel = new ViewModelCotizacion();
            mymodel.cotizacion = cotizacion_R13;
            mymodel.clientes = db.Cliente.ToList();
            ViewBag.encargadoPago = db.Contacto.Find(cotizacion_R13.contactoEncargadoPago) != null ? db.Contacto.Find(cotizacion_R13.contactoEncargadoPago).nombreCompleto : "";
            ViewBag.contacto = db.Contacto.Find(cotizacion_R13.contacto) != null ? db.Contacto.Find(cotizacion_R13.contacto).nombreCompleto : "";
            mymodel.cotizacion.costo = db.Costo.Where(c => c.idCotizacion == cotizacion_R13.idCotizacion_R13).ToList();
            return View(mymodel);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/", "/Cotizacion_R13/Create/" })]
        // GET: Cotizacion_R13/Create
        public ActionResult Create()
        {
            db.Configuration.ProxyCreationEnabled = false;
            ViewModelCotizacion myModel = new ViewModelCotizacion();
            myModel.clientes = db.Cliente.Where(x => x.softDelete == false).ToList();
            ViewBag.sucursales = GetSucursales();
            ViewBag.aportesCapacitacion = db.AporteCapacitacion.Where(x => !x.softDelete).ToList();

            //myModel.cotizacionAporteCapacitacion = new List<CotizacionAporteCapacitacion>();
            //foreach (var aporte in db.AporteCapacitacion.Where(x => !x.softDelete).ToList())
            //{
            //    var cotizacionAporte = new CotizacionAporteCapacitacion();
            //    cotizacionAporte.aCargo = 0;
            //    cotizacionAporte.idAporteCapacitacion = aporte.idAporteCapacitacion;
            //    myModel.cotizacionAporteCapacitacion.Add(cotizacionAporte);
            //}

            return View(myModel);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/", "/Cotizacion_R13/Create/" })]
        [HttpPost]
        public JsonResult ObtenerGiro(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;

            var listIdGiro = db.ClienteGiro.Where(x => x.idCliente == id).Select(z => z.idGiro).ToList();

            return Json(db.Giro.Where(x => listIdGiro.Contains(x.idGiro)).ToList());
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/", "/Cotizacion_R13/Create/" })]
        [HttpPost]
        public JsonResult ObtenerFaena(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;

            var faenas = db.FaenaCliente.Where(x => x.faena.softDelete == false && x.cliente.idCliente == id).Select(y => y.faena.idFaena).ToList();

            return Json(db.Faena.Where(x => faenas.Contains(x.idFaena)).ToList());
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/", "/Cotizacion_R13/Create/" })]
        public JsonResult obtenerDocumentoPago(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;

            var listIdDocumentoPago = db.ClienteTipoDocumentosPago.Where(x => x.idCliente == id).Select(z => z.idTipoDocumentosPago).ToList();

            return Json(db.TiposDocumentosPago.Where(x => listIdDocumentoPago.Contains(x.idTipoDocumentosPago)).ToList());
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/", "/Cotizacion_R13/Create/" })]
        public JsonResult ObtenerEncargadoDePago(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;

            var listIdContato = db.EncargadoPago.Where(x => x.idCliente == id).Select(z => z.idContacto).ToList();

            return Json(db.Contacto.Where(x => listIdContato.Contains(x.idContacto)).ToList());
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/", "/Cotizacion_R13/Create/" })]
        public JsonResult ObtenerContacto(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;

            var listIdContato = db.ClienteContactoCotizacion.Where(x => x.idCliente == id).Where(x => x.contacto.softDelete == false).Select(z => z.idContacto).ToList();

            return Json(db.Contacto.Where(x => listIdContato.Contains(x.idContacto)).ToList());
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/", "/Cotizacion_R13/Create/" })]
        public JsonResult ObtenerClienteDeudor(int id)
        {
            var docsCompromiso = db.DocumentoCompromiso
                .Where(x => x.softDelete == false)
                .Where(x => x.cotizacion.cliente.idCliente == id)
                .ToList();
            foreach (var docCompromiso in docsCompromiso)
            {
                if (docCompromiso.factura != null)
                {
                    if (docCompromiso.factura.tipo == TipoFactura.Costo_Empresa)
                    {
                        if (docCompromiso.factura.estados.OrderByDescending(x => x.fechaCreacion).FirstOrDefault().estado != EstadoFactura.Pagado
                            && DateTime.Compare(docCompromiso.factura.fechaCreacion.AddMonths(3).Date, DateTime.Now.Date) <= 0)
                        {
                            return Json(true);
                        }
                    }
                }
            }
            return Json(false);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/", "/Cotizacion_R13/Create/" })]
        public JsonResult ObtenerClienteOCPendiente(int id)
        {
            var comercializaciones = db.Comercializacion
                .Where(x => x.softDelete == false)
                .Where(x => x.cotizacion.cliente.idCliente == id)
                .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion != EstadoComercializacion.Cancelada
                    || x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion != EstadoComercializacion.Deshabilitada
                    || x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion != EstadoComercializacion.Borrador)
                .ToList();
            foreach (var comercializacion in comercializaciones)
            {
                var docsCompromisoCostoEmpresa = comercializacion.cotizacion.documentosCompromiso
                    .Where(x => x.tipoVenta.tipoPago == TipoPago.CostoEmpresa)
                    .Where(x => x.softDelete == false)
                    .ToList();
                var cont = 0;
                foreach (var docCompromiso in docsCompromisoCostoEmpresa)
                {
                    if (docCompromiso.tipoDocCompromiso.nombre.ToLower().Contains("oc"))
                    {
                        cont++;
                    }
                }
                if (cont == 0 && docsCompromisoCostoEmpresa.Count() > 0)
                {
                    return Json(true);
                }
                //if (comercializacion.cotizacion.documentosCompromiso
                //    .Where(x => x.softDelete == false)
                //    .Where(x => x.tipoVenta.tipoPago == TipoPago.Sence || x.tipoVenta.tipoPago == TipoPago.Otic)
                //    .ToList().Count() == 0)
                //{
                //}
            }
            return Json(false);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/", "/Cotizacion_R13/Create/" })]
        public JsonResult ObtenerRepresentanteLegal(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;

            var listIdContato = db.RepresentanteLegal.Where(x => x.idCliente == id).Select(z => z.idContacto).ToList();

            return Json(db.Contacto.Where(x => listIdContato.Contains(x.idContacto)).ToList());
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/", "/Cotizacion_R13/Create/" })]
        public JsonResult ObtenerCursos(string modalidad, string idSucursal, int tipoEjecucion)
        {
            //var listCursosAbiertos = db.CalendarizacionAbierta.Select(z => z.curso.idCurso).ToList();
            var cursos = new List<object>();
            if (modalidad == "Abierto")
            {
                var hoy = DateTime.Now;
                var calendarizaciones = new List<Calendarizacion>();
                if (idSucursal != null && idSucursal != "")
                {
                    var idSucursalInt = int.Parse(idSucursal);
                    calendarizaciones = db.Calendarizacions
                       .Where(c => DbFunctions.TruncateTime(c.inicioPeriodo) <= hoy.Date)
                       .Where(c => DbFunctions.TruncateTime(c.finPeriopdo) >= hoy.Date)
                       .Where(c => c.sucursal.idSucursal == idSucursalInt)
                       .ToList();
                }
                else
                {
                    calendarizaciones = db.Calendarizacions
                       .Where(c => DbFunctions.TruncateTime(c.inicioPeriodo) <= hoy.Date)
                       .Where(c => DbFunctions.TruncateTime(c.finPeriopdo) >= hoy.Date)
                       .ToList();
                }
                var cursosAbiertos = new List<CalendarizacionAbierta>();
                foreach (var calendarizacion in calendarizaciones)
                {
                    foreach (var calendarizacionAbierta in calendarizacion.calendarizacionesAbiertas)
                    {
                        if (DateTime.Compare(hoy, calendarizacionAbierta.fechaTermino) <= 0 && DateTime.Compare(calendarizacionAbierta.fechaTermino, hoy.AddMonths(2)) <= 0 && !cursosAbiertos.Contains(calendarizacionAbierta))
                        {
                            cursosAbiertos.Add(calendarizacionAbierta);
                        }
                    }
                }
                foreach (var item in cursosAbiertos)
                {
                    if ((int)item.curso.tipoEjecucion == tipoEjecucion)
                    {
                        var curso = new Curso
                        {
                            idCurso = item.idCalendarizacionAbierta,
                            nombreCurso = item.curso.nombreCurso
                        };
                        var r11 = db.R11.Where(x => x.idCurso == item.curso.idCurso).FirstOrDefault();
                        var r12 = db.CostoCursoR12.Where(x => x.idCurso == item.curso.idCurso).FirstOrDefault();
                        //if (r11 != null && r12 != null)
                        if (r11 != null)
                        {
                            cursos.Add(new { curso, horas = r11.horasPracticas + r11.horasTeoricas });
                        }
                    }
                }
                //db.Configuration.ProxyCreationEnabled = false;
                //cursos = db.Curso.Where(c => c.softDelete == false).Where(x => listCursosAbiertos.Contains(x.idCurso)).ToList();
            }
            else
            {
                //db.Configuration.ProxyCreationEnabled = false;
                //cursos = db.Curso.Where(c => c.softDelete == false).Where(x => !listCursosAbiertos.Contains(x.idCurso)).ToList();
                var cursosBD = db.Curso.Where(c => c.softDelete == false).ToList();
                foreach (var item in cursosBD)
                {
                    if ((int)item.tipoEjecucion == tipoEjecucion)
                    {
                        var curso = new Curso
                        {
                            idCurso = item.idCurso,
                            nombreCurso = item.nombreCurso
                        };
                        var r11 = db.R11.Where(x => x.idCurso == item.idCurso).FirstOrDefault();
                        var r12 = db.CostoCursoR12.Where(x => x.idCurso == item.idCurso).FirstOrDefault();
                        //if (r11 != null && r12 != null)
                        if (r11 != null)
                        {
                            cursos.Add(new { curso, horas = r11.horasPracticas + r11.horasTeoricas });
                        }
                    }
                }
            }
            return Json(cursos);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/ClienteContacto/" })]
        public JsonResult ObtenerCursosAutoCotizacion()
        {
            var cursos = new List<object>();
            var cursosBD = db.Curso
                .Where(c => c.softDelete == false)
                .Where(c => c.tipoEjecucion != TipoEjecucion.Recertificacion)
                .Where(c => c.tipoEjecucion != TipoEjecucion.Recertificacion_Sincronica)
                .Where(c => c.tipoEjecucion != TipoEjecucion.Recertificacion_Asincronica)
                .ToList();
            foreach (var item in cursosBD)
            {
                var curso = new Curso
                {
                    idCurso = item.idCurso,
                    nombreCurso = item.nombreCurso
                };
                var r11 = db.R11.Where(x => x.idCurso == item.idCurso).FirstOrDefault();
                var r12 = db.CostoCursoR12.Where(x => x.idCurso == item.idCurso).FirstOrDefault();
                if (r11 != null && r12 != null)
                {
                    cursos.Add(new { curso, horas = r11.horasPracticas + r11.horasTeoricas });
                }
                //cursos.Add(curso);
            }
            return Json(cursos);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/", "/Cotizacion_R13/Create/" })]
        public JsonResult ObtenerDatosCurso(int id, string modalidad)
        {
            if (modalidad == "Abierto")
            {
                var calendarizacionAbiera = db.CalendarizacionAbierta.Find(id);
                id = calendarizacionAbiera.curso.idCurso;
            }
            var r11 = db.R11.Where(x => x.idCurso == id).FirstOrDefault();
            if (r11 == null)
            {
                return null;
            }
            var relator = r11.relator;
            r11.relator = null;
            return Json(new
            {
                r11,
                db.R51.Where(x => x.idCurso == id).FirstOrDefault().nombreCurso,
            });
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/ClienteContacto/" })]
        public JsonResult ObtenerDatosCursoAutoCotizacion(int id, string modalidad)
        {
            var r11 = db.R11.Where(x => x.idCurso == id).FirstOrDefault();
            if (r11 != null)
            {
                return Json(new
                {
                    r11.codigoSence
                });
            }
            else
            {
                return null;
            }
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/", "/Cotizacion_R13/Create/", "/ClienteContacto/" })]
        public JsonResult ObtenerCostos(int id, string modalidad, int? cantParticipantes, int idCotizacion, string action)
        {

            if (modalidad == "Abierto")
            {
                var calendarizacionAbiera = db.CalendarizacionAbierta.Find(id);
                id = calendarizacionAbiera.curso.idCurso;
            }
            List<Costo> listaCosto = new List<Costo>();

            if (cantParticipantes == null)
            {
                cantParticipantes = 0;
            }
            List<Costo> costos = null;
            if (idCotizacion != null)
            {
                costos = db.Costo.Where(x => x.idCotizacion == idCotizacion).ToList();

            }

            if (db.CostoCursoR12.Where(x => x.idCurso == id).Select(x => x.idCostoCursoR12).Count() != 0)
            {
                int idCostoCursoR12 = db.CostoCursoR12.Where(x => x.idCurso == id).Select(x => x.idCostoCursoR12).ToList()[0];
                foreach (var item in db.CostoParticularCurso.Where(x => x.idCostoCursoR12 == idCostoCursoR12).ToList())
                {
                    Costo costo = new Costo();
                    if (item.porPersona)
                    {
                        costo.detalle = item.detalle + " *";
                        costo.cantidad = item.cantidad * (int)cantParticipantes;
                        costo.total = item.subTotal * (int)cantParticipantes;
                    }
                    else
                    {
                        costo.detalle = item.detalle;
                        costo.cantidad = item.cantidad;
                        costo.total = item.subTotal;
                    }
                    costo.valor = item.costo;
                    costo.valorMinimo = item.costo;
                    costo.valorMaximo = 999999999;

                    listaCosto.Add(costo);

                }
            }
            var curso = db.Curso.Find(id);
            R11 r11 = db.R11.Where(x => x.idCurso == id).FirstOrDefault();

            var tipoEjecucionCurso = curso.tipoEjecucion;
            var exist = false;
            foreach (var item in db.ListaDetalleCosto)
            {
                exist = costos.Any(x => x.detalle.Contains(item.detalle));
                if (item.tipoEjecucion == tipoEjecucionCurso)
                {
                    Costo costo = new Costo();

                    if (exist && action.Contains("participante"))
                    {
                        item.valor = costos.Where(x => x.detalle.Contains(item.detalle)).FirstOrDefault().valor;
                    }
                    if (item.porPersona)
                    {
                        costo.detalle = item.detalle + " *";
                        costo.cantidad = item.cantidad * (int)cantParticipantes;
                        costo.total = item.cantidad * item.valor * (int)cantParticipantes;
                    }
                    else
                    {
                        if (exist && action.Contains("participante"))
                        {
                            item.cantidad = costos.Where(x => x.detalle.Contains(item.detalle)).FirstOrDefault().cantidad;
                        }
                        costo.detalle = item.detalle;

                        if (item.detalle.ToLower().Contains("relator"))
                        {
                            item.cantidad = Convert.ToInt32(r11.horasPracticas + r11.horasTeoricas);
                        }
                        costo.cantidad = item.cantidad;
                        costo.total = item.cantidad * item.valor;
                    }
                    costo.valor = item.valor;
                    costo.valorMinimo = item.valorMinimo;
                    costo.valorMaximo = item.valorMaximo;
                    listaCosto.Add(costo);
                }

            }
            return Json(listaCosto);


        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/", "/Cotizacion_R13/Create/" })]
        public JsonResult ObtenerCostosGuardados(int id)
        {
            List<Costo> listaCosto = new List<Costo>();

            if (db.Costo.Where(x => x.idCotizacion == id).Count() != 0)
            {
                return Json(db.Costo.Where(x => x.idCotizacion == id));
            }

            return Json(listaCosto);


        }

        //public JsonResult ObtenerAportesCapacitacion(TipoEjecucion tipoEjecucion)
        //{
        //    var aportesCapacitacion = db.AporteCapacitacion.Where(x => !x.softDelete).Where(x => x.tipo == tipoEjecucion).ToList();
        //    return Json(aportesCapacitacion);
        //}

        // POST: Cotizacion_R13/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/", "/Cotizacion_R13/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ViewModelCotizacion myModel)
        {
            Cotizacion_R13 cotizacion = myModel.cotizacion;
            cotizacion.sucursal = db.Sucursal.Find(cotizacion.sucursal.idSucursal);
            if (cotizacion.tipoCurso != "Duplicado Credencial" && cotizacion.tipoCurso != "Arriendo de Sala" && cotizacion.tipoCurso != "Tramitación Licencia")
            {
                if (cotizacion.idCurso == 0 || cotizacion.idCurso == null)
                {
                    ModelState.AddModelError("cotizacion.idCurso", "El campo Curso es obligatorio");
                }
                else
                {
                    if (cotizacion.modalidad == "Abierto")
                    {
                        cotizacion.calendarizacionAbierta = db.CalendarizacionAbierta.Find(cotizacion.idCurso);
                        cotizacion.idCurso = cotizacion.calendarizacionAbierta.curso.idCurso;
                    }
                    var r11 = db.R11.Where(x => x.idCurso == cotizacion.idCurso).FirstOrDefault();
                    cotizacion.horasCurso = r11.horasPracticas + r11.horasTeoricas;
                }
            }

            if (Request["faena"] != "")
            {
                cotizacion.faena = db.Faena.Find(Convert.ToInt32(Request["idfaena"]));
            }

            myModel.cotizacionAporteCapacitacion = new List<CotizacionAporteCapacitacion>();
            if (myModel.cotizacionAporteCapacitacionPresencial != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionPresencial)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacionSincronico != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionSincronico)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacionAsincronico != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionAsincronico)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacionRecertificacion != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionRecertificacion)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacionRecertificacionSincrono != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionRecertificacionSincrono)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacionRecertificacionAsincronico != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionRecertificacionAsincronico)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }

            if (myModel.cotizacionAporteCapacitacion != null)
            {
                if (myModel.cotizacionAporteCapacitacion.Where(x => x.aCargo == null).FirstOrDefault() != null)
                {
                    ModelState.AddModelError("cotizacionAporteCapacitacion", "Se debe seleccionar los aportes de capacitación.");
                }
            }
            if (ModelState.IsValid)
            {
                cotizacion.fechaCreacion = DateTime.Now;
                // codigo cotizacion
                var ultimoCodigo = "0";
                if (db.Cotizacion_R13.OrderByDescending(x => x.idCotizacion_R13).FirstOrDefault() != null)
                {
                    ultimoCodigo = db.Cotizacion_R13.OrderByDescending(x => x.idCotizacion_R13).FirstOrDefault().codigoCotizacion;
                }
                cotizacion.codigoCotizacion = Helpers.CustomUtilsHelper.GeneracionCodigo(cotizacion.sucursal.prefijoCodigo, ultimoCodigo);
                cotizacion.softDelete = false;
                var userId = User.Identity.GetUserId();
                cotizacion.usuarioCreador = db.AspNetUsers.Find(userId);
                db.Cotizacion_R13.Add(cotizacion);
                db.SaveChanges();

                if (myModel.cotizacion.costo != null)
                {
                    foreach (Costo item in myModel.cotizacion.costo)
                    {
                        item.idCotizacion = cotizacion.idCotizacion_R13;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                if (myModel.cotizacionAporteCapacitacion != null)
                {
                    foreach (var item in myModel.cotizacionAporteCapacitacion)
                    {
                        item.idCotizacion = cotizacion.idCotizacion_R13;
                        item.cotizacion = cotizacion;
                        item.aporteCapacitacion = db.AporteCapacitacion.Find(item.idAporteCapacitacion);
                        db.CotizacionAporteCapacitacion.Add(item);
                        db.SaveChanges();
                    }
                }
                return RedirectToAction("MyIndex");
            }
            db.Configuration.ProxyCreationEnabled = false;
            myModel.clientes = db.Cliente.Where(x => x.softDelete == false).ToList();
            ViewBag.sucursales = GetSucursales();
            ViewBag.aportesCapacitacion = db.AporteCapacitacion.Where(x => !x.softDelete).ToList();

            //myModel.cotizacionAporteCapacitacion = myModel.cotizacionAporteCapacitacion == null ? new List<CotizacionAporteCapacitacion>() : myModel.cotizacionAporteCapacitacion;
            //foreach (var aporte in db.AporteCapacitacion.Where(x => !x.softDelete).ToList())
            //{
            //    if (myModel.tipoEjecucion != aporte.tipo)
            //    {
            //        var cotizacionAporte = new CotizacionAporteCapacitacion();
            //        cotizacionAporte.aCargo = 0;
            //        cotizacionAporte.idAporteCapacitacion = aporte.idAporteCapacitacion;
            //        myModel.cotizacionAporteCapacitacion.Add(cotizacionAporte);
            //    }
            //}

            return View(myModel);
        }

        // GET: Cotizacion_R13/Edit/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cotizacion_R13 cotizacion_R13 = db.Cotizacion_R13
                .Where(x => x.idCotizacion_R13 == id)
                .Include(x => x.cotizacionAporteCapacitacion.Select(y => y.aporteCapacitacion))
                .Include(x => x.calendarizacionAbierta)
                .Include(x => x.faena)
                .FirstOrDefault();
            if (cotizacion_R13 == null)
            {
                return HttpNotFound();
            }
            db.Configuration.ProxyCreationEnabled = false;
            ViewModelCotizacion mymodel = new ViewModelCotizacion();
            mymodel.cotizacion = cotizacion_R13;
            mymodel.cotizacion.costo = db.Costo.Where(x => x.idCotizacion == mymodel.cotizacion.idCotizacion_R13).ToList();
            mymodel.clientes = db.Cliente.Where(x => x.softDelete == false).ToList();
            mymodel.cotizacionAporteCapacitacion = mymodel.cotizacion.cotizacionAporteCapacitacion;
            if (cotizacion_R13.curso != null)
            {
                mymodel.tipoEjecucion = cotizacion_R13.curso.tipoEjecucion;
            }
            ViewBag.sucursales = GetSucursales();
            ViewBag.aportesCapacitacion = db.AporteCapacitacion.Where(x => !x.softDelete).ToList();

            if (mymodel.cotizacion.modalidad == "Abierto")
            {
                if (mymodel.cotizacion.calendarizacionAbierta != null)
                {

                    mymodel.cotizacion.idCurso = mymodel.cotizacion.calendarizacionAbierta.idCalendarizacionAbierta;
                }
                else
                {
                    mymodel.cotizacion.calendarizacionAbierta = db.CalendarizacionAbierta.Where(c => c.curso.idCurso == cotizacion_R13.idCurso).FirstOrDefault();
                    if (mymodel.cotizacion.calendarizacionAbierta != null)
                        mymodel.cotizacion.idCurso = mymodel.cotizacion.calendarizacionAbierta.idCalendarizacionAbierta;

                }
            }

            return View(mymodel);
        }

        // POST: Cotizacion_R13/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ViewModelCotizacion myModel)
        {
            Cotizacion_R13 cotizacion = myModel.cotizacion;
            Comercializacion comercializacion = db.Comercializacion.Where(x => x.cotizacion.idCotizacion_R13 == cotizacion.idCotizacion_R13).FirstOrDefault();
            if (comercializacion != null)
            {
                EstadoComercializacion estado = comercializacion.comercializacionEstadoComercializacion.OrderByDescending(x => x.fechaCreacion).LastOrDefault().EstadoComercializacion;
                if (estado != EstadoComercializacion.En_Proceso && estado != EstadoComercializacion.Borrador)
                    return Redirect("index");
            }


            //Delete old
            Cotizacion_R13 cotizacionOld = db.Cotizacion_R13
               .Where(c => c.idCotizacion_R13 == cotizacion.idCotizacion_R13)
               .Include(x => x.curso)
               .FirstOrDefault();
            cotizacionOld.costo = db.Costo
               .Where(c => c.idCotizacion == cotizacionOld.idCotizacion_R13)
               .ToList();
            cotizacionOld.cotizacionAporteCapacitacion = db.CotizacionAporteCapacitacion
              .Where(c => c.idCotizacion == cotizacionOld.idCotizacion_R13)
              .ToList();

            int oldCursoID = 0;
            if (cotizacion.tipoCurso == "Curso" || cotizacion.tipoCurso == "Recertificación")
            {
                if (cotizacion.costo == null)
                {
                    ModelState.AddModelError("cotizacion.idCurso", "Vuelva a intentar nuevamente.");
                }
            }

            if (cotizacionOld.costo != null)
            {
                foreach (Costo item in cotizacionOld.costo.ToList())
                {
                    db.Costo.Remove(item);
                    db.SaveChanges();
                }
            }
            if (cotizacionOld.cotizacionAporteCapacitacion != null)
            {
                foreach (var item in cotizacionOld.cotizacionAporteCapacitacion.ToList())
                {
                    db.Entry(item).State = EntityState.Deleted;
                    db.SaveChanges();

                }
            }

            if (cotizacion.tipoCurso != "Duplicado Credencial" && cotizacion.tipoCurso != "Arriendo de Sala" && cotizacion.tipoCurso != "Tramitación Licencia")
            {
                if (cotizacion.idCurso == 0 || cotizacion.idCurso == null)
                {
                    ModelState.AddModelError("cotizacion.idCurso", "El campo Curso es obligatorio");
                }

            }
            else
            {
                cotizacion.cantidadParticipante = 0;
            }
            myModel.cotizacionAporteCapacitacion = new List<CotizacionAporteCapacitacion>();
            if (myModel.cotizacionAporteCapacitacionPresencial != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionPresencial)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacionSincronico != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionSincronico)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacionAsincronico != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionAsincronico)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacionRecertificacion != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionRecertificacion)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacionRecertificacionSincrono != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionRecertificacionSincrono)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacionRecertificacionAsincronico != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionRecertificacionAsincronico)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacion != null)
            {
                if (myModel.cotizacionAporteCapacitacion.Where(x => x.aCargo == null).FirstOrDefault() != null)
                {
                    ModelState.AddModelError("cotizacionAporteCapacitacion", "Se debe seleccionar los aportes de capacitación.");
                }
            }

            if (cotizacion.modalidad == "Abierto")
            {

                cotizacionOld.calendarizacionAbierta = db.CalendarizacionAbierta.Find(cotizacion.idCurso);
                if (cotizacionOld.calendarizacionAbierta != null)
                {
                    cotizacionOld.idCurso = cotizacionOld.calendarizacionAbierta.curso.idCurso;
                }
                else
                {
                    ModelState.AddModelError("cotizacion.idCurso", "Error al cargar el curso");
                }
            }
            else
            {
                if (cotizacion.tipoCurso != "Duplicado Credencial" && cotizacion.tipoCurso != "Arriendo de Sala" && cotizacion.tipoCurso != "Tramitación Licencia")
                {
                    var curso = db.Curso.Find(cotizacion.idCurso);
                    if (curso != null)
                    {
                        if (comercializacion != null && cotizacion.idCurso != cotizacionOld.idCurso)
                        {
                            if (comercializacion.softDelete == false)
                            {
                                comercializacion.evaluaciones = null;
                                db.SaveChanges();
                                comercializacion.evaluaciones = db.Curso.Find(cotizacion.idCurso).evaluaciones.Where(e => e.softDelete == false).ToList();
                            }
                        }

                        cotizacionOld.curso = curso;
                    }
                    else
                    {
                        ModelState.AddModelError("cotizacion.idCurso", "Error al cargar el curso");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                cotizacionOld.fechaCreacion = DateTime.Now;
                // codigo cotizacion
                string[] splitCodigo = cotizacionOld.codigoCotizacion.Split('-');
                int version = 1;
                if (splitCodigo.Length > 1)
                {
                    version = Int32.Parse(splitCodigo[1]) + 1;
                }
                cotizacionOld.codigoCotizacion = splitCodigo[0] + "-" + version.ToString();

                if (Request["faena"] != "")
                {
                    cotizacionOld.faena = db.Faena.Find(Convert.ToInt32(Request["idfaena"]));
                }

                //db.Cotizacion_R13.Add(cotizacion);
                //db.SaveChanges();

                //Update Model parametros
                cotizacionOld.softDelete = false;
                cotizacionOld.sucursal = db.Sucursal.Find(cotizacion.sucursal.idSucursal);
                cotizacionOld.tieneCodigoSence = cotizacion.tieneCodigoSence;
                cotizacionOld.idCliente = cotizacion.idCliente;
                cotizacionOld.contactoEncargadoPago = cotizacion.contactoEncargadoPago;
                cotizacionOld.contacto = cotizacion.contacto;
                cotizacionOld.cantidadParticipante = cotizacion.cantidadParticipante;
                cotizacionOld.lugarRealizacion = cotizacion.lugarRealizacion;
                cotizacionOld.vigenciaCredenciales = cotizacion.vigenciaCredenciales;
                cotizacionOld.procesoPractico = cotizacion.procesoPractico;
                cotizacionOld.tipoMenu = cotizacion.tipoMenu;
                cotizacionOld.horasCurso = cotizacion.horasCurso;
                cotizacionOld.nombreDiploma = cotizacion.nombreDiploma;
                cotizacionOld.condicionesDePago = cotizacion.condicionesDePago;
                cotizacionOld.valorUnico = cotizacion.valorUnico;

                cotizacionOld.codigoCotizacion = cotizacionOld.codigoCotizacion.Remove(0, 3);
                cotizacionOld.codigoCotizacion = cotizacionOld.sucursal.prefijoCodigo + cotizacionOld.codigoCotizacion;

                //Datos del curso

                cotizacionOld.nombreEmpresa = cotizacion.nombreEmpresa;
                cotizacionOld.razonSocial = cotizacion.razonSocial;
                cotizacionOld.giro = cotizacion.giro;
                cotizacionOld.telefonoCorporativo = cotizacion.telefonoCorporativo;
                cotizacionOld.direccion = cotizacion.direccion;
                cotizacionOld.modalidad = cotizacion.modalidad;
                cotizacionOld.codigoSence = cotizacion.codigoSence;
                cotizacionOld.fechaInicio = cotizacion.fechaInicio;
                cotizacionOld.fechaTermino = cotizacion.fechaTermino;

                R11 r11 = db.R11.Where(x => x.idCurso == cotizacion.idCurso).FirstOrDefault();
                if (r11 != null)
                    cotizacionOld.horasCurso = r11.horasPracticas + r11.horasTeoricas;


                //var userId = User.Identity.GetUserId();
                //cotizacionOld.usuarioCreador = db.AspNetUsers.Find(userId);
                db.Entry(cotizacionOld).State = EntityState.Modified;
                db.SaveChanges();

                if (cotizacion.tipoCurso != "Duplicado Credencial" && cotizacion.tipoCurso != "Arriendo de Sala" && cotizacion.tipoCurso != "Tramitación Licencia")
                {
                    if (comercializacion != null && myModel.cotizacion.costo != null)
                    {
                        comercializacion.valorFinal = myModel.cotizacion.costo.Select(y => y.total).Sum();
                        db.Entry(comercializacion).State = EntityState.Modified;
                        db.SaveChanges();
                    }




                    if (myModel.cotizacion.costo != null)
                    {
                        foreach (Costo item in myModel.cotizacion.costo)
                        {
                            item.idCotizacion = cotizacionOld.idCotizacion_R13;
                            item.cotizacion = cotizacionOld;
                            db.Costo.Add(item);
                            db.SaveChanges();
                        }
                    }


                    if (myModel.cotizacionAporteCapacitacion != null)
                    {
                        foreach (var item in myModel.cotizacionAporteCapacitacion)
                        {
                            item.idCotizacion = cotizacionOld.idCotizacion_R13;
                            //item.cotizacion = cotizacion;
                            item.aporteCapacitacion = db.AporteCapacitacion.Find(item.idAporteCapacitacion);
                            db.CotizacionAporteCapacitacion.Add(item);
                            db.SaveChanges();
                        }
                    }
                }




                return RedirectToAction("Index");
            }
            db.Configuration.ProxyCreationEnabled = false;
            myModel.clientes = db.Cliente.Where(x => x.softDelete == false).ToList();
            ViewBag.sucursales = GetSucursales();
            ViewBag.aportesCapacitacion = db.AporteCapacitacion.Where(x => !x.softDelete).ToList();
            return View(myModel);
        }
        // GET: Cotizacion_R13/Edit/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/" })]
        public ActionResult Recotizar(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cotizacion_R13 cotizacion_R13 = db.Cotizacion_R13
                .Where(x => x.idCotizacion_R13 == id)
                .Include(x => x.cotizacionAporteCapacitacion.Select(y => y.aporteCapacitacion))
                .FirstOrDefault();
            if (cotizacion_R13 == null)
            {
                return HttpNotFound();
            }
            db.Configuration.ProxyCreationEnabled = false;
            ViewModelCotizacion mymodel = new ViewModelCotizacion();
            mymodel.cotizacion = cotizacion_R13;
            mymodel.cotizacion.costo = db.Costo.Where(x => x.idCotizacion == mymodel.cotizacion.idCotizacion_R13).ToList();
            mymodel.clientes = db.Cliente.Where(x => x.softDelete == false).ToList();
            mymodel.cotizacionAporteCapacitacion = mymodel.cotizacion.cotizacionAporteCapacitacion;
            if (cotizacion_R13.curso != null)
            {
                mymodel.tipoEjecucion = cotizacion_R13.curso.tipoEjecucion;
            }
            ViewBag.sucursales = GetSucursales();
            ViewBag.aportesCapacitacion = db.AporteCapacitacion.Where(x => !x.softDelete).ToList();
            return View("Edit", mymodel);
        }
        // POST: Cotizacion_R13/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Recotizar(ViewModelCotizacion myModel)
        {
            Cotizacion_R13 cotizacion = myModel.cotizacion;
            cotizacion.sucursal = db.Sucursal.Find(cotizacion.sucursal.idSucursal);
            if (cotizacion.tipoCurso != "Duplicado Credencial" && cotizacion.tipoCurso != "Arriendo de Sala" && cotizacion.tipoCurso != "Tramitación Licencia")
            {
                if (cotizacion.idCurso == 0 || cotizacion.idCurso == null)
                {
                    ModelState.AddModelError("cotizacion.idCurso", "El campo Curso es obligatorio");
                }
                else
                {
                    var r11 = db.R11.Where(x => x.idCurso == cotizacion.idCurso).FirstOrDefault();
                    cotizacion.horasCurso = r11.horasPracticas + r11.horasTeoricas;
                }
            }
            myModel.cotizacionAporteCapacitacion = new List<CotizacionAporteCapacitacion>();
            if (myModel.cotizacionAporteCapacitacionPresencial != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionPresencial)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacionSincronico != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionSincronico)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacionAsincronico != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionAsincronico)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacionRecertificacion != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionRecertificacion)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacionRecertificacionSincrono != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionRecertificacionSincrono)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacionRecertificacionAsincronico != null)
            {
                foreach (var aporte in myModel.cotizacionAporteCapacitacionRecertificacionAsincronico)
                {
                    myModel.cotizacionAporteCapacitacion.Add(aporte);
                }
            }
            if (myModel.cotizacionAporteCapacitacion != null)
            {
                if (myModel.cotizacionAporteCapacitacion.Where(x => x.aCargo == null).FirstOrDefault() != null)
                {
                    ModelState.AddModelError("cotizacionAporteCapacitacion", "Se debe seleccionar los aportes de capacitación.");
                }
            }
            if (ModelState.IsValid)
            {
                if (cotizacion.modalidad == "Abierto")
                {
                    cotizacion.calendarizacionAbierta = db.CalendarizacionAbierta.Find(cotizacion.idCurso);
                    if (cotizacion.calendarizacionAbierta != null)
                    {
                        cotizacion.idCurso = cotizacion.calendarizacionAbierta.curso.idCurso;
                    }
                }
                cotizacion.fechaCreacion = DateTime.Now;
                // codigo cotizacion
                var ultimoCodigo = "0";
                if (db.Cotizacion_R13.OrderByDescending(x => x.idCotizacion_R13).FirstOrDefault() != null)
                {
                    ultimoCodigo = db.Cotizacion_R13.OrderByDescending(x => x.idCotizacion_R13).FirstOrDefault().codigoCotizacion;
                }
                cotizacion.codigoCotizacion = Helpers.CustomUtilsHelper.GeneracionCodigo(cotizacion.sucursal.prefijoCodigo, ultimoCodigo);
                cotizacion.softDelete = false;
                var userId = User.Identity.GetUserId();
                cotizacion.usuarioCreador = db.AspNetUsers.Find(userId);
                db.Cotizacion_R13.Add(cotizacion);
                db.SaveChanges();

                if (myModel.cotizacion.costo != null)
                {
                    foreach (Costo item in myModel.cotizacion.costo)
                    {
                        item.idCotizacion = cotizacion.idCotizacion_R13;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                if (myModel.cotizacionAporteCapacitacion != null)
                {
                    foreach (var item in myModel.cotizacionAporteCapacitacion)
                    {
                        item.idCotizacion = cotizacion.idCotizacion_R13;
                        item.cotizacion = cotizacion;
                        item.aporteCapacitacion = db.AporteCapacitacion.Find(item.idAporteCapacitacion);
                        db.CotizacionAporteCapacitacion.Add(item);
                        db.SaveChanges();
                    }
                }

                return RedirectToAction("Index");
            }
            db.Configuration.ProxyCreationEnabled = false;
            myModel.clientes = db.Cliente.Where(x => x.softDelete == false).ToList();
            ViewBag.sucursales = GetSucursales();
            ViewBag.aportesCapacitacion = db.AporteCapacitacion.Where(x => !x.softDelete).ToList();
            return View(myModel);
        }

        // POST: Cotizacion_R13/Delete/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Cotizacion_R13 cotizacion_R13 = db.Cotizacion_R13.Find(id);


            if (db.Comercializacion
                .Where(x => x.softDelete == false)
                .Where(x => x.cotizacion.idCotizacion_R13 == id)
                .Count() > 0)
            {
                TempData["PosseComercializacion"] = "No se puede eliminar la cotizacion con nombre curso '" + cotizacion_R13.curso.nombreCurso + "' del cliente '" + cotizacion_R13.nombreEmpresa + "', porque ya tiene una comercializacion creada.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["PosseComercializacion"] = null;
            }

            //cotizacion_R13.costo = db.Costo.Where(x => x.cotizacion.idCotizacion_R13 == id).ToList();
            //db.Cotizacion_R13.Remove(cotizacion_R13);
            cotizacion_R13.softDelete = true;
            cotizacion_R13.sucursal = db.Sucursal.Find(cotizacion_R13.sucursal.idSucursal);
            db.SaveChanges();

            //var eliminar = db.Costo.Where(x => x.cotizacion.idCotizacion_R13 == id).ToList();
            //db.Costo.RemoveRange(eliminar.AsEnumerable());
            //db.SaveChanges();
            return RedirectToAction("Index");
        }

        public object DataR13(Cotizacion_R13 cotizacion, string firma)
        {
            var r11 = new R11();
            var contenidosEspecificos = new List<object>();
            if (cotizacion.curso != null)
            {
                r11 = db.R11.Where(r => r.idCurso == cotizacion.curso.idCurso).FirstOrDefault();
                var i = 1;
                foreach (var item in r11.conteidoEspecifico)
                {
                    contenidosEspecificos.Add(new
                    {
                        numero = i,
                        contenido = item.nombre,
                        desarrollo = item.itemConteidoEspecificoR11.LastOrDefault().contenidoEspecifico,
                        horasPracticas = item.horasP,
                        horasTeoricas = item.horasT
                    });
                    i++;
                }
            }
            //var fechaInicio = "Por definir";
            //if (cotizacion.fechaInicio != null)
            //{
            //    fechaInicio = cotizacion.fechaInicio.Value.ToString("dd de MMMM de yyyy", CultureInfo.InvariantCulture);
            //}
            var nombreEncargadoPagos = db.Contacto.Find(cotizacion.contactoEncargadoPago) != null ? db.Contacto.Find(cotizacion.contactoEncargadoPago).nombreCompleto : "";
            var nombreContacto = db.Contacto.Find(cotizacion.contacto) != null ? db.Contacto.Find(cotizacion.contacto).nombreCompleto : "";
            var valorTotal = "0";
            var valorPart = "0";
            if (cotizacion.tipoCurso == "Duplicado Credencial" || cotizacion.tipoCurso == "Arriendo de Sala" && cotizacion.tipoCurso == "Tramitación Licencia")
            {
                valorTotal = String.Format("{0:C}", cotizacion.valorUnico);
                valorPart = String.Format("{0:C}", cotizacion.valorUnico / cotizacion.cantidadParticipante);
            }
            else
            {
                if (cotizacion.costo.Count() > 0)
                {
                    valorTotal = String.Format("{0:C}", cotizacion.costo.Select(y => y.total).Sum());
                    valorPart = String.Format("{0:C}", cotizacion.costo.Select(y => y.total).Sum() / cotizacion.cantidadParticipante);
                }
            }
            var evaluaciones = new List<object>();
            var r12 = new CostoCursoR12();
            var materiales = new List<object>();
            if (cotizacion.curso != null)
            {
                foreach (var item in cotizacion.curso.evaluaciones.Where(e => e.softDelete == false))
                {
                    if (item.categoria != CategoriaEvaluacion.Diagnostico)
                    {
                        evaluaciones.Add(new
                        {
                            item.nombre,
                            item.categoria
                        });
                    }
                }
                r12 = db.CostoCursoR12.Where(r => r.idCurso == cotizacion.curso.idCurso).FirstOrDefault();
                if (r12 != null)
                {
                    r12.costoParticularCurso = db.CostoParticularCurso.Where(c => c.costoCursoR12.idCostoCursoR12 == r12.idCostoCursoR12).ToList();
                    foreach (var item in r12.costoParticularCurso)
                    {
                        if (item.cantidad > 0)
                        {
                            materiales.Add(new
                            {
                                item.detalle,
                                item.categoria
                            });
                        }
                    }
                }
            }
            var aportesCapacitacion = new List<object>();
            foreach (var item in cotizacion.cotizacionAporteCapacitacion.Where(x => x.aCargo != ACargo.No_Aplica))
            {
                var insecap = "";
                if (item.aCargo == ACargo.Insecap)
                {
                    insecap = "X";
                }
                var cliente = "";
                if (item.aCargo == ACargo.Cliente)
                {
                    cliente = "X";
                }
                aportesCapacitacion.Add(new
                {
                    item.aporteCapacitacion.nombre,
                    insecap,
                    cliente
                });
            }
            CultureInfo culture = new CultureInfo("es");
            var valorFechaInicio = "Por definir";
            if (cotizacion.fechaInicio != null) valorFechaInicio = cotizacion.fechaInicio.Value.ToString("dd \"de\" MMMM \"de\" yyyy", culture);
            var valorFechaTermino = "Por definir";
            if (cotizacion.fechaTermino != null) valorFechaTermino = cotizacion.fechaTermino.Value.ToString("dd \"de\" MMMM \"de\" yyyy", culture);
            var instructor = "";
            var runInstructor = "";
            var nombreCurso = "";
            var codigoCurso = "";
            if (cotizacion.curso != null)
            {
                instructor = r11.relator.contacto.nombreCompleto;
                runInstructor = r11.relator.contacto.run;
                nombreCurso = cotizacion.curso.nombreCurso;
                codigoCurso = cotizacion.curso.codigoCurso;
            }
            var data = new
            {
                fecha = DateTime.Now.ToString("dd \"de\" MMMM \"de\" yyyy", culture),
                fechaInicio = valorFechaInicio,
                fechaTermino = valorFechaTermino,
                cotizacion.codigoCotizacion,
                cotizacion.nombreEmpresa,
                cotizacion.nombreDiploma,
                cotizacion.lugarRealizacion,
                cotizacion.codigoSence,
                nombreCurso,
                codigoCurso,
                cotizacion.modalidad,
                cantPart = cotizacion.cantidadParticipante,
                sucursal = cotizacion.sucursal.nombre,
                r11.fundamentacionTecnica,
                r11.objetivoGeneral,
                cantidadPersonas = r11.cantPersona,
                r11.poblacionObjetivo,
                r11.requisitosIngreso,
                tecnicasMetodologicas = r11.tecnicaMetodologica,
                r11.materialDidactico,
                r11.materialEntregable,
                requisitosLegalesNormativosReglamentarios = r11.requisitosReglamentarios,
                //r11.nombreModulo,
                //r11.instructor,
                instructor,
                runInstructor,
                r11.requisitosTecnicos,
                requisitosTecnicosRelatoreConocimientosSala = r11.requisitosTecnicosRelatores,
                fechaCreacion = r11.fechaCreacion.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                totalHorasTeoricas = String.Format("{0:0.#}", r11.horasTeoricas),
                totalHorasPracticas = String.Format("{0:0.#}", r11.horasPracticas),
                horasTotales = String.Format("{0:0.#}", r11.horasPracticas + r11.horasTeoricas),
                vigencia = cotizacion.vigenciaCredenciales,
                procesoPractico = cotizacion.procesoPractico,
                fechaCaducidad = r11.fechaCaducidad.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                r11.diplomaAprobacion,
                diplomaParticipacion = r11.DiplomaParticipacion,
                contenidosEspecificos,
                nombreContacto,
                nombreEncargadoPagos,
                evaluaciones,
                valorTotal,
                valorPart,
                materiales,
                firma,
                vendedor = cotizacion.usuarioCreador.nombreCompleto,
                runVendedor = cotizacion.usuarioCreador.run,
                correoVendedor = cotizacion.usuarioCreador.Email,
                telefonoVendedor = cotizacion.usuarioCreador.telefono,
                aportesCapacitacion
            };
            return data;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/" })]
        public ActionResult DescargarR13(int? id)
        {
            var cotizacion_R13 = db.Cotizacion_R13
                .Where(c => c.softDelete == false)
                .Include(c => c.cliente)
                .Include(c => c.curso)
                .Include(c => c.costo);
            if (TempData["PosseComercializacion"] != null)
            {
                ViewBag.PosseComercializacion = TempData["PosseComercializacion"];
            }
            var tienenComercializacion = new List<Cotizacion_R13>(); ;
            var a = cotizacion_R13.ToList();
            foreach (var item in cotizacion_R13.ToList())
            {
                var dc = db.Comercializacion.Where(c => c.cotizacion.idCotizacion_R13 == item.idCotizacion_R13).ToList();
                if (db.Comercializacion
                    .Where(c => c.softDelete == false)
                    .Where(c => c.cotizacion.idCotizacion_R13 == item.idCotizacion_R13)
                    .FirstOrDefault() != null)
                {
                    tienenComercializacion.Add(item);
                }
            }
            ViewBag.tienenComercializacion = tienenComercializacion;

            var cotizacion = db.Cotizacion_R13.Find(id);
            if (cotizacion == null)
            {
                return HttpNotFound();
            }
            // template segun tipo ejecucion
            var nombreTemplate = "r13_presencial";
            if (cotizacion.curso != null)
            {
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Presencial) nombreTemplate = "r13_presencial";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono) nombreTemplate = "r13_sincronico";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono) nombreTemplate = "r13_asincronico";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion) nombreTemplate = "r13_recertificacion_presencial";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Sincronica) nombreTemplate = "r13_recertificacion_sincronico";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica) nombreTemplate = "r13_recertificacion_asincronico";
            }
            // verificar si existe template
            var template = db.Template
                .Where(t => t.nombre == nombreTemplate)
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                TempData["Error"] = "No se encontro el template para generar el reporte, debe existir un template con el nombre \"" + nombreTemplate + "\" y tipo \"word\".";
                //ModelState.AddModelError("errorTemplate", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r13\" y tipo \"word\".");
                return View("Index", cotizacion_R13.ToList());
            }
            if (cotizacion.usuarioCreador.firma == null)
            {
                // indicar q hubo un error
                // ModelState.AddModelError("errorFirmaLiderComercial", "No se encontro la firma del Lider Comercial.");
                TempData["Error"] = "No se encontro la firma del Lider Comercial.";
                return View("Index", cotizacion_R13.ToList());
            }
            if (cotizacion.curso != null)
            {
                var r11 = db.R11.Where(r => r.idCurso == cotizacion.curso.idCurso).FirstOrDefault();
                if (r11 == null)
                {
                    TempData["Error"] = "La cotización no tiene definido un R11";
                    return View("Index", cotizacion_R13.ToList());
                }

                if (r11.relator == null)
                {
                    TempData["Error"] = "La cotización no tiene definido un un relator en su R11";
                    return View("Index", cotizacion_R13.ToList());
                }
            }
            return RedirectToAction("GenerarReporteR13", new { id });
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/" })]
        [EnableJsReport()]
        public async Task<ActionResult> GenerarReporteR13(int? id)
        {
            var cotizacion = db.Cotizacion_R13
                .Where(x => x.idCotizacion_R13 == id)
                .Include(x => x.cotizacionAporteCapacitacion.Select(y => y.aporteCapacitacion))
                .FirstOrDefault();
            cotizacion.costo = db.Costo.Where(c => c.cotizacion.idCotizacion_R13 == id).ToList();
            var nombreTemplate = "r13_presencial";
            if (cotizacion.curso != null)
            {
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Presencial) nombreTemplate = "r13_presencial";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono) nombreTemplate = "r13_sincronico";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono) nombreTemplate = "r13_asincronico";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion) nombreTemplate = "r13_recertificacion_presencial";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Sincronica) nombreTemplate = "r13_recertificacion_sincronico";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica) nombreTemplate = "r13_recertificacion_asincronico";
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == nombreTemplate)
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
            var firma = await Files.BajarArchivoBytesAsync(cotizacion.usuarioCreador.firma);
            var firmaBase64 = Convert.ToBase64String(firma, 0, firma.Length);

            var nombreArchivo = "R13_COTIZACIÓN " + cotizacion.codigoCotizacion + "_" + cotizacion.nombreDiploma + "_" + cotizacion.nombreEmpresa + "";
            var elerning = "";
            if (cotizacion.curso != null)
            {
                var r11 = db.R11.Where(r => r.idCurso == cotizacion.curso.idCurso).FirstOrDefault();
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono
                    || cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono
                    || cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Sincronica
                    || cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica)
                {
                    elerning = "E";
                }
                nombreArchivo = "R13" + elerning + "_COTIZACIÓN " + cotizacion.codigoCotizacion + "_" + cotizacion.nombreDiploma + "_" + (r11.horasTeoricas + r11.horasPracticas) + " horas_" + cotizacion.nombreEmpresa + "";
            }

            var report = HttpContext
                .JsReportFeature()
                .Recipe(Recipe.Docx)
                .Engine(Engine.Handlebars)
                .Configure((r) => r.Template.Docx = new Docx
                {
                    TemplateAsset = new Asset
                    {
                        Content = base64,
                        Encoding = "base64"
                    }
                })
                .Configure((r) => r.Data = DataR13(cotizacion, "data:image/png;base64," + firmaBase64))
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"" + nombreArchivo + ".docx\"");
            return null;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/" })]
        public ActionResult GenerarPdfR13(int? id)
        {
            var cotizacion_R13 = db.Cotizacion_R13
                .Where(c => c.softDelete == false)
                .Include(c => c.cliente)
                .Include(c => c.curso)
                .Include(c => c.costo);

            if (TempData["PosseComercializacion"] != null)
            {
                ViewBag.PosseComercializacion = TempData["PosseComercializacion"];
            }
            var tienenComercializacion = new List<Cotizacion_R13>(); ;
            var a = cotizacion_R13.ToList();
            foreach (var item in cotizacion_R13.ToList())
            {
                var dc = db.Comercializacion.Where(c => c.cotizacion.idCotizacion_R13 == item.idCotizacion_R13).ToList();
                if (db.Comercializacion
                    .Where(c => c.softDelete == false)
                    .Where(c => c.cotizacion.idCotizacion_R13 == item.idCotizacion_R13)
                    .FirstOrDefault() != null)
                {
                    tienenComercializacion.Add(item);
                }
            }
            ViewBag.tienenComercializacion = tienenComercializacion;

            var cotizacion = db.Cotizacion_R13.Find(id);
            if (cotizacion == null)
            {
                return HttpNotFound();
            }
            // template segun tipo ejecucion
            var nombreTemplate = "r13_presencial";
            if (cotizacion.curso != null)
            {
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Presencial) nombreTemplate = "r13_presencial";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono) nombreTemplate = "r13_sincronico";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono) nombreTemplate = "r13_asincronico";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion) nombreTemplate = "r13_recertificacion_presencial";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Sincronica) nombreTemplate = "r13_recertificacion_sincronico";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica) nombreTemplate = "r13_recertificacion_asincronico";
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == nombreTemplate)
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                TempData["Error"] = "No se encontro el template para generar el reporte, debe existir un template con el nombre \"" + nombreTemplate + "\" y tipo \"word\".";
                //ModelState.AddModelError("errorTemplate", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r13\" y tipo \"word\".");
                return View("Index", cotizacion_R13.ToList());
            }
            if (cotizacion.usuarioCreador.firma == null)
            {
                // indicar q hubo un error
                // ModelState.AddModelError("errorFirmaLiderComercial", "No se encontro la firma del Lider Comercial.");
                TempData["Error"] = "No se encontro la firma del Lider Comercial.";
                return View("Index", cotizacion_R13.ToList());
            }

            if (cotizacion.curso != null)
            {
                var r11 = db.R11.Where(r => r.idCurso == cotizacion.curso.idCurso).FirstOrDefault();
                if (r11 == null)
                {
                    TempData["Error"] = "La cotización no tiene definido un R11";
                    return View("Index", cotizacion_R13.ToList());
                }

                if (r11.relator == null)
                {
                    TempData["Error"] = "La cotización no tiene definido un un relator en su R11";
                    return View("Index", cotizacion_R13.ToList());
                }
            }

            string hash = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                hash = Utils.Utils.GetHash(sha256Hash, DateTime.Now.ToString());
            }

            string createRequest = Url.Action("GenerarReportePdfR13", "Cotizacion_R13", new { id, id2 = hash }, Request.Url.Scheme);
            // Generate Request
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(createRequest);
            req.Method = "GET";

            // Get the Response
            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException e)
            {
                return View("Error", (object)"No se pudo generar el documento.");
            }

            var path = directory + hash;
            Byte[] bytes = System.IO.File.ReadAllBytes(path + ".pdf");

            System.IO.File.Delete(path + ".pdf");

            Response.ContentType = "application/pdf";

            var nombreArchivo = "R13_COTIZACIÓN " + cotizacion.codigoCotizacion + "_" + cotizacion.nombreDiploma + "_" + cotizacion.nombreEmpresa + "";
            var elerning = "";
            if (cotizacion.curso != null)
            {
                var r11 = db.R11.Where(r => r.idCurso == cotizacion.curso.idCurso).FirstOrDefault();
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono
                    || cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono
                    || cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Sincronica
                    || cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica)
                {
                    elerning = "E";
                }
                nombreArchivo = "R13" + elerning + "_COTIZACIÓN " + cotizacion.codigoCotizacion + "_" + cotizacion.nombreDiploma + "_" + (r11.horasTeoricas + r11.horasPracticas) + " horas_" + cotizacion.nombreEmpresa + "";
            }
            Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + nombreArchivo + ".pdf\"");

            return new FileContentResult(bytes, "application/pdf");
        }

        [EnableJsReport()]
        public async Task<ActionResult> GenerarReportePdfR13(int? id, string id2)
        {
            var cotizacion = db.Cotizacion_R13
                .Where(x => x.idCotizacion_R13 == id)
                .Include(x => x.cotizacionAporteCapacitacion.Select(y => y.aporteCapacitacion))
                .FirstOrDefault();
            cotizacion.costo = db.Costo.Where(c => c.cotizacion.idCotizacion_R13 == id).ToList();
            var nombreTemplate = "r13_presencial";
            if (cotizacion.curso != null)
            {
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Presencial) nombreTemplate = "r13_presencial";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono) nombreTemplate = "r13_sincronico";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono) nombreTemplate = "r13_asincronico";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion) nombreTemplate = "r13_recertificacion_presencial";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Sincronica) nombreTemplate = "r13_recertificacion_sincronico";
                if (cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica) nombreTemplate = "r13_recertificacion_asincronico";
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == nombreTemplate)
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
            var firma = await Files.BajarArchivoBytesAsync(cotizacion.usuarioCreador.firma);
            var firmaBase64 = Convert.ToBase64String(firma, 0, firma.Length);

            var report = HttpContext
                .JsReportFeature()
                .Recipe(Recipe.Docx)
                .Engine(Engine.Handlebars)
                .Configure((r) => r.Template.Docx = new Docx
                {
                    TemplateAsset = new Asset
                    {
                        Content = base64,
                        Encoding = "base64"
                    }
                })
                .Configure((r) => r.Data = DataR13(cotizacion, "data:image/png;base64," + firmaBase64))
                .OnAfterRender((r) =>
                {
                    var path = directory + id2;
                    using (var file = System.IO.File.Open(path + ".docx", FileMode.Create))
                    {
                        r.Content.CopyTo(file);
                    }
                    var appWord = new Microsoft.Office.Interop.Word.Application();
                    var wordDocument = appWord.Documents.Open(path + ".docx");
                    wordDocument.ExportAsFixedFormat(path + ".pdf", Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF);
                    wordDocument.Close();
                    appWord.Quit();
                    System.IO.File.Delete(path + ".docx");
                });
            return null;
        }

        private SelectList GetSucursales()
        {
            return new SelectList(db.Sucursal.Select(s => new SelectListItem
            {
                Text = s.nombre,
                Value = s.idSucursal.ToString()
            }).ToList(), "Value", "Text");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
