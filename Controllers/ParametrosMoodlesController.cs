using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    [CustomAuthorize(new string[] { "/ParametrosMoodles/Create/" })]
    public class ParametrosMoodlesController : Controller
    {
        private InsecapContext db = new InsecapContext();

        //// GET: ParametrosMoodles
        //public ActionResult Index()
        //{
        //    return View(db.ParametrosMoodles.ToList());
        //}

        // GET: ParametrosMoodles/Test/5

        //public ActionResult Test(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ParametrosMoodle parametrosMoodle = db.ParametrosMoodles.Find(id);
        //    if (parametrosMoodle == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    var resp = Moodle.LoginMoodle(parametrosMoodle);
        //    ViewBag.Token = (resp != null ? resp : "Excepción al iniciar sesión");
        //    return View(parametrosMoodle);
        //}

        //// GET: ParametrosMoodles/Test2/5
        //public ActionResult Test2(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ParametrosMoodle parametrosMoodle = db.ParametrosMoodles.Find(id);
        //    if (parametrosMoodle == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    var resp = Moodle.LoginMoodle(parametrosMoodle);
        //    string token = resp != null ? resp.token : "Excepción al iniciar sesión";
        //    ViewBag.usersResponse = Moodle.MoodleCreateUser(token);
        //    return View(parametrosMoodle);
        //}

        //// GET: ParametrosMoodles/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ParametrosMoodle parametrosMoodle = db.ParametrosMoodles.Find(id);
        //    if (parametrosMoodle == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(parametrosMoodle);
        //}

        // GET: ParametrosMoodles/Create

        public ActionResult Create()
        {
            var parametrosMoodle = db.ParametrosMoodles.FirstOrDefault();
            if (parametrosMoodle != null)
            {
                return RedirectToAction("Edit", new { id = parametrosMoodle.ID });
            }
            return View();
        }

        // POST: ParametrosMoodles/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,username,password,urlMoodle,service,contraseñaUsuarios,idRolEstudiante")] ParametrosMoodle parametrosMoodle)
        {
            ValidarUrlMoodle(parametrosMoodle.urlMoodle);
            if (ModelState.IsValid)
            {
                db.ParametrosMoodles.Add(parametrosMoodle);
                db.SaveChanges();
                return RedirectToAction("Edit", new { id = parametrosMoodle.ID });
            }
            return View(parametrosMoodle);
        }

        private void ValidarUrlMoodle(string url)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (!result)
            {
                ModelState.AddModelError("", "Se debe ingresar una URL de Moodle válida.");
            }
        }

        // GET: ParametrosMoodles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ParametrosMoodle parametrosMoodle = db.ParametrosMoodles.Find(id);
            if (parametrosMoodle == null)
            {
                return HttpNotFound();
            }
            return View(parametrosMoodle);
        }

        // POST: ParametrosMoodles/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,username,password,urlMoodle,service,contraseñaUsuarios,idRolEstudiante")] ParametrosMoodle parametrosMoodle)
        {
            ValidarUrlMoodle(parametrosMoodle.urlMoodle);
            if (ModelState.IsValid)
            {
                db.Entry(parametrosMoodle).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Edit", new { id = parametrosMoodle.ID });
            }
            return View(parametrosMoodle);
        }

        //// GET: ParametrosMoodles/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ParametrosMoodle parametrosMoodle = db.ParametrosMoodles.Find(id);
        //    if (parametrosMoodle == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(parametrosMoodle);
        //}

        //// POST: ParametrosMoodles/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    ParametrosMoodle parametrosMoodle = db.ParametrosMoodles.Find(id);
        //    db.ParametrosMoodles.Remove(parametrosMoodle);
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
