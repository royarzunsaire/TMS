using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class GiroController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: Giro
        [CustomAuthorize(new string[] { "/Giro/" })]
        public ActionResult Index()
        {
            return View(db.Giro.Where(g => g.softDelete == false).ToList());
        }

        // GET: Giro/Details/5
        [CustomAuthorize(new string[] { "/Giro/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Giro giro = db.Giro.Find(id);
            if (giro == null)
            {
                return HttpNotFound();
            }
            return View(giro);
        }

        // GET: Giro/Create
        [CustomAuthorize(new string[] { "/Giro/", "/Giro/Create/" })]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Giro/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Giro/", "/Giro/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "codigo,descripcion")] Giro giro)
        {
            if (ModelState.IsValid)
            {
                giro.fechaCreacion = DateTime.Now;
                giro.usuarioCreador = User.Identity.Name;
                giro.softDelete = false;
                db.Giro.Add(giro);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(giro);
        }

        // GET: Giro/Edit/5
        [CustomAuthorize(new string[] { "/Giro/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Giro giro = db.Giro.Find(id);
            if (giro == null)
            {
                return HttpNotFound();
            }
            return View(giro);
        }

        // POST: Giro/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Giro/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idGiro,codigo,descripcion")] Giro giro)
        {
            if (ModelState.IsValid)
            {
                giro.fechaCreacion = DateTime.Now;
                giro.usuarioCreador = User.Identity.Name;
                db.Entry(giro).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(giro);
        }

        // GET: Giro/Delete/5
        [CustomAuthorize(new string[] { "/Giro/" })]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Giro giro = db.Giro.Find(id);
            if (giro == null)
            {
                return HttpNotFound();
            }
            return View(giro);
        }

        // POST: Giro/Delete/5
        [CustomAuthorize(new string[] { "/Giro/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Giro giro = db.Giro.Find(id);
            //db.Giro.Remove(giro);
            giro.softDelete = true;
            db.Entry(giro).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
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
