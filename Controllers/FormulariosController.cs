using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class FormulariosController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: Formularios
        [CustomAuthorize(new string[] { "/Formularios/" })]
        public ActionResult Index()
        {
            return View(db.Formulario.Where(f => f.softDelete == false).ToList());
        }

        // GET: Formularios/Details/5
        [CustomAuthorize(new string[] { "/Formularios/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            viewModelAdministrarFormularios viewModel = new viewModelAdministrarFormularios();
            viewModel.Formulario = db.Formulario.Find(id);
            if (viewModel.Formulario == null)
            {
                return HttpNotFound();
            }
            return View(viewModel);
        }

        // GET: Formularios/Create
        [CustomAuthorize(new string[] { "/Formularios/", "/Formularios/Create/" })]
        public ActionResult Create()
        {
            viewModelAdministrarFormularios viewModel = new viewModelAdministrarFormularios();
            return View(viewModel);
        }

        [CustomAuthorize(new string[] { "/Formularios/", "/Formularios/Create/" })]
        [HttpPost]
        public JsonResult guardarFormulario(Formulario formulario)
        {
            formulario.fechaCreacion = DateTime.Now;
            formulario.fechaUltimaModificacion = DateTime.Now;
            formulario.usuarioCreacion = db.AspNetUsers.Find(User.Identity.GetUserId());
            formulario.usuarioUltimaModificacion = db.AspNetUsers.Find(User.Identity.GetUserId());
            formulario.softDelete = false;

            if (ModelState.IsValid)
            {
                db.Formulario.Add(formulario);
                db.SaveChanges();
                return Json(true);
            }

            //// validar documento compromiso
            //var context = new ValidationContext(formulario, serviceProvider: null, items: null);
            //var results = new List<ValidationResult>();
            //var isValid = Validator.TryValidateObject(formulario, context, results, true);
            //if (isValid)
            //{
            //    db.Formulario.Add(formulario);
            //    db.SaveChanges();
            //    return Json(true);
            //}
            //// agregar los mensajes de error del documento compromiso al modelState
            //foreach (var result in results)
            //{
            //    ModelState.AddModelError("", result.ErrorMessage);
            //}
            // enviar los errores al ajax
            var errors = ModelState.ToDictionary(kvp => kvp.Key,
                    kvp => kvp.Value.Errors
                  .Select(e => e.ErrorMessage).ToArray())
                  .Where(m => m.Value.Count() > 0);
            return Json(new { HasErrors = true, Errors = errors });
        }

        [CustomAuthorize(new string[] { "/Formularios/", "/Formularios/Create/" })]
        [HttpPost]
        public JsonResult editarFormulario(Formulario formulario)
        {
            var formularioBD = db.Formulario.Find(formulario.idFormulario);
            formularioBD.nombre = formulario.nombre;
            formularioBD.tipoFormulario = formulario.tipoFormulario;
            formularioBD.descripcion = formulario.descripcion;
            formularioBD.fechaUltimaModificacion = DateTime.Now;
            formularioBD.usuarioUltimaModificacion = db.AspNetUsers.Find(User.Identity.GetUserId());
            db.Entry(formulario).State = EntityState.Detached;
            db.Entry(formularioBD).State = EntityState.Modified;
            foreach (var pregunta in formulario.preguntasFormularios)
            {
                var preguntaBD = db.PreguntasFormulario.Find(pregunta.idPreguntasFormulario);
                preguntaBD.orden = pregunta.orden;
                preguntaBD.pregunta = pregunta.pregunta;
                db.Entry(pregunta).State = EntityState.Detached;
                db.Entry(preguntaBD).State = EntityState.Modified;
                foreach (var respuesta in pregunta.respuestaFormulario)
                {
                    var respuestaBD = db.RespuestasFormulario.Find(respuesta.idRespuestasFormulario);
                    respuestaBD.orden = respuesta.orden;
                    respuestaBD.puntaje = respuesta.puntaje;
                    respuestaBD.respuesta = respuesta.respuesta;
                    db.Entry(respuesta).State = EntityState.Detached;
                    db.Entry(respuestaBD).State = EntityState.Modified;
                }
            }

            if (ModelState.IsValid)
            {
                db.SaveChanges();
                return Json(true);
            }
            var errors = ModelState.ToDictionary(kvp => kvp.Key,
                    kvp => kvp.Value.Errors
                  .Select(e => e.ErrorMessage).ToArray())
                  .Where(m => m.Value.Count() > 0);
            return Json(new { HasErrors = true, Errors = errors });
        }

        [CustomAuthorize(new string[] { "/Formularios/", "/Formularios/Create/" })]
        [HttpPost]
        public JsonResult yaExisteValido(int id)
        {
            Formulario formulario = db.Formulario.Find(id);
            //int idTipo = formulario.idTipoFormulario;
            int contador = db.Formulario.Where(x => x.tipoFormulario == formulario.tipoFormulario).ToList().Where(c => c.softDelete == false).ToList().Count;
            if (contador > 0)
            {
                return Json(true);
            }
            else
            {
                return Json(false);
            }
        }

        //[HttpPost]
        //public JsonResult cambiarValidoFormulario(int id)
        //{
        //    Formulario formulario = db.Formulario.Find(id);
        //    if (formulario.valido)
        //    {
        //        formulario.valido = false;
        //    }
        //    else
        //    {
        //        formulario.valido = true;
        //    }
        //    db.Entry(formulario).State = EntityState.Modified;
        //    db.SaveChanges();
        //    return Json(true);
        //}

        // POST: Formularios/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Formularios/", "/Formularios/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idFormulario,nombre,descripcion,tipoFormulario,fechaCreacion,usuarioCreacion")] Formulario formulario)
        {
            if (ModelState.IsValid)
            {
                formulario.fechaCreacion = DateTime.Now;
                db.Formulario.Add(formulario);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(formulario);
        }

        // GET: Formularios/Edit/5
        [CustomAuthorize(new string[] { "/Formularios/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            viewModelAdministrarFormularios viewModel = new viewModelAdministrarFormularios();
            viewModel.Formulario = db.Formulario.Find(id);
            if (viewModel.Formulario == null)
            {
                return HttpNotFound();
            }
            return View(viewModel);
        }

        // POST: Formularios/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Formularios/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Formulario formulario)
        {
            if (ModelState.IsValid)
            {
                db.Entry(formulario).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(formulario);
        }

        //// GET: Formularios/Delete/5
        //[CustomAuthorize(new string[] { "/Formularios/" })]
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Formulario formulario = db.Formulario.Find(id);
        //    if (formulario == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(formulario);
        //}

        // POST: Formularios/Delete/5
        [CustomAuthorize(new string[] { "/Formularios/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            Formulario formulario = db.Formulario.Find(id);
            //db.Formulario.Remove(formulario);
            formulario.softDelete = true;
            db.Entry(formulario).State = EntityState.Modified;
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
