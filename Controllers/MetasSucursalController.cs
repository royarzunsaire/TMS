using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    [CustomAuthorize(new string[] { "/MetasSucursal/Sucursales/" })]
    public class MetasSucursalController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: MetasSucursal/5
        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var metasSucursal = db.MetasSucursal.Where(x => x.sucursal.idSucursal == id).FirstOrDefault();
            if (metasSucursal == null)
            {
                metasSucursal = new MetasSucursal();
                metasSucursal.sucursal = db.Sucursal.Find(id);
                metasSucursal.metas = new List<Meta>();
                db.MetasSucursal.Add(metasSucursal);
                db.SaveChanges();
            }
            return View(metasSucursal);
        }

        // GET: MetasSucursal/Sucursales
        public ActionResult Sucursales()
        {
            return View(db.Sucursal.ToList());
        }

        //// GET: MetasSucursal/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    MetasSucursal metasSucursal = db.MetasSucursal.Find(id);
        //    if (metasSucursal == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(metasSucursal);
        //}

        // GET: MetasSucursal/Create/5
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var meta = new Meta();
            meta.metasSucursal = new MetasSucursal();
            meta.metasSucursal.sucursal = db.Sucursal.Find(id);
            meta.mes = DateTime.Now;
            return View(meta);
        }

        // POST: MetasSucursal/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idMeta,mes,monto")] Meta meta, int idSucursal)
        {
            if (ModelState.IsValid)
            {
                meta.metasSucursal = db.MetasSucursal.Where(x => x.sucursal.idSucursal == idSucursal).FirstOrDefault();
                var metaBD = db.Meta
                    .Where(x => x.metasSucursal.idMetasSucursal == meta.metasSucursal.idMetasSucursal)
                    .Where(x => x.mes.Month == meta.mes.Month && x.mes.Year == meta.mes.Year)
                    .FirstOrDefault();
                if (metaBD == null)
                {
                    meta.fechaCreacion = DateTime.Now;
                    meta.usuarioCreador = db.AspNetUsers.Find(User.Identity.GetUserId());
                    db.Meta.Add(meta);
                    db.SaveChanges();
                    return RedirectToAction("Index", new { id = meta.metasSucursal.sucursal.idSucursal });
                }
                ModelState.AddModelError("", "Ya existe una meta para ese mes");
            }
            return View(meta);
        }

        //// GET: MetasSucursal/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    MetasSucursal metasSucursal = db.MetasSucursal.Find(id);
        //    if (metasSucursal == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(metasSucursal);
        //}

        //// POST: MetasSucursal/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "idMetasSucursal")] MetasSucursal metasSucursal)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(metasSucursal).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(metasSucursal);
        //}

        //// GET: MetasSucursal/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    MetasSucursal metasSucursal = db.MetasSucursal.Find(id);
        //    if (metasSucursal == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(metasSucursal);
        //}

        // POST: MetasSucursal/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Meta meta = db.Meta.Find(id);
            var idSucursal = meta.metasSucursal.sucursal.idSucursal;
            db.Meta.Remove(meta);
            db.SaveChanges();
            return RedirectToAction("Index", new { id = idSucursal });
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
