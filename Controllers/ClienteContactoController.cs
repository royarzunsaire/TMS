using jsreport.MVC;
using jsreport.Types;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SGC.CustomAuthorize;
using SGC.Models;
using SGC.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class ClienteContactoController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: ClienteContacto
        //public ActionResult Index()
        //{
        //    var clienteContacto = db.ClienteContacto.Include(c => c.cliente).Include(c => c.contacto);
        //    return View(clienteContacto);
        //}

        //GET: ClienteContacto/LandingPage
        public ClienteContactoController() { }

        public ClienteContactoController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        //*

        //
        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }





        [CustomAuthorize(new string[] { "/ClienteContacto/" })]
        public ActionResult LandingPage(string error)
        {
            var idUser = User.Identity.GetUserId();
            var clienteContacto = db.ClienteContacto
                .Where(x => x.contacto.usuario.Id == idUser)
                .ToList();
            if (clienteContacto == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var clienteContactoVM = new ViewModelLandingPageCliente();
            clienteContactoVM.clienteContacto = clienteContacto.FirstOrDefault();

            

            clienteContactoVM.comercializaciones = new List<Comercializacion>();
            clienteContactoVM.cotizaciones = new List<Cotizacion_R13>();
            clienteContactoVM.salidasTerreno = new List<SalidaTerreno>();
       
            foreach (ClienteContacto item in clienteContacto)
            {
                if (item.cliente.situacionComercial == SituacionComercial.Pendiente)
                {
                    ModelState.AddModelError("","El cliente "+ item.cliente.nombreEmpresa + " tiene sitación comercial en estado pendiente");
                    ModelState.AddModelError("", "Favor contactarse con su ejecutivo comercial para regularizar pagos u ordenes de compras pendientes.");
                    
                }
                else {
                    clienteContactoVM.comercializaciones.AddRange(db.Comercializacion
           .Where(x => x.softDelete == false)
           .Where(x => x.cotizacion.cliente.idCliente == item.cliente.idCliente)
           .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion != EstadoComercializacion.Borrador
               && x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion != EstadoComercializacion.Cancelada
               && x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion != EstadoComercializacion.Deshabilitada)
           .OrderByDescending(x => x.fechaInicio)
           .ToList());

                    clienteContactoVM.cotizaciones.AddRange(db.Cotizacion_R13
                        .Where(x => x.softDelete == false)
                        .Where(x => x.cliente.idCliente == item.cliente.idCliente)
                        .OrderByDescending(x => x.fechaCreacion)
                        .ToList());


                    clienteContactoVM.salidasTerreno.AddRange(db.SalidaTerreno
                  .Where(x => x.cliente.idCliente == item.cliente.idCliente)
                  .Where(x => x.estado == EstadoSalidaTerreno.Programado || x.estado == EstadoSalidaTerreno.Reprogramado)
                  .OrderBy(x => x.fecha)
                  .ThenBy(x => x.hora)
                  .ToList());
                }

             

            }
          
            foreach (var item in clienteContactoVM.cotizaciones)
            {
                item.costo = db.Costo.Where(x => x.idCotizacion == item.idCotizacion_R13).ToList();
            }
            clienteContactoVM.comercializaciones = clienteContactoVM.comercializaciones.OrderByDescending(x => x.fechaInicio).ToList();
            clienteContactoVM.cotizaciones = clienteContactoVM.cotizaciones.OrderByDescending(x => x.fechaInicio).ToList();
            var cursos = db.Curso
                .Where(x => x.softDelete == false)
                .OrderBy(x => x.nombreCurso)
                .ToList();
            clienteContactoVM.cursos = new List<ViewModelCurso>();
            //foreach (var curso in cursos)
            //{
            //    var cursoVM = new ViewModelCurso();
            //    cursoVM.curso = curso;
            //    cursoVM.r11 = db.R11.Where(x => x.idCurso == curso.idCurso).FirstOrDefault();
            //    if (cursoVM.r11 != null)
            //    {
            //        clienteContactoVM.cursos.Add(cursoVM);
            //    }
            //}
            //clienteContactoVM.participantes = db.Participante
            //    .Where(x => x.clien.comercializacion.cotizacion.idCliente == clienteContacto.cliente.idCliente)
            //    .ToList();
            if (error != null && error != "")
            {
                ModelState.AddModelError("", error);
            }
            var now = DateTime.Now;
            var publicidades = db.Publicidad.Where(x => x.tipo.Contains("Todos") || x.publicidadClientes.Any(y => y.cliente.clienteContactos.Any(z => z.contacto.usuario.Id == idUser)))
                  .Where(x => DbFunctions.TruncateTime(x.vigencia) >= now )
                .ToList();
            ViewBag.publicidades = publicidades;
            return View(clienteContactoVM);
        }

        [CustomAuthorize(new string[] { "/ClienteContacto/" })]
        public ActionResult ModalParticipanteCliente(int idComercializacion)
        {
            Comercializacion comercializacion = db.Comercializacion.Find(idComercializacion);
            MoodleSearchUserGrades notasParticipantes = null;
            if (comercializacion != null)
            {
                notasParticipantes = Moodle.GetNotasGrupoMoodle(comercializacion, db.ParametrosMoodles.FirstOrDefault());
            }
            if (comercializacion == null || comercializacion.participantes.Count == 0)
                return PartialView(null);
            ParticipanteController participanteController = new ParticipanteController();
           
            if (notasParticipantes != null )
                participanteController.updateNotas(notasParticipantes, comercializacion.idComercializacion, false, db.AspNetUsers.Find(User.Identity.GetUserId()));
            comercializacion = db.Comercializacion.Find(idComercializacion);

            return PartialView(comercializacion);
        }
        [HttpGet]
        public bool CorreoClienteEncuestaData()
        {
            var hoy = DateTime.Now.Date.AddMonths(-6);
            var clientes = db.Comercializacion
                .Where(x => x.softDelete == false)
                .Where(x => x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Asincrono
                || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Elearning_Sincrono
                || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Asincronica
                || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion_Sincronica
                || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Presencial
                || x.cotizacion.curso.tipoEjecucion == TipoEjecucion.Recertificacion)
                .Where(x => DbFunctions.TruncateTime(x.fechaCreacion) >= hoy)
                .Where(x => DbFunctions.TruncateTime(x.cotizacion.cliente.fechaAlertaEncuestaSatisfaccion) <= hoy || x.cotizacion.cliente.encuestaSatisfaccion || x.cotizacion.cliente.encuestaSatisfaccionElerning)
                
                .Select(x => x.cotizacion.cliente)
                .Distinct()
                .ToList();
            foreach (var cliente in clientes)
            {
                try
                {
                    CorreoClienteEncuesta(cliente);
                }
                catch (Exception e) {
                }
            
            }
            return true;
        }
        [HttpGet]
        private async Task<string> CorreoClienteEncuesta(Cliente cliente)
        {
            
            if (cliente == null )
            {
                return "Comercializacion o cliente no encontrado";
            }
            string error = "";
            List <Contacto> contactos = GetContactosCliente(cliente).Where(x => x.usuario != null).ToList();
            var copy = new MailAddress("insecap@gmail.com", "Insecap");
            foreach (var contacto in contactos) {
               
                var receiverEmail = new MailAddress(contacto.usuario.Email, contacto.nombreCompleto);
                 var roles = await UserManager.GetRolesAsync(contacto.usuario.Id);
                if (roles != null && roles.Count > 0 && roles.Contains("Representante Empresa")) {
                    var bodyHTML = "";
                    var subject = "INSECAP // ENCUESTA DE SATISFACCION// {0}";
                    //Reemplazar valores asunto
                    subject = string.Format(subject,
                        cliente.nombreEmpresa.ToUpper()
                        );
                    using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/encuesta.html")))
                    {
                        bodyHTML = reader.ReadToEnd();
                    }

                    var body  = bodyHTML.Replace("{0}", cliente.nombreEmpresa);
                    body = body.Replace("{1}", contacto.nombreCompleto);
                    body = body.Replace("{2}", contacto.usuario.Email);
                    var message = Utils.Utils.SendMail(receiverEmail, subject, body, copy);
                    //var message = "";
                    if (message != "ok")
                    {
                        error += "Al enviar al correo " + contacto.usuario.Email + " se generó el error: " + message;
                     }

                }
                


            }




           
            return error;
        }















            //[CustomAuthorize(new string[] { "/ClienteContacto/Comercializaciones/" })]
            //public ActionResult Comercializaciones()
            //{
            //    var cantidad = db.AspNetUsers.Where(x => x.Email == User.Identity.Name).ToList().Count();
            //    if (cantidad == 0)
            //    {
            //        return Redirect("~/");

            //    }
            //    var usuario = db.AspNetUsers.Where(x => x.Email == User.Identity.Name).ToList().First();

            //    cantidad = db.Contacto.Where(x => x.usuario.Id == usuario.Id).ToList().Count();

            //    if (cantidad == 0)
            //    {
            //        return Redirect("~/");
            //    }
            //    Contacto contactoRepresentateEmpresa = db.Contacto.Where(x => x.usuario.Id == usuario.Id).ToList()[0];
            //    var idClienteRepresentanteEmpresa = db.ClienteContacto.Where(x => x.idContacto == contactoRepresentateEmpresa.idContacto).Select(y => y.idCliente).ToList();
            //    viewModelRepresentanteEmpresa viewModel = new viewModelRepresentanteEmpresa();
            //    var idCotizaciones = db.Cotizacion_R13.Where(x => idClienteRepresentanteEmpresa.Contains(x.idCliente)).Select(y => y.idCotizacion_R13).ToList();

            //    viewModel.comercializaciones = db.Comercializacion.Where(x => idCotizaciones.Contains(x.cotizacion.idCotizacion_R13)).ToList();

            //    return View(viewModel.comercializaciones
            //        .Where(x => x.softDelete == false)
            //        .ToList());
            //}

            ////public JsonResult ObtenerNotasAsitenciaGeneral(int id)
            ////{
            ////    db.Configuration.ProxyCreationEnabled = false;

            ////    var participantes = db.Participante.Where(x => x.comercializacion.idComercializacion == id).Include(c => c.contacto).ToList();


            ////    List<object> datos = new List<object>();
            ////    string nombreParticipante = null;
            ////    string rutPartcipante = null;

            ////    foreach (var itemParticipante in participantes)
            ////    {
            ////        rutPartcipante = itemParticipante.contacto.run;
            ////        nombreParticipante = itemParticipante.contacto.nombreCompleto;
            ////        //Recorrer por cada foreach la cosas bucando a cada rato
            ////        db.Configuration.ProxyCreationEnabled = false;

            ////        List<Notas> notas = new List<Notas>();
            ////        foreach(var itemNotas in db.Notas.Where(x => x.idParticipante == itemParticipante.idParticipante).ToList())
            ////        {
            ////            itemNotas.participante = null;
            ////            notas.Add(itemNotas);
            ////        }
            ////        List<Asistencia> asistencias = new List<Asistencia>();
            ////        foreach (var itemAsistencias in db.Asistencias.Where(x => x.idParticipante == itemParticipante.idParticipante).Include(c => c.bloque).ToList())
            ////        {
            ////            itemAsistencias.participante = null;

            ////            asistencias.Add(itemAsistencias);
            ////        }
            ////        datos.Add(new { rut = rutPartcipante, nombre = nombreParticipante, notas, asistencias });

            ////    }
            ////    return Json(datos);
            ////}

            //[CustomAuthorize(new string[] { "/ClienteContacto/Cotizaciones/" })]
            //public ActionResult Cotizaciones()
            //{
            //    if (db.AspNetUsers.Where(x => x.Email == User.Identity.Name).Count() == 0)
            //    {
            //        return Redirect("~/");

            //    }
            //    var usuario = db.AspNetUsers.Where(x => x.Email == User.Identity.Name).ToList().First();


            //    if (db.Contacto.Where(x => x.usuario.Id == usuario.Id).Count() ==  0 ){
            //        return Redirect("~/");
            //    }
            //    Contacto contactoRepresentateEmpresa = db.Contacto.Where(x => x.usuario.Id == usuario.Id).ToList()[0];
            //    var idClienteRepresentanteEmpresa = db.ClienteContacto.Where(x => x.idContacto == contactoRepresentateEmpresa.idContacto).Select(y => y.idCliente).ToList();
            //    viewModelRepresentanteEmpresa viewModel = new viewModelRepresentanteEmpresa();
            //    viewModel.cotizaciones = db.Cotizacion_R13.Where(c => c.softDelete == false).Where(x => idClienteRepresentanteEmpresa.Contains(x.idCliente) ).Include(c => c.costo).ToList();

            //    return View(viewModel.cotizaciones.Where(x => x.isAutoCotizacion == 1));
            //}

            [CustomAuthorize(new string[] { "/ClienteContacto/", "/ClienteContacto/CrearAutoCotizacion/" })]
        public ActionResult CrearAutoCotizacion()
        {
            var idUser = User.Identity.GetUserId();
            var clienteContacto = db.ClienteContacto
                .Where(x => x.contacto.usuario.Id == idUser)
                .FirstOrDefault();
            if (clienteContacto == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            db.Configuration.ProxyCreationEnabled = false;
            ViewModelCotizacion mymodel = new ViewModelCotizacion();
            mymodel.clientes = db.Cliente.Where(x => x.idCliente == clienteContacto.idCliente).ToList();
            mymodel.clientes.FirstOrDefault().clienteContactos = new List<ClienteContacto>();
            ViewBag.sucursales = GetSucursales();
            return View(mymodel);
        }

        [CustomAuthorize(new string[] { "/ClienteContacto/", "/ClienteContacto/CrearAutoCotizacion/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearAutoCotizacion(ViewModelCotizacion myModel)
        {
            Cotizacion_R13 cotizacion = myModel.cotizacion;
            cotizacion.sucursal = db.Sucursal.Find(cotizacion.sucursal.idSucursal);
            if (cotizacion.idCurso == 0)
            {
                ModelState.AddModelError("cotizacion.idCurso", "El campo Curso es obligatorio");
            }
            ModelState.Remove("cotizacion.nombreDiploma");
            ModelState.Remove("cotizacion.contactoEncargadoPago");
            ModelState.Remove("cotizacion.contacto");
            ModelState.Remove("cotizacion.giro");

            if (ModelState.IsValid)
            {
                myModel.cotizacion.contactoEncargadoPago = 0;
                myModel.cotizacion.contacto = 0;
                myModel.cotizacion.giro = "Ninguno";
                myModel.cotizacion.nombreDiploma = db.Curso
                    .Where(x => x.idCurso == myModel.cotizacion.idCurso)
                    .FirstOrDefault().nombreCurso;
                myModel.cotizacion.isAutoCotizacion = 1;
                cotizacion.fechaCreacion = DateTime.Now;
                // codigo auto cotizacion
                var ultimoCodigo = "0";
                if (db.Cotizacion_R13.OrderByDescending(x => x.idCotizacion_R13).FirstOrDefault() != null)
                {
                    ultimoCodigo = db.Cotizacion_R13.OrderByDescending(x => x.idCotizacion_R13).FirstOrDefault().codigoCotizacion;
                }
                cotizacion.codigoCotizacion = Helpers.CustomUtilsHelper.GeneracionCodigo(cotizacion.sucursal.prefijoCodigo, ultimoCodigo);
                cotizacion.softDelete = false;
                var userId = User.Identity.GetUserId();
                cotizacion.usuarioCreador = db.AspNetUsers.Find(userId);
                db.Cotizacion_R13.Add(cotizacion);
                db.SaveChanges();
                // notificacion auto cotizacion creada
                //var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta Nueva Auto Cotización").FirstOrDefault();
                //if (notificacionConfig != null)
                //{
                //    notificacionConfig.CrearNotificacion(db, myModel.cotizacion.codigoCotizacion, myModel.cotizacion.idCotizacion_R13.ToString(), User.Identity.GetUserId());
                //}
                if (myModel.cotizacion.costo != null)
                {
                    foreach (Costo item in myModel.cotizacion.costo)
                    {
                        item.idCotizacion = cotizacion.idCotizacion_R13;
                        db.Costo.Add(item);
                        db.SaveChanges();
                    }
                }
                return RedirectToAction("LandingPage");
            }
            db.Configuration.ProxyCreationEnabled = false;
            myModel.clientes = db.Cliente.Where(x => x.idCliente == myModel.cotizacion.idCliente).ToList();
            ViewBag.sucursales = GetSucursales();
            return View(myModel);
        }

        //[CustomAuthorize(new string[] { "/ClienteContacto/Cotizaciones/" })]
        //public ActionResult EditarAutoCotizacion(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Cotizacion_R13 cotizacion_R13 = db.Cotizacion_R13.Find(id);
        //    if (cotizacion_R13 == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    db.Configuration.ProxyCreationEnabled = false;
        //    ViewModelCotizacion mymodel = new ViewModelCotizacion();
        //    mymodel.cotizacion = cotizacion_R13;
        //    mymodel.clientes = db.Cliente.ToList();

        //    return View(mymodel);
        //}

        //[CustomAuthorize(new string[] { "/ClienteContacto/Cotizaciones/" })]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult EditarAutoCotizacion(ViewModelCotizacion myModel)
        //{
        //    ModelState.Remove("cotizacion.nombreDiploma");
        //    ModelState.Remove("cotizacion.contactoEncargadoPago");
        //    ModelState.Remove("cotizacion.contacto");
        //    ModelState.Remove("cotizacion.giro");
        //    if (ModelState.IsValid)
        //    {
        //        Cotizacion_R13 cotizacion_r13 = db.Cotizacion_R13.Where(x => x.idCotizacion_R13 == myModel.cotizacion.idCotizacion_R13).First();

        //        myModel.cotizacion.contactoEncargadoPago = cotizacion_r13.contactoEncargadoPago;
        //        myModel.cotizacion.contacto = cotizacion_r13.contacto;
        //        myModel.cotizacion.giro = cotizacion_r13.giro;
        //        myModel.cotizacion.nombreDiploma = cotizacion_r13.nombreDiploma;
        //        myModel.cotizacion.isAutoCotizacion = 1;
        //        Cotizacion_R13 cotizacion = myModel.cotizacion;
        //        cotizacion.fechaCreacion = DateTime.Now;
        //        // codigo auto cotizacion
        //        var ultimoCodigo = "0";
        //        if (db.Curso.LastOrDefault() != null)
        //        {
        //            ultimoCodigo = db.Curso.LastOrDefault().codigoCurso;
        //        }
        //        cotizacion.codigoCotizacion = Helpers.CustomUtilsHelper.GeneracionCodigo(cotizacion.sucursal.prefijoCodigo, ultimoCodigo);
        //        db.Cotizacion_R13.Add(cotizacion);
        //        db.SaveChanges();

        //        if (myModel.cotizacion.costo != null)
        //        {
        //            foreach (Costo item in myModel.cotizacion.costo)
        //            {

        //                item.cotizacion = cotizacion;
        //                db.Costo.Add(item);
        //                db.SaveChanges();
        //            }
        //        }
        //        return RedirectToAction("Cotizaciones");
        //    }
        //    db.Configuration.ProxyCreationEnabled = false;
        //    myModel.clientes = db.Cliente.ToList();
        //    return View(myModel);
        //}

        //// GET: ClienteContacto/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ClienteContacto clienteContacto = await db.ClienteContacto.FindAsync(id);
        //    if (clienteContacto == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(clienteContacto);
        //}

        // GET: ClienteContacto/Create
        [CustomAuthorize(new string[] { "/Cliente/" })]
        public ActionResult Create(int id)
        {
            var cliente = db.Cliente.Find(id);
            var clienteContacto = new ClienteContacto();
            clienteContacto.cliente = cliente;
            //ViewBag.usuarios = GetUsuarios();
            ViewBag.contactosCliente = new SelectList(GetContactosCliente(cliente)
                .Select(con => new SelectListItem
                {
                    Text = "[" + con.run + "]" + " " + con.nombres + " " + con.apellidoPaterno + " " + con.apellidoMaterno,
                    Value = con.idContacto.ToString()
                }).ToList(), "Value", "Text");
            //ViewBag.contactos = GetContactos();
            return View(clienteContacto);
        }

        // POST: ClienteContacto/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Cliente/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ClienteContacto clienteContacto)
        {
            var error = false;

            if (clienteContacto.contacto.idContacto == 0)
            {
                ModelState.AddModelError("", "El campo Contactos es obligatorio");
                error = true;
            }

            if (db.ClienteContacto.Where(x => x.idCliente == clienteContacto.cliente.idCliente && x.idContacto == clienteContacto.contacto.idContacto).Any())
            {
                ModelState.AddModelError("", "Ya existe el usuario ");
                error = true;
            }
            //if (clienteContacto.contacto.usuario.Id == null)
            //{
            //    ModelState.AddModelError("", "El campo Usuarios es obligatorio");
            //    error = true;
            //}
            var cliente = db.Cliente.Find(clienteContacto.cliente.idCliente);
            var contacto = db.Contacto.Find(clienteContacto.contacto.idContacto);
            //var existUser = db.AspNetUsers.Where(x => x.Email == contacto.correo).Any();
            //if (!existUser)
            //{
            //    ModelState.AddModelError("", "Ya existe el usuario ");
            //    error = true;
            //}
            if (!error)
            {
                clienteContacto.cliente = cliente;
                ViewBag.contactosCliente = new SelectList(GetContactosCliente(cliente)
                .Select(con => new SelectListItem
                {
                    Text = "[" + con.run + "]" + " " + con.nombres + " " + con.apellidoPaterno + " " + con.apellidoMaterno,
                    Value = con.idContacto.ToString()
                }).ToList(), "Value", "Text");

                var clienteContactoGuardar = new ClienteContacto();
                clienteContactoGuardar.cliente = cliente;
                clienteContactoGuardar.contacto = contacto;
                clienteContactoGuardar.idCliente = cliente.idCliente;
                clienteContactoGuardar.idContacto = contacto.idContacto;
                clienteContactoGuardar.contacto.usuario = db.AspNetUsers.Where(x => x.Email == contacto.correo).FirstOrDefault();
                String[] role = { "Representante Empresa" };

                if (clienteContactoGuardar.contacto.usuario == null) {
              
                    var user = new ApplicationUser
                    {
                        UserName = contacto.correo,
                        Email = contacto.correo,
                        Address = contacto.direccion,
                        EmailConfirmed = true
                        
                    };
                   
                    var adminresult = await UserManager.CreateAsync(user, "Insecap2020!");
                    if (adminresult.Succeeded)
                    {
                       
                        var result = await UserManager.AddToRolesAsync(user.Id, role);
                        if (result.Succeeded)
                        {
                            clienteContactoGuardar.contacto.usuario = new AspNetUsers {Id = user.Id };
                            ModelState.AddModelError("", "El Registro se guardo correctamente!");
                        }
                        else
                        {
                            ModelState.AddModelError("", result.Errors.First());
                        }

                    }
           
                }
                else
                {


                        var roles = await UserManager.GetRolesAsync(clienteContactoGuardar.contacto.usuario.Id);
                        if (roles != null && roles.Count > 0)
                            await UserManager.RemoveFromRoleAsync(clienteContactoGuardar.contacto.usuario.Id, roles.FirstOrDefault());
                        var resultUpdate = await UserManager.AddToRolesAsync(clienteContactoGuardar.contacto.usuario.Id, role.FirstOrDefault());

                        if (resultUpdate.Succeeded)
                        {
                            ModelState.AddModelError("", "El Registro se guardo correctamente!");
                        }
                        else
                        {
                            ModelState.AddModelError("", resultUpdate.Errors.First());
                        }

                    
                 



                }
                var usuario = db.AspNetUsers.Find(clienteContactoGuardar.contacto.usuario.Id);
                usuario.PasswordHash = UserManager.PasswordHasher.HashPassword("Insecap2020!");
                usuario.UserName = clienteContactoGuardar.contacto.usuario.Email;
                usuario.nombres = contacto.nombres;
                    usuario.apellidoPaterno = contacto.apellidoPaterno;
                    usuario.apellidoMaterno = contacto.apellidoMaterno;
                    usuario.run = contacto.run;
                    usuario.telefono = contacto.telefono;
                    usuario.fechaNacimiento = contacto.fechaNacimiento;
                    usuario.tipo = TipoUsuario.completo;
                    clienteContactoGuardar.contacto.usuario = usuario;
                    db.Entry(usuario).State = EntityState.Modified;
                    db.SaveChanges();

                    clienteContactoGuardar.usuarioCreador = User.Identity.GetUserId();
                    clienteContactoGuardar.fechaCreacion = DateTime.Now;
                    db.ClienteContacto.Add(clienteContactoGuardar);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Create", new { id = clienteContacto.cliente.idCliente });
                
            }
           
            //ViewBag.usuarios = GetUsuarios();
            //ViewBag.contactos = GetContactos();
           
            return View(clienteContacto);
        }

        //Temporal
        public   JsonResult TempCrearUsuarios()
        {
            List<Cliente> clientes = db.Cliente.ToList();
            foreach (Cliente cliente in clientes) {
                var contactos = new SelectList(GetContactosCliente(cliente)
                .Select(con => new SelectListItem
                {
                    Text = "[" + con.run + "]" + " " + con.nombres + " " + con.apellidoPaterno + " " + con.apellidoMaterno,
                    Value = con.idContacto.ToString()
                }).ToList(), "Value", "Text");
                foreach (ClienteContactoCotizacion contactoId in cliente.clienteContactoCotizacion)
                {

                    ClienteContacto clienteContacto = new ClienteContacto {
                        idCliente = cliente.idCliente,
                        idContacto = contactoId.contacto.idContacto,
                        contacto = new Contacto { idContacto = contactoId.contacto.idContacto },
                        cliente = new Cliente { idCliente = cliente.idCliente },
                    };
                    CreateTemp(clienteContacto);
                    
                }

            }
           return Json(new { list = true }, JsonRequestBehavior.AllowGet);
        }

        private void  CreateTemp(ClienteContacto clienteContacto)
        {
            var error = false;
            if (clienteContacto.contacto.idContacto == 0 || db.ClienteContacto.Where(x => x.idCliente == clienteContacto.cliente.idCliente && x.idContacto == clienteContacto.contacto.idContacto).Any())
            {
                error = true;
            }
     
            var cliente = db.Cliente.Find(clienteContacto.cliente.idCliente);
            var contacto = db.Contacto.Find(clienteContacto.contacto.idContacto);
            var existUser = db.AspNetUsers.Where(x => x.UserName == contacto.correo).Any();
            if (!error && !existUser)
            {
                clienteContacto.cliente = cliente;
              
                var clienteContactoGuardar = new ClienteContacto();
                clienteContactoGuardar.cliente = cliente;
                clienteContactoGuardar.contacto = contacto;
                clienteContactoGuardar.idCliente = cliente.idCliente;
                clienteContactoGuardar.idContacto = contacto.idContacto;



                if (clienteContactoGuardar.contacto.usuario == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = contacto.correo,
                        Email = contacto.correo,
                        Address = contacto.direccion,
                        EmailConfirmed = true

                    };
                    String[] role = { "Representante Empresa" };

                    var adminresult =  UserManager.Create(user, "Insecap2020!");
                    if (adminresult.Succeeded)
                    {

                        var result =  UserManager.AddToRoles(user.Id, role);
                        if (result.Succeeded)
                        {
                            var usuario = db.AspNetUsers.Find(user.Id);
                            usuario.nombres = contacto.nombres;
                            usuario.apellidoPaterno = contacto.apellidoPaterno;
                            usuario.apellidoMaterno = contacto.apellidoMaterno;
                            usuario.run = contacto.run;
                            usuario.telefono = contacto.telefono;
                            usuario.fechaNacimiento = contacto.fechaNacimiento;
                            usuario.tipo = TipoUsuario.completo;
                            clienteContactoGuardar.contacto.usuario = usuario;
                            db.Entry(usuario).State = EntityState.Modified;
                            db.SaveChanges();
                           
                        }
                      

                    }
                    else
                    {
                        if (adminresult.Errors.First().Contains(user.Email))
                        {
                            clienteContactoGuardar.contacto.usuario = db.AspNetUsers.Where(x => x.Email == user.Email).FirstOrDefault();
                            Task<IList<string>> roles =  UserManager.GetRolesAsync(clienteContactoGuardar.contacto.usuario.Id);
                            IdentityResult c = null;
                            if (roles.Result.Count > 0)
                                c =   UserManager.RemoveFromRole(clienteContactoGuardar.contacto.usuario.Id, roles.Result.FirstOrDefault());
                            var resultUpdate =  UserManager.AddToRoles(clienteContactoGuardar.contacto.usuario.Id, roles.Result.FirstOrDefault());

                            


                        }
                       



                    }


                    clienteContactoGuardar.usuarioCreador = User.Identity.GetUserId();
                    clienteContactoGuardar.fechaCreacion = DateTime.Now;
                    db.ClienteContacto.Add(clienteContactoGuardar);
                    db.SaveChanges();
                  
                }
            }

            //ViewBag.usuarios = GetUsuarios();
            //ViewBag.contactos = GetContactos();

        
        }


        //// GET: ClienteContacto/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ClienteContacto clienteContacto = await db.ClienteContacto.FindAsync(id);
        //    if (clienteContacto == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.idCliente = new SelectList(db.Cliente, "idCliente", "nombreEmpresa", clienteContacto.idCliente);
        //    ViewBag.idContacto = new SelectList(db.Contacto, "idContacto", "nombres", clienteContacto.idContacto);
        //    return View(clienteContacto);
        //}

        //// POST: ClienteContacto/Edit/5
        //// Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        //// más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "idContacto,idCliente,vigencia,fechaCreacion,usuarioCreador")] ClienteContacto clienteContacto)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(clienteContacto).State = EntityState.Modified;
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.idCliente = new SelectList(db.Cliente, "idCliente", "nombreEmpresa", clienteContacto.idCliente);
        //    ViewBag.idContacto = new SelectList(db.Contacto, "idContacto", "nombres", clienteContacto.idContacto);
        //    return View(clienteContacto);
        //}

        //// GET: ClienteContacto/Delete/5
        //public ActionResult Delete(int? idCliente, int? idContacto)
        //{
        //    if ((idCliente == null) || (idContacto == null))
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ClienteContacto delClienteContacto = new ClienteContacto();
        //    delClienteContacto.idCliente = Int32.Parse(idCliente.ToString());
        //    delClienteContacto.idContacto = Int32.Parse(idContacto.ToString());

        //    ClienteContacto clienteContacto = db.ClienteContacto.Where(cc => cc.idCliente == idCliente && cc.idContacto == idContacto).FirstOrDefault();

        //    Contacto cont = db.Contacto.Find(idContacto);
        //    db.Contacto.Remove(cont);
        //    db.ClienteContacto.Remove(clienteContacto);
        //    db.SaveChanges();
        //    ViewModelClienteContacto vmcc = new ViewModelClienteContacto();
        //    vmcc.cliente = db.Cliente.Find(idCliente);
        //    vmcc.clienteContactos =// db.ClienteContacto.Where(s => s.idCliente == idCliente).ToList();

        //     db.ClienteContacto.Where(s => s.idCliente == idCliente)
        //        .Join(db.Contacto,
        //        cc => cc.idContacto,
        //        c => c.idContacto,
        //        (cc, c) => new clienteContactos_result()
        //        {
        //            _ClienteContacto = cc,
        //            _Contacto = c
        //        }).ToList();
        //    return View("Create", vmcc);
        //}

        // POST: ClienteContacto/Delete/5
        [CustomAuthorize(new string[] { "/Cliente/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? idCliente, int? idContacto)
        {
            if ((idCliente == null) || (idContacto == null))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ClienteContacto clienteContacto = db.ClienteContacto.Where(cc => cc.idCliente == idCliente && cc.idContacto == idContacto).FirstOrDefault();

            Contacto contacto = db.Contacto.Find(idContacto);
            var usuario = contacto.usuario;
            contacto.usuario = null;
            db.Entry(contacto).State = EntityState.Modified;
            db.ClienteContacto.Remove(clienteContacto);
            db.SaveChanges();
            return RedirectToAction("Create", new { id = idCliente });
        }

        [CustomAuthorize(new string[] { "/ClienteContacto/" })]
        [EnableJsReport()]
        public ActionResult ParticipantesClienteExcel(int? id)
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
            var participantes = GetParticipantesCliente(id);
            int numero = 0;
            List<double> notas = new List<double>();
            foreach (var item in participantes)
            {
                //var asistencia = "-";
                //var cantBloques = item.comercializacion.bloques.Count();
                //var cantAsistencias = item.asistencia.Where(x => x.asistio == true).Count();
                //if (cantBloques > 0)
                //{
                //    if (cantAsistencias * 100 / cantBloques == 0)
                //        continue;
                //    asistencia = String.Format("{0:N0}", cantAsistencias * 100 / cantBloques) + "%";

                //}
                var nota = 0.0;
                var notaTeorica = 0.0;
                var contTeorica = 0;
                foreach (var evaluacion in item.comercializacion.evaluaciones)
                {
                    if (evaluacion.categoria == CategoriaEvaluacion.Teorico)
                    {
                        if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault() != null)
                        {
                            if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != ""
                                && item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != "-")
                            {
                                notaTeorica += double.Parse(item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota);
                            }
                        }
                        contTeorica++;
                    }
                }
                if (contTeorica > 0)
                {
                    notaTeorica = notaTeorica / contTeorica;
                }
                var notaPractica = 0.0;
                var contPractica = 0;
                foreach (var evaluacion in item.comercializacion.evaluaciones)
                {
                    if (evaluacion.categoria == CategoriaEvaluacion.Practico)
                    {
                        if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault() != null)
                        {
                            if (item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != ""
                                && item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != "-")
                            {
                                notaPractica += double.Parse(item.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota);
                            }
                        }
                        contPractica++;
                    }
                }
                if (contPractica > 0)
                {
                    notaPractica = notaPractica / contPractica;
                }
                if (item.comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Teorico).Count() > 0
                    && item.comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Practico).Count() > 0)
                {
                    nota = (notaTeorica + notaPractica) / 2;
                }
                else
                {
                    if (item.comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Teorico).Count() > 0)
                    {
                        nota = notaTeorica;
                    }
                    else
                    {
                        if (item.comercializacion.evaluaciones.Where(x => x.categoria == SGC.Models.CategoriaEvaluacion.Practico).Count() > 0)
                        {
                            nota = notaPractica;
                        }
                    }
                }
                numero++;
                //var participante = new
                //{
                //    numero,
                //    item.contacto.nombres,
                //    item.contacto.apellidoPaterno,
                //    item.contacto.apellidoMaterno,
                //    item.contacto.run,
                //    empresa = item.comercializacion.cotizacion.nombreEmpresa,
                //    item.contacto.correo,
                //    item.contacto.telefono,
                //    asistencia,
                //    notaPractica,
                //    notaTeorica,
                //    nota = String.Format("{0:N1}", nota == 0 ? 1 : nota)
                //};
                //participantes.Add(item);
                notas.Add(nota == 0 ? 1 : nota);
            }




            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"Participantes.xlsx\"");
            ViewBag.notas = notas;
            return View(participantes);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private List<Participante> GetParticipantesCliente(int? idCliente)
        {
            List<Participante> participantes = new List<Participante>();
            var comercializaciones = db.Comercializacion
             .Where(x => x.softDelete == false)
             .Where(x => x.cotizacion.cliente.idCliente == idCliente)
              .Join(
                    db.ComercializacionEstadoComercializacion
                        .GroupBy(x => x.comercializacion.idComercializacion)
                        .Select(x => x.OrderByDescending(y => y.fechaCreacion).FirstOrDefault()),
                    comercializacion => comercializacion.idComercializacion,
                    estado => estado.comercializacion.idComercializacion,
                    (comercializacion, estado) => new ViewModelComercializacionEstado()
                    {
                        comercializacion = comercializacion,
                        estado = estado
                    }
                )
                .Where(x => x.estado.EstadoComercializacion != EstadoComercializacion.Borrador
                    && x.estado.EstadoComercializacion != EstadoComercializacion.Cancelada)
                .ToList();
            comercializaciones.ForEach(x => participantes.AddRange(x.comercializacion.participantes));
            return participantes;
        }

        private SelectList GetSucursales()
        {
            return new SelectList(db.Sucursal.Select(s => new SelectListItem
            {
                Text = s.nombre,
                Value = s.idSucursal.ToString()
            }).ToList(), "Value", "Text");
        }

        public List<Contacto> GetContactosCliente(Cliente cliente)
        {
            var contactos = new List<Contacto>();
            foreach (var contacto in cliente.encargadoPagos)
            {
                contacto.contacto.usuario = db.AspNetUsers.Where(x => x.Email.ToLower().Equals(contacto.contacto.correo.ToLower())).FirstOrDefault();
                contactos.Add(contacto.contacto);
            }
            foreach (var contacto in cliente.representanteLegals)
            {
                contacto.contacto.usuario = db.AspNetUsers.Where(x => x.Email.ToLower().Equals(contacto.contacto.correo.ToLower())).FirstOrDefault();
                contactos.Add(contacto.contacto);
            }
            foreach (var contacto in cliente.clienteContactoCotizacion)
            {
                contacto.contacto.usuario = db.AspNetUsers.Where(x => x.Email.ToLower().Equals(contacto.contacto.correo.ToLower())).FirstOrDefault();
                contactos.Add(contacto.contacto);
            }
            var contactosEliminar = new List<Contacto>();
            foreach (var contacto in contactos)
            {
                var clienteContacto = cliente.clienteContactos.Where(cc => cc.contacto.idContacto == contacto.idContacto).FirstOrDefault();
                if (clienteContacto != null)
                {
                    contactosEliminar.Add(clienteContacto.contacto);
                }
            }
            foreach (var contacto in contactosEliminar)
            {
                contactos.Remove(contacto);
            }
            return contactos;
        }

        public SelectList GetContactos()
        {
            return new SelectList(Utils.Utils.GetContactosDesocupados(db)
            .Select(con => new SelectListItem
            {
                Text = "[" + con.run + "]" + " " + con.nombres + " " + con.apellidoPaterno + " " + con.apellidoMaterno,
                Value = con.idContacto.ToString()
            }).ToList(), "Value", "Text");
        }

        public SelectList GetUsuarios()
        {
            //var usuarios = db.AspNetUsers.ToList();
            //var contactos = db.Contacto.Where(c => c.softDelete == false).ToList();

            string q = "SELECT anu.* FROM [DB_SGC].[dbo].[Contacto] c right join [DB_SGC].[dbo].[AspNetUsers] anu on c.usuario_Id = anu.Id where isnull(c.idContacto,1) = 1";
            return new SelectList(db.AspNetUsers.SqlQuery(q)
                .Select(c => new SelectListItem
                {
                    Text = c.UserName,
                    Value = c.Id
                }).ToList(), "Value", "Text");
        }
    }
}
