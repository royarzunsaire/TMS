using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SGC.Controllers
{
    [Authorize]
    public class OticsController : Controller
    {
        private InsecapContext db = new InsecapContext();
        private SelectList Contactos;

        public OticsController()
        {
            // obtener todos los contactos
            Contactos = new SelectList(Utils.Utils.GetContactosDesocupados(db).Select(c => new SelectListItem
            {
                Text = "[" + c.run + "]" + " " + c.nombres + " " + c.apellidoPaterno + " " + c.apellidoMaterno,
                Value = c.idContacto.ToString()
            }).ToList(), "Value", "Text");
        }

        // GET: Otics
        [CustomAuthorize(new string[] { "/Otics/" })]
        public ActionResult Index()
        {
            return View(db.Otic.Where(x => x.softDelete == false).ToList());
        }

        // GET: Otics/Details/5
        [CustomAuthorize(new string[] { "/Otics/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Otic otic = db.Otic.Where(o => o.idOtic == id).Include(c => c.contacto).FirstOrDefault();
            if (otic == null)
            {
                return HttpNotFound();
            }
            return View(otic);
        }

        // GET: Otics/Create
        [CustomAuthorize(new string[] { "/Otics/", "/Otics/Create/" })]
        public ActionResult Create()
        {
            ViewBag.Contactos = Contactos;
            return View();
        }

        // POST: Otics/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Otics/", "/Otics/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idOtic,rut,nombre,direccion,telefono")] Otic otic)
        {
            if (Request["contacto"] == null)
            {
                ModelState.AddModelError("contacto", "El campo Contacto es obligatorio");
            }
            if (ModelState.IsValid)
            {
                Contacto contacto = db.Contacto.Find(int.Parse(Request["contacto"]));

                otic.contacto = contacto;
                otic.usuarioCreador = User.Identity.GetUserId();
                otic.fechaCreacion = DateTime.Now;
                otic.softDelete = false;

                db.Otic.Add(otic);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Contactos = Contactos;
            return View(otic);
        }

        // GET: Otics/Edit/5
        [CustomAuthorize(new string[] { "/Otics/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Otic otic = db.Otic.Where(o => o.idOtic == id).Include(c => c.contacto).FirstOrDefault();
            if (otic == null)
            {
                return HttpNotFound();
            }
            ViewBag.contactoSeleccionado = new JavaScriptSerializer().Serialize(otic.contacto.idContacto.ToString());
            ViewBag.Contactos = GetContactosOtic(otic);
            return View(otic);
        }

        // POST: Otics/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Otics/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idOtic,rut,nombre,direccion,telefono")] Otic otic)
        {
            Otic oticOriginal = db.Otic.Where(o => o.idOtic == otic.idOtic).Include(c => c.contacto).FirstOrDefault();
            if (ModelState.IsValid)
            {
                Contacto contacto = db.Contacto.Find(int.Parse(Request["contacto"]));

                oticOriginal.nombre = otic.nombre;
                oticOriginal.rut = otic.rut;
                oticOriginal.direccion = otic.direccion;
                oticOriginal.telefono = otic.telefono;

                oticOriginal.contacto = contacto;
                oticOriginal.usuarioCreador = User.Identity.GetUserId();
                oticOriginal.fechaCreacion = DateTime.Now;

                db.Entry(oticOriginal).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.contactoSeleccionado = new JavaScriptSerializer().Serialize(oticOriginal.contacto.idContacto.ToString());
            if (otic.contacto != null)
            {
                ViewBag.Contactos = GetContactosOtic(otic);
            }
            else
            {
                ViewBag.Contactos = Contactos;
            }
            return View(otic);
        }

        // GET: Otics/Delete/5
        [CustomAuthorize(new string[] { "/Otics/" })]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Otic otic = db.Otic.Find(id);
            if (otic == null)
            {
                return HttpNotFound();
            }
            return View(otic);
        }

        // POST: Otics/Delete/5
        [CustomAuthorize(new string[] { "/Otics/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Otic otic = db.Otic.Find(id);
            otic.softDelete = true;
            db.Entry(otic).State = EntityState.Modified;
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

        public SelectList GetContactosOtic(Otic otic)
        {
            List<Contacto> contactos = Utils.Utils.GetContactosDesocupados(db);
            if (!contactos.Contains(otic.contacto))
            {
                contactos.Add(otic.contacto);
            }
            return new SelectList(contactos.Select(c => new SelectListItem
            {
                Text = "[" + c.run + "]" + " " + c.nombres + " " + c.apellidoPaterno + " " + c.apellidoMaterno,
                Value = c.idContacto.ToString()
            }).ToList(), "Value", "Text");
        }
    }
}
