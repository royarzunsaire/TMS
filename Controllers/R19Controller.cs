using jsreport.MVC;
using jsreport.Types;
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
    public class R19Controller : Controller
    {
        private static readonly string directory = ConfigurationManager.AppSettings["directory"] + "Files/";
        private InsecapContext db = new InsecapContext();

        // GET: R19
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Index()
        {
            return View(db.R19.ToList());
        }

        // GET: R19/ConfigurarR19/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult ConfigurarR19(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var formularios = db.Formulario
                .Where(f => f.softDelete == false)
                .Where(f => f.tipoFormulario == TipoFormulario.R19)
                .ToList();
            if (db.R19.Where(r => r.comercializacion.idComercializacion == id).FirstOrDefault() != null)
            {
                return View(db.R19.Where(r => r.comercializacion.idComercializacion == id).FirstOrDefault());
            }
            R19 r19 = new R19();
            r19.encuesta = new Encuesta();
            r19.encuesta.seccionEncuesta = new List<SeccionEncuesta>();
            foreach (var item in formularios)
            {
                var seccionEncuesta = new SeccionEncuesta();
                seccionEncuesta.formulario = item;
                r19.encuesta.seccionEncuesta.Add(seccionEncuesta);
            }
            r19.comercializacion = db.Comercializacion.Find(id);
            return View(r19);
        }

        // POST: R19/GuardarR19
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult GuardarR19(ViewModelR19 configuracionR19)
        {
            var comercializacion = db.Comercializacion.Find(configuracionR19.idComercializacion);
            foreach (var item in comercializacion.relatoresCursos)
            {
                var r19 = new R19();
                r19.comercializacion = comercializacion;
                r19.relator = item.relator;
                r19.encuesta = new Encuesta();
                r19.encuesta.seccionEncuesta = new List<SeccionEncuesta>();
                foreach (var formulario in configuracionR19.formularios)
                {
                    var seccionEncuesta = new SeccionEncuesta();
                    seccionEncuesta.formulario = db.Formulario.Find(formulario.idFormulario);
                    seccionEncuesta.posicion = formulario.posicion;
                    r19.encuesta.seccionEncuesta.Add(seccionEncuesta);
                    if (formulario.encuestaRelator)
                    {
                        r19.idFormularioRelator = formulario.idFormulario;
                    }
                }
                db.R19.Add(r19);
            }
            db.SaveChanges();
            return Json(true);
        }

        public SelectList GetRelatores(Comercializacion comercializacion)
        {
            ICollection<RelatorCurso> relatores = comercializacion.relatoresCursos;
            return new SelectList(relatores.Select(r => new SelectListItem
            {
                Text = r.relator.contacto.nombres + " " + r.relator.contacto.apellidoPaterno + " " + r.relator.contacto.apellidoMaterno,
                Value = r.idRelator.ToString()
            }).ToList(), "Value", "Text");
        }

        // GET: R19/LlenarR19/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/", "/LandingPageParticipante/" })]
        public ActionResult LlenarR19(int? id, int? id2, string id3)
        {
            if (id == null || id2 == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            R19 r19 = db.R19.Where(r => r.comercializacion.idComercializacion == id2).FirstOrDefault();
            if (r19 == null)
            {
                return RedirectToAction("List", "Participante", new { id = id2 });
                //return HttpNotFound();
            }
            var participante = db.Participante.Find(id);
            //var r19vm = new ViewModelR19Respuestas();
            //r19vm.r19 = r19;
            //r19vm.respuestas = new List<RespuestasContestadasFormulario>();
            //foreach (var item in r19.encuesta.seccionEncuesta)
            //{
            //    foreach (var pregunta in item.formulario.preguntasFormularios)
            //    {
            //        var respuesta = r19.encuesta.respuestas
            //            .Where(r => r.pregunta.idPreguntasFormulario == pregunta.idPreguntasFormulario)
            //            .Where(r => r.contacto.idContacto == participante.contacto.idContacto)
            //            .FirstOrDefault();
            //        if (respuesta != null)
            //        {
            //            r19vm.respuestas.Add(respuesta);
            //        }
            //    }
            //}
            r19.relator.idRelator = 0;
            ViewBag.idParticipante = id;
            ViewBag.relatores = GetRelatores(r19.comercializacion);
            ViewBag.tipo = id3;
            return View(r19);
        }

        // POST: R19/LlenarR19/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/", "/LandingPageParticipante/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LlenarR19(int idComercializacion, int idParticipante, Relator relator, string tipo)
        {
            var participante = db.Participante.Find(idParticipante);
            var r19 = db.R19
                .Where(r => r.comercializacion.idComercializacion == idComercializacion)
                .Where(r => r.relator.idRelator == relator.idRelator)
                .FirstOrDefault();
            if (r19.encuesta.respuestas == null)
            {
                r19.encuesta.respuestas = new List<RespuestasContestadasFormulario>();
            }
            foreach (var seccionEncuesta in r19.encuesta.seccionEncuesta)
            {
                foreach (var pregunta in seccionEncuesta.formulario.preguntasFormularios)
                {
                    if ((Request[pregunta.idPreguntasFormulario.ToString()] == null
                        || Request[pregunta.idPreguntasFormulario.ToString()] == "") && pregunta.obligatoria)
                    {
                        ViewBag.idParticipante = idParticipante;
                        ViewBag.relatores = GetRelatores(r19.comercializacion);
                        ModelState.AddModelError("", "Se deben responder todas las preguntas con *");
                        return View(r19);
                    }
                    // guardar respuesta
                    var respuesta = new RespuestasContestadasFormulario();
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
                    respuesta.contacto = participante.contacto;
                    respuesta.pregunta = pregunta;
                    r19.encuesta.respuestas.Add(respuesta);
                    db.RespuestasContestadasFormulario.Add(respuesta);
                    // eliminar respuesta ya si ya existe
                    var respuestaBD = db.RespuestasContestadasFormulario
                        .Where(r => r.pregunta.idPreguntasFormulario == pregunta.idPreguntasFormulario)
                        .Where(r => r.contacto.idContacto == participante.contacto.idContacto)
                        .Where(r => r.encuesta.idEncuesta == r19.encuesta.idEncuesta)
                        .FirstOrDefault();
                    if (respuestaBD != null)
                    {
                        db.RespuestasContestadasFormulario.Remove(respuestaBD);
                    }
                }
            }
            db.Entry(r19).State = EntityState.Modified;
            db.SaveChanges();
            if (tipo == "participante")
            {
                return RedirectToAction("LandingPage", "Participante");
            }
            return RedirectToAction("List", "Participante", new { id = r19.comercializacion.idComercializacion });
        }

        //// GET: R19/ResultadosR19/5
        //public ActionResult ResultadosR19(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var listaR19 = db.R19.Where(r => r.comercializacion.idComercializacion == id).ToList();
        //    //if (r19 == null)
        //    //{
        //    //    return HttpNotFound();
        //    //}
        //    return View(listaR19);
        //}

        // POST: R19/Delete/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            R19 r19 = db.R19.Find(id);
            var idComercializacion = r19.comercializacion.idComercializacion;
            for (int i = 0; i < r19.encuesta.seccionEncuesta.Count();)
            {
                db.SeccionEncuesta.Remove(r19.encuesta.seccionEncuesta.ElementAt<SeccionEncuesta>(0));
            }
            for (int i = 0; i < r19.encuesta.respuestas.Count();)
            {
                db.RespuestasContestadasFormulario.Remove(r19.encuesta.respuestas.ElementAt<RespuestasContestadasFormulario>(0));
            }
            db.Encuesta.Remove(r19.encuesta);
            db.R19.Remove(r19);
            db.SaveChanges();
            return RedirectToAction("ConfigurarR19", new { id = idComercializacion });
        }

        public object Data(R19 r19)
        {
            var formularios = new List<object>();
            foreach (var formulario in r19.encuesta.seccionEncuesta.OrderBy(s => s.posicion))
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
                fechaInicio = r19.comercializacion.fechaInicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                fechaTermino = r19.comercializacion.fechaTermino.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                r19.comercializacion.ciudad.nombreCiudad,
                r19.comercializacion.cotizacion.codigoCotizacion,
                r19.comercializacion.cotizacion.nombreEmpresa,
                r19.comercializacion.cotizacion.nombreDiploma,
                r19.comercializacion.cotizacion.lugarRealizacion,
                r19.comercializacion.cotizacion.codigoSence,
                r19.comercializacion.cotizacion.cantidadParticipante,
                r19.comercializacion.cotizacion.curso.nombreCurso,
                r19.comercializacion.cotizacion.curso.codigoCurso,
                sucursal = r19.comercializacion.cotizacion.sucursal.nombre,
                formularios
            };
            return data;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Reporte(int? id)
        {
            var r19 = db.R19.Find(id);
            if (r19 == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r19")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r19\" y tipo \"word\".");
                return View("ConfigurarR19", r19);
            }
            return RedirectToAction("GenerarReporte", new { id });
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [EnableJsReport()]
        public async Task<ActionResult> GenerarReporte(int? id)
        {
            var r19 = db.R19.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r19")
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
                .Configure((r) => r.Data = Data(r19))
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"r19_" + r19.comercializacion.cotizacion.codigoCotizacion + ".docx\"");
            return null;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult GenerarPdfR19(int? id)
        {
            var r19 = db.R19.Find(id);
            if (r19 == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r19")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r19\" y tipo \"word\".");
                return View("ConfigurarR19", r19);
            }

            string hash = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                hash = Utils.Utils.GetHash(sha256Hash, DateTime.Now.ToString());
            }

            string createRequest = Url.Action("GenerarReportePdfR19", "R19", new { id, id2 = hash }, Request.Url.Scheme);
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
            Response.AppendHeader("Content-Disposition", "attachment; filename=\"r19_" + r19.comercializacion.cotizacion.codigoCotizacion + ".pdf\"");

            return new FileContentResult(bytes, "application/pdf");
        }

        [EnableJsReport()]
        public async Task<ActionResult> GenerarReportePdfR19(int? id, string id2)
        {
            var r19 = db.R19.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r19")
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
                .Configure((r) => r.Data = Data(r19))
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

        private object seccionesR19(List<R19> listaR19)
        {
            var formularios = new List<object>();
            foreach (var formulario in listaR19.FirstOrDefault().encuesta.seccionEncuesta)
            {
                var soloAbiertas = true;
                foreach (var pregunta in formulario.formulario.preguntasFormularios)
                {
                    if (pregunta.tipo == TipoPregunta.Abierta || pregunta.tipo == TipoPregunta.Corta)
                    {
                        soloAbiertas = false;
                    }
                }
                if (soloAbiertas)
                {
                    formularios.Add(new
                    {
                        formulario.formulario.nombre,
                        formulario.formulario.descripcion,
                        formulario.formulario.tipoFormulario,
                    });
                }
            }
            return formularios;
        }

        private object valoresRespuestasR19(List<R19> listaR19, Comercializacion comercializacion)
        {
            var participantes = new List<object>();
            var cantParticipantes = 1;
            foreach (var contacto in comercializacion.participantes)
            {
                var porcentajes = new List<object>();
                foreach (var formulario in listaR19.FirstOrDefault().encuesta.seccionEncuesta)
                {
                    var total = 0.0;
                    var cont = 0;
                    foreach (var pregunta in formulario.formulario.preguntasFormularios)
                    {
                        if (pregunta.tipo == TipoPregunta.Alternativa)
                        {
                            foreach (var r19 in listaR19)
                            {
                                if (r19.encuesta.respuestas != null && r19.encuesta.respuestas.Count() != 0)
                                {
                                    var respuesta = r19.encuesta.respuestas
                                        .Where(r => r.pregunta.idPreguntasFormulario == pregunta.idPreguntasFormulario)
                                        .Where(r => r.contacto.idContacto == contacto.contacto.idContacto)
                                        .FirstOrDefault();
                                    if (respuesta != null)
                                    {
                                        total += double.Parse(respuesta.respuesta);
                                        cont++;
                                    }
                                }
                            }
                        }
                    }
                    if (cont != 0)
                    {
                        porcentajes.Add(new
                        {
                            valor = total.ToString("#.#")
                            //porcentaje = total / cont
                        });
                    }
                }
                participantes.Add(new
                {
                    numero = cantParticipantes,
                    porcentajes
                });
                cantParticipantes++;
            }
            return participantes;
        }

        private object promediosR19(List<R19> listaR19, Comercializacion comercializacion)
        {
            var cantParticipantesContestaron = 0;
            foreach (var participante in comercializacion.participantes)
            {
                foreach (var r19 in listaR19)
                {
                    var respuesta = r19.encuesta.respuestas
                        .Where(r => r.contacto.idContacto == participante.contacto.idContacto)
                        .FirstOrDefault();
                    if (respuesta != null)
                    {
                        cantParticipantesContestaron++;
                    }
                }
            }
            var promedios = new List<object>();
            foreach (var formulario in listaR19.FirstOrDefault().encuesta.seccionEncuesta)
            {
                var total = 0.0;
                var cont = 0;
                foreach (var contacto in comercializacion.participantes)
                {
                    foreach (var pregunta in formulario.formulario.preguntasFormularios)
                    {
                        if (pregunta.tipo == TipoPregunta.Alternativa)
                        {
                            foreach (var r19 in listaR19)
                            {
                                if (r19.encuesta.respuestas != null && r19.encuesta.respuestas.Count() != 0)
                                {
                                    var respuesta = r19.encuesta.respuestas
                                        .Where(r => r.pregunta.idPreguntasFormulario == pregunta.idPreguntasFormulario)
                                        .Where(r => r.contacto.idContacto == contacto.contacto.idContacto)
                                        .FirstOrDefault();
                                    if (respuesta != null)
                                    {
                                        total += double.Parse(respuesta.respuesta);
                                        cont++;
                                    }
                                }
                            }
                        }
                    }
                }
                if (cantParticipantesContestaron != 0)
                {
                    promedios.Add(new
                    {
                        valor = (total / cantParticipantesContestaron).ToString("#.#")
                        //valor = total.ToString("#.#")
                    });
                }
            }
            return promedios;
        }

        private string promedioFinalR19(List<R19> listaR19, Comercializacion comercializacion)
        {
            var total = 0.0;
            var cont = 0;
            foreach (var formulario in listaR19.FirstOrDefault().encuesta.seccionEncuesta)
            {
                foreach (var contacto in comercializacion.participantes)
                {
                    foreach (var pregunta in formulario.formulario.preguntasFormularios)
                    {
                        if (pregunta.tipo == TipoPregunta.Alternativa)
                        {
                            foreach (var r19 in listaR19)
                            {
                                if (r19.encuesta.respuestas != null && r19.encuesta.respuestas.Count() != 0)
                                {
                                    var respuesta = r19.encuesta.respuestas
                                        .Where(r => r.pregunta.idPreguntasFormulario == pregunta.idPreguntasFormulario)
                                        .Where(r => r.contacto.idContacto == contacto.contacto.idContacto)
                                        .FirstOrDefault();
                                    if (respuesta != null)
                                    {
                                        total += double.Parse(respuesta.respuesta);
                                        cont++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            var promedioFinal = "";
            if (cont != 0)
            {
                promedioFinal = (total / cont).ToString("#.#");
            }
            return promedioFinal;
        }

        public object DataResultados(List<R19> listaR19, Comercializacion comercializacion)
        {
            var relatores = new List<object>();
            foreach (var r19 in listaR19)
            {
                relatores.Add(new
                {
                    r19.relator.contacto.nombreCompleto
                });
            }
            var data = new
            {
                relatores,
                fecha = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                fechaInicio = comercializacion.fechaInicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                fechaTermino = comercializacion.fechaTermino.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                comercializacion.observacion,
                comercializacion.ciudad.nombreCiudad,
                comercializacion.cotizacion.codigoCotizacion,
                comercializacion.cotizacion.nombreEmpresa,
                comercializacion.cotizacion.nombreDiploma,
                comercializacion.cotizacion.lugarRealizacion,
                comercializacion.cotizacion.codigoSence,
                comercializacion.cotizacion.cantidadParticipante,
                comercializacion.cotizacion.curso.nombreCurso,
                comercializacion.cotizacion.curso.codigoCurso,
                sucursal = comercializacion.cotizacion.sucursal.nombre,
                participantes = valoresRespuestasR19(listaR19, comercializacion),
                formularios = seccionesR19(listaR19),
                promedios = promediosR19(listaR19, comercializacion),
                promedioFinal = promedioFinalR19(listaR19, comercializacion)
            };
            return data;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult ReporteResultado(int? id)
        {
            var comercializacion = db.Comercializacion.Find(id);
            var listaR19 = db.R19
                .Where(r => r.comercializacion.idComercializacion == id);
            if (listaR19 == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r25")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r25\" y tipo \"word\".");
                return View("ConfigurarR19", listaR19.FirstOrDefault());
            }
            return RedirectToAction("GenerarReporteResultado", new { id });
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [EnableJsReport()]
        public async Task<ActionResult> GenerarReporteResultado(int? id)
        {
            var comercializacion = db.Comercializacion.Find(id);
            var listaR19 = db.R19
                .Where(r => r.comercializacion.idComercializacion == id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r25")
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
                .Configure((r) => r.Data = DataResultados(listaR19.ToList(), comercializacion))
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"r25_" + comercializacion.cotizacion.codigoCotizacion + ".docx\"");
            return null;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult GenerarPdfR25(int? id)
        {
            var comercializacion = db.Comercializacion.Find(id);
            var listaR19 = db.R19
                .Where(r => r.comercializacion.idComercializacion == id);
            if (listaR19 == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r25")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r25\" y tipo \"word\".");
                return View("ConfigurarR19", listaR19.FirstOrDefault());
            }

            string hash = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                hash = Utils.Utils.GetHash(sha256Hash, DateTime.Now.ToString());
            }

            string createRequest = Url.Action("GenerarReportePdfR25", "R19", new { id, id2 = hash }, Request.Url.Scheme);
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
            Response.AppendHeader("Content-Disposition", "attachment; filename=\"r25_" + comercializacion.cotizacion.codigoCotizacion + ".pdf\"");

            return new FileContentResult(bytes, "application/pdf");
        }

        [EnableJsReport()]
        public async Task<ActionResult> GenerarReportePdfR25(int? id, string id2)
        {
            var comercializacion = db.Comercializacion.Find(id);
            var listaR19 = db.R19
                .Where(r => r.comercializacion.idComercializacion == id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r25")
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
                .Configure((r) => r.Data = DataResultados(listaR19.ToList(), comercializacion))
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
