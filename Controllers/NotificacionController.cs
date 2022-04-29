using Microsoft.AspNet.Identity;
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
    [Authorize]
    public class NotificacionController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: Notificacion
        public async Task<ActionResult> Index()
        {
            return View(await db.Notificacion.ToListAsync());
        }

        // GET: Notificacion/List/
        public async Task<ActionResult> List()
        {
            var id = User.Identity.GetUserId();
            // obtener notificaciones
            List<Notificacion> notificaciones = await db.Notificacion
                .Where(n => n.usuario.Id == id)
                .ToListAsync();
            List<Notificacion> notificacionesEnviar = new List<Notificacion>();
            foreach (var item in notificaciones)
            {
                if (item.estado.Last().nombre != NombreEstadoNotificacion.Anulado)
                {
                    notificacionesEnviar.Add(item);
                }
            }
            return View(notificacionesEnviar.OrderByDescending(n => n.fechaCreacion).ToList());
        }

        // GET: Notificacion/Notificaciones/
        public async Task<ActionResult> Notificaciones()
        {
            var id = User.Identity.GetUserId();
            // obtener notificaciones
            List<Notificacion> notificaciones = await db.Notificacion
                .Where(n => n.usuario.Id == id)
                .ToListAsync();
            List<Notificacion> notificacionesFiltroEstado = new List<Notificacion>();
            foreach (var item in notificaciones)
            {
                if (item.estado.Last().nombre != NombreEstadoNotificacion.Anulado
                    && item.estado.Last().nombre != NombreEstadoNotificacion.Leido)
                {
                    notificacionesFiltroEstado.Add(item);
                }
            }
            // select de los datos 
            var notificacionesEnviar = notificacionesFiltroEstado.Select(n => new
            {
                n.idNotificacion,
                n.titulo,
                n.mensaje,
                n.url,
                n.fechaCreacion,
                n.estado.LastOrDefault().nombre,
                n.color,
                n.tipo
            }).ToList();
            notificacionesEnviar = notificacionesEnviar.OrderByDescending(n => n.fechaCreacion).ToList();
            return Json(new
            {
                notificaciones = Newtonsoft.Json.JsonConvert.SerializeObject(notificacionesEnviar)
            });
        }

        // POST: Notificacion/MarcarVistas/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarcarVistas()
        {
            if (Request["idUsuario"] == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string idUsuario = Request["idUsuario"];
            List<Notificacion> notificaciones = await db.Notificacion
                .Where(n => n.usuario.Id == idUsuario)
                .ToListAsync();
            List<Notificacion> notificacionesFiltroEstado = new List<Notificacion>();
            foreach (var item in notificaciones)
            {
                if (item.estado.Last().nombre == NombreEstadoNotificacion.Enviado)
                {
                    notificacionesFiltroEstado.Add(item);
                }
            }
            foreach (var item in notificacionesFiltroEstado)
            {
                EstadoNotificacion estado = new EstadoNotificacion();
                estado.nombre = NombreEstadoNotificacion.Visto;
                estado.fecha = DateTime.Now;
                item.estado.Add(estado);
                db.Entry(item).State = EntityState.Modified;
            }
            await db.SaveChangesAsync();
            return Json(new { });
        }

        // POST: Notificacion/MarcarLeido/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarcarLeido()
        {
            if (Request["idUsuario"] == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (Request["idNotificacion"] == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var idNotificacion = int.Parse(Request["idNotificacion"]);
            Notificacion notificacion = await db.Notificacion
                .Where(n => n.idNotificacion == idNotificacion)
                .FirstOrDefaultAsync();
            EstadoNotificacion estado = new EstadoNotificacion();
            estado.nombre = NombreEstadoNotificacion.Leido;
            estado.fecha = DateTime.Now;
            notificacion.estado.Add(estado);
            db.Entry(notificacion).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Redirect(notificacion.url);
        }

        //// GET: Notificacion/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Notificacion notificacion = await db.Notificacion.FindAsync(id);
        //    if (notificacion == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(notificacion);
        //}

        //// GET: Notificacion/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Notificacion/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "idNotificacion,titulo,mensaje,url,tipo,fechaEstado,color,estado,fechaCreacion,usuarioCreador")] Notificacion notificacion)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Notificacion.Add(notificacion);
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    return View(notificacion);
        //}

        //// GET: Notificacion/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Notificacion notificacion = await db.Notificacion.FindAsync(id);
        //    if (notificacion == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(notificacion);
        //}

        //// POST: Notificacion/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "idNotificacion,titulo,mensaje,url,tipo,fechaEstado,color,estado,fechaCreacion,usuarioCreador")] Notificacion notificacion)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(notificacion).State = EntityState.Modified;
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    return View(notificacion);
        //}

        //// GET: Notificacion/Delete/5
        //public async Task<ActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Notificacion notificacion = await db.Notificacion.FindAsync(id);
        //    if (notificacion == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(notificacion);
        //}

        //// POST: Notificacion/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(int id)
        //{
        //    Notificacion notificacion = await db.Notificacion.FindAsync(id);
        //    db.Notificacion.Remove(notificacion);
        //    await db.SaveChangesAsync();
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
