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
    public class InventarioController : Controller
    {

        private InsecapContext db = new InsecapContext();

        // GET: Inventario
        [CustomAuthorize(new string[] { "/Inventario/" })]
        public ActionResult Index()
        {
            return View(db.Inventario.Where(g => g.softDelete == false).ToList());
        }

        // GET: Inventario/Details/
        [CustomAuthorize(new string[] { "/Inventario/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inventario inventario = db.Inventario.Find(id);
            if (inventario == null)
            {
                return HttpNotFound();
            }
            return View(inventario);
        }

        // GET: Inventario/Create
        [CustomAuthorize(new string[] { "/Inventario/", "/Inventario/Create/" })]
        public ActionResult Create()
        {
            ViewBag.categorias = GetCategorias();
            return View();
        }

        // POST: Inventario/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Inventario/", "/Inventario/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Codigo,FechaCompra,Stock,PeriodoMantencion,idCategoria")] Inventario inventario)
        {
            if (ModelState.IsValid)
            {
                inventario.FechaCreacion = DateTime.Now;
                inventario.categoria = db.Categoria.Find(inventario.idCategoria);
                inventario.usuarioCreador = User.Identity.Name;
                inventario.usuario = db.AspNetUsers.Find(User.Identity.GetUserId());
                inventario.softDelete = false;
                db.Inventario.Add(inventario);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.categorias = GetCategorias();
            return View(inventario);
        }

        // GET: Inventario/Edit/5
        [CustomAuthorize(new string[] { "/Inventario/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inventario inventario = db.Inventario.Find(id);
            if (inventario == null)
            {
                return HttpNotFound();
            }
            ViewBag.categorias = GetCategorias();
            return View(inventario);
        }

        // POST: Inventario/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Inventario/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idInventario,Codigo,FechaCompra,Stock,PeriodoMantencion,idCategoria")] Inventario inventario)
        {
            if (ModelState.IsValid)
            {
                inventario.FechaCreacion = DateTime.Now;
                inventario.categoria = db.Categoria.Find(inventario.idCategoria);
                inventario.usuarioCreador = User.Identity.Name;
                inventario.usuario = db.AspNetUsers.Find(User.Identity.GetUserId());
                inventario.categoria = db.Categoria.Find(inventario.idCategoria);
                db.Entry(inventario).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.categorias = GetCategorias();
            return View(inventario);
        }

        // GET: Inventario/Delete/5
        [CustomAuthorize(new string[] { "/Inventario/" })]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inventario inventario = db.Inventario.Find(id);
            if (inventario == null)
            {
                return HttpNotFound();
            }
            return View(inventario);
        }

        // POST: Inventario/Delete/5
        [CustomAuthorize(new string[] { "/Inventario/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Inventario inventario = db.Inventario.Find(id);
            inventario.softDelete = true;
            db.Entry(inventario).State = EntityState.Modified;
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

        public SelectList GetCategorias()
        {
            return new SelectList(db.Categoria.Where(x => x.softDelete == false).Select(c => new SelectListItem
            {
                Text = c.Nombre,
                Value = c.idCategoria.ToString()
            }).ToList(), "Value", "Text");
        }

    }
}