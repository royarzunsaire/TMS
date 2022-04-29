using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class SalasController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: Salas
        [CustomAuthorize(new string[] { "/Salas/" })]
        public ActionResult Index()
        {
            return View(db.Sala.Where(x => x.softDelete == false).ToList());
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveSala()
        {
            int idBloque = Convert.ToInt32(Request["idBloque"]);
            int id = Convert.ToInt32(Request["idSala"]);
            var error = "ok";
            var bloque = db.Bloque.Find(idBloque);
            var sala = db.Sala.Find(id);
            var bloques = bloque.comercializacion.bloques.Where(x => x.fecha == bloque.fecha).ToList();

            try
            {

                foreach (var item in bloques)
                {
                    item.sala = sala;
                    db.Entry(item).State = EntityState.Modified;

                }
                db.SaveChanges();
            }
            catch (Exception e)
            {
                error = e.Message;
            }



            var jsonResult = Json(new { error, id }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        // GET: Salas/Details/5
        [CustomAuthorize(new string[] { "/Salas/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sala sala = db.Sala.Find(id);
            if (sala == null)
            {
                return HttpNotFound();
            }
            return View(sala);
        }

        // GET: Salas/Create
        [CustomAuthorize(new string[] { "/Salas/", "/Salas/Create/" })]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Salas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Salas/", "/Salas/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idSala,nombre")] Sala sala)
        {
            if (ModelState.IsValid)
            {
                sala.softDelete = false;
                db.Sala.Add(sala);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(sala);
        }

        // GET: Salas/Edit/5
        [CustomAuthorize(new string[] { "/Salas/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sala sala = db.Sala.Find(id);
            if (sala == null)
            {
                return HttpNotFound();
            }
            return View(sala);
        }

        // POST: Salas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Salas/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idSala,nombre")] Sala sala)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sala).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sala);
        }

        // GET: Salas/Delete/5
        [CustomAuthorize(new string[] { "/Salas/" })]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sala sala = db.Sala.Find(id);
            if (sala == null)
            {
                return HttpNotFound();
            }
            return View(sala);
        }

        // POST: Salas/Delete/5
        [CustomAuthorize(new string[] { "/Salas/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Sala sala = db.Sala.Find(id);
            sala.softDelete = true;
            db.Entry(sala).State = EntityState.Modified;
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
