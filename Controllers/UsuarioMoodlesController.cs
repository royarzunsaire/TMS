namespace SGC.Controllers
{
    //public class UsuarioMoodlesController : Controller
    //{
    //    private InsecapContext db = new InsecapContext();

    //    // GET: UsuarioMoodles
    //    public ActionResult Index()
    //    {
    //        return View(db.UsuarioMoodles.ToList());
    //    }

    //    // GET: UsuarioMoodles/Details/5
    //    public ActionResult Details(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        UsuarioMoodle usuarioMoodle = db.UsuarioMoodles.Find(id);
    //        if (usuarioMoodle == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(usuarioMoodle);
    //    }

    //    // GET: UsuarioMoodles/Create
    //    public ActionResult Create()
    //    {
    //        return View();
    //    }

    //    // POST: UsuarioMoodles/Create
    //    // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
    //    // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Create([Bind(Include = "ID,username,email,firstName,lastName")] UsuarioMoodle usuarioMoodle)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            usuarioMoodle.password = RandomPassword(); // genera password aleatoriamente
    //            usuarioMoodle.creadoEnMoodle = false; // indicador para determinar si ha sido traspasado o no a la plataforma moodle
    //            db.UsuarioMoodles.Add(usuarioMoodle);
    //            db.SaveChanges();
    //            return RedirectToAction("Index");
    //        }

    //        return View(usuarioMoodle);
    //    }

    //    // GET: UsuarioMoodles/Edit/5
    //    public ActionResult Edit(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        UsuarioMoodle usuarioMoodle = db.UsuarioMoodles.Find(id);
    //        if (usuarioMoodle == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(usuarioMoodle);
    //    }

    //    // POST: UsuarioMoodles/Edit/5
    //    // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
    //    // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Edit([Bind(Include = "ID,username,email,firstName,lastName")] UsuarioMoodle usuarioMoodle)
    //    {
    //        if (ModelState.IsValid)
    //        {

    //            db.Entry(usuarioMoodle).State = EntityState.Modified;
    //            db.Entry(usuarioMoodle).Property("password").IsModified = false;
    //            db.SaveChanges();
    //            return RedirectToAction("Index");
    //        }
    //        return View(usuarioMoodle);
    //    }

    //    // GET: UsuarioMoodles/Delete/5
    //    public ActionResult Delete(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        UsuarioMoodle usuarioMoodle = db.UsuarioMoodles.Find(id);
    //        if (usuarioMoodle == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(usuarioMoodle);
    //    }

    //    // POST: UsuarioMoodles/Delete/5
    //    [HttpPost, ActionName("Delete")]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult DeleteConfirmed(int id)
    //    {
    //        UsuarioMoodle usuarioMoodle = db.UsuarioMoodles.Find(id);
    //        db.UsuarioMoodles.Remove(usuarioMoodle);
    //        db.SaveChanges();
    //        return RedirectToAction("Index");
    //    }

    //    #region regionRandomPassword 
    //    public string RandomPassword()
    //    {
    //        StringBuilder builder = new StringBuilder();
    //        builder.Append(RandomString(4, true)); // letras mayusculas
    //        builder.Append(RandomString(2, false)); // letras minusculas
    //        builder.Append("_"); // no alfanumerico
    //        builder.Append(RandomNumber(1000, 9999)); // numeros
    //        return builder.ToString();
    //    }
    //    public string RandomString(int size, bool lowerCase)
    //    {
    //        StringBuilder builder = new StringBuilder();
    //        Random random = new Random();
    //        char ch;
    //        for (int i = 0; i < size; i++)
    //        {
    //            ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
    //            builder.Append(ch);
    //        }
    //        if (lowerCase)
    //            return builder.ToString().ToLower();
    //        return builder.ToString();
    //    }

    //    public int RandomNumber(int min, int max)
    //    {
    //        Random random = new Random();
    //        return random.Next(min, max);
    //    }
    //    #endregion

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
