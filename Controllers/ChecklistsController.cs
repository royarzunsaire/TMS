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
    public class ChecklistsController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: Checklists
        [CustomAuthorize(new string[] { "/Checklists/" })]
        public ActionResult Index(string Buscar)
        {
            if (Buscar != null)
            {
                return View(db.Checklist
                    .Where(c => c.softDelete == false)
                    .Where(i => i.valor.Contains(Buscar) || i.detalle.Contains(Buscar) || i.vigencia.ToString().Contains(Buscar))
                    .ToList());
            }
            return View(db.Checklist.Where(c => c.softDelete == false).ToList());
        }

        // GET: Checklists/Details/5
        [CustomAuthorize(new string[] { "/Checklists/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Checklist checklist = db.Checklist.Find(id);
            if (checklist == null)
            {
                return HttpNotFound();
            }
            return View(checklist);
        }

        // GET: Checklists/Create
        [CustomAuthorize(new string[] { "/Checklists/", "/Checklists/Create/" })]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Checklists/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Checklists/", "/Checklists/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idChecklist,detalle,valor,vigencia")] Checklist checklist)
        {
            if (ModelState.IsValid)
            {
                checklist.softDelete = false;
                db.Checklist.Add(checklist);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(checklist);
        }

        // GET: Checklists/Edit/5
        [CustomAuthorize(new string[] { "/Checklists/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Checklist checklist = db.Checklist.Find(id);
            if (checklist == null)
            {
                return HttpNotFound();
            }
            return View(checklist);
        }

        // POST: Checklists/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Checklists/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idChecklist,detalle,valor,vigencia")] Checklist checklist)
        {
            if (ModelState.IsValid)
            {
                db.Entry(checklist).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(checklist);
        }

        //// GET: Checklists/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Checklist checklist = db.Checklist.Find(id);
        //    if (checklist == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(checklist);
        //}

        // POST: Checklists/Delete/5
        [CustomAuthorize(new string[] { "/Checklists/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Checklist checklist = db.Checklist.Find(id);
            //db.Checklist.Remove(checklist);
            checklist.softDelete = true;
            db.Entry(checklist).State = EntityState.Modified;
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
