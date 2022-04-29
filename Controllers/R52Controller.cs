using jsreport.MVC;
using jsreport.Types;
using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using SGC.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SGC.Controllers
{
    public class R52Controller : Controller
    {
        private static readonly string directory = ConfigurationManager.AppSettings["directory"] + "Files/";
        private InsecapContext db = new InsecapContext();

        // GET: R52/Relatores/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Relatores(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var comercialiacion = db.Comercializacion.Find(id);
            if (comercialiacion == null)
            {
                return HttpNotFound();
            }
            return View(comercialiacion);
        }

        // GET: R52/ConfigurarR52/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult ConfigurarR52(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var formularios = db.Formulario
                .Where(f => f.softDelete == false)
                .Where(f => f.tipoFormulario == TipoFormulario.R52)
                .ToList();
            if (db.R52.Where(r => r.comercializacion.idComercializacion == id).FirstOrDefault() != null)
            {
                return View(db.R52.Where(r => r.comercializacion.idComercializacion == id).FirstOrDefault());
            }
            R52 r52 = new R52();
            r52.encuesta = new Encuesta();
            r52.encuesta.seccionEncuesta = new List<SeccionEncuesta>();
            foreach (var item in formularios)
            {
                var seccionEncuesta = new SeccionEncuesta();
                seccionEncuesta.formulario = item;
                r52.encuesta.seccionEncuesta.Add(seccionEncuesta);
            }
            r52.comercializacion = db.Comercializacion.Find(id);
            return View(r52);
        }

        // POST: R52/GuardarR52
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult GuardarR52(ViewModelR19 configuracionR52)
        {
            var comercializacion = db.Comercializacion.Find(configuracionR52.idComercializacion);
            foreach (var item in comercializacion.relatoresCursos)
            {
                var r52 = new R52();
                r52.comercializacion = comercializacion;
                r52.relator = item.relator;
                r52.encuesta = new Encuesta();
                r52.encuesta.seccionEncuesta = new List<SeccionEncuesta>();
                foreach (var formulario in configuracionR52.formularios)
                {
                    var seccionEncuesta = new SeccionEncuesta();
                    seccionEncuesta.formulario = db.Formulario.Find(formulario.idFormulario);
                    seccionEncuesta.posicion = formulario.posicion;
                    r52.encuesta.seccionEncuesta.Add(seccionEncuesta);
                    if (formulario.encuestaRelator)
                    {
                        r52.idFormularioCualitativa = formulario.idFormulario;
                    }
                }
                db.R52.Add(r52);
            }
            db.SaveChanges();
            return Json(true);
        }

        // GET: R52/LlenarR52/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult LlenarR52(int? id, int? id2)
        {
            if (id == null || id2 == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            R52 r52 = db.R52
                .Where(r => r.comercializacion.idComercializacion == id2)
                .Where(r => r.relator.idRelator == id)
                .FirstOrDefault();
            if (r52 == null)
            {
                return HttpNotFound();
            }
            return View(r52);
        }

        // POST: R52/LlenarR52
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LlenarR52(int idComercializacion, int idRelator)
        {
            var r52 = db.R52
                .Where(r => r.comercializacion.idComercializacion == idComercializacion)
                .Where(r => r.relator.idRelator == idRelator)
                .FirstOrDefault();
            if (r52.encuesta.respuestas == null)
            {
                r52.encuesta.respuestas = new List<RespuestasContestadasFormulario>();
            }
            foreach (var seccionEncuesta in r52.encuesta.seccionEncuesta)
            {
                foreach (var pregunta in seccionEncuesta.formulario.preguntasFormularios)
                {
                    if ((Request[pregunta.idPreguntasFormulario.ToString()] == null
                        || Request[pregunta.idPreguntasFormulario.ToString()] == "") && pregunta.obligatoria)
                    {
                        ModelState.AddModelError("", "Se deben responder todas las preguntas con *");
                        return View(r52);
                    }
                    // guardar respuesta
                    var respuesta = new RespuestasContestadasFormulario();
                    // si es alternativa recive el id de la respuesta seleccionada
                    if (pregunta.tipo == TipoPregunta.Alternativa)
                    {
                        if (Request[pregunta.idPreguntasFormulario.ToString()] != null)
                        {
                            respuesta.respuestaFormulario = db.RespuestasFormulario.Find(int.Parse(Request[pregunta.idPreguntasFormulario.ToString()]));
                            respuesta.respuesta = respuesta.respuestaFormulario.puntaje.ToString();
                        }
                    }
                    else
                    {
                        respuesta.respuestaFormulario = pregunta.respuestaFormulario.FirstOrDefault();
                        respuesta.respuesta = Request[pregunta.idPreguntasFormulario.ToString()];
                    }
                    respuesta.contacto = r52.relator.contacto;
                    respuesta.pregunta = pregunta;
                    r52.encuesta.respuestas.Add(respuesta);
                    db.RespuestasContestadasFormulario.Add(respuesta);
                    // eliminar respuesta si ya existe
                    var respuestaBD = db.RespuestasContestadasFormulario
                        .Where(r => r.pregunta.idPreguntasFormulario == pregunta.idPreguntasFormulario)
                        .Where(r => r.encuesta.idEncuesta == r52.encuesta.idEncuesta)
                .FirstOrDefault();
                    if (respuestaBD != null)
                    {
                        db.RespuestasContestadasFormulario.Remove(respuestaBD);
                    }
                }
            }
            db.Entry(r52).State = EntityState.Modified;
            // calcular total
            var resultado = 0.0;
            int cont = 0;
            foreach (var respuesta in r52.encuesta.respuestas)
            {
                if (respuesta.pregunta.tipo == SGC.Models.TipoPregunta.Alternativa)
                {
                    if (respuesta.respuesta != null)
                    {
                        resultado += int.Parse(respuesta.respuesta);
                    }
                    cont++;
                }
            }
            if (cont > 0)
            {
                resultado = resultado / cont;
            }
            db.SaveChanges();
            if (resultado < 95)
            {
                // notificacion enuesta bajo 95%
                //var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta 95% R52").FirstOrDefault();
                //if (notificacionConfig != null)
                //{
                //    notificacionConfig.CrearNotificacion(db, r52.relator.contacto.nombreCompleto, idComercializacion.ToString(), User.Identity.GetUserId());
                //}
            }
            return RedirectToAction("Relatores", "R52", new { id = r52.comercializacion.idComercializacion });
        }

        // POST: R52/Delete/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            R52 r52 = db.R52.Find(id);
            var idComercializacion = r52.comercializacion.idComercializacion;
            for (int i = 0; i < r52.encuesta.seccionEncuesta.Count();)
            {
                db.SeccionEncuesta.Remove(r52.encuesta.seccionEncuesta.ElementAt<SeccionEncuesta>(0));
            }
            for (int i = 0; i < r52.encuesta.respuestas.Count();)
            {
                db.RespuestasContestadasFormulario.Remove(r52.encuesta.respuestas.ElementAt<RespuestasContestadasFormulario>(0));
            }
            db.Encuesta.Remove(r52.encuesta);
            db.R52.Remove(r52);
            db.SaveChanges();
            return RedirectToAction("ConfigurarR52", new { id = idComercializacion });
        }

        public object Data(R52 r52)
        {
            var formularios = new List<object>();
            foreach (var formulario in r52.encuesta.seccionEncuesta.OrderBy(s => s.posicion))
            {
                var preguntas = new List<object>();
                foreach (var pregunta in formulario.formulario.preguntasFormularios.OrderBy(p => p.orden))
                {
                    var respuestas = new List<object>();
                    foreach (var respuesta in pregunta.respuestaFormulario.OrderBy(p => p.orden))
                    {
                        respuestas.Add(new
                        {
                            respuesta.respuesta,
                            respuesta.tipoRespuesta,
                            respuesta.puntaje,
                            respuesta.orden
                        });
                    }
                    preguntas.Add(new
                    {
                        pregunta.pregunta,
                        pregunta.tipo,
                        pregunta.orden,
                        pregunta.obligatoria,
                        respuestas
                    });
                }
                formularios.Add(new
                {
                    formulario.formulario.nombre,
                    formulario.formulario.descripcion,
                    formulario.formulario.tipoFormulario,
                    preguntas
                });
            }
            var data = new
            {
                fecha = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                fechaInicio = r52.comercializacion.fechaInicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                fechaTermino = r52.comercializacion.fechaTermino.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                r52.comercializacion.ciudad.nombreCiudad,
                r52.comercializacion.cotizacion.codigoCotizacion,
                r52.comercializacion.cotizacion.nombreEmpresa,
                r52.comercializacion.cotizacion.nombreDiploma,
                r52.comercializacion.cotizacion.lugarRealizacion,
                r52.comercializacion.cotizacion.codigoSence,
                r52.comercializacion.cotizacion.cantidadParticipante,
                r52.comercializacion.cotizacion.curso.nombreCurso,
                r52.comercializacion.cotizacion.curso.codigoCurso,
                sucursal = r52.comercializacion.cotizacion.sucursal.nombre,
                formularios
            };
            return data;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Reporte(int? id)
        {
            var r52 = db.R52.Find(id);
            if (r52 == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r52")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r52\" y tipo \"word\".");
                return View("ConfigurarR52", r52);
            }
            return RedirectToAction("GenerarReporte", new { id });
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [EnableJsReport()]
        public async Task<ActionResult> GenerarReporte(int? id)
        {
            var r52 = db.R52.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r52")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
            var report = HttpContext
                .JsReportFeature()
                .Recipe(Recipe.Docx)
                .Engine(Engine.Handlebars)
                .Configure((r) => r.Template.Docx = new Docx
                {
                    TemplateAsset = new Asset
                    {
                        Content = base64,
                        Encoding = "base64"
                    }
                })
                .Configure((r) => r.Data = Data(r52))
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"r52_" + r52.comercializacion.cotizacion.codigoCotizacion + ".docx\"");
            return null;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult GenerarPdfR52(int? id)
        {
            var r52 = db.R52.Find(id);
            if (r52 == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r52")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r52\" y tipo \"word\".");
                return View("ConfigurarR52", r52);
            }

            string hash = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                hash = Utils.Utils.GetHash(sha256Hash, DateTime.Now.ToString());
            }

            string createRequest = Url.Action("GenerarReportePdfR52", "R52", new { id, id2 = hash }, Request.Url.Scheme);
            // Generate Request
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(createRequest);
            req.Method = "GET";

            // Get the Response
            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException e)
            {
                return View("Error", (object)"No se pudo generar el documento.");
            }

            var path = directory + hash;
            Byte[] bytes = System.IO.File.ReadAllBytes(path + ".pdf");

            System.IO.File.Delete(path + ".pdf");

            Response.ContentType = "application/pdf";
            Response.AppendHeader("Content-Disposition", "attachment; filename=\"r52_" + r52.comercializacion.cotizacion.codigoCotizacion + ".pdf\"");

            return new FileContentResult(bytes, "application/pdf");
        }

        [EnableJsReport()]
        public async Task<ActionResult> GenerarReportePdfR52(int? id, string id2)
        {
            var r52 = db.R52.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r52")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
            var report = HttpContext
                .JsReportFeature()
                .Recipe(Recipe.Docx)
                .Engine(Engine.Handlebars)
                .Configure((r) => r.Template.Docx = new Docx
                {
                    TemplateAsset = new Asset
                    {
                        Content = base64,
                        Encoding = "base64"
                    }
                })
                .Configure((r) => r.Data = Data(r52))
                .OnAfterRender((r) =>
                {
                    var path = directory + id2;
                    using (var file = System.IO.File.Open(path + ".docx", FileMode.Create))
                    {
                        r.Content.CopyTo(file);
                    }
                    var appWord = new Microsoft.Office.Interop.Word.Application();
                    var wordDocument = appWord.Documents.Open(path + ".docx");
                    wordDocument.ExportAsFixedFormat(path + ".pdf", Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF);
                    wordDocument.Close();
                    appWord.Quit();
                    System.IO.File.Delete(path + ".docx");
                });
            return null;
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
