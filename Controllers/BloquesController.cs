using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class BloquesController : Controller
    {
        private InsecapContext db = new InsecapContext();
        private Regex urlchk = new Regex(@"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        //// GET: Bloques
        //public ActionResult Index()
        //{
        //    return View(db.Bloque.ToList());
        //}

        // GET: Bloques/List/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult List(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comercializacion comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            return View(comercializacion);
        }


        // GET: Bloques/PizarraAeropuerto
        [CustomAuthorize(new string[] { "/Bloques/PizarraAeropuerto/" })]
        public ActionResult PizarraAeropuerto()
        {
            ViewBag.date = DateTime.Now.Date;
            ViewBag.sucursales = GetSucursales();
            return View();
        }
        public SelectList GetSalas()
        {
            List<Sala> salas = db.Sala.OrderBy(x => x.nombre).ToList();


            return new SelectList(salas.Select(c => new SelectListItem
            {
                Text = c.nombre,
                Value = c.idSala.ToString()
            }).ToList(), "Value", "Text");
        }

        // GET: Bloques/PizarraAeropuertoRecargar/5
        [CustomAuthorize(new string[] { "/Bloques/PizarraAeropuerto/" })]
        public ActionResult PizarraAeropuertoRecargar(string id,string fecha)
        {
            var hoy = Convert.ToDateTime(fecha);
            var bloques = new List<Bloque>();
            if (id != "" && id != null)
            {
                var idSucursalInt = int.Parse(id);
                bloques = db.Bloque
                    .Where(b => DbFunctions.TruncateTime(b.fecha) == DbFunctions.TruncateTime(hoy))
                    .Where(b => b.comercializacion.cotizacion.sucursal.idSucursal == idSucursalInt)
                    .Where(b => b.comercializacion.comercializacionEstadoComercializacion
                    .Any(y => y.EstadoComercializacion == EstadoComercializacion.Cancelada
                   || y.EstadoComercializacion == EstadoComercializacion.Borrador
                   || y.EstadoComercializacion == EstadoComercializacion.Deshabilitada) == false)
                    .Where(b => b.comercializacion.softDelete == false)
                    .ToList();
            }
            else
            {
                bloques = db.Bloque
                    .Where(b => DbFunctions.TruncateTime(b.fecha) == DbFunctions.TruncateTime(hoy))
                    .Where(b => b.comercializacion.comercializacionEstadoComercializacion
                    .Any(y => y.EstadoComercializacion == EstadoComercializacion.Cancelada
                   || y.EstadoComercializacion == EstadoComercializacion.Borrador
                   || y.EstadoComercializacion == EstadoComercializacion.Deshabilitada) == false)
                    .Where(b => b.comercializacion.softDelete == false)
                    .ToList();
            }
            var pizarraAeropuerto = new List<PizarraAeropuertoVM>();
            ViewBag.coordinadores = GetCoordinador();
            ViewBag.salas = GetSalas();
            foreach (var item in bloques)
            {
                if (item.comercializacion.cotizacion.modalidad != null)
                {
                    if (item.comercializacion.cotizacion.modalidad == "Cerrado")
                    {
                        var itemPizarra = pizarraAeropuerto
                            .Where(b => b.bloque.fecha == item.fecha)
                            .Where(b => b.bloque.sala == item.sala)
                            .Where(b => b.bloque.relator == item.relator)
                            .Where(b => b.bloque.comercializacion.idComercializacion == item.comercializacion.idComercializacion)
                            .FirstOrDefault();
                        if (itemPizarra == null)
                        {
                            var nuevoItemPizarra = new PizarraAeropuertoVM();
                            nuevoItemPizarra.bloque = item;
                            nuevoItemPizarra.curso = item.comercializacion.cotizacion.curso;
                            nuevoItemPizarra.cliente = item.comercializacion.cotizacion.cliente;
                            nuevoItemPizarra.cantParticipantes = (int)item.comercializacion.cotizacion.cantidadParticipante;
                            nuevoItemPizarra.costo = db.Costo.Where(c => c.idCotizacion == item.comercializacion.cotizacion.idCotizacion_R13).ToList();

                            pizarraAeropuerto.Add(nuevoItemPizarra);
                        }
                        else
                        {
                            if (DateTime.Compare(item.horarioInicio, itemPizarra.bloque.horarioInicio) < 0)
                            {
                                itemPizarra.bloque.horarioInicio = item.horarioInicio;
                            }
                            if (DateTime.Compare(item.horarioTermino, itemPizarra.bloque.horarioTermino) > 0)
                            {
                                itemPizarra.bloque.horarioTermino = item.horarioTermino;
                            }
                        }
                    }
                    else
                    {
                        var itemPizarra = pizarraAeropuerto
                            .Where(b => b.bloque.fecha == item.fecha)
                            .Where(b => b.bloque.sala == item.sala)
                            .Where(b => b.bloque.relator == item.relator)
                          
                            .FirstOrDefault();
                        if (itemPizarra == null)
                        {
                            var nuevoItemPizarra = new PizarraAeropuertoVM();
                            nuevoItemPizarra.bloque = item;
                            nuevoItemPizarra.curso = item.comercializacion.cotizacion.curso;
                            nuevoItemPizarra.cliente = item.comercializacion.cotizacion.cliente;
                            nuevoItemPizarra.cantParticipantes = (int)item.comercializacion.cotizacion.cantidadParticipante;
                            nuevoItemPizarra.costo = db.Costo.Where(c => c.idCotizacion == item.comercializacion.cotizacion.idCotizacion_R13).ToList();

                            pizarraAeropuerto.Add(nuevoItemPizarra);
                        }
                        else
                        {
                            itemPizarra.cantParticipantes += (int)item.comercializacion.cotizacion.cantidadParticipante;
                            if (DateTime.Compare(item.horarioInicio, itemPizarra.bloque.horarioInicio) < 0)
                            {
                                itemPizarra.bloque.horarioInicio = item.horarioInicio;
                            }
                            if (DateTime.Compare(item.horarioTermino, itemPizarra.bloque.horarioTermino) > 0)
                            {
                                itemPizarra.bloque.horarioTermino = item.horarioTermino;
                            }
                        }
                    }
                }
            }
            var moodle = db.ParametrosMoodles.FirstOrDefault();
            ViewBag.moodle = moodle != null ? moodle.urlMoodle : "";
            return View(pizarraAeropuerto);
        }

        // GET: Bloques/PizarraTriangular
        [CustomAuthorize(new string[] { "/Bloques/PizarraTriangular/" })]
        public ActionResult PizarraTriangular()
        {
            ViewBag.sucursales = GetSucursales();
            return View();
        }

        // GET: Bloques/PizarraTriangularRecargar
        [CustomAuthorize(new string[] { "/Bloques/PizarraTriangular/" })]
        public ActionResult PizarraTriangularRecargar(string id)
        {
            var hoy = DateTime.Now.Date;
            var bloques = new List<Bloque>();
            var dateOld = DateTime.Now.Date.AddMonths(-2);
            var date = DateTime.Now.Date.AddMonths(1);
            if (id != "" && id != null)
            {
                var idSucursalInt = int.Parse(id);
                bloques = db.Bloque
                    .Where(b => b.comercializacion.cotizacion.sucursal.idSucursal == idSucursalInt)
                    .Where(b => b.comercializacion.softDelete == false)
                    .Where(b => b.comercializacion.comercializacionEstadoComercializacion
                    .Any( y => y.EstadoComercializacion == EstadoComercializacion.Cancelada 
                    || y.EstadoComercializacion == EstadoComercializacion.Borrador 
                    || y.EstadoComercializacion == EstadoComercializacion.Deshabilitada) == false)
                    .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaCreacion) >= dateOld)
                    .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaCreacion) <= date)
                    .Where(b => b.comercializacion.softDelete == false)
                    .ToList();
            }
            else
            {
                bloques = db.Bloque
                    .Where(b => b.comercializacion.softDelete == false)
                    .Where(b => b.comercializacion.comercializacionEstadoComercializacion
                    .Any(y => y.EstadoComercializacion == EstadoComercializacion.Cancelada
                   || y.EstadoComercializacion == EstadoComercializacion.Borrador
                   || y.EstadoComercializacion == EstadoComercializacion.Deshabilitada) == false)
                    .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaCreacion) >= dateOld)
                    .Where(x => DbFunctions.TruncateTime(x.comercializacion.fechaCreacion) <= date)
                    .Where(b => b.comercializacion.softDelete == false)
                    .ToList();
            }
            var pizarraTriangular = new List<PizarraTriangularVM>();
            foreach (var item in bloques)
            {
                var itemPizarra = pizarraTriangular
                    //.Where(p => p.fecha == item.comercializacion.fechaInicio)
                    .Where(p => p.fecha == item.fecha)
                    .FirstOrDefault();


                if (itemPizarra == null)
                {
                    var nuevoItemPizarra = new PizarraAeropuertoVM();
                    nuevoItemPizarra.bloque = item;
                    if (item.comercializacion.cotizacion.curso != null)
                    {
                        nuevoItemPizarra.curso = item.comercializacion.cotizacion.curso;
                    }
                    nuevoItemPizarra.cliente = item.comercializacion.cotizacion.cliente;
                    nuevoItemPizarra.cantParticipantes = (int)item.comercializacion.cotizacion.cantidadParticipante;
                    itemPizarra = new PizarraTriangularVM();
                    //itemPizarra.fecha = item.comercializacion.fechaInicio;
                    itemPizarra.fecha = item.fecha;
                    itemPizarra.bloques = new List<PizarraAeropuertoVM>();
                    itemPizarra.bloques.Add(nuevoItemPizarra);
                    pizarraTriangular.Add(itemPizarra);
                }
                else
                {
                    var itemPizarraExistente = itemPizarra.bloques
                        .Where(b => b.bloque.fecha == item.fecha)
                        .Where(b => b.bloque.sala == item.sala)
                        .Where(b => b.bloque.relator == item.relator)
                        .Where(b => b.bloque.comercializacion.idComercializacion == item.comercializacion.idComercializacion)
                        .FirstOrDefault();
                    if (itemPizarraExistente == null)
                    {
                        var nuevoItemPizarra = new PizarraAeropuertoVM();
                        nuevoItemPizarra.bloque = item;
                        if (item.comercializacion.cotizacion.curso != null) {
                            nuevoItemPizarra.curso = item.comercializacion.cotizacion.curso;
                        }
                       
                        nuevoItemPizarra.cliente = item.comercializacion.cotizacion.cliente;
                        nuevoItemPizarra.cantParticipantes = (int)item.comercializacion.cotizacion.cantidadParticipante;
                        itemPizarra.bloques.Add(nuevoItemPizarra);
                    }
                    else
                    {
                        if (DateTime.Compare(item.horarioInicio, itemPizarraExistente.bloque.horarioInicio) < 0)
                        {
                            itemPizarraExistente.bloque.horarioInicio = item.horarioInicio;
                        }
                        if (DateTime.Compare(item.horarioTermino, itemPizarraExistente.bloque.horarioTermino) > 0)
                        {
                            itemPizarraExistente.bloque.horarioTermino = item.horarioTermino;
                        }
                    }
                }
            }
            //return View(bloques);
            return View(pizarraTriangular);
        }

        // GET: Bloques/Details/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bloque bloque = db.Bloque.Find(id);
            if (bloque == null)
            {
                return HttpNotFound();
            }
            return View(bloque);
        }

        // GET: Bloques/Create/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comercializacion comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            if (comercializacion.comercializacionEstadoComercializacion.LastOrDefault().EstadoComercializacion == SGC.Models.EstadoComercializacion.Terminada
                && comercializacion.comercializacionEstadoComercializacion.LastOrDefault().EstadoComercializacion == SGC.Models.EstadoComercializacion.Cancelada
                && comercializacion.comercializacionEstadoComercializacion.LastOrDefault().EstadoComercializacion == SGC.Models.EstadoComercializacion.Deshabilitada)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.salas = GetSalas(null);
            ViewBag.coordinador = GetCoordinador();
            ViewBag.lugaresAlmuerzo = GetLugaresAlmuerzo(null);
            ViewBag.relatores = GetRelatores(comercializacion);
            ViewBag.idComercializacion = comercializacion.idComercializacion;
            Bloque bloque = new Bloque { fecha=comercializacion.fechaInicio};
            return View(bloque);
        }

        // POST: Bloques/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idBloque,horarioInicio,horarioTermino,fecha,linkAutomatic,linkManual")] Bloque bloque)
        {
            Comercializacion comercializacion = db.Comercializacion.Find(int.Parse(Request["idComercializacion"]));
            // verificar is se selecciono un relator

            if (Request["relator.idRelator"] == "" && comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                ModelState.AddModelError("relator", "Se debe seleccionar un Relator");
            }
            else if(comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                bloque.relator = db.Relators.Find(int.Parse(Request["relator.idRelator"]));
            }
            // verificar is se selecciono una sala
            if (Request["sala.idSala"] == "")
            {
                ModelState.AddModelError("sala", "Se debe seleccionar una Sala");
            }
            else
            {
                bloque.sala = db.Sala.Find(int.Parse(Request["sala.idSala"]));
            }
            // verificar is se selecciono un lugar de almuerzo
            if (Request["lugarAlmuerzo.idLugarAlmuerzo"] == "")
            {
                ModelState.AddModelError("lugarAlmuerzo", "Se debe seleccionar un Lugar de Almuerzo");
            }
            else
            {
                bloque.lugarAlmuerzo = db.LugarAlmuerzo.Find(int.Parse(Request["lugarAlmuerzo.idLugarAlmuerzo"]));
            }
            // verificar que la fecha se encuentre dentro del periodo de la comercializacion
            if (DateTime.Compare((DateTime)bloque.fecha, (DateTime)comercializacion.fechaTermino) > 0)
            {
                ModelState.AddModelError("fecha", "La fecha debe encontrarse dentro del periodo de la comercialización");
            }
            if (DateTime.Compare((DateTime)comercializacion.fechaInicio, (DateTime)bloque.fecha) > 0)
            {
                ModelState.AddModelError("fecha", "La fecha debe encontrarse dentro del periodo de la comercialización");
            }
            // validar hora inicio anterior a hora termino
            if (DateTime.Compare((DateTime)bloque.horarioInicio, (DateTime)bloque.horarioTermino) >= 0)
            {
                ModelState.AddModelError("horarioInicio", "La hora de inicio debe ser anterior a la hora de término");
            }
            if (Request["coordinador.Id"] != "")
            {
                bloque.coordinador = db.AspNetUsers.Find(Request["coordinador.Id"]);
            }
     
            if (ModelState.IsValid)
            {
                bloque.comercializacion = comercializacion;
                comercializacion.bloques.Add(bloque); 
                //db.Comercializacion.Add(comercializacion);
                db.Entry(comercializacion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("List", new { id = comercializacion.idComercializacion });
            }
            ViewBag.salas = GetSalas(null);
            ViewBag.coordinador = GetCoordinador();
            ViewBag.lugaresAlmuerzo = GetLugaresAlmuerzo(null);
            ViewBag.relatores = GetRelatores(comercializacion);
            ViewBag.idComercializacion = comercializacion.idComercializacion;
            return View(bloque);
        }


        // GET: Bloques/Edit/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bloque bloque = db.Bloque.Find(id);
            if (bloque == null)
            {
                return HttpNotFound();
            }
            if (bloque.comercializacion.comercializacionEstadoComercializacion.LastOrDefault().EstadoComercializacion == SGC.Models.EstadoComercializacion.Terminada
                && bloque.comercializacion.comercializacionEstadoComercializacion.LastOrDefault().EstadoComercializacion == SGC.Models.EstadoComercializacion.Cancelada
                && bloque.comercializacion.comercializacionEstadoComercializacion.LastOrDefault().EstadoComercializacion == SGC.Models.EstadoComercializacion.Deshabilitada)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.salas = GetSalas(null);
            ViewBag.coordinador = GetCoordinador();
            ViewBag.lugaresAlmuerzo = GetLugaresAlmuerzo(null);
            ViewBag.relatores = GetRelatores(bloque.comercializacion);
            ViewBag.idComercializacion = bloque.comercializacion.idComercializacion;
            return View(bloque);
        }

        // POST: Bloques/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idBloque,horarioInicio,horarioTermino,fecha,linkAutomatic,linkManual")] Bloque bloque)
        {
            Comercializacion comercializacion = db.Comercializacion.Find(int.Parse(Request["idComercializacion"]));
            // verificar is se selecciono un relator
            if (Request["relator.idRelator"] == "" && comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                ModelState.AddModelError("relator", "Se debe seleccionar un Relator");
            }
            else if (comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
            {
                bloque.relator = db.Relators.Find(int.Parse(Request["relator.idRelator"]));
            }
            // verificar is se selecciono una sala
            if (Request["sala.idSala"] == "")
            {
                ModelState.AddModelError("sala", "Se debe seleccionar una Sala");
            }
            else
            {
                bloque.sala = db.Sala.Find(int.Parse(Request["sala.idSala"]));
            }
            // verificar is se selecciono un lugar de almuerzo
            if (Request["lugarAlmuerzo.idLugarAlmuerzo"] == "")
            {
                ModelState.AddModelError("lugarAlmuerzo", "Se debe seleccionar un Lugar de Almuerzo");
            }
            else
            {
                bloque.lugarAlmuerzo = db.LugarAlmuerzo.Find(int.Parse(Request["lugarAlmuerzo.idLugarAlmuerzo"]));
            }
            // verificar que la fecha se encuentre dentro del periodo de la comercializacion
            if (DateTime.Compare((DateTime)bloque.fecha, (DateTime)comercializacion.fechaTermino) > 0)
            {
                ModelState.AddModelError("fecha", "La fecha debe encontrarse dentro del periodo de la comercialización");
            }
            if (DateTime.Compare((DateTime)comercializacion.fechaInicio, (DateTime)bloque.fecha) > 0)
            {
                ModelState.AddModelError("fecha", "La fecha debe encontrarse dentro del periodo de la comercialización");
            }
            // validar hora inicio anterior a hora termino
            if (DateTime.Compare((DateTime)bloque.horarioInicio, (DateTime)bloque.horarioTermino) >= 0)
            {
                ModelState.AddModelError("horarioInicio", "La hora de inicio debe ser anterior a la hora de término");
            }
            if (Request["idCoordinador"] != "")
            {
                bloque.coordinador = db.AspNetUsers.Find(Request["idCoordinador"]);
            }

            if (ModelState.IsValid)
            {
                //bloque.comercializacion = comercializacion;
                Bloque bloqueBD = comercializacion.bloques.Where(b => b.idBloque == bloque.idBloque).FirstOrDefault();
                bloqueBD.fecha = bloque.fecha;
                bloqueBD.horarioInicio = bloque.horarioInicio;
                bloqueBD.horarioTermino = bloque.horarioTermino;
                bloqueBD.lugarAlmuerzo = bloque.lugarAlmuerzo;
                bloqueBD.relator = bloque.relator;
                bloqueBD.sala = bloque.sala;
                bloqueBD.coordinador = bloque.coordinador;
                db.Entry(bloqueBD).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("List", new { id = comercializacion.idComercializacion });
            }
            ViewBag.salas = GetSalas(null);
            ViewBag.coordinador = GetCoordinador();
            ViewBag.lugaresAlmuerzo = GetLugaresAlmuerzo(null);
            ViewBag.relatores = GetRelatores(comercializacion);
            ViewBag.idComercializacion = comercializacion.idComercializacion;
            return View(bloque);
        }

        // POST: Bloques/Delete/5
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Bloque bloque = db.Bloque.Find(id);
            int idComercializacion = bloque.comercializacion.idComercializacion;
            var asistencias = db.Asistencias.Where(x => x.bloque.idBloque == bloque.idBloque).ToList();
            db.Asistencias.RemoveRange(asistencias);
            db.Bloque.Remove(bloque);
            db.SaveChanges();
            return RedirectToAction("List", new { id = idComercializacion });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public SelectList GetSalas(Bloque bloque)
        {
            List<Sala> salas = db.Sala.Where(x => x.softDelete == false).ToList();
            if (bloque != null)
            {
                Sala sala = bloque.sala;
                if (!salas.Contains(sala) && sala != null)
                {
                    salas.Add(sala);
                }
            }
            return new SelectList(salas.Select(c => new SelectListItem
            {
                Text = c.nombre,
                Value = c.idSala.ToString()
            }).ToList(), "Value", "Text");
        }
        public SelectList GetCoordinador()
        {
            List<AspNetUsers> coordinadores = db.AspNetUsers
                .Where(x => x.AspNetRoles.Any(y => y.Name.Contains("Lider Comercial")
                || y.Name.Contains("DigitaciónYPostCurso")
                 || y.Name.Contains("Administrador")
                  || y.Name.Contains("APOYO TMS")
                   || y.Name.Contains("Diseño & Desarrollo")
                    || y.Name.Contains("Facturacion")

                )).OrderBy(x => x.nombres).ToList();

            
            return new SelectList(coordinadores.Select(c => new SelectListItem
            {
                Text = c.nombreCompleto + " [" + c.UserName + "]",
                Value = c.Id.ToString()
            }).ToList(), "Value", "Text");
        }

        public SelectList GetLugaresAlmuerzo(Bloque bloque)
        {
            List<LugarAlmuerzo> lugaresAlmuerzo = db.LugarAlmuerzo.Where(x => x.softDelete == false).ToList();
            if (bloque != null)
            {
                LugarAlmuerzo lugarAlmuerzo = bloque.lugarAlmuerzo;
                if (!lugaresAlmuerzo.Contains(lugarAlmuerzo) && lugarAlmuerzo != null)
                {
                    lugaresAlmuerzo.Add(lugarAlmuerzo);
                }
            }
            return new SelectList(lugaresAlmuerzo.Select(c => new SelectListItem
            {
                Text = c.nombre,
                Value = c.idLugarAlmuerzo.ToString()
            }).ToList(), "Value", "Text");
        }

        public SelectList GetRelatores(Comercializacion comercializacion)
        {
            ICollection<RelatorCurso> relatores = comercializacion.relatoresCursos;
            return new SelectList(relatores.Select(r => new SelectListItem
            {
                Text = "[" + r.relator.contacto.run + "]" + " " + r.relator.contacto.nombres + " " + r.relator.contacto.apellidoPaterno + " " + r.relator.contacto.apellidoMaterno,
                Value = r.idRelator.ToString()
            }).ToList(), "Value", "Text");
        }

        private SelectList GetSucursales()
        {
            return new SelectList(db.Sucursal.Select(s => new SelectListItem
            {
                Text = s.nombre,
                Value = s.idSucursal.ToString()
            }).ToList(), "Value", "Text");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveLink()
        {
            int id = Convert.ToInt32(Request["idComercializacion"]);
            String url = Request["linkComercializacion.linkManual"];
            int typesLink = 0;
            if (Request["linkComercializacion.linkType.idLinkType"] != "") {
                typesLink = Convert.ToInt32(Request["linkComercializacion.linkType.idLinkType"]);
            }
               
            var error = "ok";
            if (url != null && !urlchk.IsMatch(url))
            {
                error = "Formato incorrecto del link";
            }
            else if (typesLink == 0) {
                error = "Seleccione un tipo de link";
            }
            else
            {
                var comercializacion = db.Comercializacion.Find(id);
                var linkComercializacion = db.LinkComercializacion.Where(x => x.comercializacion.idComercializacion == id).Include(x => x.link).FirstOrDefault();
                if (linkComercializacion != null)
                {
                    if (url == null)
                    {
                        linkComercializacion.linkAutomatic = true;
                        linkComercializacion.linkManual = null;
                        if (linkComercializacion.link != null)
                            linkComercializacion.link = null;
                       
                    }
                    else
                    {
                        linkComercializacion.linkAutomatic = false;
                        linkComercializacion.linkManual = url;
                        
                    }
                    linkComercializacion.comercializacion = comercializacion;
                    linkComercializacion.linkType = db.LinkTypes.Find(typesLink);
                    db.Entry(linkComercializacion).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    
                    if (comercializacion != null)
                    {
                        LinkComercializacion link = new LinkComercializacion
                        {
                            linkAutomatic = url == null,
                            linkManual = url == null ? null : url,
                            comercializacion = comercializacion,
                            linkType = db.LinkTypes.Find(typesLink)
                        };
                        db.LinkComercializacion.Add(link);
                        db.SaveChanges();
                    }
                    else
                    {
                        error = "Comercializacion no encontrada";
                    }
                }


            }
            var jsonResult = Json(new { error, id }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}
