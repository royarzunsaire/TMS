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
    [CustomAuthorize(new string[] { "/MetasVendedor/Vendedores/" })]
    public class MetasVendedorController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: MetasVendedor/5
        public ActionResult Index(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var metasVendedor = db.MetasVendedor.Where(x => x.vendedor.Id == id).FirstOrDefault();
            if (metasVendedor == null)
            {
                metasVendedor = new MetasVendedor();
                metasVendedor.vendedor = db.AspNetUsers.Find(id);
                metasVendedor.metas = new List<Meta>();
                db.MetasVendedor.Add(metasVendedor);
                db.SaveChanges();
            }
            return View(metasVendedor);
        }

        // GET: MetasVendedor/Vendedores
        public ActionResult Vendedores()
        {
            var permisosRol = db.Permission
                .Where(x => x.Menu.MenuURL == "/Cotizacion_R13/" || x.Menu.MenuURL == "/Cotizacion_R13/Create/")
                .ToList();
            var permisosUsuario = db.CustomPermission
                .Where(x => x.Menu.MenuURL == "/Cotizacion_R13/" || x.Menu.MenuURL == "/Cotizacion_R13/Create/")
                .ToList();
            var vendedores = new List<AspNetUsers>();
            foreach (var permiso in permisosRol)
            {
                foreach (var usuario in permiso.AspNetRoles.AspNetUsers)
                {
                    if (!vendedores.Contains(usuario))
                    {
                        vendedores.Add(usuario);
                    }
                }
            }
            foreach (var permiso in permisosUsuario)
            {
                if (!vendedores.Contains(permiso.AspNetUsers))
                {
                    vendedores.Add(permiso.AspNetUsers);
                }
            }

            return View(vendedores);
        }

        //// GET: MetasVendedor/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    MetasVendedor metasVendedor = db.MetasVendedor.Find(id);
        //    if (metasVendedor == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(metasVendedor);
        //}

        // GET: MetasVendedor/Create/5
        public ActionResult Create(string id)
        {
            var meta = new Meta();
            meta.metasVendedor = new MetasVendedor();
            meta.metasVendedor.vendedor = db.AspNetUsers.Find(id);
            meta.mes = DateTime.Now;
            return View(meta);
        }

        // POST: MetasVendedor/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idMeta,mes,monto")] Meta meta, string Id)
        {
            if (ModelState.IsValid)
            {
                meta.metasVendedor = db.MetasVendedor.Where(x => x.vendedor.Id == Id).FirstOrDefault();
                var metaBD = db.Meta
                    .Where(x => x.metasVendedor.idMetasVendedor == meta.metasVendedor.idMetasVendedor)
                    .Where(x => x.mes.Month == meta.mes.Month && x.mes.Year == meta.mes.Year)
                    .FirstOrDefault();
                if (metaBD == null)
                {
                    meta.fechaCreacion = DateTime.Now;
                    meta.usuarioCreador = db.AspNetUsers.Find(User.Identity.GetUserId());
                    db.Meta.Add(meta);
                    db.SaveChanges();
                    return RedirectToAction("Index", new { id = meta.metasVendedor.vendedor.Id });
                }
                ModelState.AddModelError("", "Ya existe una meta para ese mes");
            }
            return View(meta);
        }

        //// GET: MetasVendedor/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    MetasVendedor metasVendedor = db.MetasVendedor.Find(id);
        //    if (metasVendedor == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(metasVendedor);
        //}

        //// POST: MetasVendedor/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "idMetasVendedor")] MetasVendedor metasVendedor)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(metasVendedor).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(metasVendedor);
        //}

        //// GET: MetasVendedor/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    MetasVendedor metasVendedor = db.MetasVendedor.Find(id);
        //    if (metasVendedor == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(metasVendedor);
        //}

        // POST: MetasVendedor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Meta meta = db.Meta.Find(id);
            var idVendedor = meta.metasVendedor.vendedor.Id;
            db.Meta.Remove(meta);
            db.SaveChanges();
            return RedirectToAction("Index", new { id = idVendedor });
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
