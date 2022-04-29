using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    [CustomAuthorize(new string[] { "/Curso/" })]
    public class CostoParticularCursoController : Controller
    {
        private InsecapContext db = new InsecapContext();

        //// GET: CostoParticularCurso
        //public ActionResult Index()
        //{
        //    /*var CostoParticularCurso =   db.CostoParticularCurso.GroupBy(g => g.idCurso).ToList();
        //    ViewModelCostoParticularCurso cursos = new ViewModelCostoParticularCurso();

        //    foreach (var item in CostoParticularCurso)
        //    {
        //        foreach (CostoParticularCurso subItem in item)
        //        {
        //            List<CostoParticularCurso> costo = new List<CostoParticularCurso>();
        //            costo.Add(subItem);
        //            cursos.costoParticularCursosMateriales = costo;
        //            break;
        //        }

        //    }*/
        //    return View(db.CostoCursoR12.Where(cc => cc.softDelete == false).ToList());
        //}

        // GET: CostoParticularCurso/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewModelCostoParticularCurso myModel = new ViewModelCostoParticularCurso();
            myModel.costoParticularCursosManoDeObra = db.CostoParticularCurso
                .Where(x => x.idCostoCursoR12 == id && x.categoria == "Mano de Obra")
                .ToList();
            myModel.costoParticularCursosEquiposYHerramientas = db.CostoParticularCurso
                .Where(x => x.idCostoCursoR12 == id && x.categoria == "Equipos y Herramientas")
                .ToList();
            myModel.costoParticularCursosMateriales = db.CostoParticularCurso
                .Where(x => x.idCostoCursoR12 == id && x.categoria == "Materiales")
                .ToList();
            myModel.costoParticularCursosOtrosGastos = db.CostoParticularCurso
                .Where(x => x.idCostoCursoR12 == id && x.categoria == "Otros Gastos")
                .ToList();
            myModel.listaCostoParticularCursos = db.ListaCostoParticularCurso.ToList();
            myModel.idCostoCursoR12 = (int)id;
            CostoCursoR12 costoCursoR12 = db.CostoCursoR12.Find(id);
            ViewBag.nombreCurso = costoCursoR12.curso.nombreCurso;
            return View(myModel);
        }

        // GET: CostoParticularCurso/R12Curso/5
        public ActionResult R12Curso(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CostoCursoR12 r12 = db.CostoCursoR12.Where(r => r.idCurso == id).FirstOrDefault();
            if (r12 == null)
            {
                return RedirectToAction("Create", new { id = id });
            }
            return RedirectToAction("Details", new { id = r12.idCostoCursoR12 });
        }

        // GET: CostoParticularCurso/Create/5
        public ActionResult Create(int? id)
        {
            CostoCursoR12 costoR12 = new CostoCursoR12();
            //var listCursosYaOcupado = db.CostoCursoR12.Select(x => x.idCurso).ToList();
            //ViewBag.Cursos = db.Curso.Where(x => !listCursosYaOcupado.Contains(x.idCurso)).ToList();
            ViewBag.idCurso = db.Curso.Find(id).idCurso;
            ViewBag.nombreCurso = db.Curso.Find(id).nombreCurso;
            ViewModelCostoParticularCurso myModel = new ViewModelCostoParticularCurso();
            myModel.listaCostoParticularCursos = db.ListaCostoParticularCurso.ToList();
            return View(myModel);
        }

        // POST: CostoParticularCurso/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ViewModelCostoParticularCurso myModel)
        {
            if (ModelState.IsValid)
            {
                CostoCursoR12 costoR12 = new CostoCursoR12();
                costoR12.idCurso = myModel.idCurso;
                costoR12.fechaCreacion = DateTime.Now;
                costoR12.softDelete = false;
                db.CostoCursoR12.Add(costoR12);
                db.SaveChanges();
                if (myModel.costoParticularCursosManoDeObra != null)
                {
                    foreach (CostoParticularCurso item in myModel.costoParticularCursosManoDeObra)
                    {
                        item.fechaCreacion = DateTime.Now;
                        item.idCostoCursoR12 = costoR12.idCostoCursoR12;
                        item.categoria = "Mano de Obra";
                        db.CostoParticularCurso.Add(item);
                        db.SaveChanges();
                        if (db.ListaCostoParticularCurso.Where(x => x.detalle == item.detalle).Count() == 0)
                        {
                            ListaCostoParticularCurso listaCostoParticularCurso = new ListaCostoParticularCurso();
                            listaCostoParticularCurso.detalle = item.detalle;
                            listaCostoParticularCurso.cantidad = item.cantidad;
                            listaCostoParticularCurso.categoria = "Mano de Obra";
                            listaCostoParticularCurso.unidad = item.unidad;
                            listaCostoParticularCurso.costo = item.costo;
                            db.ListaCostoParticularCurso.Add(listaCostoParticularCurso);
                            db.SaveChanges();
                        }

                    }
                }
                if (myModel.costoParticularCursosEquiposYHerramientas != null)
                {
                    foreach (CostoParticularCurso item in myModel.costoParticularCursosEquiposYHerramientas)
                    {
                        item.fechaCreacion = DateTime.Now;
                        item.idCostoCursoR12 = costoR12.idCostoCursoR12;
                        item.categoria = "Equipos y Herramientas";
                        db.CostoParticularCurso.Add(item);
                        db.SaveChanges();
                        if (db.ListaCostoParticularCurso.Where(x => x.detalle == item.detalle).Count() == 0)
                        {
                            ListaCostoParticularCurso listaCostoParticularCurso = new ListaCostoParticularCurso();
                            listaCostoParticularCurso.detalle = item.detalle;
                            listaCostoParticularCurso.cantidad = item.cantidad;
                            listaCostoParticularCurso.categoria = "Equipos y Herramientas";
                            listaCostoParticularCurso.unidad = item.unidad;
                            listaCostoParticularCurso.costo = item.costo;
                            db.ListaCostoParticularCurso.Add(listaCostoParticularCurso);
                            db.SaveChanges();
                        }

                    }
                }
                if (myModel.costoParticularCursosMateriales != null)
                {
                    foreach (CostoParticularCurso item in myModel.costoParticularCursosMateriales)
                    {
                        item.fechaCreacion = DateTime.Now;
                        item.idCostoCursoR12 = costoR12.idCostoCursoR12;
                        item.categoria = "Materiales";
                        db.CostoParticularCurso.Add(item);
                        db.SaveChanges();
                        if (db.ListaCostoParticularCurso.Where(x => x.detalle == item.detalle).Count() == 0)
                        {
                            ListaCostoParticularCurso listaCostoParticularCurso = new ListaCostoParticularCurso();
                            listaCostoParticularCurso.detalle = item.detalle;
                            listaCostoParticularCurso.cantidad = item.cantidad;
                            listaCostoParticularCurso.categoria = "Materiales";
                            listaCostoParticularCurso.unidad = item.unidad;
                            listaCostoParticularCurso.costo = item.costo;
                            db.ListaCostoParticularCurso.Add(listaCostoParticularCurso);
                            db.SaveChanges();
                        }

                    }
                }
                if (myModel.costoParticularCursosOtrosGastos != null)
                {

                    foreach (CostoParticularCurso item in myModel.costoParticularCursosOtrosGastos)
                    {
                        item.fechaCreacion = DateTime.Now;
                        item.idCostoCursoR12 = costoR12.idCostoCursoR12;
                        item.categoria = "Otros Gastos";
                        db.CostoParticularCurso.Add(item);
                        db.SaveChanges();
                        if (db.ListaCostoParticularCurso.Where(x => x.detalle == item.detalle).Count() == 0)
                        {
                            ListaCostoParticularCurso listaCostoParticularCurso = new ListaCostoParticularCurso();
                            listaCostoParticularCurso.detalle = item.detalle;
                            listaCostoParticularCurso.cantidad = item.cantidad;
                            listaCostoParticularCurso.categoria = "Otros Gastos";
                            listaCostoParticularCurso.unidad = item.unidad;
                            listaCostoParticularCurso.costo = item.costo;
                            db.ListaCostoParticularCurso.Add(listaCostoParticularCurso);
                            db.SaveChanges();
                        }
                    }
                }
                return RedirectToAction("Index", "Curso");
            }

            ViewBag.idCurso = myModel.idCurso;
            ViewBag.nombreCurso = db.Curso.Find(myModel.idCurso).nombreCurso;
            //ViewBag.Cursos = db.Curso.ToList();
            myModel.listaCostoParticularCursos = db.ListaCostoParticularCurso.ToList();

            return View(myModel);
        }

        // GET: CostoParticularCurso/Edit/5
        public ActionResult Edit(int id)
        {
            ViewModelCostoParticularCurso myModel = new ViewModelCostoParticularCurso();
            myModel.costoParticularCursosManoDeObra = db.CostoParticularCurso
                .Where(x => x.idCostoCursoR12 == id && x.categoria == "Mano de Obra")
                .ToList();
            myModel.costoParticularCursosEquiposYHerramientas = db.CostoParticularCurso
                .Where(x => x.idCostoCursoR12 == id && x.categoria == "Equipos y Herramientas")
                .ToList();
            myModel.costoParticularCursosMateriales = db.CostoParticularCurso
                .Where(x => x.idCostoCursoR12 == id && x.categoria == "Materiales")
                .ToList();
            myModel.costoParticularCursosOtrosGastos = db.CostoParticularCurso
                .Where(x => x.idCostoCursoR12 == id && x.categoria == "Otros Gastos")
                .ToList();
            myModel.listaCostoParticularCursos = db.ListaCostoParticularCurso.ToList();
            myModel.idCostoCursoR12 = id;
            CostoCursoR12 costoCursoR12 = db.CostoCursoR12.Find(id);
            ViewBag.Cursos = db.Curso.Where(x => x.idCurso == costoCursoR12.idCurso).ToList();
            return View(myModel);
        }

        // POST: CostoParticularCurso/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ViewModelCostoParticularCurso myModel)
        {
            if (ModelState.IsValid)
            {
                var eliminarCostoParticularCurso = db.CostoParticularCurso.Where(x => x.idCostoCursoR12 == myModel.idCostoCursoR12).ToList();
                db.CostoParticularCurso.RemoveRange(eliminarCostoParticularCurso.AsEnumerable());
                db.SaveChanges();

                if (myModel.costoParticularCursosManoDeObra != null)
                {
                    foreach (CostoParticularCurso item in myModel.costoParticularCursosManoDeObra)
                    {
                        item.fechaCreacion = DateTime.Now;
                        item.idCostoCursoR12 = myModel.idCostoCursoR12;
                        item.categoria = "Mano de Obra";
                        db.CostoParticularCurso.Add(item);
                        db.SaveChanges();
                        if (db.ListaCostoParticularCurso.Where(x => x.detalle == item.detalle).Count() == 0)
                        {
                            ListaCostoParticularCurso listaCostoParticularCurso = new ListaCostoParticularCurso();
                            listaCostoParticularCurso.detalle = item.detalle;
                            listaCostoParticularCurso.cantidad = item.cantidad;
                            listaCostoParticularCurso.categoria = "Mano de Obra";
                            listaCostoParticularCurso.unidad = item.unidad;
                            listaCostoParticularCurso.costo = item.costo;
                            db.ListaCostoParticularCurso.Add(listaCostoParticularCurso);
                            db.SaveChanges();
                        }

                    }
                }
                if (myModel.costoParticularCursosEquiposYHerramientas != null)
                {
                    foreach (CostoParticularCurso item in myModel.costoParticularCursosEquiposYHerramientas)
                    {
                        item.fechaCreacion = DateTime.Now;
                        item.idCostoCursoR12 = myModel.idCostoCursoR12;
                        item.categoria = "Equipos y Herramientas";
                        db.CostoParticularCurso.Add(item);
                        db.SaveChanges();
                        if (db.ListaCostoParticularCurso.Where(x => x.detalle == item.detalle).Count() == 0)
                        {
                            ListaCostoParticularCurso listaCostoParticularCurso = new ListaCostoParticularCurso();
                            listaCostoParticularCurso.detalle = item.detalle;
                            listaCostoParticularCurso.cantidad = item.cantidad;
                            listaCostoParticularCurso.categoria = "Equipos y Herramientas";
                            listaCostoParticularCurso.unidad = item.unidad;
                            listaCostoParticularCurso.costo = item.costo;
                            db.ListaCostoParticularCurso.Add(listaCostoParticularCurso);
                            db.SaveChanges();
                        }

                    }
                }
                if (myModel.costoParticularCursosMateriales != null)
                {
                    foreach (CostoParticularCurso item in myModel.costoParticularCursosMateriales)
                    {
                        item.fechaCreacion = DateTime.Now;
                        item.idCostoCursoR12 = myModel.idCostoCursoR12;
                        item.categoria = "Materiales";
                        db.CostoParticularCurso.Add(item);
                        db.SaveChanges();
                        if (db.ListaCostoParticularCurso.Where(x => x.detalle == item.detalle).Count() == 0)
                        {
                            ListaCostoParticularCurso listaCostoParticularCurso = new ListaCostoParticularCurso();
                            listaCostoParticularCurso.detalle = item.detalle;
                            listaCostoParticularCurso.cantidad = item.cantidad;
                            listaCostoParticularCurso.categoria = "Materiales";
                            listaCostoParticularCurso.unidad = item.unidad;
                            listaCostoParticularCurso.costo = item.costo;
                            db.ListaCostoParticularCurso.Add(listaCostoParticularCurso);
                            db.SaveChanges();
                        }

                    }
                }
                if (myModel.costoParticularCursosOtrosGastos != null)
                {

                    foreach (CostoParticularCurso item in myModel.costoParticularCursosOtrosGastos)
                    {
                        item.fechaCreacion = DateTime.Now;
                        item.idCostoCursoR12 = myModel.idCostoCursoR12;
                        item.categoria = "Otros Gastos";
                        db.CostoParticularCurso.Add(item);
                        db.SaveChanges();
                        if (db.ListaCostoParticularCurso.Where(x => x.detalle == item.detalle).Count() == 0)
                        {
                            ListaCostoParticularCurso listaCostoParticularCurso = new ListaCostoParticularCurso();
                            listaCostoParticularCurso.detalle = item.detalle;
                            listaCostoParticularCurso.cantidad = item.cantidad;
                            listaCostoParticularCurso.categoria = "Otros Gastos";
                            listaCostoParticularCurso.unidad = item.unidad;
                            listaCostoParticularCurso.costo = item.costo;
                            db.ListaCostoParticularCurso.Add(listaCostoParticularCurso);
                            db.SaveChanges();
                        }
                    }
                }
                return RedirectToAction("Index", "Curso");
            }
            myModel.listaCostoParticularCursos = db.ListaCostoParticularCurso.ToList();
            ViewBag.Cursos = db.Curso.Where(x => x.idCurso == myModel.idCurso).ToList();
            return View(myModel);
        }

        //// GET: CostoParticularCurso/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    CostoParticularCurso CostoParticularCurso = db.CostoParticularCurso.Find(id);
        //    if (CostoParticularCurso == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(CostoParticularCurso);
        //}

        //// POST: CostoParticularCurso/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    CostoCursoR12 CostoCurso = db.CostoCursoR12.Find(id);
        //    db.CostoCursoR12.Remove(CostoCurso);
        //    //CostoCurso.softDelete = true;
        //    //db.Entry(CostoCurso).State = EntityState.Modified;
        //    //db.SaveChanges();

        //    var eliminarCostoParticularCurso = db.CostoParticularCurso.Where(x => x.idCostoCursoR12 == id).ToList();
        //    db.CostoParticularCurso.RemoveRange(eliminarCostoParticularCurso.AsEnumerable());
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

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
