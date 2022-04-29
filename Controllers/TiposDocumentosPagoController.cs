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
    public class TiposDocumentosPagoController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: TiposDocumentosPago
        [CustomAuthorize(new string[] { "/TiposDocumentosPago/" })]
        public ActionResult Index()
        {
            return View(db.TiposDocumentosPago.Where(t => t.softDelete == false).ToList());
        }

        // GET: TiposDocumentosPago/Details/5
        [CustomAuthorize(new string[] { "/TiposDocumentosPago/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TiposDocumentosPago tiposDocumentosPago = db.TiposDocumentosPago.Find(id);
            if (tiposDocumentosPago == null)
            {
                return HttpNotFound();
            }
            return View(tiposDocumentosPago);
        }

        // GET: TiposDocumentosPago/Create
        [CustomAuthorize(new string[] { "/TiposDocumentosPago/", "/TiposDocumentosPago/Create/" })]
        public ActionResult Create()
        {
            return View();
        }

        // POST: TiposDocumentosPago/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/TiposDocumentosPago/", "/TiposDocumentosPago/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idTipoDocumentosPago,nombre,descripcion,habilitado,usuarioCreador,fechaCreacion")] TiposDocumentosPago tiposDocumentosPago)
        {
            if (ModelState.IsValid)
            {
                tiposDocumentosPago.fechaCreacion = DateTime.Now;
                tiposDocumentosPago.usuarioCreador = User.Identity.Name;
                tiposDocumentosPago.softDelete = false;
                db.TiposDocumentosPago.Add(tiposDocumentosPago);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tiposDocumentosPago);
        }

        // GET: TiposDocumentosPago/Edit/5
        [CustomAuthorize(new string[] { "/TiposDocumentosPago/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TiposDocumentosPago tiposDocumentosPago = db.TiposDocumentosPago.Find(id);
            if (tiposDocumentosPago == null)
            {
                return HttpNotFound();
            }
            return View(tiposDocumentosPago);
        }

        // POST: TiposDocumentosPago/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/TiposDocumentosPago/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idTipoDocumentosPago,nombre,descripcion")] TiposDocumentosPago tiposDocumentosPago)
        {
            if (ModelState.IsValid)
            {
                tiposDocumentosPago.fechaCreacion = DateTime.Now;
                tiposDocumentosPago.usuarioCreador = User.Identity.Name;
                db.Entry(tiposDocumentosPago).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tiposDocumentosPago);
        }

        // GET: TiposDocumentosPago/Delete/5
        [CustomAuthorize(new string[] { "/TiposDocumentosPago/" })]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TiposDocumentosPago tiposDocumentosPago = db.TiposDocumentosPago.Find(id);
            if (tiposDocumentosPago == null)
            {
                return HttpNotFound();
            }
            return View(tiposDocumentosPago);
        }

        // POST: TiposDocumentosPago/Delete/5
        [CustomAuthorize(new string[] { "/TiposDocumentosPago/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TiposDocumentosPago tiposDocumentosPago = db.TiposDocumentosPago.Find(id);
            //db.TiposDocumentosPago.Remove(tiposDocumentosPago);
            tiposDocumentosPago.softDelete = true;
            db.Entry(tiposDocumentosPago).State = EntityState.Modified;
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
