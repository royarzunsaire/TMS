namespace SGC.Controllers
{
    //public class VendedorController : Controller
    //{
    //    private InsecapContext db = new InsecapContext();

    //    // GET: Vendedor
    //    public ActionResult Index()
    //    {
    //        return View(db.Vendedor.ToList());
    //    }

    //    // GET: Vendedor/Details/5
    //    public ActionResult Details(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        Vendedor vendedor = db.Vendedor.Find(id);
    //        if (vendedor == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(vendedor);
    //    }

    //    // GET: Vendedor/Create
    //    public ActionResult Create()
    //    {
    //        return View();
    //    }

    //    // POST: Vendedor/Create
    //    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    //    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Create([Bind(Include = "idVendedor")] Vendedor vendedor)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            db.Vendedor.Add(vendedor);
    //            db.SaveChanges();
    //            return RedirectToAction("Index");
    //        }

    //        return View(vendedor);
    //    }

    //    // GET: Vendedor/Edit/5
    //    public ActionResult Edit(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        Vendedor vendedor = db.Vendedor.Find(id);
    //        if (vendedor == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(vendedor);
    //    }

    //    // POST: Vendedor/Edit/5
    //    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    //    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Edit([Bind(Include = "idVendedor")] Vendedor vendedor)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            db.Entry(vendedor).State = EntityState.Modified;
    //            db.SaveChanges();
    //            return RedirectToAction("Index");
    //        }
    //        return View(vendedor);
    //    }

    //    // GET: Vendedor/Delete/5
    //    public ActionResult Delete(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        Vendedor vendedor = db.Vendedor.Find(id);
    //        if (vendedor == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(vendedor);
    //    }

    //    // POST: Vendedor/Delete/5
    //    [HttpPost, ActionName("Delete")]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult DeleteConfirmed(int id)
    //    {
    //        Vendedor vendedor = db.Vendedor.Find(id);
    //        db.Vendedor.Remove(vendedor);
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
