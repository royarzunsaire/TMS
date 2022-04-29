using jsreport.MVC;
using jsreport.Types;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SGC.CustomAuthorize;
using SGC.Models;
using SGC.Models.Feedback;
using SGC.Models.SQL;
using SGC.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Net;
using System.Net.Mail;

namespace SGC.Controllers
{
    public class ParticipanteController : Controller
    {
        private static readonly string directory = ConfigurationManager.AppSettings["directory"] + "Files/";
        private static readonly string email = ConfigurationManager.AppSettings["email"];
        private static readonly string emailPassword = ConfigurationManager.AppSettings["emailPassword"];
        private static readonly string domain = ConfigurationManager.AppSettings["domain"];
        private InsecapContext db = new InsecapContext();

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
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
        [CustomAuthorize(new string[] { "/Comercializacions/", "/ClienteContacto/" })]
        [HttpPost]
        // GET: Comercializacions
        public async Task<ActionResult> IndexData()
        {
            int start = Convert.ToInt32(Request["start"]);
            int draw = Convert.ToInt32(Request["draw"]);
            String search = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];
            int recordsTotal = 0;
            int count = Convert.ToInt32(Request["length"]);

            var dataDb = db.Contacto.Where(x => x.softDelete == false);
            var idUsuario = User.Identity.GetUserId();
            var roles = await UserManager.GetRolesAsync(idUsuario);
            bool isRepresentante = roles != null && roles.Count > 0 && roles.Contains("Representante Empresa");
            List<int> clientes = new List<int>();
            //buscar rol representante empresa
            if (isRepresentante)
            {
                clientes = db.ClienteContacto.Where(x => x.contacto.usuario.Id == idUsuario).Select(y => y.cliente.idCliente).ToList();

                dataDb = db.Participante
                .Where(x => clientes.Any(y => y == x.comercializacion.cotizacion.cliente.idCliente)).Select(x => x.contacto).Distinct();
            }

            DateTime dateSearch = DateTime.MinValue;
            DateTime.TryParse(search, out dateSearch);

            if (string.IsNullOrEmpty(search))
            {
                recordsTotal = dataDb.Count();
            }
            else
            {
                dataDb = dataDb.Where(x => x.nombres.ToLower().Contains(search)
                || x.apellidoMaterno.ToLower().Contains(search)
                || x.apellidoPaterno.ToLower().Contains(search)

                || (x.nombres.ToLower() + " " + x.apellidoPaterno.ToLower() + " " + x.apellidoMaterno.ToLower()).Contains(search)

                || x.run.ToLower().Contains(search)
                || x.correo.ToLower().Contains(search));
                recordsTotal = dataDb.Count();
            }

            if (count == -1) count = recordsTotal;
            var data = dataDb.OrderByDescending(x => x.fechaCreacion)
                .Skip(start)
                .Take(count)
                .ToList();

            List<object> resultset = new List<object>();
            foreach (Contacto contacto in data)
            {
                String tempLink;
                if (isRepresentante)
                {
                    int contadorCursos = 0;
                    foreach (int idCliente in clientes)
                    {
                        contadorCursos += db.Participante.Where(x => x.contacto.idContacto == contacto.idContacto
                        && x.comercializacion.cotizacion.cliente.idCliente == idCliente).Count();
                    }
                    tempLink = "<a target=\"_blank\" href=" + Url.Action("Participante", new { id = contacto.idContacto }) + " class='btn btn-sm btn - default' style='margin - right: 10px'> " + contadorCursos + " cursos (Click aquí) </ a > ";
                }
                else
                {
                    tempLink = "<a target=\"_blank\" href=" + Url.Action("Participante", new { id = contacto.idContacto }) + " class='btn btn-sm btn - default' style='margin - right: 10px'> " + db.Participante.Where(x => x.contacto.idContacto == contacto.idContacto)
                    .Count() + " cursos (Click aquí) </ a > ";
                }

                var edit = "<a target=\"_blank\" href=" + Url.Action("Edit", "Contacto", new { id = contacto.idContacto }) + " class='btn btn-warning btn-sm  glyphicon glyphicon-edit' style='margin - right: 10px'>  </ a > ";
                var ver = "<a target=\"_blank\" href=" + Url.Action("Details", "Contacto", new { id = contacto.idContacto }) + " class='btn btn-info btn-sm glyphicon glyphicon-list' style='margin - right: 10px'>  </ a > ";
                var telefono = contacto.telefono;
                if (telefono == "999999999" || telefono == "99999999") telefono = "Sin Teléfono";

                resultset.Add(
                    new
                    {
                        contacto.run,
                        contacto.nombreCompleto,
                        contacto.correo,
                        telefono,
                        cont = tempLink,
                        edit = edit + ver
                    }
                    );
            }
            var jsonResult = Json(new { draw, recordsTotal, recordsFiltered = recordsTotal, data = resultset }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }


        // GET: Participante
        [Authorize]
        public async System.Threading.Tasks.Task<ActionResult> Participante(int id)
        {
            var idUsuario = User.Identity.GetUserId();
            var roles = await UserManager.GetRolesAsync(idUsuario);
            bool isRepresentante = roles != null && roles.Count > 0 && roles.Contains("Representante Empresa");
            ViewBag.isRepresentante = isRepresentante;
            //Validar not found
            var contacto = db.Contacto.Where(x => x.idContacto == id).FirstOrDefault();
            if (id == null || contacto == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.foto = db.Participante.Where(x => x.contacto.idContacto == id && x.credenciales != null).FirstOrDefault();
            if (ViewBag.foto != null)
            {
                Files.borrarArchivosLocales();
                await Files.BajarArchivoADirectorioLocalAsync(ViewBag.foto.credenciales);
            }
            return View(contacto);
        }
        // GET: Participante
        public async System.Threading.Tasks.Task<ActionResult> ParticipanteQR(int id, string rut)
        {

            var contacto = db.Contacto.Where(x => x.idContacto == id).FirstOrDefault();
            if (rut == null || id == null || contacto == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (!contacto.run.Contains(rut))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //Validar not found
            ViewBag.foto = db.Participante.Where(x => x.contacto.idContacto == id && x.credenciales != null).FirstOrDefault();
            if (ViewBag.foto != null)
            {
                Files.borrarArchivosLocales();
                await Files.BajarArchivoADirectorioLocalAsync(ViewBag.foto.credenciales);
            }

            return View("Participante", contacto);
        }

        [HttpPost]
        // GET: Comercializacions
        public async Task<ActionResult> ParticipanteData()
        {
            int start = Convert.ToInt32(Request["start"]);
            int draw = Convert.ToInt32(Request["draw"]);
            String search = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];
            int recordsTotal = 0;
            int count = Convert.ToInt32(Request["length"]);
            int contactoId = Convert.ToInt32(Request["contactoId"]);

            bool action = Convert.ToBoolean(Request["action"]);

            // Identifica si el usuario es representante
            bool isRepresentante = false;
            var idUsuario = User.Identity.GetUserId();
            if (idUsuario == null)
            {
                isRepresentante = false;
            }
            else
            {
                var roles = await UserManager.GetRolesAsync(idUsuario);
                isRepresentante = roles != null && roles.Count > 0 && roles.Contains("Representante Empresa");
            }

            var participantes = db.Participante.Where(x => x.contacto.idContacto == contactoId).ToList();
            List<Comercializacion> dataDb;
            if (isRepresentante)
            {
                var clientes = db.ClienteContacto.Where(x => x.contacto.usuario.Id == idUsuario).Select(y => y.cliente.idCliente).ToList();
                dataDb = participantes.Where(x => !x.comercializacion.softDelete)
                .Where(x => clientes.Any(y => y == x.comercializacion.cotizacion.cliente.idCliente)).Select(x => x.comercializacion).ToList();
            }
            else
            {
                dataDb = participantes.Where(x => !x.comercializacion.softDelete).Select(x => x.comercializacion).ToList();
            }
            if (action)
                dataDb = dataDb.Where(x => x.clientDownload && x.clientFactura && x.cotizacion.cliente.situacionComercial != SituacionComercial.Pendiente).ToList();

            DateTime dateSearch = DateTime.MinValue;
            DateTime.TryParse(search, out dateSearch);

            if (string.IsNullOrEmpty(search))
            {
                recordsTotal = dataDb.Count();

            }
            else
            {
                dataDb = dataDb.Where(x => x.cotizacion.codigoCotizacion.ToLower().Contains(search)
            || x.cotizacion.curso.nombreCurso.ToLower().Contains(search)
            || x.cotizacion.cliente.nombreEmpresa.ToLower().Contains(search)
             || DateTime.Compare(x.fechaTermino, dateSearch) == 0
               || DateTime.Compare(x.fechaInicio, dateSearch) == 0
            ).ToList();
                recordsTotal = dataDb.Count();
            }

            if (count == -1)
            {
                count = recordsTotal;
            }
            var data = dataDb
                //.OrderByDescending(x => x.fechaCreacion)
                .Skip(start)
                .Take(count)
                .ToList();

            var i = 0;

            List<object> resultset = new List<object>();
            foreach (Comercializacion comercializacion in data)
            {
                var item = comercializacion.participantes.Where(x => x.contacto.idContacto == contactoId).FirstOrDefault();
                double nota = 0.0;
                double notaTeorica = 0.0;
                double contTeorica = 0;
                var asistencia = "-";
                var fecha = DateTime.MinValue;

                if (item != null)
                {
                    // nota y asustencia 

                    var cantBloques = comercializacion.bloques.Count();
                    var cantAsistencias = item.asistencia.Where(x => x.asistio == true).Count();
                    if (cantBloques > 0)
                    {

                        asistencia = String.Format("{0:N0}", cantAsistencias * 100 / cantBloques) + "%";

                    }

                    foreach (var evaluacion in comercializacion.evaluaciones)
                    {
                        if (evaluacion.categoria == CategoriaEvaluacion.Teorico)
                        {
                            if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault() != null)
                            {
                                if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != ""
                                    && item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != "-")
                                {

                                    if (item.notas.Any(x => x.evaluacion.nombre.ToLower().Contains("final")))
                                    {
                                        fecha = item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().fechaRealizacion;
                                    }

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
                    double notaPractica = 0.0;
                    double contPractica = 0;
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
                }

                var tempLink = "<a target=\"_blank\" href=" + Url.Action("List", "Participante", new { id = comercializacion.idComercializacion }) + " style='margin - right: 10px'> " + comercializacion.cotizacion.codigoCotizacion + " </ a > ";

                if (action || isRepresentante)
                    tempLink = comercializacion.cotizacion.codigoCotizacion;
                var tipoEjecucion = comercializacion.cotizacion.curso != null ? comercializacion.cotizacion.curso.tipoEjecucion.ToString() : " ";
                tipoEjecucion = tipoEjecucion.Replace("Recertificacion_Asincronica", "R-Asincronica");
                tipoEjecucion = tipoEjecucion.Replace("Elearning_Asincrono", "E-Asincrono");
                tipoEjecucion = tipoEjecucion.Replace("Elearning_Sincrono", "E-Sincrono");
                tipoEjecucion = tipoEjecucion.Replace("Recertificacion_Sincronica", "R-Sincronica");
                var bloques = string.Join(";", comercializacion.bloques.Select(x => x.horarioInicio.ToString("HH:mm") + " - " + x.horarioTermino.ToString("HH:mm")).ToList());
                var relatores = string.Join(";", comercializacion.bloques.Select(x => x.relator.contacto.nombreCompleto).Distinct().ToList());
                string notaString = string.Format("{0} (A:{1})</br>{2}"
                    , nota
                    , asistencia, fecha == DateTime.MinValue ? "Sin fecha de realización" : String.Format("{0:dd/MM/yyyy}", fecha));
                resultset.Add(
                    new
                    {
                        codigoCotizacion = tempLink,
                        comercializacion.cotizacion.curso.nombreCurso,
                        comercializacion.cotizacion.cliente.nombreEmpresa,
                        tipoEjecucion,
                        fechaInicio = String.Format("{0:dd/MM/yyyy}", comercializacion.fechaInicio),
                        fechaTermino = String.Format("{0:dd/MM/yyyy}", comercializacion.fechaTermino),
                        vigenciaCredenciales = String.Format("{0:dd/MM/yyyy}", comercializacion.fechaTermino.AddMonths(comercializacion.vigenciaCredenciales)),

                        relatores,
                        comercial = comercializacion.usuarioCreador.nombres + " " + comercializacion.usuarioCreador.apellidoPaterno,
                        bloques,
                        nota = notaString,
                        menu = ConvertPartialViewToString(PartialView("IndexMenu", participantes.ElementAt(i)))
                    }
                    );
                i++;
            }

            var jsonResult = Json(new { draw, recordsTotal, recordsFiltered = recordsTotal, data = resultset }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        // GET: Participante/LandingPage
        public ActionResult LandingPage()
        {
            var idUsuario = User.Identity.GetUserId();
            var comercializaciones = db.Comercializacion
                .Where(x => x.softDelete == false)
                .Join(
                    db.Participante
                        .Where(x => x.contacto.usuario.Id == idUsuario),
                    comercializacion => comercializacion.idComercializacion,
                    participante => participante.comercializacion.idComercializacion,
                    (comercializacion, participante) => new ViewModelLandingPageParticipante()
                    {
                        comercializacion = comercializacion,
                        participante = participante
                    }
                ).ToList();
            return View(comercializaciones);
        }

        // GET: Participante
        [CustomAuthorize(new string[] { "/Comercializacions/", "/ClienteContacto/" })]
        public async Task<ActionResult> Index()
        {
            var idUsuario = User.Identity.GetUserId();
            var roles = await UserManager.GetRolesAsync(idUsuario);
            bool isRepresentante = roles != null && roles.Count > 0 && roles.Contains("Representante Empresa");

            return View(isRepresentante);
        }

        //// GET: Participante/test
        //public ActionResult test()
        //{
        //    var test = Moodle.test(db.ParametrosMoodles.FirstOrDefault());
        //    return View("test", "", test);
        //}

        // GET: Participante/List/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]

        public ActionResult List(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");
            var comercializacion = db.Comercializacion.Find(id);
            ViewBag.idComercializacion = id;
            ViewBag.codigoComercializacion = comercializacion.cotizacion.codigoCotizacion;
            ViewBag.nombreCurso = comercializacion.cotizacion.curso.nombreCurso;
            ViewBag.tipoEjecucion = comercializacion.cotizacion.curso.tipoEjecucion;
            ViewBag.salas = String.Join(",", comercializacion.bloques.Select(x => x.sala.nombre).Distinct());
            if (TempData["ModelState"] != null && !ModelState.Equals(TempData["ModelState"]))
            {
                ModelState.Merge((ModelStateDictionary)TempData["ModelState"]);
                TempData["ModelState"] = null;
            }

            ViewBag.linkComercializacion = db.LinkComercializacion.Where(c => c.comercializacion.idComercializacion == comercializacion.idComercializacion).FirstOrDefault();
            string domain = ConfigurationManager.AppSettings["domain"];
            @ViewBag.link = domain + "/Comercializacions/RedirectVideoLLamadaAlumnosComercializacion?id=" + comercializacion.idComercializacion;

            ViewBag.typesLink = new SelectList(db.LinkTypes.ToList().Select(c => new SelectListItem
            {
                Text = c.nombre,
                Value = c.idLinkType.ToString()
            }).ToList(), "Value", "Text");

            return View(db.Participante.Where(p => p.comercializacion.idComercializacion == id).ToList());
        }

        // GET: Participante/Details/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Participante participante = db.Participante.Find(id);
            if (participante == null)
            {
                return HttpNotFound();
            }
            return View(participante);
        }

        // GET: Participante/Notas/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Encuesta(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var feedbackMoodle = db.FeedbackMoodle.FirstOrDefault(x => x.comercializacion.idComercializacion == id);
            if (feedbackMoodle == null)
            {
                feedbackMoodle = new FeedbackMoodle
                {
                    comercializacion = db.Comercializacion.Find(id),
                    feedbackItemMoodle = new List<FeedbackItemMoodle>(),
                    feedbackItemCommentMoodle = new List<FeedbackItemCommentMoodle>()

                };

            }

            return View(feedbackMoodle);
        }


        // GET: Participante/Notas/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Notas(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var comercializacion = db.Comercializacion.Find(id);

            return View(comercializacion);
        }

        public bool GetNotas(int? id)
        {
            try
            {
                if (id == null)
                {
                    return false;
                }
                var comercializacion = db.Comercializacion.Find(id);
                if (comercializacion == null)
                {
                    return false;
                }

                if (comercializacion.evaluaciones.Where(x => x.softDelete == false).ToList().Count() == 0)
                {
                    return false;
                }


                if (DateTime.Compare(DateTime.Now.Date, comercializacion.fechaInicio.Date) < 0 || DateTime.Compare(DateTime.Now.Date, comercializacion.fechaTermino.Date) > 0)
                {
                    Moodle.AgregarParticipantesGrupoMoodle(comercializacion.participantes.Where(x => x.contacto.idUsuarioMoodle != null).Select(x => x.contacto).ToList(), comercializacion, db.ParametrosMoodles.FirstOrDefault());

                    //foreach (Participante participante in comercializacion.participantes)
                    //{
                    //    var temp = Moodle.AgregarParticipanteCursoMoodle(participante.contacto, comercializacion.cotizacion.curso, db.ParametrosMoodles.FirstOrDefault(), DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-6));

                    //}

                }

                var notasParticipantes = Moodle.GetNotasGrupoMoodle(comercializacion, db.ParametrosMoodles.FirstOrDefault());
                if (notasParticipantes == null)
                {
                    return false;
                }

                updateNotas(notasParticipantes, comercializacion.idComercializacion, false);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }
        // GET: Participante/ObtenerNotas/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult ObtenerNotas(int? id, string returnUrl)
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
            if (returnUrl == null)
            {
                returnUrl = "Notas";
            }
            ViewBag.returnUrl = Request.UrlReferrer;
            if (comercializacion.evaluaciones.Where(x => x.softDelete == false).ToList().Count() == 0)
            {
                ModelState.AddModelError("", "No se encontraron evaluaciones en la comercialización");
                //ViewBag.cantNotas = CantNotas(comercializacion);
                return View(returnUrl, comercializacion);
            }


            if (DateTime.Compare(DateTime.Now.Date, comercializacion.fechaTermino.Date) > 0)
            {

                Moodle.AgregarParticipantesCursoMoodle(comercializacion.participantes.Where(x => x.contacto.idUsuarioMoodle != null).Select(x => x.contacto).ToList(), comercializacion.cotizacion.curso, db.ParametrosMoodles.FirstOrDefault(), DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-6));
                //foreach (Participante participante in comercializacion.participantes)
                //{
                //    var temp = Moodle.AgregarParticipantesCursoMoodle(participante.contacto, comercializacion.cotizacion.curso, db.ParametrosMoodles.FirstOrDefault(), DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-6));

                //}

            }


            var notasParticipantes = Moodle.GetNotasGrupoMoodle(comercializacion, db.ParametrosMoodles.FirstOrDefault());
            if (notasParticipantes == null)
            {
                ModelState.AddModelError("", "Se produjo un error al intentar obtener las calificaciones de la plataforma Moodle");
                //ViewBag.cantNotas = CantNotas(comercializacion);
                return View(returnUrl, comercializacion);
            }




            updateNotas(notasParticipantes, comercializacion.idComercializacion, false);
            //Actualizar las fechas de matriculas de moodle
            if (comercializacion.participantes.Count() > 0)
            {
                UpdateUserEnrolments(comercializacion.participantes.ToList(), comercializacion.participantes.FirstOrDefault().comercializacion.cotizacion.curso.idCursoMoodle);

            }

            return RedirectToAction("Notas", "Participante", new { id = comercializacion.idComercializacion });
        }
        // GET: Participante/ObtenerNotasAtrasadas/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult ObtenerNotasAtrasadas(int? id, string returnUrl)
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
            if (returnUrl == null)
            {
                returnUrl = "Notas";
            }
            ViewBag.returnUrl = Request.UrlReferrer;
            if (comercializacion.evaluaciones.Where(x => x.softDelete == false).ToList().Count() == 0)
            {
                ModelState.AddModelError("", "No se encontraron evaluaciones en la comercialización");
                //ViewBag.cantNotas = CantNotas(comercializacion);
                return View(returnUrl, comercializacion);
            }

            if (DateTime.Compare(DateTime.Now.Date, comercializacion.fechaTermino.Date) > 0)
            {

                Moodle.AgregarParticipantesCursoMoodle(comercializacion.participantes.Where(x => x.contacto.idUsuarioMoodle != null).Select(x => x.contacto).ToList(), comercializacion.cotizacion.curso, db.ParametrosMoodles.FirstOrDefault(), DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-6));
                //foreach (Participante participante in comercializacion.participantes)
                //{
                //    var temp = Moodle.AgregarParticipantesCursoMoodle(participante.contacto, comercializacion.cotizacion.curso, db.ParametrosMoodles.FirstOrDefault(), DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-6));

                //}
            }

