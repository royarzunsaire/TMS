namespace SGC.Controllers
{
    //public class TipoFormatoDocumentoController : Controller
    //{
    //    private InsecapContext db = new InsecapContext();

    //    // GET: TipoFormatoDocumento
    //    public ActionResult Index()
    //    {
    //        return View(db.TipoFormatoDocumento.ToList());
    //    }

    //    // GET: TipoFormatoDocumento/Details/5
    //    public ActionResult Details(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        TipoFormatoDocumento tipoFormatoDocumento = db.TipoFormatoDocumento.Find(id);
    //        if (tipoFormatoDocumento == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(tipoFormatoDocumento);
    //    }

    //    // GET: TipoFormatoDocumento/Create
    //    public ActionResult Create()
    //    {
    //        return View();
    //    }

    //    // POST: TipoFormatoDocumento/Create
    //    // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
    //    // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Create([Bind(Include = "idTipoFormatoDocumento,tipo,vigencia")] TipoFormatoDocumento tipoFormatoDocumento)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            db.TipoFormatoDocumento.Add(tipoFormatoDocumento);
    //            db.SaveChanges();
    //            return RedirectToAction("Index");
    //        }

    //        return View(tipoFormatoDocumento);
    //    }

    //    // GET: TipoFormatoDocumento/Edit/5
    //    public ActionResult Edit(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        TipoFormatoDocumento tipoFormatoDocumento = db.TipoFormatoDocumento.Find(id);
    //        if (tipoFormatoDocumento == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(tipoFormatoDocumento);
    //    }

    //    // POST: TipoFormatoDocumento/Edit/5
    //    // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
    //    // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Edit([Bind(Include = "idTipoFormatoDocumento,tipo,vigencia")] TipoFormatoDocumento tipoFormatoDocumento)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            db.Entry(tipoFormatoDocumento).State = EntityState.Modified;
    //            db.SaveChanges();
    //            return RedirectToAction("Index");
    //        }
    //        return View(tipoFormatoDocumento);
    //    }

    //    // GET: TipoFormatoDocumento/Delete/5
    //    public ActionResult Delete(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        TipoFormatoDocumento tipoFormatoDocumento = db.TipoFormatoDocumento.Find(id);
    //        if (tipoFormatoDocumento == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(tipoFormatoDocumento);
    //    }

    //    // POST: TipoFormatoDocumento/Delete/5
    //    [HttpPost, ActionName("Delete")]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult DeleteConfirmed(int id)
    //    {
    //        TipoFormatoDocumento tipoFormatoDocumento = db.TipoFormatoDocumento.Find(id);
    //        db.TipoFormatoDocumento.Remove(tipoFormatoDocumento);
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
