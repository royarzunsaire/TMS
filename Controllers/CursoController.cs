using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using SGC.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class CursoController : Controller
    {
        private InsecapContext db = new InsecapContext();

      
        [Authorize]
        [CustomAuthorize(new string[] { "/Curso/" })]
        [HttpPost]
        // GET: Comercializacions
        public ActionResult IndexData()
        {

            int start = Convert.ToInt32(Request["start"]);
            int draw = Convert.ToInt32(Request["draw"]);
            String search = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];
            int recordsTotal = 0;
            int count = Convert.ToInt32(Request["length"]);
            bool r11Filter = Convert.ToBoolean(Request["r11"]);
            bool noR11Filter = Convert.ToBoolean(Request["noR11"]);
            bool r12Filter = Convert.ToBoolean(Request["r12"]);
            bool noR12Filter = Convert.ToBoolean(Request["noR12"]);

            var dataDb = db.Curso
               .OrderByDescending(x => x.idCurso)
              .Where(x => x.softDelete == false)
              ;
           



            string idUser = User.Identity.GetUserId();
            DateTime dateSearch = DateTime.MinValue;
            DateTime.TryParse(search, out dateSearch);

            if (string.IsNullOrEmpty(search) && !r11Filter && !noR11Filter && !r12Filter && !noR12Filter)
            {
                recordsTotal = dataDb.Count();

            }
            else
            {
                dataDb = dataDb.Where(x => x.codigoCurso.ToLower().Contains(search)
            || x.nombreCurso.ToLower().Contains(search)
            || x.tipoEjecucion.ToString().Contains(search));

                if (r11Filter || noR11Filter || r12Filter || noR12Filter) {
                    List<int> r11Filters = db.R11.Select(x => x.idCurso).ToList();
                    List<int> r12Filters = db.CostoCursoR12.Select(x => x.idCurso).ToList();
                    
                    
                    if (r12Filter || noR12Filter)
                    {
                        if (r12Filter || noR12Filter)
                        {
                            if(r12Filter && !noR12Filter)
                                dataDb = dataDb.Where(x => r12Filters.Any(y => y == x.idCurso) == true);
                            if (!r12Filter && noR12Filter)
                                dataDb = dataDb.Where(x => r12Filters.Any(y => y == x.idCurso) == false);

                        }
                     
                    }
                    if (r11Filter || noR11Filter)
                    {
                        if (r11Filter || noR11Filter)
                        {
                            if (r11Filter && !noR11Filter)
                                dataDb = dataDb.Where(x => r11Filters.Any(y => y == x.idCurso) == true);
                            if (!r11Filter && noR11Filter)
                                dataDb = dataDb.Where(x => r11Filters.Any(y => y == x.idCurso) == false);

                        }

                    }

                }

                recordsTotal = dataDb.Count();
            }

            if (count == -1)
            {
                count = recordsTotal;
            }
            var data = dataDb
                .Skip(start)
                .Take(count)
                .ToList();


            List<object> resultset = new List<object>();
            List<int> idCursos = data.Select(x => x.idCurso).ToList();
            List<R11> r11s = db.R11.Where(x => idCursos.Any(y => y == x.idCurso)).ToList();
            List<CostoCursoR12> r12s = db.CostoCursoR12.Where(x => idCursos.Any(y => y == x.idCurso)).ToList();
            foreach (Curso curso in data)
            {
                
                var r11 = r11s.FirstOrDefault(x => x.idCurso == curso.idCurso);
                var r12 = r12s.FirstOrDefault(x => x.idCurso == curso.idCurso);
                var tipoEjecucion = curso.tipoEjecucion.ToString();
                tipoEjecucion = tipoEjecucion.Replace("Recertificacion_Asincronica", "R-Asincronica");
                tipoEjecucion = tipoEjecucion.Replace("Elearning_Asincrono", "E-Asincrono");
                tipoEjecucion = tipoEjecucion.Replace("Elearning_Sincrono", "E-Sincrono");
                tipoEjecucion = tipoEjecucion.Replace("Recertificacion_Sincronica", "R-Sincronica");

                string horas = "0";
               
                if(r11 != null){
                    horas = string.Format("{0} horas (P:{1},T:{2})", r11.horasPracticas + r11.horasTeoricas, r11.horasPracticas, r11.horasTeoricas);

                  }
                ViewBag.r11 = r11;
                ViewBag.r12 = r12;
                resultset.Add(
                    new
                    {
                        codigoCurso =  curso.codigoCurso ,
                        curso.nombreCurso,
                        tipoEjecucion,
                        horas,
                        curso.idCursoMoodle,
                        menu = ConvertPartialViewToString(PartialView("IndexMenu", curso)),
                    }
                    );



            }


            var jsonResult = Json(new { draw, recordsTotal, recordsFiltered = recordsTotal, data = resultset }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public string ConvertPartialViewToString(PartialViewResult partialView)
        {
            using (var sw = new StringWriter())
            {
                partialView.View = ViewEngines.Engines
                  .FindPartialView(ControllerContext, partialView.ViewName).View;

                var vc = new ViewContext(
                  ControllerContext, partialView.View, partialView.ViewData, partialView.TempData, sw);
                partialView.View.Render(vc, sw);

                var partialViewString = sw.GetStringBuilder().ToString();

                return partialViewString;
            }
        }

        [CustomAuthorize(new string[] { "/Curso/" })]
        public ActionResult Index()
        {
           
            return View();
        }
        // GET: Curso/AdjuntarMaterial/5
        [CustomAuthorize(new string[] { "/Curso/" })]
        public ActionResult AdjuntarMaterial(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Curso curso = db.Curso.Find(id);
            if (curso == null)
            {
                return HttpNotFound();
            }
            ViewBag.r11 = db.R11.Where(x => x.idCurso == id).FirstOrDefault();
            ViewBag.r12 = db.CostoCursoR12.Where(x => x.idCurso == id).FirstOrDefault();
            return View(curso);
        }

        // POST: Curso/AdjuntarMaterial/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Curso/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdjuntarMaterial([Bind(Include = "idCurso")] Curso curso)
        {
            Curso cursoBD = db.Curso.Find(curso.idCurso);
            // guardar archivos
            HttpPostedFileBase file = Request.Files["file"];
            // validar extenciones y tamaño maximo de los archivos
            if (file.ContentLength > 0)
            {
                var archivoValido = Files.ArchivoValido(file, new[] { ".pdf" }, 3 * 1024);
                if (archivoValido != "")
                {
                    ModelState.AddModelError("", archivoValido);
                }
                else
                {
                    var archivo = await Files.CrearArchivoAsync(file, "curso/material/");
                    if (archivo == null)
                    {
                        ModelState.AddModelError("", "No se pudo guardar el archivo seleccionado.");
                    }
                    else
                    {
                        MaterialCurso materialCurso = new MaterialCurso();
                        materialCurso.archivo = archivo;
                        cursoBD.materialCurso.Add(materialCurso);
                        db.Entry(cursoBD).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("AdjuntarMaterial", new { id = curso.idCurso });
                    }
                }
            }
            ViewBag.r11 = db.R11.Where(x => x.idCurso == curso.idCurso).FirstOrDefault();
            ViewBag.r12 = db.CostoCursoR12.Where(x => x.idCurso == curso.idCurso).FirstOrDefault();
            return View(cursoBD);
        }

        // POST: Curso/AgregarUrlMaterial/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Curso/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarUrlMaterial([Bind(Include = "idCurso")] Curso curso, string url, string descripcion)
        {
            Curso cursoBD = db.Curso.Find(curso.idCurso);

            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!result)
            {
                ModelState.AddModelError("", "Se debe ingresar una URL válida.");
                ModelState.AddModelError("url", "Se debe ingresar una URL válida.");
            }

            if (descripcion.Length >= 250)
            {
                ModelState.AddModelError("", "La descripción de la URL puede tener máximo 250 caracteres.");
                ModelState.AddModelError("descripcion", "La descripción de la URL puede tener máximo 250 caracteres.");
                result = false;
            }

            if (url.Length >= 999)
            {
                ModelState.AddModelError("", "La URL puede tener máximo 999 caracteres.");
                ModelState.AddModelError("url", "La URL puede tener máximo 999 caracteres.");
                result = false;
            }

            if (result)
            {
                var materialCurso = new UrlMaterialCurso();
                materialCurso.url = url;
                materialCurso.descripcion = descripcion;
                cursoBD.urlMaterialCurso.Add(materialCurso);
                db.Entry(cursoBD).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("AdjuntarMaterial", new { id = curso.idCurso });
            }

            ViewBag.r11 = db.R11.Where(x => x.idCurso == curso.idCurso).FirstOrDefault();
            ViewBag.r12 = db.CostoCursoR12.Where(x => x.idCurso == curso.idCurso).FirstOrDefault();
            return View("AdjuntarMaterial", cursoBD);
        }

        // GET: Curso/AgregarCursoMoodle/5
        [CustomAuthorize(new string[] { "/Curso/" })]
        public ActionResult AgregarCursoMoodle(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Curso curso = db.Curso.Find(id);
            if (curso == null)
            {
                return HttpNotFound();
            }

            var r11 = db.R11.Where(r => r.idCurso == curso.idCurso).FirstOrDefault();
            var r51 = db.R51.Where(r => r.idCurso == curso.idCurso).FirstOrDefault();
            var categoria = db.CategoriaR11.Where(c => c.idCategoria == r11.idCategoria).FirstOrDefault();
            curso.idCursoMoodle = Moodle.CrearCursoMoodle(curso, r51, categoria, db.ParametrosMoodles.FirstOrDefault());
            var number = 0;
            if (!Int32.TryParse(curso.idCursoMoodle, out number))
            {
                ModelState.AddModelError("", curso.idCursoMoodle);
                curso.idCursoMoodle = null;
            }
            else
            {
                db.Entry(curso).State = EntityState.Modified;
                db.SaveChanges();
            }

            curso.idCursoMoodle = Moodle.EditarCursoMoodle(curso, r51, categoria, db.ParametrosMoodles.FirstOrDefault(),"Crear");
            db.Entry(curso).State = EntityState.Modified;
            db.SaveChanges();

            var cursos = db.Curso.Where(c => c.softDelete == false).ToList();
            var lista = new List<ViewModelCurso>();
            foreach (var item in cursos)
            {
                var cursoVM = new ViewModelCurso();
                cursoVM.curso = item;
                cursoVM.r11 = db.R11.Where(r => r.idCurso == item.idCurso).FirstOrDefault();
                cursoVM.r12 = db.CostoCursoR12.Where(r => r.idCurso == item.idCurso).FirstOrDefault();
                lista.Add(cursoVM);
            }
            return View("Index", lista);
        }

        // GET: Curso/AgregarCursoMoodle/5
        [CustomAuthorize(new string[] { "/Curso/" })]
        public ActionResult ActualizarCursoMoodle(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Curso curso = db.Curso.Find(id);
            if (curso == null)
            {
                return HttpNotFound();
            }

            var r11 = db.R11.Where(r => r.idCurso == curso.idCurso).FirstOrDefault();
            var r51 = db.R51.Where(r => r.idCurso == curso.idCurso).FirstOrDefault();
            var categoria = db.CategoriaR11.Where(c => c.idCategoria == r11.idCategoria).FirstOrDefault();

            var resultado = Moodle.EditarCursoMoodle(curso, r51, categoria, db.ParametrosMoodles.FirstOrDefault(), "Actualizar");
            if (resultado != "")
            {
                if (resultado.Length <= 5) {
                    curso.idCursoMoodle = resultado;
                    db.Entry(curso).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    ModelState.AddModelError("", resultado);
                }

            }

            var cursos = db.Curso.Where(c => c.softDelete == false).ToList();
            var lista = new List<ViewModelCurso>();
            foreach (var item in cursos)
            {
                var cursoVM = new ViewModelCurso();
                cursoVM.curso = item;
                cursoVM.r11 = db.R11.Where(r => r.idCurso == item.idCurso).FirstOrDefault();
                cursoVM.r12 = db.CostoCursoR12.Where(r => r.idCurso == item.idCurso).FirstOrDefault();
                lista.Add(cursoVM);
            }
            return View("Index", lista);
        }

        // POST: Curso/EliminarMaterial/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Curso/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EliminarMaterial(int idCurso)
        {
            int idMaterialCurso = int.Parse(Request["item.idMaterialCurso"]);
            var materialCurso = db.MaterialCurso.Find(idMaterialCurso);
            var curso = db.Curso.Find(idCurso);
            await Files.BorrarArchivoAsync(materialCurso.archivo);
            db.Storages.Remove(materialCurso.archivo);
            curso.materialCurso.Remove(materialCurso);
            db.MaterialCurso.Remove(materialCurso);
            db.SaveChanges();
            return RedirectToAction("AdjuntarMaterial", new { id = idCurso });
        }

        // POST: Curso/EliminarUrlMaterial/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Curso/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarUrlMaterial(int idCurso, int idUrlMaterialCurso)
        {
            //int idMaterialCurso = int.Parse(Request["item.idMaterialCurso"]);
            var urlMaterialCurso = db.UrlMaterialCurso.Find(idUrlMaterialCurso);
            var curso = db.Curso.Find(idCurso);
            //curso.materialCurso.Remove(urlMaterialCurso);
            db.UrlMaterialCurso.Remove(urlMaterialCurso);
            db.SaveChanges();
            return RedirectToAction("AdjuntarMaterial", new { id = idCurso });
        }

        // POST: Curso/MaterialCompleto/5
        [CustomAuthorize(new string[] { "/Curso/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MaterialCompleto(int idCurso)
        {
            Curso curso = db.Curso.Find(idCurso);
            curso.materialCompleto = true;
            curso.fechaValidacionMaterial = DateTime.Now;
            curso.usuarioValidacionMaterial = db.AspNetUsers.Find(User.Identity.GetUserId());
            db.Entry(curso).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("AdjuntarMaterial", new { id = idCurso });
        }

        //// GET: Curso/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Curso curso = db.Curso.Find(id);
        //    if (curso == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(curso);
        //}

        //// GET: Curso/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Curso/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "idCurso,codigoCurso,nombreCurso,softDelete")] Curso curso)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Curso.Add(curso);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(curso);
        //}

        //// GET: Curso/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Curso curso = db.Curso.Find(id);
        //    if (curso == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(curso);
        //}

        //// POST: Curso/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "idCurso,codigoCurso,nombreCurso,softDelete")] Curso curso)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(curso).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(curso);
        //}

        //// GET: Curso/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Curso curso = db.Curso.Find(id);
        //    if (curso == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(curso);
        //}

        // POST: Curso/Delete/5
        [CustomAuthorize(new string[] { "/Curso/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Curso curso = db.Curso.Find(id);
            curso.softDelete = true;
            db.Entry(curso).State = EntityState.Modified;
            R51 r51 = db.R51.Where(r => r.idCurso == id).FirstOrDefault();
            r51.softDelete = true;
            db.Entry(r51).State = EntityState.Modified;
            R11 r11 = db.R11.Where(r => r.idCurso == id).FirstOrDefault();
            if (r11 != null)
            {
                r11.softDelete = true;
                db.Entry(r11).State = EntityState.Modified;
            }
            CostoCursoR12 r12 = db.CostoCursoR12.Where(r => r.idCurso == id).FirstOrDefault();
            if (r12 != null)
            {
                r12.softDelete = true;
                db.Entry(r12).State = EntityState.Modified;
            }

            //if (curso.idCursoMoodle != null)
            //{
            //    Moodle.EliminarCursoMoodle(curso, db.ParametrosMoodles.FirstOrDefault());
            //}

            //db.Curso.Remove(curso);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Curso/Descargar/5
        [CustomAuthorize(new string[] { "/Curso/" })]
        public async Task<ActionResult> Descargar(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var storage = db.Storages.Find(id);
            if (storage == null)
            {
                return HttpNotFound();
            }
            return await Files.BajarArchivoDescargarAsync(storage);
        }

        // POST: Curso/EliminarMaterial/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Curso/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarIdMoodle(int idCurso, string idMoodle)
        {
            var curso = db.Curso.Find(idCurso);
            var existeCurso = Moodle.GetIdCursoMoodle(new Curso { idCursoMoodle = idMoodle }, db.ParametrosMoodles.FirstOrDefault());
            if (existeCurso != null)
            {
                curso.idCursoMoodle = idMoodle;
                db.Entry(curso).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "No se pudo encontrar el curso en Moodle.");
            var cursos = GetCursos();
            return View("Index", cursos);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private List<ViewModelCurso> GetCursos()
        {
            var cursos = db.Curso.Where(c => c.softDelete == false).ToList();
            var lista = new List<ViewModelCurso>();
            foreach (var item in cursos)
            {
                var cursoVM = new ViewModelCurso();
                cursoVM.curso = item;
                cursoVM.r11 = db.R11.Where(r => r.idCurso == item.idCurso).FirstOrDefault();
                var r12 = db.CostoCursoR12.Where(r => r.idCurso == item.idCurso).FirstOrDefault();
                if (r12 != null)
                {
                    r12.costoParticularCurso = db.CostoParticularCurso.Where(x => x.idCostoCursoR12 == r12.idCostoCursoR12).ToList();
                }
                cursoVM.r12 = r12;
                lista.Add(cursoVM);
            }
            return lista;
        }
    }
}
