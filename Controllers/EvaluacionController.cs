using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SGC.CustomAuthorize;
using SGC.Models;
using SGC.Models.SQL;
using SGC.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SGC.Controllers
{
    [Authorize]
    public class EvaluacionController : Controller
    {
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
       

        //// GET: Evaluacion
        //public ActionResult Index()
        //{
        //    return View(db.Evaluacion.ToList());
        //}

        // GET: Evaluacion/EvaluacionesCurso/5
        [CustomAuthorize(new string[] { "/Curso/" })]
        public ActionResult EvaluacionesCurso(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Curso curso = db.Curso.Find(id);
            if (curso == null)
            {
                return HttpNotFound();
            }
            return View(curso);
        }

        // GET: Evaluacion/ObtenerEvaluacionesMoodle/5
        [CustomAuthorize(new string[] { "/Curso/" })]
        public ActionResult ObtenerEvaluacionesMoodle(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var curso = db.Curso.Find(id);
            if (curso == null)
            {
                return HttpNotFound();
            }

            var evaluacionesMoodle = Moodle.GetEvaluacionesCursoMoodle(curso, db.ParametrosMoodles.FirstOrDefault());
            if (evaluacionesMoodle == null)
            {
                ModelState.AddModelError("", "Se produjo un error al intentar obtener las evaluaciones de la plataforma Moodle");
                return View("EvaluacionesCurso", curso);
            }

            foreach (var evaluacionMoodle in evaluacionesMoodle)
            {
                if (evaluacionMoodle.name.ToLower().Contains("diagnóstico")
                    || evaluacionMoodle.name.ToLower().Contains("diagnostico")
                    || evaluacionMoodle.name.ToLower().Contains("final"))
                {
                    var evaluacion = new Evaluacion();
                    evaluacion.curso = curso;
                    evaluacion.nombre = evaluacionMoodle.name;
                    evaluacion.idQuizMoodle = evaluacionMoodle.id;
                    evaluacion.tipo = TipoEvaluacion.Moodle;
                    if (evaluacionMoodle.name.ToLower().Contains("diagnóstico")
                        || evaluacionMoodle.name.ToLower().Contains("diagnostico"))
                    {
                        evaluacion.categoria = CategoriaEvaluacion.Diagnostico;
                    }
                    if (evaluacionMoodle.name.ToLower().Contains("final"))
                    {
                        evaluacion.categoria = CategoriaEvaluacion.Teorico;
                    }
                    evaluacion.usuarioCreacion = db.AspNetUsers.Find(User.Identity.GetUserId());
                    evaluacion.usuarioModificacion = db.AspNetUsers.Find(User.Identity.GetUserId());
                    evaluacion.fechaCreacion = DateTime.Now;
                    evaluacion.fechaModificacion = DateTime.Now;
                    evaluacion.softDelete = false;

                    var evaluacionExistente = curso.evaluaciones.Where(n => n.idQuizMoodle == evaluacion.idQuizMoodle).Where(e => e.softDelete == false).FirstOrDefault();
                    if (evaluacionExistente == null)
                    {
                        db.Evaluacion.Add(evaluacion);
                    }
                    else
                    {
                        evaluacionExistente.fechaModificacion = evaluacion.fechaModificacion;
                        evaluacionExistente.usuarioModificacion = evaluacion.usuarioModificacion;
                        evaluacionExistente.nombre = evaluacion.nombre;
                        db.Entry(evaluacionExistente).State = EntityState.Modified;
                    }
                }
            }

            db.SaveChanges();

            return RedirectToAction("EvaluacionesCurso", new { id = curso.idCurso });
        }

        // GET: Evaluacion/IngresarNotas/5
        [CustomAuthorize(new string[] { "/Comercializacions/", "/Relator/Perfil/" })]
        public ActionResult IngresarNotas(int? id,string returnUrl)
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
          
            if (comercializacion.participantes.Count == 0)
            {
                ModelState.AddModelError("", "No hay participantes");
                return View(comercializacion);
            }


            string tipoEjecucion = Convert.ToString(comercializacion.cotizacion.curso.tipoEjecucion);
            MoodleSearchUserGrades notasParticipantes = null;
            if (tipoEjecucion != null && (tipoEjecucion != "Presencial" && tipoEjecucion != "Recertificacion")) {
                notasParticipantes = Moodle.GetNotasGrupoMoodle(comercializacion, db.ParametrosMoodles.FirstOrDefault());
                if (notasParticipantes == null)
                {
                   ModelState.AddModelError("", "No se pudo actualizar las notas de Moodle");
                }
                else
                {
                    ParticipanteController participanteController = new ParticipanteController();
                    participanteController.updateNotas(notasParticipantes, comercializacion.idComercializacion, false, db.AspNetUsers.Find(User.Identity.GetUserId()));
                    comercializacion = db.Comercializacion.Find(id);
                }
            }


            List<string> headers = new List<string>();

            headers.Add("RUN");

            headers.Add("Nombre");

            foreach (var item in comercializacion.evaluaciones)
            {
                headers.Add(item.nombre);
            }

            List<List<string>> participantes = new List<List<string>>();

            foreach (var item in comercializacion.participantes)
            {
                List<string> participante = new List<string>();
                participante.Add(item.contacto.run);
                participante.Add(item.contacto.nombreCompleto);

                foreach (var evaluacion in comercializacion.evaluaciones)
                {
                    var nota = item.notas
                        .Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion)
                        .FirstOrDefault();
                    if (nota != null)
                    {
                        participante.Add(nota.nota);
                    }
                    else
                    {
                        participante.Add("-");
                    }
                }

                participantes.Add(participante);
            }
            ViewBag.returnUrl = returnUrl;
            ViewBag.role = UserManager.GetRoles(User.Identity.GetUserId()).FirstOrDefault();
            ViewBag.data = new JavaScriptSerializer().Serialize(participantes);
            ViewBag.headers = new JavaScriptSerializer().Serialize(headers);
            return View(comercializacion);
        }

        // POST: Evaluacion/IngresarNotas
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(new string[] { "/Comercializacions/", "/Relator/Perfil/" })]
        public ActionResult IngresarNotas(int? idComercializacion, string data, Uri returnUrl)
        {
            Comercializacion comercializacion = db.Comercializacion.Find(idComercializacion);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<List<String>> participantes = serializer.Deserialize<List<List<String>>>(data);
            var user = db.AspNetUsers.Find(User.Identity.GetUserId());
            foreach (var item in participantes)
            {
                var run = item[0];
                var participante = comercializacion.participantes.Where(p => p.contacto.run == run).FirstOrDefault();

                var i = 2;
                foreach (var evaluacion in comercializacion.evaluaciones)
                {
                    
                    var notaBD = participante.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault();
                    if (notaBD != null)
                    {
                        if (notaBD.nota != item[i]) {
                            notaBD.nota = item[i];
                            notaBD.fechaRealizacion = DateTime.Now;
                            notaBD.manual = true;
                            notaBD.fechaIngresoManual = DateTime.Now;
                            notaBD.usuarioIngreso = user;
                            db.Entry(notaBD).State = EntityState.Modified;
                        }
                       
                    }
                    else
                    {
                        if (item[i] != "-")
                        {
                            var nota = new Notas();
                            nota.evaluacion = evaluacion;
                            nota.participante = participante;
                            nota.nota = item[i];
                            nota.fechaRealizacion = DateTime.Now;
                            nota.manual = true;
                            nota.fechaIngresoManual = DateTime.Now;
                            nota.usuarioIngreso = user;

                            db.Notas.Add(nota);
                        }
                    }
                    i++;
                }
            }
            db.SaveChanges();

            if (returnUrl != null)
            {
                return RedirectToAction(returnUrl.ToString(), "Participante", new { id = idComercializacion });
            }
            return RedirectToAction("Notas", "Participante", new { id = idComercializacion });
        }

        // GET: Evaluacion/Details/5
        [CustomAuthorize(new string[] { "/Curso/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Evaluacion evaluacion = db.Evaluacion.Find(id);
            if (evaluacion == null)
            {
                return HttpNotFound();
            }
            return View(evaluacion);
        }

        // GET: Evaluacion/Create
        [CustomAuthorize(new string[] { "/Curso/" })]
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Curso curso = db.Curso.Find(id);
            if (curso == null)
            {
                return HttpNotFound();
            }
            Evaluacion evaluacion = new Evaluacion();
            evaluacion.curso = curso;
            ViewBag.formularios = GetFormularios();
            return View(evaluacion);
        }

        private SelectList GetFormularios()
        {
            return new SelectList(db.Formulario
                .Where(f => f.softDelete == false)
                .Where(f => f.tipoFormulario == TipoFormulario.Evaluacion)
                .Select(f => new SelectListItem
                {
                    Text = f.nombre,
                    Value = f.idFormulario.ToString()
                }).ToList(), "Value", "Text");
        }

        // POST: Evaluacion/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Curso/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idEvaluacion,nombre,tipo,categoria")] Evaluacion evaluacion, [Bind(Include = "idCurso")] Curso curso)
        {
            evaluacion.curso = db.Curso.Find(curso.idCurso);
            if (ModelState.IsValid)
            {
                if (Request["formulario.idFormulario"] != "" && Request["formulario.idFormulario"] != null)
                {
                    evaluacion.formulario = db.Formulario.Find(int.Parse(Request["formulario.idFormulario"]));
                }
                evaluacion.fechaCreacion = DateTime.Now;
                evaluacion.fechaModificacion = DateTime.Now;
                evaluacion.usuarioCreacion = db.AspNetUsers.Find(User.Identity.GetUserId());
                evaluacion.usuarioModificacion = db.AspNetUsers.Find(User.Identity.GetUserId());
                evaluacion.softDelete = false;
                db.Evaluacion.Add(evaluacion);
                db.SaveChanges();
                return RedirectToAction("EvaluacionesCurso", new { id = evaluacion.curso.idCurso });
            }

            ViewBag.formularios = GetFormularios();
            return View(evaluacion);
        }

        // POST: Evaluacion/ModificarCalificacion
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ModificarCalificacion(int idEvaluacion, int idParticipante, string nota)
        {
            var notaBD = db.Notas
                .Where(n => n.evaluacion.idEvaluacion == idEvaluacion)
                .Where(n => n.participante.idParticipante == idParticipante)
                .FirstOrDefault();
            var participante = db.Participante.Find(idParticipante);
            if (notaBD == null)
            {
                var evaluacion = db.Evaluacion.Find(idEvaluacion);
                var nuevaNota = new Notas();
                nuevaNota.evaluacion = evaluacion;
                nuevaNota.participante = participante;
                nuevaNota.nota = nota;
                db.Notas.Add(nuevaNota);
            }
            else
            {
                notaBD.nota = nota;
                db.Entry(notaBD).State = EntityState.Modified;
            }

            db.SaveChanges();
            return RedirectToAction("EvaluacionesParticipante", new { id = idParticipante, id2 = participante.comercializacion.idComercializacion });
        }

        // GET: Evaluacion/EvaluacionesParticipante/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult EvaluacionesParticipante(int? id, int? id2)
        {
            if (id == null || id2 == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comercializacion comercializacion = db.Comercializacion.Find(id2);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            var participante = db.Participante.Find(id);
            var vmEvaluacionesParticipante = new ViewModelEvaluacionesParticipante();
            vmEvaluacionesParticipante.comercializacion = comercializacion;
            vmEvaluacionesParticipante.participante = participante;
            return View(vmEvaluacionesParticipante);
        }

        // GET: Evaluacion/LlenarEvaluacionParticipante/5
        [CustomAuthorize(new string[] { "/LandingPageParticipante/" })]
        public ActionResult LlenarEvaluacionParticipante(int? id, int? id2)
        {
            if (id == null || id2 == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Evaluacion evaluacion = db.Evaluacion.Find(id2);
            if (evaluacion == null)
            {
                return HttpNotFound();
            }
            var participante = db.Participante.Find(id);
            if (participante == null)
            {
                return HttpNotFound();
            }
            var nota = db.Notas
                .Where(n => n.participante.idParticipante == participante.idParticipante)
                .Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion)
                .FirstOrDefault();
            if (nota == null)
            {
                return RedirectToAction("LlenarEvaluacion", "Evaluacion", new { id, id2, id3 = "participante" });
            }
            return RedirectToAction("LandingPage", "Participante");
        }

        // GET: Evaluacion/LlenarEvaluacion/5
        [CustomAuthorize(new string[] { "/Comercializacions/", "/LandingPageParticipante/" })]
        public ActionResult LlenarEvaluacion(int? id, int? id2, string id3)
        {
            if (id == null || id2 == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Evaluacion evaluacion = db.Evaluacion.Find(id2);
            if (evaluacion == null)
            {
                return HttpNotFound();
            }
            ViewBag.idParticipante = id;
            ViewBag.tipo = id3;
            return View(evaluacion);
        }

        // POST: Evaluacion/LlenarEvaluacion/5
        [CustomAuthorize(new string[] { "/Comercializacions/", "/LandingPageParticipante/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LlenarEvaluacion(int idEvaluacion, int idParticipante, string tipo)
        {
            var participante = db.Participante.Find(idParticipante);
            var evaluacion = db.Evaluacion.Find(idEvaluacion);
            var notaBD = db.Notas
                .Where(n => n.participante.idParticipante == participante.idParticipante)
                .Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion)
                .FirstOrDefault();
            var nota = new Notas();
            if (notaBD != null)
            {
                nota = notaBD;
            }
            else
            {
                nota.respuestas = new List<RespuestaEvaluacion>();
                nota.participante = participante;
                nota.evaluacion = evaluacion;
            }
            var necesitaRevisar = false;
            var total = 0;
            var cont = 0;
            foreach (var pregunta in evaluacion.formulario.preguntasFormularios)
            {
                if (notaBD != null)
                {
                    // eliminar respuesta si ya existe
                    var respuestaBD = notaBD.respuestas
                        .Where(r => r.pregunta.idPreguntasFormulario == pregunta.idPreguntasFormulario)
                        .FirstOrDefault();
                    if (respuestaBD != null)
                    {
                        db.RespuestaEvaluacion.Remove(respuestaBD);
                    }
                }

                if ((Request[pregunta.idPreguntasFormulario.ToString()] == null
                    || Request[pregunta.idPreguntasFormulario.ToString()] == "") && pregunta.obligatoria)
                {
                    ViewBag.idParticipante = idParticipante;
                    ModelState.AddModelError("", "Se deben responder todas las preguntas con *");
                    return View(evaluacion);
                }
                // guardar respuesta
                var respuesta = new RespuestaEvaluacion();
                respuesta.respuesta = null;
                respuesta.nota = nota;
                respuesta.pregunta = pregunta;

                if (pregunta.tipo == TipoPregunta.Alternativa)
                {
                    if (Request[pregunta.idPreguntasFormulario.ToString()] != null)
                    {
                        respuesta.respuestaFormulario = db.RespuestasFormulario.Find(int.Parse(Request[pregunta.idPreguntasFormulario.ToString()]));
                        respuesta.respuesta = respuesta.respuestaFormulario.puntaje.ToString();
                        respuesta.puntaje = respuesta.respuestaFormulario.puntaje;
                        total += respuesta.respuestaFormulario.puntaje;
                    }
                    cont++;
                }
                else
                {
                    respuesta.respuesta = Request[pregunta.idPreguntasFormulario.ToString()];
                    respuesta.puntaje = 0;
                    necesitaRevisar = true;
                }

                nota.respuestas.Add(respuesta);
            }
            if (cont != 0)
            {
                nota.porcentaje = total / cont;
                nota.nota = CalcularNota(nota.porcentaje);
            }
            if (necesitaRevisar)
            {
                nota.nota = "-";
            }
            if (notaBD != null)
            {
                db.Entry(nota).State = EntityState.Modified;
            }
            else
            {
                db.Notas.Add(nota);
            }
            db.SaveChanges();
            if (tipo == "participante")
            {
                return RedirectToAction("LandingPage", "Participante");
            }
            return RedirectToAction("EvaluacionesParticipante", "Evaluacion", new { id = participante.idParticipante, id2 = participante.comercializacion.idComercializacion });
        }

        // GET: Evaluacion/RevisarEvaluacion/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult RevisarEvaluacion(int? id, int? id2)
        {
            if (id == null || id2 == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Evaluacion evaluacion = db.Evaluacion.Find(id2);
            if (evaluacion == null)
            {
                return HttpNotFound();
            }
            Participante participante = db.Participante.Find(id);
            if (evaluacion == null)
            {
                return HttpNotFound();
            }
            var evaluacionParticipanteVM = new ViewModelEvaluacionParticipante();
            evaluacionParticipanteVM.participante = participante;
            evaluacionParticipanteVM.evaluacion = evaluacion;
            return View(evaluacionParticipanteVM);
        }

        // POST: Evaluacion/RevisarEvaluacion/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RevisarEvaluacion(int idEvaluacion, int idParticipante)
        {
            var participante = db.Participante.Find(idParticipante);
            var evaluacion = db.Evaluacion.Find(idEvaluacion);
            var notaBD = db.Notas
                .Where(n => n.participante.idParticipante == participante.idParticipante)
                .Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion)
                .FirstOrDefault();
            var nota = new Notas();
            if (notaBD != null)
            {
                nota = notaBD;
            }
            else
            {
                nota.respuestas = new List<RespuestaEvaluacion>();
                nota.participante = participante;
                nota.evaluacion = evaluacion;
            }
            var total = 0;
            var cont = 0;
            foreach (var pregunta in evaluacion.formulario.preguntasFormularios)
            {
                if ((Request[pregunta.idPreguntasFormulario.ToString()] == null
                    || Request[pregunta.idPreguntasFormulario.ToString()] == "") && pregunta.obligatoria)
                {
                    var evaluacionParticipanteVM = new ViewModelEvaluacionParticipante();
                    evaluacionParticipanteVM.participante = participante;
                    evaluacionParticipanteVM.evaluacion = evaluacion;
                    ModelState.AddModelError("", "Se deben responder todas las preguntas con *");
                    return View(evaluacionParticipanteVM);
                }
                var respuesta = nota.respuestas
                    .Where(r => r.pregunta.idPreguntasFormulario == pregunta.idPreguntasFormulario)
                    .FirstOrDefault();

                if (Request[pregunta.idPreguntasFormulario.ToString()] != "")
                {
                    respuesta.puntaje = int.Parse(Request[pregunta.idPreguntasFormulario.ToString()]);
                }
                else
                {
                    respuesta.puntaje = 0;
                }

                total += respuesta.puntaje;
                cont++;
            }

            if (cont != 0)
            {
                nota.porcentaje = total / cont;
                nota.nota = CalcularNota(nota.porcentaje);
            }

            db.Entry(nota).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("EvaluacionesParticipante", "Evaluacion", new { id = participante.idParticipante, id2 = participante.comercializacion.idComercializacion });
        }

        private string CalcularNota(double puntaje)
        {
            var nota = 0.0;
            if (puntaje < 60)
            {
                nota = 3 * (puntaje / 60) + 1;
            }
            else
            {
                nota = 3 * ((puntaje - 60) / 40) + 4;
            }
            return String.Format("{0:0.#}", nota);
            //return nota.ToString();
        }

        // GET: Evaluacion/Edit/5
        [CustomAuthorize(new string[] { "/Curso/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Evaluacion evaluacion = db.Evaluacion.Find(id);
            if (evaluacion == null)
            {
                return HttpNotFound();
            }
            return View(evaluacion);
        }

        // POST: Evaluacion/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Curso/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idEvaluacion,tipo")] Evaluacion evaluacion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(evaluacion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(evaluacion);
        }

        //// GET: Evaluacion/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Evaluacion evaluacion = db.Evaluacion.Find(id);
        //    if (evaluacion == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(evaluacion);
        //}

        // POST: Evaluacion/Delete/5
        [CustomAuthorize(new string[] { "/Curso/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Evaluacion evaluacion = db.Evaluacion.Find(id);
            evaluacion.softDelete = true;
            db.Entry(evaluacion).State = EntityState.Modified;
            //db.Evaluacion.Remove(evaluacion);
            db.SaveChanges();
            return RedirectToAction("EvaluacionesCurso", new { id = evaluacion.curso.idCurso });
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
