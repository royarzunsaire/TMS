namespace SGC.Controllers
{
    //public class FormatoDocumentoR50Controller : Controller
    //{
    //    private InsecapContext db = new InsecapContext();

    //    // GET: FormatoDocumentoR50
    //    public ActionResult Index()
    //    {
    //        return View(db.FormatoDocumentoR50.ToList());
    //    }

    //    // GET: FormatoDocumentoR50/Details/5
    //    public ActionResult Details(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        FormatoDocumentoR50 formatoDocumentoR50 = db.FormatoDocumentoR50.Find(id);
    //        if (formatoDocumentoR50 == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(formatoDocumentoR50);
    //    }

    //    // GET: FormatoDocumentoR50/Create
    //    public ActionResult Create()
    //    {
    //        return View();
    //    }

    //    // POST: FormatoDocumentoR50/Create
    //    // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
    //    // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Create([Bind(Include = "idFormatoDocumentoR50,nombre,descripcion,rutaArchivo")] FormatoDocumentoR50 formatoDocumentoR50)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            formatoDocumentoR50.fechaCreacion = DateTime.Now;
    //            formatoDocumentoR50.usuarioCreador = User.Identity.Name;
    //            db.FormatoDocumentoR50.Add(formatoDocumentoR50);
    //            db.SaveChanges();
    //            return RedirectToAction("Index");
    //        }

    //        return View(formatoDocumentoR50);
    //    }

    //    // GET: FormatoDocumentoR50/Edit/5
    //    public ActionResult Edit(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        FormatoDocumentoR50 formatoDocumentoR50 = db.FormatoDocumentoR50.Find(id);
    //        if (formatoDocumentoR50 == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(formatoDocumentoR50);
    //    }

    //    // POST: FormatoDocumentoR50/Edit/5
    //    // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
    //    // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Edit([Bind(Include = "idFormatoDocumentoR50,nombre,descripcion,rutaArchivo,usuarioCreador,fechaCreacion")] FormatoDocumentoR50 formatoDocumentoR50)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            db.Entry(formatoDocumentoR50).State = EntityState.Modified;
    //            db.SaveChanges();
    //            return RedirectToAction("Index");
    //        }
    //        return View(formatoDocumentoR50);
    //    }

    //    // GET: FormatoDocumentoR50/Delete/5
    //    public ActionResult Delete(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        FormatoDocumentoR50 formatoDocumentoR50 = db.FormatoDocumentoR50.Find(id);
    //        if (formatoDocumentoR50 == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(formatoDocumentoR50);
    //    }

    //    // POST: FormatoDocumentoR50/Delete/5
    //    [HttpPost, ActionName("Delete")]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult DeleteConfirmed(int id)
    //    {
    //        FormatoDocumentoR50 formatoDocumentoR50 = db.FormatoDocumentoR50.Find(id);
    //        db.FormatoDocumentoR50.Remove(formatoDocumentoR50);
    //        db.SaveChanges();
    //        return RedirectToAction("Index");
    //    }

    //    protected override void Dispose(bool disposing)
    //    {
    //        if (disposing)
    //        {
    //            db.Dispose();
    //        }
    //        base.Dispose(disposing);
    //    }
    //}
}
