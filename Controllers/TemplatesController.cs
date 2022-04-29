using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using SGC.Utils;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class TemplatesController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: Templates
        [CustomAuthorize(new string[] { "/Templates/" })]
        public ActionResult Index()
        {
            return View(db.Template.ToList());
        }

        // GET: Templates/Details/5
        [CustomAuthorize(new string[] { "/Templates/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Template template = db.Template.Find(id);
            if (template == null)
            {
                return HttpNotFound();
            }
            return View(template);
        }

        // GET: Templates/Create
        [CustomAuthorize(new string[] { "/Templates/", "/Templates/Create/" })]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Templates/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Templates/", "/Templates/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "idTemplate,nombre,tipo")] Template template)
        {
            // guardar archivos
            HttpPostedFileBase file = Request.Files["file"];
            // validar extenciones y tamaño maximo de los archivos
            if (file.ContentLength > 0)
            {
                var archivoValido = "";
                if (template.tipo == TipoTemplate.word)
                {
                    archivoValido = Files.ArchivoValido(file, new[] { ".docx" }, 3 * 1024);
                }
                //if (template.tipo == TipoTemplate.excel)
                //{
                //    archivoValido = Files.ArchivoValido(file, new[] { ".xlsx" }, 3);
                //}
                //if (template.tipo == TipoTemplate.pptx)
                //{
                //    archivoValido = Files.ArchivoValido(file, new[] { ".pptx" }, 3);
                //}
                //if (template.tipo == TipoTemplate.pdf)
                //{
                //    archivoValido = Files.ArchivoValido(file, new[] { ".pdf" }, 3);
                //}
                if (archivoValido != "")
                {
                    ModelState.AddModelError("template", archivoValido);
                }
            }
            else
            {
                ModelState.AddModelError("template", "Se debe seleccionar un archivo.");
            }
            if (ModelState.IsValid)
            {
                // guardar archivo
                template.template = await Files.CrearArchivoAsync(file, "template/");
                if (template.template == null)
                {
                    ModelState.AddModelError("template", "No se pudo guardar el archivo seleccionado.");
                }
            }
            if (ModelState.IsValid)
            {
                template.fechaUltimaModificacion = DateTime.Now;
                template.usuarioUltimaModificacion = db.AspNetUsers.Find(User.Identity.GetUserId());
                db.Template.Add(template);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(template);
        }

        // GET: Templates/Edit/5
        [CustomAuthorize(new string[] { "/Templates/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Template template = db.Template.Find(id);
            if (template == null)
            {
                return HttpNotFound();
            }
            return View(template);
        }

        // POST: Templates/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Templates/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "idTemplate,nombre,tipo")] Template template)
        {
            var templateBD = db.Template.Find(template.idTemplate);
            // guardar archivos
            HttpPostedFileBase file = Request.Files["file"];
            // validar extenciones y tamaño maximo de los archivos
            if (file.ContentLength > 0)
            {
                var archivoValido = "";
                if (template.tipo == TipoTemplate.word)
                {
                    archivoValido = Files.ArchivoValido(file, new[] { ".docx" }, 3 * 1024);
                }
                //if (template.tipo == TipoTemplate.excel)
                //{
                //    archivoValido = Files.ArchivoValido(file, new[] { ".xlsx" }, 3);
                //}
                //if (template.tipo == TipoTemplate.pptx)
                //{
                //    archivoValido = Files.ArchivoValido(file, new[] { ".pptx" }, 3);
                //}
                //if (template.tipo == TipoTemplate.pdf)
                //{
                //    archivoValido = Files.ArchivoValido(file, new[] { ".pdf" }, 3);
                //}
                if (archivoValido != "")
                {
                    ModelState.AddModelError("template", archivoValido);
                }
                if (ModelState.IsValid)
                {
                    await Files.BorrarArchivoAsync(templateBD.template);
                    db.Storages.Remove(templateBD.template);
                    // guardar archivo
                    templateBD.template = await Files.CrearArchivoAsync(file, "template/");
                    if (templateBD.template == null)
                    {
                        ModelState.AddModelError("template", "No se pudo guardar el archivo seleccionado.");
                    }
                }
            }
            if (ModelState.IsValid)
            {
                templateBD.nombre = template.nombre;
                templateBD.tipo = template.tipo;
                templateBD.fechaUltimaModificacion = DateTime.Now;
                templateBD.usuarioUltimaModificacion = db.AspNetUsers.Find(User.Identity.GetUserId());
                db.Entry(templateBD).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(template);
        }

        // GET: Templates/Delete/5
        [CustomAuthorize(new string[] { "/Templates/" })]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Template template = db.Template.Find(id);
            if (template == null)
            {
                return HttpNotFound();
            }
            return View(template);
        }

        // POST: Templates/Delete/5
        [CustomAuthorize(new string[] { "/Templates/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Template template = db.Template.Find(id);
            await Files.BorrarArchivoAsync(template.template);
            db.Storages.Remove(template.template);
            db.Template.Remove(template);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Templates/Descargar/5
        [CustomAuthorize(new string[] { "/Templates/" })]
        public async Task<ActionResult> Descargar(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Template template = db.Template.Find(id);
            if (template == null)
            {
                return HttpNotFound();
            }
            return await Files.BajarArchivoDescargarAsync(template.template);
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
