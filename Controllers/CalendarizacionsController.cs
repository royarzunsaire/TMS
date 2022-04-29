using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Helpers;
using SGC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class CalendarizacionsController : Controller
    {

        private InsecapContext db = new InsecapContext();
        private SelectList Cursos;
        


        public CalendarizacionsController()
        {
            // obtener todos los cursos
           
            Cursos = new SelectList(db.Curso.Where(c => c.softDelete == false)
                .Select(c => new SelectListItem
            {
                Text = "[" + c.tipoEjecucion + db.R11.Where(x => x.idCurso == c.idCurso).Select(x => " "+ (x.horasPracticas + x.horasTeoricas)+ " horas").FirstOrDefault()+ "] " + c.nombreCurso,
                Value = c.idCurso.ToString()
            }).ToList(), "Value", "Text");
        }

        // GET: Calendarizacions
        [CustomAuthorize(new string[] { "/Calendarizacions/" })]
        public ActionResult Index()
        {
            return View(db.Calendarizacions.ToList());
        }

        // GET: Calendario/id
        [CustomAuthorize(new string[] { "/Calendarizacions/" })]
        public ActionResult Calendario(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Calendarizacion calendarizacion = db.Calendarizacions.Find(id);
            if (calendarizacion == null)
            {
                return HttpNotFound();
            }

            List<Calendarizacion> Calendarizaciones = db.Calendarizacions.ToList();
            //CalendarizacionAbierta calendarizacionAbierta = new CalendarizacionAbierta();

            ViewModelCalendario calendario = new ViewModelCalendario();
            calendario.calendarizacion = calendarizacion;
            //calendario.calendarizacionAbierta = calendarizacionAbierta;
            calendario.calendarizaciones = Calendarizaciones;

            List<FullCalendarEvent> eventos = new List<FullCalendarEvent>();

            foreach (CalendarizacionAbierta item in calendario.calendarizacion.calendarizacionesAbiertas)
            {
                FullCalendarEvent evento = new FullCalendarEvent();
                var r11 = db.R11.Where(x => x.idCurso == item.curso.idCurso).FirstOrDefault();
                string horas = "";
                if (r11 != null) {
                    horas = " "+(r11.horasPracticas + r11.horasTeoricas) + " horas";
                }
                evento.title = "[" + item.curso.tipoEjecucion + horas+ "] " + item.curso.nombreCurso;
                if (item.descripcion != null)
                {
                    evento.description = item.descripcion;
                }
                else
                {
                    evento.description = "";
                }
                evento.start = item.fechaInicio.ToString("yyyy-MM-dd");
                evento.end = item.fechaTermino.AddDays(1).ToString("yyyy-MM-dd");
                evento.color = item.colorEvento.ToString();
                eventos.Add(evento);
            }

            calendario.eventosJson = Newtonsoft.Json.JsonConvert.SerializeObject(eventos);

            ViewBag.eventos = eventos;
            ViewBag.Cursos = Cursos;
            return View(calendario);
        }

        // POST: CalendarizacionAbiertas/CrearAbierto
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Calendarizacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearAbierto(ViewModelCalendario calendario)
        {
            if (ModelState.IsValid)
            {
                // validar fecha inicio menor a fecha termino no futura
                if (DateTime.Compare((DateTime)calendario.calendarizacionAbierta.fechaInicio, (DateTime)calendario.calendarizacionAbierta.fechaTermino) > 0)
                {
                    ModelState.AddModelError("fechaInicio", "La fecha de inicio debe ser anterior a la fecha de término");
                }
                else
                {
                    // obtener calendarizacion
                    calendario.calendarizacion = db.Calendarizacions.Find(calendario.calendarizacion.idCalendarizacion);
                    // validar fechas dentro de periodo
                    if (DateTime.Compare((DateTime)calendario.calendarizacionAbierta.fechaInicio, (DateTime)calendario.calendarizacion.finPeriopdo) > 0)
                    {
                        ModelState.AddModelError("fechaInicio", "La fecha de inicio debe encontrarse dentro del periodo");
                    }
                    else
                    {
                        if (DateTime.Compare((DateTime)calendario.calendarizacion.inicioPeriodo, (DateTime)calendario.calendarizacionAbierta.fechaInicio) > 0)
                        {
                            ModelState.AddModelError("fechaInicio", "La fecha de inicio debe encontrarse dentro del periodo");
                        }
                        else
                        {
                            if (DateTime.Compare((DateTime)calendario.calendarizacionAbierta.fechaTermino, (DateTime)calendario.calendarizacion.finPeriopdo) > 0)
                            {
                                ModelState.AddModelError("fechaInicio", "La fecha de termino debe encontrarse dentro del periodo");
                            }
                            else
                            {
                                if (DateTime.Compare((DateTime)calendario.calendarizacion.inicioPeriodo, (DateTime)calendario.calendarizacionAbierta.fechaTermino) > 0)
                                {
                                    ModelState.AddModelError("fechaInicio", "La fecha de inicio debe encontrarse dentro del periodo");
                                }
                                else
                                {
                                    // obtener curso
                                    Curso curso = db.Curso.Find(calendario.calendarizacionAbierta.curso.idCurso);
                                    // agregar datos a calendarizacion abierta
                                    calendario.calendarizacionAbierta.curso = curso;
                                    // codigo consolidacion
                                    var ultimoCodigo = "0";
                                    if (db.CalendarizacionAbierta.OrderByDescending(x => x.idCalendarizacionAbierta).FirstOrDefault() != null)
                                    {
                                        ultimoCodigo = db.CalendarizacionAbierta.OrderByDescending(x => x.idCalendarizacionAbierta).FirstOrDefault().codigoConsolidacion;
                                    }
                                    calendario.calendarizacionAbierta.codigoConsolidacion = CustomUtilsHelper.GeneracionCodigo("CON", ultimoCodigo);
                                    calendario.calendarizacionAbierta.usuarioCreador = User.Identity.GetUserId();
                                    calendario.calendarizacionAbierta.fechaCreacion = DateTime.Now;
                                    // agregar calendarizacion abierta a calendarizacion
                                    calendario.calendarizacion.calendarizacionesAbiertas.Add(calendario.calendarizacionAbierta);
                                    // guardar los cambios
                                    calendario.calendarizacion.sucursal = db.Sucursal.Find(calendario.calendarizacion.sucursal.idSucursal);
                                    db.Entry(calendario.calendarizacion).State = EntityState.Modified;
                                    db.SaveChanges();
                                    return Json(new { HasErrors = false, responseText = "OK" });
                                }
                            }
                        }
                    }
                }
            }
            // enviar los errores al ajax
            var errors = ModelState.ToDictionary(kvp => kvp.Key,
                    kvp => kvp.Value.Errors
                  .Select(e => e.ErrorMessage).ToArray())
                  .Where(m => m.Value.Count() > 0);
            return Json(new { HasErrors = true, Errors = errors });
        }

        // POST: CalendarizacionAbiertas/EliminarAbierto/5
        [CustomAuthorize(new string[] { "/Calendarizacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarAbierto()
        {
            CalendarizacionAbierta calendarizacionAbierta = db.CalendarizacionAbierta.Find(int.Parse(Request["id"]));
            db.CalendarizacionAbierta.Remove(calendarizacionAbierta);
            db.SaveChanges();
            return RedirectToAction("Calendario", new { id = int.Parse(Request["idCalendarizacion"]) });
        }

        // GET: Calendarizacions/Details/5
        [CustomAuthorize(new string[] { "/Calendarizacions/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Calendarizacion calendarizacion = db.Calendarizacions.Find(id);
            if (calendarizacion == null)
            {
                return HttpNotFound();
            }
            return View(calendarizacion);
        }

        // GET: Calendarizacions/Create
        [CustomAuthorize(new string[] { "/Calendarizacions/", "/Calendarizacions/Create/" })]
        public ActionResult Create()
        {
            ViewBag.sucursales = GetSucursales();
            return View();
        }

        // POST: Calendarizacions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Calendarizacions/", "/Calendarizacions/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idCalendarizacion,detalle,inicioPeriodo,finPeriopdo,sucursal")] Calendarizacion calendarizacion)
        {
            calendarizacion.sucursal = db.Sucursal.Find(calendarizacion.sucursal.idSucursal);
            if (ModelState.IsValid)
            {
                calendarizacion.fechaCreacion = DateTime.Now;
                calendarizacion.usuarioCreador = User.Identity.GetUserId();
                db.Calendarizacions.Add(calendarizacion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.sucursales = GetSucursales();
            return View(calendarizacion);
        }

        // GET: Calendarizacions/Edit/5
        [CustomAuthorize(new string[] { "/Calendarizacions/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Calendarizacion calendarizacion = db.Calendarizacions.Find(id);
            if (calendarizacion == null)
            {
                return HttpNotFound();
            }
            ViewBag.sucursales = GetSucursales();
            return View(calendarizacion);
        }

        // POST: Calendarizacions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Calendarizacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idCalendarizacion,detalle,inicioPeriodo,finPeriopdo,sucursal")] Calendarizacion calendarizacion)
        {
            if (ModelState.IsValid)
            {
                Calendarizacion calendarizacionOriginal = db.Calendarizacions.Find(calendarizacion.idCalendarizacion);

                calendarizacionOriginal.sucursal = db.Sucursal.Find(calendarizacion.sucursal.idSucursal);
                calendarizacionOriginal.detalle = calendarizacion.detalle;
                calendarizacionOriginal.inicioPeriodo = calendarizacion.inicioPeriodo;
                calendarizacionOriginal.finPeriopdo = calendarizacion.finPeriopdo;
                calendarizacionOriginal.usuarioCreador = User.Identity.GetUserId();
                calendarizacionOriginal.fechaCreacion = DateTime.Now;

                db.Entry(calendarizacionOriginal).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.sucursales = GetSucursales();
            return View(calendarizacion);
        }

        //// GET: Calendarizacions/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Calendarizacion calendarizacion = db.Calendarizacions.Find(id);
        //    if (calendarizacion == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(calendarizacion);
        //}

        // POST: Calendarizacions/Delete/5
        [CustomAuthorize(new string[] { "/Calendarizacions/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Calendarizacion calendarizacion = db.Calendarizacions.Find(id);
            int cantCalendarizacionesAbiertas = calendarizacion.calendarizacionesAbiertas.Count();
            for (int i = 0; i < cantCalendarizacionesAbiertas; i++)
            {
                db.CalendarizacionAbierta.Remove(calendarizacion.calendarizacionesAbiertas.ElementAt<CalendarizacionAbierta>(0));
            }
            db.Calendarizacions.Remove(calendarizacion);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private SelectList GetSucursales()
        {
            return new SelectList(db.Sucursal.Select(s => new SelectListItem
            {
                Text = s.nombre,
                Value = s.idSucursal.ToString()
            }).ToList(), "Value", "Text");
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
