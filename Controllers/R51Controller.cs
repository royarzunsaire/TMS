using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    [CustomAuthorize(new string[] { "/Curso/" })]
    public class R51Controller : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: R51
        //public ActionResult Index()
        //{
        //    //if (TempData["PosseR11"] != null)
        //    //{
        //    //    ViewBag.PosseR11 = TempData["PosseR11"];

        //    //}
        //    return View(db.R51.Where(r => r.softDelete == false).ToList());
        //}

        // GET: R51/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            R51 r51 = db.R51.Find(id);
            if (r51 == null)
            {
                return HttpNotFound();
            }
            ViewModelR51 mymodel = new ViewModelR51();
            mymodel.r51 = r51;
            mymodel.ciudad = db.Ciudad.ToList();
            mymodel.r51_Checklists = db.R51_Checklist.Where(s => s.idR51 == id).ToList();
            var listIdChecklist = db.R51_Checklist.Where(s => s.idR51 == id).Select(o => o.idChecklist).ToList();
            mymodel.checklists = db.Checklist
                .Where(i => listIdChecklist.Contains(i.idChecklist))
                .ToList();
            return View(mymodel);
        }

        // GET: R51/R51Curso/5
        public ActionResult R51Curso(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            R51 r51 = db.R51.Where(r => r.idCurso == id).FirstOrDefault();
            if (r51 == null)
            {
                return HttpNotFound();
            }
            return RedirectToAction("Details", new { id = r51.idR51 });
        }

        // GET: R51/Create
        public ActionResult Create()
        {
            ViewModelR51 mymodel = new ViewModelR51();
            mymodel.checklists = db.Checklist.Where(c => c.softDelete == false).Where(c => c.vigencia == 1).ToList();
            mymodel.ciudad = db.Ciudad.ToList();
            return View(mymodel);
        }

        // POST: R51/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(R51 r51, ICollection<R51_Checklist> r51_checkList)
        {

            if (ModelState.IsValid)
            {

                Curso curso = new Curso();
                curso.nombreCurso = r51.nombreCurso;
                // codigo curso
                var ultimoCodigo = "0";
                if (db.Curso.OrderByDescending(x => x.idCurso).FirstOrDefault() != null)
                {
                    ultimoCodigo = db.Curso.OrderByDescending(x => x.idCurso).FirstOrDefault().codigoCurso;
                }
                curso.codigoCurso = Helpers.CustomUtilsHelper.GeneracionCodigo("C", ultimoCodigo);
                curso.tipoEjecucion = r51.Curso.tipoEjecucion;
                curso.softDelete = false;
                db.Curso.Add(curso);
                db.SaveChanges();

                r51.idCurso = curso.idCurso;
                r51.Curso = curso;
                r51.fechaCreacion = DateTime.Now;
                r51.softDelete = false;
                r51.userCreador = db.AspNetUsers.Find(User.Identity.GetUserId());
                db.R51.Add(r51);
                db.SaveChanges();
                foreach (R51_Checklist dato in r51_checkList)
                {
                    dato.idR51 = r51.idR51;
                    db.R51_Checklist.Add(dato);
                }
                db.SaveChanges();
                CorreoR51Creado(r51);


                return RedirectToAction("Index", "Curso");
            }

            ViewModelR51 mymodel = new ViewModelR51();
            mymodel.checklists = db.Checklist
                .Where(c => c.softDelete == false)
                .Where(i => i.vigencia == 1)
                .ToList();
            mymodel.r51_Checklists = r51_checkList;
            mymodel.ciudad = db.Ciudad.ToList();

            return View(mymodel);


        }
        private void CorreoR51Creado(R51 r51)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
            // obtener usuarios que deben recibir el correo
            var roles = db.AspNetRoles.Where(x => x.Name.ToLower().Contains("diseño & desarrollo") && x.Id != r51.userCreador.Id).ToList();
            List<string> emails = new List<string>();
            foreach (var rol in roles)
            {
                foreach (var usuario in rol.AspNetUsers.Where(x => x.Email != r51.userCreador.Email).ToList())
                {
                    
                    emails.Add(usuario.Email + ";"+usuario.nombreCompleto);

                }
            }
            if (r51.userCreador != null) {
                emails.Add(r51.userCreador.Email + ";" + r51.userCreador.nombreCompleto);
            }
            emails = emails.Distinct().ToList();

            var subject = String.Format("NUEVO R51 INGRESADO EN SISTEMA '{0}'",r51.Curso.nombreCurso);
            var body = String.Format("Nuevo R51 creado por {0}, con el nombre de curso: {1} el día: {2}. Presione {3} para ver los detalles. Atentamente TMS. ", r51.userCreador==null ?"Sin Creador":r51.userCreador.nombreCompleto, r51.Curso.nombreCurso, String.Format("{0:dddd d , MMMM , yyyy}", r51.fechaCreacion).Replace(",", "de"), string.Format("<a href=\" {0}/R51/Details/{1} \"> aquí  </a> ",
                        Utils.Utils.domain, r51.idR51));

            foreach (string mail in emails) {
                var receiverEmail = new MailAddress(mail.Split(';')[0], mail.Split(';')[1]);

                Utils.Utils.SendMail(receiverEmail, subject, body);

            }

           

        }
        // GET: R51/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            R51 r51 = db.R51.Find(id);
            if (r51 == null)
            {
                return HttpNotFound();
            }
            ViewModelR51 mymodel = new ViewModelR51();
            mymodel.r51 = r51;
            mymodel.ciudad = db.Ciudad.ToList();
            mymodel.r51_Checklists = db.R51_Checklist.Where(s => s.idR51 == id).ToList();
            var listIdChecklist = db.R51_Checklist.Where(s => s.idR51 == id).Select(o => o.idChecklist).ToList();
            mymodel.checklists = db.Checklist
                .Where(i => listIdChecklist.Contains(i.idChecklist))
                .ToList();
            return View(mymodel);
        }

        // POST: R51/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(R51 r51, ICollection<R51_Checklist> r51_checkList)
        {
            if (ModelState.IsValid)
            {
                Curso curso = db.Curso.Find(r51.idCurso);
                curso.nombreCurso = r51.nombreCurso;
                curso.tipoEjecucion = r51.Curso.tipoEjecucion;
                db.Entry(curso).State = EntityState.Modified;
                r51.Curso = curso;
                db.Entry(r51).State = EntityState.Modified;
                foreach (R51_Checklist dato in r51_checkList)
                {
                    dato.idR51 = r51.idR51;
                    db.Entry(dato).State = EntityState.Modified;
                }
                db.SaveChanges();

                return RedirectToAction("Index", "Curso");
            }
            ViewModelR51 mymodel = new ViewModelR51();
            mymodel.r51_Checklists = db.R51_Checklist.Where(s => s.idR51 == r51.idR51).ToList();
            var listIdChecklist = db.R51_Checklist.Where(s => s.idR51 == r51.idR51).Select(o => o.idChecklist).ToList();
            mymodel.checklists = db.Checklist
                .Where(i => listIdChecklist.Contains(i.idChecklist))
                .ToList();
            mymodel.ciudad = db.Ciudad.ToList();
            mymodel.r51 = r51;
            return View(mymodel);
        }

        // GET: R51/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    R51 r51 = db.R51.Find(id);




        //    db.R51.Remove(r51);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        //// POST: R51/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    R51 r51 = db.R51.Find(id);
        //    //if (db.R11.Where(x => x.idCurso == r51.idCurso).Count() > 0)
        //    //{
        //    //    TempData["PosseR11"] = "No se puede eliminar el R51, porque ya tiene un R11 creado.";
        //    //    return RedirectToAction("Index");
        //    //}
        //    //else
        //    //{
        //    //    TempData["PosseR11"] = null;
        //    //}

        //    db.R51.Remove(r51);
        //    //r51.softDelete = true;
        //    //db.Entry(r51).State = EntityState.Modified;
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
