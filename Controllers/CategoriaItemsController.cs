namespace SGC.Controllers
{
    //public class CategoriaItemsController : Controller
    //{
    //    private InsecapContext db = new InsecapContext();

    //    // GET: CategoriaItems
    //    public async Task<ActionResult> Index()
    //    {
    //        return View(await db.CategoriaItems.ToListAsync());
    //    }

    //    // GET: CategoriaItems/Details/5
    //    public async Task<ActionResult> Details(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        CategoriaItem categoriaItem = await db.CategoriaItems.FindAsync(id);
    //        if (categoriaItem == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(categoriaItem);
    //    }

    //    // GET: CategoriaItems/Create
    //    public ActionResult Create()
    //    {
    //        return View();
    //    }

    //    // POST: CategoriaItems/Create
    //    // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
    //    // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<ActionResult> Create(CategoriaItem categoriaItem)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            categoriaItem.usuarioCreador = User.Identity.Name;
    //            categoriaItem.fechaCreacion = DateTime.Now;
    //            categoriaItem.vigencia = 1;
    //            db.CategoriaItems.Add(categoriaItem);
    //            await db.SaveChangesAsync();
    //            return RedirectToAction("Index");
    //        }

    //        return View(categoriaItem);
    //    }

    //    // GET: CategoriaItems/Edit/5
    //    public async Task<ActionResult> Edit(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        CategoriaItem categoriaItem = await db.CategoriaItems.FindAsync(id);
    //        if (categoriaItem == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(categoriaItem);
    //    }

    //    // POST: CategoriaItems/Edit/5
    //    // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
    //    // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<ActionResult> Edit(CategoriaItem categoriaItem)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            categoriaItem.usuarioCreador = User.Identity.Name;
    //            categoriaItem.fechaCreacion = DateTime.Now;
    //            categoriaItem.vigencia = 1;
    //            db.Entry(categoriaItem).State = EntityState.Modified;
    //            await db.SaveChangesAsync();
    //            return RedirectToAction("Index");
    //        }
    //        return View(categoriaItem);
    //    }

    //    // GET: CategoriaItems/Delete/5
    //    public async Task<ActionResult> Delete(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        CategoriaItem categoriaItem = await db.CategoriaItems.FindAsync(id);
    //        if (categoriaItem == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(categoriaItem);
    //    }

    //    // POST: CategoriaItems/Delete/5
    //    [HttpPost, ActionName("Delete")]
    //    [ValidateAntiForgeryToken]
    //    public async Task<ActionResult> DeleteConfirmed(int id)
    //    {
    //        CategoriaItem categoriaItem = await db.CategoriaItems.FindAsync(id);
    //        db.CategoriaItems.Remove(categoriaItem);
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
