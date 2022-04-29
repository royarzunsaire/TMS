using Microsoft.AspNet.Identity;
using jsreport.MVC;
using jsreport.Types;
using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SGC.Controllers
{
    [Authorize]
    public class ClienteController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: Cliente
        [CustomAuthorize(new string[] { "/Cliente/" })]
        public ActionResult Index()
        {
            List<ViewModelCliente> lvmc = new List<ViewModelCliente>();
            var c = db.Cliente.Where(x => x.softDelete == false).ToList();
            foreach (Cliente cl in c)
            {
                ViewModelCliente vmc = new ViewModelCliente();
                vmc._cliente = cl;
                vmc._mandante = db.Mandante.Find(cl.idMandante);
                lvmc.Add(vmc);
            }
            return View(lvmc);
        }

        // GET: Cliente
        public ActionResult MyIndex()
        {
            string idUsuario = User.Identity.GetUserId();
            List<ViewModelCliente> lvmc = new List<ViewModelCliente>();
            var clientes = db.Cliente.Where(x => x.softDelete == false && x.usuariosAsignados.Count() > 0).ToList();
            foreach (Cliente cliente in clientes)
            {
                ViewModelCliente vmc = new ViewModelCliente();

                if (cliente.usuariosAsignados.Any(x => x.usuario.Id == idUsuario))
                {
                    vmc._cliente = cliente;
                    vmc._mandante = db.Mandante.Find(cliente.idMandante);
                    lvmc.Add(vmc);
                }
            }
            return View(lvmc);
        }

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReporteClientes()
        {
            var contactos = db.ClienteContactoCotizacion.Where(x => x.contacto.softDelete == false && x.cliente.softDelete == false)
                .OrderBy(x => x.cliente.nombreEmpresa).ThenBy(x => x.contacto.nombres).ThenBy(x => x.contacto.apellidoPaterno).ToList();

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"r04.xlsx\"");

            return View(contactos);
        }

        [EnableJsReport()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReporteMisClientes()
        {
            string idUsuario = User.Identity.GetUserId();
            var contactos = db.ClienteContactoCotizacion.Where(x => x.contacto.softDelete == false && x.cliente.softDelete == false
            && x.cliente.usuariosAsignados.Where(y => y.usuario.Id == idUsuario).Count() > 0)
                .OrderBy(x => x.cliente.nombreEmpresa).ThenBy(x => x.contacto.nombres).ThenBy(x => x.contacto.apellidoPaterno).ToList();

            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"r04_Mis_Clientes.xlsx\"");

            return View(contactos);
        }

        // GET: Cliente/Details/5
        [CustomAuthorize(new string[] { "/Cliente/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cliente cliente = db.Cliente.Find(id);
            if (cliente == null)
            {
                return HttpNotFound();
            }
            ViewBag.faenas = db.FaenaCliente.Where(x => x.cliente.idCliente == cliente.idCliente).Select(c => c.faena.nombre).ToList();

            return View(cliente);
        }

        // GET: Cliente/Create
        [CustomAuthorize(new string[] { "/Cliente/", "/Cliente/Create/" })]
        public ActionResult Create()
        {
            // opciones seleccionadas
            ViewBag.girosSeleccionados = new JavaScriptSerializer().Serialize("");
            ViewBag.faenasSeleccionados = new JavaScriptSerializer().Serialize("");
            ViewBag.representanteLegalSeleccionados = new JavaScriptSerializer().Serialize("");
            ViewBag.encargadoPagosSeleccionados = new JavaScriptSerializer().Serialize("");
            ViewBag.tiposDocumentosPagoSeleccionados = new JavaScriptSerializer().Serialize("");
            // opciones disponibles
            ViewBag.Giros = GetGiros();
            ViewBag.EncargadoPagos = GetContactos();
            ViewBag.RepresentanteLegal = GetContactos();
            ViewBag.TiposDocumentosPago = GetTipoDocumentoPago();
            ViewBag.Mandante = GetMandante();
            ViewBag.Usuarios = GetUsuarios();
            ViewBag.faenas = Getfaenas();

            Cliente cliente = new Cliente();
            return View(cliente);
        }

        // POST: Cliente/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Cliente/", "/Cliente/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Cliente cliente)
        {
            // verificar que se selecciono al menos un giro
            if (Request["giros"] == null)
            {
                ModelState.AddModelError("giros", "Se debe seleccionar un giro.");
            }
            else
            {
                // guardar los giros seleccionados
                cliente.clienteGiros = new List<ClienteGiro>();
                int[] giros = Array.ConvertAll(Request["giros"].Split(','), int.Parse);
                for (int i = 0; i < giros.Length; i++)
                {
                    Giro giro = db.Giro.Find(giros[i]);
                    ClienteGiro clienteGiro = new ClienteGiro();
                    clienteGiro.giro = giro;
                    clienteGiro.cliente = cliente;
                    clienteGiro.fechaCreacion = DateTime.Now;
                    clienteGiro.usuarioCreador = User.Identity.Name;
                    cliente.clienteGiros.Add(clienteGiro);
                }
            }
            // verificar que se selecciono al menos un tipo documento pago
            if (Request["tiposDocumentosPagos"] == null)
            {
                ModelState.AddModelError("tiposDocumentosPagos", "Se debe seleccionar un tipo de documento de pago.");
            }
            else
            {
                // guardar los tipos documento pago seleccionados
                cliente.clienteTipoDocumentosPagos = new List<ClienteTipoDocumentosPago>();
                int[] tiposDocumentosPagos = Array.ConvertAll(Request["tiposDocumentosPagos"].Split(','), int.Parse);
                for (int i = 0; i < tiposDocumentosPagos.Length; i++)
                {
                    TiposDocumentosPago tiposDocumentosPago = db.TiposDocumentosPago.Find(tiposDocumentosPagos[i]);
                    ClienteTipoDocumentosPago clienteTipoDocumentosPago = new ClienteTipoDocumentosPago();
                    clienteTipoDocumentosPago.tipoDocumentosPago = tiposDocumentosPago;
                    clienteTipoDocumentosPago.cliente = cliente;
                    clienteTipoDocumentosPago.fechaCreacion = DateTime.Now;
                    clienteTipoDocumentosPago.usuarioCreador = User.Identity.Name;
                    cliente.clienteTipoDocumentosPagos.Add(clienteTipoDocumentosPago);
                }
            }
            // verificar que se selecciono al menos un encargado de pagos
            if (Request["encargadosPagos"] == null)
            {
                //ModelState.AddModelError("encargadosPagos", "Se debe seleccionar un encargado de pagos.");
            }
            else
            {
                // guardar los encargados pagos seleccionados
                cliente.encargadoPagos = new List<EncargadoPago>();
                int[] encargadosPagos = Array.ConvertAll(Request["encargadosPagos"].Split(','), int.Parse);
                for (int i = 0; i < encargadosPagos.Length; i++)
                {
                    Contacto contacto = db.Contacto.Find(encargadosPagos[i]);
                    EncargadoPago encargadoPago = new EncargadoPago();
                    encargadoPago.contacto = contacto;
                    encargadoPago.cliente = cliente;
                    encargadoPago.fechaCreacion = DateTime.Now;
                    encargadoPago.usuarioCreador = User.Identity.Name;
                    cliente.encargadoPagos.Add(encargadoPago);
                }
            }
            // verificar si se selecciono un representante legal
            if (Request["representantesLegales"] != null)
            {
                // guardar los representantes legales seleccionados
                cliente.representanteLegals = new List<RepresentanteLegal>();
                int[] representantesLegales = Array.ConvertAll(Request["representantesLegales"].Split(','), int.Parse);
                for (int i = 0; i < representantesLegales.Length; i++)
                {
                    Contacto contacto = db.Contacto.Find(representantesLegales[i]);
                    RepresentanteLegal representanteLegal = new RepresentanteLegal();
                    representanteLegal.contacto = contacto;
                    representanteLegal.cliente = cliente;
                    representanteLegal.fechaCreacion = DateTime.Now;
                    representanteLegal.usuarioCreador = User.Identity.Name;
                    cliente.representanteLegals.Add(representanteLegal);
                }
            }
            if (ModelState.IsValid)
            {
                cliente.ultimaFechaEnvioCorreo = DateTime.Now;
                cliente.fechaCreacion = DateTime.Now;
                cliente.fechaAlertaEncuestaSatisfaccion = DateTime.Now;
                cliente.fechaDescEspecial = DateTime.Now;
                cliente.usuarioCreador = User.Identity.Name;
                cliente.softDelete = false;
                cliente.postVenta = false;
                cliente.encuestaSatisfaccion = false;
                db.Cliente.Add(cliente);
                db.SaveChanges();

                // verificar que se selecciono al menos un faena
                if (Request["faena"] != null)
                {
                    // guardar los giros seleccionados
                    var faenasOld = db.FaenaCliente.Where(x => x.cliente.idCliente == cliente.idCliente).ToList();
                    foreach (FaenaCliente item in faenasOld.ToList())
                    {
                        db.FaenaCliente.Remove(item);
                    }

                    int[] faenasNew = Array.ConvertAll(Request["faena"].Split(','), int.Parse);
                    for (int i = 0; i < faenasNew.Length; i++)
                    {
                        Faena faena = db.Faena.Find(faenasNew[i]);
                        FaenaCliente faenaCliente = new FaenaCliente { faena = faena, cliente = cliente };
                        db.FaenaCliente.Add(faenaCliente);

                    }
                    db.SaveChanges();
                }

                // verificar que se selecciono al menos un usuario
                if (Request["usuarios"] != null)
                {
                    // obtener usuarios seleccionados
                    var usuariosOld = db.ClienteUsuario.Where(x => x.cliente.idCliente == cliente.idCliente).ToList();
                    foreach (ClienteUsuario item in usuariosOld.ToList())
                    {
                        db.ClienteUsuario.Remove(item);
                    }

                    string[] usuariosNew = Request["usuarios"].Split(',');
                    for (int i = 0; i < usuariosNew.Length; i++)
                    {
                        AspNetUsers usuario = db.AspNetUsers.Find(usuariosNew[i]);
                        ClienteUsuario clienteUsuario = new ClienteUsuario { cliente = cliente, usuario = usuario, fechaAsignado = DateTime.Today };
                        db.ClienteUsuario.Add(clienteUsuario);

                    }
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }
            // opciones seleccionadas
            if (cliente.clienteGiros != null)
            {
                ViewBag.girosSeleccionados = new JavaScriptSerializer().Serialize(cliente.clienteGiros.Select(c => c.giro.idGiro).ToList());
            }
            else
            {
                ViewBag.girosSeleccionados = new JavaScriptSerializer().Serialize("");
            }
            if (cliente.representanteLegals != null)
            {
                ViewBag.representanteLegalSeleccionados = new JavaScriptSerializer().Serialize(cliente.representanteLegals.Select(c => c.contacto.idContacto).ToList());
            }
            else
            {
                ViewBag.representanteLegalSeleccionados = new JavaScriptSerializer().Serialize("");
            }
            if (cliente.encargadoPagos != null)
            {
                ViewBag.encargadoPagosSeleccionados = new JavaScriptSerializer().Serialize(cliente.encargadoPagos.Select(c => c.contacto.idContacto).ToList());
            }
            else
            {
                ViewBag.encargadoPagosSeleccionados = new JavaScriptSerializer().Serialize("");
            }
            if (cliente.clienteTipoDocumentosPagos != null)
            {
                ViewBag.tiposDocumentosPagoSeleccionados = new JavaScriptSerializer().Serialize(cliente.clienteTipoDocumentosPagos.Select(c => c.tipoDocumentosPago.idTipoDocumentosPago).ToList());
            }
            else
            {
                ViewBag.tiposDocumentosPagoSeleccionados = new JavaScriptSerializer().Serialize("");
            }
            if (Request["faena"] != "")
            {
                ViewBag.faenasSeleccionados = new JavaScriptSerializer().Serialize(Request["faena"].Split(','));
            }
            else
            {
                ViewBag.faenasSeleccionados = new JavaScriptSerializer().Serialize("");
            }


            // opciones disponibles
            ViewBag.faenas = Getfaenas();
            ViewBag.Giros = GetGiros();
            ViewBag.EncargadoPagos = GetContactos();
            ViewBag.RepresentanteLegal = GetContactos();
            ViewBag.Usuarios = GetUsuarios();
            ViewBag.TiposDocumentosPago = GetTipoDocumentoPago();
            ViewBag.Mandante = GetMandante();

            return View(cliente);
        }

        // GET: Cliente/Edit/5
        [CustomAuthorize(new string[] { "/Cliente/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cliente cliente = db.Cliente.Find(id);
            if (cliente == null)
            {
                return HttpNotFound();
            }
            // opciones seleccionadas
            if (cliente.clienteGiros != null)
            {
                ViewBag.girosSeleccionados = new JavaScriptSerializer().Serialize(cliente.clienteGiros.Select(c => c.idGiro).ToList());
            }
            else
            {
                ViewBag.girosSeleccionados = new JavaScriptSerializer().Serialize("");
            }
            if (cliente.representanteLegals != null)
            {
                ViewBag.representanteLegalSeleccionados = new JavaScriptSerializer().Serialize(cliente.representanteLegals.Select(c => c.idContacto).ToList());
            }
            else
            {
                ViewBag.representanteLegalSeleccionados = new JavaScriptSerializer().Serialize("");
            }
            if (cliente.usuariosAsignados != null)
            {
                ViewBag.usuariosSeleccionados = new JavaScriptSerializer().Serialize(cliente.usuariosAsignados.Select(c => c.usuario.Id).ToList());
            }
            else
            {
                ViewBag.usuariosSeleccionados = new JavaScriptSerializer().Serialize("");
            }
            if (cliente.encargadoPagos != null)
            {
                ViewBag.encargadoPagosSeleccionados = new JavaScriptSerializer().Serialize(cliente.encargadoPagos.Select(c => c.idContacto).ToList());
            }
            else
            {
                ViewBag.encargadoPagosSeleccionados = new JavaScriptSerializer().Serialize("");
            }
            if (cliente.clienteTipoDocumentosPagos != null)
            {
                ViewBag.tiposDocumentosPagoSeleccionados = new JavaScriptSerializer().Serialize(cliente.clienteTipoDocumentosPagos.Select(c => c.idTipoDocumentosPago).ToList());
            }
            else
            {
                ViewBag.tiposDocumentosPagoSeleccionados = new JavaScriptSerializer().Serialize("");
            }
            // opciones disponibles (incluye seleccionadas)
            if (cliente.clienteGiros != null)
            {
                ViewBag.Giros = GetGirosCliente(cliente);
            }
            else
            {
                ViewBag.Giros = GetGiros();
            }
            if (cliente.clienteTipoDocumentosPagos != null)
            {
                ViewBag.TiposDocumentosPago = GetTiposDocumentosPagoCliente(cliente);
            }
            else
            {
                ViewBag.TiposDocumentosPago = GetTipoDocumentoPago();
            }
            if (cliente.representanteLegals != null)
            {
                ViewBag.RepresentanteLegal = GetRepresentantesLegalesCliente(cliente);
            }
            else
            {
                ViewBag.RepresentanteLegal = GetContactos();
            }
            if (cliente.usuariosAsignados != null)
            {
                ViewBag.Usuarios = GetUsuariosCliente(cliente);
            }
            else
            {
                ViewBag.Usuarios = GetUsuarios();
            }
            if (cliente.encargadoPagos != null)
            {
                ViewBag.EncargadoPagos = GetEncargadosPagosCliente(cliente);
            }
            else
            {
                ViewBag.EncargadoPagos = GetContactos();
            }
            if (cliente.mandante != null)
            {
                ViewBag.Mandante = GetMandanteCliente(cliente);
            }
            else
            {
                ViewBag.Mandante = GetMandante();

            }
            ViewBag.faenas = Getfaenas();
            var faenas = db.FaenaCliente.Where(x => x.cliente.idCliente == cliente.idCliente).Select(c => c.faena.idFaena).ToList();
            if (faenas != null)
            {
                ViewBag.faenasSeleccionados = new JavaScriptSerializer().Serialize(faenas);
            }
            else
            {
                ViewBag.faenasSeleccionados = new JavaScriptSerializer().Serialize("");
            }


            return View(cliente);
        }

        // POST: Cliente/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Cliente/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Cliente cliente)
        {
            var clienteBD = db.Cliente.AsNoTracking().Where(x => x.idCliente == cliente.idCliente).FirstOrDefault();
            // eliminar giros cliente
            List<ClienteGiro> clienteGiros = db.ClienteGiro.Where(c => c.idCliente == cliente.idCliente).ToList();
            foreach (ClienteGiro clienteGiro in clienteGiros)
            {
                db.ClienteGiro.Remove(clienteGiro);
                db.SaveChanges();
            }
            // eliminar tipos documentos pago cliente
            List<ClienteTipoDocumentosPago> clienteTipoDocumentosPagos = db.ClienteTipoDocumentosPago.Where(c => c.idCliente == cliente.idCliente).ToList();
            foreach (ClienteTipoDocumentosPago clienteTipoDocumentosPago in clienteTipoDocumentosPagos)
            {
                db.ClienteTipoDocumentosPago.Remove(clienteTipoDocumentosPago);
                db.SaveChanges();
            }
            // eliminar los encargados pago cliente
            List<EncargadoPago> encargadoPagos = db.EncargadoPago.Where(c => c.idCliente == cliente.idCliente).ToList();
            foreach (EncargadoPago encargadoPago in encargadoPagos)
            {
                db.EncargadoPago.Remove(encargadoPago);
                db.SaveChanges();
            }
            // eliminar los representantes legales cliente
            List<RepresentanteLegal> representanteLegals = db.RepresentanteLegal.Where(c => c.idCliente == cliente.idCliente).ToList();
            foreach (RepresentanteLegal representanteLegal in representanteLegals)
            {
                db.RepresentanteLegal.Remove(representanteLegal);
                db.SaveChanges();
            }
            // verificar que se selecciono al menos un giro
            if (Request["giros"] == null)
            {
                ModelState.AddModelError("giros", "Se debe seleccionar un giro.");
            }
            else
            {
                // guardar los giros seleccionados
                cliente.clienteGiros = new List<ClienteGiro>();
                int[] giros = Array.ConvertAll(Request["giros"].Split(','), int.Parse);
                for (int i = 0; i < giros.Length; i++)
                {
                    Giro giro = db.Giro.Find(giros[i]);
                    ClienteGiro clienteGiro = new ClienteGiro();
                    clienteGiro.giro = giro;
                    clienteGiro.cliente = cliente;
                    clienteGiro.fechaCreacion = DateTime.Now;
                    clienteGiro.usuarioCreador = User.Identity.Name;
                    cliente.clienteGiros.Add(clienteGiro);
                    db.ClienteGiro.Add(clienteGiro);
                }
            }





            // verificar que se selecciono al menos un tipo documento pago
            if (Request["tiposDocumentosPagos"] == null)
            {
                ModelState.AddModelError("tiposDocumentosPagos", "Se debe seleccionar un tipo de documento de pago.");
            }
            else
            {
                // guardar los tipos documento pago seleccionados
                cliente.clienteTipoDocumentosPagos = new List<ClienteTipoDocumentosPago>();
                int[] tiposDocumentosPagos = Array.ConvertAll(Request["tiposDocumentosPagos"].Split(','), int.Parse);
                for (int i = 0; i < tiposDocumentosPagos.Length; i++)
                {
                    TiposDocumentosPago tiposDocumentosPago = db.TiposDocumentosPago.Find(tiposDocumentosPagos[i]);
                    ClienteTipoDocumentosPago clienteTipoDocumentosPago = new ClienteTipoDocumentosPago();
                    clienteTipoDocumentosPago.tipoDocumentosPago = tiposDocumentosPago;
                    clienteTipoDocumentosPago.cliente = cliente;
                    clienteTipoDocumentosPago.fechaCreacion = DateTime.Now;
                    clienteTipoDocumentosPago.usuarioCreador = User.Identity.Name;
                    cliente.clienteTipoDocumentosPagos.Add(clienteTipoDocumentosPago);
                    db.ClienteTipoDocumentosPago.Add(clienteTipoDocumentosPago);
                }
            }
            // verificar que se selecciono al menos un encargado de pagos
            if (Request["encargadosPagos"] == null)
            {
                //ModelState.AddModelError("encargadosPagos", "Se debe seleccionar un encargado de pagos.");
            }
            else
            {
                // guardar los encargados pagos seleccionados
                cliente.encargadoPagos = new List<EncargadoPago>();
                int[] encargadosPagos = Array.ConvertAll(Request["encargadosPagos"].Split(','), int.Parse);
                for (int i = 0; i < encargadosPagos.Length; i++)
                {
                    Contacto contacto = db.Contacto.Find(encargadosPagos[i]);
                    EncargadoPago encargadoPago = new EncargadoPago();
                    encargadoPago.contacto = contacto;
                    encargadoPago.cliente = cliente;
                    encargadoPago.fechaCreacion = DateTime.Now;
                    encargadoPago.usuarioCreador = User.Identity.Name;
                    cliente.encargadoPagos.Add(encargadoPago);
                    db.EncargadoPago.Add(encargadoPago);
                }
            }
            // verificar si se selecciono un representante legal
            if (Request["representantesLegales"] != null)
            {
                // guardar los representantes legales seleccionados
                cliente.representanteLegals = new List<RepresentanteLegal>();
                int[] representantesLegales = Array.ConvertAll(Request["representantesLegales"].Split(','), int.Parse);
                for (int i = 0; i < representantesLegales.Length; i++)
                {
                    Contacto contacto = db.Contacto.Find(representantesLegales[i]);
                    RepresentanteLegal representanteLegal = new RepresentanteLegal();
                    representanteLegal.contacto = contacto;
                    representanteLegal.cliente = cliente;
                    representanteLegal.fechaCreacion = DateTime.Now;
                    representanteLegal.usuarioCreador = User.Identity.Name;
                    cliente.representanteLegals.Add(representanteLegal);
                    db.RepresentanteLegal.Add(representanteLegal);
                }
            }
            if (ModelState.IsValid)
            {

                //TODO: se deberia revisar que campos aqui deben modificarse
                cliente.ultimaFechaEnvioCorreo = cliente.ultimaFechaEnvioCorreo;
                cliente.fechaCreacion = DateTime.Now;
                cliente.fechaDescEspecial = DateTime.Now;
                cliente.fechaAlertaEncuestaSatisfaccion = clienteBD.fechaAlertaEncuestaSatisfaccion;
                cliente.usuarioCreador = User.Identity.Name;
                db.Entry(cliente).State = EntityState.Modified;
                db.SaveChanges();

                // verificar que se selecciono al menos un faena
                if (Request["faena"] != null)
                {
                    // guardar los giros seleccionados
                    var faenasOld = db.FaenaCliente.Where(x => x.cliente.idCliente == cliente.idCliente).ToList();
                    foreach (FaenaCliente item in faenasOld.ToList())
                    {
                        db.FaenaCliente.Remove(item);
                    }

                    int[] faenasNew = Array.ConvertAll(Request["faena"].Split(','), int.Parse);
                    for (int i = 0; i < faenasNew.Length; i++)
                    {
                        Faena faena = db.Faena.Find(faenasNew[i]);
                        FaenaCliente faenaCliente = new FaenaCliente { faena = faena, cliente = cliente };
                        db.FaenaCliente.Add(faenaCliente);

                    }
                    db.SaveChanges();
                }

                // verificar que se selecciono al menos un usuario
                if (Request["usuarios"] != null)
                {
                    // obtener usuarios seleccionados
                    var usuariosOld = db.ClienteUsuario.Where(x => x.cliente.idCliente == cliente.idCliente).ToList();
                    foreach (ClienteUsuario item in usuariosOld.ToList())
                    {
                        db.ClienteUsuario.Remove(item);
                    }

                    string[] usuariosNew = Request["usuarios"].Split(',');
                    for (int i = 0; i < usuariosNew.Length; i++)
                    {
                        AspNetUsers usuario = db.AspNetUsers.Find(usuariosNew[i]);
                        ClienteUsuario clienteUsuario = new ClienteUsuario { cliente = cliente, usuario = usuario, fechaAsignado = DateTime.Today };
                        db.ClienteUsuario.Add(clienteUsuario);

                    }
                    db.SaveChanges();
                }


                return RedirectToAction("Index");
            }
            //Cliente clienteBD = db.Cliente.AsNoTracking().Where(c => c.idCliente == cliente.idCliente).FirstOrDefault();
            // opciones seleccionadas
            if (cliente.clienteGiros != null)
            {
                ViewBag.girosSeleccionados = new JavaScriptSerializer().Serialize(cliente.clienteGiros.Select(c => c.giro.idGiro).ToList());
            }
            else
            {
                ViewBag.girosSeleccionados = new JavaScriptSerializer().Serialize("");
            }
            if (cliente.representanteLegals != null)
            {
                ViewBag.representanteLegalSeleccionados = new JavaScriptSerializer().Serialize(cliente.representanteLegals.Select(c => c.contacto.idContacto).ToList());
            }
            else
            {
                ViewBag.representanteLegalSeleccionados = new JavaScriptSerializer().Serialize("");
            }
            if (cliente.encargadoPagos != null)
            {
                ViewBag.encargadoPagosSeleccionados = new JavaScriptSerializer().Serialize(cliente.encargadoPagos.Select(c => c.contacto.idContacto).ToList());
            }
            else
            {
                ViewBag.encargadoPagosSeleccionados = new JavaScriptSerializer().Serialize("");
            }
            if (cliente.clienteTipoDocumentosPagos != null)
            {
                ViewBag.tiposDocumentosPagoSeleccionados = new JavaScriptSerializer().Serialize(cliente.clienteTipoDocumentosPagos.Select(c => c.tipoDocumentosPago.idTipoDocumentosPago).ToList());
            }
            else
            {
                ViewBag.tiposDocumentosPagoSeleccionados = new JavaScriptSerializer().Serialize("");
            }
            // opciones disponibles (incluye seleccionadas)
            if (cliente.clienteGiros != null)
            {
                ViewBag.Giros = GetGirosCliente(cliente);
            }
            else
            {
                ViewBag.Giros = GetGiros();
            }
            if (cliente.clienteTipoDocumentosPagos != null)
            {
                ViewBag.TiposDocumentosPago = GetTiposDocumentosPagoCliente(cliente);
            }
            else
            {
                ViewBag.TiposDocumentosPago = GetTipoDocumentoPago();
            }
            if (cliente.representanteLegals != null)
            {
                ViewBag.RepresentanteLegal = GetRepresentantesLegalesCliente(cliente);
            }
            else
            {
                ViewBag.RepresentanteLegal = GetContactos();
            }
            if (cliente.encargadoPagos != null)
            {
                ViewBag.EncargadoPagos = GetEncargadosPagosCliente(cliente);
            }
            else
            {
                ViewBag.EncargadoPagos = GetContactos();
            }
            if (cliente.mandante != null)
            {
                ViewBag.Mandante = GetMandanteCliente(cliente);
            }
            else
            {
                ViewBag.Mandante = GetMandante();
            }
            ViewBag.faenas = Getfaenas();
            int[] faenas = Array.ConvertAll(Request["faena"].Split(','), int.Parse);
            if (faenas != null)
            {

                ViewBag.faenasSeleccionados = new JavaScriptSerializer().Serialize(faenas);
            }
            else
            {
                ViewBag.faenasSeleccionados = new JavaScriptSerializer().Serialize("");
            }
            return View(cliente);
        }

        //// GET: Cliente/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Cliente cliente = db.Cliente.Find(id);
        //    if (cliente == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(cliente);
        //}

        // POST: Cliente/Delete/5
        [CustomAuthorize(new string[] { "/Cliente/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Cliente cliente = db.Cliente.Find(id);
            cliente.softDelete = true;
            db.Entry(cliente).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Relators/R43/5
        public ActionResult R43(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // obtener el cliente del a base de datos
            var cliente = db.Cliente.Find(id);
            if (cliente == null)
            {
                return HttpNotFound();
            }
            return View(cliente);
        }

        // GET: Relator/LlenarR43/5
        public ActionResult LlenarR43(int? id, int? id2, TipoFormulario id3)
        {
            if (id == null || id2 == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var cliente = db.Cliente.Find(id2);
            if (cliente == null)
            {
                return HttpNotFound();
            }
            var r43 = db.R43.Find(id);
            if (r43 == null)
            {
                r43 = new R43();
                r43.encuesta = new Encuesta();
                r43.encuesta.seccionEncuesta = new List<SeccionEncuesta>();
                r43.cliente = cliente;
                var idUsuario = User.Identity.GetUserId();
                r43.clienteContacto = db.ClienteContacto.Where(x => x.idCliente == id2).Where(x => x.contacto.usuario.Id == idUsuario).FirstOrDefault();
                var seccionEncuesta = new SeccionEncuesta();
                seccionEncuesta.formulario = db.Formulario.Where(r => r.tipoFormulario == id3).Where(x => x.softDelete == false).FirstOrDefault();
                seccionEncuesta.posicion = 0;
                r43.encuesta.seccionEncuesta.Add(seccionEncuesta);
                r43.encuesta.respuestas = new List<RespuestasContestadasFormulario>();
                if (seccionEncuesta.formulario == null)
                {
                    return RedirectToAction("LandingPage", "ClienteContacto", new { error = "No existe un formulario r43 o r43_E" });
                }
            }
            return View(r43);
        }

        // POST: Relator/LlenarR43
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LlenarR43(int idR43, int idCliente, TipoFormulario tipoFormulario)
        {
            var r43 = db.R43.Find(idR43);
            if (r43 == null)
            {
                r43 = new R43();
                r43.encuesta = new Encuesta();
                r43.encuesta.seccionEncuesta = new List<SeccionEncuesta>();
                r43.cliente = db.Cliente.Find(idCliente);
                var idUsuario = User.Identity.GetUserId();
                r43.clienteContacto = db.ClienteContacto.Where(x => x.idCliente == idCliente).Where(x => x.contacto.usuario.Id == idUsuario).FirstOrDefault();
                var seccionEncuesta = new SeccionEncuesta();
                seccionEncuesta.formulario = db.Formulario.Where(r => r.tipoFormulario == tipoFormulario).Where(x => x.softDelete == false).FirstOrDefault();
                seccionEncuesta.posicion = 0;
                r43.encuesta.seccionEncuesta.Add(seccionEncuesta);
                r43.encuesta.respuestas = new List<RespuestasContestadasFormulario>();
            }
            foreach (var seccionEncuesta in r43.encuesta.seccionEncuesta)
            {
                foreach (var pregunta in seccionEncuesta.formulario.preguntasFormularios)
                {
                    if ((Request[pregunta.idPreguntasFormulario.ToString()] == null
                        || Request[pregunta.idPreguntasFormulario.ToString()] == "") && pregunta.obligatoria)
                    {
                        ModelState.AddModelError("", "Se deben responder todas las preguntas con *");
                        return View(r43);
                    }
                    // guardar respuesta
                    var respuesta = new RespuestasContestadasFormulario();
                    // si es alternativa recive el id de la respuesta seleccionada
                    if (pregunta.tipo == TipoPregunta.Alternativa)
                    {
                        if (Request[pregunta.idPreguntasFormulario.ToString()] != null)
                        {
                            respuesta.respuestaFormulario = db.RespuestasFormulario.Find(int.Parse(Request[pregunta.idPreguntasFormulario.ToString()]));
                            respuesta.respuesta = respuesta.respuestaFormulario.puntaje.ToString();
                        }
                    }
                    else
                    {
                        respuesta.respuestaFormulario = pregunta.respuestaFormulario.FirstOrDefault();
                        respuesta.respuesta = Request[pregunta.idPreguntasFormulario.ToString()];
                    }
                    respuesta.contacto = r43.clienteContacto.contacto;
                    respuesta.pregunta = pregunta;
                    r43.encuesta.respuestas.Add(respuesta);
                    db.RespuestasContestadasFormulario.Add(respuesta);
                    // eliminar respuesta si ya existe
                    var respuestaBD = db.RespuestasContestadasFormulario
                        .Where(r => r.pregunta.idPreguntasFormulario == pregunta.idPreguntasFormulario)
                        .Where(r => r.encuesta.idEncuesta == r43.encuesta.idEncuesta)
                        .FirstOrDefault();
                    if (respuestaBD != null)
                    {
                        db.RespuestasContestadasFormulario.Remove(respuestaBD);
                    }
                }
            }
            r43.fecha = DateTime.Now;
            if (idR43 == 0)
            {
                db.R43.Add(r43);
            }
            else
            {
                db.Entry(r43).State = EntityState.Modified;
            }
            if (r43.encuesta.seccionEncuesta.FirstOrDefault().formulario.tipoFormulario == TipoFormulario.R43)
            {
                r43.cliente.encuestaSatisfaccion = false;
            }
            else
            {
                r43.cliente.encuestaSatisfaccionElerning = false;
            }
            db.Entry(r43.cliente).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Landingpage", "ClienteContacto", new { });
        }

        // GET: Relator/VerR43/5
        public ActionResult VerR43(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var r43 = db.R43.Find(id);
            if (r43 == null)
            {
                return HttpNotFound();
            }
            return View(r43);
        }

        //public object DataR43(R43 r43)
        //{
        //    var data = new
        //    {
        //        fechaHoy = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
        //        fecha = r43.fecha.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture)
        //    };
        //    return data;
        //}

        //public ActionResult DescargarR16(int? id)
        //{
        //    var r16 = db.R16.Find(id);
        //    if (r16 == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    // descargar template
        //    var template = db.Template
        //        .Where(t => t.nombre == "r16")
        //        .Where(t => t.tipo == TipoTemplate.word)
        //        .FirstOrDefault();
        //    if (template == null)
        //    {
        //        // indicar q hubo un error
        //        ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r16\" y tipo \"word\".");
        //        return View("R16", r16.relator);
        //    }
        //    return RedirectToAction("GenerarReporteR16", new { id });
        //}

        //[EnableJsReport()]
        //public ActionResult GenerarReporteR16(int? id)
        //{
        //    var r16 = db.R16.Find(id);
        //    // descargar template
        //    var template = db.Template
        //        .Where(t => t.nombre == "r16")
        //        .Where(t => t.tipo == TipoTemplate.word)
        //        .FirstOrDefault();
        //    if (template == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
        //    var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
        //    var report = HttpContext
        //        .JsReportFeature()
        //        .Recipe(Recipe.Docx)
        //        .Engine(Engine.Handlebars)
        //        .Configure((r) => r.Template.Docx = new Docx
        //        {
        //            TemplateAsset = new Asset
        //            {
        //                Content = base64,
        //                Encoding = "base64"
        //            }
        //        })
        //        .Configure((r) => r.Data = DataR16(r16))
        //        .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"r16_" + r16.idR16 + "_" + r16.relator.contacto.run + ".docx\"");
        //    return null;
        //}

        //public ActionResult GenerarPdfR16(int? id)
        //{
        //    var r16 = db.R16.Find(id);
        //    if (r16 == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    // descargar template
        //    var template = db.Template
        //        .Where(t => t.nombre == "r16")
        //        .Where(t => t.tipo == TipoTemplate.word)
        //        .FirstOrDefault();
        //    if (template == null)
        //    {
        //        // indicar q hubo un error
        //        ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r16\" y tipo \"word\".");
        //        return View("R16", r16.relator);
        //    }

        //    string hash = "";
        //    using (SHA256 sha256Hash = SHA256.Create())
        //    {
        //        hash = Utils.Utils.GetHash(sha256Hash, DateTime.Now.ToString());
        //    }

        //    string createRequest = Url.Action("GenerarReportePdfR16", "Relator", new { id, id2 = hash }, Request.Url.Scheme);
        //    // Generate Request
        //    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(createRequest);
        //    req.Method = "GET";

        //    // Get the Response
        //    try
        //    {
        //        HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
        //    }
        //    catch (WebException e)
        //    {
        //        return View("Error", (object)"No se pudo generar el documento.");
        //    }

        //    var path = directory + hash;
        //    Byte[] bytes = System.IO.File.ReadAllBytes(path + ".pdf");

        //    System.IO.File.Delete(path + ".pdf");

        //    Response.ContentType = "application/pdf";
        //    Response.AppendHeader("Content-Disposition", "attachment; filename=r16_" + r16.idR16 + "_" + r16.relator.contacto.run + ".pdf");

        //    return new FileContentResult(bytes, "application/pdf");
        //}

        //[EnableJsReport()]
        //public ActionResult GenerarReportePdfR16(int? id, string id2)
        //{
        //    var r16 = db.R16.Find(id);
        //    // descargar template
        //    var template = db.Template
        //        .Where(t => t.nombre == "r16")
        //        .Where(t => t.tipo == TipoTemplate.word)
        //        .FirstOrDefault();
        //    if (template == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
        //    var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
        //    var report = HttpContext
        //        .JsReportFeature()
        //        .Recipe(Recipe.Docx)
        //        .Engine(Engine.Handlebars)
        //        .Configure((r) => r.Template.Docx = new Docx
        //        {
        //            TemplateAsset = new Asset
        //            {
        //                Content = base64,
        //                Encoding = "base64"
        //            }
        //        })
        //        .Configure((r) => r.Data = DataR16(r16))
        //        .OnAfterRender((r) =>
        //        {
        //            var path = directory + id2;
        //            using (var file = System.IO.File.Open(path + ".docx", FileMode.Create))
        //            {
        //                r.Content.CopyTo(file);
        //            }
        //            var appWord = new Microsoft.Office.Interop.Word.Application();
        //            var wordDocument = appWord.Documents.Open(path + ".docx");
        //            wordDocument.ExportAsFixedFormat(path + ".pdf", Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF);
        //            wordDocument.Close();
        //            appWord.Quit();
        //            System.IO.File.Delete(path + ".docx");
        //        });
        //    return null;
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public MultiSelectList GetGiros()
        {
            return new MultiSelectList(db.Giro.Where(x => x.softDelete == false).Select(c => new SelectListItem
            {
                Text = c.descripcion,
                Value = c.idGiro.ToString()
            }).ToList(), "Value", "Text");
        }

        public MultiSelectList GetContactos()
        {
            return new MultiSelectList(Utils.Utils.GetContactosDesocupados(db)
            .Select(con => new SelectListItem
            {
                Text = "[" + con.run + "]" + " " + con.nombres + " " + con.apellidoPaterno + " " + con.apellidoMaterno,
                Value = con.idContacto.ToString()
            }).ToList(), "Value", "Text");
        }

        public MultiSelectList GetTipoDocumentoPago()
        {
            return new MultiSelectList(db.TiposDocumentosPago.Where(x => x.softDelete == false).Select(c => new SelectListItem
            {
                Text = c.nombre,
                Value = c.idTipoDocumentosPago.ToString()
            }).ToList(), "Value", "Text");
        }

        public SelectList GetMandante()
        {
            return new SelectList(db.Mandante.Where(x => x.softDelete == false).Select(c => new SelectListItem
            {
                Text = c.nombreMandante + " [" + c.rut + "]",
                Value = c.idMandante.ToString()
            }).ToList(), "Value", "Text");
        }
        public SelectList GetUsuarios()
        {
            return new SelectList(db.AspNetUsers
                .Where(x => x.tipo == TipoUsuario.completo)
                .Where(x => x.AspNetRoles
                .Any(y => !y.Name.Contains("Representante Empresa")
                && !y.Name.Contains("Participante")
                && !y.Name.Contains("APOYO TMS")
                && !y.Name.Contains("Relator")))
                .Select(c => new SelectListItem
                {
                    Text = c.nombres + " " + c.apellidoPaterno,
                    Value = c.Id.ToString()
                }).ToList(), "Value", "Text");
        }
        public SelectList Getfaenas()
        {
            return new SelectList(db.Faena.Where(x => x.softDelete == false).Select(c => new SelectListItem
            {
                Text = c.nombre,
                Value = c.idFaena.ToString()
            }).ToList(), "Value", "Text");
        }
        public MultiSelectList GetGirosCliente(Cliente cliente)
        {
            List<Giro> giros = db.Giro.Where(x => x.softDelete == false).ToList();
            List<ClienteGiro> girosCliente = cliente.clienteGiros.ToList();
            foreach (var giro in girosCliente)
            {
                if (!giros.Contains(giro.giro))
                {
                    giros.Add(giro.giro);
                }
            }
            return new MultiSelectList(giros.Select(c => new SelectListItem
            {
                Text = c.descripcion,
                Value = c.idGiro.ToString()
            }).ToList(), "Value", "Text");
        }

        public MultiSelectList GetEncargadosPagosCliente(Cliente cliente)
        {
            List<Contacto> contactos = Utils.Utils.GetContactosDesocupados(db);
            List<EncargadoPago> encargadosPagos = cliente.encargadoPagos.ToList();
            foreach (var contacto in encargadosPagos)
            {
                if (!contactos.Contains(contacto.contacto))
                {
                    contactos.Add(contacto.contacto);
                }
            }
            return new SelectList(contactos.Select(con => new SelectListItem
            {
                Text = "[" + con.run + "]" + " " + con.nombres + " " + con.apellidoPaterno + " " + con.apellidoMaterno,
                Value = con.idContacto.ToString()
            }).ToList(), "Value", "Text");
        }

        public MultiSelectList GetRepresentantesLegalesCliente(Cliente cliente)
        {
            List<Contacto> contactos = Utils.Utils.GetContactosDesocupados(db);
            List<RepresentanteLegal> representantesLegales = cliente.representanteLegals.ToList();
            foreach (var contacto in representantesLegales)
            {
                if (!contactos.Contains(contacto.contacto))
                {
                    contactos.Add(contacto.contacto);
                }
            }
            return new SelectList(contactos.Select(c => new SelectListItem
            {
                Text = "[" + c.run + "]" + " " + c.nombres + " " + c.apellidoPaterno + " " + c.apellidoMaterno,
                Value = c.idContacto.ToString()
            }).ToList(), "Value", "Text");
        }

        public MultiSelectList GetTiposDocumentosPagoCliente(Cliente cliente)
        {
            List<TiposDocumentosPago> tiposdocumentospago = db.TiposDocumentosPago.Where(x => x.softDelete == false).ToList();
            List<ClienteTipoDocumentosPago> tiposdocumentospagoCliente = cliente.clienteTipoDocumentosPagos.ToList();
            foreach (var tipoDocumentosPago in tiposdocumentospagoCliente)
            {
                if (!tiposdocumentospago.Contains(tipoDocumentosPago.tipoDocumentosPago))
                {
                    tiposdocumentospago.Add(tipoDocumentosPago.tipoDocumentosPago);
                }
            }
            return new MultiSelectList(tiposdocumentospago.Select(c => new SelectListItem
            {
                Text = c.nombre,
                Value = c.idTipoDocumentosPago.ToString()
            }).ToList(), "Value", "Text");
        }

        public SelectList GetMandanteCliente(Cliente cliente)
        {
            List<Mandante> mandantes = db.Mandante.Where(x => x.softDelete == false).ToList();
            Mandante mandante = cliente.mandante;
            if (!mandantes.Contains(mandante))
            {
                mandantes.Add(mandante);
            }
            return new SelectList(mandantes.Select(c => new SelectListItem
            {
                Text = c.nombreMandante + " [" + c.rut + "]",
                Value = c.idMandante.ToString()
            }).ToList(), "Value", "Text");
        }

        public SelectList GetUsuariosCliente(Cliente cliente)
        {
            List<AspNetUsers> usuarios = db.AspNetUsers.Where(x => x.tipo == TipoUsuario.completo)
                .Where(x => x.AspNetRoles
                .Any(y => !y.Name.Contains("Representante Empresa")
                && !y.Name.Contains("Participante")
                && !y.Name.Contains("APOYO TMS")
                && !y.Name.Contains("Relator"))).ToList();
            List<ClienteUsuario> clientesUsuarios = cliente.usuariosAsignados.ToList();

            foreach (var usuario in clientesUsuarios)
            {
                if (!usuarios.Contains(usuario.usuario))
                {
                    usuarios.Add(usuario.usuario);
                }
            }
            return new SelectList(usuarios.Select(c => new SelectListItem
            {
                Text = c.nombres + " " + c.apellidoPaterno,
                Value = c.Id.ToString()
            }).ToList(), "Value", "Text");
        }
    }
}
