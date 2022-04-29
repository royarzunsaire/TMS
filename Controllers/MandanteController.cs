using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class MandanteController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: Mandante
        [CustomAuthorize(new string[] { "/Mandante/" })]
        public async Task<ActionResult> Index()
        {
            return View(await db.Mandante.Where(m => m.softDelete == false).ToListAsync());
        }

        // GET: Mandante/Details/5
        [CustomAuthorize(new string[] { "/Mandante/" })]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Mandante mandante = await db.Mandante.FindAsync(id);
            if (mandante == null)
            {
                return HttpNotFound();
            }
            return View(mandante);
        }

        [CustomAuthorize(new string[] { "/Cliente/", "/Cliente/Create/" })]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult NewModalCreate(string idDropDownList, int? idCliente)
        {
            ViewBag.idDropDownList = idDropDownList;
            ViewBag.idCliente = idCliente;
            return PartialView("ModalCreate");
        }

        [CustomAuthorize(new string[] { "/Cliente/", "/Cliente/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ModalCreate(Mandante mandante, int? idCliente, string tipo)
        {
            if (ModelState.IsValid)
            {
                mandante.fechaCreacion = DateTime.Now;
                mandante.vigencia = 1;
                mandante.usuarioCreador = User.Identity.Name;
                mandante.softDelete = false;
                db.Mandante.Add(mandante);
                db.SaveChanges();
                return new HttpStatusCodeResult(HttpStatusCode.Accepted);
            }
            ViewBag.idDropDownList = tipo;
            ViewBag.idCliente = idCliente;
            return PartialView("ModalCreate", mandante);
        }

        [CustomAuthorize(new string[] { "/Cliente/", "/Cliente/Create/" })]
        [HttpPost]
        public JsonResult List()
        {

            return Json((from c in db.Mandante
                         .Where(c => c.softDelete == false)
                         let Text = c.nombreMandante + " [" + c.rut + "]"
                         let Value = c.idMandante.ToString()
                         select new { Text, Value }).ToList());

        }

        [CustomAuthorize(new string[] { "/Mandante/", "/Mandante/Create/" })]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Mandante/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Mandante/", "/Mandante/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Mandante mandante)
        {
            if (ModelState.IsValid)
            {
                mandante.fechaCreacion = DateTime.Now;
                mandante.vigencia = 1;
                mandante.usuarioCreador = User.Identity.Name;
                mandante.softDelete = false;
                db.Mandante.Add(mandante);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(mandante);
        }

        // GET: Mandante/Edit/5
        [CustomAuthorize(new string[] { "/Mandante/" })]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Mandante mandante = await db.Mandante.FindAsync(id);
            if (mandante == null)
            {
                return HttpNotFound();
            }
            return View(mandante);
        }

        // POST: Mandante/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Mandante/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Mandante mandante)
        {
            if (ModelState.IsValid)
            {
                mandante.fechaCreacion = DateTime.Now;
                mandante.usuarioCreador = User.Identity.Name;
                db.Entry(mandante).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(mandante);
        }

        //// GET: Mandante/Delete/5
        //[CustomAuthorize(new string[] { "/Mandante/" })]
        //public async Task<ActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Mandante mandante = await db.Mandante.FindAsync(id);
        //    if (mandante == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(mandante);
        //}

        // POST: Mandante/Delete/5
        [CustomAuthorize(new string[] { "/Mandante/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Mandante mandante = await db.Mandante.FindAsync(id);
            mandante.softDelete = true;
            db.Entry(mandante).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
            //if (db.Cliente.Where(c => c.idMandante == mandante.idMandante).Count() == 0)
            //{
            //    db.Mandante.Remove(mandante);
            //    await db.SaveChangesAsync();
            //    return RedirectToAction("Index");
            //}
            //return View(db.Mandante.Find(id));
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
