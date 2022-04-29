using SGC.CustomAuthorize;
using SGC.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    [CustomAuthorize(new string[] { "/Relator/Perfil/" })]
    public class RelatorCursoSolicitadoController : Controller
    {
        private InsecapContext db = new InsecapContext();

        //// GET: RelatorCursoSolicitado
        //public ActionResult Index()
        //{
        //    var relatorCursoSolicitado = db.RelatorCursoSolicitado.Include(r => r.curso).Include(r => r.relator);
        //    return View(relatorCursoSolicitado.ToList());
        //}

        // GET: RelatorCursoSolicitado/SolicitarCursos/5
        public ActionResult SolicitarCursos(int? id)
        {
            var relatorCursoSolicitado = db.RelatorCursoSolicitado.Where(rcs => rcs.idRelator == id);
            ViewBag.idRelator = id;
            return View(relatorCursoSolicitado.ToList());
        }

        //// GET: RelatorCursoSolicitado/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    RelatorCursoSolicitado relatorCursoSolicitado = db.RelatorCursoSolicitado.Find(id);
        //    if (relatorCursoSolicitado == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(relatorCursoSolicitado);
        //}

        // GET: RelatorCursoSolicitado/Create
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.idCurso = new SelectList(db.Curso.Where(c => c.softDelete == false), "idCurso", "nombreCurso");
            var relatorCursoSolicitado = new RelatorCursoSolicitado();
            relatorCursoSolicitado.idRelator = (int)id;
            ViewBag.cursos = GetCursos(relatorCursoSolicitado.idRelator);
            var r11s = db.R11.ToList();
            foreach (var item in r11s)
            {
                Relator a = item.relator;
                item.relator = null;
            }
            //ViewBag.datosCursos = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(r11s);
            ViewBag.datosCursos = Newtonsoft.Json.JsonConvert.SerializeObject(r11s);
            return View(relatorCursoSolicitado);
        }

        // POST: RelatorCursoSolicitado/Create/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idCurso,idRelator")] RelatorCursoSolicitado relatorCursoSolicitado)
        {
            if (ModelState.IsValid)
            {
                db.RelatorCursoSolicitado.Add(relatorCursoSolicitado);
                db.SaveChanges();
                return RedirectToAction("SolicitarCursos", new { id = relatorCursoSolicitado.idRelator });
            }
            ViewBag.idCurso = new SelectList(db.Curso.Where(c => c.softDelete == false), "idCurso", "nombreCurso", relatorCursoSolicitado.idCurso);
            ViewBag.cursos = GetCursos(relatorCursoSolicitado.idRelator);
            var r11s = db.R11.ToList();
            foreach (var item in r11s)
            {
                Relator a = item.relator;
                item.relator = null;
            }
            ViewBag.datosCursos = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(r11s);
            return View(relatorCursoSolicitado);
        }

        public List<Curso> GetCursos(int idRelator)
        {
            var relator = db.Relators.Find(idRelator);
            var cursos = db.Curso.Where(c => c.softDelete == false).ToList();
            foreach (var item in relator.relatorCursoSolicitado)
            {
                if (cursos.Contains(item.curso))
                {
                    cursos.Remove(item.curso);
                }
            }
            foreach (var item in relator.relatorCurso)
            {
                if (cursos.Contains(item.curso) && item.softDelete == false)
                {
                    cursos.Remove(item.curso);
                }
            }
            return cursos;
        }

        //// GET: RelatorCursoSolicitado/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    RelatorCursoSolicitado relatorCursoSolicitado = db.RelatorCursoSolicitado.Find(id);
        //    if (relatorCursoSolicitado == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.idCurso = new SelectList(db.Curso, "idCurso", "codigoCurso", relatorCursoSolicitado.idCurso);
        //    ViewBag.idRelator = new SelectList(db.Relators, "idRelator", "urlStorageIdentification", relatorCursoSolicitado.idRelator);
        //    return View(relatorCursoSolicitado);
        //}

        //// POST: RelatorCursoSolicitado/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "idCurso,idRelator")] RelatorCursoSolicitado relatorCursoSolicitado)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(relatorCursoSolicitado).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.idCurso = new SelectList(db.Curso, "idCurso", "codigoCurso", relatorCursoSolicitado.idCurso);
        //    ViewBag.idRelator = new SelectList(db.Relators, "idRelator", "urlStorageIdentification", relatorCursoSolicitado.idRelator);
        //    return View(relatorCursoSolicitado);
        //}

        //// GET: RelatorCursoSolicitado/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    RelatorCursoSolicitado relatorCursoSolicitado = db.RelatorCursoSolicitado.Find(id);
        //    if (relatorCursoSolicitado == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(relatorCursoSolicitado);
        //}

        // POST: RelatorCursoSolicitado/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed()
        {
            var idRelator = int.Parse(Request["idRelator"]);
            var idCurso = int.Parse(Request["idCurso"]);
            RelatorCursoSolicitado relatorCursoSolicitado = db.RelatorCursoSolicitado.Where(rcs => rcs.idRelator == idRelator).Where(rcs => rcs.idCurso == idCurso).FirstOrDefault();
            db.RelatorCursoSolicitado.Remove(relatorCursoSolicitado);
            db.SaveChanges();
            return RedirectToAction("SolicitarCursos", new { id = idRelator });
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
