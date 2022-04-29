namespace SGC.Controllers
{
    //public class EstadoComercialController : Controller
    //{
    //    private InsecapContext db = new InsecapContext();

    //    // GET: EstadoComercial
    //    public ActionResult Index()
    //    {
    //        return View(db.EstadoComercial.ToList());
    //    }

    //    // GET: EstadoComercial/Details/5
    //    public ActionResult Details(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        EstadoComercial estadoComercial = db.EstadoComercial.Find(id);
    //        if (estadoComercial == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(estadoComercial);
    //    }

    //    // GET: EstadoComercial/Create
    //    public ActionResult Create()
    //    {
    //        return View();
    //    }

    //    // POST: EstadoComercial/Create
    //    // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
    //    // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Create([Bind(Include = "idEstadoComercial,nombre,descripcion")] EstadoComercial estadoComercial)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            estadoComercial.fechaCreacion = DateTime.Now;
    //            estadoComercial.usuarioCreador = User.Identity.Name;
    //            db.EstadoComercial.Add(estadoComercial);
    //            db.SaveChanges();
    //            return RedirectToAction("Index");
    //        }

    //        return View(estadoComercial);
    //    }

    //    // GET: EstadoComercial/Edit/5
    //    public ActionResult Edit(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        EstadoComercial estadoComercial = db.EstadoComercial.Find(id);
    //        if (estadoComercial == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(estadoComercial);
    //    }

    //    // POST: EstadoComercial/Edit/5
    //    // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
    //    // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Edit([Bind(Include = "idEstadoComercial,nombre,descripcion,usuarioCreador,fechaCreacion")] EstadoComercial estadoComercial)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            db.Entry(estadoComercial).State = EntityState.Modified;
    //            db.SaveChanges();
    //            return RedirectToAction("Index");
    //        }
    //        return View(estadoComercial);
    //    }

    //    // GET: EstadoComercial/Delete/5
    //    public ActionResult Delete(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        EstadoComercial estadoComercial = db.EstadoComercial.Find(id);
    //        if (estadoComercial == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(estadoComercial);
    //    }

    //    // POST: EstadoComercial/Delete/5
    //    [HttpPost, ActionName("Delete")]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult DeleteConfirmed(int id)
    //    {
    //        EstadoComercial estadoComercial = db.EstadoComercial.Find(id);
    //        db.EstadoComercial.Remove(estadoComercial);
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
