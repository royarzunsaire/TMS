using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace SGC.Controllers
{
    [Authorize]
    public class InventarioCaracteristicasController : Controller
    {

        private InsecapContext db = new InsecapContext();

        // GET: InventarioCaracteristicas
        [CustomAuthorize(new string[] { "/InventarioCaracteristicas/" })]
        public ActionResult Index()
        {
            return View(db.InventarioCaracteristicas.Where(g => g.softDelete == false).ToList());
        }

        // GET: InventarioCaracteristicas/Details/
        [CustomAuthorize(new string[] { "/InventarioCaracteristicas/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventarioCaracteristicas inventariocaracteristicas = db.InventarioCaracteristicas.Find(id);
            if (inventariocaracteristicas == null)
            {
                return HttpNotFound();
            }
            return View(inventariocaracteristicas);
        }

        // GET: InventarioCaracteristicas/Create
        [CustomAuthorize(new string[] { "/InventarioCaracteristicas/", "/InventarioCaracteristicas/Create/" })]
        public ActionResult Create()
        {
            ViewBag.categorias = GetCategorias();
            return View();
        }

        // POST: InventarioCaracteristicas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/InventarioCaracteristicas/", "/InventarioCaracteristicas/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idCategoria,Detalle,Caracteristica1,Caracteristica2,Caracteristica3,Caracteristica4,Caracteristica5")] InventarioCaracteristicas inventariocaracteristicas)
        {
            if (ModelState.IsValid)
            {

                inventariocaracteristicas.categoria = db.Categoria.Find(inventariocaracteristicas.idCategoria);
                inventariocaracteristicas.softDelete = false;
                db.InventarioCaracteristicas.Add(inventariocaracteristicas);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.categorias = GetCategorias();
            return View(inventariocaracteristicas);
        }

        // GET: InventarioCaracteristicas/Edit/5
        [CustomAuthorize(new string[] { "/InventarioCaracteristicas/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventarioCaracteristicas inventariocaracteristicas = db.InventarioCaracteristicas.Find(id);
            if (inventariocaracteristicas == null)
            {
                return HttpNotFound();
            }
            ViewBag.categorias = GetCategorias();
            return View(inventariocaracteristicas);
        }

        // POST: InventarioCaracteristicas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/InventarioCaracteristicas/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idInventarioCaracteristica,idCategoria,Detalle,Caracteristica1,Caracteristica2,Caracteristica3,Caracteristica4,Caracteristica5")] InventarioCaracteristicas inventariocaracteristicas)
        {
            if (ModelState.IsValid)
            {
                inventariocaracteristicas.categoria = db.Categoria.Find(inventariocaracteristicas.idCategoria);
                inventariocaracteristicas.softDelete = false;
                db.Entry(inventariocaracteristicas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.categorias = GetCategorias();
            return View(inventariocaracteristicas);
        }

        public SelectList GetCategorias()
        {
            return new SelectList(db.Categoria.Where(x => x.softDelete == false).Select(c => new SelectListItem
            {
                Text = c.Nombre,
                Value = c.idCategoria.ToString()
            }).ToList(), "Value", "Text");
        }

        // GET: InventarioCaracteristicas/Delete/5
        [CustomAuthorize(new string[] { "/InventarioCaracteristicas/" })]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventarioCaracteristicas inventariocaracteristicas = db.InventarioCaracteristicas.Find(id);
            if (inventariocaracteristicas == null)
            {
                return HttpNotFound();
            }
            return View(inventariocaracteristicas);
        }

        // POST: InventarioCaracteristicas/Delete/5
        [CustomAuthorize(new string[] { "/InventarioCaracteristicas/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            InventarioCaracteristicas inventariocaracteristicas = db.InventarioCaracteristicas.Find(id);
            inventariocaracteristicas.softDelete = true;
            db.Entry(inventariocaracteristicas).State = EntityState.Modified;
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