using SGC.CustomAuthorize;
using SGC.Models;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class CategoriaR11Controller : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: CategoriaR11
        [CustomAuthorize(new string[] { "/CategoriaR11/" })]
        public ActionResult Index(string Buscar)
        {
            if (TempData["PosseR11"] != null)
            {
                ViewBag.PosseR11 = TempData["PosseR11"];

            }
            return View(db.CategoriaR11.Where(c => c.softDelete == false).ToList());
        }

        // GET: CategoriaR11/Details/5
        [CustomAuthorize(new string[] { "/CategoriaR11/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CategoriaR11 categoriaR11 = db.CategoriaR11.Find(id);
            if (categoriaR11 == null)
            {
                return HttpNotFound();
            }
            return View(categoriaR11);
        }

        // GET: CategoriaR11/Create
        [CustomAuthorize(new string[] { "/CategoriaR11/", "/CategoriaR11/Create/" })]
        public ActionResult Create()
        {
            return View();
        }

        // POST: CategoriaR11/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/CategoriaR11/", "/CategoriaR11/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idCategoria,categoria,identificador,vigencia")] CategoriaR11 categoriaR11)
        {
            if (ModelState.IsValid)
            {
                categoriaR11.softDelete = false;
                db.CategoriaR11.Add(categoriaR11);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(categoriaR11);
        }

        // GET: CategoriaR11/Edit/5
        [CustomAuthorize(new string[] { "/CategoriaR11/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CategoriaR11 categoriaR11 = db.CategoriaR11.Find(id);
            if (categoriaR11 == null)
            {
                return HttpNotFound();
            }
            return View(categoriaR11);
        }

        // POST: CategoriaR11/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/CategoriaR11/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idCategoria,categoria,identificador,vigencia")] CategoriaR11 categoriaR11)
        {
            if (ModelState.IsValid)
            {
                db.Entry(categoriaR11).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(categoriaR11);
        }

        //// GET: CategoriaR11/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    CategoriaR11 categoriaR11 = db.CategoriaR11.Find(id);
        //    if (categoriaR11 == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(categoriaR11);
        //}

        // POST: CategoriaR11/Delete/5
        [CustomAuthorize(new string[] { "/CategoriaR11/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CategoriaR11 categoriaR11 = db.CategoriaR11.Find(id);

            //if (db.R11.Where(x => x.idCategoria == id).Count() > 0)
            //{
            //    TempData["PosseR11"] = "No se puede eliminar la categoria porque ya tiene asignada R11";
            //    return RedirectToAction("Index");
            //}
            //else
            //{
            TempData["PosseR11"] = null;

            //}

            categoriaR11.softDelete = true;
            db.Entry(categoriaR11).State = EntityState.Modified;
            //db.CategoriaR11.Remove(categoriaR11);
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
