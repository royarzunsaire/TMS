using SGC.CustomAuthorize;
using SGC.Models;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class ConfiguracionUsuarioParticipanteController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: ConfiguracionUsuarioParticipante
        [CustomAuthorize(new string[] { "/ConfiguracionUsuarioParticipante/" })]
        public ActionResult Index()
        {
            ViewBag.roles = GetRoles();
            return View(db.ConfiguracionUsuarioParticipante.FirstOrDefault());
        }

        // POST: ConfiguracionUsuarioParticipante/
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/ConfiguracionUsuarioParticipante/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "idConfiguracionUsuarioParticipante,contrasenia")] ConfiguracionUsuarioParticipante configuracion)
        {
            if (Request["rol.Id"] == "" | Request["rol.Id"] == null)
            {
                ModelState.AddModelError("rol.Id", "El campo Rol es obligatorio");
            }
            if (ModelState.IsValid)
            {
                var configuracionBD = db.ConfiguracionUsuarioParticipante.FirstOrDefault();
                var rol = db.AspNetRoles.Find(Request["rol.Id"]);
                if (configuracionBD != null)
                {
                    configuracionBD.contrasenia = configuracion.contrasenia;
                    configuracionBD.rol = rol;
                    db.Entry(configuracionBD).State = EntityState.Modified;
                }
                else
                {
                    configuracion.rol = rol;
                    db.ConfiguracionUsuarioParticipante.Add(configuracion);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.roles = GetRoles();
            return View(configuracion);
        }

        public SelectList GetRoles()
        {
            return new SelectList(db.AspNetRoles
                .Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Id.ToString()
                })
                .ToList(), "Value", "Text");
        }

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
