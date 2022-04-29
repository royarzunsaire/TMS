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
    public class LugarAlmuerzoesController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: LugarAlmuerzoes
        [CustomAuthorize(new string[] { "/LugarAlmuerzoes/" })]
        public ActionResult Index()
        {
            return View(db.LugarAlmuerzo.Where(x => x.softDelete == false).ToList());
        }

        // GET: LugarAlmuerzoes/Details/5
        [CustomAuthorize(new string[] { "/LugarAlmuerzoes/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LugarAlmuerzo lugarAlmuerzo = db.LugarAlmuerzo.Find(id);
            if (lugarAlmuerzo == null)
            {
                return HttpNotFound();
            }
            return View(lugarAlmuerzo);
        }

        // GET: LugarAlmuerzoes/Create
        [CustomAuthorize(new string[] { "/LugarAlmuerzoes/", "/LugarAlmuerzoes/Create/" })]
        public ActionResult Create()
        {
            return View();
        }

        // POST: LugarAlmuerzoes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/LugarAlmuerzoes/", "/LugarAlmuerzoes/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idLugarAlmuerzo,nombre")] LugarAlmuerzo lugarAlmuerzo)
        {
            if (ModelState.IsValid)
            {
                lugarAlmuerzo.softDelete = false;
                db.LugarAlmuerzo.Add(lugarAlmuerzo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(lugarAlmuerzo);
        }

        // GET: LugarAlmuerzoes/Edit/5
        [CustomAuthorize(new string[] { "/LugarAlmuerzoes/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LugarAlmuerzo lugarAlmuerzo = db.LugarAlmuerzo.Find(id);
            if (lugarAlmuerzo == null)
            {
                return HttpNotFound();
            }
            return View(lugarAlmuerzo);
        }

        // POST: LugarAlmuerzoes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/LugarAlmuerzoes/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idLugarAlmuerzo,nombre")] LugarAlmuerzo lugarAlmuerzo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lugarAlmuerzo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(lugarAlmuerzo);
        }

        // GET: LugarAlmuerzoes/Delete/5
        [CustomAuthorize(new string[] { "/LugarAlmuerzoes/" })]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LugarAlmuerzo lugarAlmuerzo = db.LugarAlmuerzo.Find(id);
            if (lugarAlmuerzo == null)
            {
                return HttpNotFound();
            }
            return View(lugarAlmuerzo);
        }

        // POST: LugarAlmuerzoes/Delete/5
        [CustomAuthorize(new string[] { "/LugarAlmuerzoes/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LugarAlmuerzo lugarAlmuerzo = db.LugarAlmuerzo.Find(id);
            lugarAlmuerzo.softDelete = true;
            db.Entry(lugarAlmuerzo).State = EntityState.Modified;
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