            var notasParticipantes = Moodle.GetNotasGrupoMoodle(comercializacion, db.ParametrosMoodles.FirstOrDefault());
            if (notasParticipantes == null)
            {
                ModelState.AddModelError("", "Se produjo un error al intentar obtener las calificaciones de la plataforma Moodle");
                //ViewBag.cantNotas = CantNotas(comercializacion);
                return View(returnUrl, comercializacion);
            }

            updateNotas(notasParticipantes, comercializacion.idComercializacion, true);
            //Actualizar las fechas de matriculas de moodle
            if (comercializacion.participantes.Count() > 0)
            {
                UpdateUserEnrolments(comercializacion.participantes.ToList(), comercializacion.participantes.FirstOrDefault().comercializacion.cotizacion.curso.idCursoMoodle);

            }

            return RedirectToAction("Notas", "Participante", new { id = comercializacion.idComercializacion });
        }

        [HttpGet]
        public ActionResult GetAllFeedback()
        {
            var comercializaciones = db.Comercializacion.Where(x => x.cotizacion.tipoCurso == "Curso" && x.fechaTermino <= DateTime.Today).ToList();
            foreach (var comercializacion in comercializaciones)
            {
                try
                {
                    GetFeedback(comercializacion.idComercializacion);

                }
                catch (Exception e)
                {

                }

            }
            return View(comercializaciones);
        }

        public bool GetAllFeedbackDaily()
        {
            var hoy = DateTime.Now.Date;
            var comercializacions = db.Comercializacion
                 .Where(x => DateTime.Compare(hoy, x.fechaTermino) == 0)
                .ToList();
            foreach (var comer in comercializacions)
            {
                try
                {
                    if (comer.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono || comer.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono)

                        GetFeedback(comer.idComercializacion);
                }
                catch (Exception e)
                {

                }

            }
            return true;
        }

        public bool GetAllAsistenciaDiaria()
        {
            var hoy = DateTime.Now.Date;
            var comercializacions = db.Comercializacion
                 .Where(x => DateTime.Compare(hoy, x.fechaTermino) == 0)
                .ToList();
            foreach (var comer in comercializacions)
            {
                try
                {
                    GetFeedback(comer.idComercializacion);
                }
                catch (Exception e)
                {

                }

            }
            return true;
        }
        // GET: Participante/GetFeedback/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult GetFeedback(int? id)
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

            var change = true;
            var feedback = Moodle.GetFeedback(comercializacion, db.ParametrosMoodles.FirstOrDefault());
            var user = db.AspNetUsers.Find(User.Identity.GetUserId());
            FeedbackMoodle feedbackDb = db.FeedbackMoodle.FirstOrDefault(x => x.comercializacion.idComercializacion == id);
            if (feedbackDb != null)
            {
                //change = feedback.completedcount != feedbackDb.completedcount || feedback.itemscount != feedbackDb.itemscount;
                if (change)
                {
                    var comment = db.FeedbackItemCommentMoodle.Where(x => x.feedbackMoodle.idFeedbackMoodle == feedbackDb.idFeedbackMoodle).ToList();
                    comment.ForEach(x => db.Entry(x).State = EntityState.Deleted);
                    var feedbackItemMoodle = db.FeedbackItemMoodle.Where(x => x.feedbackMoodle.idFeedbackMoodle == feedbackDb.idFeedbackMoodle).ToList();
                    foreach (var item in feedbackItemMoodle)
                    {
                        var data = db.FeedbackItemDataMoodle.Where(x => x.feedbackItemMoodle.idFeedbackItemMoodle == item.idFeedbackItemMoodle).ToList();
                        data.ForEach(x => db.Entry(x).State = EntityState.Deleted);
                    }
                    db.SaveChanges();
                    feedbackItemMoodle.ForEach(x => db.Entry(x).State = EntityState.Deleted);

                    feedbackDb.completedcount = feedback.completedcount;
                    feedbackDb.itemscount = feedback.itemscount;
                    feedbackDb.comercializacion = comercializacion;
                    feedbackDb.usuario = user;
                    feedbackDb.lastUpdate = DateTime.Now;
                    db.SaveChanges();

                }
            }
            else
            {
                if (feedback != null)
                {
                    feedbackDb = new FeedbackMoodle
                    {
                        completedcount = feedback.completedcount,
                        itemscount = feedback.itemscount,
                        comercializacion = comercializacion,
                        usuario = user,
                        lastUpdate = DateTime.Now
                    };
                    db.FeedbackMoodle.Add(feedbackDb);
                    db.SaveChanges();
                }

            }

            if (feedback != null && feedback.itemsdata != null && change)
            {
                foreach (var item in feedback.itemsdata)
                {
                    FeedbackItemMoodle feedbackItem = new FeedbackItemMoodle
                    {
                        feedback = item.item.feedback,
                        typ = item.item.label,
                        name = item.item.name,
                        presentation = item.item.presentation,
                        feedbackMoodle = feedbackDb
                    };
                    db.FeedbackItemMoodle.Add(feedbackItem);
                    db.SaveChanges();
                    if (item.item.typ.Contains("textarea"))
                    {
                        foreach (var itemData in item.data.Where(x => x != "" && x != " "))
                        {
                            FeedbackItemCommentMoodle feedbackItemCommentMoodle = new FeedbackItemCommentMoodle
                            {

                                value = itemData.Replace("<br />", ""),
                                feedbackMoodle = feedbackDb

                            };
                            db.FeedbackItemCommentMoodle.Add(feedbackItemCommentMoodle);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        foreach (var itemData in item.dataObject)
                        {

                            FeedbackItemDataMoodle feedbackItemDataMoodle = new FeedbackItemDataMoodle
                            {
                                answercount = itemData.answercount,
                                answertext = itemData.answertext,
                                value = String.Format("{0}", (Convert.ToInt32(itemData.value) + 2) * 10),
                                feedbackItemMoodle = feedbackItem

                            };
                            db.FeedbackItemDataMoodle.Add(feedbackItemDataMoodle);
                            db.SaveChanges();
                        }
                    }
                }
            }

            return RedirectToAction("Encuesta", new { id = comercializacion.idComercializacion });

        }

        // GET: Participante/GetFeedback/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult GetQuizAprendizaje(int? id)
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

            var QuizAprendizaje = Moodle.GetQuizAprendizaje(comercializacion, db.ParametrosMoodles.FirstOrDefault());


            if (QuizAprendizaje != null && QuizAprendizaje.Count() > 0)
            {
                foreach (var test in QuizAprendizaje)
                {
                    string userid = test.userid.ToString();
                    var resultado = "";
                    if (db.QuizAprendizajeParticipantesResultados.Where(x => x.participante.contacto.idUsuarioMoodle == userid
                    && x.comercializacion.idComercializacion == comercializacion.idComercializacion).Any() == false)
                    {
                        foreach (var respuesta in test.responses)
                        {
                            var respuestas = new QuizAprendizajeParticipantesRespuestas
                            {
                                quizAprendizajePreguntas = db.QuizAprendizajePreguntas.Where(x => x.pregunta == respuesta.name).FirstOrDefault(),
                                quizAprendizajeRespuestas = db.QuizAprendizajeRespuestas.Where(x => x.codigoRespuesta == respuesta.rawval).FirstOrDefault(),
                                participante = db.Participante.Where(x => x.contacto.idUsuarioMoodle == userid && x.comercializacion.idComercializacion == comercializacion.idComercializacion).FirstOrDefault(),
                                comercializacion = comercializacion,
                                fecha = UnixTimeStampToDateTime(test.timemodified)
                            };

                            db.QuizAprendizajeParticipantesRespuestas.Add(respuestas);

                            switch (respuesta.rawval)
                            {
                                case 1:
                                    resultado += "a";
                                    break;
                                case 2:
                                    resultado += "b";
                                    break;
                                case 3:
                                    resultado += "c";
                                    break;
                            }
                            if (resultado.Length == 1 || resultado.Length == 3)
                            {
                                resultado += "-";
                            }
                        }

                        var resultados = new QuizAprendizajeParticipantesResultados
                        {
                            quizAprendizajeResultados = db.QuizAprendizajeResultados.Where(x => x.resultado == resultado).FirstOrDefault(),
                            fecha = UnixTimeStampToDateTime(test.timemodified),
                            enviado = false,
                            participante = db.Participante.Where(x => x.contacto.idUsuarioMoodle == userid && x.comercializacion.idComercializacion == comercializacion.idComercializacion).FirstOrDefault(),
                            comercializacion = comercializacion
                        };
                        db.QuizAprendizajeParticipantesResultados.Add(resultados);
                        db.SaveChanges();
                        CorreoResultadoQuizAprendizaje(comercializacion.idComercializacion, db.Participante.Where(x => x.contacto.idUsuarioMoodle == userid && x.comercializacion.idComercializacion == comercializacion.idComercializacion).Select(x => x.idParticipante).FirstOrDefault());
                    }
                }

            }

