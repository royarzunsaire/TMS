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
    public class R11Controller : Controller
    {
        private static readonly string directory = ConfigurationManager.AppSettings["directory"] + "Files/";
        //private static readonly string directory = System.Web.HttpContext.Current.Server.MapPath("~/Files/");
        private InsecapContext db = new InsecapContext();

        // GET: R11
        //public ActionResult Index(string Buscar)
        //{
        //    ViewModelR11 viewModelR11 = new ViewModelR11();
        //    if (Buscar != null)
        //    {
        //        viewModelR11.r51  = db.R51
        //            .Where(r => r.softDelete == false)
        //            .Where(i => i.fechaCreacion.ToString().Contains(Buscar))
        //            .ToList();
        //    }
        //    else
        //    {
        //        viewModelR11.r51 = db.R51.Where(r => r.softDelete == false).ToList();
        //    }
        //    viewModelR11.r11 = db.R11.Where(r => r.softDelete == false).ToList();
        //    var listaIdCurso = viewModelR11.r51.Select(o => o.idCurso).ToList();
        //    viewModelR11.curso = db.Curso.Where(x => listaIdCurso.Contains(x.idCurso)).ToList();
        //    return View(viewModelR11);
        //}

        // GET: R11/Details/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Curso/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            R11 r11 = db.R11.Find(id);
            if (r11 == null)
            {
                return HttpNotFound();
            }
            ViewModelR11 viewModel = new ViewModelR11();

            viewModel.curso = db.Curso.Where(x => x.idCurso == r11.idCurso).ToList();
            viewModel.categoriaR11 = db.CategoriaR11.Where(x => x.vigencia == 1).ToList();
            viewModel.r11Entity = r11;
            viewModel.escolaridadR11 = db.EscolaridadR11.Where(x => x.idR11 == r11.idR11).ToList();
            viewModel.contenidoEspecifico = db.ContenidoEspecificoR11.Where(x => x.idR11 == r11.idR11).ToList();
            var listIdContenidoEspecifico = db.ContenidoEspecificoR11.Where(x => x.idR11 == r11.idR11).Select(o => o.idContenidoEspecificoR11).ToList();
            viewModel.itemContenidoEspecificoR11 = db.ItemContenidoEspecificoR11.Where(x => listIdContenidoEspecifico.Contains(x.idContenidoEspecificoR11)).ToList();

            return View(viewModel);
        }

        // GET: R11/R11Curso/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Curso/" })]
        public ActionResult R11Curso(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            R11 r11 = db.R11.Where(r => r.idCurso == id).FirstOrDefault();
            if (r11 == null)
            {
                return RedirectToAction("Create", new { id = id });
            }
            return RedirectToAction("Details", new { id = r11.idR11 });
        }

        // GET: R11/Create
        [Authorize]
        [CustomAuthorize(new string[] { "/Curso/" })]
        public ActionResult Create(int id)
        {
            ViewModelR11 viewModel = new ViewModelR11();
            viewModel.curso = db.Curso.Where(x => x.idCurso == id).ToList();
            viewModel.categoriaR11 = db.CategoriaR11.Where(x => x.softDelete == false).Where(x => x.vigencia == 1).ToList();
            var idRelatores = db.RelatorCurso.Where(x => x.idCurso == id).Select(y => y.idRelator).ToList();
            //viewModel.instructores = db.Relators.Where(x => idRelatores.Contains(x.idRelator)).Select(y=> y.contacto).ToList();
            ViewBag.relatores = GetRelatores();
            return View(viewModel);
        }

        //private string GetFileFromBlob(string fileName)
        //{

        //    MemoryStream ms = new MemoryStream();

        //    CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=storageinsecap;AccountKey=BKxl/Mf5BVdR//yF1Ui9An5pFM4bDuRHue5iypm9nJ8ucF2OsjjZBFozXuUAbseyZCxoKkMTjFqT5ymILPaLrA==;EndpointSuffix=core.windows.net");

        //    CloudBlobClient BlobClient = storageAccount.CreateCloudBlobClient();
        //    CloudBlobContainer c1 = BlobClient.GetContainerReference("test");

        //    if (c1.Exists())
        //    {
        //        CloudBlob file = c1.GetBlobReference(fileName);

        //        return file.Uri.ToString();

        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}


        // POST: R11/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Curso/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ViewModelR11 viewModelR11)
        {

            R11 r11 = viewModelR11.r11Entity;
            r11.idCurso = viewModelR11.curso[0].idCurso;
            r11.nombreCurso = db.Curso.Where(x => x.idCurso == r11.idCurso).First().nombreCurso;
            r11.idCategoria = Convert.ToInt32(viewModelR11.categoriaR11.First().categoria);
            r11.fechaCreacion = DateTime.Now;
            r11.relator = db.Relators.Find(viewModelR11.r11Entity.relator.idRelator);
            r11.softDelete = false;

            ValidarContenidoEspecifico(viewModelR11.contenidoEspecifico);

            if (ModelState.IsValid && r11.relator != null)
            {
                db.R11.Add(r11);
                db.SaveChanges();
                foreach (var itemEscolaridad in viewModelR11.escolaridadR11.ToList())
                {

                    itemEscolaridad.idR11 = r11.idR11;
                    db.EscolaridadR11.Add(itemEscolaridad);
                    db.SaveChanges();

                }
                int contador = 0;
                foreach (var itemContenidoEspecifico in viewModelR11.contenidoEspecifico.ToList())
                {
                    itemContenidoEspecifico.idR11 = r11.idR11;
                    db.ContenidoEspecificoR11.Add(itemContenidoEspecifico);
                    db.SaveChanges();


                    string[] listItemBreakLine = viewModelR11.itemContenidoEspecificoR11[contador].contenidoEspecifico.Split(
                                new[] { "\r\n", "\r", "\n" },
                                StringSplitOptions.None
                            );
                    foreach (string itemBreakLine in listItemBreakLine)
                    {
                        ItemContenidoEspecificoR11 nuevoItem = new ItemContenidoEspecificoR11();
                        nuevoItem.idContenidoEspecificoR11 = itemContenidoEspecifico.idContenidoEspecificoR11;
                        nuevoItem.contenidoEspecifico = itemBreakLine;

                        if (nuevoItem.contenidoEspecifico != "" && nuevoItem.contenidoEspecifico != null)
                        {
                            db.ItemContenidoEspecificoR11.Add(nuevoItem);
                            db.SaveChanges();
                        }
                    }
                    contador++;

                }
                return RedirectToAction("Index", "Curso");
            }
            viewModelR11.curso = db.Curso.Where(x => x.idCurso == viewModelR11.r11Entity.idCurso).ToList();
            viewModelR11.categoriaR11 = db.CategoriaR11.Where(x => x.softDelete == false).Where(x => x.vigencia == 1).ToList();
            ViewBag.relatores = GetRelatores();
            return View(viewModelR11);
        }

        // GET: R11/Edit/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Curso/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            R11 r11 = db.R11.Find(id);
            if (r11 == null)
            {
                return HttpNotFound();
            }
            ViewModelR11 viewModel = new ViewModelR11();

            viewModel.curso = db.Curso.Where(x => x.idCurso == r11.idCurso).ToList();
            viewModel.categoriaR11 = db.CategoriaR11.Where(x => x.softDelete == false).Where(x => x.vigencia == 1).ToList();
            viewModel.r11Entity = r11;
            viewModel.escolaridadR11 = db.EscolaridadR11.Where(x => x.idR11 == r11.idR11).ToList();
            viewModel.contenidoEspecifico = db.ContenidoEspecificoR11.Where(x => x.idR11 == r11.idR11).ToList();
            var listIdContenidoEspecifico = db.ContenidoEspecificoR11.Where(x => x.idR11 == r11.idR11).Select(o => o.idContenidoEspecificoR11).ToList();
            viewModel.itemContenidoEspecificoR11 = db.ItemContenidoEspecificoR11.Where(x => listIdContenidoEspecifico.Contains(x.idContenidoEspecificoR11)).ToList();

            ViewBag.relatores = GetRelatores();
            return View(viewModel);
        }

        // POST: R11/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Curso/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ViewModelR11 viewModelR11)
        {
            R11 r11 = db.R11.Find(viewModelR11.r11Entity.idR11);
            r11.cantPersona = viewModelR11.r11Entity.cantPersona;
            r11.codigoSence = viewModelR11.r11Entity.codigoSence;
            r11.diplomaAprobacion = viewModelR11.r11Entity.diplomaAprobacion;
            r11.DiplomaParticipacion = viewModelR11.r11Entity.DiplomaParticipacion;
            r11.fechaCaducidad = viewModelR11.r11Entity.fechaCaducidad;
            r11.fundamentacionTecnica = viewModelR11.r11Entity.fundamentacionTecnica;
            r11.horasPracticas = viewModelR11.r11Entity.horasPracticas;
            r11.horasTeoricas = viewModelR11.r11Entity.horasTeoricas;
            r11.idCategoria = viewModelR11.r11Entity.idCategoria;
            r11.materialDidactico = viewModelR11.r11Entity.materialDidactico;
            r11.materialEntregable = viewModelR11.r11Entity.materialEntregable;
            r11.mesesDuracionVigencia = viewModelR11.r11Entity.mesesDuracionVigencia;
            r11.objetivoGeneral = viewModelR11.r11Entity.objetivoGeneral;
            r11.poblacionObjetivo = viewModelR11.r11Entity.poblacionObjetivo;
            r11.requisitosIngreso = viewModelR11.r11Entity.requisitosIngreso;
            r11.requisitosReglamentarios = viewModelR11.r11Entity.requisitosReglamentarios;
            r11.requisitosTecnicos = viewModelR11.r11Entity.requisitosTecnicos;
            r11.requisitosTecnicosRelatores = viewModelR11.r11Entity.requisitosTecnicosRelatores;
            r11.tecnicaMetodologica = viewModelR11.r11Entity.tecnicaMetodologica;
            //r11.fechaCreacion = viewModelR11.r11Entity.fechaCreacion;

            r11.idCurso = viewModelR11.curso[0].idCurso;

            r11.nombreCurso = db.Curso.Where(x => x.idCurso == r11.idCurso).First().nombreCurso;
            r11.relator = db.Relators.Find(viewModelR11.r11Entity.relator.idRelator);

            ValidarContenidoEspecifico(viewModelR11.contenidoEspecifico);

            if (ModelState.IsValid)
            {
                db.Entry(r11).State = EntityState.Modified;
                db.SaveChanges();


                foreach (var itemEscolaridad in viewModelR11.escolaridadR11.ToList())
                {
                    itemEscolaridad.idR11 = r11.idR11;
                    db.Entry(itemEscolaridad).State = EntityState.Modified;
                    db.SaveChanges();

                }
                var eliminarConteEspecificoR11 = db.ContenidoEspecificoR11.Where(x => x.idR11 == r11.idR11).ToList();
                db.ContenidoEspecificoR11.RemoveRange(eliminarConteEspecificoR11.AsEnumerable());
                db.SaveChanges();
                int contador = 0;
                foreach (var itemContenidoEspecifico in viewModelR11.contenidoEspecifico.ToList())
                {
                    itemContenidoEspecifico.idR11 = r11.idR11;
                    db.ContenidoEspecificoR11.Add(itemContenidoEspecifico);
                    db.SaveChanges();

                    var eliminarItemConteEspecificoR11 = db.ItemContenidoEspecificoR11.Where(x => x.idContenidoEspecificoR11 == itemContenidoEspecifico.idContenidoEspecificoR11).ToList();
                    db.ItemContenidoEspecificoR11.RemoveRange(eliminarItemConteEspecificoR11.AsEnumerable());
                    db.SaveChanges();

                    string[] listItemBreakLine = viewModelR11.itemContenidoEspecificoR11[contador].contenidoEspecifico.Split(
                                new[] { "\r\n", "\r", "\n" },
                                StringSplitOptions.None
                            );
                    foreach (string itemBreakLine in listItemBreakLine)
                    {
                        ItemContenidoEspecificoR11 nuevoItem = new ItemContenidoEspecificoR11();
                        nuevoItem.idContenidoEspecificoR11 = itemContenidoEspecifico.idContenidoEspecificoR11;
                        nuevoItem.contenidoEspecifico = itemBreakLine;

                        if (nuevoItem.contenidoEspecifico != "" && nuevoItem.contenidoEspecifico != null)
                        {
                            db.ItemContenidoEspecificoR11.Add(nuevoItem);
                            db.SaveChanges();
                        }
                    }
                    contador++;

                }
                return RedirectToAction("Index", "Curso");
            }
            viewModelR11.curso = db.Curso.Where(x => x.idCurso == r11.idCurso).ToList();
            viewModelR11.categoriaR11 = db.CategoriaR11.Where(x => x.softDelete == false).Where(x => x.vigencia == 1).ToList();
            ViewBag.relatores = GetRelatores();
            return View(viewModelR11);
        }

        private void ValidarContenidoEspecifico(IList<ContenidoEspecificoR11> contenidoEspecifico)
        {
            if (contenidoEspecifico == null)
            {
                ModelState.AddModelError("contenidoEspecifico", "Se debe ingresar un Contenido Especifico.");
            }
            else
            {
                if (contenidoEspecifico.Count() <= 0)
                {
                    ModelState.AddModelError("contenidoEspecifico", "Se debe ingresar un Contenido Especifico.");
                }
            }
        }

        //// GET: R11/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    R11 r11 = db.R11.Find(id);
        //    if (r11 == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(r11);
        //}

        //// POST: R11/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    R11 r11 = db.R11.Find(id);
        //    //r11.softDelete = true;
        //    //db.Entry(r11).State = EntityState.Modified;
        //    db.R11.Remove(r11);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        public object Data(R11 r11, string firma)
        {
            var contenidosEspecificos = new List<object>();
            var i = 1;
            foreach (var item in r11.conteidoEspecifico)
            {
                var descripciones = new List<object>();
                foreach (var descripcion in item.itemConteidoEspecificoR11)
                {
                    descripciones.Add(new
                    {
                        descripcion.contenidoEspecifico
                    });
                }
                contenidosEspecificos.Add(new
                {
                    n = i,
                    contenido = item.nombre,
                    descripciones,
                    hp = String.Format("{0:N1}", item.horasP),
                    ht = String.Format("{0:N1}", item.horasT)
                });
                i++;
            }
            var escolaridad = new List<object>();
            foreach (var item in r11.escolaridadR11)
            {
                if (item.marca)
                {
                    escolaridad.Add(new
                    {
                        sino = "X"
                    });
                }
                else
                {
                    escolaridad.Add(new
                    {
                        sino = ""
                    });
                }
            }
            r11.codigoSence = r11.codigoSence == null ? "Por definir" : r11.codigoSence;
            var data = new
            {
                r11.nombreCurso,
                r11.fundamentacionTecnica,
                r11.objetivoGeneral,
                cantidadPersonas = r11.cantPersona,
                r11.poblacionObjetivo,
                r11.requisitosIngreso,
                tecnicasMetodologicas = r11.tecnicaMetodologica,
                r11.materialDidactico,
                r11.materialEntregable,
                requisitosLegalesNormativosReglamentarios = r11.requisitosReglamentarios,
                //r11.nombreModulo,
                instructor = r11.relator.contacto.nombreCompleto,
                runInstructor = r11.relator.contacto.run,
                //r11.instructor,
                r11.requisitosTecnicos,
                requisitosTecnicosRelatoreConocimientosSala = r11.requisitosTecnicosRelatores,
                fechaCreacion = r11.fechaCreacion.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                tht = String.Format("{0:N1}", r11.horasTeoricas),
                thp = String.Format("{0:N1}", r11.horasPracticas),
                hto = String.Format("{0:N1}", r11.horasPracticas + r11.horasTeoricas),
                r11.mesesDuracionVigencia,
                fechaCaducidad = r11.fechaCaducidad.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                r11.codigoSence,
                r11.diplomaAprobacion,
                diplomaParticipacion = r11.DiplomaParticipacion,
                ce = contenidosEspecificos,
                escolaridad,
                firma
            };
            return data;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Curso/" })]
        public ActionResult Reporte(int? id)
        {
            var r11 = db.R11.Find(id);
            var curso = db.Curso.Find(r11.idCurso);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r11")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                ViewModelR11 viewModel = new ViewModelR11();
                viewModel.curso = db.Curso.Where(x => x.idCurso == r11.idCurso).ToList();
                viewModel.categoriaR11 = db.CategoriaR11.Where(x => x.vigencia == 1).ToList();
                viewModel.r11Entity = r11;
                viewModel.escolaridadR11 = db.EscolaridadR11.Where(x => x.idR11 == r11.idR11).ToList();
                viewModel.contenidoEspecifico = db.ContenidoEspecificoR11.Where(x => x.idR11 == r11.idR11).ToList();
                var listIdContenidoEspecifico = db.ContenidoEspecificoR11.Where(x => x.idR11 == r11.idR11).Select(o => o.idContenidoEspecificoR11).ToList();
                viewModel.itemContenidoEspecificoR11 = db.ItemContenidoEspecificoR11.Where(x => listIdContenidoEspecifico.Contains(x.idContenidoEspecificoR11)).ToList();
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r11\" y tipo \"word\".");
                return View("Details", viewModel);
            }
            if (r11.relator.imagenFirma == null)
            {
                ViewModelR11 viewModel = new ViewModelR11();
                viewModel.curso = db.Curso.Where(x => x.idCurso == r11.idCurso).ToList();
                viewModel.categoriaR11 = db.CategoriaR11.Where(x => x.vigencia == 1).ToList();
                viewModel.r11Entity = r11;
                viewModel.escolaridadR11 = db.EscolaridadR11.Where(x => x.idR11 == r11.idR11).ToList();
                viewModel.contenidoEspecifico = db.ContenidoEspecificoR11.Where(x => x.idR11 == r11.idR11).ToList();
                var listIdContenidoEspecifico = db.ContenidoEspecificoR11.Where(x => x.idR11 == r11.idR11).Select(o => o.idContenidoEspecificoR11).ToList();
                viewModel.itemContenidoEspecificoR11 = db.ItemContenidoEspecificoR11.Where(x => listIdContenidoEspecifico.Contains(x.idContenidoEspecificoR11)).ToList();
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la firma del Instructor.");
                return View("Details", viewModel);
            }
            return RedirectToAction("GenerarReporte", new { id });
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Curso/" })]
        [EnableJsReport()]
        public async Task<ActionResult> GenerarReporte(int? id)
        {
            var r11 = db.R11.Find(id);
            var curso = db.Curso.Find(r11.idCurso);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r11")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);

            var firma = await Files.BajarArchivoBytesAsync(r11.relator.imagenFirma);
            var firmaBase64 = Convert.ToBase64String(firma, 0, firma.Length);

            var elerning = "";
            if (curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono || curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono)
            {
                elerning = " (E)";
            }
            var nombreArchivo = "R11 V3" + elerning + " - " + curso.nombreCurso + " - " + (r11.horasPracticas + r11.horasTeoricas) + " HORAS";

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
                .Configure((r) => r.Data = Data(r11, "data:image/png;base64," + firmaBase64))
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"" + nombreArchivo + ".docx\"");
            return null;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Curso/" })]
        public ActionResult ReportePdf(int? id)
        {
            var r11 = db.R11.Find(id);
            if (r11 == null)
            {
                return HttpNotFound();
            }
            var curso = db.Curso.Find(r11.idCurso);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r11")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                ViewModelR11 viewModel = new ViewModelR11();
                viewModel.curso = db.Curso.Where(x => x.idCurso == r11.idCurso).ToList();
                viewModel.categoriaR11 = db.CategoriaR11.Where(x => x.vigencia == 1).ToList();
                viewModel.r11Entity = r11;
                viewModel.escolaridadR11 = db.EscolaridadR11.Where(x => x.idR11 == r11.idR11).ToList();
                viewModel.contenidoEspecifico = db.ContenidoEspecificoR11.Where(x => x.idR11 == r11.idR11).ToList();
                var listIdContenidoEspecifico = db.ContenidoEspecificoR11.Where(x => x.idR11 == r11.idR11).Select(o => o.idContenidoEspecificoR11).ToList();
                viewModel.itemContenidoEspecificoR11 = db.ItemContenidoEspecificoR11.Where(x => listIdContenidoEspecifico.Contains(x.idContenidoEspecificoR11)).ToList();
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r11\" y tipo \"word\".");
                return View("Details", viewModel);
            }
            if (r11.relator.imagenFirma == null)
            {
                ViewModelR11 viewModel = new ViewModelR11();
                viewModel.curso = db.Curso.Where(x => x.idCurso == r11.idCurso).ToList();
                viewModel.categoriaR11 = db.CategoriaR11.Where(x => x.vigencia == 1).ToList();
                viewModel.r11Entity = r11;
                viewModel.escolaridadR11 = db.EscolaridadR11.Where(x => x.idR11 == r11.idR11).ToList();
                viewModel.contenidoEspecifico = db.ContenidoEspecificoR11.Where(x => x.idR11 == r11.idR11).ToList();
                var listIdContenidoEspecifico = db.ContenidoEspecificoR11.Where(x => x.idR11 == r11.idR11).Select(o => o.idContenidoEspecificoR11).ToList();
                viewModel.itemContenidoEspecificoR11 = db.ItemContenidoEspecificoR11.Where(x => listIdContenidoEspecifico.Contains(x.idContenidoEspecificoR11)).ToList();
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro la firma del Instructor.");
                return View("Details", viewModel);
            }

            string hash = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                hash = Utils.Utils.GetHash(sha256Hash, DateTime.Now.ToString());
            }

            string createRequest = Url.Action("GenerarReportePdfR11", "R11", new { id, id2 = hash }, Request.Url.Scheme);
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
                ViewModelR11 viewModel = new ViewModelR11();
                viewModel.curso = db.Curso.Where(x => x.idCurso == r11.idCurso).ToList();
                viewModel.categoriaR11 = db.CategoriaR11.Where(x => x.vigencia == 1).ToList();
                viewModel.r11Entity = r11;
                viewModel.escolaridadR11 = db.EscolaridadR11.Where(x => x.idR11 == r11.idR11).ToList();
                viewModel.contenidoEspecifico = db.ContenidoEspecificoR11.Where(x => x.idR11 == r11.idR11).ToList();
                var listIdContenidoEspecifico = db.ContenidoEspecificoR11.Where(x => x.idR11 == r11.idR11).Select(o => o.idContenidoEspecificoR11).ToList();
                viewModel.itemContenidoEspecificoR11 = db.ItemContenidoEspecificoR11.Where(x => listIdContenidoEspecifico.Contains(x.idContenidoEspecificoR11)).ToList();
                // indicar q hubo un error
                ModelState.AddModelError("", "No se pudo generar el documento.");
                return View("Details", viewModel);
            }

            var path = directory + hash;
            Byte[] bytes = System.IO.File.ReadAllBytes(path + ".pdf");

            System.IO.File.Delete(path + ".pdf");

            Response.ContentType = "application/pdf";

            var elerning = "";
            if (curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono || curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono)
            {
                elerning = " (E)";
            }
            var nombreArchivo = "\"R11 V3" + elerning + " - " + curso.nombreCurso + " - " + (r11.horasPracticas + r11.horasTeoricas) + " HORAS.pdf\"";

            Response.AppendHeader("Content-Disposition", "attachment; filename=" + nombreArchivo);

            return new FileContentResult(bytes, "application/pdf");
        }

        [EnableJsReport()]
        public async Task<ActionResult> GenerarReportePdfR11(int? id, string id2)
        {
            var r11 = db.R11.Find(id);
            var curso = db.Curso.Find(r11.idCurso);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r11")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);

            var firma = await Files.BajarArchivoBytesAsync(r11.relator.imagenFirma);
            var firmaBase64 = Convert.ToBase64String(firma, 0, firma.Length);

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
                .Configure((r) => r.Data = Data(r11, "data:image/png;base64," + firmaBase64))
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

        public SelectList GetRelatores()
        {
            return new SelectList(db.Relators
                .Where(r => r.softDelete == false)
                .ToList()
                .Select(r => new SelectListItem
                {
                    Text = "[" + r.contacto.run + "]" + " " + r.contacto.nombres + " " + r.contacto.apellidoPaterno + " " + r.contacto.apellidoMaterno,
                    Value = r.idRelator.ToString()
                }).ToList(), "Value", "Text");
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
