using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class ListaDetalleCostoController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: ListaDetalleCosto
        [CustomAuthorize(new string[] { "/ListaDetalleCosto/" })]
        public ActionResult Index()
        {
            return View(db.ListaDetalleCosto.ToList());
        }

        // GET: ListaDetalleCosto/Details/5
        [CustomAuthorize(new string[] { "/ListaDetalleCosto/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ListaDetalleCosto listaDetalleCosto = db.ListaDetalleCosto.Find(id);
            if (listaDetalleCosto == null)
            {
                return HttpNotFound();
            }
            return View(listaDetalleCosto);
        }

        // GET: ListaDetalleCosto/Create
        [CustomAuthorize(new string[] { "/ListaDetalleCosto/", "/ListaDetalleCosto/Create/" })]
        public ActionResult Create()
        {
            return View();
        }

        // POST: ListaDetalleCosto/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/ListaDetalleCosto/", "/ListaDetalleCosto/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idListaDetalleCosto,detalle,activo,cantidad,valor,valorMinimo,valorMaximo,porPersona,tipoEjecucion")] ListaDetalleCosto listaDetalleCosto)
        {
            if (ModelState.IsValid)
            {
                listaDetalleCosto.fechaCreacion = DateTime.Now;
                listaDetalleCosto.usuarioCreador = User.Identity.Name;
                db.ListaDetalleCosto.Add(listaDetalleCosto);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(listaDetalleCosto);
        }

        // GET: ListaDetalleCosto/Edit/5
        [CustomAuthorize(new string[] { "/ListaDetalleCosto/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ListaDetalleCosto listaDetalleCosto = db.ListaDetalleCosto.Find(id);
            if (listaDetalleCosto == null)
            {
                return HttpNotFound();
            }
            return View(listaDetalleCosto);
        }

        // POST: ListaDetalleCosto/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/ListaDetalleCosto/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idListaDetalleCosto,detalle,activo,cantidad,valor,valorMinimo,valorMaximo,porPersona,tipoEjecucion")] ListaDetalleCosto listaDetalleCosto)
        {
            if (ModelState.IsValid)
            {
                listaDetalleCosto.fechaCreacion = DateTime.Now;
                listaDetalleCosto.usuarioCreador = User.Identity.Name;
                db.Entry(listaDetalleCosto).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(listaDetalleCosto);
        }

        //// GET: ListaDetalleCosto/Delete/5
        //[CustomAuthorize(new string[] { "/ListaDetalleCosto/" })]
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ListaDetalleCosto listaDetalleCosto = db.ListaDetalleCosto.Find(id);
        //    if (listaDetalleCosto == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(listaDetalleCosto);
        //}

        // POST: ListaDetalleCosto/Delete/5
        [CustomAuthorize(new string[] { "/ListaDetalleCosto/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ListaDetalleCosto listaDetalleCosto = db.ListaDetalleCosto.Find(id);
            db.ListaDetalleCosto.Remove(listaDetalleCosto);
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
