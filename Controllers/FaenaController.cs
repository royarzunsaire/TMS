using SGC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SGC.Controllers
{
    public class FaenaController : Controller
    {
        private InsecapContext db = new InsecapContext();
        // GET: Faena
        public ActionResult Index()
        {
            var faenas = db.Faena.ToList();
            return View(faenas);
        }

        // GET: Faena/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Faena/Create
        public ActionResult Create()
        {
           
            return View();
        }

        // POST: Faena/Create
        [HttpPost]
        public ActionResult Create(Faena faena)
        {
            try
            {
                var exist = db.Faena.Any(x => x.nombre.ToLower().Trim().Equals(faena.nombre.ToLower().Trim()));
                // TODO: Add insert logic here
                if (ModelState.IsValid && !exist)
                {
                    db.Faena.Add(faena);
                    db.SaveChanges();
                }
               
                else {
                      if (exist)
                    {
                        ModelState.AddModelError("", "Ya existe ese nommbre de faena");
                    }
                    return View(faena);
                }
                return RedirectToAction("Index");

            }
            catch
            {
                return View();
            }
        }

        // GET: Faena/Edit/5
        public ActionResult Edit(int id)
        {
            var faena = db.Faena.Find(id);
            if (faena == null)
            {
                ModelState.AddModelError("", "No se encontro la faena");
                return RedirectToAction("Index");
            }
           
            return View(faena);
        }

        // POST: Faena/Edit/5
        [HttpPost]
        public ActionResult Edit(Faena faena)
        {
            var exist = db.Faena.Any(x => x.nombre.ToLower().Trim().Equals(faena.nombre.ToLower().Trim()) && x.idFaena != faena.idFaena);
            try
            {
                var nombre = faena.nombre;
                if (ModelState.IsValid && !exist)
                {
                    
                    ModelState.AddModelError("", "Faena " + nombre + " actualizada correctamente");
                    db.Entry(faena).State = EntityState.Modified;
                    db.SaveChanges();
                }

                else
                {
                    if (exist)
                    {
                        ModelState.AddModelError("", "Ya existe ese nommbre de faena " + nombre);
                    }
                    return View(faena);
                }
               

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Faena/Delete/5
        public ActionResult Delete(int id)
        {
            var faena = db.Faena.Find(id);
            if (faena != null)
            {
                var nombre = faena.nombre;
                ModelState.AddModelError("", "Faena " + nombre + " eliminada correctamente");
                faena.softDelete = true;
                db.Entry(faena).State = EntityState.Modified;
                db.SaveChanges();
            
        }
            else {
                ModelState.AddModelError("", "No se encontro la faena");
            }
          
            return RedirectToAction("Index");
        }

        // POST: Faena/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
