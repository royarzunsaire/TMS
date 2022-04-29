using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class SalidaTerrenoController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: SalidaTerreno
        [CustomAuthorize(new string[] { "/SalidaTerreno/" })]
        public ActionResult Index()
        {
            return View(db.SalidaTerreno.ToList());
        }

        // GET: SalidaTerreno/Calendario
        [CustomAuthorize(new string[] { "/SalidaTerreno/Calendario/" })]
        public ActionResult Calendario()
        {
            List<SalidaTerreno> salidasTerreno = db.SalidaTerreno.Where(x => x.softdelete == false).ToList();

            List<FullCalendarEvent> eventos = new List<FullCalendarEvent>();

            foreach (var item in salidasTerreno)
            {
                FullCalendarEvent evento = new FullCalendarEvent();
                if (item.cliente != null)
                {
                    evento.title = item.cliente.nombreEmpresa;
                }
                if (item.posibleCliente != null)
                {
                    evento.title = item.posibleCliente;
                }
                evento.description = item.vendedor.UserName;
                if (item.observacion != null)
                {
                    item.observacion = item.observacion.Replace(System.Environment.NewLine, " ");
                }
                evento.description = new
                {
                    vendedor = item.vendedor.UserName,
                    estado = item.estado.ToString(),
                    motivo = item.motivo.Replace(System.Environment.NewLine, " "),
                    resumen = item.observacion
                };
                evento.start = item.fecha.ToString("yyyy-MM-dd") + " " + item.hora.ToString("HH:mm");
                evento.end = item.fecha.ToString("yyyy-MM-dd") + " " + item.hora.ToString("HH:mm");
                //evento.end = item.fecha.AddDays(1).ToString("yyyy-MM-dd");
                if (item.estado == EstadoSalidaTerreno.Realizado)
                {
                    evento.color = "blue";
                }
                if (item.estado == EstadoSalidaTerreno.Cancelado)
                {
                    evento.color = "red";
                }
                if (item.estado == EstadoSalidaTerreno.Programado)
                {
                    evento.color = "green";
                }
                if (item.estado == EstadoSalidaTerreno.Reprogramado)
                {
                    evento.color = "orange";
                }
                eventos.Add(evento);
            }

            ViewBag.eventosJson = Newtonsoft.Json.JsonConvert.SerializeObject(eventos);

            ViewBag.eventos = eventos;
            return View(salidasTerreno);
        }

        // GET: SalidaTerreno/SalidasCliente
        [CustomAuthorize(new string[] { "/SalidaTerreno/SalidasCliente/" })]
        public ActionResult SalidasCliente()
        {
            string userId = User.Identity.GetUserId();
            Contacto contacto = db.Contacto.Where(c => c.usuario.Id == userId).FirstOrDefault();
            if (contacto == null)
            {
                return View(new List<SalidaTerreno>());
            }
            ClienteContacto clienteContacto = db.ClienteContacto.Where(cc => cc.idContacto == contacto.idContacto).FirstOrDefault();
            if (clienteContacto == null)
            {
                return View(new List<SalidaTerreno>());
            }
            List<SalidaTerreno> salidasTerreno = db.SalidaTerreno
                .Where(st => st.cliente.idCliente == clienteContacto.idCliente)
                .ToList();
            return View(salidasTerreno);
        }

        // GET: SalidaTerreno/MisSalidas
        [CustomAuthorize(new string[] { "/SalidaTerreno/MisSalidas/" })]
        public ActionResult MisSalidas()
        {
            string userId = User.Identity.GetUserId();
            List<SalidaTerreno> salidasTerreno = db.SalidaTerreno
                .Where(st => st.vendedor.Id == userId)
                .ToList();
            return View(salidasTerreno);
        }

        // GET: SalidaTerreno/Details/5
        [CustomAuthorize(new string[] { "/SalidaTerreno/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalidaTerreno salidaTerreno = db.SalidaTerreno.Find(id);
            if (salidaTerreno == null)
            {
                return HttpNotFound();
            }
            return View(salidaTerreno);
        }

        // GET: SalidaTerreno/Create
        [CustomAuthorize(new string[] { "/SalidaTerreno/MisSalidas/" })]
        public ActionResult Create()
        {
            SalidaTerreno salidaTerreno = new SalidaTerreno();
            ViewBag.clientes = GetClientes(salidaTerreno);
            ViewBag.sucursales = GetSucursales();
            return View();
        }

        // POST: SalidaTerreno/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/SalidaTerreno/MisSalidas/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "fecha,hora,motivo,posibleCliente")] SalidaTerreno salidaTerreno, Sucursal sucursal)
        {
            if (Request["cliente.idCliente"] != "")
            {
                if (salidaTerreno.posibleCliente == null)
                {
                    salidaTerreno.cliente = db.Cliente.Find(int.Parse(Request["cliente.idCliente"]));
                }
                else
                {
                    salidaTerreno.cliente = db.Cliente.Find(int.Parse(Request["cliente.idCliente"]));
                    ModelState.AddModelError("", "Solo se puede seleccionar o ingresar un cliente");
                }
            }
            else
            {
                if (salidaTerreno.posibleCliente == null)
                {
                    ModelState.AddModelError("", "Se debe seleccionar o ingresar un cliente");
                }
            }
            if (ModelState.IsValid)
            {
                var vendedor = db.AspNetUsers.Find(User.Identity.GetUserId());
                salidaTerreno.vendedor = vendedor;
                salidaTerreno.sucursal = db.Sucursal.Find(sucursal.idSucursal);
                salidaTerreno.estado = EstadoSalidaTerreno.Programado;
                salidaTerreno.usuarioCreador = vendedor.Id;
                salidaTerreno.fechaCreacion = DateTime.Now;
                db.SalidaTerreno.Add(salidaTerreno);
                db.SaveChanges();
                return RedirectToAction("MisSalidas");
            }
            ViewBag.clientes = GetClientes(salidaTerreno);
            ViewBag.sucursales = GetSucursales();
            return View(salidaTerreno);
        }

        // POST: SalidaTerreno/Resumen
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/SalidaTerreno/MisSalidas/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Resumen()
        {
            int idSalidaTerreno = int.Parse(Request["idSalidaTerreno"]);
            SalidaTerreno salidaTerreno = db.SalidaTerreno.Find(idSalidaTerreno);
            salidaTerreno.observacion = Request["resumen"];
            // validar tituloCurricular
            var context = new ValidationContext(salidaTerreno, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(salidaTerreno, context, results, true);
            if (isValid)
            {
                db.Entry(salidaTerreno).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { HasErrors = false, responseText = "OK" });
            }
            ModelState.AddModelError("", "El campo Resumen puede tener máximo 999 caracteres");
            // enviar los errores al ajax
            var errors = ModelState.ToDictionary(kvp => kvp.Key,
                    kvp => kvp.Value.Errors
                  .Select(e => e.ErrorMessage).ToArray())
                  .Where(m => m.Value.Count() > 0);
            return Json(new { HasErrors = true, Errors = errors });
        }

        // POST: SalidaTerreno/CambiarEstado
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/SalidaTerreno/MisSalidas/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstado()
        {
            int idSalidaTerreno = int.Parse(Request["item.idSalidaTerreno"]);
            SalidaTerreno salidaTerrenoBD = db.SalidaTerreno.Find(idSalidaTerreno);
            EstadoSalidaTerreno nuevoEstado = (EstadoSalidaTerreno)int.Parse(Request["item.estado"]);
            if (salidaTerrenoBD.estado == EstadoSalidaTerreno.Reprogramado)
            {
                if (nuevoEstado == EstadoSalidaTerreno.Programado)
                {
                    ModelState.AddModelError("", "No se puede volver al estado: Programado");
                    // enviar los errores al ajax
                    var errors = ModelState.ToDictionary(kvp => kvp.Key,
                            kvp => kvp.Value.Errors
                          .Select(e => e.ErrorMessage).ToArray())
                          .Where(m => m.Value.Count() > 0);
                    return Json(new { HasErrors = true, Errors = errors });
                }
            }
            if (nuevoEstado == EstadoSalidaTerreno.Realizado)
            {
                if (DateTime.Compare(salidaTerrenoBD.fecha, DateTime.Now) > 0)
                {
                    ModelState.AddModelError("", "La fecha es incorrecta");
                    // enviar los errores al ajax
                    var errors = ModelState.ToDictionary(kvp => kvp.Key,
                            kvp => kvp.Value.Errors
                          .Select(e => e.ErrorMessage).ToArray())
                          .Where(m => m.Value.Count() > 0);
                    return Json(new { HasErrors = true, Errors = errors });
                }
                if (salidaTerrenoBD.observacion == null)
                {
                    ModelState.AddModelError("", "Se debe ingresar el resumen");
                    // enviar los errores al ajax
                    var errors = ModelState.ToDictionary(kvp => kvp.Key,
                            kvp => kvp.Value.Errors
                          .Select(e => e.ErrorMessage).ToArray())
                          .Where(m => m.Value.Count() > 0);
                    return Json(new { HasErrors = true, Errors = errors });
                }
            }
            salidaTerrenoBD.estado = nuevoEstado;
            db.Entry(salidaTerrenoBD).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new { HasErrors = false, responseText = "OK" });
        }

        // GET: SalidaTerreno/Edit/5
        //[CustomAuthorize(new int[] { 88 })]
        [CustomAuthorize(new string[] { "/SalidaTerreno/MisSalidas/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalidaTerreno salidaTerreno = db.SalidaTerreno.Find(id);
            if (salidaTerreno.estado == EstadoSalidaTerreno.Cancelado || salidaTerreno.estado == EstadoSalidaTerreno.Realizado)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (salidaTerreno == null)
            {
                return HttpNotFound();
            }
            ViewBag.clientes = GetClientes(salidaTerreno);
            ViewBag.sucursales = GetSucursales();
            return View(salidaTerreno);
        }

        // POST: SalidaTerreno/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/SalidaTerreno/MisSalidas/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idSalidaTerreno,fecha,hora,motivo,posibleCliente")] SalidaTerreno salidaTerreno, Sucursal sucursal)
        {
            SalidaTerreno salidaTerrenoBD = db.SalidaTerreno.Find(salidaTerreno.idSalidaTerreno);
            if (Request["cliente.idCliente"] != "")
            {
                if (salidaTerreno.posibleCliente == null)
                {
                    salidaTerrenoBD.cliente = db.Cliente.Find(int.Parse(Request["cliente.idCliente"]));
                }
                else
                {
                    salidaTerrenoBD.cliente = db.Cliente.Find(int.Parse(Request["cliente.idCliente"]));
                    ModelState.AddModelError("", "Solo se puede seleccionar o ingresar un cliente");
                }
            }
            else
            {
                if (salidaTerreno.posibleCliente == null)
                {
                    ModelState.AddModelError("", "Se debe seleccionar o ingresar un cliente");
                }
                else
                {
                    var cliente = salidaTerrenoBD.cliente;
                    cliente = null;
                    salidaTerrenoBD.cliente = cliente;
                }
            }
            if (ModelState.IsValid)
            {
                salidaTerrenoBD.sucursal = db.Sucursal.Find(sucursal.idSucursal);
                salidaTerrenoBD.posibleCliente = salidaTerreno.posibleCliente;
                salidaTerrenoBD.fecha = salidaTerreno.fecha;
                salidaTerrenoBD.hora = salidaTerreno.hora;
                salidaTerrenoBD.motivo = salidaTerreno.motivo;
                salidaTerrenoBD.usuarioCreador = User.Identity.GetUserId();
                salidaTerrenoBD.fechaCreacion = DateTime.Now;
                db.Entry(salidaTerrenoBD).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("MisSalidas");
            }
            ViewBag.clientes = GetClientes(salidaTerreno);
            ViewBag.sucursales = GetSucursales();
            return View(salidaTerreno);
        }

        // GET: SalidaTerreno/Delete/5
        //[CustomAuthorize(new int[] { 88 })]
        [CustomAuthorize(new string[] { "/SalidaTerreno/MisSalidas/" })]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalidaTerreno salidaTerreno = db.SalidaTerreno.Find(id);
            if (salidaTerreno.estado == EstadoSalidaTerreno.Cancelado || salidaTerreno.estado == EstadoSalidaTerreno.Realizado)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (salidaTerreno == null)
            {
                return HttpNotFound();
            }
            return View(salidaTerreno);
        }

        // POST: SalidaTerreno/Delete/5
        [CustomAuthorize(new string[] { "/SalidaTerreno/MisSalidas/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SalidaTerreno salidaTerreno = db.SalidaTerreno.Find(id);
            db.SalidaTerreno.Remove(salidaTerreno);
            db.SaveChanges();
            return RedirectToAction("MisSalidas");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public SelectList GetClientes(SalidaTerreno salidaTerreno)
        {
            List<Cliente> clientes = db.Cliente.Where(x => x.softDelete == false).ToList();
            if (salidaTerreno.cliente != null)
            {
                Cliente cliente = salidaTerreno.cliente;
                if (!clientes.Contains(cliente))
                {
                    clientes.Add(cliente);
                }
            }
            return new SelectList(clientes.Select(c => new SelectListItem
            {
                Text = "" + c.rut + " | " + c.nombreEmpresa,
                Value = c.idCliente.ToString()
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
    }
}
