using SGC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace SGC.Controllers
{
    public class LinkController : Controller
    {
        private InsecapContext db = new InsecapContext();
        private Regex urlchk = new Regex(@"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // GET: Link
        public ActionResult Index()
        {
            var links = db.Link.OrderBy(x => x.type.nombre).ToList();
            return View(links);
        }

        // GET: Link/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Link/Create
        public ActionResult Create(int id)
        {
            var link = new Link();
            ViewBag.type = GetLinkType();
            if (id != 0)
            {
                link = db.Link.Find(id);
            }
            return View(link);
        }
        public SelectList GetLinkType()
        {
            List<LinkType> links = db.LinkTypes.OrderBy(x => x.nombre).ToList();


            return new SelectList(links.Select(c => new SelectListItem
            {
                Text = c.nombre ,
                Value = c.idLinkType.ToString()
            }).ToList(), "Value", "Text");
        }
        // POST: Link/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Link link)
        {
            ViewBag.type = GetLinkType();
            try
            {
                // TODO: Add insert logic here
                if (!urlchk.IsMatch(link.url))
                {
                    ModelState.AddModelError("url", "Formato incorrecto del link");
                }
                var NotUnique = db.Link.Any(x => x.url.Equals(link.url));
                if (NotUnique)
                {
                    ModelState.AddModelError("url", "El link ingresado ya existe");
                }
                link.type = db.LinkTypes.Find(link.type.idLinkType);
                if (ModelState.IsValid) {
                    
                    if (link.idLink == 0)
                    {
                        db.Link.Add(link);
                    }
                    else {
                        db.Entry(link).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(link);
                //return RedirectToAction("Index");
            }
            catch
            {
              
                return View(link);
            }
        }

      

        // GET: Link/Delete/5
        public ActionResult Delete(int id)
        {
            try {
                var link = db.Link.Find(id);
                db.Link.Remove(link);
                db.SaveChanges();
                ModelState.AddModelError("", "Eliminado correctamente");
            }
            catch(Exception e)
            {
                ModelState.AddModelError("", e.Message);

            }

            return RedirectToAction("Index");
        }

 
    }
}
