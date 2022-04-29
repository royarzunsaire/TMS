using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class RelatorCursoController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: RelatorCurso/
        [CustomAuthorize(new string[] { "/RelatorCurso/" })]
        public ActionResult Index()
        {
            var relatorCurso = db.RelatorCurso
                .Where(rc => rc.softDelete == false)
                .Where(rc => rc.curso.softDelete == false)
                .ToList();
            ViewBag.cursos = GetCursos();
            ViewBag.relatores = GetRelatores();
            return View(relatorCurso);
        }
      
     
        public bool RelatorComercializacion()
        {
            var date = new DateTime(2021, 08, 31);
            List<RelatorCurso> relatorCurso = db.RelatorCurso.ToList();
            foreach (var item in relatorCurso)
            {
                foreach (var itemC in item.comercializaciones.Where(x => x.fechaInicio < date).Where(x => x.softDelete == false)
               
               .ToList())
                {
                    itemC.relatoresConfirmados.Add(item.relator);
                    db.Entry(itemC).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

              return true;
        }
        // GET: RelatorCurso/Solicitudes
        [CustomAuthorize(new string[] { "/RelatorCurso/" })]
        public ActionResult Solicitudes()
        {
            var relatorCursoSolicitado = db.RelatorCursoSolicitado
                .ToList();
            ViewBag.cursos = GetCursos();
            ViewBag.relatores = GetRelatores();
            return View(relatorCursoSolicitado);
        }

        //// GET: RelatorCurso/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    RelatorCurso relatorCurso = db.RelatorCurso.Find(id);
        //    if (relatorCurso == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(relatorCurso);
        //}

        // GET: RelatorCurso/Create
        [CustomAuthorize(new string[] { "/RelatorCurso/", "/Relator/Create/" })]
        public ActionResult Create()
        {
            ViewBag.cursos = GetCursos();
            ViewBag.relatores = GetRelatores();
            return View();
        }

        // POST: RelatorCurso/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/RelatorCurso/", "/Relator/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idCurso,idRelator,validoSence,reuf")] RelatorCurso relatorCurso)
        {
            //if (Request["cursos"] == "")
            //{
            //    ModelState.AddModelError("cursos", "El campo Curso es obligatorio");
            //}
            //if (Request["relatores"] == "")
            //{
            //    ModelState.AddModelError("relatores", "El campo Relator es obligatorio");
            //}
            //Curso curso = db.Curso.Find(int.Parse(Request["cursos"]));
            //Relator relator = db.Relators.Find(int.Parse(Request["relator"]));
            //relatorCurso.idCurso = curso.idCurso;
            //relatorCurso.idRelator = relator.idRelator;
            if (ModelState.IsValid)
            {
                relatorCurso.curso = db.Curso.Find(relatorCurso.idCurso);
                relatorCurso.relator = db.Relators.Find(relatorCurso.idRelator);
                relatorCurso.usuarioCreador = User.Identity.GetUserId();
                relatorCurso.fechaCreacion = DateTime.Now;
                relatorCurso.softDelete = false;

                if (relatorCurso.validoSence)
                {
                    relatorCurso.fechaValidoSence = DateTime.Now;
                }

                RelatorCurso relatorCursoBD = db.RelatorCurso
                    .Where(rc => rc.idCurso == relatorCurso.idCurso)
                    .Where(rc => rc.idRelator == relatorCurso.idRelator)
                    .FirstOrDefault();
                if (relatorCursoBD == null)
                {
                    db.RelatorCurso.Add(relatorCurso);
                }
                else
                {
                    if (relatorCursoBD.softDelete == false)
                    {
                        ModelState.AddModelError("", "El relator ya esta relacionado a ese curso");
                        ViewBag.cursos = GetCursos();
                        ViewBag.relatores = GetRelatores();
                        return View(relatorCurso);
                    }
                    else
                    {
                        relatorCursoBD.validoSence = relatorCurso.validoSence;
                        relatorCursoBD.reuf = relatorCurso.reuf;
                        relatorCursoBD.usuarioCreador = User.Identity.GetUserId();
                        relatorCursoBD.fechaCreacion = DateTime.Now;
                        relatorCursoBD.softDelete = false;

                        if (relatorCursoBD.validoSence)
                        {
                            relatorCursoBD.fechaValidoSence = DateTime.Now;
                        }

                        db.Entry(relatorCursoBD).State = EntityState.Modified;
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.cursos = GetCursos();
            ViewBag.relatores = GetRelatores();
            return View(relatorCurso);
        }

        // GET: RelatorCurso/Aceptar/5
        [CustomAuthorize(new string[] { "/RelatorCurso/" })]
        public ActionResult Aceptar(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            String[] ids = id.ToString().Split('-');
            int idCurso = int.Parse(ids[0]);
            int idRelator = int.Parse(ids[1]);
            RelatorCurso relatorCurso = new RelatorCurso();
            relatorCurso.idCurso = idCurso;
            relatorCurso.idRelator = idRelator;
            relatorCurso.curso = db.Curso.Find(idCurso);
            relatorCurso.relator = db.Relators.Find(idRelator);
            return View(relatorCurso);
        }

        // POST: RelatorCurso/Aceptar/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/RelatorCurso/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Aceptar([Bind(Include = "idCurso,idRelator,validoSence,reuf")] RelatorCurso relatorCurso)
        {
            if (ModelState.IsValid)
            {
                relatorCurso.curso = db.Curso.Find(relatorCurso.idCurso);
                relatorCurso.relator = db.Relators.Find(relatorCurso.idRelator);
                relatorCurso.usuarioCreador = User.Identity.GetUserId();
                relatorCurso.fechaCreacion = DateTime.Now;
                relatorCurso.softDelete = false;

                if (relatorCurso.validoSence)
                {
                    relatorCurso.fechaValidoSence = DateTime.Now;
                }

                RelatorCurso relatorCursoBD = db.RelatorCurso
                    .Where(rc => rc.idCurso == relatorCurso.idCurso)
                    .Where(rc => rc.idRelator == relatorCurso.idRelator)
                    .FirstOrDefault();
                if (relatorCursoBD == null)
                {
                    db.RelatorCurso.Add(relatorCurso);
                    var relatorCursoSolicitado = db.RelatorCursoSolicitado
                        .Where(rcs => rcs.idRelator == relatorCurso.idRelator)
                        .Where(rcs => rcs.idCurso == relatorCurso.idCurso)
                        .FirstOrDefault();
                    db.RelatorCursoSolicitado.Remove(relatorCursoSolicitado);
                    // notificacion curso aceptado
                    var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Notificacion Curso Aceptado").FirstOrDefault();
                    if (notificacionConfig != null)
                    {
                        notificacionConfig.CrearNotificacionUsuario(db, relatorCurso.curso.nombreCurso, relatorCurso.idRelator.ToString(), User.Identity.GetUserId(), relatorCurso.relator.contacto.usuario);
                    }
                }
                else
                {
                    if (relatorCursoBD.softDelete == false)
                    {
                        ModelState.AddModelError("", "El relator ya esta relacionado a ese curso");
                        return View(relatorCurso);
                    }
                    else
                    {
                        relatorCursoBD.validoSence = relatorCurso.validoSence;
                        relatorCursoBD.reuf = relatorCurso.reuf;
                        relatorCursoBD.usuarioCreador = User.Identity.GetUserId();
                        relatorCursoBD.fechaCreacion = DateTime.Now;
                        relatorCursoBD.softDelete = false;

                        if (relatorCursoBD.validoSence)
                        {
                            relatorCursoBD.fechaValidoSence = DateTime.Now;
                        }

                        db.Entry(relatorCursoBD).State = EntityState.Modified;
                        var relatorCursoSolicitado = db.RelatorCursoSolicitado
                            .Where(rcs => rcs.idRelator == relatorCurso.idRelator)
                            .Where(rcs => rcs.idCurso == relatorCurso.idCurso)
                            .FirstOrDefault();
                        db.RelatorCursoSolicitado.Remove(relatorCursoSolicitado);
                        // notificacion curso aceptado
                        var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Notificacion Curso Aceptado").FirstOrDefault();
                        if (notificacionConfig != null)
                        {
                            notificacionConfig.CrearNotificacionUsuario(db, relatorCurso.curso.nombreCurso, relatorCurso.idRelator.ToString(), User.Identity.GetUserId(), relatorCurso.relator.contacto.usuario);
                        }
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Solicitudes");
            }
            return View(relatorCurso);
        }

        // POST: RelatorCurso/Rechazar/5
        [CustomAuthorize(new string[] { "/RelatorCurso/" })]
        [HttpPost, ActionName("Rechazar")]
        [ValidateAntiForgeryToken]
        public ActionResult Rechazar()
        {
            int idCurso = int.Parse(Request["idCurso"]);
            int idRelator = int.Parse(Request["idRelator"]);
            var relatorCursoSolicitado = db.RelatorCursoSolicitado
                .Where(rcs => rcs.idCurso == idCurso)
                .Where(rcs => rcs.idRelator == idRelator)
                .FirstOrDefault();
            db.RelatorCursoSolicitado.Remove(relatorCursoSolicitado);
            db.SaveChanges();
            // notificacion curso rechazado
            var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Notificacion Curso Rechazado").FirstOrDefault();
            if (notificacionConfig != null)
            {
                notificacionConfig.CrearNotificacionUsuario(db, db.Curso.Find(idCurso).nombreCurso, idRelator.ToString(), User.Identity.GetUserId(), db.Relators.Find(idRelator).contacto.usuario);
            }
            return RedirectToAction("Solicitudes");
        }

        // GET: RelatorCurso/Edit/5
        [CustomAuthorize(new string[] { "/RelatorCurso/" })]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            String[] ids = id.ToString().Split('-');
            int idCurso = int.Parse(ids[0]);
            int idRelator = int.Parse(ids[1]);
            RelatorCurso relatorCurso = db.RelatorCurso
                .Where(rc => rc.idCurso == idCurso)
                .Where(rc => rc.idRelator == idRelator)
                .FirstOrDefault();
            if (relatorCurso == null)
            {
                return HttpNotFound();
            }
            return View(relatorCurso);
        }

        // POST: RelatorCurso/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/RelatorCurso/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idCurso,idRelator,validoSence,reuf")] RelatorCurso relatorCurso)
        {
            RelatorCurso relatorCursoBD = db.RelatorCurso
                .Where(rc => rc.idCurso == relatorCurso.idCurso)
                .Where(rc => rc.idRelator == relatorCurso.idRelator)
                .FirstOrDefault();
            if (ModelState.IsValid)
            {
                if (relatorCurso.validoSence && !relatorCursoBD.validoSence)
                {
                    relatorCursoBD.fechaValidoSence = DateTime.Now;
                }

                relatorCursoBD.validoSence = relatorCurso.validoSence;
                relatorCursoBD.reuf = relatorCurso.reuf;
                relatorCursoBD.usuarioCreador = User.Identity.GetUserId();
                relatorCursoBD.fechaCreacion = DateTime.Now;

                db.Entry(relatorCursoBD).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = relatorCursoBD.idCurso });
            }
            return View(relatorCurso);
        }

        // GET: RelatorCurso/Delete/5
        [CustomAuthorize(new string[] { "/RelatorCurso/" })]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            String[] ids = id.ToString().Split('-');
            int idCurso = int.Parse(ids[0]);
            int idRelator = int.Parse(ids[1]);
            RelatorCurso relatorCurso = db.RelatorCurso
                .Where(rc => rc.idCurso == idCurso)
                .Where(rc => rc.idRelator == idRelator)
                .FirstOrDefault();
            if (relatorCurso == null)
            {
                return HttpNotFound();
            }
            return View(relatorCurso);
        }

        // POST: RelatorCurso/Delete/5
        [CustomAuthorize(new string[] { "/RelatorCurso/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            String[] ids = id.ToString().Split('-');
            int idCurso = int.Parse(ids[0]);
            int idRelator = int.Parse(ids[1]);
            RelatorCurso relatorCurso = db.RelatorCurso
                .Where(rc => rc.idCurso == idCurso)
                .Where(rc => rc.idRelator == idRelator)
                .FirstOrDefault();
            relatorCurso.softDelete = true;
            db.Entry(relatorCurso).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index", new { id = relatorCurso.curso.idCurso });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //public SelectList GetRelatoresCurso(int idCurso)
        //{
        //    List<Relator> relatoresBD = db.Relators
        //        .Where(r => r.softDelete == false)
        //        .ToList();
        //    List<Relator> relatores = new List<Relator>();
        //    foreach (var relator in relatoresBD)
        //    {
        //        RelatorCurso relatorCursoBD = db.RelatorCurso
        //            .Where(rc => rc.idCurso == idCurso)
        //            .Where(rc => rc.idRelator == relator.idRelator)
        //            .FirstOrDefault();
        //        if (relatorCursoBD == null)
        //        {
        //            relatores.Add(relator);
        //        }
        //        else
        //        {
        //            if (relatorCursoBD.softDelete == true)
        //            {
        //                relatores.Add(relator);
        //            }
        //        }
        //    }
        //    //string q = "select distinct r.* from DB_SGC.dbo.Relators r left join DB_SGC.dbo.RelatorCursoes rc on rc.idRelator = r.idRelator where r.softDelete = 0 and(rc.idCurso <> " + idCurso + " and rc.softDelete = 0) or(rc.idCurso = " + idCurso + " and rc.softDelete = 1)";
        //    return new SelectList(relatores
        //        .Select(re => new SelectListItem
        //        {
        //            Text = "[" + re.contacto.run + "]" + " " + re.contacto.nombres + " " + re.contacto.apellidoPaterno + " " + re.contacto.apellidoMaterno,
        //            Value = re.idRelator.ToString()
        //        }).ToList(), "Value", "Text");
        //}

        public SelectList GetRelatores()
        {
            List<Relator> relatores = db.Relators
                .Where(r => r.softDelete == false)
                .ToList();
            return new SelectList(relatores
                .Select(r => new SelectListItem
                {
                    Text = "[" + r.contacto.run + "]" + " " + r.contacto.nombres + " " + r.contacto.apellidoPaterno + " " + r.contacto.apellidoMaterno,
                    Value = r.idRelator.ToString()
                }).ToList(), "Value", "Text");
        }

        public SelectList GetCursos()
        {
            var cursos = db.Curso
          .Where(x => x.softDelete == false)
          .Join(
              db.R11,
              curso => curso.idCurso,
              r11 => r11.idCurso,
              (curso, r11) => new 
              {
                  curso = curso,
                  r11 = r11
              }
          ).ToList();
            return new SelectList(cursos
                .Select(c => new SelectListItem
                {
                    Text = "[" + c.curso.tipoEjecucion + "- "+ (c.r11.horasPracticas + c.r11.horasTeoricas) + " horas] " + c.curso.nombreCurso,
                    Value = c.curso.idCurso.ToString()
                }).ToList(), "Value", "Text");
        }
    }
}
