namespace SGC.Controllers
{
    ////[CustomAuthorize(new int[] { 67 })]
    //public class ItemController : Controller
    //{
    //    private InsecapContext db = new InsecapContext();
    //    private SelectList CategoriaItems;
    //    public ItemController() {
    //        CategoriaItems = new SelectList(db.CategoriaItems.Select(c => new SelectListItem
    //        {
    //            Text = c.nombre,
    //            Value = c.idCategoriaItem.ToString()
    //        }).ToList(), "Value", "Text");

    //    }
    //    // GET: Item
    //    public async Task<ActionResult> Index()
    //    {
    //        return View(await db.Items.ToListAsync());
    //    }

    //    // GET: Item/Details/5
    //    public async Task<ActionResult> Details(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        Item item = await db.Items.FindAsync(id);
    //        if (item == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(item);
    //    }

    //    // GET: Item/Create
    //    public ActionResult Create()
    //    {
    //        ViewBag.categoriaItem = CategoriaItems;
    //        return View();
    //    }

    //    // POST: Item/Create
    //    // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
    //    // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<ActionResult> Create( Item item)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            item.fechaCreacion = DateTime.Now;
    //            item.usuarioCreador = User.Identity.Name;
    //            item.vigencia = 1;
    //            db.Items.Add(item);

    //            await db.SaveChangesAsync();
    //            return RedirectToAction("Index");
    //        }

    //        return View(item);
    //    }

    //    // GET: Item/Edit/5
    //    public async Task<ActionResult> Edit(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        Item item = await db.Items.FindAsync(id);
    //        if (item == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        if(item.categoria != null)
    //        {
    //            var selected = CategoriaItems.Where(x => x.Value == item.categoria.idCategoriaItem.ToString()).First();
    //            selected.Selected = true;
    //        }


    //        ViewBag.categoriaItem = CategoriaItems;

    //        return View(item);
    //    }

    //    // POST: Item/Edit/5
    //    // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
    //    // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<ActionResult> Edit(Item item)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            item.fechaCreacion = DateTime.Now;
    //            item.usuarioCreador = User.Identity.Name;

    //            db.Entry(item).State = EntityState.Modified;
    //            await db.SaveChangesAsync();
    //            return RedirectToAction("Index");
    //        }
    //        return View(item);
    //    }

    //    // GET: Item/Delete/5
    //    public async Task<ActionResult> Delete(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        Item item = await db.Items.FindAsync(id);
    //        if (item == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(item);
    //    }

    //    // POST: Item/Delete/5
    //    [HttpPost, ActionName("Delete")]
    //    [ValidateAntiForgeryToken]
    //    public async Task<ActionResult> DeleteConfirmed(int id)
    //    {
    //        Item item = await db.Items.FindAsync(id);
    //        db.Items.Remove(item);
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
