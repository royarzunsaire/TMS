using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SGC.Controllers
{
    public class R06_ActaReunionController : Controller
    {

        private InsecapContext db = new InsecapContext();

        // GET: R06_ActaReunion
        public ActionResult Index()
        {
            return View(db.R06_ActaReunion.ToList());
        }

        // GET: R06_ActaReunion/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            R06_ActaReunion r06_ActaReunion = db.R06_ActaReunion.Find(id);
            if (r06_ActaReunion == null)
            {
                return HttpNotFound();
            }
            r06_ActaReunion.ParticipantesReunion = GetParticipantes(id);
            return View(r06_ActaReunion);
        }

        // GET: R06_ActaReunion/Create
        public ActionResult Create()
        {

            clearParticipantesReunionList();
            ViewBag.participantes = GetPosiblesParticipantes();
            return View();
        }

        // POST: R06_ActaReunion/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create([Bind(Include = "fecha,horarioInicio,horarioTermino,temasTratados,acuerdos")] R06_ActaReunion r06_ActaReunion)
        {

            var idUser = User.Identity.GetUserId();
            r06_ActaReunion.usuarioCreador = db.AspNetUsers.Find(idUser);
            r06_ActaReunion.dateCreation = DateTime.Now;
            r06_ActaReunion.dateEdited = DateTime.Now;
            r06_ActaReunion.ParticipantesReunion = new List<ParticipantesReunion>();
            r06_ActaReunion.softDelete = false;

            R06_ActaReunion ActaReunion = AgregarNuevosParticipantes(r06_ActaReunion);

            if (ModelState.IsValid)
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.R06_ActaReunion.Add(ActaReunion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        // GET: R06_ActaReunion/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            R06_ActaReunion r06_ActaReunion = db.R06_ActaReunion.Find(id);
            if (r06_ActaReunion == null)
            {
                return HttpNotFound();
            }
            r06_ActaReunion.ParticipantesReunion = GetParticipantes(id);
            ViewBag.participantes = GetPosiblesParticipantes();
            return View(r06_ActaReunion);
        }

        // POST: R06_ActaReunion/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Edit([Bind(Include = "idR06,fecha,horarioInicio,horarioTermino,temasTratados,acuerdos")] R06_ActaReunion r06_ActaReunion)
        {
            R06_ActaReunion newActaReunion = db.R06_ActaReunion.Find(r06_ActaReunion.idR06);
            var idUser = User.Identity.GetUserId();
            newActaReunion.usuarioUltimaEdicion = db.AspNetUsers.Find(idUser);
            newActaReunion.dateEdited = DateTime.Now;
            newActaReunion.fecha = r06_ActaReunion.fecha;
            newActaReunion.horarioInicio = r06_ActaReunion.horarioInicio;
            newActaReunion.horarioTermino = r06_ActaReunion.horarioTermino;
            newActaReunion.temasTratados = r06_ActaReunion.temasTratados;
            newActaReunion.temasTratados = r06_ActaReunion.temasTratados;
            newActaReunion.ParticipantesReunion = GetParticipantes(r06_ActaReunion.idR06);

            if (ModelState.IsValid)
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.Entry(newActaReunion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        public R06_ActaReunion AgregarNuevosParticipantes(R06_ActaReunion r06_ActaReunion)
        {
            var data = db.ParticipantesReunion.ToList();

            foreach (ParticipantesReunion participante in data)
            {
                if (participante.R06 == null)
                {
                    participante.R06 = r06_ActaReunion;
                    r06_ActaReunion.ParticipantesReunion.Add(participante);

                }

            }
            return r06_ActaReunion;
        }
        // GET: R06_ActaReunion/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            R06_ActaReunion r06_ActaReunion = db.R06_ActaReunion.Find(id);
            if (r06_ActaReunion == null)
            {
                return HttpNotFound();
            }
            r06_ActaReunion.ParticipantesReunion = GetParticipantes(id);
            return View(r06_ActaReunion);
        }

        // POST: R06_ActaReunion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            R06_ActaReunion r06_ActaReunion = db.R06_ActaReunion.Find(id);
            r06_ActaReunion.softDelete = true;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult agregarParticipanteSistema(int idR06, string participanteID)
        {

            AspNetUsers participante = db.AspNetUsers.Find(participanteID);

            ParticipantesReunion participanteSistema = new ParticipantesReunion();

            List<object> resultset = new List<object>();



            //si no tiene nombre completo
            if (participante.nombreCompleto.Length.Equals(2))
            {
                participanteSistema.nombre = participante.UserName;
            }
            else
            {
                participanteSistema.nombre = participante.nombreCompleto;
            }
            participanteSistema.telefono = participante.telefono;
            participanteSistema.empArea = participante.AspNetRoles.FirstOrDefault().Name;
            participanteSistema.firma = "pendiente";
            participanteSistema.idAspNetUser = participante;


            if (idR06 != 0)
            {
                R06_ActaReunion actaReunion = db.R06_ActaReunion.Find(idR06);
                participanteSistema.R06 = actaReunion;
            }
            db.ParticipantesReunion.Add(participanteSistema);
            db.SaveChanges();

            resultset.Add(
              new
              {
                  nombre = participanteSistema.nombre,
                  telefono = participanteSistema.telefono,
                  empArea = participanteSistema.empArea,
                  boton = "<a class='btn btn-danger btn-sm glyphicon glyphicon-trash'></a>",
              }
              );

            var jsonResult = Json(new { data = resultset }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpPost]
        public ActionResult agregarParticipanteExterno(int idR06, string nombre, string telefono, string empArea)
        {

            ParticipantesReunion participanteExterno = new ParticipantesReunion();

            List<object> resultset = new List<object>();

            participanteExterno.nombre = nombre;
            participanteExterno.telefono = telefono;
            participanteExterno.empArea = empArea;
            participanteExterno.firma = "";

   
            if (idR06 != 0)
            {
                R06_ActaReunion actaReunion = db.R06_ActaReunion.Find(idR06);
                participanteExterno.R06 = actaReunion;
            }
            db.ParticipantesReunion.Add(participanteExterno);
            db.SaveChanges();

            resultset.Add(
              new
              {
                  nombre = participanteExterno.nombre,
                  telefono = participanteExterno.telefono,
                  empArea = participanteExterno.empArea,
                  boton = "<a class='btn btn-danger btn-sm glyphicon glyphicon-trash'></a>",
              }
              );

            var jsonResult = Json(new { data = resultset }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpPost]
        public bool eliminarParticipante(int idR06, string nombre)
        {
            string nombreClean = nombre.Trim();
            if (nombre != null)
            {

                ParticipantesReunion participante = db.ParticipantesReunion.Where(p => p.nombre == nombreClean).Where(p => p.R06 == null).FirstOrDefault();

                if (idR06 != 0)
                {
                    participante = db.ParticipantesReunion.Where(p => p.nombre == nombreClean).Where(p => p.R06.idR06 == idR06).FirstOrDefault();
                }
                db.ParticipantesReunion.Remove(participante);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        public void clearParticipantesReunionList()
        {
            var data = db.ParticipantesReunion.ToList();
            foreach (ParticipantesReunion participante in data)
            {
                if (participante.R06 == null)
                {
                    db.ParticipantesReunion.Remove(participante);
                    db.SaveChanges();
                }

            }
        }

        public SelectList GetPosiblesParticipantes()
        {
            List<AspNetUsers> participantes = db.AspNetUsers
            .Where(x => x.AspNetRoles.Any(y => y.Name.Contains("Relator")
            || y.Name.Contains("DigitaciónYPostCurso")
            || y.Name.Contains("Representante Empresa")
            || y.Name.Contains("Lider Comercial")
             || y.Name.Contains("Diseño & Desarrollo")
              || y.Name.Contains("Facturacion")
               || y.Name.Contains("Administrador")
                || y.Name.Contains("APOYO TMS")

            )).OrderBy(x => x.nombres).ToList();

            return new SelectList(participantes.Select(c => new SelectListItem
            {
                Text = c.nombreCompleto + "    [" + c.UserName + "]",
                Value = c.Id.ToString()
            }).ToList(), "Value", "Text");
        }

        public List<ParticipantesReunion> GetParticipantes(int? id)
        {

            List<ParticipantesReunion> participantes = new List<ParticipantesReunion>();
            foreach (ParticipantesReunion item in db.ParticipantesReunion)
            {
                if (item.R06.idR06 == id)
                {
                    participantes.Add(item);
                }
            }

            return participantes;
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
