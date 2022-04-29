using SGC.CustomAuthorize;
using SGC.Models;
using SGC.Utils;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    [CustomAuthorize(new string[] { "/Sucursal/" })]
    public class SucursalController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: Sucursal
        public ActionResult Index()
        {
            return View(db.Sucursal.ToList());
        }

        // GET: Sucursal/Firma
        public async Task<ActionResult> Firma(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sucursal sucursal = db.Sucursal.Find(id);
            if (sucursal == null)
            {
                return HttpNotFound();
            }
            Files.borrarArchivosLocales();
            await Files.BajarArchivoADirectorioLocalAsync(sucursal.firmaAdministrador);
            return View(sucursal);
        }

        // POST: Sucursal/Firma/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Firma([Bind(Include = "idSucursal,nombreAdministrador,runAdministrador,direccionAdministrador")] Sucursal sucursal)
        {
            var sucursalBD = db.Sucursal.Find(sucursal.idSucursal);
            sucursalBD.nombreAdministrador = sucursal.nombreAdministrador;
            sucursalBD.runAdministrador = sucursal.runAdministrador;
            sucursalBD.direccionAdministrador = sucursal.direccionAdministrador;
            sucursal = sucursalBD;
            var firmaAntigua = sucursal.firmaAdministrador;
            HttpPostedFileBase file = Request.Files[0];
            // verificar que se selecciono un archivo
            if (file.ContentLength <= 0)
            {
                ModelState.AddModelError("", "Se debe seleccionar un archivo.");
            }
            else
            {
                // validar extenciones y tamaño maximo del archivo
                var archivoValido = Files.ArchivoValido(file, new[] { ".png" }, 20);
                if (archivoValido != "")
                {
                    ModelState.AddModelError("", archivoValido);
                }
                else
                {
                    sucursal.firmaAdministrador = await Files.RemplazarArchivoAsync(sucursal.firmaAdministrador, file, "sucursales/firmas/");
                    if (sucursal.firmaAdministrador == null)
                    {
                        ModelState.AddModelError("", "No se pudo guardar el archivo seleccionado.");
                    }
                }
            }
            if (ModelState.IsValid)
            {
                if (firmaAntigua != null)
                {
                    db.Storages.Remove(firmaAntigua);
                }
                db.Entry(sucursal).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            sucursal.firmaAdministrador = firmaAntigua;
            Files.borrarArchivosLocales();
            await Files.BajarArchivoADirectorioLocalAsync(sucursal.firmaAdministrador);
            return View(sucursal);
        }

        // GET: Sucursal/Descargar/5
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

        //// GET: Sucursal/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Sucursal sucursal = db.Sucursal.Find(id);
        //    if (sucursal == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(sucursal);
        //}

        //// GET: Sucursal/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Sucursal/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "idSucursal,nombre,prefijoCodigo")] Sucursal sucursal)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Sucursal.Add(sucursal);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(sucursal);
        //}

        //// GET: Sucursal/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Sucursal sucursal = db.Sucursal.Find(id);
        //    if (sucursal == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(sucursal);
        //}

        //// POST: Sucursal/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "idSucursal,nombre,prefijoCodigo")] Sucursal sucursal)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(sucursal).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(sucursal);
        //}

        //// GET: Sucursal/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Sucursal sucursal = db.Sucursal.Find(id);
        //    if (sucursal == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(sucursal);
        //}

        //// POST: Sucursal/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Sucursal sucursal = db.Sucursal.Find(id);
        //    db.Sucursal.Remove(sucursal);
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
