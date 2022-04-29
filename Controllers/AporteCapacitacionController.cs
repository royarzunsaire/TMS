using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class AporteCapacitacionController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: AporteCapacitacion
        [CustomAuthorize(new string[] { "/AporteCapacitacion/" })]
        public ActionResult Index()
        {
            return View(db.AporteCapacitacion.Where(x => x.softDelete == false).ToList());
        }

        //// GET: AporteCapacitacion/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    AporteCapacitacion aporteCapacitacion = db.AporteCapacitacion.Find(id);
        //    if (aporteCapacitacion == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(aporteCapacitacion);
        //}

        // GET: AporteCapacitacion/Create
        [CustomAuthorize(new string[] { "/AporteCapacitacion/", "/AporteCapacitacion/Create/" })]
        public ActionResult Create()
        {
            return View();
        }

        // POST: AporteCapacitacion/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/AporteCapacitacion/", "/AporteCapacitacion/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idAporteCapacitacion,nombre,tipo,fechaCreacion,softDelete")] AporteCapacitacion aporteCapacitacion)
        {
            if (ModelState.IsValid)
            {
                var idUsuarioCreador = User.Identity.GetUserId();
                aporteCapacitacion.usuarioCreador = db.AspNetUsers.Find(idUsuarioCreador);
                aporteCapacitacion.fechaCreacion = DateTime.Now;
                db.AporteCapacitacion.Add(aporteCapacitacion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(aporteCapacitacion);
        }

        //// GET: AporteCapacitacion/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    AporteCapacitacion aporteCapacitacion = db.AporteCapacitacion.Find(id);
        //    if (aporteCapacitacion == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(aporteCapacitacion);
        //}

        //// POST: AporteCapacitacion/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "idAporteCapacitacion,nombre,tipo,fechaCreacion,softDelete")] AporteCapacitacion aporteCapacitacion)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(aporteCapacitacion).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(aporteCapacitacion);
        //}

        //// GET: AporteCapacitacion/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    AporteCapacitacion aporteCapacitacion = db.AporteCapacitacion.Find(id);
        //    if (aporteCapacitacion == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(aporteCapacitacion);
        //}

        // POST: AporteCapacitacion/Delete/5
        [CustomAuthorize(new string[] { "/AporteCapacitacion/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AporteCapacitacion aporteCapacitacion = db.AporteCapacitacion.Find(id);
            aporteCapacitacion.softDelete = true;
            db.Entry(aporteCapacitacion).State = EntityState.Modified;
            //db.AporteCapacitacion.Remove(aporteCapacitacion);
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
