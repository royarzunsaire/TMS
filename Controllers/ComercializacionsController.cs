using jsreport.MVC;
using jsreport.Types;
using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using SGC.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Linq.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ZXing;
using ZXing.QrCode;

namespace SGC.Controllers
{
    public class ComercializacionsController : Controller
    {
        //private static readonly string directory = System.Web.HttpContext.Current.Server.MapPath("~/Files/");
        //private static readonly string directory = @"C:/Users/sgc_admin/Desktop/SGC_Testing/Files/";
        private static readonly string directory = ConfigurationManager.AppSettings["directory"] + "Files/";
        private static readonly string email = ConfigurationManager.AppSettings["email"];
        private static readonly string emailPassword = ConfigurationManager.AppSettings["emailPassword"];
        private static readonly string domain = ConfigurationManager.AppSettings["domain"];
        private InsecapContext db = new InsecapContext();

        private static List<string> errors = new List<string>();

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        // GET: Comercializacions
        public ActionResult IndexPostCurso()
        {
            DateTime fechaInicio = DateTime.Parse("01/10/2021");
            var ventas = db.Comercializacion
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= fechaInicio)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) <= DateTime.Today)
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
                .ToList();
            viewModel.ForEach(x => x.postCurso = postCurso.FirstOrDefault(y => y.comercializacion.idComercializacion == x.comercializacion.idComercializacion));

            ViewBag.DJOPendientes = viewModel.Where(x => x.postCurso != null && x.postCurso.djo != true)
                .Where(x => x.comercializacion.cotizacion.codigoSence != null && x.comercializacion.cotizacion.codigoSence != "" && x.comercializacion.cotizacion.tieneCodigoSence != "on")
                .Where(x => x.comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono || x.comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono)
                .Count();

            var DJPPentientes = 0;
            foreach (ViewModelPostCurso comercializacion in viewModel
                .Where(x => x.comercializacion.cotizacion.codigoSence != null && x.comercializacion.cotizacion.codigoSence != "" && x.comercializacion.cotizacion.tieneCodigoSence != "on")
                .Where(x => x.comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono || x.comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono))
            {
                foreach (Participante participante in comercializacion.comercializacion.participantes.Where(x => !x.conDeclaracionJuradaPersona).
                    Where(x => x.comercializacion.cotizacion.tipoCurso == "Curso" || x.comercializacion.cotizacion.tipoCurso == "Recertificación").ToList())
                {
                    DJPPentientes++;
                }
            }


            ViewBag.DJPPendientes = DJPPentientes;
            ViewBag.r24Pendientes = viewModel.Where(x => x.postCurso != null && x.postCurso.djo != true)
                .Where(x => x.comercializacion.cotizacion.codigoSence != null && x.comercializacion.cotizacion.codigoSence != "" && x.comercializacion.cotizacion.tieneCodigoSence != "on")
                .Where(x => x.comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Presencial || x.comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion)
                .Count();
            ViewBag.emailNoEnviados = viewModel.Where(x => x.postCurso == null || x.postCurso.mailClient != true)
                .Count();
            return View("Index");
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        // GET: Comercializacions
        public ActionResult MyIndex()
        {
            string idUser = User.Identity.GetUserId();
            ViewBag.enProceso = db.Comercializacion
                .Where(c => c.softDelete == false && (c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on") == false)
                .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.En_Proceso)
                .Where(x => x.usuarioCreador.Id == idUser).Where(c => c.cotizacion.sucursal.nombre != "Distancia").Where(c => c.cotizacion.sucursal.nombre != "SPD")
                .Count();
            ViewBag.enProcesoSence = db.Comercializacion
                .Where(c => c.softDelete == false && c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on")
                .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.En_Proceso)
                .Where(x => x.usuarioCreador.Id == idUser).Where(c => c.cotizacion.sucursal.nombre != "Distancia").Where(c => c.cotizacion.sucursal.nombre != "SPD")
                .Count();
            ViewBag.terminadas = db.Comercializacion
                .Where(c => c.softDelete == false)
                .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada)
                .Where(x => x.usuarioCreador.Id == idUser).Where(c => c.cotizacion.sucursal.nombre != "Distancia").Where(c => c.cotizacion.sucursal.nombre != "SPD")
                .Count();
            ViewBag.terminadasSence = db.Comercializacion
                .Where(c => c.softDelete == false && c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on")
                .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada_SENCE)
                .Where(x => x.usuarioCreador.Id == idUser).Where(c => c.cotizacion.sucursal.nombre != "Distancia").Where(c => c.cotizacion.sucursal.nombre != "SPD")
                .Count();
            return View("Index");
        }
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        // GET: Comercializacions
        public ActionResult MyIndexCoordinador()
        {
            ViewBag.enProceso = db.Comercializacion
                .Where(c => c.softDelete == false && (c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on") == false)
                .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.En_Proceso)
                .Count();
            ViewBag.enProcesoSence = db.Comercializacion
                .Where(c => c.softDelete == false && c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on")
                .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.En_Proceso)
                .Count();
            ViewBag.terminadas = db.Comercializacion
                .Where(c => c.softDelete == false)
                .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada)
                .Count();
            ViewBag.terminadasSence = db.Comercializacion
                .Where(c => c.softDelete == false && c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on")
                .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada_SENCE)
                .Count();
            return View("Index");
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        // GET: Comercializacions
        public ActionResult IndexCoordinador()
        {
            return View();
        }
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        // GET: Comercializacions
        public ActionResult PostCurso(int id)
        {
            var posCurso = db.PostCurso.Where(x => x.comercializacion.idComercializacion == id).FirstOrDefault();
            var now = DateTime.Now;
            if (posCurso == null)
            {
                posCurso = new PostCurso
                {
                    idPostCurso = 0,
                    comercializacion = db.Comercializacion.Find(id),
                    fechaInfoCheck = now,
                    fechaCredReady = now,
                    fechaDjo = now,
                    fechaMailClient = now,
                };

            }
            return View(posCurso);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult PostCurso([Bind(Include = "idPostCurso,infoCheck,mailClient,djo,credReady,fechaInfoCheck,fechaCredReady,fechaDjo,fechaMailClient")] PostCurso poscurso)
        {
            var oldPosCurso = db.PostCurso.Find(poscurso.idPostCurso);
            var user = db.AspNetUsers.Find(User.Identity.GetUserId());
            var now = DateTime.Now;
            var dateMin = new DateTime(1970, 1, 1);
            if (oldPosCurso == null)
            {
                var idComercializacion = Convert.ToInt32(Request["comercializacion.idComercializacion"]);
                poscurso.comercializacion = db.Comercializacion.Find(idComercializacion);
                //creadores
                poscurso.creadorCredReady = user;
                poscurso.creadorDjo = user;
                poscurso.creadorMailClient = user;
                poscurso.creadorInfoCheck = user;

                if (!poscurso.credReady)
                    poscurso.fechaCredReady = dateMin;
                if (!poscurso.djo)
                    poscurso.fechaDjo = dateMin;
                if (!poscurso.mailClient)
                    poscurso.fechaMailClient = dateMin;
                if (!poscurso.infoCheck)
                    poscurso.fechaInfoCheck = dateMin;

                db.PostCurso.Add(poscurso);
            }
            else
            {

                if (!poscurso.credReady)
                    oldPosCurso.fechaCredReady = dateMin;
                if (!poscurso.djo)
                    oldPosCurso.fechaDjo = dateMin;
                if (!poscurso.mailClient)
                    oldPosCurso.fechaMailClient = dateMin;
                if (!poscurso.infoCheck)
                    oldPosCurso.fechaInfoCheck = dateMin;


                if (oldPosCurso.credReady != poscurso.credReady)
                {
                    oldPosCurso.credReady = poscurso.credReady;
                    oldPosCurso.creadorCredReady = user;
                }
                if (oldPosCurso.djo != poscurso.djo)
                {
                    oldPosCurso.djo = poscurso.djo;
                    oldPosCurso.creadorDjo = user;
                }
                if (oldPosCurso.infoCheck != poscurso.infoCheck)
                {
                    oldPosCurso.infoCheck = poscurso.infoCheck;
                    oldPosCurso.creadorInfoCheck = user;
                }
                if (oldPosCurso.mailClient != poscurso.mailClient)
                {
                    oldPosCurso.mailClient = poscurso.mailClient;
                    oldPosCurso.creadorMailClient = user;
                }
                db.Entry(oldPosCurso).State = EntityState.Modified;

            }
            db.SaveChanges();
            return RedirectToAction("IndexPostCurso");
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        // GET: Comercializacions
        public ActionResult Index()
        {
            ViewBag.enProceso = db.Comercializacion
                  .Where(c => c.softDelete == false && (c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on") == false)
                    .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.En_Proceso)
                    .Where(c => c.cotizacion.sucursal.nombre != "Distancia").Where(c => c.cotizacion.sucursal.nombre != "SPD")
                    .Count();
            ViewBag.enProcesoSence = db.Comercializacion
                .Where(c => c.softDelete == false && c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on")
                  .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.En_Proceso)
                  .Where(c => c.cotizacion.sucursal.nombre != "Distancia").Where(c => c.cotizacion.sucursal.nombre != "SPD")
                  .Count();
            ViewBag.terminadas = db.Comercializacion
                .Where(c => c.softDelete == false)
                  .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada)
                  .Where(c => c.cotizacion.sucursal.nombre != "Distancia").Where(c => c.cotizacion.sucursal.nombre != "SPD")
                  .Count();
            ViewBag.terminadasSence = db.Comercializacion
                .Where(c => c.softDelete == false && c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on")
                  .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada_SENCE)
                  .Where(c => c.cotizacion.sucursal.nombre != "Distancia").Where(c => c.cotizacion.sucursal.nombre != "SPD")
                  .Count();

            if (errors.Count() > 0)
            {
                foreach (string error in errors)
                {

                    ModelState.AddModelError("", error);
                }
                errors = new List<string>();
            }

            return View();
        }
        [Authorize]
        [HttpPost]
        // GET: Comercializacions
        public ActionResult SaveClientFactura()
        {
            var id = Convert.ToInt32(Request["id"]);
            var value = Request["value"];

            if (id == null || value == null)
            {
                return Json(new
                {
                    error = "Variables Vacias",

                }, JsonRequestBehavior.AllowGet); ;
            }
            var comercializacionBD = db.Comercializacion
            .Where(x => x.softDelete == false)
            .Where(x => x.idComercializacion == id).FirstOrDefault();
            if (comercializacionBD == null)
            {
                return Json(new
                {
                    error = "Comercializacion No encontrada",

                }, JsonRequestBehavior.AllowGet); ;
            }

            comercializacionBD.clientFactura = Convert.ToBoolean(value);
            db.Entry(comercializacionBD).State = EntityState.Modified;
            db.SaveChanges();

            var jsonResult = Json(new
            {
                id = Request["id"],
                value = Request["value"],
                error = "",
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        // GET: Comercializacions
        public ActionResult IndexCoordinadorData()
        {

            int start = Convert.ToInt32(Request["start"]);
            int draw = Convert.ToInt32(Request["draw"]);
            String search = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];
            int recordsTotal = 0;
            int count = Convert.ToInt32(Request["length"]);
            string view = Request["view"];

            var dataDb = db.Bloque
               .Where(x => x.comercializacion.cotizacion != null && x.comercializacion.cotizacion.softDelete == false && x.comercializacion.softDelete == false)
              .Where(x => x.comercializacion.cotizacion.curso != null);

            DateTime dateSearch = DateTime.MinValue;
            DateTime.TryParse(search, out dateSearch);




            if (string.IsNullOrEmpty(search))
            {
                recordsTotal = dataDb.Count();
            }
            else
            {


                dataDb = dataDb.Where(x => x.comercializacion.cotizacion.codigoCotizacion.ToLower().Contains(search)
            || x.comercializacion.cotizacion.cliente.nombreEmpresa.ToLower().Contains(search)
            || x.comercializacion.cotizacion.curso.nombreCurso.ToLower().Contains(search)
            || x.comercializacion.cotizacion.tipoCurso.ToLower().Contains(search)
            || x.comercializacion.comercializacionEstadoComercializacion.Where(e => e.EstadoComercializacion.ToString().Contains(search)).Any()
            || x.comercializacion.cotizacion.curso.tipoEjecucion.ToString().ToLower().Contains(search)
            || x.comercializacion.cotizacion.usuarioCreador.nombres.ToLower().Contains(search)
             || x.comercializacion.cotizacion.usuarioCreador.apellidoMaterno.ToLower().Contains(search)
              || x.comercializacion.cotizacion.usuarioCreador.apellidoPaterno.ToLower().Contains(search)
              || DateTime.Compare(x.comercializacion.fechaTermino, dateSearch) == 0
               || DateTime.Compare(x.comercializacion.fechaInicio, dateSearch) == 0

               );

            }


            var dataTemp = dataDb
                .GroupBy(x => new { x.comercializacion, x.fecha, x.sala })
                .Select(gcs => new
                {
                    comercializacion = gcs.Key.comercializacion,
                    fecha = gcs.Key.fecha,
                    sala = gcs.Key.sala,
                    bloques = gcs.ToList()
                })
                .OrderByDescending(x => x.fecha);
            recordsTotal = dataTemp.Count();
            if (count == -1)
            {
                count = recordsTotal;
            }

            var data = dataTemp
                .Skip(start)
                .Take(count)
                .ToList();

            ViewBag.coordinadores = GetCoordinador();
            ViewBag.templatesR50 = GetTemplatesR50();
            List<object> resultset = new List<object>();
            foreach (var bloque in data)
            {
                var curso = "";
                if (bloque.comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && bloque.comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && bloque.comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
                {
                    curso = bloque.comercializacion.cotizacion.curso.nombreCurso;
                }
                else
                {
                    curso = bloque.comercializacion.cotizacion.tipoCurso;

                }

                @ViewBag.clipboard = GenerateClipboard(bloque.comercializacion);

                var coordinador = bloque.bloques.Count() + " Bloques: ";
                if (bloque.bloques.Count() == 0 || bloque.bloques.All(x => x.coordinador == null))
                {
                    coordinador += "Sin coordinadores";
                }
                else
                {
                    var bloquesConCoordinador = bloque.bloques.Where(x => x.coordinador != null);
                    coordinador += String.Join(";", bloquesConCoordinador.Select(x => x.coordinador.nombreCompleto).Distinct());

                    if (bloquesConCoordinador.Count() < bloque.bloques.Count())
                    {
                        for (int i = 0; i < (bloque.bloques.Count() - bloquesConCoordinador.Count()); i++)
                        {
                            coordinador += "; Sin coordinadores";
                        }
                    }
                }
                resultset.Add(
                    new
                    {
                        bloque.comercializacion.cotizacion.codigoCotizacion,
                        curso,
                        cliente = bloque.comercializacion.cotizacion.cliente.nombreEmpresa,
                        bloque = String.Format("{0:dd/MM/yyyy}", bloque.fecha) + " (" + String.Format("{0:HH:mm}", bloque.bloques.OrderBy(x => x.horarioInicio).FirstOrDefault().horarioInicio) + "-" + String.Format("{0:HH:mm}", bloque.bloques.OrderBy(x => x.horarioInicio).LastOrDefault().horarioTermino) + " )",
                        sala = bloque.sala.nombre,
                        tipoEjecucion = bloque.comercializacion.cotizacion.curso != null ? bloque.comercializacion.cotizacion.curso.tipoEjecucion.ToString() : " ",
                        comercial = bloque.comercializacion.usuarioCreador.nombres + " " + bloque.comercializacion.usuarioCreador.apellidoPaterno,
                        alumnos = bloque.comercializacion.cotizacion.cantidadParticipante,
                        relator = String.Join(";", bloque.bloques.Select(x => x.relator.contacto.nombreCompleto).Distinct()),
                        coordinador,
                        menu = ConvertPartialViewToString(PartialView("IndexMenuCoordinador", bloque.bloques.Where(x => x.comercializacion != null).FirstOrDefault())),

                    }
                    ); ;



            }


            var jsonResult = Json(new { draw, recordsTotal, recordsFiltered = recordsTotal, data = resultset }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveCoordinador()
        {
            int idBloque = Convert.ToInt32(Request["idBloque"]);
            String id = Request["idCoordinador"];
            var error = "ok";
            var bloque = db.Bloque.Find(idBloque);
            var coordinador = db.AspNetUsers.Find(id);
            var bloques = bloque.comercializacion.bloques.Where(x => x.sala.idSala == bloque.sala.idSala && x.fecha == bloque.fecha).ToList();

            try
            {

                foreach (var item in bloques)
                {
                    item.coordinador = coordinador;
                    db.Entry(item).State = EntityState.Modified;

                }
                db.SaveChanges();
            }
            catch (Exception e)
            {
                error = e.Message;
            }



            var jsonResult = Json(new { error, id }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public SelectList GetCoordinador()
        {
            List<AspNetUsers> coordinadores = db.AspNetUsers
                .Where(x => x.AspNetRoles.Any(y => y.Name.Contains("Lider Comercial")
                || y.Name.Contains("DigitaciónYPostCurso")
                 || y.Name.Contains("Administrador")
                  || y.Name.Contains("APOYO TMS")
                   || y.Name.Contains("Diseño & Desarrollo")
                    || y.Name.Contains("Facturacion")

                )).OrderBy(x => x.nombres).ToList();


            return new SelectList(coordinadores.Select(c => new SelectListItem
            {
                Text = c.nombreCompleto + " [" + c.UserName + "]",
                Value = c.Id.ToString()
            }).ToList(), "Value", "Text");
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
            bool ep = Convert.ToBoolean(Request["ep"]);
            bool eps = Convert.ToBoolean(Request["eps"]);
            bool t = Convert.ToBoolean(Request["t"]);
            bool ts = Convert.ToBoolean(Request["ts"]);
            String te = Convert.ToString(Request["te"]);
            String es = Convert.ToString(Request["es"]);

            var dataDb = db.Comercializacion
              .Where(x => x.softDelete == false)
               .Where(x => x.cotizacion != null && x.cotizacion.softDelete == false)
              ;

            string idUser = User.Identity.GetUserId();
            if (view.Contains("MyIndexCoordinador"))
            {
                dataDb = dataDb.Where(x => x.bloques.Where(y => y.coordinador.Id == idUser).Any());
            }
            else if (view.Contains("MyIndex"))
            {
                dataDb = dataDb.Where(x => x.usuarioCreador.Id == idUser);
            }



            DateTime dateSearch = DateTime.MinValue;
            DateTime.TryParse(search, out dateSearch);




            if (string.IsNullOrEmpty(search) && !ep && !eps && !t && !ts)
            {
                recordsTotal = dataDb.Count();

            }
            else
            {
                if (!string.IsNullOrEmpty(search))
                {
                    var documentos = db.DocumentoCompromiso.Where(x => x.softDelete == false && x.numeroSerie.Contains(search)).ToList();
                    List<int> DocumentosList = new List<int>();
                    if (documentos != null && documentos.Count() > 0)
                    {
                        DocumentosList = documentos.Select(x => x.cotizacion.idCotizacion_R13).ToList();
                    }

                    dataDb = dataDb.Where(x => x.cotizacion.codigoCotizacion.ToLower().Contains(search)
                || x.cotizacion.cliente.nombreEmpresa.ToLower().Contains(search)
                || x.cotizacion.curso.nombreCurso.ToLower().Contains(search)
                || x.cotizacion.tipoCurso.ToLower().Contains(search)
                || x.comercializacionEstadoComercializacion.Where(e => e.EstadoComercializacion.ToString().Contains(search)).Any()
                || x.cotizacion.curso.tipoEjecucion.ToString().ToLower().Contains(search)
                || x.cotizacion.usuarioCreador.nombres.ToLower().Contains(search)
                 || x.cotizacion.usuarioCreador.apellidoMaterno.ToLower().Contains(search)
                  || x.cotizacion.usuarioCreador.apellidoPaterno.ToLower().Contains(search)
                  || x.faena.nombre.ToLower().Contains(search)
                  || DateTime.Compare(x.fechaTermino, dateSearch) == 0
                   || DateTime.Compare(x.fechaInicio, dateSearch) == 0
                    || DocumentosList.Any(y => y == x.cotizacion.idCotizacion_R13)
                   );

                }

            }
            if (ep || eps || ts || t)
            {
                ViewBag.enProceso = db.Comercializacion
                    .Where(c => (c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on") == false)
                    .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.En_Proceso)
                    .Count();
                ViewBag.enProcesoSence = db.Comercializacion
                    .Where(c => c.softDelete == false && c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on")
                    .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.En_Proceso)
                    .Count();
                ViewBag.terminadas = db.Comercializacion
                    .Where(c => c.softDelete == false)
                    .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada)
                    .Count();
                ViewBag.terminadasSence = db.Comercializacion
                    .Where(c => c.softDelete == false && c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on")
                    .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada_SENCE)
                    .Count();

                if (ep)
                {
                    dataDb = dataDb.Where(c => (c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on") == false)
                        .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.En_Proceso);
                }
                else if (eps)
                {
                    dataDb = dataDb.Where(c => c.softDelete == false && c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on")
                        .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.En_Proceso);
                }
                else if (t)
                {
                    dataDb = dataDb.Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada);
                }
                else if (ts)
                {
                    dataDb = dataDb.Where(c => c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on")
                        .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada_SENCE);
                }
            }
            if (te != "")
            {
                dataDb = dataDb.Where(c => c.cotizacion.curso.tipoEjecucion.ToString() == te);
            }
            if (es != "")
            {
                switch (es)
                {
                    case "En Proceso":
                        dataDb = dataDb.Where(c => (c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on") == false)
                        .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.En_Proceso);
                        break;
                    case "En Proceso SENCE":
                        dataDb = dataDb.Where(c => c.softDelete == false && c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on")
                        .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.En_Proceso);
                        break;
                    case "Teminadas":
                        dataDb = dataDb.Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada);
                        break;
                    case "Terminadas SENCE":
                        dataDb = dataDb.Where(c => c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on")
                        .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada_SENCE);
                        break;
                }
            }
            recordsTotal = dataDb.Count();
            if (count == -1)
            {
                count = recordsTotal;
            }
            var data = dataDb.OrderByDescending(x => x.fechaInicio)
                .Skip(start)
                .Take(count)
                .ToList();

            ViewBag.templatesR50 = GetTemplatesR50();
            List<object> resultset = new List<object>();
            List<int> idComer = data.Select(x => x.idComercializacion).ToList();
            var encuestas = db.FeedbackMoodle.Where(x => idComer.Any(y => y == x.comercializacion.idComercializacion)).ToList();


            foreach (Comercializacion comercializacion in data)
            {
                double sumMaxPointTotal = 0;
                double totalPointTotal = 0;
                double totalPorcent = 0;
                var feedback = encuestas.FirstOrDefault(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion);
                if (feedback != null)
                {
                    foreach (var feedbackItem in feedback.feedbackItemMoodle)
                    {
                        var valueMax = feedbackItem.feedbackItemDataMoodle.Count() == 0 ? 0 : feedbackItem.feedbackItemDataMoodle.Max(y => Convert.ToInt32(y.value));
                        totalPointTotal += valueMax * feedback.completedcount;
                        sumMaxPointTotal += Math.Round(feedbackItem.feedbackItemDataMoodle.Sum(x => x.answercount * Convert.ToInt32(x.value)), 2, MidpointRounding.ToEven);

                    }
                    totalPorcent = Math.Round(sumMaxPointTotal * 100 / totalPointTotal, 2, MidpointRounding.ToEven);

                }
                var curso = "";
                if (comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
                {
                    curso = comercializacion.cotizacion.curso.nombreCurso;
                }
                else
                {
                    curso = comercializacion.cotizacion.tipoCurso;

                }
                @ViewBag.clipboard = GenerateClipboard(comercializacion);
                @ViewBag.coordinador = view.Contains("Coordinador");
                @ViewBag.link = domain + "/Comercializacions/RedirectVideoLLamadaAlumnosComercializacion?id=" + comercializacion.idComercializacion;

                PostCurso postCurso = null;
                if (view.Contains("PostCurso"))
                {
                    postCurso = db.PostCurso.FirstOrDefault(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion);

                }
                if (postCurso == null)
                {
                    postCurso = new PostCurso { comercializacion = new Comercializacion(), mailClient = false, djo = false, credReady = false, infoCheck = false };
                }

                var tipoEjecucion = comercializacion.cotizacion.curso != null ? comercializacion.cotizacion.curso.tipoEjecucion.ToString() : " ";
                tipoEjecucion = tipoEjecucion.Replace("Recertificacion_Asincronica", "R-Asincronica");
                tipoEjecucion = tipoEjecucion.Replace("Elearning_Asincrono", "E-Asincrono");
                tipoEjecucion = tipoEjecucion.Replace("Elearning_Sincrono", "E-Sincrono");
                tipoEjecucion = tipoEjecucion.Replace("Recertificacion_Sincronica", "R-Sincronica");

                var fecha = String.Format("{0:dd/MM/yyyy}", comercializacion.fechaInicio) == String.Format("{0:dd/MM/yyyy}", comercializacion.fechaTermino) ? String.Format("{0:dd/MM/yyyy}", comercializacion.fechaInicio) : String.Format("{0:dd/MM/yyyy}", comercializacion.fechaInicio) + " a " + String.Format("{0:dd/MM/yyyy}", comercializacion.fechaTermino);
                resultset.Add(
                    new
                    {
                        comercializacion.cotizacion.codigoCotizacion,
                        curso,
                        cliente = comercializacion.cotizacion.cliente.nombreEmpresa,
                        fecha,
                        fechaInicio = String.Format("{0:dd/MM/yyyy}", comercializacion.fechaInicio),
                        fechaTermino = String.Format("{0:dd/MM/yyyy}", comercializacion.fechaTermino),
                        tipoEjecucion,
                        estado = comercializacion.comercializacionEstadoComercializacion.LastOrDefault().EstadoComercializacion.ToString(),
                        comercial = comercializacion.usuarioCreador.nombres + " " + comercializacion.usuarioCreador.apellidoPaterno,

                        postCurso = ConvertPartialViewToString(PartialView("IndexMenuPostCurso", postCurso)),

                        totalPorcent = totalPorcent + " %",
                        menu = ConvertPartialViewToString(PartialView("IndexMenu", comercializacion)),
                    }
                    );



            }


            var jsonResult = Json(new { draw, recordsTotal, recordsFiltered = recordsTotal, data = resultset }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }





        private string GenerateClipboard(Comercializacion comercializacion)
        {
            string clipboard = "*{0} : {1}&Tipo de Venta: {2}&SENCE: {7}&Bloques: {3}&Cantidad: {4}&Ejecutivo: {5}&Instructor: {6}";


            String bloqueString = "";
            string tipoEjecucion = comercializacion.cotizacion.curso != null ? comercializacion.cotizacion.curso.tipoEjecucion.ToString() : " ";
            if (tipoEjecucion.ToString().Contains("Presencial") || tipoEjecucion.ToString().Contains("Recertificacion") || tipoEjecucion.ToString().Contains("Recertificacion_Sincronica") || tipoEjecucion.ToString().Contains("Elearning_Sincrono"))
            {


                bloqueString = "No hay bloques el día " + String.Format("{0:dddd d , MMMM , yyyy}", comercializacion.fechaInicio).Replace(",", "de");
                int bloqueCont = comercializacion.bloques.ToList().Count();
                if (bloqueCont > 0)
                {
                    bloqueString = "";
                    String dateString = "";
                    foreach (Bloque bloque in comercializacion.bloques)
                    {
                        String currentDate = String.Format("{0:dddd d , MMMM , yyyy}", bloque.fecha.Date).Replace(",", "de");
                        if (!dateString.Equals(currentDate))
                        {

                            bloqueString += " Bloques del día " + currentDate + ": ";
                            bloqueString += bloque.horarioInicio.ToString("HH:mm") + " - " + bloque.horarioTermino.ToString("HH:mm") + "";
                        }
                        else
                        {
                            bloqueString += bloque.horarioInicio.ToString("HH:mm") + " - " + bloque.horarioTermino.ToString("HH:mm") + "";
                        }
                        dateString = currentDate;

                    }
                }

            }
            else if (tipoEjecucion.ToString().Contains("Recertificacion_Asincronica") || tipoEjecucion.ToString().Contains("Elearning_Asincrono"))
            {
                bloqueString = " Usted dispone del siguiente rango de fechas para realizar el curso:  " + comercializacion.fechaInicio.ToString("dd/MM/yyyy") + " - " + comercializacion.fechaTermino.ToString("dd/MM/yyyy");


            }
            string relatores = "Sin relatores";
            if (comercializacion.relatoresCursos != null)
            {
                relatores = string.Join(";", comercializacion.relatoresCursos.Select(x => x.relator.contacto.nombreCompleto));

            }
            var curso = "";
            if (comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                curso = comercializacion.cotizacion.curso.nombreCurso;
            }
            else
            {
                curso = comercializacion.cotizacion.tipoCurso;

            }

            return String.Format(clipboard,
              curso,
              comercializacion.cotizacion.codigoCotizacion,

              comercializacion.cotizacion.curso != null ? comercializacion.cotizacion.curso.tipoEjecucion.ToString() : " ",
              bloqueString,
              comercializacion.cotizacion.cantidadParticipante,
              comercializacion.usuarioCreador.nombres + " " + comercializacion.usuarioCreador.apellidoPaterno,
              relatores,
              comercializacion.cotizacion.tieneCodigoSence == null ? "SI" : "NO"
              );
        }

        // GET: Comercializacions/Details/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Details(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comercializacion comercializacion = db.Comercializacion
                .Where(c => c.idComercializacion == id)
                .FirstOrDefault();
            List<string> codigoCot = new List<string>();
            List<string> idCom = new List<string>();
            foreach (DocumentoCompromiso documento in comercializacion.cotizacion.documentosCompromiso)
            {
                List<DocumentoCompromiso> allDoc = db.DocumentoCompromiso
              .Where(c => c.numeroSerie == documento.numeroSerie)
              .Where(c => c.cotizacion.idCliente == comercializacion.cotizacion.idCliente)
              .ToList();


                string codTemp = "";
                foreach (DocumentoCompromiso documentoId in allDoc)
                {
                    Comercializacion comercializacionDoc = db.Comercializacion
              .Where(c => c.cotizacion.idCotizacion_R13 == documentoId.cotizacion.idCotizacion_R13)
              .FirstOrDefault();
                    if (codTemp != "")
                    {
                        codTemp = String.Join(",", new string[2] { codTemp, comercializacionDoc.idComercializacion.ToString() });
                    }
                    else
                    {
                        codTemp = comercializacionDoc.idComercializacion.ToString();
                    }




                }
                idCom.Add(codTemp);
                codigoCot.Add(String.Join(",", allDoc.Select(x => x.cotizacion.codigoCotizacion)));

            }
            comercializacion.cotizacion.costo = db.Costo.Where(c => c.idCotizacion == comercializacion.cotizacion.idCotizacion_R13).ToList();
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            if (comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                if (db.R11.Where(r => r.idCurso == comercializacion.cotizacion.curso.idCurso).FirstOrDefault() != null)
                {
                    ViewBag.sence = db.R11.Where(r => r.idCurso == comercializacion.cotizacion.curso.idCurso).FirstOrDefault().codigoSence;
                }
                else
                {
                    ViewBag.sence = null;
                }
            }
            else
            {
                ViewBag.sence = null;
            }
            Contacto cliente = db.Contacto.Where(x => x.idContacto == comercializacion.cotizacion.contacto).FirstOrDefault();

            ViewBag.emailCliente = cliente.correo;
            ViewBag.codigoCot = codigoCot;
            ViewBag.idCom = idCom;
            ViewBag.nombreContacto = cliente.nombreCompleto;
            ViewBag.nombreEncargado = db.Contacto.Find(comercializacion.cotizacion.contactoEncargadoPago).nombreCompleto;
            return View(comercializacion);
        }

        // GET: Comercializacions/Create/id
        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/" })]
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // obtener cotizacion
            Cotizacion_R13 cotizacion = db.Cotizacion_R13.Where(x => x.idCotizacion_R13 == id).Include(x => x.costo).Include(x => x.faena).FirstOrDefault();
            if (cotizacion == null)
            {
                return HttpNotFound();
            }
            //cotizacion.costo = new List<Costo>();
            //cotizacion.costo = db.Costo.Where(c => c.idCotizacion == cotizacion.idCotizacion_R13).ToList();
            Comercializacion verificarComercializacion = db.Comercializacion.Where(x => x.softDelete == false).Where(c => c.cotizacion.idCotizacion_R13 == id).FirstOrDefault();
            if (verificarComercializacion != null)
            {
                return RedirectToAction("Details", new { id = verificarComercializacion.idComercializacion });
            }
            // crear la comercializacion
            Comercializacion comercializacion = new Comercializacion();
            comercializacion.cotizacion = cotizacion;
            comercializacion.faena = cotizacion.faena;
            //comercializacion.pagos = new List<Pago>();
            if (cotizacion.fechaInicio != null) comercializacion.fechaInicio = cotizacion.fechaInicio.Value;
            if (cotizacion.fechaTermino != null) comercializacion.fechaTermino = cotizacion.fechaTermino.Value;
            //comercializacion.vigenciaCredenciales = DateTime.Now.AddYears(2);
            comercializacion.vigenciaCredenciales = cotizacion.vigenciaCredenciales;
            comercializacion.clientFactura = true;
            // obtener todos los datos requeridos para crear la comercializacion
            CargarDatosVista(comercializacion);
            return View(comercializacion);
        }

        private void CargarDatosVista(Comercializacion comercializacion)
        {
            if (comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                ViewBag.relatores = GetRelatoresCurso(comercializacion);
                ViewBag.relatoresSence = GetRelatoresCursoSence(comercializacion, true);
                ViewBag.relatoresNoSence = GetRelatoresCursoSence(comercializacion, false);
                ViewBag.otics = GetOtics();
                ViewBag.infoRelatores = GetInfoRelatoresCurso(comercializacion.cotizacion.idCurso.Value);
                if (db.R11.Where(r => r.idCurso == comercializacion.cotizacion.curso.idCurso).FirstOrDefault() != null)
                {
                    ViewBag.sence = db.R11.Where(r => r.idCurso == comercializacion.cotizacion.curso.idCurso).FirstOrDefault().codigoSence;
                }
                else
                {
                    ViewBag.sence = null;
                }
            }
            else
            {
                ViewBag.relatores = new List<RelatorCurso>();
                ViewBag.relatoresSence = new List<RelatorCurso>();
                ViewBag.relatoresNoSence = new List<RelatorCurso>();
                ViewBag.otics = GetOtics();
                ViewBag.infoRelatores = new List<RelatorCurso>();
                ViewBag.sence = null;
            }
            ViewBag.tiposDocCompromiso = GetTiposDocCompromisoCliente(comercializacion.cotizacion.idCliente);
            ViewBag.ciudades = GetCiudades();
            ViewBag.cantPago = 0;
            if (comercializacion.cotizacion.documentosCompromiso != null)
            {
                ViewBag.cantPago = comercializacion.cotizacion.documentosCompromiso.Count();
            }
            ViewBag.cantRelator = 0;
            if (comercializacion.relatoresCursos != null)
            {
                ViewBag.cantRelator = comercializacion.relatoresCursos.Count();
            }
            ViewBag.contactosCliente = GetContactosCliente(comercializacion.cotizacion);
            ViewBag.encargadosPagoCliente = GetEncargadosPagoCliente(comercializacion.cotizacion);
            var tempClient = db.Cliente.Where(x => x.softDelete == false).ToList();
            ViewBag.clientes = tempClient;
            tempClient = tempClient.Select(x => new Cliente
            {
                idCliente = x.idCliente,
                nombreEmpresa = x.nombreEmpresa,
                razonSocial = x.razonSocial,
                telefonoCorporativo = x.telefonoCorporativo,
                direccion = x.direccion,
                rut = x.rut,
                descEspecial = x.descEspecial,
                idMandante = x.idMandante

            }).ToList();

            var jsonResult = Json(tempClient, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            ViewBag.clientesJson = new JavaScriptSerializer().Serialize(jsonResult.Data);


            if (comercializacion.faena != null)
            {

                ViewBag.faenasSeleccionados = new JavaScriptSerializer().Serialize(comercializacion.faena.idFaena);
            }
            else
            {
                ViewBag.faenasSeleccionados = new JavaScriptSerializer().Serialize("");
            }
            ViewBag.faenas = Getfaenas(comercializacion);


            //ViewBag.encargadoPago = db.Contacto.Find(comercializacion.cotizacion.contactoEncargadoPago) != null ? db.Contacto.Find(comercializacion.cotizacion.contactoEncargadoPago).nombreCompleto : "";
            //ViewBag.contacto = db.Contacto.Find(comercializacion.cotizacion.contacto) != null ? db.Contacto.Find(comercializacion.cotizacion.contacto).nombreCompleto : "";
        }

        [CustomAuthorize(new string[] { "/Cotizacion_R13/", "/Cotizacion_R13/Create/", "/Comercializacions/" })]
        [HttpPost]
        public JsonResult CargarContactos(int? id)
        {
            var cotizacion = db.Cotizacion_R13.Find(id);
            return Json((from c in cotizacion.cliente.clienteContactoCotizacion
                         let Text = "[" + c.contacto.run + "]" + " " + c.contacto.nombres + " " + c.contacto.apellidoPaterno + " " + c.contacto.apellidoMaterno
                         let Value = c.idContacto.ToString()
                         select new { Text, Value }).ToList());

        }

        [CustomAuthorize(new string[] { "/Cotizacion_R13/", "/Cotizacion_R13/Create/", "/Comercializacions/" })]
        [HttpPost]
        public JsonResult CargarEncargadosPago(int? id)
        {
            var cotizacion = db.Cotizacion_R13.Find(id);
            return Json((from c in cotizacion.cliente.encargadoPagos
                         let Text = "[" + c.contacto.run + "]" + " " + c.contacto.nombres + " " + c.contacto.apellidoPaterno + " " + c.contacto.apellidoMaterno
                         let Value = c.idContacto.ToString()
                         select new { Text, Value }).ToList());

        }

        public SelectList GetContactosCliente(Cotizacion_R13 cotizacion)
        {
            return new SelectList(cotizacion.cliente.clienteContactoCotizacion
            .Select(x => new SelectListItem
            {
                Text = "[" + x.contacto.run + "]" + " " + x.contacto.nombres + " " + x.contacto.apellidoPaterno + " " + x.contacto.apellidoMaterno,
                Value = x.contacto.idContacto.ToString()
            }).ToList(), "Value", "Text");
        }

        public SelectList GetEncargadosPagoCliente(Cotizacion_R13 cotizacion)
        {
            return new SelectList(cotizacion.cliente.encargadoPagos
            .Select(x => new SelectListItem
            {
                Text = "[" + x.contacto.run + "]" + " " + x.contacto.nombres + " " + x.contacto.apellidoPaterno + " " + x.contacto.apellidoMaterno,
                Value = x.contacto.idContacto.ToString()
            }).ToList(), "Value", "Text");
        }

        private bool ArchivoValido(HttpPostedFileBase file, string nombreValidador)
        {
            // validar que se selecciono un archivo
            if (file.ContentLength <= 0)
            {
                ModelState.AddModelError(nombreValidador, "Se debe seleccionar un archivo.");
                return false;
            }
            else
            {
                // validar extenciones y tamaño maximo de los archivos
                var archivoValido = Files.ArchivoValido(file, new[] { ".pdf" }, 3 * 1024);
                if (archivoValido != "")
                {
                    ModelState.AddModelError(nombreValidador, archivoValido);
                    return false;
                }
            }
            return true;
        }

        private bool DocumentoCompromisoValido(DocumentoCompromiso documentoCompromiso, int? i)
        {
            var monto = 0;
            if (!int.TryParse(Request["montoDocCompromiso" + i], out monto))
            {
                ModelState.AddModelError("validarMontoDocCompromiso" + i, "El Campo Monto no es válido");
            }
            // validar documento compromiso
            var context = new ValidationContext(documentoCompromiso, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(documentoCompromiso, context, results, true);
            if (isValid)
            {
                return true;
            }
            // agregar los mensajes de error del documento compromiso al modelState
            foreach (var result in results)
            {
                if (result.MemberNames.Contains("numeroSerie"))
                {
                    ModelState.AddModelError("validarIdentificadorDocCompromiso" + i, result.ErrorMessage);
                }
                if (result.MemberNames.Contains("monto"))
                {
                    ModelState.AddModelError("validarMontoDocCompromiso" + i, result.ErrorMessage);
                }
            }
            return false;
        }

        //private List<DocumentoCompromiso> GuardarDocumentosCompromiso(List<DocumentoCompromiso> documentoCompromisosAntiguos)
        //{
        //    List<DocumentoCompromiso> documentoCompromisosNuevos = new List<DocumentoCompromiso>();
        //    for (int i = 0; i < int.Parse(Request["cantDocCompromiso"]); i++)
        //    {
        //        if (Request["tipoDocCompromiso" + i] != null)
        //        {
        //            // crear documento compromiso
        //            DocumentoCompromiso documentoCompromiso = new DocumentoCompromiso();
        //            DocumentoCompromiso documentoCompromisoAntiguo = new DocumentoCompromiso();
        //            if (Request["idDocCompromiso" + i] != null)
        //            {
        //                if (int.Parse(Request["idDocCompromiso" + i]) != 0)
        //                {
        //                    documentoCompromisoAntiguo = documentoCompromisosAntiguos
        //                        .Where(dc => dc.idDocumentoCompromiso == int.Parse(Request["idDocCompromiso" + i]))
        //                        .FirstOrDefault();
        //                    documentoCompromiso = documentoCompromisoAntiguo;
        //                }
        //            }
        //            documentoCompromiso.numeroSerie = Request["identificadorDocCompromiso" + i];
        //            if (Request["montoDocCompromiso" + i] != "")
        //            {
        //                var monto = 0;
        //                if (int.TryParse(Request["montoDocCompromiso" + i], out monto))
        //                {
        //                    documentoCompromiso.monto = monto;
        //                }
        //                else
        //                {
        //                    documentoCompromiso.monto = null;
        //                }
        //            }
        //            else
        //            {
        //                documentoCompromiso.monto = null;
        //            }
        //            documentoCompromiso.tipoDocCompromiso = db.TiposDocumentosPago.Find(int.Parse(Request["tipoDocCompromiso" + i]));
        //            documentoCompromiso.usuarioCreador = db.AspNetUsers.Find(User.Identity.GetUserId());
        //            documentoCompromiso.usuarioUltimaModificacion = db.AspNetUsers.Find(User.Identity.GetUserId());
        //            documentoCompromiso.fechaCreacion = DateTime.Now;
        //            documentoCompromiso.fechaUltimaModificacion = DateTime.Now;
        //            documentoCompromiso.softDelete = false;
        //            // guardar archivos
        //            HttpPostedFileBase file = Request.Files["docCompromiso" + i];
        //            if (documentoCompromiso == documentoCompromisoAntiguo && file.ContentLength <= 0)
        //            {
        //                DocumentoCompromisoValido(documentoCompromiso, i);
        //                documentoCompromisosNuevos.Add(documentoCompromiso);
        //            }
        //            else
        //            {
        //                var docCompromisoValido = DocumentoCompromisoValido(documentoCompromiso, i);
        //                var archivoValido = ArchivoValido(file, "validarDocCompromiso" + i);
        //                if (docCompromisoValido)
        //                {
        //                    if (archivoValido)
        //                    {
        //                        documentoCompromiso.documento = Files.CrearArchivoSinSubir(file, "documento-compromiso/");
        //                    }
        //                }
        //                documentoCompromisosNuevos.Add(documentoCompromiso);
        //            }
        //        }
        //    }
        //    return documentoCompromisosNuevos;
        //}

        //private void VerificarSiDocsCompromisoInresados(List<DocumentoCompromiso> documentosCompromiso, string tipoDocumentoOtro)
        //{
        //    if (documentosCompromiso.Count() == 0)
        //    {
        //        if (tipoDocumentoOtro == null)
        //        {
        //            ModelState.AddModelError("docCompromiso", "Se debe seleccionar un tipo de documento");
        //        }
        //    }
        //}

        private async Task<List<DocumentoCompromiso>> GuardarArchivos(List<DocumentoCompromiso> documentosCompromiso)
        {
            foreach (var item in documentosCompromiso)
            {
                if (item.documento != null)
                {
                    if (item.documento.file != null)
                    {
                        item.documento = await Files.SubirArchivoAsync(item.documento, item.documento.file);
                        if (item.documento == null)
                        {
                            ModelState.AddModelError("docCompromiso", "No se pudo guardar el archivo seleccionado.");
                        }
                    }
                }
            }
            return documentosCompromiso;
        }

        private async Task BorrarArchivosAntiguos(List<DocumentoCompromiso> documentosCompromiso, List<string> keysDocumentosAntiguo)
        {
            foreach (var itemAntiguo in keysDocumentosAntiguo)
            {
                bool borrarArchivo = true;
                foreach (var itemNuevo in documentosCompromiso)
                {
                    if (itemNuevo.documento.key == itemAntiguo)
                    {
                        borrarArchivo = false;
                    }
                }
                if (borrarArchivo)
                {
                    var documento = db.Storages.Where(s => s.key == itemAntiguo).FirstOrDefault();
                    await Files.BorrarArchivoAsync(documento);
                    db.Storages.Remove(documento);
                }
            }
        }

        private void BorrarDocumentosCompromiso()
        {
            var q = "select * from DocumentoCompromiso where ISNULL(cotizacion_idCotizacion_R13, '') = '' AND softdelete = 0;";
            var documentosCompromisoAntiguos = db.DocumentoCompromiso.SqlQuery(q).ToList();
            foreach (var item in documentosCompromisoAntiguos)
            {
                item.softDelete = true;
                db.Entry(item).State = EntityState.Modified;
                //db.DocumentoCompromiso.Remove(item);
            }
            db.SaveChanges();
        }

        private List<RelatorCurso> GuardarRelatores()
        {
            List<RelatorCurso> relatoresCurso = new List<RelatorCurso>();
            // guardar los relatores seleccionados
            for (int i = 0; i < int.Parse(Request["cantRelator"]); i++)
            {
                if (Request["relator" + i] != null)
                {
                    int[] ids = Array.ConvertAll(Request["relator" + i].Split('-'), int.Parse);
                    int idRelator = ids[0];
                    int idCurso = ids[1];
                    RelatorCurso relator = db.RelatorCurso
                        .Where(r => r.idCurso == idCurso)
                        .Where(r => r.idRelator == idRelator)
                        .FirstOrDefault();
                    relatoresCurso.Add(relator);
                }
            }
            return relatoresCurso;
        }

        private void VerificarSiRelatoresIngresados(List<RelatorCurso> relatoresCurso)
        {
            if (relatoresCurso.Count() == 0)
            {
                ModelState.AddModelError("relatorCurso", "Se debe seleccionar un relator");
            }
        }

        private List<DocumentoCompromiso> GuardarPagos(List<DocumentoCompromiso> documentoCompromisosAntiguos)
        {
            //List<Pago> pagos = new List<Pago>();
            List<DocumentoCompromiso> documentoCompromisosNuevos = new List<DocumentoCompromiso>();
            for (int i = 0; i < int.Parse(Request["cantPago"]); i++)
            {
                // crear pago
                Pago pago = new Pago();
                if (Request["tipoPago" + i] != null)
                {
                    if (Request["tipoPago" + i] == "0")
                    {
                        pago.tipoPago = TipoPago.Otic;
                        Otic otic = db.Otic.Find(int.Parse(Request["otic" + i]));
                        pago.otic = otic;
                    }
                    if (Request["tipoPago" + i] == "1")
                    {
                        pago.tipoPago = TipoPago.Sence;
                    }
                    if (Request["tipoPago" + i] == "2")
                    {
                        pago.tipoPago = TipoPago.CostoEmpresa;
                    }
                    //pagos.Add(pago);
                }
                if (Request["tipoDocCompromiso" + i] != null)
                {
                    // crear documento compromiso
                    DocumentoCompromiso documentoCompromiso = new DocumentoCompromiso();
                    DocumentoCompromiso documentoCompromisoAntiguo = new DocumentoCompromiso();
                    if (Request["idDocCompromiso" + i] != null)
                    {
                        if (int.Parse(Request["idDocCompromiso" + i]) != 0)
                        {
                            documentoCompromisoAntiguo = documentoCompromisosAntiguos
                                .Where(dc => dc.idDocumentoCompromiso == int.Parse(Request["idDocCompromiso" + i]))
                                .FirstOrDefault();
                            documentoCompromiso = documentoCompromisoAntiguo;
                        }
                    }
                    documentoCompromiso.numeroSerie = Request["identificadorDocCompromiso" + i];
                    if (Request["montoDocCompromiso" + i] != "")
                    {
                        var monto = 0;
                        if (int.TryParse(Request["montoDocCompromiso" + i], out monto))
                        {
                            documentoCompromiso.monto = monto;
                        }
                        else
                        {
                            documentoCompromiso.monto = null;
                        }
                    }
                    else
                    {
                        documentoCompromiso.monto = null;
                    }
                    if (Request["tipoDocCompromiso" + i] == "0")
                    {
                        // si el tipo doc compromiso es null es porq es OC de sence u otic
                        documentoCompromiso.tipoDocCompromiso = null;
                    }
                    else
                    {
                        documentoCompromiso.tipoDocCompromiso = db.TiposDocumentosPago.Find(int.Parse(Request["tipoDocCompromiso" + i]));
                    }
                    documentoCompromiso.usuarioCreador = db.AspNetUsers.Find(User.Identity.GetUserId());
                    documentoCompromiso.usuarioUltimaModificacion = db.AspNetUsers.Find(User.Identity.GetUserId());
                    documentoCompromiso.fechaCreacion = DateTime.Now;
                    documentoCompromiso.fechaUltimaModificacion = DateTime.Now;
                    documentoCompromiso.softDelete = false;
                    documentoCompromiso.tipoVenta = pago;
                    // guardar archivos
                    HttpPostedFileBase file = Request.Files["docCompromiso" + i];
                    if (documentoCompromiso == documentoCompromisoAntiguo && file.ContentLength <= 0)
                    {
                        DocumentoCompromisoValido(documentoCompromiso, i);
                        documentoCompromisosNuevos.Add(documentoCompromiso);
                    }
                    else
                    {
                        var docCompromisoValido = DocumentoCompromisoValido(documentoCompromiso, i);
                        var archivoValido = ArchivoValido(file, "validarDocCompromiso" + i);
                        if (docCompromisoValido)
                        {
                            if (archivoValido)
                            {
                                documentoCompromiso.documento = Files.CrearArchivoSinSubir(file, "documento-compromiso/");
                            }
                        }
                        documentoCompromisosNuevos.Add(documentoCompromiso);
                    }
                }
            }
            ValidarEliminarDocumentosCompromisoAntiguos(documentoCompromisosNuevos, documentoCompromisosAntiguos);
            return documentoCompromisosNuevos;
        }

        private void ValidarEliminarDocumentosCompromisoAntiguos(List<DocumentoCompromiso> documentoCompromisosNuevos, List<DocumentoCompromiso> documentoCompromisosAntiguos)
        {
            foreach (var docAntiguo in documentoCompromisosAntiguos)
            {
                var eliminado = true;
                foreach (var docNuevo in documentoCompromisosNuevos)
                {
                    if (docNuevo.idDocumentoCompromiso == docAntiguo.idDocumentoCompromiso)
                    {
                        eliminado = false;
                    }
                }
                if (eliminado)
                {
                    if (docAntiguo.factura != null)
                    {
                        if (!docAntiguo.factura.softDelete)
                        {
                            ModelState.AddModelError("", "No se puede eliminar un Tipo de Venta si ya tiene una factura.");
                        }
                    }
                }
            }
        }

        private void VerificarSiPagosIngresados(List<DocumentoCompromiso> pagos, bool sence)
        {
            if (pagos.Count() == 0)
            {
                ModelState.AddModelError("pago", "Se debe seleccionar un tipo de venta");
                return;
            }
            if (sence)
            {
                var ocSence = false;
                foreach (var pago in pagos)
                {
                    if (pago.tipoVenta.tipoPago == TipoPago.Sence || pago.tipoVenta.tipoPago == TipoPago.Otic)
                    {
                        ocSence = true;
                    }
                }
                if (!ocSence)
                {
                    ModelState.AddModelError("pago", "Se debe seleccionar una Orden de Compra de SENCE");
                }
            }

        }

        private void ValidarFechaInicioAnteriorFechaTermino(Comercializacion comercializacion)
        {
            // validar fecha inicio anterior a fecha termino
            if (DateTime.Compare((DateTime)comercializacion.fechaInicio, (DateTime)comercializacion.fechaTermino) > 0)
            {
                ModelState.AddModelError("fechaInicio", "La fecha de inicio debe ser anterior a la fecha de término");
            }
        }

        private void ValidarMontoFinal(Comercializacion comercializacion)
        {
            int montoAcmulado = 0;
            foreach (var item in comercializacion.cotizacion.documentosCompromiso.Where(x => x.softDelete == false).ToList())
            {
                if (item.monto != null)
                {
                    montoAcmulado += (int)item.monto;
                }
            }
            if (montoAcmulado != comercializacion.valorFinal - comercializacion.descuento)
            {
                ModelState.AddModelError("", "El monto total " + ((int)comercializacion.valorFinal - comercializacion.descuento).ToString("C", CultureInfo.CurrentCulture) + " no coincide con el total de los montos ingresados " + montoAcmulado.ToString("C", CultureInfo.CurrentCulture));
            }
        }

        private void ValidarDatosCotizacion(Comercializacion comercializacion)
        {
            ValidarFechaInicioAnteriorFechaTermino(comercializacion);
            // verificar que se ingreso la cantidad de participantes
            if (Request["cotizacion.cantidadParticipante"] == "")
            {
                ModelState.AddModelError("cotizacion.cantidadParticipante", "El campo Cantidad participantes es obligatorio");
            }
            // verificar que se selecciono el contacto
            if (Request["cotizacion.contacto"] == "" || Request["cotizacion.contacto"] == null)
            {
                ModelState.AddModelError("cotizacion.contacto", "El campo Contacto es obligatorio");
            }
            // verificar que se selecciono el encargado de pago
            if (Request["cotizacion.contactoEncargadoPago"] == "" || Request["cotizacion.contactoEncargadoPago"] == null)
            {
                ModelState.AddModelError("cotizacion.contactoEncargadoPago", "El campo Contacto Encargado Pagos es obligatorio");
            }
            if (comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                // verificar que se ingreso un lugar de realizacion
                if (Request["cotizacion.lugarRealizacion"] == "")
                {
                    ModelState.AddModelError("cotizacion.lugarRealizacion", "El campo Lugar de Realización es obligatorio");
                }
            }
        }

        private void ValidarComercializaciónNoDuplicada(Cotizacion_R13 cotizacion)
        {
            var comercializacion = db.Comercializacion
                .Where(x => x.cotizacion.idCotizacion_R13 == cotizacion.idCotizacion_R13)
                .Where(x => x.softDelete == false)
                .FirstOrDefault();
            if (comercializacion != null)
            {
                ModelState.AddModelError("", "Ya existe una comercialización para esa cotización.");
            }
        }

        // POST: Comercializacions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "idComercializacion,tipoDocumentoOtro,senceNet,R23ConDescriptorContenidos,observacion,fechaInicio,fechaTermino,vigenciaCredenciales,ciudad,valorFinal,clientDownload")] Comercializacion comercializacion)
        {
            // obtener cotizacion
            Cotizacion_R13 cotizacion = db.Cotizacion_R13.Find(int.Parse(Request["idCotizacion"]));
            ValidarComercializaciónNoDuplicada(cotizacion);
            cotizacion.costo = db.Costo.Where(c => c.idCotizacion == cotizacion.idCotizacion_R13).ToList();
            List<string> keysDocumentosAntiguo = new List<string>();
            foreach (var item in cotizacion.documentosCompromiso)
            {
                keysDocumentosAntiguo.Add(item.documento.key);
            }
            comercializacion.cotizacion = cotizacion;
            ValidarDatosCotizacion(comercializacion);
            // guardar datos
            comercializacion.ciudad = db.Ciudad.Find(comercializacion.ciudad.idCiudad);
            if (comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                comercializacion.relatoresCursos = GuardarRelatores();
                VerificarSiRelatoresIngresados(comercializacion.relatoresCursos.ToList());
            }
            if (Request["tieneSence"] != "")
            {
                bool tieneSence = Request["tieneSence"] == "true,false";
                comercializacion.cotizacion.tieneCodigoSence = tieneSence ? "on" : null;
            }

            comercializacion.cotizacion.documentosCompromiso = GuardarPagos(comercializacion.cotizacion.documentosCompromiso.ToList());
            VerificarSiPagosIngresados(comercializacion.cotizacion.documentosCompromiso.ToList(), comercializacion.cotizacion.codigoSence != null && comercializacion.cotizacion.tieneCodigoSence != "on");
            //comercializacion.cotizacion.documentosCompromiso = GuardarDocumentosCompromiso(comercializacion.cotizacion.documentosCompromiso.ToList());
            //VerificarSiDocsCompromisoInresados(comercializacion.cotizacion.documentosCompromiso.ToList(), comercializacion.tipoDocumentoOtro);
            ValidarMontoFinal(comercializacion);
            if (Request["cotizacion.giro"] != "" && Request["cotizacion.giro"] != null)
            {
                comercializacion.cotizacion.giro = Request["cotizacion.giro"];
            }
            else
            {
                ModelState.AddModelError("cotizacion.giro", "Campo Obligatorio");
            }
            // datos cotizacion
            if (Request["cotizacion.contacto"] != "" && Request["cotizacion.contacto"] != null)
            {
                comercializacion.cotizacion.contacto = int.Parse(Request["cotizacion.contacto"]);
            }
            if (Request["cotizacion.contactoEncargadoPago"] != "" && Request["cotizacion.contactoEncargadoPago"] != null)
            {
                comercializacion.cotizacion.contactoEncargadoPago = int.Parse(Request["cotizacion.contactoEncargadoPago"]);
            }
            comercializacion.cotizacion.cantidadParticipante = null;
            if (comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                if (Request["cotizacion.cantidadParticipante"] != "")
                {
                    comercializacion.cotizacion.cantidadParticipante = int.Parse(Request["cotizacion.cantidadParticipante"]);
                }
                comercializacion.cotizacion.lugarRealizacion = Request["cotizacion.lugarRealizacion"];

                if (Request["cotizacion.procesoPractico"].Contains("true"))
                {
                    comercializacion.cotizacion.procesoPractico = true;
                }
                else
                {
                    comercializacion.cotizacion.procesoPractico = false;
                }
                comercializacion.evaluaciones = comercializacion.cotizacion.curso.evaluaciones.Where(e => e.softDelete == false).ToList();
            }
            else
            {
                comercializacion.cotizacion.cantidadParticipante = 0;
                comercializacion.vigenciaCredenciales = 0;
                //comercializacion.cotizacion.nombreDiploma = comercializacion.cotizacion.tipoCurso;
                if (comercializacion.cotizacion.tipoCurso == "Arriendo de Sala")
                {
                    CrearBloqueArriendoSala(comercializacion);
                }
            }
            comercializacion.cotizacion.sucursal = db.Sucursal.Find(comercializacion.cotizacion.sucursal.idSucursal);
            // subir archivos
            if (ModelState.IsValid)
            {
                await GuardarArchivos(comercializacion.cotizacion.documentosCompromiso.ToList());
                await BorrarArchivosAntiguos(comercializacion.cotizacion.documentosCompromiso.ToList(), keysDocumentosAntiguo);
            }
            comercializacion.clientFactura = true;
            // validar la comercializacion
            if (ModelState.IsValid)
            {
                var user = db.AspNetUsers.Find(User.Identity.GetUserId());
                // datos comercializacion
                comercializacion.vigencia = true;
                comercializacion.usuarioCreador = user;
                comercializacion.usuarioUltimaEdicion = user;
                comercializacion.fechaCreacion = DateTime.Now;
                comercializacion.fechaDescuento = DateTime.Now;
                comercializacion.fechaUltimaEdicion = DateTime.Now;
                comercializacion.softDelete = false;
                comercializacion.clientFactura = true;
                if (Request["faena.idFaena"] != "")
                {
                    comercializacion.faena = db.Faena.Find(Convert.ToInt32(Request["faena.idFaena"]));
                }

                // guardar estado
                comercializacion.comercializacionEstadoComercializacion = new List<ComercializacionEstadoComercializacion>();
                ComercializacionEstadoComercializacion comercializacionEstadoComercializacion = new ComercializacionEstadoComercializacion();
                comercializacionEstadoComercializacion.EstadoComercializacion = EstadoComercializacion.En_Proceso;
                comercializacionEstadoComercializacion.fechaCreacion = DateTime.Now;
                comercializacionEstadoComercializacion.usuarioCreador = User.Identity.GetUserId();
                comercializacion.comercializacionEstadoComercializacion.Add(comercializacionEstadoComercializacion);
                db.Comercializacion.Add(comercializacion);
                cotizacion.cliente.postVenta = false;
                db.Entry(cotizacion.cliente).State = EntityState.Modified;
                db.SaveChanges();
                BorrarDocumentosCompromiso();
                if (comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
                {
                    if (comercializacion.relatoresCursos.Where(x => !x.validoSence).FirstOrDefault() != null
                        && comercializacion.cotizacion.codigoSence != null && comercializacion.cotizacion.codigoSence != "")
                    {
                        // notificar curso con relator sin sence
                        //var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta Instructor No vinculado a sence").FirstOrDefault();
                        //if (notificacionConfig != null)
                        //{
                        //    notificacionConfig.CrearNotificacion(db, comercializacion.cotizacion.codigoCotizacion, comercializacion.idComercializacion.ToString(), User.Identity.GetUserId());
                        //}
                    }
                }


                AgregarHistorialComercializacion(comercializacion.idComercializacion);
                if (comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
                {
                    Sala sala = db.Sala.Where(x => x.nombre.ToLower().Contains("temporal")).FirstOrDefault();
                    LugarAlmuerzo lugarAlmuerzo = db.LugarAlmuerzo.Where(x => x.nombre.ToLower().Contains("temporal")).FirstOrDefault();
                    if (sala != null && lugarAlmuerzo != null)
                    {
                        Bloque bloque = new Bloque
                        {
                            comercializacion = comercializacion,
                            fecha = comercializacion.fechaInicio,
                            horarioInicio = new DateTime(comercializacion.fechaInicio.Year, comercializacion.fechaInicio.Month, comercializacion.fechaInicio.Day, 09, 00, 00),
                            horarioTermino = new DateTime(comercializacion.fechaInicio.Year, comercializacion.fechaInicio.Month, comercializacion.fechaInicio.Day, 13, 00, 00),
                            relator = comercializacion.relatoresCursos.FirstOrDefault().relator,
                            sala = sala,
                            lugarAlmuerzo = lugarAlmuerzo

                        };
                        db.Bloque.Add(bloque);
                        db.SaveChanges();
                    }

                }

                if (comercializacion.fechaInicio == DateTime.Today)
                {
                    foreach (var relator in comercializacion.relatoresCursos)
                    {
                        // notificacion relator confirmacion curso
                        //var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta de Confirmacion de Curso Relator").FirstOrDefault();
                        //if (notificacionConfig != null)
                        //{
                        //    notificacionConfig.CrearNotificacionUsuario(db, comercializacion.cotizacion.curso.nombreCurso, comercializacion.idComercializacion + "-" + relator.relator.idRelator, relator.relator.contacto.usuario.Id, relator.relator.contacto.usuario);
                        //}
                        EnviarMailConfirmacinCursoRelator(comercializacion, relator);
                    }
                }

                return RedirectToAction("List", "Bloques", new { id = comercializacion.idComercializacion });
                //return RedirectToAction("MyIndex");
            }

            CargarDatosVista(comercializacion);
            return View(comercializacion);
        }

        private void CrearBloqueArriendoSala(Comercializacion comercializacion)
        {
            var bloque = new Bloque();
            bloque.comercializacion = comercializacion;
            bloque.fecha = comercializacion.fechaInicio;
            bloque.horarioInicio = comercializacion.fechaInicio;
            bloque.horarioTermino = comercializacion.fechaInicio;
            db.Bloque.Add(bloque);
        }

        private void ModificarBloqueArriendoSala(Comercializacion comercializacion)
        {
            var bloque = db.Bloque.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion).FirstOrDefault();
            bloque.fecha = comercializacion.fechaInicio;
            db.Entry(bloque).State = EntityState.Modified;
        }

        // POST: Comercializacions/Borrador
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Borrador([Bind(Include = "idComercializacion,tipoDocumentoOtro,senceNet,R23ConDescriptorContenidos,observacion,fechaInicio,fechaTermino,vigenciaCredenciales,ciudad,valorFinal")] Comercializacion comercializacion)
        {
            ModelState.Remove("fechaInicio");
            ModelState.Remove("fechaTermino");
            ModelState.Remove("ciudad.idCiudad");
            ModelState.Remove("vigenciaCredenciales");
            // obtener cotizacion
            Cotizacion_R13 cotizacion = db.Cotizacion_R13.Find(int.Parse(Request["idCotizacion"]));
            ValidarComercializaciónNoDuplicada(cotizacion);
            cotizacion.costo = db.Costo.Where(c => c.idCotizacion == cotizacion.idCotizacion_R13).ToList();
            List<string> keysDocumentosAntiguo = new List<string>();
            foreach (var item in cotizacion.documentosCompromiso)
            {
                keysDocumentosAntiguo.Add(item.documento.key);
            }
            comercializacion.cotizacion = cotizacion;
            // guardar datos
            comercializacion.ciudad = db.Ciudad.Find(comercializacion.ciudad.idCiudad);
            if (comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                comercializacion.relatoresCursos = GuardarRelatores();
            }
            comercializacion.cotizacion.documentosCompromiso = GuardarPagos(comercializacion.cotizacion.documentosCompromiso.ToList());
            // guardar estado
            comercializacion.comercializacionEstadoComercializacion = new List<ComercializacionEstadoComercializacion>();
            ComercializacionEstadoComercializacion comercializacionEstadoComercializacion = new ComercializacionEstadoComercializacion();
            comercializacionEstadoComercializacion.EstadoComercializacion = EstadoComercializacion.Borrador;
            comercializacionEstadoComercializacion.fechaCreacion = DateTime.Now;
            comercializacionEstadoComercializacion.usuarioCreador = User.Identity.GetUserId();
            comercializacion.comercializacionEstadoComercializacion.Add(comercializacionEstadoComercializacion);
            // datos cotizacion
            if (Request["cotizacion.contacto"] != "" && Request["cotizacion.contacto"] != null)
            {
                comercializacion.cotizacion.contacto = int.Parse(Request["cotizacion.contacto"]);
            }
            if (Request["cotizacion.contactoEncargadoPago"] != "" && Request["cotizacion.contactoEncargadoPago"] != null)
            {
                comercializacion.cotizacion.contactoEncargadoPago = int.Parse(Request["cotizacion.contactoEncargadoPago"]);
            }
            comercializacion.cotizacion.cantidadParticipante = 0;
            if (comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                if (Request["cotizacion.cantidadParticipante"] != "")
                {
                    comercializacion.cotizacion.cantidadParticipante = int.Parse(Request["cotizacion.cantidadParticipante"]);
                }
                if (Request["cotizacion.lugarRealizacion"] != "")
                {
                    comercializacion.cotizacion.lugarRealizacion = Request["cotizacion.lugarRealizacion"];
                }

                if (Request["cotizacion.procesoPractico"].Contains("true"))
                {
                    comercializacion.cotizacion.procesoPractico = true;
                }
                else
                {
                    comercializacion.cotizacion.procesoPractico = false;
                }
                comercializacion.evaluaciones = comercializacion.cotizacion.curso.evaluaciones.Where(e => e.softDelete == false).ToList();
            }
            else
            {
                //comercializacion.vigenciaCredenciales = DateTime.Now;
                comercializacion.vigenciaCredenciales = 0;
            }
            comercializacion.cotizacion.sucursal = db.Sucursal.Find(comercializacion.cotizacion.sucursal.idSucursal);
            // guardar archivos
            var docsValidos = true;
            foreach (var item in comercializacion.cotizacion.documentosCompromiso)
            {
                var docCompromisoValido = DocumentoCompromisoValido(item, null);
                var archivoValido = false;
                if (item.documento != null)
                {
                    if (item.documento.file == null)
                    {
                        if (keysDocumentosAntiguo.Contains(item.documento.key))
                        {
                            archivoValido = true;
                        }
                    }
                    else
                    {
                        archivoValido = ArchivoValido(item.documento.file, "false");
                    }
                }
                if (!docCompromisoValido || !archivoValido)
                {
                    docsValidos = false;
                }
            }
            if (!docsValidos)
            {
                CargarDatosVista(comercializacion);
                return View("Create", comercializacion);
            }
            await GuardarArchivos(comercializacion.cotizacion.documentosCompromiso.ToList());
            await BorrarArchivosAntiguos(comercializacion.cotizacion.documentosCompromiso.ToList(), keysDocumentosAntiguo);
            // datos comercializacion
            comercializacion.vigencia = true;
            comercializacion.usuarioUltimaEdicion = db.AspNetUsers.Find(User.Identity.GetUserId());
            comercializacion.fechaUltimaEdicion = DateTime.Now;
            comercializacion.usuarioCreador = db.AspNetUsers.Find(User.Identity.GetUserId());
            comercializacion.fechaCreacion = DateTime.Now;
            comercializacion.fechaDescuento = DateTime.Now;
            comercializacion.softDelete = false;
            // guardar fechas borrador
            if (DateTime.Compare(comercializacion.fechaInicio, DateTime.MinValue) == 0)
            {
                if (cotizacion.fechaInicio != null)
                {
                    comercializacion.fechaInicio = cotizacion.fechaInicio.Value;
                }
                else
                {
                    ModelState.AddModelError("fechaInicio", "El campo Fecha de Inicio es obligatorio");
                    CargarDatosVista(comercializacion);
                    return View("Create", comercializacion);
                }
            }
            if (DateTime.Compare(comercializacion.fechaTermino, DateTime.MinValue) == 0)
            {
                if (cotizacion.fechaTermino != null)
                {
                    comercializacion.fechaTermino = cotizacion.fechaTermino.Value;
                }
                else
                {
                    ModelState.AddModelError("fechaTermino", "El campo Fecha de Termino es obligatorio");
                    CargarDatosVista(comercializacion);
                    return View("Create", comercializacion);
                }
            }
            //if (DateTime.Compare(comercializacion.vigenciaCredenciales, DateTime.MinValue) == 0)
            //{
            //    comercializacion.vigenciaCredenciales = DateTime.Now.AddYears(2);
            //}
            // guardar la comercializacion
            db.Comercializacion.Add(comercializacion);
            db.SaveChanges();
            BorrarDocumentosCompromiso();
            return RedirectToAction("Index");
        }

        // GET: Comercializacions/Edit/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comercializacion comercializacion = db.Comercializacion.Where(x => x.idComercializacion == id).Include(x => x.faena).FirstOrDefault();

            comercializacion.cotizacion.costo = db.Costo.Where(c => c.idCotizacion == comercializacion.cotizacion.idCotizacion_R13).ToList();
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            if (comercializacion.comercializacionEstadoComercializacion.LastOrDefault().EstadoComercializacion == SGC.Models.EstadoComercializacion.Terminada
                && comercializacion.comercializacionEstadoComercializacion.LastOrDefault().EstadoComercializacion == SGC.Models.EstadoComercializacion.Cancelada
                && comercializacion.comercializacionEstadoComercializacion.LastOrDefault().EstadoComercializacion == SGC.Models.EstadoComercializacion.Deshabilitada)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            ViewBag.sucursales = new SelectList(db.Sucursal.Select(s => new SelectListItem
            {
                Text = s.nombre,
                Value = s.idSucursal.ToString()
            }).ToList(), "Value", "Text");
            CargarDatosVista(comercializacion);


            return View(comercializacion);
        }
        public SelectList Getfaenas(Comercializacion comercializacion)
        {
            return new SelectList(db.FaenaCliente.Where(x => x.faena.softDelete == false && x.cliente.idCliente == comercializacion.cotizacion.cliente.idCliente).Select(c => new SelectListItem
            {
                Text = c.faena.nombre,
                Value = c.faena.idFaena.ToString()
            }).ToList(), "Value", "Text");
        }
        private void EliminarPagos(List<Pago> pagos)
        {
            int cantPagos = pagos.Count();
            for (int i = 0; i < cantPagos; i++)
            {

                db.Pago.Remove(pagos.ElementAt<Pago>(0));
            }
        }

        // POST: Comercializacions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "idComercializacion,tipoDocumentoOtro,senceNet,clientDownload,R23ConDescriptorContenidos,observacion,fechaInicio,fechaTermino,vigenciaCredenciales,bloque,ciudad")] Comercializacion comercializacion)
        {
            // obtener la comercializacion de la base de datos
            Comercializacion comercializacionBD = db.Comercializacion.Where(x => x.idComercializacion == comercializacion.idComercializacion).Include(x => x.faena).FirstOrDefault();
            comercializacionBD.cotizacion.costo = db.Costo.Where(c => c.idCotizacion == comercializacionBD.cotizacion.idCotizacion_R13).ToList();
            List<string> keysDocumentosAntiguo = new List<string>();
            foreach (var item in comercializacionBD.cotizacion.documentosCompromiso)
            {
                keysDocumentosAntiguo.Add(item.documento.key);
            }
            comercializacion.cotizacion = comercializacionBD.cotizacion;
            ValidarDatosCotizacion(comercializacion);
            // guardar datos
            comercializacionBD.ciudad = db.Ciudad.Find(comercializacion.ciudad.idCiudad);
            if (comercializacionBD.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                var relatoresCursos = comercializacionBD.relatoresCursos;
                comercializacionBD.relatoresCursos = GuardarRelatores();
                VerificarSiRelatoresIngresados(comercializacionBD.relatoresCursos.ToList());
                //EliminarPagos(comercializacionBD.pagos.ToList());
                //comercializacionBD.pagos = GuardarPagos();
                //VerificarSiPagosIngresados(comercializacionBD.pagos.ToList());
            }
            comercializacionBD.cotizacion.documentosCompromiso = GuardarPagos(comercializacionBD.cotizacion.documentosCompromiso.ToList());
            VerificarSiPagosIngresados(comercializacionBD.cotizacion.documentosCompromiso.ToList(), comercializacionBD.cotizacion.codigoSence != null && comercializacionBD.cotizacion.tieneCodigoSence != "on");
            comercializacionBD.tipoDocumentoOtro = comercializacion.tipoDocumentoOtro;
            ValidarMontoFinal(comercializacionBD);
            if (Request["cotizacion.giro"] != "" && Request["cotizacion.giro"] != null)
            {
                comercializacion.cotizacion.giro = Request["cotizacion.giro"];
            }
            else
            {
                ModelState.AddModelError("cotizacion.giro", "Campo Obligatorio");
            }
            // datos cotizacion
            if (Request["cotizacion.contacto"] != "" && Request["cotizacion.contacto"] != null)
            {
                comercializacion.cotizacion.contacto = int.Parse(Request["cotizacion.contacto"]);
            }
            if (Request["cotizacion.contactoEncargadoPago"] != "" && Request["cotizacion.contactoEncargadoPago"] != null)
            {
                comercializacion.cotizacion.contactoEncargadoPago = int.Parse(Request["cotizacion.contactoEncargadoPago"]);
            }
            if (Request["cotizacion.idCliente"] != "" && Request["cotizacion.idCliente"] != null)
            {
                comercializacion.cotizacion.idCliente = int.Parse(Request["cotizacion.idCliente"]);
            }
            comercializacionBD.cotizacion.cantidadParticipante = null;
            if (comercializacionBD.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                if (Request["cotizacion.cantidadParticipante"] != "")
                {
                    comercializacionBD.cotizacion.cantidadParticipante = int.Parse(Request["cotizacion.cantidadParticipante"]);
                }
                comercializacionBD.cotizacion.lugarRealizacion = Request["cotizacion.lugarRealizacion"];
                comercializacionBD.vigenciaCredenciales = comercializacion.vigenciaCredenciales;

                if (Request["cotizacion.procesoPractico"].Contains("true"))
                {
                    comercializacionBD.cotizacion.procesoPractico = true;
                }
                else
                {
                    comercializacionBD.cotizacion.procesoPractico = false;
                }
            }
            else
            {
                comercializacionBD.cotizacion.cantidadParticipante = 0;
                comercializacion.cotizacion.nombreDiploma = comercializacion.cotizacion.tipoCurso;
                if (comercializacion.cotizacion.tipoCurso == "Arriendo de Sala")
                {
                    ModificarBloqueArriendoSala(comercializacion);
                }
            }
            comercializacion.cotizacion.sucursal = db.Sucursal.Find(comercializacion.cotizacion.sucursal.idSucursal);
            // subir archivos
            if (ModelState.IsValid)
            {
                await GuardarArchivos(comercializacionBD.cotizacion.documentosCompromiso.ToList());
                await BorrarArchivosAntiguos(comercializacionBD.cotizacion.documentosCompromiso.ToList(), keysDocumentosAntiguo);
            }
            else
            {
                var errors = ModelState.Select(x => x.Value.Errors)
                                       .Where(y => y.Count > 0)
                                       .ToList();
            }
            // validar la comercializacion
            bool updateNotas = false;
            if (ModelState.IsValid)
            {
                updateNotas = comercializacion.fechaTermino.Ticks != comercializacionBD.fechaTermino.Ticks;


                // datos comercializacion
                comercializacionBD.fechaInicio = comercializacion.fechaInicio;
                comercializacionBD.fechaTermino = comercializacion.fechaTermino;
                comercializacionBD.senceNet = comercializacion.senceNet;
                comercializacionBD.clientDownload = comercializacion.clientDownload;
                comercializacionBD.R23ConDescriptorContenidos = comercializacion.R23ConDescriptorContenidos;
                comercializacionBD.observacion = comercializacion.observacion;
                comercializacionBD.usuarioUltimaEdicion = db.AspNetUsers.Find(User.Identity.GetUserId());
                comercializacionBD.fechaUltimaEdicion = DateTime.Now;
                //Actualizar datos del cliente
                comercializacionBD.cotizacion.idCliente = comercializacion.cotizacion.idCliente;
                comercializacionBD.cotizacion.nombreDiploma = Request["cotizacion.nombreDiploma"];
                var cliente = db.Cliente.Find(comercializacion.cotizacion.idCliente);
                comercializacionBD.cotizacion.nombreEmpresa = cliente.nombreEmpresa;
                comercializacionBD.cotizacion.razonSocial = cliente.razonSocial;
                comercializacionBD.cotizacion.giro = Request["cotizacion.giro"];
                comercializacionBD.cotizacion.telefonoCorporativo = cliente.telefonoCorporativo;
                comercializacionBD.cotizacion.direccion = cliente.direccion;
                comercializacionBD.cotizacion.contactoEncargadoPago = comercializacion.cotizacion.contactoEncargadoPago;
                comercializacionBD.cotizacion.contacto = comercializacion.cotizacion.contacto;

                if (Request["faena.idFaena"] != "")
                {
                    comercializacionBD.faena = db.Faena.Find(Convert.ToInt32(Request["faena.idFaena"]));
                    db.Entry(comercializacionBD.faena).State = EntityState.Modified;
                }
                if (Request["tieneSence"] != "")
                {
                    bool tieneSence = Request["tieneSence"] == "true,false";
                    comercializacionBD.cotizacion.tieneCodigoSence = tieneSence ? "on" : null;
                }
                comercializacionBD.cotizacion.contactoEncargadoPago = Convert.ToInt32(Request["cotizacion.contactoEncargadoPago"]);
                // guardar estado
                var borrador = false;
                if (comercializacionBD.comercializacionEstadoComercializacion.LastOrDefault().EstadoComercializacion == EstadoComercializacion.Borrador)
                {
                    borrador = true;
                    ComercializacionEstadoComercializacion comercializacionEstadoComercializacion = new ComercializacionEstadoComercializacion();
                    comercializacionEstadoComercializacion.EstadoComercializacion = EstadoComercializacion.En_Proceso;
                    comercializacionEstadoComercializacion.fechaCreacion = DateTime.Now;
                    comercializacionEstadoComercializacion.usuarioCreador = User.Identity.GetUserId();
                    comercializacionBD.comercializacionEstadoComercializacion.Add(comercializacionEstadoComercializacion);
                }
                var relatores = comercializacionBD.relatoresCursos;
                if (relatores.Count() == 1)
                {
                    foreach (var bloque in comercializacionBD.bloques)
                    {
                        bloque.relator = relatores.FirstOrDefault().relator;
                    }

                }
                // guardar la comercializacion
                comercializacion.idComercializacion = 0;
                db.Entry(comercializacionBD).State = EntityState.Modified;
                comercializacionBD.cotizacion.cliente.postVenta = false;
                db.Entry(comercializacionBD.cotizacion).State = EntityState.Modified;
                db.Entry(comercializacionBD.cotizacion.cliente).State = EntityState.Modified;

                db.SaveChanges();

                if (comercializacion.fechaInicio == DateTime.Today)
                {
                    foreach (var relator in comercializacionBD.relatoresCursos)
                    {
                        if (comercializacionBD.relatoresCursos.Count() > comercializacionBD.relatoresConfirmados.Count())
                        {
                            // notificacion relator confirmacion curso
                            //var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta de Confirmacion de Curso Relator").FirstOrDefault();
                            //if (notificacionConfig != null)
                            //{
                            //    notificacionConfig.CrearNotificacionUsuario(db, comercializacion.cotizacion.curso.nombreCurso, comercializacion.idComercializacion + "-" + relator.relator.idRelator, relator.relator.contacto.usuario.Id, relator.relator.contacto.usuario);
                            //}
                            EnviarMailConfirmacinCursoRelator(comercializacionBD, relator);
                        }
                    }
                }
                //var cotizacionDB = db.Cotizacion_R13.Find(comercializacionBD.cotizacion.idCotizacion_R13);
                //cotizacionDB.nombreDiploma = comercializacion.cotizacion.nombreDiploma;
                //db.Entry(cotizacionDB).State = EntityState.Modified;
                BorrarDocumentosCompromiso();
                if (borrador)
                {
                    if (comercializacionBD.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
                    {
                        // notificar curso con relator sin sence
                        if (comercializacionBD.relatoresCursos.Where(x => !x.validoSence).FirstOrDefault() != null
                            && comercializacionBD.cotizacion.codigoSence != null && comercializacionBD.cotizacion.codigoSence != "")
                        {
                            //var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta Instructor No vinculado a sence").FirstOrDefault();
                            //if (notificacionConfig != null)
                            //{
                            //    notificacionConfig.CrearNotificacion(db, comercializacionBD.cotizacion.codigoCotizacion, comercializacionBD.idComercializacion.ToString(), User.Identity.GetUserId());
                            //}
                        }
                    }
                }
                else
                {
                    // notificacion comercializacion modificada
                    //var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta Comercialización Modificada").FirstOrDefault();
                    //if (notificacionConfig != null)
                    //{
                    //    notificacionConfig.CrearNotificacion(db, comercializacionBD.cotizacion.codigoCotizacion, comercializacionBD.idComercializacion.ToString(), User.Identity.GetUserId());
                    //}
                }



                AgregarHistorialComercializacion(comercializacionBD.idComercializacion);
                if (updateNotas)
                {
                    Task.Factory.StartNew(() =>
                    {
                        var id = comercializacionBD.idComercializacion;
                        UpdateParticipantes(id);
                    });

                }
                return RedirectToAction("Index");
            }

            CargarDatosVista(comercializacionBD);


            return View(comercializacionBD);
        }
        private void UpdateParticipantes(int id)
        {
            InsecapContext dbUpdate = new InsecapContext();
            var comercializacion = dbUpdate.Comercializacion.Find(id);
            try
            {
                Moodle.AgregarParticipantesGrupoMoodle(comercializacion.participantes.Where(x => x.contacto.idUsuarioMoodle != null).Select(x => x.contacto).ToList(), comercializacion, dbUpdate.ParametrosMoodles.FirstOrDefault());

            }
            catch (Exception e)
            {
            }

        }

        // POST: Comercializacions/EditBorrador/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditBorrador([Bind(Include = "idComercializacion,tipoDocumentoOtro,senceNet,R23ConDescriptorContenidos,observacion,fechaInicio,fechaTermino,vigenciaCredenciales,bloque,ciudad")] Comercializacion comercializacion)
        {
            ModelState.Remove("fechaInicio");
            ModelState.Remove("fechaTermino");
            ModelState.Remove("ciudad.idCiudad");
            ModelState.Remove("vigenciaCredenciales");
            // obtener la comercializacion de la base de datos
            Comercializacion comercializacionBD = db.Comercializacion.Find(comercializacion.idComercializacion);
            comercializacionBD.cotizacion.costo = db.Costo.Where(c => c.idCotizacion == comercializacionBD.cotizacion.idCotizacion_R13).ToList();
            List<string> keysDocumentosAntiguo = new List<string>();
            foreach (var item in comercializacionBD.cotizacion.documentosCompromiso)
            {
                keysDocumentosAntiguo.Add(item.documento.key);
            }
            // guardar datos
            comercializacionBD.ciudad = db.Ciudad.Find(comercializacion.ciudad.idCiudad);
            if (comercializacionBD.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacionBD.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacionBD.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                var relatoresCursos = comercializacionBD.relatoresCursos;
                comercializacionBD.relatoresCursos = GuardarRelatores();
                //EliminarPagos(comercializacionBD.pagos.ToList());
                //comercializacionBD.pagos = GuardarPagos();
            }
            comercializacionBD.cotizacion.documentosCompromiso = GuardarPagos(comercializacionBD.cotizacion.documentosCompromiso.ToList());
            comercializacionBD.tipoDocumentoOtro = comercializacion.tipoDocumentoOtro;
            // datos cotizacion
            if (Request["cotizacion.contacto"] != "" && Request["cotizacion.contacto"] != null)
            {
                comercializacion.cotizacion.contacto = int.Parse(Request["cotizacion.contacto"]);
            }
            if (Request["cotizacion.contactoEncargadoPago"] != "" && Request["cotizacion.contactoEncargadoPago"] != null)
            {
                comercializacion.cotizacion.contactoEncargadoPago = int.Parse(Request["cotizacion.contactoEncargadoPago"]);
            }
            comercializacionBD.cotizacion.cantidadParticipante = 0;
            if (comercializacionBD.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacionBD.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacionBD.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                if (Request["cotizacion.cantidadParticipante"] != "")
                {
                    comercializacionBD.cotizacion.cantidadParticipante = int.Parse(Request["cotizacion.cantidadParticipante"]);
                }
                if (Request["cotizacion.lugarRealizacion"] != "")
                {
                    comercializacionBD.cotizacion.lugarRealizacion = Request["cotizacion.lugarRealizacion"];
                }
                if (Request["cotizacion.procesoPractico"].Contains("true"))
                {
                    comercializacionBD.cotizacion.procesoPractico = true;
                }
                else
                {
                    comercializacionBD.cotizacion.procesoPractico = false;
                }
            }
            comercializacion.cotizacion.sucursal = db.Sucursal.Find(comercializacion.cotizacion.sucursal.idSucursal);
            var docsValidos = true;
            foreach (var item in comercializacionBD.cotizacion.documentosCompromiso)
            {
                var docCompromisoValido = DocumentoCompromisoValido(item, null);
                var archivoValido = false;
                if (item.documento != null)
                {
                    if (item.documento.file == null)
                    {
                        if (keysDocumentosAntiguo.Contains(item.documento.key))
                        {
                            archivoValido = true;
                        }
                    }
                    else
                    {
                        archivoValido = ArchivoValido(item.documento.file, "false");
                    }
                }
                if (!docCompromisoValido || !archivoValido)
                {
                    docsValidos = false;
                }
            }
            if (!docsValidos)
            {
                CargarDatosVista(comercializacionBD);
                return View("Edit", comercializacionBD);
            }
            await GuardarArchivos(comercializacionBD.cotizacion.documentosCompromiso.ToList());
            await BorrarArchivosAntiguos(comercializacionBD.cotizacion.documentosCompromiso.ToList(), keysDocumentosAntiguo);
            // datos comercializacion
            comercializacionBD.fechaInicio = comercializacion.fechaInicio;
            comercializacionBD.fechaTermino = comercializacion.fechaTermino;
            comercializacionBD.vigenciaCredenciales = comercializacion.vigenciaCredenciales;
            comercializacionBD.senceNet = comercializacion.senceNet;
            comercializacionBD.R23ConDescriptorContenidos = comercializacion.R23ConDescriptorContenidos;
            comercializacionBD.observacion = comercializacion.observacion;
            comercializacionBD.usuarioUltimaEdicion = db.AspNetUsers.Find(User.Identity.GetUserId());
            comercializacionBD.fechaUltimaEdicion = DateTime.Now;
            // guardar fechas borrador
            if (DateTime.Compare(comercializacionBD.fechaInicio, DateTime.MinValue) == 0)
            {
                if (comercializacionBD.cotizacion.fechaInicio != null)
                {
                    comercializacionBD.fechaInicio = comercializacionBD.cotizacion.fechaInicio.Value;
                }
                else
                {
                    ModelState.AddModelError("fechaInicio", "El campo Fecha de Inicio es obligatorio");
                    CargarDatosVista(comercializacionBD);
                    return View("Create", comercializacionBD);
                }
            }
            if (DateTime.Compare(comercializacionBD.fechaTermino, DateTime.MinValue) == 0)
            {
                if (comercializacionBD.cotizacion.fechaTermino != null)
                {
                    comercializacionBD.fechaTermino = comercializacionBD.cotizacion.fechaTermino.Value;
                }
                else
                {
                    ModelState.AddModelError("fechaTermino", "El campo Fecha de Termino es obligatorio");
                    CargarDatosVista(comercializacionBD);
                    return View("Create", comercializacionBD);
                }
            }
            //if (DateTime.Compare(comercializacion.vigenciaCredenciales, DateTime.MinValue) == 0)
            //{
            //    comercializacionBD.vigenciaCredenciales = DateTime.Now.AddYears(2);
            //}
            // guardar la comercializacion
            comercializacion.idComercializacion = 0;
            db.Entry(comercializacionBD).State = EntityState.Modified;
            db.SaveChanges();
            BorrarDocumentosCompromiso();
            return RedirectToAction("Index");
        }

        // POST: Comercializacions/Cancelado/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancelado([Bind(Include = "idComercializacion")] Comercializacion comercializacion)
        {
            // obtener la comercializacion de la base de datos
            Comercializacion comercializacionBD = db.Comercializacion.Find(comercializacion.idComercializacion);
            // cambiar estado a terminado
            ComercializacionEstadoComercializacion comercializacionEstadoComercializacion = new ComercializacionEstadoComercializacion();
            comercializacionEstadoComercializacion.EstadoComercializacion = EstadoComercializacion.Cancelada;
            comercializacionEstadoComercializacion.fechaCreacion = DateTime.Now;
            comercializacionEstadoComercializacion.usuarioCreador = User.Identity.GetUserId();
            comercializacionBD.comercializacionEstadoComercializacion.Add(comercializacionEstadoComercializacion);
            // guardar cambios a la comercializacion
            db.Entry(comercializacionBD).State = EntityState.Modified;
            db.SaveChanges();
            // notificacion comercializacion finalizada
            //var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta Comercialización Cancelada").FirstOrDefault();
            //if (notificacionConfig != null)
            //{
            //    notificacionConfig.CrearNotificacion(db, comercializacionBD.cotizacion.codigoCotizacion, comercializacionBD.idComercializacion.ToString(), User.Identity.GetUserId());
            //}
            return RedirectToAction("Index");
        }

        // POST: Comercializacions/TerminadoSence/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TerminadoSence([Bind(Include = "idComercializacion")] Comercializacion comercializacion)
        {
            // obtener la comercializacion de la base de datos
            Comercializacion comercializacionBD = db.Comercializacion.Find(comercializacion.idComercializacion);
            // validar cambio de estado
            var valido = true;
            // borrador
            if (comercializacionBD.comercializacionEstadoComercializacion.OrderByDescending(x => x.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Borrador)
            {
                ModelState.AddModelError("", "No se puede terminar la comercialización si está en estado Borrador.");
                valido = false;
            }
            // fecha
            if (DateTime.Compare(comercializacionBD.fechaInicio, DateTime.Now) >= 0)
            {
                ModelState.AddModelError("", "No se puede terminar si no ha pasado la fecha de inicio de la comercialización.");
                valido = false;
            }
            // oc sence
            if (comercializacionBD.cotizacion.documentosCompromiso.Where(x => x.tipoVenta.tipoPago == TipoPago.Sence).FirstOrDefault() == null
                 && comercializacionBD.cotizacion.documentosCompromiso.Where(x => x.tipoVenta.tipoPago == TipoPago.Otic).FirstOrDefault() == null)
            {
                ModelState.AddModelError("", "No se encontro la orden de compra de SENCE o OTIC para esta comercialización.");
                valido = false;
            }
            // r24
            UpdateR24(comercializacionBD);
            bool hasR24 = db.R24.Where(x => x.comercializacion.idComercializacion == comercializacionBD.idComercializacion).Any();
            if (!hasR24)
            {
                ModelState.AddModelError("", "No se encontro el R24 para esta comercialización.");
                valido = false;
            }
            if (!valido)
            {
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            // cambiar estado a terminado sence
            ComercializacionEstadoComercializacion comercializacionEstadoComercializacion = new ComercializacionEstadoComercializacion();
            comercializacionEstadoComercializacion.EstadoComercializacion = EstadoComercializacion.Terminada_SENCE;
            comercializacionEstadoComercializacion.fechaCreacion = DateTime.Now;
            comercializacionEstadoComercializacion.usuarioCreador = User.Identity.GetUserId();
            comercializacionBD.comercializacionEstadoComercializacion.Add(comercializacionEstadoComercializacion);
            // guardar cambios a la comercializacion
            comercializacionBD.clientDownload = true;
            db.Entry(comercializacionBD).State = EntityState.Modified;
            db.SaveChanges();
            // notificacion comercializacion finalizada
            //var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta Comercialización Terminada SENCE").FirstOrDefault();
            //if (notificacionConfig != null)
            //{
            //    notificacionConfig.CrearNotificacion(db, comercializacionBD.cotizacion.codigoCotizacion, comercializacionBD.idComercializacion.ToString(), User.Identity.GetUserId());
            //}
            CorreoComercializacionTerminada(comercializacionBD);
            AlertaSinOCCostoEmpresa(comercializacionBD);
            return RedirectToAction("Index");
        }

        // POST: Comercializacions/Terminado/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Terminado([Bind(Include = "idComercializacion")] Comercializacion comercializacion)
        {
            // obtener la comercializacion de la base de datos
            Comercializacion comercializacionBD = db.Comercializacion.Find(comercializacion.idComercializacion);
            // validar cambio de estado
            // validar cambio de estado
            var valido = true;
            // borrador
            if (comercializacionBD.comercializacionEstadoComercializacion.OrderByDescending(x => x.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Borrador)
            {
                ModelState.AddModelError("", "No se puede terminar la comercialización si está en estado Borrador.");
                valido = false;
            }
            // fecha
            if (DateTime.Compare(comercializacionBD.fechaInicio, DateTime.Now) >= 0)
            {
                ModelState.AddModelError("", "No se puede terminar si no ha pasado la fecha de inicio de la comercialización.");
                valido = false;
            }
            // oc
            if (comercializacionBD.cotizacion.documentosCompromiso.Where(x => x.tipoVenta.tipoPago == TipoPago.CostoEmpresa).FirstOrDefault() != null)
            {
                if (comercializacionBD.cotizacion.documentosCompromiso.Where(x => x.tipoVenta.tipoPago == TipoPago.CostoEmpresa).Where(x => x.tipoDocCompromiso.nombre.ToLower().Contains("oc") || x.tipoDocCompromiso.nombre.ToLower().Contains("transferencia") || x.tipoDocCompromiso.nombre.ToLower().Contains("contado")).FirstOrDefault() == null)
                {
                    ModelState.AddModelError("", "No se encontro la orden de compra para esta comercialización.");
                    valido = false;
                }
            }
            else
            {
                if (comercializacionBD.cotizacion.documentosCompromiso.Where(x => x.tipoVenta.tipoPago == TipoPago.Sence).FirstOrDefault() == null
                    && comercializacionBD.cotizacion.documentosCompromiso.Where(x => x.tipoVenta.tipoPago == TipoPago.Otic).FirstOrDefault() == null)
                {
                    ModelState.AddModelError("", "No se encontro la orden de compra de para esta comercialización.");
                    valido = false;
                }
            }
            // r24
            if (comercializacionBD.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacionBD.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacionBD.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                UpdateR24(comercializacionBD);
                bool hasR24 = db.R24.Where(x => x.comercializacion.idComercializacion == comercializacionBD.idComercializacion).Any();
                if (!hasR24)
                {
                    ModelState.AddModelError("", "No se encontro el R24 para esta comercialización.");
                    valido = false;
                }
            }
            if (!valido)
            {
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            // cambiar estado a terminado
            ComercializacionEstadoComercializacion comercializacionEstadoComercializacion = new ComercializacionEstadoComercializacion();
            comercializacionEstadoComercializacion.EstadoComercializacion = EstadoComercializacion.Terminada;
            comercializacionEstadoComercializacion.fechaCreacion = DateTime.Now;
            comercializacionEstadoComercializacion.usuarioCreador = User.Identity.GetUserId();
            comercializacionBD.comercializacionEstadoComercializacion.Add(comercializacionEstadoComercializacion);
            // si es primera venta se debe realizar alerta encuesta de satisfacion al cliente


            var esPrimeraVenta = db.ComercializacionEstadoComercializacion
             .Where(x => x.comercializacion.softDelete == false)
             .Where(x => x.comercializacion.cotizacion.cliente.idCliente == comercializacionBD.cotizacion.cliente.idCliente)
             .Where(x => x.EstadoComercializacion == EstadoComercializacion.Terminada)
             .FirstOrDefault();
            if (esPrimeraVenta == null)
            {
                if (comercializacionBD.cotizacion.tipoCurso == "Curso" || comercializacionBD.cotizacion.tipoCurso == "Recertificación")
                {
                    if (comercializacionBD.cotizacion.curso.tipoEjecucion == TipoEjecucion.Presencial
                   || comercializacionBD.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion)
                    {
                        comercializacionBD.cotizacion.cliente.encuestaSatisfaccion = true;
                        db.Entry(comercializacionBD.cotizacion.cliente).State = EntityState.Modified;
                    }
                    else
                    {
                        comercializacionBD.cotizacion.cliente.encuestaSatisfaccionElerning = true;
                        db.Entry(comercializacionBD.cotizacion.cliente).State = EntityState.Modified;
                    }
                    //var notificacionConfigEncuesta = db.NotificacionConfig.Where(x => x.nombre == "Alerta de Encuesta de Satisfacción Cliente").FirstOrDefault();
                    //if (notificacionConfigEncuesta != null)
                    //{
                    //    notificacionConfigEncuesta.CrearNotificacion(db, comercializacionBD.cotizacion.cliente.nombreEmpresa, comercializacionBD.cotizacion.cliente.idCliente.ToString(), "");
                    //}
                    //var notificacionConfigEncuestaCliente = db.NotificacionConfig.Where(x => x.nombre == "Alerta de Encuesta de Satisfacción para Cliente").FirstOrDefault();
                    //if (notificacionConfigEncuestaCliente != null)
                    //{
                    //    var idUsuario = User.Identity.GetUserId();
                    //    notificacionConfigEncuestaCliente.CrearNotificacionUsuario(db, "", "", idUsuario, db.AspNetUsers.Find(idUsuario));
                    //}
                }

            }
            // alerta resultado r25 < 95%
            if (comercializacionBD.r19.Count() != 0)
            {
                if (promedioFinalR19(comercializacionBD.r19.ToList(), comercializacionBD) < 95)
                {
                    //var notificacionConfigResultadoR25 = db.NotificacionConfig.Where(x => x.nombre == "Alerta 95% R25 Resultados").FirstOrDefault();
                    //if (notificacionConfigResultadoR25 != null)
                    //{
                    //    notificacionConfigResultadoR25.CrearNotificacion(db, comercializacionBD.cotizacion.codigoCotizacion, comercializacionBD.idComercializacion.ToString(), User.Identity.GetUserId());
                    //}
                }
                // alerta si no todos los participantes respondieron r19
                if (comercializacionBD.r19.Count() < comercializacionBD.participantes.Count())
                {
                    //var notificacionConfigRespondieronR19 = db.NotificacionConfig.Where(x => x.nombre == "Alerta Participante no Respondio R19").FirstOrDefault();
                    //if (notificacionConfigRespondieronR19 != null)
                    //{
                    //    notificacionConfigRespondieronR19.CrearNotificacion(db, comercializacionBD.cotizacion.codigoCotizacion, comercializacionBD.idComercializacion.ToString(), User.Identity.GetUserId());
                    //}
                }
            }
            // guardar cambios a la comercializacion
            comercializacionBD.clientDownload = true;
            db.Entry(comercializacionBD).State = EntityState.Modified;
            db.SaveChanges();
            // notificacion comercializacion finalizada
            //var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta Comercialización Terminada").FirstOrDefault();
            //if (notificacionConfig != null)
            //{
            //    notificacionConfig.CrearNotificacion(db, comercializacionBD.cotizacion.codigoCotizacion, comercializacionBD.idComercializacion.ToString(), User.Identity.GetUserId());
            //}
            if (DateTime.Compare(comercializacionBD.fechaInicio, new DateTime(2021, 04, 01)) > 0)
                CorreoComercializacionTerminada(comercializacionBD);
            AlertaSinOCCostoEmpresa(comercializacionBD);
            return RedirectToAction("Index");
        }

        private void CorreoComercializacionTerminada(Comercializacion comercializacion)
        {
            MailPriority priority = MailPriority.Normal;
            string subjectSence = "";
            if (comercializacion.cotizacion.codigoSence != null && comercializacion.cotizacion.codigoSence != "" && comercializacion.cotizacion.tieneCodigoSence != "on")
            {
                subjectSence = "***";
                priority = MailPriority.Normal;
            }
            // obtener usuarios que deben recibir el correo
            var notificacion = db.NotificacionConfig.Where(x => x.nombre == "Alerta Comercialización Terminada").FirstOrDefault();
            var roles = notificacion.roles;
            foreach (var rol in roles)
            {
                foreach (var usuario in rol.AspNetUsers)
                {

                    var receiverEmail = new MailAddress(usuario.Email, usuario.nombreCompleto);

                    var subject = subjectSence + "Comercialización Terminada ( {0} )";
                    var textoEmail = "Estimado/a {1},{0}{0} Se ha terminado la comercialización {2}, ingrese {3} para ver los detalles.{0}{0} Codigo: {4}{0} Curso: {5}{0} Fecha de Inicio: {6}{0} Fecha de Termino: {7}{0} Cliente: {8}{0}{0}Atte.{0}{0}Insecap";


                    subject = string.Format(subject, comercializacion.cotizacion.codigoCotizacion);
                    var body = string.Format(textoEmail,
                        //Environment.NewLine,
                        "<br>",
                        usuario.nombreCompleto, comercializacion.cotizacion.codigoCotizacion,
                        string.Format("<a href=\" {0}/Comercializacions/Details/{1} \"> aquí  </a> ",
                            domain, comercializacion.idComercializacion),
                        comercializacion.cotizacion.codigoCotizacion,
                        comercializacion.cotizacion.curso == null ? comercializacion.cotizacion.tipoCurso : comercializacion.cotizacion.curso.nombreCurso,
                        String.Format("{0:d}", comercializacion.fechaInicio),
                        String.Format("{0:d}", comercializacion.fechaTermino),
                        comercializacion.cotizacion.cliente.nombreEmpresa
                        );
                    Utils.Utils.SendMail(receiverEmail, subject, body, null, priority);

                }
            }

        }

        private double promedioFinalR19(List<R19> listaR19, Comercializacion comercializacion)
        {
            var total = 0.0;
            var cont = 0;
            foreach (var formulario in listaR19.FirstOrDefault().encuesta.seccionEncuesta)
            {
                foreach (var contacto in comercializacion.participantes)
                {
                    foreach (var pregunta in formulario.formulario.preguntasFormularios)
                    {
                        if (pregunta.tipo == TipoPregunta.Alternativa)
                        {
                            foreach (var r19 in listaR19)
                            {
                                if (r19.encuesta.respuestas != null && r19.encuesta.respuestas.Count() != 0)
                                {
                                    var respuesta = r19.encuesta.respuestas
                                        .Where(r => r.pregunta.idPreguntasFormulario == pregunta.idPreguntasFormulario)
                                        .Where(r => r.contacto.idContacto == contacto.contacto.idContacto)
                                        .FirstOrDefault();
                                    if (respuesta != null)
                                    {
                                        total += double.Parse(respuesta.respuesta);
                                        cont++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            var promedioFinal = 0.0;
            if (cont != 0)
            {
                promedioFinal = (total / cont);
            }
            return promedioFinal;
        }

        private void AlertaSinOCCostoEmpresa(Comercializacion comercializacion)
        {
            var docsCompromisoCostoEmpresa = comercializacion.cotizacion.documentosCompromiso
                .Where(x => x.tipoVenta.tipoPago == SGC.Models.TipoPago.CostoEmpresa)
                .Where(x => x.softDelete == false)
                .ToList();
            if (docsCompromisoCostoEmpresa.Count() > 0)
            {
                var cont = 0;
                foreach (var docCompromiso in docsCompromisoCostoEmpresa)
                {
                    if (docCompromiso.tipoDocCompromiso.nombre.ToLower().Contains("oc"))
                    {
                        cont++;
                    }
                }
                if (cont == 0)
                {
                    // notificacion comercializacion sin OC costo empresa
                    //var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta Falta Orden de Compra").FirstOrDefault();
                    //if (notificacionConfig != null)
                    //{
                    //    notificacionConfig.CrearNotificacion(db, comercializacion.cotizacion.codigoCotizacion, comercializacion.idComercializacion.ToString(), User.Identity.GetUserId());
                    //}
                }
            }
            //var docsCompromisoCostoEmpresa = comercializacion.cotizacion.documentosCompromiso
            //    .Where(x => x.tipoVenta.tipoPago == TipoPago.CostoEmpresa)
            //    .Where(x => x.softDelete == false)
            //    .ToList();
            //if(comercializacion.cotizacion.documentosCompromiso
            //    .Where(x => x.softDelete == false)
            //    .Where(x => x.tipoVenta.tipoPago == TipoPago.Sence || x.tipoVenta.tipoPago == TipoPago.Otic)
            //    .ToList().Count() == 0)
            //{
            //    var cont = 0;
            //    foreach (var docCompromiso in docsCompromisoCostoEmpresa)
            //    {
            //        if (docCompromiso.tipoDocCompromiso.nombre.ToLower().Contains("oc"))
            //        {
            //            cont++;
            //        }
            //    }
            //    if (cont == 0)
            //    {
            //        // notificacion comercializacion sin OC costo empresa
            //        var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta Falta Orden de Compra").FirstOrDefault();
            //        if (notificacionConfig != null)
            //        {
            //            notificacionConfig.CrearNotificacion(db, comercializacion.cotizacion.codigoCotizacion, comercializacion.idComercializacion.ToString(), User.Identity.GetUserId());
            //        }
            //    }
            //}
        }

        //[CustomAuthorize(new string[] { "/Comercializacions/" })]
        //// GET: Comercializacions/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Comercializacion comercializacion = db.Comercializacion.Find(id);
        //    if (comercializacion == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    if (comercializacion.comercializacionEstadoComercializacion.LastOrDefault().EstadoComercializacion == SGC.Models.EstadoComercializacion.Terminada
        //        && comercializacion.comercializacionEstadoComercializacion.LastOrDefault().EstadoComercializacion == SGC.Models.EstadoComercializacion.Cancelada
        //        && comercializacion.comercializacionEstadoComercializacion.LastOrDefault().EstadoComercializacion == SGC.Models.EstadoComercializacion.Deshabilitada)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    if (db.R11.Where(r => r.idCurso == comercializacion.cotizacion.curso.idCurso).FirstOrDefault() != null)
        //    {
        //        ViewBag.sence = db.R11.Where(r => r.idCurso == comercializacion.cotizacion.curso.idCurso).FirstOrDefault().codigoSence;
        //    }
        //    else
        //    {
        //        ViewBag.sence = null;
        //    }
        //    return View(comercializacion);
        //}

        // POST: Comercializacions/Delete/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Comercializacion comercializacion = db.Comercializacion.Find(id);
            comercializacion.softDelete = true;

            db.Asistencias.RemoveRange(db.Asistencias.Where(x => x.bloque.comercializacion.idComercializacion == comercializacion.idComercializacion));
            db.Bloque.RemoveRange(db.Bloque.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion));


            if (comercializacion.idGrupoMoodle != null)
            {
                Moodle.EliminarGrupoMoodle(comercializacion, db.ParametrosMoodles.FirstOrDefault());
            }

            db.Entry(comercializacion).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Comercializacions/Descargar/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public async Task<ActionResult> Descargar(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var storage = db.Storages.Find(id);
            if (storage == null)
            {
                return HttpNotFound();
            }
            return await Files.BajarArchivoDescargarAsync(storage);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult EliminarGrupoMoodle(int? id)
        {
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                errors.Add("Comercializacion no encontrada");
                return RedirectToAction("Index");
            }
            if (comercializacion.participantes.Count() > 0)
            {
                errors.Add("Debe eliminar los alumnos antes de poder eliminar el grupo");
                return RedirectToAction("Index");
            }
            Moodle.EliminarGrupoMoodle(comercializacion, db.ParametrosMoodles.FirstOrDefault());
            comercializacion.idGrupoMoodle = null;
            db.Entry(comercializacion).State = EntityState.Modified;
            db.SaveChanges();
            errors.Add("Grupo eliminado correctamente");
            return RedirectToAction("Index");
        }
        // GET: Comercializacions/AgregarGrupoMoodle/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult AgregarGrupoMoodle(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comercializacion comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            if (comercializacion.cotizacion.curso.idCursoMoodle == null)
            {
                ModelState.AddModelError("", "El curso " + comercializacion.cotizacion.curso.codigoCurso + " no tiene ID de Moodle");

            }

            //No está dejando crear cursos
            //if (comercializacion.senceNet == null)
            //{
            //    ModelState.AddModelError("", "No existe el SENCE NET");
            //    //Tratar de no repetirlo
            //    ViewBag.templatesR50 = GetTemplatesR50();
            //    return View("Index", db.Comercializacion
            //        .Where(x => x.softDelete == false)
            //        .ToList());
            //}
            //Validar Con Sence o sin sence
            if (ModelState.IsValid)
            {

                string tempCodigoCotizacion = comercializacion.cotizacion.codigoCotizacion;
                String codigoSence = "";

                comercializacion.cotizacion.codigoCotizacion = comercializacion.cotizacion.codigoCotizacion.Split('-')[0];
                comercializacion.idGrupoMoodle = Moodle.CrearGrupoMoodle(comercializacion, db.ParametrosMoodles.FirstOrDefault());


                var number = 0;
                if (comercializacion.idGrupoMoodle.Contains("Se produjo un error") && !Int32.TryParse(comercializacion.idGrupoMoodle, out number))

                {
                    ModelState.AddModelError("", "No fue posible crear el grupo " + comercializacion.cotizacion.codigoCotizacion);
                    comercializacion.idGrupoMoodle = null;
                }
                else
                {
                    ModelState.AddModelError("", "Grupo " + comercializacion.cotizacion.codigoCotizacion + " Creado correctamente");

                }


                try
                {
                    if (comercializacion.cotizacion.curso.tipoEjecucion != TipoEjecucion.Presencial && comercializacion.idGrupoMoodle != null && comercializacion.cotizacion.tieneCodigoSence != "on" && comercializacion.cotizacion.codigoSence != null)
                    {

                        comercializacion.cotizacion.codigoCotizacion = "SENCE-" + comercializacion.senceNet;
                        codigoSence = Moodle.CrearGrupoMoodle(comercializacion, db.ParametrosMoodles.FirstOrDefault());
                        if (!codigoSence.Contains("Se produjo un error"))
                        {
                            ModelState.AddModelError("", "Grupo " + comercializacion.cotizacion.codigoCotizacion + " creado");
                        }
                        else
                        {
                            ModelState.AddModelError("", "No fue posible crear el grupo " + comercializacion.cotizacion.codigoCotizacion);
                            //comercializacion.idGrupoMoodle = null;
                        }

                    }

                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "No fue posible crear el grupo " + comercializacion.cotizacion.codigoCotizacion + " o ya existe");
                    //ModelState.AddModelError("", e.Message);
                }

                try
                {
                    //Crear grupo moodle
                    if (comercializacion.idGrupoMoodle != null && !comercializacion.idGrupoMoodle.Contains("Se produjo un error"))
                    {
                        comercializacion.cotizacion.codigoCotizacion = "nosence";
                        String codigoNoSence = Moodle.CrearGrupoMoodle(comercializacion, db.ParametrosMoodles.FirstOrDefault());
                        if (!codigoNoSence.Contains("Se produjo un error"))
                        {
                            ModelState.AddModelError("", "Grupo nosence creado");
                        }
                        else
                        {
                            ModelState.AddModelError("", "No fue posible crear el grupo " + comercializacion.cotizacion.codigoCotizacion);
                            //comercializacion.idGrupoMoodle = null;
                        }
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "No fue posible crear el grupo " + comercializacion.cotizacion.codigoCotizacion + " o ya existe");
                    ModelState.AddModelError("", e.Message);
                }

                comercializacion.cotizacion.codigoCotizacion = tempCodigoCotizacion;
                db.Entry(comercializacion).State = EntityState.Modified;
                db.SaveChanges();
                errors.Add("Grupo agregado correctamente");
            }


            ViewBag.templatesR50 = GetTemplatesR50();
            return RedirectToAction("Index");
        }

        // GET: Comercializacions/ActualizarEvaluaciones/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult ActualizarEvaluaciones(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comercializacion comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            foreach (var evaluacion in comercializacion.cotizacion.curso.evaluaciones.Where(e => e.softDelete == false).ToList())
            {
                if (!comercializacion.evaluaciones.Contains(evaluacion))
                {
                    comercializacion.evaluaciones.Add(evaluacion);
                }
            }
            for (int i = 0; i < comercializacion.evaluaciones.Count(); i++)
            {
                if (!comercializacion.cotizacion.curso.evaluaciones.Where(e => e.softDelete == false).ToList().Contains(comercializacion.evaluaciones.ElementAt(i)))
                {
                    comercializacion.evaluaciones.Remove(comercializacion.evaluaciones.ElementAt(i));
                    i--;
                }
            }
            db.Entry(comercializacion).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Edit", new { id = comercializacion.idComercializacion });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public SelectList GetTiposDocCompromisoCliente(int idCliente)
        {
            //SelectList tiposDocCompromiso = new SelectList(db.TipoDocCompromiso.Select(c => new SelectListItem
            //{
            //    Text = c.nombre,
            //    Value = c.idTipoDocCompromiso.ToString()
            //}).ToList(), "Value", "Text");
            return new SelectList(db.ClienteTipoDocumentosPago
                .Where(t => t.idCliente == idCliente)
                .Select(c => new SelectListItem
                {
                    Text = c.tipoDocumentosPago.nombre,
                    Value = c.tipoDocumentosPago.idTipoDocumentosPago.ToString()
                }).ToList(), "Value", "Text");
        }

        public SelectList GetRelatoresCurso(Comercializacion comercializacion)
        {
            List<RelatorCurso> relatores = db.RelatorCurso
                .Where(r => r.softDelete == false)
                .Where(r => r.idCurso == comercializacion.cotizacion.curso.idCurso)
                .ToList();
            return new SelectList(relatores
                .Select(r => new SelectListItem
                {
                    Text = "[" + r.relator.contacto.run + "]" + " " + r.relator.contacto.nombres + " " + r.relator.contacto.apellidoPaterno + " " + r.relator.contacto.apellidoMaterno,
                    Value = r.idRelator.ToString() + "-" + r.idCurso.ToString()
                }).ToList(), "Value", "Text");
        }

        public SelectList GetRelatoresCursoSence(Comercializacion comercializacion, bool sence)
        {
            List<RelatorCurso> relatores = db.RelatorCurso
                .Where(r => r.softDelete == false)
                .Where(r => r.idCurso == comercializacion.cotizacion.curso.idCurso)
                .Where(r => r.validoSence == sence)
                .ToList();
            return new SelectList(relatores
                .Select(r => new SelectListItem
                {
                    Text = "[" + r.relator.contacto.run + "]" + " " + r.relator.contacto.nombres + " " + r.relator.contacto.apellidoPaterno + " " + r.relator.contacto.apellidoMaterno,
                    Value = r.idRelator.ToString() + "-" + r.idCurso.ToString()
                }).ToList(), "Value", "Text");
        }

        //public SelectList GetSalas(Comercializacion comercializacion)
        //{
        //    List<Sala> salas = db.Sala.Where(x => x.softDelete == false).ToList();
        //    return new SelectList(salas.Select(c => new SelectListItem
        //    {
        //        Text = c.nombre,
        //        Value = c.idSala.ToString()
        //    }).ToList(), "Value", "Text");
        //}

        //public SelectList GetLugaresAlmuerzo(Comercializacion comercializacion)
        //{
        //    List<LugarAlmuerzo> lugaresAlmuerzo = db.LugarAlmuerzo.Where(x => x.softDelete == false).ToList();
        //    return new SelectList(lugaresAlmuerzo.Select(c => new SelectListItem
        //    {
        //        Text = c.nombre,
        //        Value = c.idLugarAlmuerzo.ToString()
        //    }).ToList(), "Value", "Text");
        //}

        public SelectList GetOtics()
        {
            return new SelectList(db.Otic.Where(x => x.softDelete == false).Select(c => new SelectListItem
            {
                Text = c.nombre,
                Value = c.idOtic.ToString()
            }).ToList(), "Value", "Text");
        }

        public SelectList GetCiudades()
        {
            return new SelectList(db.Ciudad.Select(c => new SelectListItem
            {
                Text = c.nombreCiudad,
                Value = c.idCiudad.ToString()
            }).ToList(), "Value", "Text");
        }

        public List<RelatorCurso> GetInfoRelatoresCurso(int idCurso)
        {
            return db.RelatorCurso.Where(r => r.softDelete == false).Where(r => r.idCurso == idCurso).ToList();
        }

        //public String[] GetRelatoresSeleccionados(Comercializacion comercializacion)
        //{
        //    String[] relatoresSeleccionados = comercializacion.relatoresCursos.Select(g => g.idRelator.ToString()).ToArray();
        //    for (int i = 0; i < relatoresSeleccionados.Count(); i++)
        //    {
        //        relatoresSeleccionados[i] = relatoresSeleccionados[i] + "-" + comercializacion.cotizacion.idCurso;
        //    }
        //    return relatoresSeleccionados;
        //}

        public SelectList GetTemplatesR50()
        {
            return new SelectList(db.Template.Where(x => x.nombre.Contains("r50"))
                .Select(c => new SelectListItem
                {
                    Text = c.nombre,
                    Value = c.nombre
                }).ToList(), "Value", "Text");
        }

        // POST: Comercializacions/Descuento
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Descuento(int idComercializacion, int descuento)
        {
            var comercializacion = db.Comercializacion.Find(idComercializacion);
            comercializacion.descuento = descuento;
            comercializacion.usuarioCreadorDescuento = db.AspNetUsers.Find(User.Identity.GetUserId());
            comercializacion.fechaDescuento = DateTime.Now;
            db.Entry(comercializacion).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Edit", new { id = idComercializacion });
        }

        // GET: Comercializacions/CargarObservaciones/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult CargarObservaciones(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View("Observaciones", db.Comercializacion.Find(id));
        }

        // POST: Comercializacions/EnviarObservaciones
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        public ActionResult EnviarObservacion(int idComercializacion, string textoObservacion)
        {
            var comercializacion = db.Comercializacion.Find(idComercializacion);
            var observacion = new Observacion();
            observacion.fechaCreacion = DateTime.Now;
            observacion.usuarioCreador = db.AspNetUsers.Find(User.Identity.GetUserId());
            observacion.observacion = textoObservacion;
            comercializacion.observaciones.Add(observacion);
            db.Entry(comercializacion).State = EntityState.Modified;
            db.SaveChanges();
            return View("Observaciones", comercializacion);
        }

        // GET: Comercializacions/ComentariosR50/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult ComentariosR50(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.idRelator = 0;
            return View(db.Comercializacion.Find(id));
        }

        // GET: Comercializacions/ComentariosR50Relator/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/Perfil/" })]
        public ActionResult ComentariosR50Relator(int? id, int? id2)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (id2 == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.idRelator = id2;
            return View("ComentariosR50", db.Comercializacion.Find(id));
        }

        // POST: Comercializacions/ComentariosR50
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/", "/Relator/Perfil/" })]
        [HttpPost]
        public ActionResult ComentariosR50(int idComercializacion, string comentarioInstructor, string comentarioOtec, int idRelator)
        {
            var comercializacion = db.Comercializacion.Find(idComercializacion);
            comercializacion.comentarioInstructor = comentarioInstructor;
            comercializacion.comentarioOtec = comentarioOtec;
            db.Entry(comercializacion).State = EntityState.Modified;
            db.SaveChanges();
            if (idRelator != 0)
            {
                return RedirectToAction("CursosRealizar", "Relator", new { id = idRelator });
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        // GET: Comercializacions/IngresarR23/
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public async Task<ActionResult> IngresarR23(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }

            Files.borrarArchivosLocales();
            List<R23> r23s = db.R23.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion).ToList();
            ViewBag.r23s = r23s;

            foreach (R23 r23 in r23s)
            {

                await Files.BajarArchivoADirectorioLocalAsync(r23.file);
            }

            return View(comercializacion);
        }
        // GET: Comercializacions/IngresarR24/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public async Task<ActionResult> IngresarR24(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            UpdateR24(comercializacion);
            Files.borrarArchivosLocales();
            List<R24> r24s = db.R24.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion).ToList();
            ViewBag.r24s = r24s;

            foreach (R24 r24 in r24s)
            {
                await Files.BajarArchivoADirectorioLocalAsync(r24.file);
            }

            return View(comercializacion);
        }
        // GET: Comercializacions/IngresarR24/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public async Task<ActionResult> IngresarCredenciales(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }

            Files.borrarArchivosLocales();
            List<CredencialesFile> credencialesFile = db.CredencialesFile.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion).ToList();
            ViewBag.credencialesFile = credencialesFile;

            foreach (CredencialesFile credenciales in credencialesFile)
            {
                await Files.BajarArchivoADirectorioLocalAsync(credenciales.file);
            }

            return View(comercializacion);
        }
        public void UpdateR24(Comercializacion comercializacion)
        {
            if (comercializacion.r24 != null)
            {

                db.R24.Add(new R24
                {
                    dateUpload = DateTime.Now,
                    comercializacion = comercializacion,
                    description = "Actualizado por el sistema",
                    file = comercializacion.r24,
                    userUpload = db.AspNetUsers.Find(User.Identity.GetUserId())
                });
                comercializacion.r24 = null;
                db.Entry(comercializacion).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        // POST: Comercializacions/IngresarR23
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public async Task<ActionResult> DeleteR23(int idComercializacion, int idStorage)
        {

            if (idComercializacion == null || idStorage == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var comercializacion = db.Comercializacion.Find(idComercializacion);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            //Eliminar los viejos
            List<R23> r23s = db.R23.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion && x.file.idStorage == idStorage).ToList();
            foreach (R23 oldR23 in r23s.ToList())
            {
                await Files.BorrarArchivoAsync(oldR23.file);
                ModelState.AddModelError("", "Se ha eliminado correctamente el fichero " + oldR23.file.nombreArchivo);
                db.Storages.Remove(oldR23.file);
                db.R23.Remove(oldR23);
                db.SaveChanges();

            }


            List<R23> r23sView = db.R23.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion).ToList();
            ViewBag.r23s = r23sView;
            Files.borrarArchivosLocales();
            foreach (R23 r23 in r23sView)
            {
                await Files.BajarArchivoADirectorioLocalAsync(r23.file);
            }
            return View("IngresarR23", comercializacion);
        }
        // POST: Comercializacions/IngresarR23
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public async Task<ActionResult> DeleteCredenciales(int idComercializacion, int idStorage)
        {

            if (idComercializacion == null || idStorage == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var comercializacion = db.Comercializacion.Find(idComercializacion);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            //Eliminar los viejos
            List<CredencialesFile> credencialesFiles = db.CredencialesFile.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion && x.file.idStorage == idStorage).ToList();
            foreach (CredencialesFile old in credencialesFiles.ToList())
            {
                await Files.BorrarArchivoAsync(old.file);
                ModelState.AddModelError("", "Se ha eliminado correctamente el fichero " + old.file.nombreArchivo);
                db.Storages.Remove(old.file);
                db.CredencialesFile.Remove(old);
                db.SaveChanges();

            }


            List<CredencialesFile> credencialesFileView = db.CredencialesFile.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion).ToList();
            ViewBag.CredencialesFile = credencialesFileView;
            Files.borrarArchivosLocales();
            foreach (CredencialesFile credencialesFile in credencialesFileView)
            {
                await Files.BajarArchivoADirectorioLocalAsync(credencialesFile.file);
            }
            return View("IngresarCredenciales", comercializacion);
        }
        // POST: Comercializacions/IngresarR24
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public async Task<ActionResult> DeleteR24(int idComercializacion, int idStorage)
        {

            if (idComercializacion == null || idStorage == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var comercializacion = db.Comercializacion.Find(idComercializacion);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            //Eliminar los viejos
            List<R24> r24s = db.R24.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion && x.file.idStorage == idStorage).ToList();
            foreach (R24 oldR24 in r24s.ToList())
            {
                await Files.BorrarArchivoAsync(oldR24.file);
                ModelState.AddModelError("", "Se ha eliminado correctamente el fichero " + oldR24.file.nombreArchivo);
                db.Storages.Remove(oldR24.file);
                db.R24.Remove(oldR24);
                db.SaveChanges();

            }


            List<R24> r24sView = db.R24.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion).ToList();
            ViewBag.r24s = r24sView;
            Files.borrarArchivosLocales();
            foreach (R24 r24 in r24sView)
            {
                await Files.BajarArchivoADirectorioLocalAsync(r24.file);
            }
            return View("IngresarR24", comercializacion);
        }

        // POST: Comercializacions/IngresarR23
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        public async Task<ActionResult> IngresarR23(int idComercializacion, List<ViewModelR23> r23Model)
        {
            var comercializacion = db.Comercializacion.Find(idComercializacion);
            bool done = true;
            List<R23> r23s = db.R23.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion).ToList();
            r23Model = r23Model.Where(x => x != null && x.files != null).ToList();

            // verificar que se selecciono un archivo
            if (r23Model != null && r23Model.Count() <= 0)
            {
                ModelState.AddModelError("", "Se debe seleccionar un archivo.");
                done = false;
            }
            else if (r23s.Count() >= 5)
            {
                ModelState.AddModelError("", "Solo puede tener 5 archivos como máximo");
                done = false;
            }
            else
            {
                // validar extenciones y tamaño maximo del archivo

                foreach (ViewModelR23 item in r23Model)
                {
                    var archivoValido = Files.ArchivoValido(item.files, new[] { ".pdf" }, 10 * 1024);
                    if (archivoValido != "")
                    {
                        ModelState.AddModelError("", archivoValido + " ( " + item.files.FileName + " )");
                        done = false;
                    }

                }

            }


            if (done)
            {



                //agregar los nuevos ficheros
                foreach (ViewModelR23 item in r23Model)
                {
                    Storage r23 = await Files.CrearArchivoAsync(item.files, "Comercializacion/r24/");
                    if (r23 == null)
                    {
                        ModelState.AddModelError("", "No se pudo guardar el " + item.files.FileName + " archivo.");
                        //files.Remove(file);
                    }
                    else
                    {
                        db.R23.Add(new R23
                        {
                            dateUpload = DateTime.Now,
                            comercializacion = comercializacion,
                            description = item.descritions,
                            file = r23,
                            userUpload = db.AspNetUsers.Find(User.Identity.GetUserId())
                        });
                        db.SaveChanges();
                    }


                }



            }
            r23s = db.R23.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion)
                .Where(x => x.file != null)
                .ToList();
            ViewBag.r23s = r23s;
            Files.borrarArchivosLocales();
            foreach (R23 r23 in r23s)
            {
                await Files.BajarArchivoADirectorioLocalAsync(r23.file);
            }
            return View(comercializacion);
        }


        // POST: Comercializacions/IngresarR24
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        public async Task<ActionResult> IngresarR24(int idComercializacion, List<ViewModelR24> r24Model)
        {
            var comercializacion = db.Comercializacion.Find(idComercializacion);
            bool done = true;
            List<R24> r24s = db.R24.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion).ToList();
            r24Model = r24Model.Where(x => x != null && x.files != null).ToList();

            // verificar que se selecciono un archivo
            if (r24Model != null && r24Model.Count() <= 0)
            {
                ModelState.AddModelError("", "Se debe seleccionar un archivo.");
                done = false;
            }
            else if (r24s.Count() >= 5)
            {
                ModelState.AddModelError("", "Solo puede tener 5 archivos como máximo");
                done = false;
            }
            else
            {
                // validar extenciones y tamaño maximo del archivo

                foreach (ViewModelR24 item in r24Model)
                {
                    var archivoValido = Files.ArchivoValido(item.files, new[] { ".pdf" }, 10 * 1024);
                    if (archivoValido != "")
                    {
                        ModelState.AddModelError("", archivoValido + " ( " + item.files.FileName + " )");
                        done = false;
                    }

                }

            }


            if (done)
            {



                //agregar los nuevos ficheros
                foreach (ViewModelR24 item in r24Model)
                {
                    Storage r24 = await Files.CrearArchivoAsync(item.files, "Comercializacion/r24/");
                    if (r24 == null)
                    {
                        ModelState.AddModelError("", "No se pudo guardar el " + item.files.FileName + " archivo.");
                        //files.Remove(file);
                    }
                    else
                    {
                        db.R24.Add(new R24
                        {
                            dateUpload = DateTime.Now,
                            comercializacion = comercializacion,
                            description = item.descritions,
                            file = r24,
                            userUpload = db.AspNetUsers.Find(User.Identity.GetUserId())
                        });
                        db.SaveChanges();
                    }


                }



            }
            r24s = db.R24.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion)
                .Where(x => x.file != null)
                .ToList();
            ViewBag.r24s = r24s;
            Files.borrarArchivosLocales();
            foreach (R24 r24 in r24s)
            {
                await Files.BajarArchivoADirectorioLocalAsync(r24.file);
            }
            return View(comercializacion);
        }
        // POST: Comercializacions/IngresarR24
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        public async Task<ActionResult> IngresarCredenciales(int idComercializacion, List<ViewModelCredenciales> viewModelCredenciales)
        {
            var comercializacion = db.Comercializacion.Find(idComercializacion);
            bool done = true;
            List<CredencialesFile> credencialesFiles = db.CredencialesFile.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion).ToList();
            viewModelCredenciales = viewModelCredenciales.Where(x => x != null && x.files != null).ToList();

            // verificar que se selecciono un archivo
            if (viewModelCredenciales != null && viewModelCredenciales.Count() <= 0)
            {
                ModelState.AddModelError("", "Se debe seleccionar un archivo.");
                done = false;
            }
            else if (credencialesFiles.Count() >= 5)
            {
                ModelState.AddModelError("", "Solo puede tener 5 archivos como máximo");
                done = false;
            }
            else
            {
                // validar extenciones y tamaño maximo del archivo

                foreach (ViewModelCredenciales item in viewModelCredenciales)
                {
                    var archivoValido = Files.ArchivoValido(item.files, new[] { ".pdf" }, 10 * 1024);
                    if (archivoValido != "")
                    {
                        ModelState.AddModelError("", archivoValido + " ( " + item.files.FileName + " )");
                        done = false;
                    }

                }

            }


            if (done)
            {



                //agregar los nuevos ficheros
                foreach (ViewModelCredenciales item in viewModelCredenciales)
                {
                    Storage file = await Files.CrearArchivoAsync(item.files, "Comercializacion/r24/");
                    if (file == null)
                    {
                        ModelState.AddModelError("", "No se pudo guardar el " + item.files.FileName + " archivo.");
                        //files.Remove(file);
                    }
                    else
                    {
                        db.CredencialesFile.Add(new CredencialesFile
                        {
                            dateUpload = DateTime.Now,
                            comercializacion = comercializacion,
                            description = item.descritions,
                            file = file,
                            userUpload = db.AspNetUsers.Find(User.Identity.GetUserId())
                        });
                        db.SaveChanges();
                    }


                }



            }
            credencialesFiles = db.CredencialesFile.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion)
                .Where(x => x.file != null)
                .ToList();
            ViewBag.credencialesFile = credencialesFiles;
            Files.borrarArchivosLocales();
            foreach (CredencialesFile credencial in credencialesFiles)
            {
                await Files.BajarArchivoADirectorioLocalAsync(credencial.file);
            }
            return View(comercializacion);
        }
        // GET: Comercializacions/ConfirmarCurso/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/Perfil/" })]
        public ActionResult ConfirmarCurso(string id)
        {
            if (id == null || id == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var ids = id.Split('-');
            var comercializacion = db.Comercializacion.Find(int.Parse(ids[0]));
            var relator = db.Relators.Find(int.Parse(ids[1]));
            if (comercializacion == null || relator == null)
            {
                return HttpNotFound();
            }
            ViewBag.idRelator = relator.idRelator;
            return View(comercializacion);
        }

        // POST: Comercializacions/ConfirmarCurso
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/Perfil/" })]
        [HttpPost]
        public ActionResult ConfirmarCurso(int idComercializacion, int idRelator)
        {
            var comercializacion = db.Comercializacion.Find(idComercializacion);
            var relator = db.Relators.Find(idRelator);
            comercializacion.relatoresConfirmados.Add(relator);
            db.Entry(comercializacion).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index", "Home", new { });
        }

        public void AgregarHistorialComercializacion(int idComercializacion)
        {
            var comercializacion = db.Comercializacion.Find(idComercializacion);
            var historialComercializacion = new HistorialComercializacion();
            historialComercializacion.comercializacion = comercializacion;
            var idUser = User.Identity.GetUserId();
            historialComercializacion.usuarioModificacion = db.AspNetUsers.Find(idUser);
            historialComercializacion.fechaModificacion = DateTime.Now;
            // ---- Empresa ----
            historialComercializacion.nombreEmpresa = comercializacion.cotizacion.cliente.nombreEmpresa;
            historialComercializacion.rutEmpresa = comercializacion.cotizacion.cliente.rut;
            historialComercializacion.razonSocialEmpresa = comercializacion.cotizacion.razonSocial;
            historialComercializacion.giroEmpresa = comercializacion.cotizacion.giro;
            historialComercializacion.contactoEmpresa = db.Contacto.Find(comercializacion.cotizacion.contacto).nombreCompleto;
            historialComercializacion.telefonoCorporativoEmpresa = comercializacion.cotizacion.cliente.telefonoCorporativo;
            historialComercializacion.contactoEncargadoPagosEmpresa = db.Contacto.Find(comercializacion.cotizacion.contactoEncargadoPago).nombreCompleto;
            historialComercializacion.direccionEmpresa = comercializacion.cotizacion.cliente.direccion;
            // ---- Curso ----
            historialComercializacion.tipoCurso = comercializacion.cotizacion.tipoCurso;
            // ---- Comercializacion ----
            historialComercializacion.codigoCotizacionComercializacion = comercializacion.cotizacion.codigoCotizacion;
            historialComercializacion.liderComercialComercializacion = comercializacion.usuarioCreador.UserName;
            historialComercializacion.estadoComercializacion = comercializacion.comercializacionEstadoComercializacion.OrderByDescending(x => x.fechaCreacion).FirstOrDefault().EstadoComercializacion.ToString();
            historialComercializacion.fechaInicioComercializacion = comercializacion.fechaInicio.ToString("dd/MM/yyyy");
            historialComercializacion.fechaTerminoComercializacion = comercializacion.fechaTermino.ToString("dd/MM/yyyy");
            historialComercializacion.ciudadComercializacion = comercializacion.ciudad.nombreCiudad;
            historialComercializacion.observacionComercializacion = comercializacion.observacion;
            // ---- Tipo Venta ----
            historialComercializacion.tiposVenta = new List<TipoVentaHistorialComercializacion>();
            foreach (var tipoVenta in comercializacion.cotizacion.documentosCompromiso)
            {
                var historialTipoVenta = new TipoVentaHistorialComercializacion();
                historialTipoVenta.tipoVenta = tipoVenta.tipoVenta.tipoPago.ToString();
                historialTipoVenta.otic = tipoVenta.tipoVenta.otic != null ? tipoVenta.tipoVenta.otic.nombre : null;
                historialTipoVenta.documento = tipoVenta.tipoDocCompromiso != null ? tipoVenta.tipoDocCompromiso.nombre : "OC";
                historialTipoVenta.numeroSerie = tipoVenta.numeroSerie;
                historialTipoVenta.monto = String.Format("{0:C}", tipoVenta.monto);
                historialTipoVenta.nombreArchivo = tipoVenta.documento.nombreArchivo;
                historialComercializacion.tiposVenta.Add(historialTipoVenta);
            }
            // segun los distintos tipos de cotizacion
            if (comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                // ---- Comercializacion ----
                historialComercializacion.mesesVigenciacredencialesComercializacion = comercializacion.vigenciaCredenciales.ToString();
                historialComercializacion.senceNetComercializacion = comercializacion.senceNet;
                historialComercializacion.lugarRealizacionComercializacion = comercializacion.cotizacion.lugarRealizacion;
                historialComercializacion.esProcesoPractico = comercializacion.cotizacion.procesoPractico;
                historialComercializacion.diplomaDescriptorContenidosComercializacion = comercializacion.R23ConDescriptorContenidos;
                // ---- Curso ----
                historialComercializacion.modalidadCurso = comercializacion.cotizacion.modalidad;
                historialComercializacion.nombreCurso = comercializacion.cotizacion.curso.nombreCurso;
                historialComercializacion.tipoEjecucionCurso = comercializacion.cotizacion.curso.tipoEjecucion.ToString();
                historialComercializacion.nombreDiplomaCurso = comercializacion.cotizacion.nombreDiploma;
                historialComercializacion.sinCodigoSenceCurso = comercializacion.cotizacion.tieneCodigoSence == "on" ? true : false;
                historialComercializacion.codigoSenceCurso = comercializacion.cotizacion.codigoSence;
                historialComercializacion.codigoConsolidacionCurso = comercializacion.cotizacion.calendarizacionAbierta != null ? comercializacion.cotizacion.calendarizacionAbierta.codigoConsolidacion : null;
                // ---- Relatores ----
                historialComercializacion.relatores = new List<RelatorHistorialComercializacion>();
                foreach (var relator in comercializacion.relatoresCursos)
                {
                    var historialRelator = new RelatorHistorialComercializacion();
                    historialRelator.nombre = relator.relator.contacto.nombreCompleto;
                    historialRelator.run = relator.relator.contacto.run;
                    historialRelator.correoElectronico = relator.relator.contacto.correo;
                    historialRelator.telefono = relator.relator.contacto.telefono;
                    historialRelator.validoSence = relator.validoSence ? "Sí" : "No";
                    historialComercializacion.relatores.Add(historialRelator);
                }
                // ---- Costos ----
                historialComercializacion.cantidadParticipantesCosto = comercializacion.cotizacion.cantidadParticipante.ToString();
                historialComercializacion.condicionesPagoCosto = comercializacion.cotizacion.condicionesDePago;
                historialComercializacion.tipoMenuCosto = comercializacion.cotizacion.tipoMenu;
                // ---- Costos ----
                historialComercializacion.costos = new List<CostoHistorialComercializacion>();
                foreach (var costo in db.Costo.Where(x => x.idCotizacion == comercializacion.cotizacion.idCotizacion_R13).ToList())
                {
                    var historialCosto = new CostoHistorialComercializacion();
                    historialCosto.detalle = costo.detalle;
                    historialCosto.cantidad = costo.cantidad.ToString();
                    historialCosto.valor = costo.valor.ToString();
                    historialCosto.total = costo.total.ToString();
                    historialComercializacion.costos.Add(historialCosto);
                }
            }
            // ---- Costos ----
            historialComercializacion.totalCosto = comercializacion.valorFinal.Value;
            historialComercializacion.descuentoCosto = comercializacion.descuento;
            historialComercializacion.usuarioCreadorDescuentoCosto = comercializacion.usuarioCreadorDescuento != null ? comercializacion.usuarioCreadorDescuento.UserName : null;
            historialComercializacion.fechaDescuentoCosto = comercializacion.fechaDescuento.ToString("dd/MM/yyyy");
            // ---- Guardar ----
            db.HistorialComercializacion.Add(historialComercializacion);
            db.SaveChanges();
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        // GET: HistorialVersiones
        public ActionResult HistorialVersiones(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var historial = db.HistorialComercializacion
                .Where(x => x.comercializacion.idComercializacion == id)
                .OrderByDescending(x => x.fechaModificacion)
                .ToList();
            if (historial == null)
            {
                return HttpNotFound();
            }
            ViewBag.codigoCotizacion = db.Comercializacion.Find(id).cotizacion.codigoCotizacion;
            return View(historial);
        }

        // GET: Comercializacions/Version/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Version(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var versionComercializacion = db.HistorialComercializacion.Find(id);
            if (versionComercializacion == null)
            {
                return HttpNotFound();
            }
            return View(versionComercializacion);
        }



        // GET: Test
        [Authorize]
        [HttpGet]
        public ActionResult Temporal()
        {
            var date = new DateTime(2021, 05, 13).Date;
            var estados = db.ComercializacionEstadoComercializacion.Where(x => x.EstadoComercializacion == EstadoComercializacion.Terminada && x.comercializacion.softDelete == false && DateTime.Compare(x.fechaCreacion, date) > 0).ToList();
            var comercializacions = estados.Select(x => x.comercializacion).ToList();
            comercializacions.ForEach(x => CorreoComercializacionTerminada(x));
            return RedirectToAction("Index");

        }
        // --------------------------- Reportes -------------------------------


        // ------------------------------ R23 ---------------------------------
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/", "/ClienteContacto/" })]
        [EnableJsReport()]
        public async Task<ActionResult> GenerarReporteQR(int? id)
        {
            var comercializacion = db.Comercializacion.Find(id);
            // con contenido especifico o no
            var nombreTemplate = "QR";

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


            var elerning = "";
            if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono || comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono)
            {
                elerning = "(E)";
            }
            var nombreArchivo = "QR" + elerning + " " + comercializacion.cotizacion.nombreDiploma + " (" + comercializacion.cotizacion.codigoCotizacion + ")";

            var data = DataQR(comercializacion);
            if (data == null)
            {
                ModelState.AddModelError("", "La comercialización " + comercializacion.cotizacion.codigoCotizacion + " no tiene participantes");
                return RedirectToAction("Index", "Comercializacions");
            }

            // jsreport
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
                .Configure((r) => r.Data = data)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"" + nombreArchivo + ".docx\"");
            return null;
        }

        public object DataQR(Comercializacion comercializacion)
        {
            CultureInfo culture = new CultureInfo("es");
            var participantesTemp = new List<object>();
            var participantes = new List<object>();

            var i = 0;
            var total = 1;

            if (comercializacion.participantes.Count() > 0)
            {
                foreach (var item in comercializacion.participantes)
                {




                    // genera qr para verificacion credenciales
                    var writer = new BarcodeWriter { Format = BarcodeFormat.QR_CODE };
                    var qr = writer.Write(Url.Action("ParticipanteQR", "Participante", new { id = item.contacto.idContacto, rut = item.contacto.run }, Request.Url.Scheme));
                    var qrByteArray = Utils.Utils.ImageToByte2(qr);
                    var qrBase64 = Convert.ToBase64String(qrByteArray, 0, qrByteArray.Length);
                    // data

                    var participante = new
                    {
                        existe = true,
                        item.contacto.nombreCompleto,
                        item.contacto.run,
                        fecha = DateTime.Now.ToString("dd MMM yy", culture),
                        comercializacion.ciudad.nombreCiudad,
                        comercializacion.cotizacion.codigoCotizacion,
                        qr = "data:image/png;base64," + qrBase64
                    };

                    switch (i)
                    {
                        case 0:
                            participantesTemp.Add(participante);
                            break;
                        case 1:
                            participantesTemp.Add(participante);
                            break;
                        case 2:
                            participantesTemp.Add(participante);
                            break;
                        case 3:
                            participantesTemp.Add(participante);
                            break;
                        case 4:
                            participantesTemp.Add(participante);
                            break;
                        case 5:
                            participantesTemp.Add(participante);
                            break;
                    }


                    if (i == 5 || total == comercializacion.participantes.Count())
                    {
                        participantes.Add(participantesTemp);
                        participantesTemp = new List<object>();
                        i = -1;
                    }
                    i++;
                    total++;

                }

                var cont = 0;
                if (((List<object>)participantes.ElementAt(participantes.Count() - 1)).Count() < 6)
                {
                    cont = (6 - ((List<object>)participantes.ElementAt(participantes.Count() - 1)).Count());

                }
                else if (((List<object>)participantes.ElementAt(participantes.Count() - 1)).Count() % 6 != 0)
                {
                    cont = 6 - (((List<object>)participantes.ElementAt(participantes.Count() - 1)).Count() % 6);
                }


                for (int j = (6 - cont); j < 6; j++)
                {
                    var participante = new
                    {
                        existe = false,
                        nombreCompleto = " ",
                        run = " ",
                        fecha = DateTime.Now.ToString("dd MMM yy", culture),
                        nombreCiudad = " ",
                        codigoCotizacion = " ",
                        qr = "data:image/png;base64,"

                    };
                    switch (j)
                    {
                        case 0:
                            ((List<object>)participantes.ElementAt(participantes.Count() - 1)).Add(participante);
                            break;
                        case 1:
                            ((List<object>)participantes.ElementAt(participantes.Count() - 1)).Add(participante);
                            break;
                        case 2:
                            ((List<object>)participantes.ElementAt(participantes.Count() - 1)).Add(participante);
                            break;
                        case 3:
                            ((List<object>)participantes.ElementAt(participantes.Count() - 1)).Add(participante);
                            break;
                        case 4:
                            ((List<object>)participantes.ElementAt(participantes.Count() - 1)).Add(participante);
                            break;
                        case 5:
                            ((List<object>)participantes.ElementAt(participantes.Count() - 1)).Add(participante);
                            break;
                    }
                }


            }
            else
            {
                return null;
            }

            var participantesList = participantes.Select(x => new
            {
                a = ((List<object>)x).ElementAt(0),
                b = ((List<object>)x).ElementAt(1),
                c = ((List<object>)x).ElementAt(2),
                d = ((List<object>)x).ElementAt(3),
                e = ((List<object>)x).ElementAt(4),
                f = ((List<object>)x).ElementAt(5)

            }).ToList();
            // data
            var data = new
            {
                fecha = DateTime.Now.ToString("dd MMM yy", culture),
                comercializacion.ciudad.nombreCiudad,
                comercializacion.cotizacion.codigoCotizacion,
                participantes = participantesList
            };
            return data;
        }







        public object DataR23(Comercializacion comercializacion, int idRelator, string firmaRelator, string firmaInsecap, string aprobacion)
        {
            CultureInfo culture = new CultureInfo("es");
            var r11 = db.R11.Where(r => r.idCurso == comercializacion.cotizacion.idCurso).FirstOrDefault();
            var participantes = new List<object>();
            int numero = 0;
            foreach (var item in comercializacion.participantes)
            {
                // nota teorica
                var notaTeorica = 0.0;
                var contTeorica = 0;
                foreach (var evaluacion in comercializacion.evaluaciones)
                {
                    if (evaluacion.categoria == CategoriaEvaluacion.Teorico)
                    {
                        if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault() != null)
                        {
                            if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != ""
                                && item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != "-")
                            {
                                notaTeorica += double.Parse(item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota);
                            }
                        }
                        contTeorica++;
                    }
                }
                if (contTeorica > 0)
                {
                    notaTeorica = notaTeorica / contTeorica;
                }
                // nota practica
                var notaPractica = 0.0;
                var contPractica = 0;
                foreach (var evaluacion in comercializacion.evaluaciones)
                {
                    if (evaluacion.categoria == CategoriaEvaluacion.Practico)
                    {
                        if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault() != null)
                        {
                            if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != ""
                                && item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != "-")
                            {
                                notaPractica += double.Parse(item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota);
                            }
                        }
                        contPractica++;
                    }
                }
                if (contPractica > 0)
                {
                    notaPractica = notaPractica / contPractica;
                }
                // ver q notas se muestran y promedio
                var notaT = "";
                var notaP = "";
                var notaTP = "";
                var notaTPno = "Si";
                var textoAprobacion = "aprobación";
                var promedio = (notaPractica + notaTeorica) / 2;
                if (comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Teorico).Count() > 0)
                {
                    notaT = "Si";
                    if (notaTeorica < 5)
                    {
                        textoAprobacion = "participación";
                    }
                }
                if (comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Practico).Count() > 0)
                {
                    notaP = "Si";
                    if (notaPractica < 5)
                    {
                        textoAprobacion = "participación";
                    }
                }
                if (notaT != "" && notaP != "" || notaT == "" && notaP == "")
                {
                    notaTP = "Si";
                    notaTPno = "";
                    notaT = "";
                    notaP = "";
                    if (promedio < 5)
                    {
                        textoAprobacion = "participación";
                    }
                }
                // descripcion contenidos
                var contenidos = new List<object>();
                foreach (var contenido in r11.conteidoEspecifico)
                {
                    var descripciones = new List<object>();
                    foreach (var descripcion in contenido.itemConteidoEspecificoR11)
                    {
                        descripciones.Add(new
                        {
                            descripcion.contenidoEspecifico
                        });
                    }
                    contenidos.Add(new
                    {
                        contenido.nombre,
                        contenido.horasP,
                        contenido.horasT,
                        descripciones
                    });
                }
                // genera qr para verificacion credenciales
                var writer = new BarcodeWriter { Format = BarcodeFormat.QR_CODE };
                var qr = writer.Write(Url.Action("Validar", "ValidarCredenciales", new { id = comercializacion.cotizacion.codigoCotizacion, id2 = item.contacto.run }, Request.Url.Scheme));
                var qrByteArray = Utils.Utils.ImageToByte2(qr);
                var qrBase64 = Convert.ToBase64String(qrByteArray, 0, qrByteArray.Length);
                // data
                numero++;
                // Si es asincronico calcula la fecha de termino segun horas del curso
                DateTime fechaTermino;
                if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono
                    || comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica)
                {
                    int diasCurso = (int)((r11.horasPracticas + r11.horasTeoricas) / 8);
                    fechaTermino = comercializacion.fechaInicio;
                    if (diasCurso > 1)
                    {
                        fechaTermino = comercializacion.fechaInicio.AddDays(diasCurso - 1);
                    }
                }
                else
                {
                    fechaTermino = comercializacion.fechaTermino;
                }

                var participante = new
                {
                    numero,
                    item.contacto.nombreCompleto,
                    item.contacto.nombres,
                    item.contacto.apellidoPaterno,
                    item.contacto.apellidoMaterno,
                    item.contacto.run,
                    item.contacto.correo,
                    item.contacto.telefono,
                    fecha = DateTime.Now.ToString("dd MMM yy", culture),
                    fechaInicio = comercializacion.fechaInicio.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                    fechaTermino = fechaTermino.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                    comercializacion.ciudad.nombreCiudad,
                    comercializacion.cotizacion.codigoCotizacion,
                    comercializacion.cotizacion.nombreEmpresa,
                    comercializacion.cotizacion.nombreDiploma,
                    comercializacion.cotizacion.lugarRealizacion,
                    comercializacion.cotizacion.codigoSence,
                    nombreCurso = comercializacion.cotizacion.curso.nombreCurso.ToUpper(),
                    comercializacion.cotizacion.curso.codigoCurso,
                    vigencia = comercializacion.fechaTermino.AddMonths(comercializacion.vigenciaCredenciales).ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                    //vigencia = comercializacion.vigenciaCredenciales.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                    horas = String.Format("{0:0.#}", r11.horasPracticas + r11.horasTeoricas),
                    relator = idRelator == 0 ? null : comercializacion.relatoresCursos.Where(r => r.idRelator == idRelator).FirstOrDefault().relator.contacto.nombreCompleto.ToUpper(),
                    notaTeorica = notaTeorica.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture),
                    notaPractica = notaPractica.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture),
                    notaT,
                    notaP,
                    notaTP,
                    notaTPno,
                    firmaRelator,
                    firmaInsecap,
                    contenidos,
                    textoAprobacion,
                    promedio,
                    qr = "data:image/png;base64," + qrBase64
                };
                // ver si aprobado / reprobado
                if (item.notas.Count() > 0)
                {
                    if (aprobacion == "aprobados")
                    {
                        if (comercializacion.evaluaciones.Where(x => x.categoria == CategoriaEvaluacion.Teorico).Count() > 0
                            && comercializacion.evaluaciones.Where(x => x.categoria == CategoriaEvaluacion.Practico).Count() <= 0)
                        {
                            if (notaTeorica >= 5)
                            {
                                participantes.Add(participante);
                            }
                        }
                        if (comercializacion.evaluaciones.Where(x => x.categoria == CategoriaEvaluacion.Practico).Count() > 0
                            && comercializacion.evaluaciones.Where(x => x.categoria == CategoriaEvaluacion.Teorico).Count() <= 0)
                        {
                            if (notaPractica >= 5)
                            {
                                participantes.Add(participante);
                            }
                        }
                        //if (notaT != "" && notaP != "" || notaT == "" && notaP == "")
                        if (comercializacion.evaluaciones.Where(x => x.categoria == CategoriaEvaluacion.Practico).Count() > 0
                            && comercializacion.evaluaciones.Where(x => x.categoria == CategoriaEvaluacion.Teorico).Count() > 0)
                        {
                            if (promedio >= 5)
                            {
                                participantes.Add(participante);
                            }
                        }
                    }
                    else if (aprobacion == "reprobados")
                    {
                        if (comercializacion.evaluaciones.Where(x => x.categoria == CategoriaEvaluacion.Teorico).Count() > 0
                            && comercializacion.evaluaciones.Where(x => x.categoria == CategoriaEvaluacion.Practico).Count() <= 0)
                        {
                            if (notaTeorica < 5)
                            {
                                participantes.Add(participante);
                            }
                        }
                        if (comercializacion.evaluaciones.Where(x => x.categoria == CategoriaEvaluacion.Practico).Count() > 0
                            && comercializacion.evaluaciones.Where(x => x.categoria == CategoriaEvaluacion.Teorico).Count() <= 0)
                        {
                            if (notaPractica < 5)
                            {
                                participantes.Add(participante);
                            }
                        }
                        if (comercializacion.evaluaciones.Where(x => x.categoria == CategoriaEvaluacion.Practico).Count() > 0
                            && comercializacion.evaluaciones.Where(x => x.categoria == CategoriaEvaluacion.Teorico).Count() > 0)
                        {
                            if (promedio < 5)
                            {
                                participantes.Add(participante);
                            }
                        }
                    }
                    else
                    {
                        participantes.Add(participante);
                    }
                }
            }
            // ver si es con sence
            var codigoSence = r11.codigoSence;
            var sence = "";
            if (codigoSence != null && comercializacion.cotizacion.tieneCodigoSence != "on")
            {
                sence = "SENCE";
            }
            // data
            var data = new
            {
                fecha = DateTime.Now.ToString("dd MMM yy", culture),
                fechaInicio = comercializacion.fechaInicio.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                fechaTermino = comercializacion.fechaTermino.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                comercializacion.ciudad.nombreCiudad,
                comercializacion.cotizacion.codigoCotizacion,
                comercializacion.cotizacion.nombreEmpresa,
                comercializacion.cotizacion.nombreDiploma,
                comercializacion.cotizacion.lugarRealizacion,
                comercializacion.cotizacion.codigoSence,
                comercializacion.cotizacion.curso.nombreCurso,
                comercializacion.cotizacion.curso.codigoCurso,
                sucursal = comercializacion.cotizacion.sucursal.nombre,
                vigencia = comercializacion.fechaTermino.AddMonths(comercializacion.vigenciaCredenciales).ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                //vigencia = comercializacion.vigenciaCredenciales.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                horas = String.Format("{0:0.#}", r11.horasPracticas + r11.horasTeoricas),
                relator = idRelator == 0 ? null : comercializacion.relatoresCursos.Where(r => r.idRelator == idRelator).FirstOrDefault().relator.contacto.nombreCompleto.ToUpper(),
                sence,
                participantes
            };
            return data;
        }

        public object DataR23Uno(Participante participanteRecibido, int idRelator, string firmaRelator, string firmaInsecap)
        {
            CultureInfo culture = new CultureInfo("es");
            var r11 = db.R11.Where(r => r.idCurso == participanteRecibido.comercializacion.cotizacion.idCurso).FirstOrDefault();
            var participantes = new List<object>();
            int numero = 0;
            var notaTeorica = 0.0;
            var contTeorica = 0;
            foreach (var evaluacion in participanteRecibido.comercializacion.evaluaciones)
            {
                if (evaluacion.categoria == CategoriaEvaluacion.Teorico)
                {
                    if (participanteRecibido.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault() != null)
                    {
                        if (participanteRecibido.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != ""
                            && participanteRecibido.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != "-")
                        {
                            notaTeorica += double.Parse(participanteRecibido.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota);
                        }
                    }
                    contTeorica++;
                }
            }
            if (contTeorica > 0)
            {
                notaTeorica = notaTeorica / contTeorica;
            }
            var notaPractica = 0.0;
            var contPractica = 0;
            foreach (var evaluacion in participanteRecibido.comercializacion.evaluaciones)
            {
                if (evaluacion.categoria == CategoriaEvaluacion.Practico)
                {
                    if (participanteRecibido.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault() != null)
                    {
                        if (participanteRecibido.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != ""
                            && participanteRecibido.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != "-")
                        {
                            notaPractica += double.Parse(participanteRecibido.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota);
                        }
                    }
                    contPractica++;
                }
            }
            if (contPractica > 0)
            {
                notaPractica = notaPractica / contPractica;
            }
            var notaT = "";
            var notaP = "";
            var notaTP = "";
            var notaTPno = "Si";
            if (participanteRecibido.comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Teorico).Count() > 0)
            {
                notaT = "Si";
            }
            if (participanteRecibido.comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Practico).Count() > 0)
            {
                notaP = "Si";
            }
            if (notaT != "" && notaP != "" || notaT == "" && notaP == "")
            {
                notaTP = "Si";
                notaTPno = "";
                notaT = "";
                notaP = "";
            }
            var textoAprobacion = "aprobación";
            var promedio = (notaPractica + notaTeorica) / 2;
            if (participanteRecibido.comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Teorico).Count() > 0)
            {
                notaT = "Si";
                if (notaTeorica < 5)
                {
                    textoAprobacion = "participación";
                }
            }
            if (participanteRecibido.comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Practico).Count() > 0)
            {
                notaP = "Si";
                if (notaPractica < 5)
                {
                    textoAprobacion = "participación";
                }
            }
            if (notaT != "" && notaP != "" || notaT == "" && notaP == "")
            {
                notaTP = "Si";
                notaTPno = "";
                notaT = "";
                notaP = "";
                if (promedio < 5)
                {
                    textoAprobacion = "participación";
                }
            }
            // descripcion contenidos
            var contenidos = new List<object>();
            foreach (var contenido in r11.conteidoEspecifico)
            {
                var descripciones = new List<object>();
                foreach (var descripcion in contenido.itemConteidoEspecificoR11)
                {
                    descripciones.Add(new
                    {
                        descripcion.contenidoEspecifico
                    });
                }
                contenidos.Add(new
                {
                    contenido.nombre,
                    contenido.horasP,
                    contenido.horasT,
                    descripciones
                });
            }
            // genera qr para verificacion credenciales
            var writer = new BarcodeWriter { Format = BarcodeFormat.QR_CODE };
            var qr = writer.Write(Url.Action("Validar", "ValidarCredenciales", new { id = participanteRecibido.comercializacion.cotizacion.codigoCotizacion, id2 = participanteRecibido.contacto.run }, Request.Url.Scheme));
            var qrByteArray = Utils.Utils.ImageToByte2(qr);
            var qrBase64 = Convert.ToBase64String(qrByteArray, 0, qrByteArray.Length);
            numero++;

            // Si es asincronico calcula la fecha de termino segun horas del curso
            DateTime fechaTermino;
            if (participanteRecibido.comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono
                || participanteRecibido.comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica)
            {
                int diasCurso = (int)((r11.horasPracticas + r11.horasTeoricas) / 8);
                fechaTermino = participanteRecibido.comercializacion.fechaInicio;
                if (diasCurso > 1)
                {
                    fechaTermino = participanteRecibido.comercializacion.fechaInicio.AddDays(diasCurso - 1);
                }
            }
            else
            {
                fechaTermino = participanteRecibido.comercializacion.fechaTermino;
            }

            var participante = new
            {
                numero,
                participanteRecibido.contacto.nombreCompleto,
                participanteRecibido.contacto.nombres,
                participanteRecibido.contacto.apellidoPaterno,
                participanteRecibido.contacto.apellidoMaterno,
                participanteRecibido.contacto.run,
                participanteRecibido.contacto.correo,
                participanteRecibido.contacto.telefono,
                fecha = DateTime.Now.ToString("dd MMM yy", culture),
                fechaInicio = participanteRecibido.comercializacion.fechaInicio.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                fechaTermino = fechaTermino.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                participanteRecibido.comercializacion.ciudad.nombreCiudad,
                participanteRecibido.comercializacion.cotizacion.codigoCotizacion,
                participanteRecibido.comercializacion.cotizacion.nombreEmpresa,
                participanteRecibido.comercializacion.cotizacion.nombreDiploma,
                participanteRecibido.comercializacion.cotizacion.lugarRealizacion,
                participanteRecibido.comercializacion.cotizacion.codigoSence,
                nombreCurso = participanteRecibido.comercializacion.cotizacion.curso.nombreCurso.ToUpper(),
                participanteRecibido.comercializacion.cotizacion.curso.codigoCurso,
                vigencia = participanteRecibido.comercializacion.fechaTermino.AddMonths(participanteRecibido.comercializacion.vigenciaCredenciales).ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                //vigencia = participanteRecibido.comercializacion.vigenciaCredenciales.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                horas = String.Format("{0:0.#}", r11.horasPracticas + r11.horasTeoricas),
                relator = idRelator == 0 ? null : participanteRecibido.comercializacion.relatoresCursos.Where(r => r.idRelator == idRelator).FirstOrDefault().relator.contacto.nombreCompleto.ToUpper(),
                notaTeorica = notaTeorica.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture),
                notaPractica = notaPractica.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture),
                notaT,
                notaP,
                notaTP,
                notaTPno,
                firmaRelator,
                firmaInsecap,
                contenidos,
                textoAprobacion,
                promedio,
                qr = "data:image/png;base64," + qrBase64
            };

            //if (participanteRecibido.notas.Count() > 0)
            //{
            participantes.Add(participante);
            //}

            // ver si es con sence
            var codigoSence = r11.codigoSence;
            var sence = "";
            if (codigoSence != null && participanteRecibido.comercializacion.cotizacion.tieneCodigoSence != "on")
            {
                sence = "SENCE";
            }
            var data = new
            {
                fecha = DateTime.Now.ToString("dd MMM yy", culture),
                fechaInicio = participanteRecibido.comercializacion.fechaInicio.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                fechaTermino = participanteRecibido.comercializacion.fechaTermino.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                participanteRecibido.comercializacion.ciudad.nombreCiudad,
                participanteRecibido.comercializacion.cotizacion.codigoCotizacion,
                participanteRecibido.comercializacion.cotizacion.nombreEmpresa,
                participanteRecibido.comercializacion.cotizacion.nombreDiploma,
                participanteRecibido.comercializacion.cotizacion.lugarRealizacion,
                participanteRecibido.comercializacion.cotizacion.codigoSence,
                participanteRecibido.comercializacion.cotizacion.curso.nombreCurso,
                participanteRecibido.comercializacion.cotizacion.curso.codigoCurso,
                sucursal = participanteRecibido.comercializacion.cotizacion.sucursal.nombre,
                vigencia = participanteRecibido.comercializacion.fechaTermino.AddMonths(participanteRecibido.comercializacion.vigenciaCredenciales).ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                //vigencia = participanteRecibido.comercializacion.vigenciaCredenciales.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                horas = String.Format("{0:0.#}", r11.horasPracticas + r11.horasTeoricas),
                relator = idRelator == 0 ? null : participanteRecibido.comercializacion.relatoresCursos.Where(r => r.idRelator == idRelator).FirstOrDefault().relator.contacto.nombreCompleto.ToUpper(),
                sence,
                participantes
            };
            return data;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerarR23(int? id, int? id2, string id3)
        {
            if (id2 == null)
            {
                return HttpNotFound();
            }
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            // con contenido especifico o no
            var nombreTemplate = "r23";
            if (comercializacion.R23ConDescriptorContenidos)
            {
                nombreTemplate = "r23_contenidos";
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == nombreTemplate)
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"" + nombreTemplate + "\" y tipo \"word\".");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            if (comercializacion.cotizacion.sucursal.firmaAdministrador == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la firma del Lider Comercial.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            var relator = db.Relators.Find(id2);
            if (relator.imagenFirma == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la firma del Relator.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            return RedirectToAction("GenerarReporteR23", new { id, id2, id3 });
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerarR23Uno(int? id, int? id2)
        {
            if (id2 == null)
            {
                return HttpNotFound();
            }
            var participante = db.Participante.Find(id);
            if (participante == null)
            {
                return HttpNotFound();
            }
            // con contenido especifico o no
            var nombreTemplate = "r23";
            if (participante.comercializacion.R23ConDescriptorContenidos)
            {
                nombreTemplate = "r23_contenidos";
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == nombreTemplate)
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"" + nombreTemplate + "\" y tipo \"word\".");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            if (participante.comercializacion.cotizacion.sucursal.firmaAdministrador == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la firma del Lider Comercial.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            var relator = db.Relators.Find(id2);
            if (relator.imagenFirma == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la firma del Relator.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            return RedirectToAction("GenerarReporteR23Uno", new { id, id2 });
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [EnableJsReport()]
        public async Task<ActionResult> GenerarReporteR23(int? id, int? id2, string id3)
        {
            var comercializacion = db.Comercializacion.Find(id);
            // con contenido especifico o no
            var nombreTemplate = "r23";
            if (comercializacion.R23ConDescriptorContenidos)
            {
                nombreTemplate = "r23_contenidos";
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
            // obtener firmas
            var relator = db.Relators.Find(id2);
            var firmaRelator = await Files.BajarArchivoBytesAsync(relator.imagenFirma);
            var firmaRelatorBase64 = Convert.ToBase64String(firmaRelator, 0, firmaRelator.Length);
            var firmaInsecap = await Files.BajarArchivoBytesAsync(comercializacion.cotizacion.sucursal.firmaAdministrador);
            var firmaInsecapBase64 = Convert.ToBase64String(firmaInsecap, 0, firmaInsecap.Length);

            var elerning = "";
            if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono || comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono)
            {
                elerning = "(E)";
            }
            var nombreArchivo = "R23" + elerning + " " + comercializacion.cotizacion.nombreDiploma + " (" + comercializacion.cotizacion.codigoCotizacion + ")";

            // jsreport
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
                .Configure((r) => r.Data = DataR23(comercializacion, (int)id2, "data:image/png;base64," + firmaRelatorBase64, "data:image/png;base64," + firmaInsecapBase64, id3))
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"" + nombreArchivo + ".docx\"");
            return null;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [EnableJsReport()]
        public async Task<ActionResult> GenerarReporteR23Uno(int? id, int? id2)
        {
            var participante = db.Participante.Find(id);
            // con contenido especifico o no
            var nombreTemplate = "r23";
            if (participante.comercializacion.R23ConDescriptorContenidos)
            {
                nombreTemplate = "r23_contenidos";
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
            // obtener firmas
            var relator = db.Relators.Find(id2);
            var firmaRelator = await Files.BajarArchivoBytesAsync(relator.imagenFirma);
            var firmaRelatorBase64 = Convert.ToBase64String(firmaRelator, 0, firmaRelator.Length);
            var firmaInsecap = await Files.BajarArchivoBytesAsync(participante.comercializacion.cotizacion.sucursal.firmaAdministrador);
            var firmaInsecapBase64 = Convert.ToBase64String(firmaInsecap, 0, firmaInsecap.Length);

            var elerning = "";
            if (participante.comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono || participante.comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono)
            {
                elerning = "(E)";
            }
            var nombreArchivo = "R23" + elerning + " " + participante.contacto.run + " - " + participante.comercializacion.cotizacion.nombreDiploma + " (" + participante.comercializacion.cotizacion.codigoCotizacion + ")";

            // jsreport
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
                .Configure((r) => r.Data = DataR23Uno(participante, (int)id2, "data:image/png;base64," + firmaRelatorBase64, "data:image/png;base64," + firmaInsecapBase64))
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"" + nombreArchivo + ".docx\"");
            return null;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/", "/ClienteContacto/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GenerarPdfR23(int? id, int? id2, string id3, string id4)
        {
            if (id2 == null)
            {
                return HttpNotFound();
            }
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            if (id4 != null && id4.Contains("cliente"))
            {
                List<R23> r23s = db.R23.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion).ToList();
                if (r23s.Count() > 0)
                {
                    return await Files.BajarArchivoDescargarAsync(r23s.FirstOrDefault().file);
                }
            }
            // con contenido especifico o no
            var nombreTemplate = "r23";
            if (id2 == 0 && (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono
                || comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica))
            {
                nombreTemplate = "r23_a";
            }
            if (comercializacion.R23ConDescriptorContenidos)
            {
                nombreTemplate = "r23_contenidos";
                if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono
                    || comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica)
                {
                    nombreTemplate = "r23_contenidos_a";
                }
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == nombreTemplate)
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            var relator = db.Relators.Find(id2);
            if (id4 == "cliente")
            {
                if (template == null || comercializacion.cotizacion.sucursal.firmaAdministrador == null || relator.imagenFirma == null)
                {
                    // indicar q hubo un error
                    return RedirectToAction("LandingPage", "ClienteContacto", new { error = "No se pudo generar el documento." });
                }
                //if (bloqueadoOCPendiente(comercializacion))
                //{
                //    // indicar q hubo un error
                //    return RedirectToAction("LandingPage", "ClienteContacto", new { error = "Bloqueado por OC pendiente." });
                //}
            }
            else
            {
                if (template == null)
                {
                    // indicar q hubo un error
                    ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"" + nombreTemplate + "\" y tipo \"word\".");
                    ViewBag.templatesR50 = GetTemplatesR50();
                    return View("Index", db.Comercializacion
                        .Where(x => x.softDelete == false)
                        .ToList());
                }
                if (comercializacion.cotizacion.sucursal.firmaAdministrador == null)
                {
                    // indicar q hubo un error
                    ModelState.AddModelError("", "No se encontro la firma del Lider Comercial.");
                    ViewBag.templatesR50 = GetTemplatesR50();
                    return View("Index", db.Comercializacion
                        .Where(x => x.softDelete == false)
                        .ToList());
                }

                if (id2 != 0)
                {
                    if (relator.imagenFirma == null)
                    {
                        // indicar q hubo un error
                        ModelState.AddModelError("", "No se encontro la firma del Relator.");
                        ViewBag.templatesR50 = GetTemplatesR50();
                        return View("Index", db.Comercializacion
                            .Where(x => x.softDelete == false)
                            .ToList());
                    }
                }

            }

            string hash = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                hash = Utils.Utils.GetHash(sha256Hash, DateTime.Now.ToString());
            }

            string createRequest = Url.Action("GenerarReportePdfR23", "Comercializacions", new { id, id2, id3 = hash, id4 = id3 }, Request.Url.Scheme);
            // Generate Request
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(createRequest);
            req.Method = "GET";

            // Get the Response
            try
            {
                Utils.Utils.CerrarNode();
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Utils.Utils.CerrarNode();
            }
            catch (WebException e)
            {
                return View("Error", (object)"No se pudo generar el documento.");
            }

            var path = directory + hash;
            Byte[] bytes = System.IO.File.ReadAllBytes(path + ".pdf");

            System.IO.File.Delete(path + ".pdf");

            Response.ContentType = "application/pdf";

            var elerning = "";
            if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono || comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono)
            {
                elerning = "(E)";
            }
            var nombreArchivo = "\"R23" + elerning + " " + comercializacion.cotizacion.nombreDiploma + " (" + comercializacion.cotizacion.codigoCotizacion + ").pdf\"";

            Response.AppendHeader("Content-Disposition", "attachment; filename=" + nombreArchivo);

            return new FileContentResult(bytes, "application/pdf");
        }

        [EnableJsReport()]
        public async Task<ActionResult> GenerarReportePdfR23(int? id, int? id2, string id3, string id4)
        {
            var comercializacion = db.Comercializacion.Find(id);
            // con contenido especifico o no
            var nombreTemplate = "r23";
            if (id2 == 0 && (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono
                || comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica))
            {
                nombreTemplate = "r23_a";
            }
            if (comercializacion.R23ConDescriptorContenidos)
            {
                nombreTemplate = "r23_contenidos";
                if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono
                    || comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica)
                {
                    nombreTemplate = "r23_contenidos_a";
                }
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

            String firmaRelatorBase64 = "";
            if (id2 != 0)
            {
                var relator = db.Relators.Find(id2);
                var firmaRelator = await Files.BajarArchivoBytesAsync(relator.imagenFirma);
                firmaRelatorBase64 = Convert.ToBase64String(firmaRelator, 0, firmaRelator.Length);
            }
            var firmaInsecap = await Files.BajarArchivoBytesAsync(comercializacion.cotizacion.sucursal.firmaAdministrador);
            var firmaInsecapBase64 = Convert.ToBase64String(firmaInsecap, 0, firmaInsecap.Length);
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
                .Configure((r) => r.Data = DataR23(comercializacion, (int)id2, "data:image/png;base64," + firmaRelatorBase64, "data:image/png;base64," + firmaInsecapBase64, id4))
                .OnAfterRender((r) =>
                {
                    var log = new string[11];
                    log[0] = "-------------------------------------------------";
                    log[1] = "nuevo R23 " + DateTime.Now + " id: " + id;
                    var path = directory + id3;
                    log[2] = "path: " + path;
                    try
                    {
                        using (var file = System.IO.File.Open(path + ".docx", FileMode.Create))
                        {
                            r.Content.CopyTo(file);
                        }
                        log[3] = "docx generado por jsreport guardado a carpeta files";
                        var appWord = new Microsoft.Office.Interop.Word.Application();
                        log[4] = "instancia microsoft interop creada";
                        var wordDocument = appWord.Documents.Open(path + ".docx");
                        log[5] = "docx abierto por microsoft interop";
                        wordDocument.ExportAsFixedFormat(path + ".pdf", Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF);
                        log[6] = "docx exportado como pdf por microsoft interop";
                        wordDocument.Close();
                        log[7] = "docx cerrao por microsoft interop";
                        appWord.Quit();
                        log[8] = "instancia microsoft interop cerrada";
                        System.IO.File.Delete(path + ".docx");
                        log[9] = "docx borrado";
                    }
                    catch (Exception e)
                    {
                        log[10] = "error: " + e.Message;
                    }
                    System.IO.File.WriteAllLines(path + "log.txt", log);
                });
            return null;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/", "/ClienteContacto/" })]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerarPdfR23Uno(int? id, int? id2)
        {
            if (id2 == null)
            {
                return HttpNotFound();
            }
            var participante = db.Participante.Find(id);
            if (participante == null)
            {
                return HttpNotFound();
            }
            // con contenido especifico o no
            var nombreTemplate = "r23";
            if (id2 == 0)
            {
                nombreTemplate = "r23_a";
            }
            if (participante.comercializacion.R23ConDescriptorContenidos)
            {
                nombreTemplate = "r23_contenidos";
                if (participante.comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono
                    || participante.comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica)
                {
                    nombreTemplate = "r23_contenidos_a";
                }
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == nombreTemplate)
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"" + nombreTemplate + "\" y tipo \"word\".");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            if (participante.comercializacion.cotizacion.sucursal.firmaAdministrador == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la firma del Lider Comercial.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            if (id2 != 0)
            {
                var relator = db.Relators.Find(id2);
                if (relator.imagenFirma == null)
                {
                    // indicar q hubo un error
                    ModelState.AddModelError("", "No se encontro la firma del Relator.");
                    ViewBag.templatesR50 = GetTemplatesR50();
                    return View("Index", db.Comercializacion
                        .Where(x => x.softDelete == false)
                        .ToList());
                }

            }

            string hash = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                hash = Utils.Utils.GetHash(sha256Hash, DateTime.Now.ToString());
            }

            string createRequest = Url.Action("GenerarReportePdfR23Uno", "Comercializacions", new { id, id2, id3 = hash }, Request.Url.Scheme);
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

            var elerning = "";
            if (participante.comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono || participante.comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono)
            {
                elerning = "(E)";
            }
            var nombreArchivo = "\"R23" + elerning + " " + participante.comercializacion.cotizacion.nombreDiploma + " (" + participante.comercializacion.cotizacion.codigoCotizacion + ").pdf\"";

            Response.AppendHeader("Content-Disposition", "attachment; filename=" + nombreArchivo);

            return new FileContentResult(bytes, "application/pdf");
        }


        [EnableJsReport()]
        public async Task<ActionResult> GenerarReportePdfR23Uno(int? id, int? id2, string id3)
        {
            var participante = db.Participante.Find(id);
            // con contenido especifico o no
            var nombreTemplate = "r23";
            if (id2 == 0)
            {
                nombreTemplate = "r23_a";
            }
            if (participante.comercializacion.R23ConDescriptorContenidos)
            {
                nombreTemplate = "r23_contenidos";
                if (participante.comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono
                    || participante.comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica)
                {
                    nombreTemplate = "r23_contenidos_a";
                }
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

            var firmaRelatorBase64 = "data:image/png;base64,";
            if (id2 != 0)
            {
                var relator = db.Relators.Find(id2);
                var firmaRelator = await Files.BajarArchivoBytesAsync(relator.imagenFirma);
                firmaRelatorBase64 += Convert.ToBase64String(firmaRelator, 0, firmaRelator.Length);
            }

            var firmaInsecap = await Files.BajarArchivoBytesAsync(participante.comercializacion.cotizacion.sucursal.firmaAdministrador);
            var firmaInsecapBase64 = Convert.ToBase64String(firmaInsecap, 0, firmaInsecap.Length);
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
                .Configure((r) => r.Data = DataR23Uno(participante, (int)id2, firmaRelatorBase64, "data:image/png;base64," + firmaInsecapBase64))
                .OnAfterRender((r) =>
                {
                    var path = directory + id3;
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

        // ------------------------------ R18 ---------------------------------
        //Genera libro de clases de la comercializacion

        public object DataR18(Comercializacion comercializacion)
        {
            var bloques = new List<object>();
            foreach (var item in comercializacion.bloques)
            {
                var bloque = new
                {
                    fecha = item.fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    horarioInicio = item.horarioInicio.ToString("HH:mm", CultureInfo.InvariantCulture),
                    horarioTermino = item.horarioTermino.ToString("HH:mm", CultureInfo.InvariantCulture),
                    lugarAlmuerzo = item.lugarAlmuerzo.nombre,
                    nombreRelator = item.relator.contacto.nombreCompleto,
                    runRelator = item.relator.contacto.run,
                    sala = item.sala.nombre
                };
                bloques.Add(bloque);
            }
            var relatores = new List<object>();
            var coma = " ";
            foreach (var item in comercializacion.relatoresCursos)
            {
                var relator = new
                {
                    nombre = item.relator.contacto.nombreCompleto,
                    nombreComa = coma + item.relator.contacto.nombreCompleto,
                    item.relator.contacto.run
                };
                relatores.Add(relator);
                coma = ", ";
            }
            var participantes = new List<object>();
            int numero = 0;
            foreach (var item in comercializacion.participantes)
            {
                numero++;
                var participante = new
                {
                    numero,
                    item.contacto.nombres,
                    item.contacto.apellidoPaterno,
                    item.contacto.apellidoMaterno,
                    item.contacto.run,
                    empresa = comercializacion.cotizacion.nombreEmpresa,
                    item.contacto.correo,
                    item.contacto.telefono
                };
                participantes.Add(participante);
            }
            var contenidos = new List<object>();
            var r11 = db.R11.Where(r => r.idCurso == comercializacion.cotizacion.curso.idCurso).FirstOrDefault();
            foreach (var item in r11.conteidoEspecifico)
            {
                var actividades = new List<object>();
                foreach (var itemContenido in item.itemConteidoEspecificoR11)
                {
                    var actividad = new
                    {
                        actividad = itemContenido.contenidoEspecifico
                    };
                    actividades.Add(actividad);
                }
                var contenido = new
                {
                    fecha = "",
                    tema = item.nombre,
                    actividades,
                    horaInicio = "",
                    horaTermino = ""
                };
                contenidos.Add(contenido);
            }
            // ver si es con sence
            var codigoSence = db.R11.Where(r => r.idCurso == comercializacion.cotizacion.curso.idCurso).FirstOrDefault().codigoSence;
            var sence = "";
            if (codigoSence != null && comercializacion.cotizacion.tieneCodigoSence != "on")
            {
                sence = "SENCE";
            }
            // ver si es recertificacion
            var recertificacion = "No Recertificación";
            if (comercializacion.cotizacion.tipoCurso.Contains("Recertificación"))
            {
                recertificacion = "";
            }
            // genera qr para asistencia
            var writer = new BarcodeWriter { Format = BarcodeFormat.QR_CODE };
            var qr = writer.Write(Url.Action("IngresarAsistencia", "Participante", new { id = comercializacion.idComercializacion }, Request.Url.Scheme));
            var qrByteArray = Utils.Utils.ImageToByte2(qr);
            var qrBase64 = Convert.ToBase64String(qrByteArray, 0, qrByteArray.Length);
            // data
            var data = new
            {
                fecha = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                fechaInicio = comercializacion.fechaInicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                fechaTermino = comercializacion.fechaTermino.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                comercializacion.ciudad.nombreCiudad,
                comercializacion.cotizacion.codigoCotizacion,
                comercializacion.cotizacion.nombreEmpresa,
                comercializacion.cotizacion.nombreDiploma,
                comercializacion.cotizacion.lugarRealizacion,
                comercializacion.cotizacion.codigoSence,
                comercializacion.cotizacion.cantidadParticipante,
                comercializacion.cotizacion.curso.nombreCurso,
                comercializacion.cotizacion.curso.codigoCurso,
                comercializacion.cotizacion.modalidad,
                sucursal = comercializacion.cotizacion.sucursal.nombre,
                comercializacion.senceNet,
                bloques,
                relatores,
                //abierto,
                //cerrado,
                participantes,
                contenidos,
                sence,
                recertificacion,
                qr = "data:image/png;base64," + qrBase64
            };
            return data;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/", "/Relator/Perfil/" })]
        public ActionResult GenerarR18(int? id)
        {
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r18")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r18\" y tipo \"word\".");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            return RedirectToAction("GenerarReporteR18", new { id });
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/", "/Relator/Perfil/" })]
        [EnableJsReport()]
        public async Task<ActionResult> GenerarReporteR18(int? id)
        {
            var comercializacion = db.Comercializacion.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r18")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
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
                .Configure((r) => r.Data = DataR18(comercializacion))
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"r18_" + comercializacion.cotizacion.codigoCotizacion + ".docx\"");
            return null;
        }

        // ------------------------------ R22 ---------------------------------

        public object DataR22(Comercializacion comercializacion, string firma)
        {
            var tipoEjecucion = "";
            if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono)
            {
                tipoEjecucion = "Asíncrono";
            }
            if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono)
            {
                tipoEjecucion = "Síncrono";
            }
            if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Presencial)
            {
                tipoEjecucion = "Presencial";
            }
            if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion)
            {
                tipoEjecucion = "Recertificación";
            }
            if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Sincronica)
            {
                tipoEjecucion = "Recertificación Síncrona";
            }
            if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica)
            {
                tipoEjecucion = "Recertificación Asíncrona";
            }
            var participantes = new List<object>();
            int numero = 0;
            foreach (var item in comercializacion.participantes)
            {
                var asistencia = "-";
                var cantBloques = comercializacion.bloques.Count();
                var cantAsistencias = item.asistencia.Where(x => x.asistio == true).Count();
                if (cantBloques > 0)
                {
                    if (cantAsistencias * 100 / cantBloques == 0)
                        continue;
                    asistencia = String.Format("{0:N0}", cantAsistencias * 100 / cantBloques) + "%";

                }
                var condicion = "R";
                var notaTeorica = 0.0;
                var contTeorica = 0;
                foreach (var evaluacion in comercializacion.evaluaciones)
                {
                    if (evaluacion.categoria == CategoriaEvaluacion.Teorico)
                    {
                        if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault() != null)
                        {
                            if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != ""
                                && item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != "-")
                            {
                                notaTeorica += double.Parse(item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota);
                            }
                        }
                        contTeorica++;
                    }
                }
                if (contTeorica > 0)
                {
                    notaTeorica = notaTeorica / contTeorica;
                }
                var notaPractica = 0.0;
                var contPractica = 0;
                foreach (var evaluacion in comercializacion.evaluaciones)
                {
                    if (evaluacion.categoria == CategoriaEvaluacion.Practico)
                    {
                        if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault() != null)
                        {
                            if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != ""
                                && item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != "-")
                            {
                                notaPractica += double.Parse(item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota);
                            }
                        }
                        contPractica++;
                    }
                }
                if (contPractica > 0)
                {
                    notaPractica = notaPractica / contPractica;
                }
                if (comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Teorico).Count() > 0
                    && comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Practico).Count() > 0)
                {
                    if ((notaTeorica + notaPractica) / 2 >= 5)
                    {
                        condicion = "A";
                    }
                }
                else
                {
                    if (comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Teorico).Count() > 0)
                    {
                        if (notaTeorica >= 5)
                        {
                            condicion = "A";
                        }
                    }
                    else
                    {
                        if (comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Practico).Count() > 0)
                        {
                            if (notaPractica >= 5)
                            {
                                condicion = "A";
                            }
                        }
                        else
                        {
                            condicion = "-";
                        }
                    }
                }
                numero++;
                var participante = new
                {
                    numero,
                    item.contacto.nombres,
                    item.contacto.apellidoPaterno,
                    item.contacto.apellidoMaterno,
                    item.contacto.run,
                    empresa = comercializacion.cotizacion.nombreEmpresa,
                    item.contacto.correo,
                    item.contacto.telefono,
                    asistencia,
                    condicion,
                    notaPractica = String.Format("{0:N1}", notaPractica == 0 ? 1 : notaPractica),
                    notaTeorica = String.Format("{0:N1}", notaTeorica == 0 ? 1 : notaTeorica)
                };
                participantes.Add(participante);
            }
            var userId = User.Identity.GetUserId();
            var clienteContacto = db.ClienteContacto.Where(x => x.contacto.usuario.Id == userId).FirstOrDefault();
            var nombreCliente = "";
            var runCliente = "";
            var nombreMandante = comercializacion.cotizacion.nombreEmpresa;
            if (comercializacion.cotizacion.cliente.mandante != null)
            {
                nombreMandante = comercializacion.cotizacion.cliente.mandante.nombreMandante;
            }

            if (clienteContacto != null)
            {
                nombreCliente = clienteContacto.contacto.nombreCompleto;
                runCliente = clienteContacto.contacto.run;
            }
            CultureInfo culture = new CultureInfo("es");
            comercializacion.cotizacion.cantidadParticipante = participantes.Count();
            var data = new
            {
                fecha = DateTime.Now.ToString("dd MMMM, yyyy", culture),
                fechaEntrega = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                fechaInicio = comercializacion.fechaInicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                fechaTermino = comercializacion.fechaTermino.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                comercializacion.ciudad.nombreCiudad,
                comercializacion.cotizacion.codigoCotizacion,
                comercializacion.cotizacion.nombreEmpresa,
                comercializacion.cotizacion.nombreDiploma,
                comercializacion.cotizacion.lugarRealizacion,
                comercializacion.cotizacion.codigoSence,
                comercializacion.cotizacion.cantidadParticipante,
                comercializacion.cotizacion.curso.nombreCurso,
                comercializacion.cotizacion.curso.codigoCurso,
                tipoEjecucion,
                comercializacion.cotizacion.modalidad,
                sucursal = comercializacion.cotizacion.sucursal.nombre,
                comercializacion.senceNet,
                participantes,
                nombreCliente,
                runCliente,
                nombreVendedor = comercializacion.usuarioCreador.nombreCompleto,
                runVendedor = comercializacion.usuarioCreador.run,
                nombreMandante, //=comercializacion.cotizacion.cliente.mandante.nombreMandante??"",
                firma
            };
            return data;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult GenerarR22(int? id)
        {
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r22")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r22\" y tipo \"word\".");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            if (comercializacion.cotizacion.sucursal.firmaAdministrador == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la firma del Administrador de Sucursal.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            return RedirectToAction("GenerarReporteR22", new { id });
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [EnableJsReport()]
        public async Task<ActionResult> GenerarReporteR22(int? id)
        {
            var comercializacion = db.Comercializacion.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r22")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
            var firma = await Files.BajarArchivoBytesAsync(comercializacion.cotizacion.sucursal.firmaAdministrador);
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
                .Configure((r) => r.Data = DataR22(comercializacion, "data:image/png;base64," + firmaBase64))
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"r22_" + comercializacion.cotizacion.codigoCotizacion + ".docx\"");
            return null;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/", "/ClienteContacto/" })]
        public ActionResult GenerarPdfR22(int? id, string id2)
        {
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r22")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (id2 == "cliente")
            {
                if (template == null || comercializacion.cotizacion.sucursal.firmaAdministrador == null)
                {
                    // indicar q hubo un error
                    return RedirectToAction("LandingPage", "ClienteContacto", new { error = "No se pudo generar el documento." });
                }
                //if (bloqueadoOCPendiente(comercializacion))
                //{
                //    // indicar q hubo un error
                //    return RedirectToAction("LandingPage", "ClienteContacto", new { error = "Bloqueado por OC pendiente." });
                //}
            }
            else
            {
                if (template == null)
                {
                    // indicar q hubo un error
                    ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r22\" y tipo \"word\".");
                    ViewBag.templatesR50 = GetTemplatesR50();
                    return View("Index", db.Comercializacion
                        .Where(x => x.softDelete == false)
                        .ToList());
                }
                if (comercializacion.cotizacion.sucursal.firmaAdministrador == null)
                {
                    // indicar q hubo un error
                    ModelState.AddModelError("", "No se encontro la firma del Administrador de Sucursal.");
                    ViewBag.templatesR50 = GetTemplatesR50();
                    return View("Index", db.Comercializacion
                        .Where(x => x.softDelete == false)
                        .ToList());
                }
            }

            string hash = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                hash = Utils.Utils.GetHash(sha256Hash, DateTime.Now.ToString());
            }

            string createRequest = Url.Action("GenerarReportePdfR22", "Comercializacions", new { id, id2 = hash }, Request.Url.Scheme);
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
            Response.AppendHeader("Content-Disposition", "attachment; filename=\"r22_" + comercializacion.cotizacion.codigoCotizacion + ".pdf\"");

            return new FileContentResult(bytes, "application/pdf");
        }

        [EnableJsReport()]
        public async Task<ActionResult> GenerarReportePdfR22(int? id, string id2)
        {
            var comercializacion = db.Comercializacion.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r22")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
            var firma = await Files.BajarArchivoBytesAsync(comercializacion.cotizacion.sucursal.firmaAdministrador);
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
                .Configure((r) => r.Data = DataR22(comercializacion, "data:image/png;base64," + firmaBase64))
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

        // ------------------------------ R24 ---------------------------------

        public object DataR24(Comercializacion comercializacion, string firma, string type)
        {
            var participantes = new List<object>();
            int numero = 0;

            if (type.Equals("sence"))
            {
                comercializacion.participantes = comercializacion.participantes.Where(x => x.conSence).ToList();
            }
            else if (type.Equals("nosence"))
            {
                comercializacion.participantes = comercializacion.participantes.Where(x => !x.conSence).ToList();
            }

            foreach (var item in comercializacion.participantes)
            {
                var asistencia = "-";
                var cantBloques = comercializacion.bloques.Count();
                var cantAsistencias = item.asistencia.Where(x => x.asistio == true).Count();
                if (cantBloques > 0)
                {
                    //if (cantAsistencias * 100 / cantBloques == 0)
                    //    continue;
                    asistencia = String.Format("{0:N0}", cantAsistencias * 100 / cantBloques) + "%";

                }
                var nota = 0.0;
                var notaTeorica = 0.0;
                var contTeorica = 0;
                foreach (var evaluacion in comercializacion.evaluaciones)
                {
                    if (evaluacion.categoria == CategoriaEvaluacion.Teorico)
                    {
                        if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault() != null)
                        {
                            if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != ""
                                && item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != "-")
                            {
                                notaTeorica += double.Parse(item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota);
                            }
                        }
                        contTeorica++;
                    }
                }
                if (contTeorica > 0)
                {
                    notaTeorica = notaTeorica / contTeorica;
                }
                var notaPractica = 0.0;
                var contPractica = 0;
                foreach (var evaluacion in comercializacion.evaluaciones)
                {
                    if (evaluacion.categoria == CategoriaEvaluacion.Practico)
                    {
                        if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault() != null)
                        {
                            if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != ""
                                && item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != "-")
                            {
                                notaPractica += double.Parse(item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota);
                            }
                        }
                        contPractica++;
                    }
                }
                if (contPractica > 0)
                {
                    notaPractica = notaPractica / contPractica;
                }
                if (comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Teorico).Count() > 0
                    && comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Practico).Count() > 0)
                {
                    nota = (notaTeorica + notaPractica) / 2;
                }
                else
                {
                    if (comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Teorico).Count() > 0)
                    {
                        nota = notaTeorica;
                    }
                    else
                    {
                        if (comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Practico).Count() > 0)
                        {
                            nota = notaPractica;
                        }
                    }
                }
                numero++;
                var participante = new
                {
                    numero,
                    item.contacto.nombres,
                    item.contacto.apellidoPaterno,
                    item.contacto.apellidoMaterno,
                    item.contacto.run,
                    empresa = comercializacion.cotizacion.nombreEmpresa,
                    item.contacto.correo,
                    item.contacto.telefono,
                    asistencia,
                    notaPractica,
                    notaTeorica,
                    nota = String.Format("{0:N1}", nota == 0 ? 1 : nota)
                };
                participantes.Add(participante);
            }
            var r11 = db.R11.Where(x => x.idCurso == comercializacion.cotizacion.curso.idCurso).FirstOrDefault();
            var mandante = comercializacion.cotizacion.nombreEmpresa;
            if (comercializacion.cotizacion.cliente.mandante != null)
            {
                mandante = comercializacion.cotizacion.cliente.mandante.nombreMandante;
            }
            CultureInfo culture = new CultureInfo("es");
            comercializacion.cotizacion.cantidadParticipante = participantes.Count();
            var data = new
            {
                fecha = DateTime.Now.ToString("dd \"de\" MMMM \"de\" yyyy", culture),
                fechaEmision = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                fechaInicio = comercializacion.fechaInicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                fechaTermino = comercializacion.fechaTermino.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                comercializacion.ciudad.nombreCiudad,
                comercializacion.cotizacion.codigoCotizacion,
                comercializacion.cotizacion.nombreEmpresa,
                comercializacion.cotizacion.nombreDiploma,
                comercializacion.cotizacion.lugarRealizacion,
                comercializacion.cotizacion.codigoSence,
                comercializacion.cotizacion.cantidadParticipante,
                comercializacion.cotizacion.curso.nombreCurso,
                comercializacion.cotizacion.curso.codigoCurso,
                comercializacion.cotizacion.modalidad,
                sucursal = comercializacion.cotizacion.sucursal.nombre,
                comercializacion.senceNet,
                participantes,
                nombreVendedor = comercializacion.usuarioCreador.nombreCompleto,
                runVendedor = comercializacion.usuarioCreador.run,
                mandante,
                horas = String.Format("{0:0.#}", r11.horasPracticas + r11.horasTeoricas),
                firma,
                administradorSucursal = comercializacion.cotizacion.sucursal.nombreAdministrador
            };
            return data;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult GenerarR24(int? id)
        {
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r24")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r24\" y tipo \"word\".");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            if (comercializacion.cotizacion.sucursal.firmaAdministrador == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la firma del Administrador de Sucursal.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            return RedirectToAction("GenerarReporteR24", new { id });
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [EnableJsReport()]
        public async Task<ActionResult> GenerarReporteR24(int? id)
        {
            var comercializacion = db.Comercializacion.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r24")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
            var firma = await Files.BajarArchivoBytesAsync(comercializacion.cotizacion.sucursal.firmaAdministrador);
            var firmaBase64 = Convert.ToBase64String(firma, 0, firma.Length);

            var elerning = "";
            if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono || comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono)
            {
                elerning = " (E)";
            }
            var nombreArchivo = "R24 " + elerning + " " + comercializacion.cotizacion.nombreDiploma + " " + comercializacion.fechaInicio.ToString("dd-MM-yyyy") + " (" + comercializacion.cotizacion.codigoCotizacion + ")";

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
                .Configure((r) => r.Data = DataR24(comercializacion, "data:image/png;base64," + firmaBase64, "all"))
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"" + nombreArchivo + ".docx\"");
            return null;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/", "/ClienteContacto/" })]
        public ActionResult GenerarPdfR24(int? id, string type, string id2)
        {
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r24")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (id2 == "cliente")
            {
                if (template == null || comercializacion.cotizacion.sucursal.firmaAdministrador == null)
                {
                    // indicar q hubo un error
                    return RedirectToAction("LandingPage", "ClienteContacto", new { error = "No se pudo generar el documento." });
                }
                //if (bloqueadoOCPendiente(comercializacion))
                //{
                //    // indicar q hubo un error
                //    return RedirectToAction("LandingPage", "ClienteContacto", new { error = "Bloqueado por OC pendiente." });
                //}
            }
            else
            {
                if (template == null)
                {
                    // indicar q hubo un error
                    ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r24\" y tipo \"word\".");
                    ViewBag.templatesR50 = GetTemplatesR50();
                    return View("Index", db.Comercializacion
                        .Where(x => x.softDelete == false)
                        .ToList());
                }
                if (comercializacion.cotizacion.sucursal.firmaAdministrador == null)
                {
                    // indicar q hubo un error
                    ModelState.AddModelError("", "No se encontro la firma del Administrador de Sucursal.");
                    ViewBag.templatesR50 = GetTemplatesR50();
                    return View("Index", db.Comercializacion
                        .Where(x => x.softDelete == false)
                        .ToList());
                }
            }

            string hash = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                hash = Utils.Utils.GetHash(sha256Hash, DateTime.Now.ToString());
            }

            string createRequest = Url.Action("GenerarReportePdfR24", "Comercializacions", new { id, id2 = hash, type }, Request.Url.Scheme);
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

            var elerning = "";
            if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono || comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono)
            {
                elerning = " (E)";
            }
            type = type.Equals("all") ? "_todos" : "_" + type;
            var nombreArchivo = "\"R24 " + elerning + " " + comercializacion.cotizacion.nombreDiploma + " " + comercializacion.fechaInicio.ToString("dd-MM-yyyy") + " (" + comercializacion.cotizacion.codigoCotizacion + ")" + type + ".pdf\"";
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + nombreArchivo);

            return new FileContentResult(bytes, "application/pdf");
        }

        [EnableJsReport()]
        public async Task<ActionResult> GenerarReportePdfR24(int? id, string id2, string type)
        {
            var comercializacion = db.Comercializacion.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r24")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
            var firma = await Files.BajarArchivoBytesAsync(comercializacion.cotizacion.sucursal.firmaAdministrador);
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
                .Configure((r) => r.Data = DataR24(comercializacion, "data:image/png;base64," + firmaBase64, type))
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

        // ------------------------------ R50 ---------------------------------

        public object DataR50(Comercializacion comercializacion, string firma)
        {
            var participantes = new List<object>();
            int numero = 0;
            var labels = new List<string>();
            var cfs = new List<string>();
            var cis = new List<string>();
            foreach (var item in comercializacion.participantes)
            {
                var asistencia = "-";
                var cantBloques = comercializacion.bloques.Count();
                var cantAsistencias = item.asistencia.Where(x => x.asistio == true).Count();
                if (cantBloques > 0)
                {
                    //if (cantAsistencias * 100 / cantBloques == 0)
                    //    continue;
                    asistencia = String.Format("{0:N0}", cantAsistencias * 100 / cantBloques) + "%";
                }
                var nota = 0.0;
                var notaTeorica = 0.0;
                var contTeorica = 0;
                foreach (var evaluacion in comercializacion.evaluaciones)
                {
                    if (evaluacion.categoria == CategoriaEvaluacion.Teorico)
                    {
                        if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault() != null)
                        {
                            if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != ""
                                && item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != "-")
                            {
                                notaTeorica += double.Parse(item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota);
                            }
                        }
                        contTeorica++;
                    }
                }
                if (contTeorica > 0)
                {
                    notaTeorica = notaTeorica / contTeorica;
                }
                var notaPractica = 0.0;
                var contPractica = 0;
                foreach (var evaluacion in comercializacion.evaluaciones)
                {
                    if (evaluacion.categoria == CategoriaEvaluacion.Practico)
                    {
                        if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault() != null)
                        {
                            if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != ""
                                && item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != "-")
                            {
                                notaPractica += double.Parse(item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota);
                            }
                        }
                        contPractica++;
                    }
                }
                if (contPractica > 0)
                {
                    notaPractica = notaPractica / contPractica;
                }
                var notaDiagnostico = 0.0;
                var contDiagnostico = 0;
                foreach (var evaluacion in comercializacion.evaluaciones)
                {
                    if (evaluacion.categoria == CategoriaEvaluacion.Diagnostico)
                    {
                        if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault() != null)
                        {
                            if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != ""
                                && item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != "-")
                            {
                                notaDiagnostico += double.Parse(item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota);
                            }
                        }
                        contDiagnostico++;
                    }
                }
                if (contDiagnostico > 0)
                {
                    notaDiagnostico = notaDiagnostico / contDiagnostico;
                }
                if (comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Teorico).Count() > 0
                    && comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Practico).Count() > 0)
                {
                    nota = (notaTeorica + notaPractica) / 2;
                }
                else
                {
                    if (comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Teorico).Count() > 0)
                    {
                        nota = notaTeorica;
                    }
                    else
                    {
                        if (comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Practico).Count() > 0)
                        {
                            nota = notaPractica;
                        }
                    }
                }
                var conocimientoInicial = notaDiagnostico * 100 / 7;
                var conocimientoFinal = nota * 100 / 7;
                var avance = conocimientoFinal - conocimientoInicial;
                numero++;
                var participante = new
                {
                    n = numero,
                    item.contacto.nombres,
                    item.contacto.apellidoPaterno,
                    item.contacto.apellidoMaterno,
                    item.contacto.run,
                    empresa = comercializacion.cotizacion.nombreEmpresa,
                    item.contacto.correo,
                    item.contacto.telefono,
                    asistencia,
                    np = String.Format("{0:N1}", notaPractica == 0 ? 1 : notaPractica),
                    nt = String.Format("{0:N1}", notaTeorica == 0 ? 1 : notaTeorica),
                    nd = String.Format("{0:N1}", notaDiagnostico == 0 ? 1 : notaDiagnostico),
                    nf = String.Format("{0:N1}", nota == 0 ? 1 : nota),
                    ci = String.Format("{0:N0}", conocimientoInicial) + "%",
                    cf = String.Format("{0:N0}", conocimientoFinal) + "%",
                    a = String.Format("{0:N0}", avance) + "%"
                };
                participantes.Add(participante);
                labels.Add(item.contacto.nombreCompleto);
                cis.Add(String.Format("{0:N0}", conocimientoInicial));
                cfs.Add(String.Format("{0:N0}", conocimientoFinal));
            }

            var datasets = new List<object>();
            datasets.Add(new
            {
                label = "Conocimiento Inicial",
                data = cis
            });
            datasets.Add(new
            {
                label = "Conocimiento Final",
                data = cfs
            });
            if (labels.Count() == 0)
            {
                labels.Add("");
            }
            var graficoAvance = new
            {
                labels,
                datasets
            };
            var comentarioInstructor = "";
            if (comercializacion.comentarioInstructor != null)
            {
                comentarioInstructor = comercializacion.comentarioInstructor;
            }
            var comentarioOtec = "";
            if (comercializacion.comentarioOtec != null) { comentarioOtec = comercializacion.comentarioOtec; };
            var mandante = comercializacion.cotizacion.nombreEmpresa;
            if (comercializacion.cotizacion.cliente.mandante != null) { mandante = comercializacion.cotizacion.cliente.mandante.nombreMandante; };

            var r11 = db.R11.Where(x => x.idCurso == comercializacion.cotizacion.curso.idCurso).FirstOrDefault();
            CultureInfo culture = new CultureInfo("es");
            var data = new
            {
                fecha = DateTime.Now.ToString("dd \"de\" MMMM \"de\" yyyy", culture),
                fechaEmision = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                fechaInicio = comercializacion.fechaInicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                fechaTermino = comercializacion.fechaTermino.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                comercializacion.ciudad.nombreCiudad,
                comercializacion.cotizacion.codigoCotizacion,
                comercializacion.cotizacion.nombreEmpresa,
                comercializacion.cotizacion.nombreDiploma,
                comercializacion.cotizacion.lugarRealizacion,
                comercializacion.cotizacion.codigoSence,
                cantParticip = participantes.Count(),
                comercializacion.cotizacion.curso.nombreCurso,
                comercializacion.cotizacion.curso.codigoCurso,
                comercializacion.cotizacion.modalidad,
                sucursal = comercializacion.cotizacion.sucursal.nombre,
                comercializacion.senceNet,
                participantes,
                nombreVendedor = comercializacion.usuarioCreador.nombreCompleto,
                runVendedor = comercializacion.usuarioCreador.run,
                mandante,
                horas = String.Format("{0:0.#}", r11.horasPracticas + r11.horasTeoricas),
                r11.objetivoGeneral,
                r11.tecnicaMetodologica,
                firma,
                administradorSucursal = comercializacion.cotizacion.sucursal.nombreAdministrador,
                comentarioInstructor,
                comentarioOtec,
                avanceParticipantes = graficoAvance
            };
            return data;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerarR50(int? id, string id2)
        {
            if (id2 == null || id2 == "")
            {
                id2 = "r50";
            }
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == id2)
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r50\" y tipo \"word\".");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            if (comercializacion.cotizacion.sucursal.firmaAdministrador == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la firma del Administrador de Sucursal.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            return RedirectToAction("GenerarReporteR50", new { id, id2 });
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [EnableJsReport()]
        public async Task<ActionResult> GenerarReporteR50(int? id, string id2)
        {
            var comercializacion = db.Comercializacion.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == id2)
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
            var firma = await Files.BajarArchivoBytesAsync(comercializacion.cotizacion.sucursal.firmaAdministrador);
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
                .Configure((r) => r.Data = DataR50(comercializacion, "data:image/png;base64," + firmaBase64))
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"r50_" + comercializacion.cotizacion.codigoCotizacion + ".docx\"");
            return null;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/", "/ClienteContacto/" })]
        public ActionResult GenerarPdfR50(int? id, string id2, string id3)
        {
            if (id2 == null || id2 == "")
            {
                id2 = "r50";
            }
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == id2)
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (id3 == "cliente")
            {
                if (template == null || comercializacion.cotizacion.sucursal.firmaAdministrador == null)
                {
                    // indicar q hubo un error
                    return RedirectToAction("LandingPage", "ClienteContacto", new { error = "No se pudo generar el documento." });
                }
                //if (bloqueadoOCPendiente(comercializacion))
                //{
                //    // indicar q hubo un error
                //    return RedirectToAction("LandingPage", "ClienteContacto", new { error = "Bloqueado por OC pendiente." });
                //}
            }
            else
            {
                if (template == null)
                {
                    // indicar q hubo un error
                    ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r50\" y tipo \"word\".");
                    ViewBag.templatesR50 = GetTemplatesR50();
                    return View("Index", db.Comercializacion
                        .Where(x => x.softDelete == false)
                        .ToList());
                }
                if (comercializacion.cotizacion.sucursal.firmaAdministrador == null)
                {
                    // indicar q hubo un error
                    ModelState.AddModelError("", "No se encontro la firma del Administrador de Sucursal.");
                    ViewBag.templatesR50 = GetTemplatesR50();
                    return View("Index", db.Comercializacion
                        .Where(x => x.softDelete == false)
                        .ToList());
                }
            }

            string hash = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                hash = Utils.Utils.GetHash(sha256Hash, DateTime.Now.ToString());
            }

            string createRequest = Url.Action("GenerarReportePdfR50", "Comercializacions", new { id, id2 = hash, id3 = id2 }, Request.Url.Scheme);
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
            Response.AppendHeader("Content-Disposition", "attachment; filename=\"r50_" + comercializacion.cotizacion.codigoCotizacion + ".pdf\"");

            return new FileContentResult(bytes, "application/pdf");
        }

        [EnableJsReport()]
        public async Task<ActionResult> GenerarReportePdfR50(int? id, string id2, string id3)
        {
            var comercializacion = db.Comercializacion.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == id3)
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
            var firma = await Files.BajarArchivoBytesAsync(comercializacion.cotizacion.sucursal.firmaAdministrador);
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
                .Configure((r) => r.Data = DataR50(comercializacion, "data:image/png;base64," + firmaBase64))
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

        // ------------------------------ R17 ---------------------------------

        public object DataR17(Comercializacion comercializacion, int idRelator, string firmaRelator, string firmaAdministrador)
        {
            var horas = 0.0;
            var bloques = new List<object>();
            foreach (var item in comercializacion.bloques)
            {
                if (item.relator.idRelator == idRelator)
                {
                    bloques.Add(new
                    {
                        fecha = item.fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                        horarioInicio = item.horarioInicio.ToString("HH:mm", CultureInfo.InvariantCulture),
                        horarioTermino = item.horarioTermino.ToString("HH:mm", CultureInfo.InvariantCulture),
                        item.sala,
                        item.lugarAlmuerzo
                    });
                }
                TimeSpan tiempo = item.horarioTermino - item.horarioInicio;
                horas += tiempo.TotalHours;
            }
            var variosBloques = "";
            if (comercializacion.bloques.Count() > 1)
            {
                variosBloques = "Sí";
            }
            comercializacion.cotizacion.costo = db.Costo.Where(x => x.idCotizacion == comercializacion.cotizacion.idCotizacion_R13).ToList();
            var valor = comercializacion.cotizacion.costo.Where(x => x.detalle.ToLower() == "relator").FirstOrDefault().valor;
            var valorPalabras = Utils.Utils.NumeroALetras(valor);
            var relator = db.Relators.Find(idRelator);
            var tipo = "";
            var noRecertificacion = "";
            var presencial = "";
            var elerning = "";
            var recertificacion = "";
            if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Sincronica
                || comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica
                || comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion)
            {
                tipo = "recertificación";
                recertificacion = "Sí";
            }
            else
            {
                noRecertificacion = "Sí";
                tipo = "hora de instrucción";
            }
            if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono
                || comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono)
            {
                elerning = "Sí";
            }
            if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Presencial)
            {
                presencial = "Sí";
            }
            String lugarRealizacion = "";
            if (comercializacion.cotizacion.lugarRealizacion != null)
            {
                lugarRealizacion = comercializacion.cotizacion.lugarRealizacion;
            }
            var culture = new CultureInfo("es");
            string senceNET = comercializacion.senceNet == null ? "" : comercializacion.senceNet;
            string nombreMandante = comercializacion.cotizacion.cliente.mandante == null ? comercializacion.cotizacion.cliente.razonSocial : comercializacion.cotizacion.cliente.mandante.nombreMandante;
            string direccion = relator.contacto.direccion == null ? "" : relator.contacto.direccion;
            string estadoCivil = relator.contacto.estadoCivil == null ? "" : relator.contacto.estadoCivil;
            string fechaNacimiento = relator.contacto.fechaNacimiento == null ? "" : relator.contacto.fechaNacimiento.Value.ToString("dd \"de\" MMMM \"de\" yyyy", culture);
            var data = new
            {
                fecha = DateTime.Now.ToString("dd \"de\" MMMM \"de\" yyyy", culture),
                fechaCorta = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                fechaInicio = comercializacion.fechaInicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                fechaTermino = comercializacion.fechaTermino.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                comercializacion.ciudad.nombreCiudad,
                comercializacion.cotizacion.codigoCotizacion,
                comercializacion.cotizacion.nombreEmpresa,
                comercializacion.cotizacion.nombreDiploma,
                lugarRealizacion,
                cantParticip = comercializacion.cotizacion.cantidadParticipante,
                comercializacion.cotizacion.curso.nombreCurso,
                comercializacion.cotizacion.curso.codigoCurso,
                comercializacion.cotizacion.modalidad,
                senceNET,//null
                nombreVendedor = comercializacion.usuarioCreador.nombreCompleto,
                runVendedor = comercializacion.usuarioCreador.run,
                nombreMandante,//null
                firmaRelator,
                firmaAdministrador,
                sucursal = comercializacion.cotizacion.sucursal.nombre,
                comercializacion.cotizacion.sucursal.nombreAdministrador,
                comercializacion.cotizacion.sucursal.runAdministrador,
                comercializacion.cotizacion.sucursal.direccionAdministrador,
                relator.contacto.nombreCompleto,
                relator.contacto.run,
                direccion,//null
                fechaNacimiento,//null
                relator.contacto.correo,
                relator.contacto.telefono,
                relator.contacto.nombres,
                relator.contacto.apellidoPaterno,
                relator.contacto.apellidoMaterno,
                estadoCivil,//null
                relator.datosBancarios.numeroCuenta,
                tipoCuenta = relator.datosBancarios.tipoCuenta.ToString(),
                relator.datosBancarios.nombreBanco,
                bloques,
                variosBloques,
                horas,
                tipo,
                noRecertificacion,
                valor = String.Format("{0:N0}", valor),
                valorPalabras,
                presencial,
                elerning,
                recertificacion
            };
            return data;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerarR17(int? id, int id2)
        {
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r17")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r17\" y tipo \"word\".");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            if (comercializacion.cotizacion.sucursal.firmaAdministrador == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la firma del Administrador de Sucursal.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            var relator = db.Relators.Find(id2);
            if (relator.imagenFirma == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la firma del Relator.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            if (relator.datosBancarios == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la información bancaria del Relator.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            comercializacion.cotizacion.costo = db.Costo.Where(x => x.idCotizacion == comercializacion.cotizacion.idCotizacion_R13).ToList();
            var valor = comercializacion.cotizacion.costo.Where(x => x.detalle.ToLower() == "relator").FirstOrDefault();
            if (valor == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el valor del Relator.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            return RedirectToAction("GenerarReporteR17", new { id, id2 });
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [EnableJsReport()]
        public async Task<ActionResult> GenerarReporteR17(int? id, int id2)
        {
            var comercializacion = db.Comercializacion.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r17")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
            var relator = db.Relators.Find(id2);
            var firmaRelator = await Files.BajarArchivoBytesAsync(relator.imagenFirma);
            var firmaRelatorBase64 = Convert.ToBase64String(firmaRelator, 0, firmaRelator.Length);
            var firmaAdministrador = await Files.BajarArchivoBytesAsync(comercializacion.cotizacion.sucursal.firmaAdministrador);
            var firmaAdministradorBase64 = Convert.ToBase64String(firmaAdministrador, 0, firmaAdministrador.Length);
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
                .Configure((r) => r.Data = DataR17(comercializacion, id2, "data:image/png;base64," + firmaRelatorBase64, "data:image/png;base64," + firmaAdministradorBase64))
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"r17_" + comercializacion.cotizacion.codigoCotizacion + ".docx\"");
            return null;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/", "/Relator/Perfil/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerarPdfR17(int? id, int id2)
        {
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r17")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r17\" y tipo \"word\".");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            if (comercializacion.cotizacion.sucursal.firmaAdministrador == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la firma del Administrador de Sucursal.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            var relator = db.Relators.Find(id2);
            if (relator.imagenFirma == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la firma del Relator.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            if (relator.datosBancarios == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la información bancaria del Relator.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            comercializacion.cotizacion.costo = db.Costo.Where(x => x.idCotizacion == comercializacion.cotizacion.idCotizacion_R13).ToList();
            var valor = comercializacion.cotizacion.costo.Where(x => x.detalle.ToLower() == "relator").FirstOrDefault();
            if (valor == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el valor del Relator.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }

            string hash = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                hash = Utils.Utils.GetHash(sha256Hash, DateTime.Now.ToString());
            }

            string createRequest = Url.Action("GenerarReportePdfR17", "Comercializacions", new { id, id2 = hash, id3 = id2 }, Request.Url.Scheme);
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
            Response.AppendHeader("Content-Disposition", "attachment; filename=\"r17_" + comercializacion.cotizacion.codigoCotizacion + ".pdf\"");

            return new FileContentResult(bytes, "application/pdf");
        }

        [EnableJsReport()]
        public async Task<ActionResult> GenerarReportePdfR17(int? id, string id2, int id3)
        {
            var comercializacion = db.Comercializacion.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r17")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
            var relator = db.Relators.Find(id3);
            var firmaRelator = await Files.BajarArchivoBytesAsync(relator.imagenFirma);
            var firmaRelatorBase64 = Convert.ToBase64String(firmaRelator, 0, firmaRelator.Length);
            var firmaAdministrador = await Files.BajarArchivoBytesAsync(comercializacion.cotizacion.sucursal.firmaAdministrador);
            var firmaAdministradorBase64 = Convert.ToBase64String(firmaAdministrador, 0, firmaAdministrador.Length);
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
                .Configure((r) => r.Data = DataR17(comercializacion, id3, "data:image/png;base64," + firmaRelatorBase64, "data:image/png;base64," + firmaAdministradorBase64))
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

        // ------------------------------ Credenciales ---------------------------------

        // PDF

        public object DataCredenciales(Comercializacion comercializacion, string firmaAdministrador, IDictionary<int, string> fotos)
        {
            CultureInfo culture = new CultureInfo("es");
            var r11 = db.R11.Where(r => r.idCurso == comercializacion.cotizacion.idCurso).FirstOrDefault();
            var participantes = new List<object>();
            var nombreCurso = comercializacion.cotizacion.curso.nombreCurso;

            if (comercializacion.cotizacion.tipoCurso.ToLower().Contains("recertificación"))
            {
                nombreCurso = nombreCurso.Replace("Recertificación de ", "");
                comercializacion.cotizacion.nombreDiploma = comercializacion.cotizacion.nombreDiploma.Replace("Recertificación de ", "");
            }
            comercializacion.participantes = comercializacion.participantes.Where(x => x.credenciales != null).ToList();
            for (var i = 0; i < comercializacion.participantes.Count(); i++)
            {
                // primer participante
                // genera qr para verificacion credenciales
                var writer = new BarcodeWriter { Format = BarcodeFormat.QR_CODE };
                var qr = writer.Write(Url.Action("Validar", "ValidarCredenciales", new { id = comercializacion.cotizacion.codigoCotizacion, id2 = comercializacion.participantes.ElementAt(i).contacto.run }, Request.Url.Scheme));
                var qrByteArray = Utils.Utils.ImageToByte2(qr);
                var qrBase64 = Convert.ToBase64String(qrByteArray, 0, qrByteArray.Length);
                // datos participante
                var participanteA = new
                {
                    numero = i,
                    comercializacion.participantes.ElementAt(i).contacto.nombreCompleto,
                    comercializacion.participantes.ElementAt(i).contacto.nombres,
                    comercializacion.participantes.ElementAt(i).contacto.apellidoPaterno,
                    comercializacion.participantes.ElementAt(i).contacto.apellidoMaterno,
                    comercializacion.participantes.ElementAt(i).contacto.run,
                    comercializacion.participantes.ElementAt(i).contacto.correo,
                    comercializacion.participantes.ElementAt(i).contacto.telefono,
                    fecha = DateTime.Now.ToString("dd-MM-yy", culture),
                    fechaInicio = comercializacion.fechaInicio.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                    fechaTermino = comercializacion.fechaTermino.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                    comercializacion.ciudad.nombreCiudad,
                    comercializacion.cotizacion.codigoCotizacion,
                    comercializacion.cotizacion.nombreEmpresa,
                    comercializacion.cotizacion.nombreDiploma,
                    comercializacion.cotizacion.lugarRealizacion,
                    comercializacion.cotizacion.codigoSence,
                    nombreCurso = nombreCurso.ToUpper(),
                    comercializacion.cotizacion.curso.codigoCurso,
                    vigencia = comercializacion.fechaTermino.AddMonths(comercializacion.vigenciaCredenciales).ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                    //vigencia = comercializacion.vigenciaCredenciales.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                    horas = String.Format("{0:0.#}", r11.horasPracticas + r11.horasTeoricas),
                    qr = "data:image/png;base64," + qrBase64,
                    foto = fotos[comercializacion.participantes.ElementAt(i).idParticipante],
                    firma = firmaAdministrador,
                    existe = "Sí"
                };
                var sinParticipante = new
                {
                    numero = i,
                    comercializacion.participantes.ElementAt(i).contacto.nombreCompleto,
                    comercializacion.participantes.ElementAt(i).contacto.nombres,
                    comercializacion.participantes.ElementAt(i).contacto.apellidoPaterno,
                    comercializacion.participantes.ElementAt(i).contacto.apellidoMaterno,
                    comercializacion.participantes.ElementAt(i).contacto.run,
                    comercializacion.participantes.ElementAt(i).contacto.correo,
                    comercializacion.participantes.ElementAt(i).contacto.telefono,
                    fecha = DateTime.Now.ToString("dd-MM-yy", culture),
                    fechaInicio = comercializacion.fechaInicio.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                    fechaTermino = comercializacion.fechaTermino.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                    comercializacion.ciudad.nombreCiudad,
                    comercializacion.cotizacion.codigoCotizacion,
                    comercializacion.cotizacion.nombreEmpresa,
                    comercializacion.cotizacion.nombreDiploma,
                    comercializacion.cotizacion.lugarRealizacion,
                    comercializacion.cotizacion.codigoSence,
                    nombreCurso = nombreCurso.ToUpper(),
                    comercializacion.cotizacion.curso.codigoCurso,
                    vigencia = comercializacion.fechaTermino.AddMonths(comercializacion.vigenciaCredenciales).ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                    //vigencia = comercializacion.vigenciaCredenciales.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                    horas = String.Format("{0:0.#}", r11.horasPracticas + r11.horasTeoricas),
                    qr = "data:image/png;base64," + qrBase64,
                    foto = fotos[comercializacion.participantes.ElementAt(i).idParticipante],
                    firma = firmaAdministrador,
                    existe = ""
                };
                // segundo participante
                i++;
                var participanteB = new object();
                if (comercializacion.participantes.Count() > i)
                {
                    // genera qr para verificacion credenciales
                    writer = new BarcodeWriter { Format = BarcodeFormat.QR_CODE };
                    qr = writer.Write(Url.Action("Validar", "ValidarCredenciales", new { id = comercializacion.cotizacion.codigoCotizacion, id2 = comercializacion.participantes.ElementAt(i).contacto.run }, Request.Url.Scheme));
                    qrByteArray = Utils.Utils.ImageToByte2(qr);
                    qrBase64 = Convert.ToBase64String(qrByteArray, 0, qrByteArray.Length);
                    // datos participante
                    participanteB = new
                    {
                        numero = i,
                        comercializacion.participantes.ElementAt(i).contacto.nombreCompleto,
                        comercializacion.participantes.ElementAt(i).contacto.nombres,
                        comercializacion.participantes.ElementAt(i).contacto.apellidoPaterno,
                        comercializacion.participantes.ElementAt(i).contacto.apellidoMaterno,
                        comercializacion.participantes.ElementAt(i).contacto.run,
                        comercializacion.participantes.ElementAt(i).contacto.correo,
                        comercializacion.participantes.ElementAt(i).contacto.telefono,
                        fecha = DateTime.Now.ToString("dd-MM-yy", culture),
                        fechaInicio = comercializacion.fechaInicio.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                        fechaTermino = comercializacion.fechaTermino.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                        comercializacion.ciudad.nombreCiudad,
                        comercializacion.cotizacion.codigoCotizacion,
                        comercializacion.cotizacion.nombreEmpresa,
                        comercializacion.cotizacion.nombreDiploma,
                        comercializacion.cotizacion.lugarRealizacion,
                        comercializacion.cotizacion.codigoSence,
                        nombreCurso = nombreCurso.ToUpper(),
                        comercializacion.cotizacion.curso.codigoCurso,
                        vigencia = comercializacion.fechaTermino.AddMonths(comercializacion.vigenciaCredenciales).ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                        //vigencia = comercializacion.vigenciaCredenciales.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                        horas = String.Format("{0:0.#}", r11.horasPracticas + r11.horasTeoricas),
                        qr = "data:image/png;base64," + qrBase64,
                        foto = fotos[comercializacion.participantes.ElementAt(i).idParticipante],
                        firma = firmaAdministrador,
                        existe = "Sí"
                    };
                }
                else
                {
                    participanteB = sinParticipante;
                }
                // terver participante
                i++;
                var participanteC = new object();
                if (comercializacion.participantes.Count() > i)
                {
                    // genera qr para verificacion credenciales
                    writer = new BarcodeWriter { Format = BarcodeFormat.QR_CODE };
                    qr = writer.Write(Url.Action("Validar", "ValidarCredenciales", new { id = comercializacion.cotizacion.codigoCotizacion, id2 = comercializacion.participantes.ElementAt(i).contacto.run }, Request.Url.Scheme));
                    qrByteArray = Utils.Utils.ImageToByte2(qr);
                    qrBase64 = Convert.ToBase64String(qrByteArray, 0, qrByteArray.Length);
                    // datos participante
                    participanteC = new
                    {
                        numero = i,
                        comercializacion.participantes.ElementAt(i).contacto.nombreCompleto,
                        comercializacion.participantes.ElementAt(i).contacto.nombres,
                        comercializacion.participantes.ElementAt(i).contacto.apellidoPaterno,
                        comercializacion.participantes.ElementAt(i).contacto.apellidoMaterno,
                        comercializacion.participantes.ElementAt(i).contacto.run,
                        comercializacion.participantes.ElementAt(i).contacto.correo,
                        comercializacion.participantes.ElementAt(i).contacto.telefono,
                        fecha = DateTime.Now.ToString("dd-MM-yy", culture),
                        fechaInicio = comercializacion.fechaInicio.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                        fechaTermino = comercializacion.fechaTermino.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                        comercializacion.ciudad.nombreCiudad,
                        comercializacion.cotizacion.codigoCotizacion,
                        comercializacion.cotizacion.nombreEmpresa,
                        comercializacion.cotizacion.nombreDiploma,
                        comercializacion.cotizacion.lugarRealizacion,
                        comercializacion.cotizacion.codigoSence,
                        nombreCurso = nombreCurso.ToUpper(),
                        comercializacion.cotizacion.curso.codigoCurso,
                        vigencia = comercializacion.fechaTermino.AddMonths(comercializacion.vigenciaCredenciales).ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                        //vigencia = comercializacion.vigenciaCredenciales.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                        horas = String.Format("{0:0.#}", r11.horasPracticas + r11.horasTeoricas),
                        qr = "data:image/png;base64," + qrBase64,
                        foto = fotos[comercializacion.participantes.ElementAt(i).idParticipante],
                        firma = firmaAdministrador,
                        existe = "Sí"
                    };
                }
                else
                {
                    participanteC = sinParticipante;
                }
                // cuarto participante
                i++;
                var participanteD = new object();
                if (comercializacion.participantes.Count() > i)
                {
                    // genera qr para verificacion credenciales
                    writer = new BarcodeWriter { Format = BarcodeFormat.QR_CODE };
                    qr = writer.Write(Url.Action("Validar", "ValidarCredenciales", new { id = comercializacion.cotizacion.codigoCotizacion, id2 = comercializacion.participantes.ElementAt(i).contacto.run }, Request.Url.Scheme));
                    qrByteArray = Utils.Utils.ImageToByte2(qr);
                    qrBase64 = Convert.ToBase64String(qrByteArray, 0, qrByteArray.Length);
                    // datos participante
                    participanteD = new
                    {
                        numero = i,
                        comercializacion.participantes.ElementAt(i).contacto.nombreCompleto,
                        comercializacion.participantes.ElementAt(i).contacto.nombres,
                        comercializacion.participantes.ElementAt(i).contacto.apellidoPaterno,
                        comercializacion.participantes.ElementAt(i).contacto.apellidoMaterno,
                        comercializacion.participantes.ElementAt(i).contacto.run,
                        comercializacion.participantes.ElementAt(i).contacto.correo,
                        comercializacion.participantes.ElementAt(i).contacto.telefono,
                        fecha = DateTime.Now.ToString("dd-MM-yy", culture),
                        fechaInicio = comercializacion.fechaInicio.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                        fechaTermino = comercializacion.fechaTermino.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                        comercializacion.ciudad.nombreCiudad,
                        comercializacion.cotizacion.codigoCotizacion,
                        comercializacion.cotizacion.nombreEmpresa,
                        comercializacion.cotizacion.nombreDiploma,
                        comercializacion.cotizacion.lugarRealizacion,
                        comercializacion.cotizacion.codigoSence,
                        nombreCurso = nombreCurso.ToUpper(),
                        comercializacion.cotizacion.curso.codigoCurso,
                        vigencia = comercializacion.fechaTermino.AddMonths(comercializacion.vigenciaCredenciales).ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                        //vigencia = comercializacion.vigenciaCredenciales.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                        horas = String.Format("{0:0.#}", r11.horasPracticas + r11.horasTeoricas),
                        qr = "data:image/png;base64," + qrBase64,
                        foto = fotos[comercializacion.participantes.ElementAt(i).idParticipante],
                        firma = firmaAdministrador,
                        existe = "Sí"
                    };
                }
                else
                {
                    participanteD = sinParticipante;
                }
                // quinto participante
                i++;
                var participanteE = new object();
                if (comercializacion.participantes.Count() > i)
                {
                    // genera qr para verificacion credenciales
                    writer = new BarcodeWriter { Format = BarcodeFormat.QR_CODE };
                    qr = writer.Write(Url.Action("Validar", "ValidarCredenciales", new { id = comercializacion.cotizacion.codigoCotizacion, id2 = comercializacion.participantes.ElementAt(i).contacto.run }, Request.Url.Scheme));
                    qrByteArray = Utils.Utils.ImageToByte2(qr);
                    qrBase64 = Convert.ToBase64String(qrByteArray, 0, qrByteArray.Length);
                    // datos participante
                    participanteE = new
                    {
                        numero = i,
                        comercializacion.participantes.ElementAt(i).contacto.nombreCompleto,
                        comercializacion.participantes.ElementAt(i).contacto.nombres,
                        comercializacion.participantes.ElementAt(i).contacto.apellidoPaterno,
                        comercializacion.participantes.ElementAt(i).contacto.apellidoMaterno,
                        comercializacion.participantes.ElementAt(i).contacto.run,
                        comercializacion.participantes.ElementAt(i).contacto.correo,
                        comercializacion.participantes.ElementAt(i).contacto.telefono,
                        fecha = DateTime.Now.ToString("dd-MM-yy", culture),
                        fechaInicio = comercializacion.fechaInicio.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                        fechaTermino = comercializacion.fechaTermino.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                        comercializacion.ciudad.nombreCiudad,
                        comercializacion.cotizacion.codigoCotizacion,
                        comercializacion.cotizacion.nombreEmpresa,
                        comercializacion.cotizacion.nombreDiploma,
                        comercializacion.cotizacion.lugarRealizacion,
                        comercializacion.cotizacion.codigoSence,
                        nombreCurso = nombreCurso.ToUpper(),
                        comercializacion.cotizacion.curso.codigoCurso,
                        vigencia = comercializacion.fechaTermino.AddMonths(comercializacion.vigenciaCredenciales).ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                        //vigencia = comercializacion.vigenciaCredenciales.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                        horas = String.Format("{0:0.#}", r11.horasPracticas + r11.horasTeoricas),
                        qr = "data:image/png;base64," + qrBase64,
                        foto = fotos[comercializacion.participantes.ElementAt(i).idParticipante],
                        firma = firmaAdministrador,
                        existe = "Sí"
                    };
                }
                else
                {
                    participanteE = sinParticipante;
                }
                // data
                participantes.Add(new
                {
                    a = participanteA,
                    b = participanteB,
                    c = participanteC,
                    d = participanteD,
                    e = participanteE
                });
            }
            // ver si es con sence
            var codigoSence = r11.codigoSence;
            var sence = "";
            if (codigoSence != null && comercializacion.cotizacion.tieneCodigoSence != "on")
            {
                sence = "SENCE";
            }
            // data
            var data = new
            {
                fecha = DateTime.Now.ToString("dd MMM yy", culture),
                fechaInicio = comercializacion.fechaInicio.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                fechaTermino = comercializacion.fechaTermino.ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                comercializacion.ciudad.nombreCiudad,
                comercializacion.cotizacion.codigoCotizacion,
                comercializacion.cotizacion.nombreEmpresa,
                comercializacion.cotizacion.nombreDiploma,
                comercializacion.cotizacion.lugarRealizacion,
                comercializacion.cotizacion.codigoSence,
                comercializacion.cotizacion.curso.nombreCurso,
                comercializacion.cotizacion.curso.codigoCurso,
                sucursal = comercializacion.cotizacion.sucursal.nombre,
                vigencia = comercializacion.fechaTermino.AddMonths(comercializacion.vigenciaCredenciales).ToString("dd-MM-yy", CultureInfo.InvariantCulture),
                //vigencia = comercializacion.vigenciaCredenciales.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                horas = String.Format("{0:0.#}", r11.horasPracticas + r11.horasTeoricas),
                sence,
                participantes
            };
            return data;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult GenerarCredenciales(int? id)
        {
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "credenciales")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"credenciales\" y tipo \"word\".");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            if (comercializacion.cotizacion.sucursal.firmaAdministrador == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la firma del Administrador de Sucursal.");
                ViewBag.templatesR50 = GetTemplatesR50();
                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            foreach (var participante in comercializacion.participantes)
            {
                if (participante.credenciales == null)
                {
                    // indicar q hubo un error
                    ModelState.AddModelError("", "No se encontro la foto del participante " + participante.contacto.run + ".");
                    ViewBag.templatesR50 = GetTemplatesR50();
                    return View("Index", db.Comercializacion
                        .Where(x => x.softDelete == false)
                        .ToList());
                }
            }
            return RedirectToAction("GenerarReporteCredenciales", new { id });
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [EnableJsReport()]
        public async Task<ActionResult> GenerarReporteCredenciales(int? id)
        {
            var comercializacion = db.Comercializacion.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "credenciales")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
            var firmaAdministrador = await Files.BajarArchivoBytesAsync(comercializacion.cotizacion.sucursal.firmaAdministrador);
            var firmaAdministradorBase64 = Convert.ToBase64String(firmaAdministrador, 0, firmaAdministrador.Length);
            var fotos = new Dictionary<int, string>();
            foreach (var participante in comercializacion.participantes)
            {
                var foto = await Files.BajarArchivoBytesAsync(participante.credenciales);
                var fotoBase64 = "data:image/jpeg;base64," + Convert.ToBase64String(foto, 0, foto.Length);
                fotos.Add(
                    participante.idParticipante,
                    fotoBase64
                );
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
                .Configure((r) => r.Data = DataCredenciales(comercializacion, "data:image/png;base64," + firmaAdministradorBase64, fotos))
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"credenciales_" + comercializacion.cotizacion.codigoCotizacion + ".docx\"");
            return null;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/", "/ClienteContacto/" })]

        public async Task<ActionResult> GenerarPdfCredenciales(int? id, string id2)
        {
            var comercializacion = db.Comercializacion.Find(id);
            ViewBag.templatesR50 = GetTemplatesR50();
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            if (id2 != null && id2.Contains("cliente"))
            {
                List<CredencialesFile> credenciales = db.CredencialesFile.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion).ToList();
                if (credenciales.Count() > 0)
                {
                    return await Files.BajarArchivoDescargarAsync(credenciales.FirstOrDefault().file);
                }

                return RedirectToAction("LandingPage", "ClienteContacto", new { error = "Faltan fotos de los participantes" });
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "credenciales")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"credenciales\" y tipo \"word\".");
                return View("Index", db.Comercializacion
     .Where(x => x.softDelete == false)
     .ToList());
            }
            if (comercializacion.cotizacion.sucursal.firmaAdministrador == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la firma del Administrador de Sucursal.");

                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            //foreach (var participante in comercializacion.participantes)
            //{

            //    if (participante.credenciales == null)
            //    {
            //        // indicar q hubo un error
            //        ModelState.AddModelError("", "No se encontro la foto del participante " + participante.contacto.run + ".");

            //        return View("Index", db.Comercializacion
            //            .Where(x => x.softDelete == false)
            //            .ToList());
            //    }
            //}
            if (comercializacion.participantes.All(x => x.credenciales == null))
            {
                ModelState.AddModelError("", "Debe Existe al menos un participante con foto ");

                return View("Index", db.Comercializacion
                    .Where(x => x.softDelete == false)
                    .ToList());
            }
            Random rnd = new Random();
            string hash = "" + rnd.Next(11111, 99999);
            //using (SHA256 sha256Hash = SHA256.Create())
            //{
            //    hash = Utils.Utils.GetHash(sha256Hash, DateTime.Now.ToString());
            //}
            //return RedirectToAction("GenerarReportePdfCredenciales", "Comercializacions",new { id , id2 = hash });
            string createRequest = Url.Action("GenerarReportePdfCredenciales", "Comercializacions", new { id, id2 = hash }, Request.Url.Scheme);
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
            Response.AppendHeader("Content-Disposition", "attachment; filename=\"Credenciales_" + comercializacion.cotizacion.codigoCotizacion + ".pdf\"");

            return new FileContentResult(bytes, "application/pdf");
        }

        [EnableJsReport()]
        public async Task<ActionResult> GenerarReportePdfCredenciales(int? id, string id2)
        {
            var comercializacion = db.Comercializacion.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "credenciales")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
            var firmaAdministrador = await Files.BajarArchivoBytesAsync(comercializacion.cotizacion.sucursal.firmaAdministrador);
            var firmaAdministradorBase64 = Convert.ToBase64String(firmaAdministrador, 0, firmaAdministrador.Length);
            var fotos = new Dictionary<int, string>();
            foreach (var participante in comercializacion.participantes.Where(x => x.credenciales != null).ToList())
            {
                var foto = await Files.BajarArchivoBytesAsync(participante.credenciales);
                var fotoBase64 = "data:image/jpeg;base64," + Convert.ToBase64String(foto, 0, foto.Length);
                fotos.Add(
                    participante.idParticipante,
                    fotoBase64
                );
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
                .Configure((r) => r.Data = DataCredenciales(comercializacion, "data:image/png;base64," + firmaAdministradorBase64, fotos))
            //.OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"rddddd_.docx\"");
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

        // Excel

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [EnableJsReport()]
        public ActionResult ExcelCredenciales(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comercializacion comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            var r11 = db.R11.Where(x => x.idCurso == comercializacion.cotizacion.curso.idCurso).FirstOrDefault();
            ViewBag.horas = r11.horasPracticas + r11.horasTeoricas;

            var elerning = "";
            if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono || comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono)
            {
                elerning = " (E)";
            }
            var nombreArchivo = "CRED" + elerning + ". (" + comercializacion.cotizacion.codigoCotizacion + ")";

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"" + nombreArchivo + ".xlsx\"");
            return View(comercializacion);
        }

        // GET: Comercializacions/CerrarNode
        /**
        public void CerrarNode()
        {
            foreach (Process proc in Process.GetProcessesByName("jsreport"))
            {
                proc.Kill();
            }
        }**/

        private bool bloqueadoOCPendiente(Comercializacion comercializacion)
        {
            var docsCompromisoCostoEmpresa = comercializacion.cotizacion.documentosCompromiso
                .Where(x => x.tipoVenta.tipoPago == SGC.Models.TipoPago.CostoEmpresa)
                .Where(x => x.softDelete == false)
                .ToList();
            if (docsCompromisoCostoEmpresa.Count() > 0)
            {
                var cont = 0;
                foreach (var docCompromiso in docsCompromisoCostoEmpresa)
                {
                    if (docCompromiso.tipoDocCompromiso.nombre.ToLower().Contains("oc"))
                    {
                        cont++;
                    }
                }
                if (cont == 0)
                {
                    return true;
                }
            }
            return false;
        }



        //Mis funciones nuevas 
        [HttpGet]
        public ActionResult RedirectVideoLLamadaAlumnosComercializacion(int id, String rut)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comercializacion comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //verificar la fecha de inicio sino es hoy no buscar link
            if (DateTime.Now.Date.Ticks < comercializacion.fechaInicio.Date.Ticks || DateTime.Now.Date.Ticks > comercializacion.fechaTermino.Date.Ticks)
            {
                ViewBag.error = "Fuera de Fecha";
                ViewBag.rut = "";
                ViewBag.comercializacion = comercializacion;
                return View();
            }
            var now = DateTime.Now.Date;
            var time = DateTime.Now.TimeOfDay;
            if (rut != null && rut != "")
            {

                try
                {
                    var bloque = comercializacion.bloques.Where(x => TimeSpan.Compare(x.horarioInicio.TimeOfDay, DateTime.Now.TimeOfDay) <= 0 && TimeSpan.Compare(x.horarioTermino.TimeOfDay, DateTime.Now.TimeOfDay) >= 0).FirstOrDefault();
                    var part = comercializacion.participantes.Where(x => x.contacto.run.Equals(rut)).FirstOrDefault();
                    var subQuery = db.Asistencias.Where(x => x.bloque.comercializacion.idComercializacion == comercializacion.idComercializacion && x.bloque.fecha.Year == now.Year && x.bloque.fecha.Month == now.Month && x.bloque.fecha.Day == now.Day).ToList();
                    var asistencia = subQuery.Where(x => x.participante.contacto.runCompleto.Contains(rut) && TimeSpan.Compare(x.bloque.horarioInicio.TimeOfDay, DateTime.Now.TimeOfDay) < 0 && TimeSpan.Compare(x.bloque.horarioTermino.TimeOfDay, DateTime.Now.TimeOfDay) > 0).FirstOrDefault();
                    if (asistencia != null && asistencia.descripcion == "")
                    {
                        asistencia.descripcion = "Ingresó por el enlace a las " + DateTime.Now.ToString("hh:mm tt");
                        db.Entry(asistencia).State = EntityState.Modified;

                    }
                    else if (asistencia == null && bloque != null && part != null)
                    {

                        asistencia = new Asistencia
                        {
                            bloque = bloque,
                            participante = part,
                            asistio = false,
                            fecha = now,
                            descripcion = "Ingresó por el enlace a las " + DateTime.Now.ToString("hh:mm tt")

                        };
                        db.Asistencias.Add(asistencia);

                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                }


            }
            LinkComercializacion linkComercializacion = db.LinkComercializacion.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion).Include(x => x.linkType).Include(x => x.link).FirstOrDefault();

            //verificar la fecha de inicio sino es hoy no buscar link
            if (linkComercializacion == null)
            {
                ViewBag.error = "Sin Configurar los links";
                ViewBag.rut = "";
                ViewBag.comercializacion = comercializacion;
                return View();
            }
            else
            {
                if (!linkComercializacion.linkAutomatic && linkComercializacion.linkManual != null)
                {
                    return Redirect(linkComercializacion.linkManual);
                }
            }

            //Buscar equivalentes
            List<Comercializacion> comercializacionst = db.Comercializacion
                  .Where(x => DateTime.Compare(now, x.fechaInicio) >= 0)
                  .Where(x => DateTime.Compare(now, x.fechaTermino) <= 0)
                  .Where(x => x.idComercializacion != comercializacion.idComercializacion)
                  .OrderBy(x => x.fechaCreacion)
                  .Join(db.LinkComercializacion,
                   com => com.idComercializacion,
                    linkCom => linkCom.comercializacion.idComercializacion,
                       (com, linkCom) => new { com, linkCom }
                  )
                  .Where(x => x.linkCom.linkAutomatic && x.linkCom.linkType.idLinkType == linkComercializacion.linkType.idLinkType)
                  .Select(x => x.com)
                  .ToList();

            List<LinkComercializacion> linkComerEqui = new List<LinkComercializacion>();
            linkComerEqui.Add(linkComercializacion);
            foreach (Comercializacion temp in comercializacionst)
            {
                bool areEquivalent = (comercializacion.bloques.Count() == temp.bloques.Count())
                    && !comercializacion.bloques.Select(x => x.horarioInicio.ToString("t")).Except(temp.bloques.Select(y => y.horarioInicio.ToString("t"))).Any()
                     && !comercializacion.bloques.Select(x => x.horarioTermino.ToString("t")).Except(temp.bloques.Select(y => y.horarioTermino.ToString("t"))).Any()
                     && !comercializacion.bloques.Select(x => x.relator.idRelator).Except(temp.bloques.Select(y => y.relator.idRelator)).Any()

                     ;

                if (areEquivalent)
                {
                    linkComerEqui.Add(db.LinkComercializacion.Where(x => x.comercializacion.idComercializacion == temp.idComercializacion).Include(x => x.linkType).Include(x => x.link).FirstOrDefault());

                }



            }
            var haveLink = linkComerEqui.FirstOrDefault(x => x.link != null);
            if (haveLink != null)
            {
                foreach (var equi in linkComerEqui.Where(x => x.idLinkComercializacion != haveLink.idLinkComercializacion).ToList())
                {
                    equi.link = haveLink.link;
                    db.Entry(equi).State = EntityState.Modified;
                }
                db.SaveChanges();
            }


            //Redireccionar si existe ya ok
            if (linkComercializacion != null && linkComercializacion.linkAutomatic && linkComercializacion.link != null && linkComercializacion.link.type.idLinkType == linkComercializacion.linkType.idLinkType)
            {
                return Redirect(linkComercializacion.link.url);

            }

            Link link = null;
            if ((linkComercializacion != null && linkComercializacion.link == null) || linkComercializacion.link.type.idLinkType != linkComercializacion.linkType.idLinkType)
            {

                var LinkEnUso = db.LinkComercializacion.Where(x => DateTime.Compare(now, x.comercializacion.fechaTermino) <= 0).Where(x => x.linkAutomatic && x.link != null && x.link.type.idLinkType == linkComercializacion.linkType.idLinkType).Select(x => x.link).ToList();
                var links = db.Link.Where(x => x.type.idLinkType == linkComercializacion.linkType.idLinkType).ToList().Where(x => !LinkEnUso.Any(y => y.idLink == x.idLink)).ToList();
                if (links.Count() > 0)
                {
                    linkComercializacion.link = links.FirstOrDefault();
                }

                db.Entry(linkComercializacion).State = EntityState.Modified;
                db.SaveChanges();
            }

            if (linkComercializacion.link == null)
            {
                ViewBag.error = "Sin Links disponibles";
                ViewBag.rut = "";
                ViewBag.comercializacion = comercializacion;
                return View();
            }


            return Redirect(linkComercializacion.link.url);

        }

        //[HttpGet]
        //public ActionResult CorreoAlumnosComercializacion(int id, string mail = null)
        //{

        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }



        //    Comercializacion comercializacion = db.Comercializacion.Where(x => x.idComercializacion == id).FirstOrDefault();

        //    if (comercializacion == null)
        //    {

        //        return RedirectToAction("Index");
        //    }
        //    if (comercializacion.participantes.Count == 0)
        //    {
        //        ModelState.AddModelError("", "No hay participantes disponibles");
        //        TempData["ModelState"] = ModelState;
        //        return RedirectToAction("List", "Participante", new { id = id });
        //    }
        //    if (DateTime.Compare(DateTime.Now.Date, comercializacion.fechaTermino) > 0)
        //    {

        //        ModelState.AddModelError("", "La comercialización finalizó el día " + comercializacion.fechaTermino.ToString("MM/dd/yyyy"));
        //        TempData["ModelState"] = ModelState;
        //        return RedirectToAction("List", "Participante", new { id = id });
        //    }


        //    Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
        //    comercializacion.cotizacion.codigoCotizacion = comercializacion.cotizacion.codigoCotizacion.Split('-')[0];
        //    var bodyHTML = "";
        //    var subject = "{0} // ENLACE PLATAFORMA E-LEARNING // INSECAP // {1} // {2}";
        //    //Reemplazar valores asunto
        //    subject = string.Format(subject,
        //        comercializacion.cotizacion.cliente.nombreEmpresa.ToUpper(),
        //        comercializacion.cotizacion.curso.nombreCurso.ToUpper(),
        //        comercializacion.cotizacion.codigoCotizacion.ToUpper()
        //        );
        //    String bloqueString = "";
        //    string tipoEjecucion = comercializacion.cotizacion.curso.tipoEjecucion.ToString();
        //    if (tipoEjecucion.ToString().Contains("Recertificacion_Sincronica") || tipoEjecucion.ToString().Contains("Elearning_Sincrono"))
        //    {


        //        bloqueString = "<br> Bloques del día " + String.Format("{0:dddd d , MMMM , yyyy}", comercializacion.fechaInicio).Replace(",", "de") + " : <br><br> 9:00 - 13:00 <br> 14:00 - 18:00 <br>";
        //        int bloqueCont = comercializacion.bloques.ToList().Count();
        //        if (bloqueCont > 0)
        //        {
        //            bloqueString = "";
        //            String dateString = "";
        //            foreach (Bloque bloque in comercializacion.bloques)
        //            {
        //                String currentDate = String.Format("{0:dddd d , MMMM , yyyy}", bloque.fecha.Date).Replace(",", "de");
        //                if (!dateString.Equals(currentDate))
        //                {

        //                    bloqueString += "<br><br> Bloques del día " + currentDate + ": <br>";
        //                    bloqueString += bloque.horarioInicio.ToString("HH:mm") + " - " + bloque.horarioTermino.ToString("HH:mm") + "<br>";
        //                }
        //                else
        //                {
        //                    bloqueString += bloque.horarioInicio.ToString("HH:mm") + " - " + bloque.horarioTermino.ToString("HH:mm") + "<br>";
        //                }
        //                dateString = currentDate;

        //            }
        //        }
        //        using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/cursos.html")))
        //        {

        //            bodyHTML = reader.ReadToEnd();
        //        }
        //    }
        //    else if (tipoEjecucion.ToString().Contains("Recertificacion_Asincronica") || tipoEjecucion.ToString().Contains("Elearning_Asincrono"))
        //    {
        //        bloqueString = "<br> Usted dispone del siguiente rango de fechas para realizar el curso: <br> " + comercializacion.fechaInicio.ToString("MM/dd/yyyy") + " - " + comercializacion.fechaTermino.ToString("MM/dd/yyyy");

        //        using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/cursos_a.html")))
        //        {
        //            bodyHTML = reader.ReadToEnd();
        //        }
        //    }
        //    else
        //    {
        //        ModelState.AddModelError("", "Este curso no permite el envío de correos");
        //        TempData["ModelState"] = ModelState;
        //        return RedirectToAction("List", "Participante", new { id = id });
        //    }

        //    if (mail != null)
        //    {
        //        comercializacion.participantes = new List<Participante>();
        //        Contacto contacto = new Contacto
        //        {
        //            correo = mail,
        //            nombres = "",
        //            apellidoMaterno = "",
        //            apellidoPaterno = "",
        //            run = "El nombre de usuario es su rut considerando puntos y guión, si son guión k debe ser con minúscula. Ejemplo: 11.111.111 - k"
        //        };
        //        comercializacion.participantes.Add(new Participante { contacto = contacto });
        //    }

        //    var message = "";

        //    foreach (Participante participante in comercializacion.participantes)
        //    {
        //        var body = bodyHTML;
        //        var receiverEmail = new MailAddress(participante.contacto.correo, participante.contacto.nombreCompleto);
        //        //Reemplazar valores cuerpo
        //        body = body.Replace("{0}", participante.contacto.nombreCompleto);
        //        body = body.Replace("{1}", comercializacion.cotizacion.curso.nombreCurso.ToUpper());
        //        body = body.Replace("{2}", comercializacion.cotizacion.cliente.nombreEmpresa.ToUpper());
        //        body = body.Replace("{3}", bloqueString);
        //        body = body.Replace("{4}", participante.contacto.runCompleto);
        //        body = body.Replace("{5}", "chile");
        //        if (tipoEjecucion.ToString().Contains("Recertificacion_Sincronica") || tipoEjecucion.ToString().Contains("Elearning_Sincrono"))
        //        {
        //            String rutTemp = mail == null ? participante.contacto.runCompleto : "null";
        //            body = body.Replace("{6}", domain + "/Comercializacions/RedirectVideoLLamadaAlumnosComercializacion?id=" + comercializacion.idComercializacion + "&rut=" + rutTemp);
        //        }
        //        message = SendMail(receiverEmail, subject, body);
        //        if (message != "ok")
        //        {
        //            ModelState.AddModelError("", message);
        //        }
        //    }



        //    Contacto contactoCliente = db.Contacto.Where(x => x.idContacto == comercializacion.cotizacion.contacto).FirstOrDefault();
        //    if (contactoCliente != null)
        //    {
        //        var body = bodyHTML;
        //        var receiverEmail = new MailAddress(contactoCliente.correo, contactoCliente.nombreCompleto);
        //        //Reemplazar valores cuerpo
        //        body = body.Replace("{0}", contactoCliente.nombreCompleto);
        //        body = body.Replace("{1}", comercializacion.cotizacion.curso.nombreCurso.ToUpper());
        //        body = body.Replace("{2}", comercializacion.cotizacion.cliente.nombreEmpresa.ToUpper());
        //        body = body.Replace("{3}", bloqueString);
        //        body = body.Replace("{4}", "No Permitido el acceso");
        //        body = body.Replace("{5}", "No Permitido el acceso");
        //        if (tipoEjecucion.ToString().Contains("Recertificacion_Sincronica") || tipoEjecucion.ToString().Contains("Elearning_Sincrono"))
        //            body = body.Replace("{6}", domain + "/Comercializacions/RedirectVideoLLamadaAlumnosComercializacion?id=" + comercializacion.idComercializacion + "&rut=null");
        //        message = SendMail(receiverEmail, subject, body);
        //        if (message != "ok")
        //        {
        //            ModelState.AddModelError("", message);
        //        }

        //    }
        //    CorreoInsecapComercializacion(comercializacion, bloqueString);

        //    ModelState.AddModelError("", "Correos enviados satisfactoriamente");
        //    TempData["ModelState"] = ModelState;
        //    return RedirectToAction("List", "Participante", new { id = id });

        //}


        [HttpGet]
        public ActionResult CorreoAlumnosComercializacion(int id, int idParticipante, string mail)
        {

            if (id == null && idParticipante == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var error = "OK";
            Comercializacion comercializacion = db.Comercializacion.Where(x => x.idComercializacion == id).FirstOrDefault();
            Participante participante = comercializacion.participantes.Where(x => x.idParticipante == idParticipante).FirstOrDefault();
            string tipoEjecucion = comercializacion.cotizacion.curso.tipoEjecucion.ToString();
            if (comercializacion == null)
            {
                error = "Comercializacion no encontrada";
            }
            if (idParticipante != 0 && participante == null)
            {
                error = "Participante no encontrado";
            }
            //if (comercializacion.participantes.Count == 0)
            //{
            //    ModelState.AddModelError("", "No hay participantes disponibles");
            //    TempData["ModelState"] = ModelState;
            //    return RedirectToAction("List", "Participante", new { id = id });
            //}

            //    if (DateTime.Compare(DateTime.Now.Date, comercializacion.fechaTermino) > 0)
            //{
            //    error = "La comercialización finalizó el día" + comercializacion.fechaTermino.ToString("MM/dd/yyyy");

            //}


            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
            comercializacion.cotizacion.codigoCotizacion = comercializacion.cotizacion.codigoCotizacion.Split('-')[0];
            var bodyHTML = "";
            var subject = "{0} // ENLACE PLATAFORMA E-LEARNING // INSECAP // {1} // {2}";
            //Reemplazar valores asunto
            subject = string.Format(subject,
                comercializacion.cotizacion.cliente.nombreEmpresa.ToUpper(),
                comercializacion.cotizacion.curso.nombreCurso.ToUpper(),
                comercializacion.cotizacion.codigoCotizacion.ToUpper()
                );
            String bloqueString = "";

            if (tipoEjecucion.ToString().Contains("Recertificacion_Sincronica") || tipoEjecucion.ToString().Contains("Elearning_Sincrono"))
            {


                bloqueString = "<br> Bloques del día " + String.Format("{0:dddd d , MMMM , yyyy}", comercializacion.fechaInicio).Replace(",", "de") + " : <br><br> 9:00 - 13:00 <br> 14:00 - 18:00 <br>";
                int bloqueCont = comercializacion.bloques.ToList().Count();
                if (bloqueCont > 0)
                {
                    bloqueString = "";
                    String dateString = "";
                    foreach (Bloque bloque in comercializacion.bloques)
                    {
                        String currentDate = String.Format("{0:dddd d , MMMM , yyyy}", bloque.fecha.Date).Replace(",", "de");
                        if (!dateString.Equals(currentDate))
                        {

                            bloqueString += "<br><br> Bloques del día " + currentDate + ": <br>";
                            bloqueString += bloque.horarioInicio.ToString("HH:mm") + " - " + bloque.horarioTermino.ToString("HH:mm") + "<br>";
                        }
                        else
                        {
                            bloqueString += bloque.horarioInicio.ToString("HH:mm") + " - " + bloque.horarioTermino.ToString("HH:mm") + "<br>";
                        }
                        dateString = currentDate;

                    }
                }
                using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/cursos.html")))
                {

                    bodyHTML = reader.ReadToEnd();
                }
            }
            else if (tipoEjecucion.ToString().Contains("Recertificacion_Asincronica") || tipoEjecucion.ToString().Contains("Elearning_Asincrono") || tipoEjecucion.ToString().Contains("Presencial"))
            {
                bloqueString = "<br> Usted dispone del siguiente rango de fechas para realizar el curso: <br> " + String.Format("{0:dddd d , MMMM , yyyy}", comercializacion.fechaInicio).Replace(",", "de") + " hasta " + String.Format("{0:dddd d , MMMM , yyyy}", comercializacion.fechaTermino).Replace(",", "de");

                using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/cursos_a.html")))
                {
                    bodyHTML = reader.ReadToEnd();
                }
            }
            else
            {
                error = "Esta comercializacion no permite el envío de correos";

            }
            var mailReturn = "";
            if (error == "OK")
            {

                if (mail != "")
                {

                    Contacto contacto = new Contacto
                    {
                        correo = mail,
                        nombres = "",
                        apellidoMaterno = "",
                        apellidoPaterno = "",
                        run = "El nombre de usuario es su rut considerando puntos y guión, si son guión k debe ser con minúscula. Ejemplo: 11.111.111 - k"
                    };
                    participante = new Participante { contacto = contacto };
                }

                var message = "";
                var body = bodyHTML;

                if (idParticipante != 0 || (idParticipante == 0 && mail != ""))
                {

                    var receiverEmail = new MailAddress(participante.contacto.correo, participante.contacto.nombreCompleto);
                    //Reemplazar valores cuerpo
                    body = body.Replace("{0}", participante.contacto.nombreCompleto);
                    body = body.Replace("{1}", comercializacion.cotizacion.curso.nombreCurso.ToUpper());
                    body = body.Replace("{2}", comercializacion.cotizacion.cliente.nombreEmpresa.ToUpper());
                    body = body.Replace("{3}", bloqueString);
                    body = body.Replace("{4}", participante.contacto.runCompleto);
                    body = body.Replace("{5}", "chile");
                    if (tipoEjecucion.ToString().Contains("Recertificacion_Sincronica") || tipoEjecucion.ToString().Contains("Elearning_Sincrono"))
                    {
                        body = body.Replace("{6}", domain + "/Comercializacions/RedirectVideoLLamadaAlumnosComercializacion?id=" + comercializacion.idComercializacion + "&rut=" + participante.contacto.runCompleto);
                    }
                    message = Utils.Utils.SendMail(receiverEmail, subject, body);
                    if (message != "ok")
                    {
                        error = "Al enviar al correo " + participante.contacto.correo + " se generó el error " + message;

                    }


                }
                if (idParticipante == 0)
                {
                    Contacto contactoCliente = db.Contacto.Where(x => x.idContacto == comercializacion.cotizacion.contacto).FirstOrDefault();
                    if (contactoCliente != null)
                    {
                        body = bodyHTML;
                        var receiverEmail = new MailAddress(contactoCliente.correo, contactoCliente.nombreCompleto);
                        //Reemplazar valores cuerpo
                        body = body.Replace("{0}", contactoCliente.nombreCompleto);
                        body = body.Replace("{1}", comercializacion.cotizacion.curso.nombreCurso.ToUpper());
                        body = body.Replace("{2}", comercializacion.cotizacion.cliente.nombreEmpresa.ToUpper());
                        body = body.Replace("{3}", bloqueString);
                        body = body.Replace("{4}", "No Permitido el acceso");
                        body = body.Replace("{5}", "No Permitido el acceso");
                        if (tipoEjecucion.ToString().Contains("Recertificacion_Sincronica") || tipoEjecucion.ToString().Contains("Elearning_Sincrono"))
                            body = body.Replace("{6}", domain + "/Comercializacions/RedirectVideoLLamadaAlumnosComercializacion?id=" + comercializacion.idComercializacion + "&rut=null");
                        message = Utils.Utils.SendMail(receiverEmail, subject, body);
                        if (message != "ok")
                        {
                            error = error + " No se envió el correo por:  " + message; ;

                        }

                    }
                    CorreoInsecapComercializacion(comercializacion, bloqueString);
                }


                if (idParticipante != 0)
                {
                    try
                    {
                        foreach (var errorModel in db.GetValidationErrors())
                        {
                            db.Entry(errorModel.Entry.Entity).State = EntityState.Unchanged;
                        }

                        comercializacion.participantes.Where(x => x.idParticipante == idParticipante).Select(x => x).FirstOrDefault().correoEnviado = true;

                        db.Entry(comercializacion).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        error = error + " pero no ne guardo por: " + e.Message; ;
                    }
                    error = "Correo enviado a " + participante.contacto.nombres + " al correo " + participante.contacto.correo;
                    mailReturn = participante.contacto.correo;
                }
                else if (idParticipante == 0 && mail != "")
                {
                    error = "Correo enviado a INSECAP, Relator y Contacto y al correo: " + participante.contacto.correo;

                }
                else
                {
                    error = "Correo enviado a INSECAP, Relator y Contacto";
                    mailReturn = " ";

                }


            }




            var jsonResult = Json(new { error, mail = mailReturn }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;


        }

        [HttpGet]
        public ActionResult GetListParticipantes(int id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var error = "OK";
            Comercializacion comercializacion = db.Comercializacion.Where(x => x.idComercializacion == id).FirstOrDefault();
            List<int> participantes = new List<int>();
            if (comercializacion != null)
            {
                if (comercializacion.participantes.Count() == 0)
                {
                    error = "Sin participantes";
                }

                participantes.AddRange(comercializacion.participantes.Select(x => x.idParticipante).ToList());

            }
            else
            {
                error = "Comercializacion no encotrada";
            }

            var jsonResult = Json(new { error, participantes }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public void CorreoInsecapComercializacion(Comercializacion comercializacion, String bloques)
        {
            List<Contacto> contactos = new List<Contacto>();
            contactos.Add(new Contacto { nombres = "INSECAP", correo = "contacto@insecap.email" });
            contactos.Add(new Contacto { nombres = comercializacion.usuarioCreador.nombreCompleto, correo = comercializacion.usuarioCreador.Email });
            List<AspNetUsers> netUsers = db.AspNetUsers.Where(x => x.AspNetRoles.Any(y => y.Name == "Administrador") == true).ToList();
            string tipoEjecucion = comercializacion.cotizacion.curso.tipoEjecucion.ToString();

            foreach (AspNetUsers users in netUsers)
            {
                contactos.Add(new Contacto { nombres = users.nombreCompleto, correo = users.Email });
            }
            foreach (RelatorCurso relator in comercializacion.relatoresCursos)
            {
                contactos.Add(relator.relator.contacto);
            }
            var bodyHTML = "";
            if (tipoEjecucion.ToString().Contains("Recertificacion_Sincronica") || tipoEjecucion.ToString().Contains("Elearning_Sincrono"))
            {

                using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/insecap.html")))
                {
                    bodyHTML = reader.ReadToEnd();
                }
            }
            else if (tipoEjecucion.ToString().Contains("Recertificacion_Asincronica") || tipoEjecucion.ToString().Contains("Elearning_Asincrono") || tipoEjecucion.ToString().Contains("Presencial"))
            {
                using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/insecap_a.html")))
                {
                    bodyHTML = reader.ReadToEnd();
                }
            }



            var subject = "{0} // ENLACE PLATAFORMA E-LEARNING // INSECAP // {1} // {2}";
            //Reemplazar valores asunto
            subject = string.Format(subject,
                comercializacion.cotizacion.cliente.nombreEmpresa.ToUpper(),
                comercializacion.cotizacion.curso.nombreCurso.ToUpper(),
                comercializacion.cotizacion.codigoCotizacion.ToUpper()
                );



            foreach (Contacto contacto in contactos)
            {
                var body = bodyHTML;
                var receiverEmail = new MailAddress(contacto.correo, "INSECAP");
                //Reemplazar valores cuerpo
                body = body.Replace("{0}", contacto.nombres);
                body = body.Replace("{1}", comercializacion.cotizacion.curso.nombreCurso.ToUpper());
                body = body.Replace("{2}", comercializacion.cotizacion.cliente.nombreEmpresa.ToUpper());
                body = body.Replace("{3}", bloques);
                if (tipoEjecucion.ToString().Contains("Recertificacion_Sincronica") || tipoEjecucion.ToString().Contains("Elearning_Sincrono"))
                    body = body.Replace("{4}", domain + "/Comercializacions/RedirectVideoLLamadaAlumnosComercializacion?id=" + comercializacion.idComercializacion + "&rut=null");
                else if (tipoEjecucion.ToString().Contains("Recertificacion_Asincronica") || tipoEjecucion.ToString().Contains("Elearning_Asincrono") || tipoEjecucion.ToString().Contains("Presencial"))
                {
                    body = body.Replace("{4}", "No Permitido el acceso");
                    body = body.Replace("{5}", "No Permitido el acceso");
                }

                string message = Utils.Utils.SendMail(receiverEmail, subject, body);
                if (message != "ok")
                {
                    ModelState.AddModelError("", message);
                }



            }




        }



        //Correo cliente 



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



        //Correo cliente
        [HttpGet]
        public ActionResult MailClientPrepare()
        {
            bool send = true;
            try
            {
                List<Comercializacion> comercializacions = db.Comercializacion
                .Where(x => x.softDelete == false)
                .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(z => z.fechaCreacion).Take(1).Any(y => y.EstadoComercializacion == EstadoComercializacion.En_Proceso) || x.comercializacionEstadoComercializacion.OrderByDescending(z => z.fechaCreacion).Take(1).Any(y => y.EstadoComercializacion == EstadoComercializacion.Terminada_SENCE))
                .Where(x => x.cotizacion.cliente.enviarCapacitacionRealizadaEnvioCorreo == true || x.cotizacion.cliente.enviarResumenEnvioCorreo == true)
                .Include(x => x.cotizacion)
                .Include(x => x.cotizacion.cliente)
                .Include(x => x.cotizacion.curso)
                .ToList();

                //filtrar por el boton
                List<Comercializacion> comercializacionsCapacitacionRealizada = comercializacions
                    .Where(x => DateTime.Now.Date.Subtract(x.fechaTermino.Date).Days <= 1 && DateTime.Now.Date.Subtract(x.fechaTermino.Date).Days > 0)
                    .Where(x => x.cotizacion.cliente.enviarCapacitacionRealizadaEnvioCorreo == true)
                    .ToList();
                //filtrar por el boton
                List<Comercializacion> comercializacionsResumen = comercializacions
                    .Where(x => DateTime.Now.Date.Subtract(x.cotizacion.cliente.ultimaFechaEnvioCorreo.Date).Days >= x.cotizacion.cliente.cantDiasEnvioCorreo && DateTime.Now.Date.Subtract(x.cotizacion.cliente.ultimaFechaEnvioCorreo.Date).Days > 0 && DateTime.Now.Date.Subtract(x.fechaTermino.Date).Days > 0)
                    .Where(x => x.cotizacion.cliente.enviarResumenEnvioCorreo == true)
                    .ToList();

                MailClientSend(comercializacionsCapacitacionRealizada);
                MailClientSend(comercializacionsResumen);
                MailLog("/Email/Log/cliente.txt", "Enviado Correctamente");
            }
            catch (Exception e)
            {
                MailLog("/Email/Log/cliente.txt", e.Message);
                send = false;
            }


            //Agrupar clientes iguales por contacto y id cliente en cotizacion 
            //Agregar tipo de ejecucion al correo
            //Validar todos los que no sean presenciales o recertificacion que tengan alumnos 



            return Json(new
            {
                mailSend = send
            },
                   JsonRequestBehavior.AllowGet);
        }

        private void MailClientSend(List<Comercializacion> comercializacions)
        {
            var bodyHTML = "";
            var subject = "**Servicios Finalizados OC.Pendiente**";
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/Cliente/mail.html")))
            {

                bodyHTML = reader.ReadToEnd();
            }
            foreach (Comercializacion comercializacion in comercializacions.ToList())
            {

                List<Comercializacion> temp = comercializacions.Where(x => x.cotizacion.idCliente == comercializacion.cotizacion.idCliente && x.cotizacion.contacto == comercializacion.cotizacion.contacto).ToList();
                if (temp.Count > 0)
                {
                    Comercializacion comercializacionFirst = temp.FirstOrDefault();
                    List<String> correos = new List<string>();
                    var tabla = ConvertPartialViewToString(PartialView("MailClient", temp));
                    List<string> comerciales = temp.Select(x => x.cotizacion.usuarioCreador).Distinct().Select(x => x.Email).ToList();
                    // obtener de aca los correos de todos los contactos o solo deun contacto 
                    Contacto contacto = db.Contacto.Find(comercializacionFirst.cotizacion.contacto);
                    correos.Add("contacto@insecap.email");
                    correos.Add(contacto.correo);
                    correos.AddRange(comerciales);


                    String body = bodyHTML.Replace("{0}", contacto.nombreCompleto);
                    //Incluir los correos si tiene varios creadores y los telefonos
                    body = body.Replace("{2}", String.Join(" , ", comerciales));
                    body = body.Replace("{3}", comercializacionFirst.cotizacion.usuarioCreador.telefono);
                    body = body.Replace("{1}", tabla);

                    foreach (String mail in correos)
                    {
                        var receiverEmail = new MailAddress(mail, "INSECAP Capacitación");
                        Utils.Utils.SendMail(receiverEmail, subject, body);
                    }
                    //SendMail(new MailAddress("jrodriguez@insecap.cl", "INSECAP Capacitación"), subject, body);
                    comercializacion.cotizacion.cliente.ultimaFechaEnvioCorreo = DateTime.Now;
                    db.Entry(comercializacion.cotizacion.cliente).State = EntityState.Modified;
                    db.SaveChanges();
                    comercializacions.RemoveAll(x => x.cotizacion.idCliente == comercializacion.cotizacion.idCliente && x.cotizacion.contacto == comercializacion.cotizacion.contacto);

                }

            }

        }

        private void MailLog(string fileRoute, string error)
        {


            using (System.IO.StreamWriter file =
          new System.IO.StreamWriter(Server.MapPath(@"~" + fileRoute)))
            {

                file.WriteLine(String.Format("{0} - {1}", DateTime.Now, error));

            }

        }
        [HttpGet]
        public ActionResult BloqueoCliente()
        {

            var comercializaciones = db.Comercializacion
                 .Where(x => x.softDelete == false)
                .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(z => z.fechaCreacion).Take(1).Any(y => y.EstadoComercializacion == EstadoComercializacion.Terminada) || x.comercializacionEstadoComercializacion.OrderByDescending(z => z.fechaCreacion).Take(1).Any(y => y.EstadoComercializacion == EstadoComercializacion.Terminada_SENCE))
                .ToList();
            comercializaciones.ForEach(x => x.clientDownload = true);
            comercializaciones.ForEach(x => x.clientFactura = true);
            comercializaciones.ForEach(x => db.Entry(x).State = EntityState.Modified);
            db.SaveChanges();
            return Json(new
            {
                value = "Terminado"
            },
                   JsonRequestBehavior.AllowGet);

        }



        // ------------------------------- Reporte cliente -----------------------
        [EnableJsReport()]
        [HttpGet]
        public ActionResult ReporteParticipanteComercial(int id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var comercializacion = db.Comercializacion.Where(x => x.softDelete == false && x.idComercializacion == id).FirstOrDefault();
            if (comercializacion == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            string cod = comercializacion.cotizacion.codigoCotizacion.Split('-')[0];
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Cotizacion_" + cod + ".xlsx\"");
            return View(comercializacion);
        }
        //Mis funciones
        [HttpGet]
        public ActionResult MailAsinc()
        {

            DateTime hoy = DateTime.Now.Date;
            var comercializaciones = db.Comercializacion
                 .Where(x => x.softDelete == false)
                 .Where(x => x.participantes.Count() != 0)
                 .Where(x => x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica)
                .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(z => z.fechaCreacion).Take(1).Any(y => y.EstadoComercializacion == EstadoComercializacion.En_Proceso))
                .Where(x => DateTime.Compare(x.fechaInicio, hoy) <= 0 && DateTime.Compare(x.fechaTermino, hoy) >= 0)
                .ToList();

            //Correo config

            var bodyHTML = "";
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/cursos_automatic_a.html")))
            {
                bodyHTML = reader.ReadToEnd();
            }


            foreach (Comercializacion comercializacion in comercializaciones)
            {
                try
                {

                    int cantDays = (comercializacion.fechaTermino - comercializacion.fechaInicio).Days;

                    //periodo = Math.Ceiling(periodo);
                    List<DateTime> daysAdd = new List<DateTime>();
                    int periodo = 5;
                    int i = 0;
                    bool done = true;
                    while (done)
                    {
                        if (daysAdd.Count() == 0)
                        {
                            if (DateTime.Compare(comercializacion.fechaInicio.AddDays(periodo).Date, comercializacion.fechaTermino) < 0)
                            {
                                daysAdd.Add(comercializacion.fechaInicio.AddDays(periodo).Date);
                            }
                            else
                            {
                                done = false;
                            }

                        }
                        else
                        {
                            if (DateTime.Compare(daysAdd.ElementAt(i - 1).AddDays(periodo).Date, comercializacion.fechaTermino) < 0)
                            {
                                daysAdd.Add(daysAdd.ElementAt(i - 1).AddDays(periodo).Date);
                            }
                            else
                            {
                                done = false;
                            }
                        }
                        i++;
                    }



                    foreach (Participante participante in comercializacion.participantes.Where(x => x.notas.Any(y => y.evaluacion == null || y.nota == "-")))
                    {

                        if (daysAdd.Contains(hoy.Date) || DateTime.Compare(comercializacion.fechaTermino, hoy) == 0)
                        {
                            var receiverEmail = new MailAddress(participante.contacto.correo, participante.contacto.nombreCompleto);
                            //var receiverEmail = new MailAddress("jrodriguez@insecap.cl", participante.contacto.nombreCompleto);
                            String bloqueString = "<br> Usted dispone del siguiente rango de fechas para realizar el curso: <br> " + comercializacion.fechaInicio.ToString("dd/MM/yyyy") + " - " + comercializacion.fechaTermino.ToString("dd/MM/yyyy");
                            var subject = "RECORDATORIO // {0} // {1} // ENLACE PLATAFORMA E-LEARNING // INSECAP // {2}";

                            subject = string.Format(subject,
                   comercializacion.cotizacion.cliente.nombreEmpresa.ToUpper(),
                   comercializacion.cotizacion.curso.nombreCurso.ToUpper(),
                   comercializacion.cotizacion.codigoCotizacion.ToUpper()
                   );
                            var body = bodyHTML;
                            body = body.Replace("{0}", participante.contacto.nombreCompleto);
                            body = body.Replace("{1}", comercializacion.cotizacion.curso.nombreCurso.ToUpper());
                            body = body.Replace("{2}", comercializacion.cotizacion.cliente.nombreEmpresa.ToUpper());
                            body = body.Replace("{3}", bloqueString);
                            body = body.Replace("{4}", participante.contacto.runCompleto);
                            body = body.Replace("{5}", "chile");
                            body = body.Replace("{6}", comercializacion.fechaTermino.ToString("dd/MM/yyyy"));

                            Utils.Utils.SendMail(receiverEmail, subject, body);
                            receiverEmail = new MailAddress("contacto@insecap.email", participante.contacto.nombreCompleto);
                            Utils.Utils.SendMail(receiverEmail, subject, body);


                        }




                    }
                }
                catch (Exception e)
                {
                }
            }

            return Json(new
            {
                value = "Terminado"
            },
                   JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public ActionResult MailDJP()
        {

            DateTime hoy = DateTime.Now.Date.AddDays(-2);
            DateTime inicio = new DateTime(2021, 08, 03);
            var comercializaciones = db.Comercializacion
                 .Where(x => x.softDelete == false)
                 .Where(x => x.participantes.Count() != 0)
                 .Where(x => x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono)
                 .Where(x => x.cotizacion.tieneCodigoSence != "on" && x.cotizacion.codigoSence != null && x.cotizacion.codigoSence != "")
                 .Where(x => DbFunctions.TruncateTime(x.fechaTermino) >= inicio && DbFunctions.TruncateTime(x.fechaTermino) <= hoy)
                .ToList();

            //Correo config

            var bodyHTML = "";
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/djp.html")))
            {
                bodyHTML = reader.ReadToEnd();
            }


            foreach (Comercializacion comercializacion in comercializaciones)
            {
                try
                {

                    foreach (Participante participante in comercializacion.participantes.Where(x => !x.conDeclaracionJuradaPersona).ToList())
                    {


                        var receiverEmail = new MailAddress(participante.contacto.correo, participante.contacto.nombreCompleto);
                        //var receiverEmail = new MailAddress("jrodriguez@insecap.cl", participante.contacto.nombreCompleto);
                        String bloqueString = "<br> Usted dispone del siguiente rango de fechas para realizar el curso: <br> " + comercializacion.fechaInicio.ToString("dd/MM/yyyy") + " - " + comercializacion.fechaTermino.ToString("dd/MM/yyyy");
                        var subject = "RECORDATORIO DECLARACIÓN JURADA// {0} // {1} // ENLACE PLATAFORMA E-LEARNING // INSECAP // {2}";

                        subject = string.Format(subject,
               comercializacion.cotizacion.cliente.nombreEmpresa.ToUpper(),
               comercializacion.cotizacion.curso.nombreCurso.ToUpper(),
               comercializacion.cotizacion.codigoCotizacion.ToUpper()
               );
                        var body = bodyHTML;
                        body = body.Replace("{0}", participante.contacto.nombreCompleto);
                        body = body.Replace("{1}", comercializacion.cotizacion.curso.nombreCurso.ToUpper());
                        body = body.Replace("{2}", comercializacion.cotizacion.cliente.nombreEmpresa.ToUpper());
                        body = body.Replace("{3}", comercializacion.fechaInicio.ToString("dd/MM/yyyy"));
                        body = body.Replace("{4}", comercializacion.fechaTermino.ToString("dd/MM/yyyy"));

                        Utils.Utils.SendMail(receiverEmail, subject, body);
                        receiverEmail = new MailAddress("wcarvajar@insecap.cl", participante.contacto.nombreCompleto);
                        Utils.Utils.SendMail(receiverEmail, subject, body);


                    }
                }
                catch (Exception e)
                {
                }
            }

            return Json(new
            {
                value = "Terminado"
            },
                   JsonRequestBehavior.AllowGet);

        }
        //Correo Comercial
        [HttpGet]
        public ActionResult MailComercial()
        {
            var send = true;
            try
            {
                var bodyHTML = "";
                var subject = "**Curso ejecutado Post Venta**";
                using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/Comercial/mail.html")))
                {

                    bodyHTML = reader.ReadToEnd();
                }
                var hoy = DateTime.Now.Date.AddDays(-1);
                var comercializacions = db.Comercializacion
                .Where(x => x.softDelete == false)
                //.Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(z => z.fechaCreacion).Take(1).Any(y => y.EstadoComercializacion == EstadoComercializacion.En_Proceso) || x.comercializacionEstadoComercializacion.OrderByDescending(z => z.fechaCreacion).Take(1).Any(y => y.EstadoComercializacion == EstadoComercializacion.Terminada_SENCE))
                //.Where(x => DbFunctions.TruncateTime(x.fechaTermino) == hoy)
                .Where(x => x.fechaTermino.Year == hoy.Year && x.fechaTermino.Month == hoy.Month && x.fechaTermino.Day == hoy.Day)
                .Include(x => x.cotizacion.costo)
                .Include(x => x.cotizacion.cliente)
                .Include(x => x.cotizacion.curso)
                .ToList();


                foreach (Comercializacion comercializacion in comercializacions.ToList())
                {

                    List<Comercializacion> temp = comercializacions.Where(x => x.cotizacion.idCliente == comercializacion.cotizacion.idCliente && x.cotizacion.contacto == comercializacion.cotizacion.contacto).ToList();
                    if (temp.Count > 0)
                    {
                        Comercializacion comercializacionFirst = temp.FirstOrDefault();
                        List<String> correos = new List<string>();
                        ViewBag.domain = domain;
                        var tabla = ConvertPartialViewToString(PartialView("MailComercial", temp));
                        // obtener de aca los correos de todos los contactos o solo deun contacto 
                        correos.Add("contacto@insecap.email");
                        correos.Add(comercializacionFirst.cotizacion.usuarioCreador.Email);


                        String body = bodyHTML.Replace("{0}", comercializacionFirst.cotizacion.usuarioCreador.nombreCompleto);
                        //Incluir los correos si tiene varios creadores y los telefonos
                        body = body.Replace("{3}", "TICA ");
                        body = body.Replace("{1}", tabla);

                        foreach (String mail in correos)
                        {
                            var receiverEmail = new MailAddress(mail, "INSECAP Capacitación");
                            Utils.Utils.SendMail(receiverEmail, subject, body);
                        }
                        //SendMail(new MailAddress("jrodriguez@insecap.cl", "INSECAP Capacitación"), subject, body);
                        comercializacion.cotizacion.cliente.ultimaFechaEnvioCorreo = DateTime.Now;
                        db.Entry(comercializacion.cotizacion.cliente).State = EntityState.Modified;
                        db.SaveChanges();
                        comercializacions.RemoveAll(x => x.cotizacion.idCliente == comercializacion.cotizacion.idCliente && x.cotizacion.contacto == comercializacion.cotizacion.contacto);
                    }
                }
                MailLog("/Email/Log/cliente.txt", "Enviado Correctamente");
            }
            catch (Exception e)
            {
                MailLog("/Email/Log/cliente.txt", e.Message);
                send = false;
            }

            //Agrupar clientes iguales por contacto y id cliente en cotizacion 
            //Agregar tipo de ejecucion al correo
            //Validar todos los que no sean presenciales o recertificacion que tengan alumnos 

            return Json(new
            {
                mailSend = send
            },
                   JsonRequestBehavior.AllowGet);
        }

        //Correo Post Curso
        [HttpGet]
        public ActionResult AlertaPostCurso()
        {
            var send = true;
            try
            {
                var bodyHTML = "";
                using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/PostCurso/mail.html")))
                {
                    bodyHTML = reader.ReadToEnd();
                }
                List<AspNetUsers> netUsers = db.AspNetUsers.Where(x => x.AspNetRoles.Any(y => y.Name == "DigitaciónYPostCurso") == true).ToList();
                var hoy = DateTime.Now.Date.AddDays(-1);
                var comercializacions = db.Comercializacion
                .Where(x => x.softDelete == false)
                .Where(x => x.fechaTermino.Year == hoy.Year && x.fechaTermino.Month == hoy.Month && x.fechaTermino.Day == hoy.Day)
                .Include(x => x.cotizacion.costo)
                .Include(x => x.cotizacion.cliente)
                .Include(x => x.cotizacion.curso)
                .ToList();

                foreach (Comercializacion comercializacion in comercializacions.ToList())
                {
                    var subject = "ALERTA POST CURSO [" + comercializacion.cotizacion.codigoCotizacion + "] " + "[" + comercializacion.cotizacion.curso.tipoEjecucion + "]";
                    List<String> correos = new List<string>();
                    ViewBag.domain = domain;
                    var tabla = ConvertPartialViewToString(PartialView("alertaPostCurso", comercializacion));

                    // obtener de aca los correos de todos los contactos o solo deun contacto 
                    correos.AddRange(netUsers.Select(y => y.Email).ToList());
                    correos.Add("contacto@insecap.email");

                    String body = bodyHTML.Replace("{3}", "TICA ");
                    body = body.Replace("{1}", tabla);

                    foreach (String mail in correos)
                    {
                        var receiverEmail = new MailAddress(mail, "INSECAP Capacitación");
                        Utils.Utils.SendMail(receiverEmail, subject, body);
                    }
                }

                MailLog("/Email/Log/cliente.txt", "Enviado Correctamente");
            }
            catch (Exception e)
            {
                MailLog("/Email/Log/cliente.txt", e.Message);
                send = false;
            }

            //Agrupar clientes iguales por contacto y id cliente en cotizacion 
            //Agregar tipo de ejecucion al correo
            //Validar todos los que no sean presenciales o recertificacion que tengan alumnos 

            return Json(new
            {
                mailSend = send
            },
                   JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult MailSenceEjecutado()
        {
            var send = true;
            try
            {
                var bodyHTML = "";
                var subject = "**{0}//Curso SENCE ejecutado** ";
                using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/Sence/mail.html")))
                {

                    bodyHTML = reader.ReadToEnd();
                }
                List<AspNetUsers> netUsers = db.AspNetUsers.Where(x => x.AspNetRoles.Any(y => y.Name == "DigitaciónYPostCurso") == true).ToList();
                var hoy5 = DateTime.Now.Date.AddDays(-5);
                var hoy10 = DateTime.Now.Date.AddDays(-10);
                var hoy15 = DateTime.Now.Date.AddDays(-15);

                var comercializacions = db.Comercializacion
                .Where(x => x.softDelete == false)
                .Where(x => x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono
                || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Sincronica
                 || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Presencial
                  || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion
                )
                .Where(x => x.cotizacion.tieneCodigoSence != "on" && x.cotizacion.codigoSence != null && x.cotizacion.codigoSence != "")
                .Where(x => !x.comercializacionEstadoComercializacion.OrderByDescending(z => z.fechaCreacion).Any(y => y.EstadoComercializacion == EstadoComercializacion.Terminada) && !x.comercializacionEstadoComercializacion.OrderByDescending(z => z.fechaCreacion).Any(y => y.EstadoComercializacion == EstadoComercializacion.Terminada_SENCE) && !x.comercializacionEstadoComercializacion.OrderByDescending(z => z.fechaCreacion).Any(y => y.EstadoComercializacion == EstadoComercializacion.Cancelada) && !x.comercializacionEstadoComercializacion.OrderByDescending(z => z.fechaCreacion).Any(y => y.EstadoComercializacion == EstadoComercializacion.Borrador))
                .Where(x => DbFunctions.TruncateTime(x.fechaTermino) == hoy5 || DbFunctions.TruncateTime(x.fechaTermino) == hoy10 || DbFunctions.TruncateTime(x.fechaTermino) == hoy15 || DbFunctions.TruncateTime(x.fechaTermino) < hoy15)
                .Include(x => x.cotizacion.costo)
                .Include(x => x.cotizacion.cliente)
                .Include(x => x.cotizacion.curso)
                .ToList();

                foreach (Comercializacion comercializacion in comercializacions.ToList())
                {
                    List<String> correos = new List<string>();
                    correos.AddRange(netUsers.Select(y => y.Email).ToList());
                    ViewBag.domain = domain;
                    // obtener de aca los correos de todos los contactos o solo deun contacto 
                    correos.Add("contacto@insecap.email");
                    correos.Add(comercializacion.cotizacion.usuarioCreador.Email);

                    String body = bodyHTML.Replace("{0}", comercializacion.cotizacion.curso.nombreCurso + "(" + comercializacion.cotizacion.codigoCotizacion + ")");
                    //Incluir los correos si tiene varios creadores y los telefonos
                    body = body.Replace("{1}", String.Format("{0:dddd d , MMMM , yyyy}", comercializacion.fechaInicio).Replace(",", "de"));
                    body = body.Replace("{2}", Convert.ToString((DateTime.Now - comercializacion.fechaTermino).Days));
                    var tabla = "";
                    var listado = "";
                    if (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono || comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Sincronica)
                    {
                        listado = "Para asegurar su rápida gestión, adjuntamos listado de Alumnos y estado de DJP:";
                        tabla = ConvertPartialViewToString(PartialView("MailSence", comercializacion));
                    }
                    body = body.Replace("{5}", listado);
                    body = body.Replace("{3}", tabla);

                    body = body.Replace("{4}", "TICA ");
                    foreach (String mail in correos)
                    {

                        var receiverEmail = new MailAddress(mail, "INSECAP Capacitación");
                        Utils.Utils.SendMail(receiverEmail, subject.Replace("{0}", comercializacion.cotizacion.codigoCotizacion), body);
                    }
                }
                MailLog("/Email/Log/cliente.txt", "Enviado Correctamente");
            }
            catch (Exception e)
            {
                MailLog("/Email/Log/cliente.txt", e.Message);
                send = false;
            }

            //Agrupar clientes iguales por contacto y id cliente en cotizacion 
            //Agregar tipo de ejecucion al correo
            //Validar todos los que no sean presenciales o recertificacion que tengan alumnos 

            return Json(new
            {
                mailSend = send
            },
                   JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult MailAsincEjecutado()
        {
            var send = true;
            try
            {
                var bodyHTML = "";
                var subject = "INSECAP // ESTADO ALUMNOS CURSO // {1} // ASINCRONICO // {0}";
                using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/Asincronico/mail.html")))
                {

                    bodyHTML = reader.ReadToEnd();
                }

                var hoy = DateTime.Now.Date;
                var comercializacions = db.Comercializacion
                .Where(x => x.softDelete == false)
                .Where(x => DbFunctions.TruncateTime(x.fechaTermino) >= hoy && DbFunctions.TruncateTime(x.fechaInicio) <= hoy)
                .Where(x => x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono
                || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica
                )
                .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(z => z.fechaCreacion).Any(y => y.EstadoComercializacion == EstadoComercializacion.En_Proceso) && x.comercializacionEstadoComercializacion.Count() == 1)

                .Include(x => x.cotizacion.costo)
                .Include(x => x.cotizacion.cliente)
                .Include(x => x.cotizacion.curso)
                .ToList();

                foreach (Comercializacion comercializacion in comercializacions.ToList().Take(2))
                {
                    List<String> correos = new List<string>();
                    correos.Add(comercializacion.usuarioCreador.Email);
                    correos.Add(db.Contacto.Find(comercializacion.cotizacion.contacto).correo);
                    ViewBag.domain = domain;
                    // obtener de aca los correos de todos los contactos o solo deun contacto 
                    correos.Add("contacto@insecap.email");

                    String body = bodyHTML.Replace("{0}", comercializacion.cotizacion.curso.nombreCurso + " (" + comercializacion.cotizacion.codigoCotizacion + ")");
                    //Incluir los correos si tiene varios creadores y los telefonos
                    body = body.Replace("{2}", "del " + String.Format("{0:dddd d , MMMM , yyyy}", comercializacion.fechaInicio).Replace(",", "de") + " al " + String.Format("{0:dddd d , MMMM , yyyy}", comercializacion.fechaTermino).Replace(",", "de"));

                    UrlHelper u = new UrlHelper(this.ControllerContext.RequestContext);
                    string url = u.Action("NotasExcel", "Participante", new { id = comercializacion.idComercializacion });
                    body = body.Replace("{5}", "<a href=\"" + domain + url + "\">Descargar Excel</a>"); ;

                    body = body.Replace("{4}", "+56 " + comercializacion.usuarioCreador.telefono.Substring(comercializacion.usuarioCreador.telefono.Length - 9, comercializacion.usuarioCreador.telefono.Length));
                    foreach (String mail in correos)
                    {

                        var receiverEmail = new MailAddress(mail, "INSECAP Capacitación");
                        //var receiverEmail = new MailAddress("wcarvajal@insecap.cl", "INSECAP Capacitación");
                        Utils.Utils.SendMail(receiverEmail, string.Format(subject, comercializacion.cotizacion.codigoCotizacion, comercializacion.cotizacion.curso.nombreCurso.ToUpper()), body);
                    }
                }
                MailLog("/Email/Log/cliente.txt", "Enviado Correctamente");
            }
            catch (Exception e)
            {
                MailLog("/Email/Log/cliente.txt", e.Message);
                send = false;
            }

            //Agrupar clientes iguales por contacto y id cliente en cotizacion 
            //Agregar tipo de ejecucion al correo
            //Validar todos los que no sean presenciales o recertificacion que tengan alumnos 

            return Json(new
            {
                mailSend = send
            },
                   JsonRequestBehavior.AllowGet);
        }

        public ActionResult RegistroQR(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comercializacion comercializacion = db.Comercializacion.Find(id);

            try
            {
                ViewBag.qr = GenerarQR(Request.Url.Scheme + "://" + Request.Url.Authority + Url.Action("Registro", "Participante", new { id = comercializacion.idComercializacion }), comercializacion.idComercializacion);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            AgregarGrupoMoodle(id);
            return View(comercializacion);
        }

        private string GenerarQR(string qrcodeText, int id)
        {
            string folderPath = "~/Images/";
            string imagePath = "~/Images/QrCode-" + id + ".jpg";
            // If the directory doesn't exist then create it.
            if (!Directory.Exists(Server.MapPath(folderPath)))
            {
                Directory.CreateDirectory(Server.MapPath(folderPath));
            }

            var barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;
            barcodeWriter.Options = new QrCodeEncodingOptions { Height = 250, Width = 250, Margin = 0 };
            var result = barcodeWriter.Write(qrcodeText);

            string barcodePath = Server.MapPath(imagePath);
            var barcodeBitmap = new Bitmap(result);
            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream fs = new FileStream(barcodePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    barcodeBitmap.Save(memory, ImageFormat.Png);
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            return imagePath;
        }

        private void EnviarMailConfirmacinCursoRelator(Comercializacion comercializacion, RelatorCurso relator)
        {
            var senderEmail = new MailAddress(email, "Insecap");
            var receiverEmail = new MailAddress(relator.relator.contacto.correo, relator.relator.contacto.nombreCompleto);
            var copy = new MailAddress("insecap@gmail.com", "Insecap");

            var password = emailPassword;
            var subject = "CONFIRMACIÓN CURSO // {0} // INSECAP {1}";
            subject = String.Format(subject, comercializacion.cotizacion.curso.nombreCurso.ToUpper(), comercializacion.cotizacion.codigoCotizacion);
            var textoEmail = "Estimado/a {1},{0}{0}Favor confirmar que realizará el curso {2}, codigo {8} modalidad {7} para la empresa {4} con {5} participantes en las fechas: {0} {6} {0} , ingrese aquí {3}.{0}{0}Atte.{0}{0}Insecap";
            //var configEmail = db.TextoEmail.Where(x => x.email == "Confirmación Curso Relator").FirstOrDefault();
            String bloqueString = "Por definir";
            string tipoEjecucion = comercializacion.cotizacion.curso != null ? comercializacion.cotizacion.curso.tipoEjecucion.ToString() : " ";
            if (tipoEjecucion.ToString().Contains("Presencial") || tipoEjecucion.ToString().Contains("Recertificacion") || tipoEjecucion.ToString().Contains("Recertificacion_Sincronica") || tipoEjecucion.ToString().Contains("Elearning_Sincrono"))
            {
                bloqueString = "No hay bloques el día " + String.Format("{0:dddd d , MMMM , yyyy}", comercializacion.fechaInicio).Replace(",", "de");
                int bloqueCont = comercializacion.bloques.ToList().Count();
                if (bloqueCont > 0)
                {
                    bloqueString = "";
                    String dateString = "";
                    foreach (Bloque bloque in comercializacion.bloques)
                    {
                        String currentDate = String.Format("{0:dddd d , MMMM , yyyy}", bloque.fecha.Date).Replace(",", "de");
                        if (!dateString.Equals(currentDate))
                        {
                            bloqueString += "Bloques del día " + currentDate + ": ";
                            bloqueString += bloque.horarioInicio.ToString("HH:mm") + " - " + bloque.horarioTermino.ToString("HH:mm") + "";
                        }
                        else
                        {
                            bloqueString += bloque.horarioInicio.ToString("HH:mm") + " - " + bloque.horarioTermino.ToString("HH:mm") + "";
                        }
                        dateString = currentDate;

                    }
                }
            }
            var body = string.Format(textoEmail,
                Environment.NewLine,
                relator.relator.contacto.nombreCompleto,
                comercializacion.cotizacion.curso.nombreCurso,
                string.Format("{0}/Comercializacions/ConfirmarCurso/{1}-{2}",
                    domain, comercializacion.idComercializacion, relator.relator.idRelator)
                , comercializacion.cotizacion.cliente.nombreEmpresa,
                comercializacion.cotizacion.cantidadParticipante,
                bloqueString,
                tipoEjecucion.ToString(),
                comercializacion.cotizacion.codigoCotizacion
                );
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail.Address, password)
            };

            using (var mess = new MailMessage(senderEmail, receiverEmail)
            {
                Subject = subject,
                Body = body,
            }
            )
            {
                mess.CC.Add(copy);
                smtp.Send(mess);
            }

        }
    }
}