            return RedirectToAction("QuizAprendizaje", new { id = comercializacion.idComercializacion });
        }

        [HttpGet]
        public ActionResult QuizAprendizaje(int id)
        {
            var resultado = db.QuizAprendizajeParticipantesResultados.Where(x => x.comercializacion.idComercializacion == id).ToList();
            var comercializacion = db.Comercializacion.Find(id);
            ViewBag.idComercializacion = comercializacion.idComercializacion;
            ViewBag.nombreCurso = comercializacion.cotizacion.curso.nombreCurso;
            ViewBag.codigoCotizacion = comercializacion.cotizacion.codigoCotizacion;

            return View(resultado);
        }

        public ActionResult UpdateTest()
        {
            SavePreguntas();

            return RedirectToAction("Index");
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public bool SavePreguntas()
        {
            var pregunta1 = new QuizAprendizajePreguntas
            {
                idQuizAprendizajePreguntas = 1,
                pregunta = "Al enfrentarte a un nuevo desafío (armar un mueble) en el que se requiere que desarrolles una habilidad en específico, imaginando que te encuentras solo en una habitación con las herramientas necesarias ¿Qué método prefieres?"
            };

            var pregunta2 = new QuizAprendizajePreguntas
            {
                idQuizAprendizajePreguntas = 2,
                pregunta = "Tienes la intención de preparar una cena especial para tu familia, sin embargo, se trata de una receta que jamás has realizado ¿Qué harías?:"
            };

            var pregunta3 = new QuizAprendizajePreguntas
            {
                idQuizAprendizajePreguntas = 3,
                pregunta = "Estás redactando una carta y no estás seguro de cómo se deletrea la palabra “ecenario” o “escenario” ¿qué harías?"
            };

            db.QuizAprendizajePreguntas.Add(pregunta1);
            db.QuizAprendizajePreguntas.Add(pregunta2);
            db.QuizAprendizajePreguntas.Add(pregunta3);

            SaveRespuestas(pregunta1, pregunta2, pregunta3);
            SaveResultados();

            db.SaveChanges();

            return true;
        }

        public bool SaveRespuestas(QuizAprendizajePreguntas pregunta1, QuizAprendizajePreguntas pregunta2, QuizAprendizajePreguntas pregunta3)
        {
            var respuesta1_1 = new QuizAprendizajeRespuestas
            {
                idQuizAprendizajeRespuestas = 1,
                quizAprendizajePreguntas = pregunta1,
                codigoRespuesta = 1,
                respuesta = "Buscar mucha información que te sirva de apoyo para aprender esa nueva habilidad. (Utilizar manuales, seguir instrucciones escritas, tutoriales, etc.)"
            };
            var respuesta1_2 = new QuizAprendizajeRespuestas
            {
                idQuizAprendizajeRespuestas = 2,
                quizAprendizajePreguntas = pregunta1,
                codigoRespuesta = 2,
                respuesta = "Intentar poner en práctica esa habilidad desde un comienzo pese a no poseerla."
            };
            var respuesta1_3 = new QuizAprendizajeRespuestas
            {
                idQuizAprendizajeRespuestas = 3,
                quizAprendizajePreguntas = pregunta1,
                codigoRespuesta = 3,
                respuesta = "Prefieres llamar a alguien que te explique cómo se hace paso a paso."
            };
            var respuesta2_1 = new QuizAprendizajeRespuestas
            {
                idQuizAprendizajeRespuestas = 4,
                quizAprendizajePreguntas = pregunta2,
                codigoRespuesta = 1,
                respuesta = "Realizarla a tu manera e ir probando frecuentemente el sabor de esta para guiarte."
            };
            var respuesta2_2 = new QuizAprendizajeRespuestas
            {
                idQuizAprendizajeRespuestas = 5,
                quizAprendizajePreguntas = pregunta2,
                codigoRespuesta = 2,
                respuesta = "Guiarte con una receta familiar que tenga imágenes explicativas, además de ver un tutorial."
            };
            var respuesta2_3 = new QuizAprendizajeRespuestas
            {
                idQuizAprendizajeRespuestas = 6,
                quizAprendizajePreguntas = pregunta2,
                codigoRespuesta = 3,
                respuesta = "Seguir las instrucciones de un familiar o conocido que haya cocinado ese plato anteriormente."
            };
            var respuesta3_1 = new QuizAprendizajeRespuestas
            {
                idQuizAprendizajeRespuestas = 7,
                quizAprendizajePreguntas = pregunta3,
                codigoRespuesta = 1,
                respuesta = "Indagar en un diccionario para saber cómo se escribe"
            };
            var respuesta3_2 = new QuizAprendizajeRespuestas
            {
                idQuizAprendizajeRespuestas = 8,
                quizAprendizajePreguntas = pregunta3,
                codigoRespuesta = 2,
                respuesta = "Escribir ambas palabras en una hoja y escoger una de las dos"
            };
            var respuesta3_3 = new QuizAprendizajeRespuestas
            {
                idQuizAprendizajeRespuestas = 9,
                quizAprendizajePreguntas = pregunta3,
                codigoRespuesta = 3,
                respuesta = "Meditar la palabra en tu mente o repetirla en voz baja y, según como suene mejor para ti, escribirla"
            };

            db.QuizAprendizajeRespuestas.Add(respuesta1_1);
            db.QuizAprendizajeRespuestas.Add(respuesta1_2);
            db.QuizAprendizajeRespuestas.Add(respuesta1_3);
            db.QuizAprendizajeRespuestas.Add(respuesta2_1);
            db.QuizAprendizajeRespuestas.Add(respuesta2_2);
            db.QuizAprendizajeRespuestas.Add(respuesta2_3);
            db.QuizAprendizajeRespuestas.Add(respuesta3_1);
            db.QuizAprendizajeRespuestas.Add(respuesta3_2);
            db.QuizAprendizajeRespuestas.Add(respuesta3_3);

            return true;
        }
        public bool SaveResultados()
        {
            var resultado1 = new QuizAprendizajeResultados
            {
                resultado = "a-b-c",
                tipoAprendizaje = "Visual",
                descripcion = "Se refiere a las personas que aprenden preferentemente mediante la observación. Pueden tener dificultad para recordar instrucciones y mensajes verbales.Durante charlas o conferencias prefieren seguir la conferencia en fotocopias y trípticos o tomar notas antes que seguir la explicación oral.Para estudiar prefieren leer o hacer resúmenes."
            };
            var resultado2 = new QuizAprendizajeResultados
            {
                resultado = "a-a-b",
                tipoAprendizaje = "Práctica (Kinésica)",
                descripcion = "Se refiere a las personas que aprenden por medio de las actividades físicas. Ellos aprenden cuando hacen cosas, a través del movimiento y la manipulación física. Necesitan moverse constantemente y tienden a buscar cualquier pretexto para levantarse."
            };
            var resultado3 = new QuizAprendizajeResultados
            {
                resultado = "c-c-a",
                tipoAprendizaje = "Auditiva",
                descripcion = "Se refiere a las personas que aprenden mejor cuando reciben la información oralmente y cuando pueden hablar y explicar esa información a otra persona. Tienen facilidad de palabra y expresan sus emociones verbalmente. Tienden a tener éxito cuando las instrucciones son dadas en voz alta o ellos deben responder oralmente."
            };
            var resultado4 = new QuizAprendizajeResultados
            {
                resultado = "a-a-a",
                tipoAprendizaje = "Integral",
                descripcion = "Este tipo de personas no se inclina hacia una preferencia de aprendizaje preferentemente, sino que se adapta al contexto educativo. Por lo general son personas muy adaptables a la hora de recibir información e impartirla."
            };
            var resultado5 = new QuizAprendizajeResultados
            {
                resultado = "a-c-b",
                tipoAprendizaje = "Integral",
                descripcion = "Este tipo de personas no se inclina hacia una preferencia de aprendizaje preferentemente, sino que se adapta al contexto educativo. Por lo general son personas muy adaptables a la hora de recibir información e impartirla."
            };
            var resultado6 = new QuizAprendizajeResultados
            {
                resultado = "b-b-a",
                tipoAprendizaje = "Integral",
                descripcion = "Este tipo de personas no se inclina hacia una preferencia de aprendizaje preferentemente, sino que se adapta al contexto educativo. Por lo general son personas muy adaptables a la hora de recibir información e impartirla."
            };
            var resultado7 = new QuizAprendizajeResultados
            {
                resultado = "b-c-b",
                tipoAprendizaje = "Integral",
                descripcion = "Este tipo de personas no se inclina hacia una preferencia de aprendizaje preferentemente, sino que se adapta al contexto educativo. Por lo general son personas muy adaptables a la hora de recibir información e impartirla."
            };
            var resultado8 = new QuizAprendizajeResultados
            {
                resultado = "b-c-c",
                tipoAprendizaje = "Integral",
                descripcion = "Este tipo de personas no se inclina hacia una preferencia de aprendizaje preferentemente, sino que se adapta al contexto educativo. Por lo general son personas muy adaptables a la hora de recibir información e impartirla."
            };
            var resultado9 = new QuizAprendizajeResultados
            {
                resultado = "c-a-c",
                tipoAprendizaje = "Integral",
                descripcion = "Este tipo de personas no se inclina hacia una preferencia de aprendizaje preferentemente, sino que se adapta al contexto educativo. Por lo general son personas muy adaptables a la hora de recibir información e impartirla."
            };
            var resultado10 = new QuizAprendizajeResultados
            {
                resultado = "c-b-b",
                tipoAprendizaje = "Integral",
                descripcion = "Este tipo de personas no se inclina hacia una preferencia de aprendizaje preferentemente, sino que se adapta al contexto educativo. Por lo general son personas muy adaptables a la hora de recibir información e impartirla."
            };
            var resultado11 = new QuizAprendizajeResultados
            {
                resultado = "b-b-c",
                tipoAprendizaje = "Visual con tendencia práctica",
                descripcion = "Las personas de preferencia visual con tendencia práctica son personas que, pese a que aprenden preferentemente mediante la observación, apoyándose mayormente en material visual como guías, trípticos, fotocopias, apuntes, etc. También en menor porción les ayuda aprender el movimiento y la manipulación de objetos o, en otras palabras, cuando se incluye práctica en la teoría."
            };
            var resultado12 = new QuizAprendizajeResultados
            {
                resultado = "a-a-c",
                tipoAprendizaje = "Visual con tendencia práctica",
                descripcion = "Las personas de preferencia visual con tendencia práctica son personas que, pese a que aprenden preferentemente mediante la observación, apoyándose mayormente en material visual como guías, trípticos, fotocopias, apuntes, etc. También en menor porción les ayuda aprender el movimiento y la manipulación de objetos o, en otras palabras, cuando se incluye práctica en la teoría."
            };
            var resultado13 = new QuizAprendizajeResultados
            {
                resultado = "a-b-b",
                tipoAprendizaje = "Visual con tendencia práctica",
                descripcion = "Las personas de preferencia visual con tendencia práctica son personas que, pese a que aprenden preferentemente mediante la observación, apoyándose mayormente en material visual como guías, trípticos, fotocopias, apuntes, etc. También en menor porción les ayuda aprender el movimiento y la manipulación de objetos o, en otras palabras, cuando se incluye práctica en la teoría."
            };
            var resultado14 = new QuizAprendizajeResultados
            {
                resultado = "c-b-c",
                tipoAprendizaje = "Visual con tendencia auditiva",
                descripcion = "Las personas de preferencia visual con tendencia auditiva son personas que, pese a que aprenden preferentemente mediante la observación, apoyándose mayormente en material visual como guías, trípticos, fotocopias, apuntes, etc. También en menor porción requieren de la instrucción oral a la hora de adquirir conocimientos."
            };
            var resultado15 = new QuizAprendizajeResultados
            {
                resultado = "a-c-c",
                tipoAprendizaje = "Visual con tendencia auditiva",
                descripcion = "Las personas de preferencia visual con tendencia auditiva son personas que, pese a que aprenden preferentemente mediante la observación, apoyándose mayormente en material visual como guías, trípticos, fotocopias, apuntes, etc. También en menor porción requieren de la instrucción oral a la hora de adquirir conocimientos."
            };
            var resultado16 = new QuizAprendizajeResultados
            {
                resultado = "a-b-a",
                tipoAprendizaje = "Visual con tendencia auditiva",
                descripcion = "Las personas de preferencia visual con tendencia auditiva son personas que, pese a que aprenden preferentemente mediante la observación, apoyándose mayormente en material visual como guías, trípticos, fotocopias, apuntes, etc. También en menor porción requieren de la instrucción oral a la hora de adquirir conocimientos."
            };
            var resultado17 = new QuizAprendizajeResultados
            {
                resultado = "a-a-b",
                tipoAprendizaje = "Práctica con tendencia visual",
                descripcion = "Las personas de preferencia práctica (kinésica) con tendencia visual necesitan poner en práctica aquello que están aprendiendo y por medio de la experiencia integran con mayor facilidad los conocimientos nuevos, pero también se apoyan con menor frecuencia en material visual que sirve como referencia a su proceso experimental."
            };
            var resultado18 = new QuizAprendizajeResultados
            {
                resultado = "b-b-b",
                tipoAprendizaje = "Práctica con tendencia visual",
                descripcion = "Las personas de preferencia práctica (kinésica) con tendencia visual necesitan poner en práctica aquello que están aprendiendo y por medio de la experiencia integran con mayor facilidad los conocimientos nuevos, pero también se apoyan con menor frecuencia en material visual que sirve como referencia a su proceso experimental."
            };
            var resultado19 = new QuizAprendizajeResultados
            {
                resultado = "b-a-c",
                tipoAprendizaje = "Práctica con tendencia visual",
                descripcion = "Las personas de preferencia práctica (kinésica) con tendencia visual necesitan poner en práctica aquello que están aprendiendo y por medio de la experiencia integran con mayor facilidad los conocimientos nuevos, pero también se apoyan con menor frecuencia en material visual que sirve como referencia a su proceso experimental."
            };
            var resultado20 = new QuizAprendizajeResultados
            {
                resultado = "c-a-b",
                tipoAprendizaje = "Práctica con tendencia auditiva",
                descripcion = "Las personas de preferencia práctica (kinésica) con tendencia auditiva necesitan poner en práctica aquello que están aprendiendo y por medio de la experiencia integran con mayor facilidad los conocimientos nuevos, pero también requieren de instrucción oral y/o supervisión ocasional durante su proceso experimental de aprendizaje."
            };
            var resultado21 = new QuizAprendizajeResultados
            {
                resultado = "b-c-b",
                tipoAprendizaje = "Práctica con tendencia auditiva",
                descripcion = "Las personas de preferencia práctica (kinésica) con tendencia auditiva necesitan poner en práctica aquello que están aprendiendo y por medio de la experiencia integran con mayor facilidad los conocimientos nuevos, pero también requieren de instrucción oral y/o supervisión ocasional durante su proceso experimental de aprendizaje."
            };
            var resultado22 = new QuizAprendizajeResultados
            {
                resultado = "b-a-a",
                tipoAprendizaje = "Práctica con tendencia auditiva",
                descripcion = "Las personas de preferencia práctica (kinésica) con tendencia auditiva necesitan poner en práctica aquello que están aprendiendo y por medio de la experiencia integran con mayor facilidad los conocimientos nuevos, pero también requieren de instrucción oral y/o supervisión ocasional durante su proceso experimental de aprendizaje."
            };
            var resultado23 = new QuizAprendizajeResultados
            {
                resultado = "a-c-a",
                tipoAprendizaje = "Auditiva con tendencia visual",
                descripcion = "Las personas de preferencia auditiva con tendencia visual requieren de la explicación e interacción oral constante con el docente en su proceso de aprendizaje, sin embargo, también mantienen un equilibrio entre prestar atención a lo que se dicta y el material gráfico disponible."
            };
            var resultado24 = new QuizAprendizajeResultados
            {
                resultado = "c-b-a",
                tipoAprendizaje = "Auditiva con tendencia visual",
                descripcion = "Las personas de preferencia auditiva con tendencia visual requieren de la explicación e interacción oral constante con el docente en su proceso de aprendizaje, sin embargo, también mantienen un equilibrio entre prestar atención a lo que se dicta y el material gráfico disponible."
            };
            var resultado25 = new QuizAprendizajeResultados
            {
                resultado = "c-c-c",
                tipoAprendizaje = "Auditiva con tendencia visual",
                descripcion = "Las personas de preferencia auditiva con tendencia visual requieren de la explicación e interacción oral constante con el docente en su proceso de aprendizaje, sin embargo, también mantienen un equilibrio entre prestar atención a lo que se dicta y el material gráfico disponible."
            };
            var resultado26 = new QuizAprendizajeResultados
            {
                resultado = "b-c-a",
                tipoAprendizaje = "Auditiva con tendencia práctica",
                descripcion = "Las personas de preferencia auditiva con tendencia práctica (kinésica) requieren de la explicación e interacción oral constante con el docente en su proceso de aprendizaje, sin embargo, necesitan poner en práctica aquello de lo que se le habla. Normalmente expresan sus ideas tanto con un lenguaje verbal como un lenguaje corporal."
            };
            var resultado27 = new QuizAprendizajeResultados
            {
                resultado = "c-a-a",
                tipoAprendizaje = "Auditiva con tendencia práctica",
                descripcion = "Las personas de preferencia auditiva con tendencia práctica (kinésica) requieren de la explicación e interacción oral constante con el docente en su proceso de aprendizaje, sin embargo, necesitan poner en práctica aquello de lo que se le habla. Normalmente expresan sus ideas tanto con un lenguaje verbal como un lenguaje corporal."
            };
            var resultado28 = new QuizAprendizajeResultados
            {
                resultado = "c-c-b",
                tipoAprendizaje = "Auditiva con tendencia práctica",
                descripcion = "Las personas de preferencia auditiva con tendencia práctica (kinésica) requieren de la explicación e interacción oral constante con el docente en su proceso de aprendizaje, sin embargo, necesitan poner en práctica aquello de lo que se le habla. Normalmente expresan sus ideas tanto con un lenguaje verbal como un lenguaje corporal."
            };

            db.QuizAprendizajeResultados.Add(resultado1);
            db.QuizAprendizajeResultados.Add(resultado2);
            db.QuizAprendizajeResultados.Add(resultado3);
            db.QuizAprendizajeResultados.Add(resultado4);
            db.QuizAprendizajeResultados.Add(resultado5);
            db.QuizAprendizajeResultados.Add(resultado6);
            db.QuizAprendizajeResultados.Add(resultado7);
            db.QuizAprendizajeResultados.Add(resultado8);
            db.QuizAprendizajeResultados.Add(resultado9);
            db.QuizAprendizajeResultados.Add(resultado10);
            db.QuizAprendizajeResultados.Add(resultado11);
            db.QuizAprendizajeResultados.Add(resultado12);
            db.QuizAprendizajeResultados.Add(resultado13);
            db.QuizAprendizajeResultados.Add(resultado14);
            db.QuizAprendizajeResultados.Add(resultado15);
            db.QuizAprendizajeResultados.Add(resultado16);
            db.QuizAprendizajeResultados.Add(resultado17);
            db.QuizAprendizajeResultados.Add(resultado18);
            db.QuizAprendizajeResultados.Add(resultado19);
            db.QuizAprendizajeResultados.Add(resultado20);
            db.QuizAprendizajeResultados.Add(resultado21);
            db.QuizAprendizajeResultados.Add(resultado22);
            db.QuizAprendizajeResultados.Add(resultado23);
            db.QuizAprendizajeResultados.Add(resultado24);
            db.QuizAprendizajeResultados.Add(resultado25);
            db.QuizAprendizajeResultados.Add(resultado26);
            db.QuizAprendizajeResultados.Add(resultado27);
            db.QuizAprendizajeResultados.Add(resultado28);

            return true;
        }

        [HttpGet]
        public ActionResult getFeedbackAll()
        {


            return RedirectToAction("Participantes");

        }
        public void updateNotas(MoodleSearchUserGrades notasParticipantes, int idComercializacion, bool notaAtrasada, AspNetUsers user = null)
        {
            Comercializacion comercializacion = db.Comercializacion.Find(idComercializacion);
            user = user == null ? db.AspNetUsers.Find(User.Identity.GetUserId()) : null;
            //intentos



            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            foreach (var notaParticipante in notasParticipantes.usergrades)
            {
                var participante = comercializacion.participantes.Where(p => p.contacto.idUsuarioMoodle == notaParticipante.userid).FirstOrDefault();

                if (participante != null)
                {
                    //intentos


                    //idParticipantes.Add("'"+participante.contacto.idUsuarioMoodle+ "'");
                    if (participante.notas == null)
                    {
                        participante.notas = new List<Notas>();
                    }
                    int cont = 0;
                    foreach (var grade in notaParticipante.gradeitems)
                    {
                        if (cont + 1 == notaParticipante.gradeitems.Count())
                        {
                            break;
                        }


                        Evaluacion evaluacion = comercializacion.evaluaciones.Where(e => e.idQuizMoodle == grade.iteminstance).FirstOrDefault();

                        Notas nota = new Notas();
                        nota.nota = grade.gradeformatted.Replace(".", ",");
                        //nota.porcentaje = grade.percentageformatted;
                        nota.porcentaje = 0;
                        nota.descripcion = grade.itemname;
                        nota.idNotaMoodle = grade.id;
                        nota.participante = participante;
                        nota.evaluacion = evaluacion;
                        nota.manual = false;
                        nota.fechaIngresoManual = DateTime.Now;
                        nota.usuarioIngreso = user;

                        dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        dtDateTime = dtDateTime.AddSeconds(Convert.ToDouble(grade.gradedatesubmitted)).ToLocalTime();
                        bool dentroDelRango = true;
                        nota.fechaRealizacion = dtDateTime;

                        if (evaluacion != null)
                        {
                            var oldNota = participante.notas.Where(n => n.idNotaMoodle == nota.idNotaMoodle).Where(n => n.evaluacion.idEvaluacion == nota.evaluacion.idEvaluacion).FirstOrDefault();

                            if (notaAtrasada == false)
                            {
                                if (nota.descripcion.ToLower().Contains("final"))
                                {

                                    if (nota.nota != "-" && (DateTime.Compare(nota.fechaRealizacion.Date, comercializacion.fechaInicio.Date) < 0 || DateTime.Compare(nota.fechaRealizacion.Date, comercializacion.fechaTermino.AddDays(1).Date) > 0))
                                    {
                                        dentroDelRango = false;
                                        nota.nota = "-";
                                    }
                                }
                            }

                            if (oldNota == null)
                            {
                                participante.notas.Add(nota);
                            }
                            else
                            {
                                //if (oldNota.manual && nota.descripcion.ToLower().Contains("final") && DateTime.Compare(nota.fechaRealizacion.Date, comercializacion.fechaInicio.Date) >= 0 && DateTime.Compare(nota.fechaRealizacion.Date, comercializacion.fechaTermino.AddDays(1).Date) <= 0)
                                if (oldNota.manual && oldNota.nota != "-" && Convert.ToDouble(oldNota.nota == "-" ? "1" : oldNota.nota) >= Convert.ToDouble(nota.nota == "-" ? "1" : nota.nota))
                                {
                                    nota.nota = oldNota.nota;
                                    nota.manual = oldNota.manual;
                                    nota.usuarioIngreso = oldNota.usuarioIngreso;
                                    nota.fechaIngresoManual = oldNota.fechaIngresoManual;
                                }

                                if (((DateTime.Compare(nota.fechaRealizacion, oldNota.fechaRealizacion) > 0) && (dentroDelRango == true || notaAtrasada == true)) || (DateTime.Compare(nota.fechaRealizacion, oldNota.fechaRealizacion) == 0 && oldNota.nota != grade.gradeformatted.Replace(".", ",") && nota.nota != "-"))
                                {
                                    participante.notas.Where(n => n.idNotaMoodle == nota.idNotaMoodle).Where(n => n.evaluacion.idEvaluacion == nota.evaluacion.idEvaluacion).FirstOrDefault().manual = nota.manual;
                                    participante.notas.Where(n => n.idNotaMoodle == nota.idNotaMoodle).Where(n => n.evaluacion.idEvaluacion == nota.evaluacion.idEvaluacion).FirstOrDefault().usuarioIngreso = nota.usuarioIngreso;
                                    participante.notas.Where(n => n.idNotaMoodle == nota.idNotaMoodle).Where(n => n.evaluacion.idEvaluacion == nota.evaluacion.idEvaluacion).FirstOrDefault().fechaIngresoManual = nota.fechaIngresoManual;

                                    participante.notas.Where(n => n.idNotaMoodle == nota.idNotaMoodle).Where(n => n.evaluacion.idEvaluacion == nota.evaluacion.idEvaluacion).FirstOrDefault().nota = nota.nota;
                                    participante.notas.Where(n => n.idNotaMoodle == nota.idNotaMoodle).Where(n => n.evaluacion.idEvaluacion == nota.evaluacion.idEvaluacion).FirstOrDefault().descripcion = nota.descripcion;
                                    participante.notas.Where(n => n.idNotaMoodle == nota.idNotaMoodle).Where(n => n.evaluacion.idEvaluacion == nota.evaluacion.idEvaluacion).FirstOrDefault().fechaRealizacion = nota.participante.notas.Where(n => n.idNotaMoodle == nota.idNotaMoodle).Where(n => n.evaluacion.idEvaluacion == nota.evaluacion.idEvaluacion).FirstOrDefault().fechaRealizacion = nota.fechaRealizacion;
                                }

                                if (nota.nota != null && nota.nota != "-" && nota.descripcion.ToLower().Contains("final") && (comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono || comercializacion.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica))
                                {
                                    foreach (Asistencia asistencia in db.Asistencias.Where(x => x.participante.idParticipante == participante.idParticipante).ToList())
                                    {
                                        asistencia.asistio = true;
                                        db.Entry(asistencia).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }


                                }
                                db.Entry(participante.notas.Where(n => n.idNotaMoodle == nota.idNotaMoodle).Where(n => n.evaluacion.idEvaluacion == nota.evaluacion.idEvaluacion).FirstOrDefault()).State = EntityState.Modified;
                            }
                        }
                        cont++;
                    }
                }
            }
            // actualizar intentos
            string idEvaluacionMoodle = "0";
            Evaluacion evaluacionFinal = comercializacion.evaluaciones.FirstOrDefault(e => e.nombre.ToLower().Contains("final"));

            if (evaluacionFinal != null)
            {
                idEvaluacionMoodle = "'" + evaluacionFinal.idQuizMoodle + "'";

            }
            try
            {
                if (comercializacion.participantes.Count() > 0)
                {
                    UpdateUserEnrolments(comercializacion.participantes.ToList(), comercializacion.cotizacion.curso.idCursoMoodle);

                    List<string> idParticipantes = comercializacion.participantes.Select(x => "'" + x.contacto.idUsuarioMoodle + "'").ToList();
                    var sql = string.Format("SELECT	mdl8s_quiz_attempts.id, mdl8s_quiz_attempts.quiz, mdl8s_quiz_attempts.userid, mdl8s_quiz_attempts.attempt, mdl8s_quiz_attempts.timestart, mdl8s_quiz_attempts.timefinish, mdl8s_quiz_attempts.timemodified, mdl8s_quiz_attempts.sumgrades, mdl8s_quiz.sumgrades AS totalgrades FROM mdl8s_quiz_attempts 	INNER JOIN	mdl8s_quiz ON mdl8s_quiz_attempts.quiz = mdl8s_quiz.id WHERE	mdl8s_quiz_attempts.quiz IN ({0}) AND mdl8s_quiz_attempts.userid IN ({1}) AND state='finished'",
                    idEvaluacionMoodle, string.Join(",", idParticipantes));
                    var modelAttempts = MySqlUtils.ExecuteSelectQuery(sql, typeof(mdl8s_quiz_attempts));
                    if (modelAttempts.GetType() != typeof(List<ErrorException>))
                    {
                        foreach (mdl8s_quiz_attempts attempt in modelAttempts)
                        {
                            bool exist = db.Attempts.Any(x => x.idAttemptsMoodle == attempt.id && x.quiz == attempt.quiz && x.userid == attempt.userid);
                            if (!exist)
                            {
                                dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                                Attempts newAttempts = new Attempts
                                {
                                    idAttemptsMoodle = attempt.id,
                                    quiz = attempt.quiz,
                                    userid = attempt.userid,
                                    attempt = attempt.attempt,
                                    sumGrades = attempt.sumgrades,
                                    totalGrades = attempt.totalgrades,
                                    timeFinish = dtDateTime.AddSeconds(Convert.ToDouble(attempt.timefinish)).ToLocalTime(),
                                    participante = comercializacion.participantes.FirstOrDefault(x => Convert.ToInt32(x.contacto.idUsuarioMoodle) == attempt.userid),
                                    evaluacion = evaluacionFinal
                                };
                                db.Attempts.Add(newAttempts);
                            }
                        }


                        sql = string.Format("select * from mdl8s_quiz_overrides where userid IN ({1}) AND quiz IN ({0})",
                       idEvaluacionMoodle,
                        string.Join(",", idParticipantes));
                        var modelOver = MySqlUtils.ExecuteSelectQuery(sql, typeof(mdl8s_quiz_overrides));
                        if (modelOver.GetType() != typeof(List<ErrorException>))
                        {
                            foreach (mdl8s_quiz_overrides overrides in modelOver)
                            {
                                bool exist = db.AttemptsQuizUser.Any(x => x.userid == overrides.userid && x.quiz == overrides.quiz);

                                if (!exist)
                                {
                                    AttemptsQuizUser attemptsQuizUser = new AttemptsQuizUser
                                    {

                                        quiz = overrides.quiz,
                                        userid = overrides.userid,
                                        attempt = overrides.attempts,

                                        participante = comercializacion.participantes.FirstOrDefault(x => Convert.ToInt32(x.contacto.idUsuarioMoodle) == overrides.userid),
                                        evaluacion = evaluacionFinal

                                    };
                                    db.AttemptsQuizUser.Add(attemptsQuizUser);
                                }


                            }

                        }

                    }

                }


            }
            catch (Exception e) { }

            db.SaveChanges();
        }
        // POST: Participante/DeletePorCliente/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Acciones(string idComercializacion, string participantes, string idaction, DateTime fechaMatricula)
        {
            List<int> l2 = participantes.Split(',').ToList().ConvertAll<int>(x => Convert.ToInt32(x));

            if (idaction == "eliminar")
            {
                return DeleteAll(idComercializacion, l2);
            }
            else if (idaction == "matricula")
            {
                return UpdateMatricula(idComercializacion, l2, fechaMatricula);
            }
            return RedirectToAction("List", "Participante", new { id = idComercializacion });
        }
        public ActionResult DeleteAll(string idComercializacion, List<int> participantes)
        {
            List<Participante> participante = db.Participante.Where(x => participantes.Any(y => y == x.idParticipante) == true).ToList();

            foreach (Participante item in participante)
            {
                if (item.agregadoAGrupo)
                {
                    Moodle.RemoverParticipanteGrupoMoodle(item.contacto, item.comercializacion, db.ParametrosMoodles.FirstOrDefault());
                    Moodle.EliminarParticipanteCursoMoodle(item.contacto, item.comercializacion.cotizacion.curso, db.ParametrosMoodles.FirstOrDefault());

                }
                if (item.credenciales != null)
                {
                    Files.BorrarArchivo(item.credenciales);
                    db.Storages.Remove(item.credenciales);
                }
            }

            db.Asistencias.RemoveRange(db.Asistencias.Where(x => participantes.Any(y => y == x.participante.idParticipante) == true).ToList());
            db.Attempts.RemoveRange(db.Attempts.Where(x => participantes.Any(y => y == x.participante.idParticipante) == true).ToList());
            db.AttemptsQuizUser.RemoveRange(db.AttemptsQuizUser.Where(x => participantes.Any(y => y == x.participante.idParticipante) == true).ToList());
            db.RespuestaEvaluacion.RemoveRange(db.RespuestaEvaluacion.Where(x => participantes.Any(y => y == x.nota.idParticipante) == true).ToList());
            db.Notas.RemoveRange(db.Notas.Where(x => participantes.Any(y => y == x.participante.idParticipante) == true).ToList());
            db.Participante.RemoveRange(participante);
            //db.Contacto.Remove(participante.contacto);
            db.SaveChanges();

            return RedirectToAction("List", "Participante", new { id = idComercializacion });
        }
        public ActionResult UpdateMatricula(string idComercializacion, List<int> participantes, DateTime fechaMatricula)
        {
            List<Participante> participante = db.Participante.Where(x => participantes.Any(y => y == x.idParticipante) == true).ToList();
            if (participante.Count() > 0)
            {
                ActualizarMatricula(participante, participante.Select(x => x.contacto.idUsuarioMoodle).ToList(), participante.FirstOrDefault().comercializacion.cotizacion.curso.idCursoMoodle, fechaMatricula);

            }
            return RedirectToAction("List", "Participante", new { id = idComercializacion });
        }
        private void UpdateUserEnrolments(List<Participante> participantes, string idCurso)
        {

            List<string> idUsuarioMoodle = participantes.Select(x => x.contacto.idUsuarioMoodle).ToList();
            var user_enrolments = GetUserEnrolments(idUsuarioMoodle, idCurso);
            if (user_enrolments.GetType() != typeof(List<ErrorException>))
            {
                foreach (mdl8s_user_enrolments enrol in user_enrolments)
                {
                    Participante participante = participantes.FirstOrDefault(x => x.contacto.idUsuarioMoodle == Convert.ToString(enrol.userid));
                    System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

                    participante.timestart = dtDateTime.AddSeconds(enrol.timestart).ToLocalTime();
                    participante.timeend = dtDateTime.AddSeconds(enrol.timeend).ToLocalTime();

                    db.Entry(participante).State = EntityState.Modified;

                }
                db.SaveChanges();
            }
        }
        public List<object> GetUserEnrolments(List<string> idUsuarioMoodle, string idCurso)
        {
            var sql = string.Format("SELECT mdl8s_user_enrolments.id,mdl8s_user_enrolments.status,mdl8s_user_enrolments.enrolid,mdl8s_user_enrolments.userid,mdl8s_user_enrolments.modifierid,mdl8s_user_enrolments.timecreated,mdl8s_user_enrolments.timemodified,mdl8s_user_enrolments.timeend,mdl8s_user_enrolments.timestart FROM mdl8s_user_enrolments INNER JOIN mdl8s_enrol ON mdl8s_user_enrolments.enrolid = mdl8s_enrol.id INNER JOIN mdl8s_course ON mdl8s_enrol.courseid = mdl8s_course.id WHERE mdl8s_user_enrolments.userid IN ({0}) AND mdl8s_course.idnumber IN ('{1}')",

              string.Join(",", idUsuarioMoodle), idCurso);
            return MySqlUtils.ExecuteSelectQuery(sql, typeof(mdl8s_user_enrolments));

        }
        public ActionResult ActualizarMatricula(List<Participante> participantes, List<string> idUsuarioMoodle, string idCurso, DateTime fechaMatricula)
        {

            if (idUsuarioMoodle == null || idUsuarioMoodle.Count() == 0 || idCurso == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user_enrolments = GetUserEnrolments(idUsuarioMoodle, idCurso);
            ErrorException error = null;
            if (user_enrolments.GetType() != typeof(List<ErrorException>))
            {

                if (user_enrolments.Count() > 0)
                {

                    fechaMatricula = new DateTime(
        fechaMatricula.Year,
        fechaMatricula.Month,
        fechaMatricula.Day, 23, 59, 00, 00,
        fechaMatricula.Kind);

                    foreach (mdl8s_user_enrolments enrol in user_enrolments)
                    {

                        DateTime unixStart = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
                        long epoch = (long)Math.Floor((fechaMatricula.ToUniversalTime() - unixStart).TotalSeconds);
                        enrol.timeend = epoch;

                        var update = MySqlUtils.ExecuteUpdateQuery(enrol, typeof(mdl8s_user_enrolments), "mdl8s_user_enrolments", new List<string> { "id" });
                        if (update.GetType() != typeof(List<ErrorException>))
                        {
                            Participante participante = participantes.FirstOrDefault(x => x.contacto.idUsuarioMoodle == Convert.ToString(enrol.userid));
                            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

                            participante.timestart = dtDateTime.AddSeconds(enrol.timestart).ToLocalTime();
                            participante.timeend = fechaMatricula;
                            db.Entry(participante).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                        else
                        {
                            error = (ErrorException)user_enrolments.FirstOrDefault();
                            ModelState.AddModelError("", error.message);
                        }
                    }
                }
            }
            else
            {
                error = (ErrorException)user_enrolments.FirstOrDefault();
                ModelState.AddModelError("", error.message);
            }
            return RedirectToAction("Notas", "Participante");
        }

        public ActionResult AgregarIntento(int idUsuarioMoodle, int idEvaluacion, int idComercializacion, string accion)
        {

            if (idUsuarioMoodle == null || idEvaluacion == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            bool addIntento = false;
            var sql = string.Format("select * from mdl8s_quiz_overrides where userid IN ({0}) AND quiz IN ({1})",

            "'" + idUsuarioMoodle + "'", "'" + idEvaluacion + "'");
            var modelOver = MySqlUtils.ExecuteSelectQuery(sql, typeof(mdl8s_quiz_overrides));

            sql = string.Format("SELECT	mdl8s_quiz_attempts.id, mdl8s_quiz_attempts.quiz, mdl8s_quiz_attempts.userid, mdl8s_quiz_attempts.attempt, mdl8s_quiz_attempts.timestart, mdl8s_quiz_attempts.timefinish, mdl8s_quiz_attempts.timemodified, mdl8s_quiz_attempts.sumgrades, mdl8s_quiz.sumgrades AS totalgrades FROM mdl8s_quiz_attempts 	INNER JOIN	mdl8s_quiz ON mdl8s_quiz_attempts.quiz = mdl8s_quiz.id WHERE	mdl8s_quiz_attempts.quiz IN ({0}) AND mdl8s_quiz_attempts.userid IN ({1}) AND state='finished'",
             "'" + idEvaluacion + "'", "'" + idUsuarioMoodle + "'");
            var modelAttempts = MySqlUtils.ExecuteSelectQuery(sql, typeof(mdl8s_quiz_attempts));
            if (modelAttempts.Count() > 0)
            {
                int countFinished = modelAttempts.Select(x => (mdl8s_quiz_attempts)x).Max(x => x.attempt);
                var update = (mdl8s_quiz_overrides)modelOver.FirstOrDefault();
                if (update != null)
                {
                    if (update.attempts < (countFinished + 1))
                    {
                        addIntento = true;
                        update.attempts = countFinished + 1;
                        MySqlUtils.ExecuteUpdateQuery(update, typeof(mdl8s_quiz_overrides), "mdl8s_quiz_overrides", new List<string> { "id", "quiz" });
                    }
                }
                else
                {
                    addIntento = true;
                    var intento = new mdl8s_quiz_overrides
                    {
                        id = 0,
                        attempts = countFinished + 1,
                        userid = idUsuarioMoodle,
                        quiz = idEvaluacion
                    };
                    MySqlUtils.ExecuteInsertQuery(intento, typeof(mdl8s_quiz_overrides), "mdl8s_quiz_overrides");
                }
            }
            if (addIntento)
            {
                var attemptsEnd = db.Attempts.Where(x => x.quiz == idEvaluacion && x.userid == idUsuarioMoodle).ToList();
                if (attemptsEnd.Count() > 0)
                {
                    var attemptsQuizUser = db.AttemptsQuizUser.FirstOrDefault(x => x.quiz == idEvaluacion && x.userid == idUsuarioMoodle);
                    if (attemptsQuizUser != null)
                    {
                        attemptsQuizUser.attempt++;
                        db.Entry(attemptsQuizUser).State = EntityState.Modified;
                    }
                    else
                    {
                        var newAttemptsQuizUser = new AttemptsQuizUser
                        {
                            quiz = attemptsEnd.FirstOrDefault().quiz,
                            userid = attemptsEnd.FirstOrDefault().userid,
                            attempt = attemptsEnd.Count() + 1,

                            participante = attemptsEnd.FirstOrDefault().participante,
                            evaluacion = attemptsEnd.FirstOrDefault().evaluacion

                        };
                        db.AttemptsQuizUser.Add(newAttemptsQuizUser);
                    };
                    db.SaveChanges();
                }

            }
            if (accion.ToLower().Contains("resumen"))
                return RedirectToAction("Resumen", "Participante", new { id = idComercializacion });
            return RedirectToAction("Notas", "Participante", new { id = idComercializacion });
        }
        public SelectList GetContactos(Comercializacion comercializacion)
        {
            var contactos = db.Contacto
                .Where(c => c.softDelete == false)
                .Where(c => c.tipoContacto == TipoContacto.Participante)
                .ToList();
            var contactosValidos = new List<Contacto>();
            foreach (var contacto in contactos)
            {
                var valido = true;
                foreach (var participante in comercializacion.participantes)
                {
                    if (contacto.idContacto == participante.contacto.idContacto)
                    {
                        valido = false;
                        break;
                    }
                }
                if (valido)
                {
                    contactosValidos.Add(contacto);
                }
            }
            return new SelectList(contactosValidos
                .Select(con => new SelectListItem
                {
                    Text = "[" + con.run + "]" + " " + con.nombres + " " + con.apellidoPaterno + " " + con.apellidoMaterno,
                    Value = con.idContacto.ToString()
                })
                .ToList(), "Value", "Text");
        }
        private string CrearUsuarioParticipante(Contacto contacto)
        {
            var configuracionUsuarioParticipante = db.ConfiguracionUsuarioParticipante.FirstOrDefault();

            var user = new ApplicationUser
            {
                UserName = contacto.run,
                Email = contacto.correo,
                EmailConfirmed = true
            };

            // Then create:
            var adminresult = UserManager.Create(user, configuracionUsuarioParticipante.contrasenia);

            string[] roles = { configuracionUsuarioParticipante.rol.Name };

            //Add User to the selected Roles 
            if (adminresult.Succeeded)
            {
                var result = UserManager.AddToRoles(user.Id, roles);
            }
            else
            {
                ModelState.AddModelError("", adminresult.Errors.First());
            }
            return user.Id;
        }

        private bool ExisteConfiguracionParticipante()
        {
            if (db.ConfiguracionUsuarioParticipante.FirstOrDefault() == null)
            {
                ModelState.AddModelError("", "No se encontró la configuración del participante.");
            }
            return db.ConfiguracionUsuarioParticipante.FirstOrDefault() != null ? true : false;
        }

        // GET: Participante/Create
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Create(int? id)
        {
            Comercializacion comercializacion = db.Comercializacion.Find(id);
            ViewBag.idComercializacion = id;
            ViewBag.contactos = GetContactos(comercializacion);
            return View();
        }
        private void ValidarContacto(Contacto contactoValidar)
        {
            ViewBag.error = false;
            var contacto = db.Contacto
                .Where(c => c.softDelete == false)
                .Where(c => c.run == contactoValidar.run)
                .Where(c => c.tipoContacto == TipoContacto.Participante)
                .FirstOrDefault();
            if (contacto != null)
            {
                ViewBag.error = true;
                ModelState.AddModelError("", "El particpante ingresado ya existe");
            }
            contacto = db.Contacto
                .Where(c => c.softDelete == false)
                .Where(c => c.correo == contactoValidar.correo)
                .Where(c => c.tipoContacto == TipoContacto.Participante)
                .FirstOrDefault();
            if (contacto != null)
            {
                ViewBag.error = true;
                ModelState.AddModelError("contacto.correo", "El Correo Electrónico ingresado ya existe");
            }
        }

        // POST: Participante/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idParticipante,contacto")] Participante participante, int idComercializacion)
        {
            ValidarContacto(participante.contacto);
            Comercializacion comercializacion = db.Comercializacion.Find(idComercializacion);
            ValidarCantParticipantes(comercializacion);
            participante.comercializacion = comercializacion;
            participante.contacto.fechaCreacion = DateTime.Now;
            participante.contacto.usuarioCreador = User.Identity.GetUserId();
            participante.contacto.softDelete = false;
            participante.contacto.tipoContacto = TipoContacto.Participante;
            // validar que exista configuracion particiapnte
            ExisteConfiguracionParticipante();
            if (ModelState.IsValid)
            {
                var idUsuario = CrearUsuarioParticipante(participante.contacto);
                participante.contacto.usuario = db.AspNetUsers.Find(idUsuario);
                participante.contacto.usuario.tipo = TipoUsuario.parcial;
                db.Participante.Add(participante);
                db.SaveChanges();
                return RedirectToAction("List", new { id = idComercializacion });
            }
            ViewBag.idComercializacion = idComercializacion;
            ViewBag.contactos = GetContactos(comercializacion);
            return View(participante);
        }

        // GET: Participante/CreatePorCliente
        [CustomAuthorize(new string[] { "/ClienteContacto/" })]
        public ActionResult CreatePorCliente(int? id)
        {
            Comercializacion comercializacion = db.Comercializacion.Find(id);
            ViewBag.comercializacion = comercializacion;
            return View();
        }

        // POST: Participante/CreatePorCliente
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/ClienteContacto/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePorCliente([Bind(Include = "idParticipante,contacto")] Participante participante, int idComercializacion)
        {
            //ValidarContacto(participante.contacto);
            Comercializacion comercializacion = db.Comercializacion.Find(idComercializacion);
            ValidarCantParticipantes(comercializacion);
            participante.comercializacion = comercializacion;
            participante.contacto.fechaCreacion = DateTime.Now;
            participante.contacto.usuarioCreador = User.Identity.GetUserId();
            participante.contacto.softDelete = false;
            participante.contacto.tipoContacto = TipoContacto.Participante;

            // validar que exista configuracion particiapnte
            ExisteConfiguracionParticipante();
            // validar correo
            var contactoValidarCorreo = db.Contacto
                .Where(c => c.softDelete == false)
                .Where(c => c.correo == participante.contacto.correo)
                .Where(c => c.tipoContacto == TipoContacto.Participante)
                .FirstOrDefault();
            if (contactoValidarCorreo != null)
            {
                if (participante.contacto.run != contactoValidarCorreo.run)
                {
                    ModelState.AddModelError("", "El Correo Electrónico ingresado ya existe");
                }
            }

            if (ModelState.IsValid)
            {
                var contactoBD = db.Contacto
                    .Where(c => c.softDelete == false)
                    .Where(c => c.run == participante.contacto.run)
                    .Where(c => c.tipoContacto == TipoContacto.Participante)
                    .FirstOrDefault();
                if (contactoBD != null)
                {
                    contactoBD.nombres = participante.contacto.nombres;
                    contactoBD.apellidoPaterno = participante.contacto.apellidoPaterno;
                    contactoBD.apellidoMaterno = participante.contacto.apellidoMaterno;
                    //contactoBD.fechaNacimiento = participante.contacto.fechaNacimiento;
                    contactoBD.telefono = participante.contacto.telefono;
                    contactoBD.correo = participante.contacto.correo;

                    contactoBD.usuarioCreador = participante.contacto.usuarioCreador;
                    contactoBD.fechaCreacion = participante.contacto.fechaCreacion;

                    contactoBD.usuario.Email = participante.contacto.correo;

                    db.Entry(contactoBD).State = EntityState.Modified;

                    participante.contacto = contactoBD;
                }
                else
                {
                    var idUsuario = CrearUsuarioParticipante(participante.contacto);
                    participante.contacto.usuario = db.AspNetUsers.Find(idUsuario);
                    participante.contacto.usuario.tipo = TipoUsuario.parcial;
                }

                var participanteBD = db.Participante
                    .Where(p => p.contacto.idContacto == participante.contacto.idContacto)
                    .Where(p => p.comercializacion.idComercializacion == comercializacion.idComercializacion)
                    .FirstOrDefault();
                if (participanteBD == null)
                {
                    db.Participante.Add(participante);
                }

                db.SaveChanges();
                return RedirectToAction("LandingPage", "ClienteContacto");
            }
            ViewBag.comercializacion = comercializacion;
            return View(participante);
        }

        private bool ValidarCantParticipantes(Comercializacion comercializacion)
        {
            if (comercializacion.participantes.Count() + 1 > comercializacion.cotizacion.cantidadParticipante)
            {
                ModelState.AddModelError("", "No se puede superar la cantidad de participantes de la comercialización.");
                return false;
            }
            return true;
        }

        // GET: Participante/IngresarParticipantes/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult IngresarParticipantes(int? id)
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

            int cont = (int)comercializacion.cotizacion.cantidadParticipante + 1;

            List<List<string>> participantes = new List<List<string>>();

            foreach (var item in comercializacion.participantes)
            {
                List<string> participante = new List<string>();
                participante.Add(item.contacto.sinRut.ToString());
                participante.Add(item.contacto.run);
                participante.Add(item.contacto.nombres);
                participante.Add(item.contacto.apellidoPaterno);
                participante.Add(item.contacto.apellidoMaterno);
                //participante.Add(item.contacto.fechaNacimiento.ToString("dd/MM/yyyy"));
                participante.Add(item.contacto.telefono);
                participante.Add(item.contacto.correo);

                participantes.Add(participante);

                cont--;
            }

            for (int i = 0; i < cont; i++)
            {
                List<string> participante = new List<string>();
                participante.Add("False");
                participante.Add("");
                participante.Add("");
                participante.Add("");
                participante.Add("");
                participante.Add("");
                participante.Add("");

                participantes.Add(participante);
            }

            ViewBag.data = new JavaScriptSerializer().Serialize(participantes);

            return View(comercializacion);
        }

        [HttpGet]
        public ActionResult Registro(int? id)
        {
            Comercializacion comercializacion = db.Comercializacion.Find(id);

            return View(comercializacion);
        }

        [HttpPost]
        public ActionResult Registro(int? idComercializacion, [Bind(Include = "run,nombres,apellidoPaterno,apellidoMaterno,telefono,correo")] Contacto contacto)
        {
            int idParticipante = 0;
            Comercializacion comercializacion = db.Comercializacion.Find(idComercializacion);

            var participante = new Participante();

            if (comercializacion.cotizacion.codigoSence != null && comercializacion.cotizacion.codigoSence != "" && comercializacion.cotizacion.tieneCodigoSence != "on")
            {
                participante.conSence = true;
            }
            participante.contacto = new Contacto();
            participante.comercializacion = comercializacion;
            participante.timestart = comercializacion.fechaInicio;
            participante.timeend = comercializacion.fechaTermino;
            participante.contacto.run = contacto.run;
            participante.contacto.nombres = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(contacto.nombres.ToLower()).Split(' ').FirstOrDefault();
            participante.contacto.apellidoPaterno = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(contacto.apellidoPaterno.ToLower());
            participante.contacto.apellidoMaterno = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(contacto.apellidoMaterno.ToLower());
            participante.contacto.telefono = contacto.telefono;
            participante.contacto.correo = contacto.correo;
            participante.contacto.fechaCreacion = DateTime.Now;
            participante.contacto.usuarioCreador = User.Identity.GetUserId();
            participante.contacto.softDelete = false;
            participante.contacto.tipoContacto = TipoContacto.Participante;

            // validar contacto
            var context = new ValidationContext(participante.contacto, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(participante.contacto, context, results, true);
            string expresion = @"[ñáéíóú]";
            if (Regex.IsMatch(participante.contacto.correo, expresion))
            {
                ModelState.AddModelError("", "El correo no puede contener tildes o Ñ");
                isValid = false;
            }
            if (ModelState.IsValid)
            {
                // validar correo
                var contactoValidarCorreo = db.Contacto
                    .Where(c => c.softDelete == false)
                    .Where(c => c.run != participante.contacto.run)
                    .Where(c => c.correo == participante.contacto.correo)
                    .Where(c => c.tipoContacto == TipoContacto.Participante)
                    .FirstOrDefault();
                if (contactoValidarCorreo != null)
                {
                    isValid = false;
                    ValidationResult result = new ValidationResult("Ya estás inscrito en este curso");
                    results.Add(result);
                }

                var validarParticipante = db.Participante
                    .Where(x => x.contacto.run == participante.contacto.run)
                    .Where(x => x.comercializacion.idComercializacion == participante.comercializacion.idComercializacion)
                    .FirstOrDefault();
                if (validarParticipante != null)
                {
                    isValid = false;
                    ValidationResult result = new ValidationResult("Ya estás inscrito en este curso");
                    results.Add(result);
                }
            }

            //if (isValid && fechaValida)
            if (isValid)
            {
                var contactoBD = db.Contacto
                        .Where(c => c.softDelete == false)
                        .Where(c => c.run == participante.contacto.run)
                        .Where(c => c.tipoContacto == TipoContacto.Participante)
                        .FirstOrDefault();
                if (contactoBD != null)
                {
                    contactoBD.nombres = participante.contacto.nombres;
                    contactoBD.apellidoPaterno = participante.contacto.apellidoPaterno;
                    contactoBD.apellidoMaterno = participante.contacto.apellidoMaterno;
                    contactoBD.telefono = participante.contacto.telefono;
                    contactoBD.correo = participante.contacto.correo;
                    contactoBD.usuarioCreador = participante.contacto.usuarioCreador;
                    contactoBD.fechaCreacion = participante.contacto.fechaCreacion;

                    if (contactoBD.usuario == null)
                    {
                        contactoBD.usuario = db.AspNetUsers.Where(x => x.Email == participante.contacto.correo).FirstOrDefault();
                        if (contactoBD.usuario == null)
                        {
                            contactoBD.usuario.tipo = TipoUsuario.parcial;
                        }
                    }
                    contactoBD.usuario.Email = participante.contacto.correo;

                    db.Entry(contactoBD).State = EntityState.Modified;
                    db.SaveChanges();

                    participante.contacto = contactoBD;
                }
                else
                {
                    var idUsuario = CrearUsuarioParticipante(participante.contacto);

                    participante.contacto.usuario = db.AspNetUsers.Find(idUsuario);
                    if (participante.contacto.usuario == null)
                    {
                        participante.contacto.usuario = db.AspNetUsers.Where(x => x.run == participante.contacto.run || x.UserName == participante.contacto.run).FirstOrDefault();
                    }
                    participante.contacto.usuario.tipo = TipoUsuario.parcial;
                }

                var participanteBD = db.Participante
                    .Where(p => p.contacto.idContacto == participante.contacto.idContacto)
                    .Where(p => p.comercializacion.idComercializacion == comercializacion.idComercializacion)
                    .FirstOrDefault();
                if (participanteBD == null)
                {
                    comercializacion.participantes.Add(participante);

                    //Busca si el usuario ya existe y pregunta si tiene credencial, si es verdadero le asigna la credencial antigua.
                    Participante credencialParticipante = db.Participante
                        .Where(p => p.contacto.run == participante.contacto.run && p.contacto.tipoContacto == participante.contacto.tipoContacto)
                        .Where(p => p.credenciales != null).OrderByDescending(p => p.timestart).FirstOrDefault();

                    if (credencialParticipante != null)
                    {
                        participante.credenciales = credencialParticipante.credenciales;
                    }

                    db.Participante.Add(participante);
                    db.SaveChanges();
                    idParticipante = participante.idParticipante;

                    try
                    {
                        AgregarUsuarioMoodle(idParticipante);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    try
                    {
                        AsignarUsuarioGrupoMoodle(idParticipante);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    ViewBag.mensaje = "Ingreso Exitoso";
                }
            }

            // agregar los mensajes de error del titulo curricular al modelState
            foreach (var result in results)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }

            return View(comercializacion);
        }

        // POST: Participante/IngresarParticipantes
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IngresarParticipantes(int? idComercializacion, string data)
        {
            Comercializacion comercializacion = db.Comercializacion.Find(idComercializacion);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<List<String>> participantes = serializer.Deserialize<List<List<String>>>(data);
            var contParticipante = 0;
            var valido = true;

            // validar que exista configuracion particiapnte
            if (!ExisteConfiguracionParticipante())
            {
                ViewBag.data = data;
                return View(comercializacion);
            }



            //var countPart = participantes.Count(x => x.Count(y => y != "") > 0);
            //var countPart = participantes.Where(x => x.);
            var countPart = 0;
            participantes = participantes.Where(x => x.Count(y => y != "") > 0).ToList();
            foreach (var item in participantes)
            {
                contParticipante++;
                // excluir las filas de la tabla que estan vacias
                //var vacio = false;
                int cont = 0;
                foreach (var atributo in item.Skip(1))
                {
                    if (atributo == "")
                    {
                        cont++;
                    }
                }

                if (cont != item.Count()-1)
                {
                    countPart++;
                    var participante = new Participante();
                    if (comercializacion.cotizacion.codigoSence != null && comercializacion.cotizacion.codigoSence != "" && comercializacion.cotizacion.tieneCodigoSence != "on")
                    {
                        participante.conSence = true;
                    }
                    participante.contacto = new Contacto();
                    participante.comercializacion = comercializacion;
                    participante.timestart = comercializacion.fechaInicio;
                    participante.timeend = comercializacion.fechaTermino;
                    participante.contacto.sinRut = Boolean.Parse(item[0].Trim());
                    participante.contacto.run = item[1].Trim();
                    participante.contacto.nombres = item[2].Trim();
                    participante.contacto.apellidoPaterno = item[3].Trim();
                    participante.contacto.apellidoMaterno = item[4].Trim();

                    //// verificar fecha valida
                    //DateTime fechaNacimiento = new DateTime();
                    //var fechaValida = DateTime.TryParse(item[4], out fechaNacimiento);

                    //participante.contacto.fechaNacimiento = fechaNacimiento;
                    participante.contacto.telefono = item[5].Trim();
                    participante.contacto.correo = item[6].Trim().ToLower();

                    participante.contacto.fechaCreacion = DateTime.Now;
                    participante.contacto.usuarioCreador = User.Identity.GetUserId();
                    participante.contacto.softDelete = false;
                    participante.contacto.tipoContacto = TipoContacto.Participante;

                    // validar contacto
                    var context = new ValidationContext(participante.contacto, serviceProvider: null, items: null);
                    var results = new List<ValidationResult>();
                    var isValid = Validator.TryValidateObject(participante.contacto, context, results, true);
                    string expresion = @"[ñáéíóú]";

                    if (Regex.IsMatch(participante.contacto.correo, expresion))
                    {
                        ModelState.AddModelError("", "El correo del participante " + contParticipante + " no puede contener tildes o Ñ");
                        isValid = false;
                    }

                    if (ModelState.IsValid)
                    {
                        // validar correo
                        var contactoValidarCorreo = db.Contacto
                            .Where(c => c.softDelete == false)
                            .Where(c => c.run != participante.contacto.run)
                            .Where(c => c.correo == participante.contacto.correo)
                            .Where(c => c.tipoContacto == TipoContacto.Participante)
                            .FirstOrDefault();
                        if (contactoValidarCorreo != null)
                        {
                            isValid = false;
                            ValidationResult result = new ValidationResult("El Correo Electrónico " + participante.contacto.correo + " ya existe del participante con nombre: " + contactoValidarCorreo.nombreCompleto + " y RUT: " + contactoValidarCorreo.run + " en la fila " + contParticipante);
                            results.Add(result);
                        }


                    }

                    //if (isValid && fechaValida)
                    if (isValid)
                    {

                        var contactoBD = db.Contacto
                                .Where(c => c.softDelete == false)
                                .Where(c => c.run == participante.contacto.run)
                                .Where(c => c.tipoContacto == TipoContacto.Participante)
                                .FirstOrDefault();
                        if (contactoBD != null)
                        {
                            contactoBD.nombres = participante.contacto.nombres;
                            contactoBD.apellidoPaterno = participante.contacto.apellidoPaterno;
                            contactoBD.apellidoMaterno = participante.contacto.apellidoMaterno;
                            //contactoBD.fechaNacimiento = participante.contacto.fechaNacimiento;
                            contactoBD.telefono = participante.contacto.telefono;
                            contactoBD.correo = participante.contacto.correo;
                            contactoBD.usuarioCreador = participante.contacto.usuarioCreador;
                            contactoBD.fechaCreacion = participante.contacto.fechaCreacion;

                            if (contactoBD.usuario == null)
                            {
                                contactoBD.usuario = db.AspNetUsers.Where(x => x.Email == participante.contacto.correo).FirstOrDefault();
                                if (contactoBD.usuario == null)
                                {
                                    contactoBD.usuario.tipo = TipoUsuario.parcial;
                                }
                            }
                            contactoBD.usuario.Email = participante.contacto.correo;

                            db.Entry(contactoBD).State = EntityState.Modified;
                            db.SaveChanges();

                            participante.contacto = contactoBD;
                        }
                        else
                        {

                            var idUsuario = CrearUsuarioParticipante(participante.contacto);

                            participante.contacto.usuario = db.AspNetUsers.Find(idUsuario);
                            if (participante.contacto.usuario == null)
                            {
                                participante.contacto.usuario = db.AspNetUsers.Where(x => x.run == participante.contacto.run || x.UserName == participante.contacto.run).FirstOrDefault();
                            }
                            participante.contacto.usuario.tipo = TipoUsuario.parcial;
                        }

                        var participanteBD = db.Participante
                            .Where(p => p.contacto.idContacto == participante.contacto.idContacto)
                            .Where(p => p.comercializacion.idComercializacion == comercializacion.idComercializacion)
                            .FirstOrDefault();
                        if (participanteBD == null)
                        {
                            if (comercializacion.participantes.Count() > comercializacion.cotizacion.cantidadParticipante)
                            {
                                ModelState.AddModelError("", "La cantidad de participantes de esta comercializacion es " + comercializacion.cotizacion.cantidadParticipante + ", solo se ingresaron " + comercializacion.cotizacion.cantidadParticipante + " participantes.");
                                valido = false;
                                break;
                            }
                            comercializacion.participantes.Add(participante);


                            //Busca si el usuario ya existe y pregunta si tiene credencial, si es verdadero le asigna la credencial antigua.
                            Participante credencialParticipante = db.Participante
                                .Where(p => p.contacto.run == participante.contacto.run && p.contacto.tipoContacto == participante.contacto.tipoContacto)
                                .Where(p => p.credenciales != null).OrderByDescending(p => p.timestart).FirstOrDefault();

                            if (credencialParticipante != null)
                            {
                                participante.credenciales = credencialParticipante.credenciales;
                            }

                            db.Participante.Add(participante);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        valido = false;
                        ModelState.AddModelError("", "El participante " + contParticipante + " no es válido");
                    }
                    // agregar los mensajes de error del titulo curricular al modelState
                    foreach (var result in results)
                    {
                        ModelState.AddModelError("", result.ErrorMessage);
                    }
                    //// agregar mensaje error si fecha invalida
                    //if (!fechaValida)
                    //{
                    //    ModelState.AddModelError("", "El campo Fecha de Nacimiento no es válido");
                    //}
                }
            }

            if (countPart > comercializacion.cotizacion.cantidadParticipante)
            {
                ModelState.AddModelError("", "La cantidad de participantes de esta comercializacion es " + comercializacion.cotizacion.cantidadParticipante + "y se ingresaron " + countPart + " participantes.");
                valido = false;
            }

            var deleteParticipantes = comercializacion.participantes.Where(x => !participantes.Any(y => y[1].Trim() == x.contacto.run)).ToList();
            foreach (var delete in deleteParticipantes)
            {
                var partDelete = db.Participante.Include(x => x.contacto).FirstOrDefault(x => x.idParticipante == delete.idParticipante);
                bool asistio = partDelete.asistencia.Any(x => x.asistio);
                bool haveNota = partDelete.notas.Any(x => x.nota != "-");

                if (!asistio && !haveNota)
                {
                    try
                    {
                        db.Participante.Remove(partDelete);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        valido = false;
                        ModelState.AddModelError("", "No es posible eliminar al alumno " + partDelete.contacto.nombreCompleto + " con RUT: " + partDelete.contacto.run + ". Es posible que tenga asistencia o notas asociadas a esta comercialización");
                    }
                }
                else
                {
                    valido = false;
                    ModelState.AddModelError("", "No es posible eliminar al alumno " + partDelete.contacto.nombreCompleto + " con RUT: " + partDelete.contacto.run + ". Es posible que tenga asistencia o notas asociadas a esta comercialización");
                }

            }

            if (valido)
            {


                return RedirectToAction("List", new { id = idComercializacion });
            }

            ViewBag.data = data;
            return View(comercializacion);
        }

        // POST: Participante/Seleccionar
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Seleccionar(int idComercializacion, Contacto contacto)
        {
            contacto = db.Contacto.Find(contacto.idContacto);
            Comercializacion comercializacion = db.Comercializacion.Find(idComercializacion);
            ValidarCantParticipantes(comercializacion);
            var participante = new Participante();
            participante.comercializacion = comercializacion;
            participante.contacto = contacto;

            db.Participante.Add(participante);
            db.SaveChanges();

            return RedirectToAction("List", new { id = idComercializacion });
        }

        private bool ValidarUsuarioContactoMoodle(Contacto contactoValidar)
        {
            var validarUsuario = Moodle.ValidarSiUsuarioYaExiste(contactoValidar, db.ParametrosMoodles.FirstOrDefault());
            // validarUsuario == "" es q no existe
            // validarUsuario != "" es q si existe
            //if (validarUsuario != "" && contactoValidar.idUsuarioMoodle != null)
            if (validarUsuario != "")
            {
                //ModelState.AddModelError("", validarUsuario);
                return false;
            }
            return true;
        }

        private bool ValidarEmailContactoMoodle(Contacto contactoValidar)
        {
            var validarCorreo = Moodle.ValidarSiEmailYaExiste(contactoValidar, db.ParametrosMoodles.FirstOrDefault());
            if (validarCorreo != "")
            {
                ModelState.AddModelError("", validarCorreo);
                return false;
            }
            return true;
        }

        // GET: Participante/AgregarUsuarioMoodle/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult AgregarUsuarioMoodle(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Participante participante = db.Participante.Find(id);
            if (participante == null)
            {
                return HttpNotFound();
            }

            // ValidarUsuarioContactoMoodle es falso si existe
            // ValidarUsuarioContactoMoodle es verdadero si no existe
            if (ValidarUsuarioContactoMoodle(participante.contacto))
            {
                if (ValidarEmailContactoMoodle(participante.contacto))
                {
                    participante.contacto.idUsuarioMoodle = Moodle.CrearUsuarioMoodle(participante.contacto, db.ParametrosMoodles.FirstOrDefault());
                    var number = 0;
                    if (!Int32.TryParse(participante.contacto.idUsuarioMoodle, out number))
                    {
                        ModelState.AddModelError("", participante.contacto.idUsuarioMoodle);
                        participante.contacto.idUsuarioMoodle = null;
                    }
                    else
                    {
                        db.Entry(participante.contacto).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            else
            {
                if (participante.contacto.idUsuarioMoodle == null)
                {
                    participante.contacto.idUsuarioMoodle = Moodle.idUsuarioExistente(participante.contacto, db.ParametrosMoodles.FirstOrDefault());
                    if (participante.contacto.idUsuarioMoodle != "")
                    {
                        db.Entry(participante.contacto).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        participante.contacto.idUsuarioMoodle = null;
                        ModelState.AddModelError("", "Se produjo un error al ingresar el participante a la plataforma Moodle");
                    }
                }
            }
            ViewBag.linkComercializacion = db.LinkComercializacion.Where(c => c.comercializacion.idComercializacion == participante.comercializacion.idComercializacion).FirstOrDefault();
            string domain = ConfigurationManager.AppSettings["domain"];
            @ViewBag.link = domain + "/Comercializacions/RedirectVideoLLamadaAlumnosComercializacion?id=" + participante.comercializacion.idComercializacion;

            ViewBag.typesLink = new SelectList(db.LinkTypes.ToList().Select(c => new SelectListItem
            {
                Text = c.nombre,
                Value = c.idLinkType.ToString()
            }).ToList(), "Value", "Text");

            ViewBag.idComercializacion = participante.comercializacion.idComercializacion;
            ViewBag.codigoComercializacion = participante.comercializacion.cotizacion.codigoCotizacion;
            ViewBag.nombreCurso = participante.comercializacion.cotizacion.curso.nombreCurso;
            //Actualizar las fechas de matriculas de moodle
            UpdateUserEnrolments(new List<Participante> { participante }, participante.comercializacion.cotizacion.curso.idCursoMoodle);

            return View("List", db.Participante
                .Where(p => p.comercializacion.idComercializacion == participante.comercializacion.idComercializacion)
                .ToList());
        }

        // GET: Participante/AgregarUsuariosMoodle/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult AgregarUsuariosMoodle(int? id)
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
            foreach (var participante in comercializacion.participantes)
            {
                if (ValidarUsuarioContactoMoodle(participante.contacto))
                {
                    if (ValidarEmailContactoMoodle(participante.contacto))
                    {
                        participante.contacto.idUsuarioMoodle = Moodle.CrearUsuarioMoodle(participante.contacto, db.ParametrosMoodles.FirstOrDefault());
                        var number = 0;
                        if (!Int32.TryParse(participante.contacto.idUsuarioMoodle, out number))
                        {
                            ModelState.AddModelError("", participante.contacto.idUsuarioMoodle);
                            participante.contacto.idUsuarioMoodle = null;
                        }
                        else
                        {
                            db.Entry(participante.contacto).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    if (participante.contacto.idUsuarioMoodle == null)
                    {
                        participante.contacto.idUsuarioMoodle = Moodle.idUsuarioExistente(participante.contacto, db.ParametrosMoodles.FirstOrDefault());
                        if (participante.contacto.idUsuarioMoodle != "")
                        {
                            db.Entry(participante.contacto).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            participante.contacto.idUsuarioMoodle = null;
                            ModelState.AddModelError("", "Se produjo un error al ingresar el participante a la plataforma Moodle");
                        }
                    }
                }
            }
            ViewBag.idComercializacion = comercializacion.idComercializacion;
            ViewBag.codigoComercializacion = comercializacion.cotizacion.codigoCotizacion;
            ViewBag.nombreCurso = comercializacion.cotizacion.curso.nombreCurso;
            ViewBag.tipoEjecucion = comercializacion.cotizacion.curso.tipoEjecucion;
            if (TempData["ModelState"] != null && !ModelState.Equals(TempData["ModelState"]))
            {
                ModelState.Merge((ModelStateDictionary)TempData["ModelState"]);
                TempData["ModelState"] = null;
            }

            ViewBag.linkComercializacion = db.LinkComercializacion.Where(c => c.comercializacion.idComercializacion == comercializacion.idComercializacion).FirstOrDefault();
            string domain = ConfigurationManager.AppSettings["domain"];
            @ViewBag.link = domain + "/Comercializacions/RedirectVideoLLamadaAlumnosComercializacion?id=" + comercializacion.idComercializacion;

            ViewBag.typesLink = new SelectList(db.LinkTypes.ToList().Select(c => new SelectListItem
            {
                Text = c.nombre,
                Value = c.idLinkType.ToString()
            }).ToList(), "Value", "Text");
            //actualizar las fechas de matricula por las de moodle
            UpdateUserEnrolments(comercializacion.participantes.ToList(), comercializacion.cotizacion.curso.idCursoMoodle);
            return View("List", db.Participante
                .Where(p => p.comercializacion.idComercializacion == comercializacion.idComercializacion)
                .ToList());
        }

        public void UsuarioGrupoMoodle(int? id, List<Grupo> grupos)
        {
            InsecapContext dbUpdate = new InsecapContext();
            Participante participante = dbUpdate.Participante.Find(id);

            string idGrupo = "";
            var parametrosMoodle = dbUpdate.ParametrosMoodles.FirstOrDefault();
            var asignarParticipanteGrupo = Moodle.AgregarParticipanteGrupoMoodle(participante.contacto, participante.comercializacion, parametrosMoodle);
            string temp = participante.comercializacion.idGrupoMoodle;

            //}

            if (asignarParticipanteGrupo != "")
            {
                ModelState.AddModelError("", asignarParticipanteGrupo);
            }
            else
            {
                grupos = grupos.Where(x => x != null).ToList();
                if (grupos.Count() > 0)
                {
                    foreach (Grupo grupo in grupos)
                    {
                        try
                        {
                            idGrupo = grupo.id;
                            participante.comercializacion.idGrupoMoodle = idGrupo;
                            asignarParticipanteGrupo = Moodle.AgregarParticipanteGrupoMoodle(participante.contacto, participante.comercializacion, parametrosMoodle);
                        }
                        catch (Exception e)
                        {

                        }
                    }

                    participante.comercializacion.idGrupoMoodle = temp;
                    participante.agregadoAGrupo = true;
                    dbUpdate.Entry(participante.contacto).State = EntityState.Modified;
                    dbUpdate.SaveChanges();
                }

            }
            TempData["ModelState"] = ModelState;
        }

        public void UsuarioGrupoMoodle(int? id)
        {
            InsecapContext dbUpdate = new InsecapContext();
            Participante participante = dbUpdate.Participante.Find(id);

            List<Grupo> grupos = new List<Grupo>();
            string idGrupo = "";
            var parametrosMoodle = dbUpdate.ParametrosMoodles.FirstOrDefault();
            var asignarParticipanteGrupo = Moodle.AgregarParticipanteGrupoMoodle(participante.contacto, participante.comercializacion, parametrosMoodle);
            string temp = participante.comercializacion.idGrupoMoodle;
            if ((participante.comercializacion.cotizacion.tieneCodigoSence != "on" && participante.comercializacion.cotizacion.codigoSence != null && participante.comercializacion.senceNet != null)
                && (participante.comercializacion.cotizacion.curso.tipoEjecucion != TipoEjecucion.Presencial && participante.comercializacion.cotizacion.tipoCurso != "Recertificación"))
            {
                grupos.Add(Moodle.FindGroupByNameAndCourse(Convert.ToString(participante.comercializacion.cotizacion.curso.idCursoMoodle), "SENCE-" + participante.comercializacion.senceNet, parametrosMoodle));

            }
            //else
            //{

            grupos.Add(Moodle.FindGroupByNameAndCourse(Convert.ToString(participante.comercializacion.cotizacion.curso.idCursoMoodle), "nosence", parametrosMoodle));

            //}

            if (asignarParticipanteGrupo != "")
            {
                ModelState.AddModelError("", asignarParticipanteGrupo);
            }
            else
            {
                grupos = grupos.Where(x => x != null).ToList();
                if (grupos.Count() > 0)
                {
                    foreach (Grupo grupo in grupos)
                    {
                        try
                        {
                            idGrupo = grupo.id;
                            participante.comercializacion.idGrupoMoodle = idGrupo;
                            asignarParticipanteGrupo = Moodle.AgregarParticipanteGrupoMoodle(participante.contacto, participante.comercializacion, parametrosMoodle);
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }
                participante.comercializacion.idGrupoMoodle = temp;
                participante.agregadoAGrupo = true;
                dbUpdate.Entry(participante.contacto).State = EntityState.Modified;
                dbUpdate.SaveChanges();
            }
            TempData["ModelState"] = ModelState;
        }

        // GET: Participante/AsignarUsuarioGrupoMoodle/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult AsignarUsuarioGrupoMoodle(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Participante participante = db.Participante.Find(id);
            if (participante == null)
            {
                return HttpNotFound();
            }
            else if (participante.comercializacion.idGrupoMoodle == null)
            {
                ModelState.AddModelError("", "No se ha creado el grupo a moodle");
            }
            else
            {
                UsuarioGrupoMoodle(id);
            }

            ViewBag.idComercializacion = participante.comercializacion.idComercializacion;
            ViewBag.codigoComercializacion = participante.comercializacion.cotizacion.codigoCotizacion;
            ViewBag.nombreCurso = participante.comercializacion.cotizacion.curso.nombreCurso;
            ViewBag.tipoEjecucion = participante.comercializacion.cotizacion.curso.tipoEjecucion;
            if (TempData["ModelState"] != null && !ModelState.Equals(TempData["ModelState"]))
            {
                ModelState.Merge((ModelStateDictionary)TempData["ModelState"]);
                TempData["ModelState"] = null;
            }

            ViewBag.linkComercializacion = db.LinkComercializacion.Where(c => c.comercializacion.idComercializacion == participante.comercializacion.idComercializacion).FirstOrDefault();
            string domain = ConfigurationManager.AppSettings["domain"];
            @ViewBag.link = domain + "/Comercializacions/RedirectVideoLLamadaAlumnosComercializacion?id=" + participante.comercializacion.idComercializacion;

            ViewBag.typesLink = new SelectList(db.LinkTypes.ToList().Select(c => new SelectListItem
            {
                Text = c.nombre,
                Value = c.idLinkType.ToString()
            }).ToList(), "Value", "Text");
            return View("List", db.Participante
                .Where(p => p.comercializacion.idComercializacion == participante.comercializacion.idComercializacion)
                .ToList());
        }

        // GET: Participante/AgregarUsuariosGrupoMoodle/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public async Task<ActionResult> AgregarUsuariosGrupoMoodle(int? id)
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
            else if (comercializacion.idGrupoMoodle == null)
            {
                ModelState.AddModelError("", "No se ha creado el grupo a moodle");
            }
            else
            {
                var parametrosMoodle = db.ParametrosMoodles.FirstOrDefault();
                var result = Moodle.UpdateAllUsuarioMoodle(comercializacion.participantes.Where(x => x.contacto.idUsuarioMoodle != null).ToList(), parametrosMoodle);

                result = Moodle.AgregarParticipantesGrupoMoodle(comercializacion.participantes.Where(x => x.contacto.idUsuarioMoodle != null).Select(x => x.contacto).ToList(), comercializacion, parametrosMoodle);
                if (result == null)
                {
                    foreach (var participante in comercializacion.participantes)
                    {
                        participante.agregadoAGrupo = true;
                        db.Entry(participante).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    List<Grupo> grupos = new List<Grupo>();
                    if ((comercializacion.cotizacion.tieneCodigoSence != "on" && comercializacion.cotizacion.codigoSence != null && comercializacion.senceNet != null)
                        && (comercializacion.cotizacion.curso.tipoEjecucion != TipoEjecucion.Presencial && comercializacion.cotizacion.tipoCurso != "Recertificación"))
                    {
                        grupos.Add(Moodle.FindGroupByNameAndCourse(Convert.ToString(comercializacion.cotizacion.curso.idCursoMoodle), "SENCE-" + comercializacion.senceNet, parametrosMoodle));
                    }
                    else
                    {
                        grupos.Add(Moodle.FindGroupByNameAndCourse(Convert.ToString(comercializacion.cotizacion.curso.idCursoMoodle), "nosence", parametrosMoodle));
                    }
                    grupos = grupos.Where(x => x != null).ToList();
                    foreach (var grupo in grupos)
                    {
                        if (grupo.name.Contains("SENCE"))
                        {
                            string alumnos = string.Join(",", comercializacion.participantes.Select(x => "'" + x.contacto.idUsuarioMoodle + "'").ToArray());

                            string sql = "SELECT mdl8s_groups.id,mdl8s_groups.courseid,mdl8s_groups.name FROM mdl8s_groups INNER JOIN mdl8s_groups_members ON mdl8s_groups.id = mdl8s_groups_members.groupid WHERE courseid = '{0}' AND ( mdl8s_groups.name  LIKE '%SENCE%' OR mdl8s_groups.name  LIKE '%nosence%'  ) AND mdl8s_groups.id <> '{1}' AND mdl8s_groups_members.userid IN ({2})";
                            sql = string.Format(sql, comercializacion.cotizacion.curso.idCursoMoodle, grupo.id, alumnos);
                            var groups = MySqlUtils.ExecuteSelectQuery(sql, typeof(mdl8s_groups));
                            foreach (mdl8s_groups items in groups)
                            {
                                sql = "DELETE FROM mdl8s_groups_members WHERE mdl8s_groups_members.groupid = '{0}' AND mdl8s_groups_members.userid IN ({1})";
                                sql = string.Format(sql, items.id, alumnos);
                                MySqlUtils.ExecuteSelectQuery(sql, typeof(mdl8s_groups));
                            }
                        }
                        comercializacion.idGrupoMoodle = grupo.id;
                        result = Moodle.AgregarParticipantesGrupoMoodle(comercializacion.participantes.Where(x => x.contacto.idUsuarioMoodle != null).Select(x => x.contacto).ToList(), comercializacion, parametrosMoodle);
                        if (result != null)
                            ModelState.AddModelError("", "Error al agregar al grupo " + grupo.name + ": " + result);

                    }
                }
                else
                {
                    ModelState.AddModelError("", "Error al agregar al grupo " + comercializacion.cotizacion.codigoCotizacion + ": " + result);
                }
            }
            ViewBag.idComercializacion = comercializacion.idComercializacion;
            ViewBag.codigoComercializacion = comercializacion.cotizacion.codigoCotizacion;
            ViewBag.nombreCurso = comercializacion.cotizacion.curso.nombreCurso;
            ViewBag.tipoEjecucion = comercializacion.cotizacion.curso.tipoEjecucion;
            if (TempData["ModelState"] != null && !ModelState.Equals(TempData["ModelState"]))
            {
                ModelState.Merge((ModelStateDictionary)TempData["ModelState"]);
                TempData["ModelState"] = null;
            }

            ViewBag.linkComercializacion = db.LinkComercializacion.Where(c => c.comercializacion.idComercializacion == comercializacion.idComercializacion).FirstOrDefault();
            string domain = ConfigurationManager.AppSettings["domain"];
            @ViewBag.link = domain + "/Comercializacions/RedirectVideoLLamadaAlumnosComercializacion?id=" + comercializacion.idComercializacion;

            ViewBag.typesLink = new SelectList(db.LinkTypes.ToList().Select(c => new SelectListItem
            {
                Text = c.nombre,
                Value = c.idLinkType.ToString()
            }).ToList(), "Value", "Text");
            return View("List", db.Participante
                .Where(p => p.comercializacion.idComercializacion == comercializacion.idComercializacion)
                .ToList());
        }

        // GET: Participante/Credenciales
        [CustomAuthorize(new string[] { "/Comercializacions/", "/Relator/Perfil/" })]
        public async Task<ActionResult> Credenciales(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Participante participante = db.Participante.Find(id);
            if (participante == null)
            {
                return HttpNotFound();
            }
            Files.borrarArchivosLocales();
            await Files.BajarArchivoADirectorioLocalAsync(participante.credenciales);
            ViewBag.idComercializacion = participante.comercializacion.idComercializacion;
            ViewBag.returnUrl = Request.UrlReferrer;
            return View(participante);
        }

        // POST: Participante/Credenciales/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Comercializacions/", "/Relator/Perfil/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Credenciales([Bind(Include = "idParticipante")] Participante participante, Uri returnUrl)
        {
            participante = db.Participante.Find(participante.idParticipante);
            List<Participante> participantes = db.Participante
                    .Where(p => p.contacto.run == participante.contacto.run && p.contacto.tipoContacto == participante.contacto.tipoContacto).ToList();

            var credencialesAntiguas = participante.credenciales;
            Storage credencialNueva = null;
            HttpPostedFileBase file = Request.Files[0];
            // verificar que se selecciono un archivo
            if (file.ContentLength <= 0)
            {
                ModelState.AddModelError("", "Se debe seleccionar un archivo.");
            }
            else
            {
                // validar extenciones y tamaño maximo del archivo
                var archivoValido = Files.ArchivoValido(file, new[] { ".jpeg", ".jpg", ".png" }, 3 * 1024);
                if (archivoValido != "")
                {
                    ModelState.AddModelError("", archivoValido);
                }
                else
                {
                    participante.credenciales = await Files.RemplazarArchivoPublicoAsync(participante.credenciales, file, "participantes/credenciales/");
                    credencialNueva = participante.credenciales;
                    if (participante.credenciales == null)
                    {
                        ModelState.AddModelError("", "No se pudo guardar el archivo seleccionado.");
                    }
                }
            }
            if (ModelState.IsValid)
            {
                foreach (Participante par in participantes)
                {
                    if (par.credenciales != null)
                    {
                        db.Storages.Remove(par.credenciales);
                    }
                }

                foreach (Participante par in participantes)
                {
                    par.credenciales = credencialNueva;
                    db.Entry(par).State = EntityState.Modified;
                }

                db.SaveChanges();
                if (returnUrl != null)
                {
                    return Redirect(returnUrl.ToString());
                }
                return RedirectToAction("List", new { id = participante.comercializacion.idComercializacion });
            }
            participante.credenciales = credencialesAntiguas;
            Files.borrarArchivosLocales();
            await Files.BajarArchivoADirectorioLocalAsync(participante.credenciales);
            ViewBag.idComercializacion = participante.comercializacion.idComercializacion;
            ViewBag.returnUrl = returnUrl;
            return View(participante);
        }

        //// POST: Participante/DescargarCredenciales/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult DescargarCredenciales([Bind(Include = "idParticipante")] Participante participante)
        //{
        //    participante = db.Participante.Find(participante.idParticipante);
        //    byte[] file = Files.BajarArchivo(participante.credenciales);
        //    return File(file, System.Net.Mime.MediaTypeNames.Application.Octet, participante.credenciales.nombreArchivo);
        //}

        // GET: Participante/Edit/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Participante participante = db.Participante.Find(id);
            if (participante == null)
            {
                return HttpNotFound();
            }
            return View(participante);
        }

        // POST: Participante/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idParticipante")] Participante participante)
        {
            if (ModelState.IsValid)
            {
                db.Entry(participante).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(participante);
        }

        // GET: Participante/Delete/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Participante participante = db.Participante.Find(id);
            if (participante == null)
            {
                return HttpNotFound();
            }
            return View(participante);
        }

        // POST: Participante/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, int idComercializacion)
        {
            Participante participante = db.Participante.Find(id);

            if (participante.agregadoAGrupo)
            {
                Moodle.RemoverParticipanteGrupoMoodle(participante.contacto, participante.comercializacion, db.ParametrosMoodles.FirstOrDefault());
                Moodle.EliminarParticipanteCursoMoodle(participante.contacto, participante.comercializacion.cotizacion.curso, db.ParametrosMoodles.FirstOrDefault());

            }

            if (participante.credenciales != null)
            {
                Participante credencialParticipante = db.Participante
                                .Where(p => p.contacto.run == participante.contacto.run && p.contacto.tipoContacto == participante.contacto.tipoContacto)
                                .Where(p => p.credenciales != null)
                                .Where(p => p.idParticipante != id).FirstOrDefault();
                if (credencialParticipante == null)
                {
                    Files.BorrarArchivo(participante.credenciales);
                    db.Storages.Remove(participante.credenciales);
                }
            }
            db.Asistencias.RemoveRange(db.Asistencias.Where(x => x.participante.idParticipante == id).ToList());
            db.Attempts.RemoveRange(db.Attempts.Where(x => x.participante.idParticipante == id).ToList());
            db.AttemptsQuizUser.RemoveRange(db.AttemptsQuizUser.Where(x => x.participante.idParticipante == id).ToList());
            db.RespuestaEvaluacion.RemoveRange(db.RespuestaEvaluacion.Where(x => x.nota.idParticipante == id).ToList());
            db.Notas.RemoveRange(db.Notas.Where(x => x.participante.idParticipante == id).ToList());
            db.Participante.Remove(participante);


            //db.Contacto.Remove(participante.contacto);
            db.SaveChanges();
            return RedirectToAction("List", new { id = idComercializacion });
        }

        // POST: Participante/DeletePorCliente/5
        [CustomAuthorize(new string[] { "/ClienteContacto/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePorCliente(int id)
        {
            Participante participante = db.Participante.Find(id);

            if (participante.asistencia.Count() > 0
                //|| participante.credenciales != null
                || participante.notas.Count() > 0)
            {
                ViewBag.returnAction = "LandingPage";
                ViewBag.returnController = "ClienteContacto";
                return View("Error", (object)"No se puede eliminar al participante si ya tiene notas o asistencia.");
            }

            if (participante.agregadoAGrupo)
            {
                Moodle.RemoverParticipanteGrupoMoodle(participante.contacto, participante.comercializacion, db.ParametrosMoodles.FirstOrDefault());
            }

            if (participante.credenciales != null)
            {
                Files.BorrarArchivo(participante.credenciales);
                db.Storages.Remove(participante.credenciales);
            }
            db.Participante.Remove(participante);
            db.SaveChanges();
            return RedirectToAction("LandingPage", "ClienteContacto");
        }

        // GET: Participante/Descargar/5
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

        // GET: Participante/IngresarAsistencia
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult IngresarAsistencia(int? id)
        {
            ViewBag.idComercializacion = id;
            //ViewBag.curso = db.Comercializacion.Find(id).cotizacion.curso.nombreCurso;
            return View();
        }

        // POST: Participante/IngresarAsistencia
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IngresarAsistencia(int idComercializacion)
        {
            var idUsuario = User.Identity.GetUserId();
            var participante = db.Participante
                .Where(x => x.contacto.usuario.Id == idUsuario)
                .Where(x => x.comercializacion.idComercializacion == idComercializacion)
                .FirstOrDefault();
            if (participante == null)
            {
                ModelState.AddModelError("", "No se encuentra ingresado como participante.");
                ViewBag.idComercializacion = idComercializacion;
                return View();
            }
            var hoy = DateTime.Now;
            var agregoAsistencia = false;
            foreach (var bloque in participante.comercializacion.bloques)
            {
                if (TimeSpan.Compare(bloque.horarioInicio.TimeOfDay, hoy.TimeOfDay) <= 0
                    && TimeSpan.Compare(bloque.horarioTermino.TimeOfDay, hoy.TimeOfDay) >= 0
                    && DateTime.Compare(bloque.fecha.Date, hoy.Date) == 0)
                {
                    var asistenciaBD = db.Asistencias
                        .Where(x => x.bloque.idBloque == bloque.idBloque)
                        .Where(x => x.participante.idParticipante == participante.idParticipante)
                        .FirstOrDefault();
                    if (asistenciaBD != null)
                    {
                        asistenciaBD.fecha = hoy;
                        asistenciaBD.asistio = true;
                        db.Entry(asistenciaBD).State = EntityState.Modified;
                    }
                    else
                    {
                        var asistencia = new Asistencia();
                        asistencia.bloque = bloque;
                        asistencia.fecha = hoy;
                        asistencia.asistio = true;
                        asistencia.participante = participante;
                        db.Asistencias.Add(asistencia);
                    }
                    agregoAsistencia = true;
                }
            }
            if (agregoAsistencia)
            {
                db.SaveChanges();
                return RedirectToAction("Index", "Home", new { });
            }
            ModelState.AddModelError("", "No se encontro un curso con la fecha correspondiente.");
            ViewBag.idComercializacion = idComercializacion;
            return View();
        }
        // ------------------------------- Lista de asistencia -----------------------
        [EnableJsReport()]
        [HttpGet]
        public ActionResult AsistenciaExcel(int id)
        {
            var comercializacion = db.Comercializacion.Where(x => x.softDelete == false && x.idComercializacion == id).FirstOrDefault();
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            string name = comercializacion.cotizacion.codigoCotizacion.Split('-').ElementAt(0);
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Comercializacion_" + name + ".xlsx\"");
            return View(comercializacion);
        }

        [EnableJsReport()]
        [HttpGet]
        public ActionResult ContactosExcel(int id)
        {
            var comercializacion = db.Comercializacion.Where(x => x.softDelete == false && x.idComercializacion == id).FirstOrDefault();
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            string name = comercializacion.cotizacion.codigoCotizacion.Split('-').ElementAt(0);
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Comercializacion_" + name + ".xlsx\"");
            return View(comercializacion);
        }

        [EnableJsReport()]
        [HttpGet]
        public ActionResult NotasExcel(int id)
        {

            var comercializacion = db.Comercializacion.Where(x => x.softDelete == false && x.idComercializacion == id).FirstOrDefault();
            GetNotas(comercializacion.idComercializacion);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            string name = comercializacion.cotizacion.codigoCotizacion.Split('-').ElementAt(0);
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Comercializacion_" + name + ".xlsx\"");
            return View(comercializacion);
        }

        // GET: Participante/Asistencia/5
        [CustomAuthorize(new string[] { "/Comercializacions/", "/Relator/Perfil/" })]
        public ActionResult Asistencia(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var comercializacion = db.Comercializacion.Find(id);
            ViewBag.returnUrl = Request.UrlReferrer;
            return View(comercializacion);
        }

        // GET: Participante/Asistencia/5
        [CustomAuthorize(new string[] { "/Comercializacions/", "/Relator/Perfil/" })]
        public ActionResult Resumen(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var comercializacion = db.Comercializacion.Find(id);
            ViewBag.returnUrl = Request.UrlReferrer;
            return View(comercializacion);
        }

        [CustomAuthorize(new string[] { "/Comercializacions/", "/Relator/Perfil/" })]
        [HttpPost]
        public ActionResult GuardarAsistencia(int idComercializacion, List<int> idBloque, List<int> idParticipante, List<string> descripcion, List<bool> select, string returnUrl)
        {
            int i = 0;

            foreach (int bloque in idBloque)
            {
                int participante = idParticipante[i];
                var asistencia = db.Asistencias
               .Where(x => x.bloque.idBloque == bloque)
               .Where(x => x.participante.idParticipante == participante)
               .FirstOrDefault();

                if (asistencia != null)
                {
                    asistencia.asistio = select[i];
                    asistencia.fecha = DateTime.Now;
                    asistencia.descripcion = descripcion[i];
                    db.Entry(asistencia).State = EntityState.Modified;
                }
                else
                {
                    asistencia = new Asistencia();
                    asistencia.bloque = db.Bloque.Find(bloque);
                    asistencia.fecha = DateTime.Now;
                    asistencia.asistio = select[i];
                    asistencia.descripcion = descripcion[i];
                    asistencia.participante = db.Participante.Find(participante);
                    db.Asistencias.Add(asistencia);
                }

                i++;
            }

            db.SaveChanges();
            var comercializacion = db.Comercializacion.Find(idComercializacion);
            ViewBag.returnUrl = Request.UrlReferrer;
            return View(returnUrl, comercializacion);
        }

        public bool updateAsistencia(int? id)
        {
            try
            {
                if (id == null)
                {
                    return false;
                }
                var comercializacion = db.Comercializacion.Find(id);
                if (comercializacion == null)
                {
                    return false;
                }
                if (comercializacion.participantes.Count() <= 0)
                {
                    return false;
                }

                foreach (var participante in comercializacion.participantes)
                {
                    if (participante.notas.Any(x => x.nota == "-"))
                    {
                        return false;
                    }

                    foreach (var bloque in comercializacion.bloques)
                    {
                        var asistencia = db.Asistencias
                            .Where(x => x.bloque.idBloque == bloque.idBloque)
                            .Where(x => x.participante.idParticipante == participante.idParticipante)
                            .FirstOrDefault();

                        if (asistencia != null)
                        {
                            asistencia.asistio = true;
                            asistencia.fecha = DateTime.Now;
                            db.Entry(asistencia).State = EntityState.Modified;
                        }
                        else
                        {
                            asistencia = new Asistencia();
                            asistencia.bloque = bloque;
                            asistencia.fecha = DateTime.Now;
                            asistencia.asistio = true;
                            asistencia.descripcion = "";
                            asistencia.participante = participante;
                            db.Asistencias.Add(asistencia);
                        }
                    }
                }
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        // POST: Participante/CambiarAsistencia
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Comercializacions/", "/Relator/Perfil/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public bool CambiarAsistencia(int idBloque, int idParticipante, string descripcion, bool select)
        {
            bool guardado = true;
            var asistencia = db.Asistencias
               .Where(x => x.bloque.idBloque == idBloque)
               .Where(x => x.participante.idParticipante == idParticipante)
               .FirstOrDefault();
            try
            {

                if (asistencia != null)
                {

                    asistencia.asistio = select;
                    asistencia.fecha = DateTime.Now;
                    asistencia.descripcion = descripcion;
                    db.Entry(asistencia).State = EntityState.Modified;
                }
                else
                {
                    asistencia = new Asistencia();
                    asistencia.bloque = db.Bloque.Find(idBloque);
                    asistencia.fecha = DateTime.Now;
                    asistencia.asistio = select;
                    asistencia.descripcion = descripcion;
                    asistencia.participante = db.Participante.Find(idParticipante);
                    db.Asistencias.Add(asistencia);
                }
                db.SaveChanges();
            }
            catch (Exception e)
            {
                guardado = false;
            }

            return guardado;
        }
        [CustomAuthorize(new string[] { "/Comercializacions/", "/Relator/Perfil/" })]
        [HttpPost]
        public bool CambiarSenceDJP(int id, bool check, string type)
        {

            if (id == null)
            {
                return false;
            }
            bool guardado = true;
            var participante = db.Participante.Find(id);
            try
            {
                if (participante != null)
                {
                    if (type.Contains("sence"))
                    {
                        participante.conSence = check;
                    }
                    else if (type.Contains("djp"))
                    {
                        participante.conDeclaracionJuradaPersona = check;
                    }

                    db.Entry(participante).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    guardado = false;
                }

            }
            catch (Exception e)
            {
                guardado = false;
            }

            return guardado;
        }

        // GET: Participante/ActualizarEvaluaciones/
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult ActualizarEvaluaciones(int? id, string returnalUrl)
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

            return RedirectToAction(returnalUrl, new { id = comercializacion.idComercializacion });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //Mis funciones
        [HttpGet]
        public ActionResult UpdateGrade()
        {
            DateTime hoy = DateTime.Now.Date;
            var comercializaciones = db.Comercializacion
                .Where(x => x.softDelete == false)
                .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(z => z.fechaCreacion).Take(1).Any(y => y.EstadoComercializacion == EstadoComercializacion.En_Proceso))
                .Where(x => DateTime.Compare(x.fechaInicio, hoy) <= 0 && DateTime.Compare(x.fechaTermino, hoy) >= 0)
                .ToList();

            foreach (var comercializacion in comercializaciones)
            {
                try
                {
                    GetNotas(comercializacion.idComercializacion);
                    switch (comercializacion.cotizacion.curso.tipoEjecucion)
                    {
                        case TipoEjecucion.Elearning_Sincrono:
                            GetFeedback(comercializacion.idComercializacion);
                            break;
                        case TipoEjecucion.Recertificacion_Sincronica:
                            break;
                        case TipoEjecucion.Elearning_Asincrono:
                            updateAsistencia(comercializacion.idComercializacion);
                            break;
                        case TipoEjecucion.Recertificacion_Asincronica:
                            updateAsistencia(comercializacion.idComercializacion);
                            break;
                        case TipoEjecucion.Presencial:
                            GetFeedback(comercializacion.idComercializacion);
                            break;
                        case TipoEjecucion.Recertificacion:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(comercializacion.cotizacion.codigoCotizacion + ": " + e);
                }
            }

            //comercializaciones.ForEach(x => GetNotas(x.idComercializacion));

            db.SaveChanges();
            return Json(new
            {
                value = "Terminado"
            },
                   JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult ActualizarCredenciales()
        {
            bool actualizado = false;

            List<Participante> participantes = db.Participante.Where(p => p.credenciales != null).ToList();

            foreach (Participante participante in participantes)
            {
                List<Participante> participantesSinCredencial = db.Participante
                    .Where(p => p.contacto.run == participante.contacto.run && p.contacto.tipoContacto == participante.contacto.tipoContacto)
                    .Where(p => p.credenciales == null).ToList();

                foreach (Participante participanteSinCredencial in participantesSinCredencial)
                {
                    participanteSinCredencial.credenciales = participante.credenciales;
                    db.Entry(participanteSinCredencial).State = EntityState.Modified;
                    actualizado = true;
                }
            }

            if (actualizado) db.SaveChanges();

            return Json(new
            {
                Actualizado = actualizado
            },
                   JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CorreoResultadoQuizAprendizaje(int id, int idParticipante)
        {
            var participante = db.Participante.Find(idParticipante);
            var resultados = db.QuizAprendizajeParticipantesResultados.Where(x => x.participante.idParticipante == idParticipante && x.comercializacion.idComercializacion == id).ToList();

            string nombreParticipante = resultados.Select(x => x.participante.contacto.nombreCompleto).FirstOrDefault();
            string tipoAprendizaje = resultados.Select(x => x.quizAprendizajeResultados.tipoAprendizaje).FirstOrDefault();
            string descripcion = resultados.Select(x => x.quizAprendizajeResultados.descripcion).FirstOrDefault();
            //Correo config

            var bodyHTML = "";
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/quiz_aprendizaje_alumno.html")))
            {
                bodyHTML = reader.ReadToEnd();
            }
            try
            {
                var receiverEmail = new MailAddress(participante.contacto.correo, nombreParticipante);
                var subject = "Resultado Quiz de Aprendizaje";
                var body = bodyHTML;
                body = body.Replace("{0}", nombreParticipante);
                body = body.Replace("{1}", tipoAprendizaje);
                body = body.Replace("{2}", descripcion);

                Utils.Utils.SendMail(receiverEmail, subject, body);
            }
            catch (Exception e)
            {
            }
            return Json(new
            {
                value = "Terminado"
            },
                   JsonRequestBehavior.AllowGet);
        }

        //private void MailLog(string fileRoute, string error)
        //{
        //    using (System.IO.StreamWriter file =
        //  new System.IO.StreamWriter(Server.MapPath(@"~" + fileRoute)))
        //    {
        //        file.WriteLine(String.Format("{0} - {1}", DateTime.Now, error));
        //    }
        //}


        ////Correo Post Curso
        //[HttpGet]
        //public ActionResult alertaParticipantesVencidos()
        //{
        //    var send = true;
        //    try
        //    {
        //        var bodyHTML = "";
        //        using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/PostCurso/mail.html")))
        //        {
        //            bodyHTML = reader.ReadToEnd();
        //        }

        //        //Busca los participantes que se les vencerá el curso en un periodo de 1 mes.
        //        var participantes = db.Participante.Where(x => DateTime.Today <= DbFunctions.AddMonths(x.comercializacion.fechaTermino, x.comercializacion.vigenciaCredenciales)
        //        && DbFunctions.AddDays(DateTime.Today, 90) >= DbFunctions.AddMonths(x.comercializacion.fechaTermino, x.comercializacion.vigenciaCredenciales)).ToList();

        //        foreach (Comercializacion comercializacion in comercializacions.ToList())
        //        {
        //            var subject = "ALERTA POST CURSO [" + comercializacion.cotizacion.codigoCotizacion + "] " + "[" + comercializacion.cotizacion.curso.tipoEjecucion + "]";
        //            List<String> correos = new List<string>();
        //            ViewBag.domain = domain;
        //            var tabla = ConvertPartialViewToString(PartialView("alertaPostCurso", comercializacion));

        //            // obtener de aca los correos de todos los contactos o solo deun contacto 
        //            correos.AddRange(netUsers.Select(y => y.Email).ToList());
        //            correos.Add("contacto@insecap.email");

        //            String body = bodyHTML.Replace("{3}", "TICA ");
        //            body = body.Replace("{1}", tabla);

        //            foreach (String mail in correos)
        //            {
        //                var receiverEmail = new MailAddress(mail, "INSECAP Capacitación");
        //                Utils.Utils.SendMail(receiverEmail, subject, body);
        //            }
        //        }

        //        MailLog("/Email/Log/cliente.txt", "Enviado Correctamente");
        //    }
        //    catch (Exception e)
        //    {
        //        MailLog("/Email/Log/cliente.txt", e.Message);
        //        send = false;
        //    }

        //    //Agrupar clientes iguales por contacto y id cliente en cotizacion 
        //    //Agregar tipo de ejecucion al correo
        //    //Validar todos los que no sean presenciales o recertificacion que tengan alumnos 

        //    return Json(new
        //    {
        //        mailSend = send
        //    },
        //           JsonRequestBehavior.AllowGet);
        //}
    }
}