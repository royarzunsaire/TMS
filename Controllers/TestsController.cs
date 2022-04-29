namespace SGC.Controllers
{
    //public class TestsController : Controller
    //{
    //    private InsecapContext db = new InsecapContext();

    //    // GET: Tests
    //    public async Task<ActionResult> Index()
    //    {
    //        return View(await db.Test.ToListAsync());
    //    }

    //    // GET: Tests/Details/5
    //    public async Task<ActionResult> Details(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        Test test = await db.Test.FindAsync(id);
    //        if (test == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(test);
    //    }

    //    // GET: Tests/Create
    //    public ActionResult Create()
    //    {
    //        return View();
    //    }

    //    // POST: Tests/Create
    //    // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
    //    // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<ActionResult> Create([Bind(Include = "idCliente,Name,ModifiedDate,LastName")] Test test)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            db.Test.Add(test);
    //            await db.SaveChangesAsync();
    //            return RedirectToAction("Index");
    //        }

    //        return View(test);
    //    }

    //    // GET: Tests/Edit/5
    //    public async Task<ActionResult> Edit(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        Test test = await db.Test.FindAsync(id);
    //        if (test == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(test);
    //    }

    //    // POST: Tests/Edit/5
    //    // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
    //    // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<ActionResult> Edit([Bind(Include = "idCliente,Name,ModifiedDate,LastName")] Test test)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            db.Entry(test).State = EntityState.Modified;
    //            await db.SaveChangesAsync();
    //            return RedirectToAction("Index");
    //        }
    //        return View(test);
    //    }

    //    // GET: Tests/Delete/5
    //    public async Task<ActionResult> Delete(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        Test test = await db.Test.FindAsync(id);
    //        if (test == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(test);
    //    }

    //    // POST: Tests/Delete/5
    //    [HttpPost, ActionName("Delete")]
    //    [ValidateAntiForgeryToken]
    //    public async Task<ActionResult> DeleteConfirmed(int id)
    //    {
    //        Test test = await db.Test.FindAsync(id);
    //        db.Test.Remove(test);
    //        await db.SaveChangesAsync();
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
