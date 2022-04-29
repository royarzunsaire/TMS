using jsreport.MVC;
using jsreport.Types;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SGC.CustomAuthorize;
using SGC.Models;
using SGC.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SGC.Controllers
{
    public class RelatorController : Controller
    {
        private static readonly string directory = ConfigurationManager.AppSettings["directory"] + "Files/";
        private InsecapContext db = new InsecapContext();

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

        // GET: Relators
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/" })]
        public ActionResult Index()
        {
            var relators = db.Relators;
            return View(relators.Where(r => r.softDelete == false).ToList());
        }

        // GET: Relators/Details/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/" })]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // obtener el relator del a base de datos
            Relator relator = db.Relators
                .Where(r => r.idRelator == id)
                .FirstOrDefault();
            if (relator == null)
            {
                return HttpNotFound();
            }
            Files.borrarArchivosLocales();
            await Files.BajarArchivoADirectorioLocalAsync(relator.imagenFirma);
            return View(relator);
        }

        private string CrearUsuarioRelator(Contacto contacto)
        {
            var configuracionUsuarioRelator = db.ConfiguracionUsuarioRelator.FirstOrDefault();

            var user = new ApplicationUser
            {
                UserName = contacto.correo,
                Email = contacto.correo,
                EmailConfirmed = true
            };

            // Then create:
            var adminresult = UserManager.Create(user, configuracionUsuarioRelator.contrasenia);

            string[] roles = { configuracionUsuarioRelator.rol.Name };

            //Add User to the selected Roles 
            if (adminresult.Succeeded)
            {
                var result = UserManager.AddToRoles(user.Id, roles);
            }
            else
            {
                ModelState.AddModelError("", adminresult.Errors.First().Replace("nombre", "correo electrónico"));
            }
            return user.Id;
        }

        private bool ExisteConfiguracionRelator()
        {
            if (db.ConfiguracionUsuarioRelator.FirstOrDefault() == null)
            {
                ModelState.AddModelError("", "No se encontró la configuración del relator.");
            }
            return db.ConfiguracionUsuarioRelator.FirstOrDefault() != null ? true : false;
        }

        //// GET: Relators/Create
        //public ActionResult Create(int? id)
        //{
        //    return View();
        //}

        //// POST: Relators/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "contacto,vinculadoSENCE,idImageFirma,idImageDocumentoAutorizacion,idImageCedula")] Relator relator)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // validar fecha nacimiento no futura
        //        if (DateTime.Compare(relator.contacto.fechaNacimiento, DateTime.Now) > 0)
        //        {
        //            ModelState.AddModelError("contacto.fechaNacimiento", "El campo Fecha de Nacimiento no es válido");
        //            return View(relator);
        //        }
        //        // datos contacto
        //        AspNetUsers usuario = db.AspNetUsers.Find(User.Identity.GetUserId());
        //        relator.contacto.vigente = true;
        //        relator.contacto.usuario = usuario;
        //        relator.contacto.usuarioCreador = User.Identity.Name;
        //        relator.contacto.fechaCreacion = DateTime.Now;
        //        // datos relator
        //        relator.usuarioCreador = User.Identity.Name;
        //        relator.fechaCreacion = DateTime.Now;
        //        relator.softDelete = false;
        //        //db.Contacto.Add(relator.contacto);
        //        db.Relators.Add(relator);
        //        // TODO: Guardar archivos
        //        db.SaveChanges();
        //        return RedirectToAction("Edit", new { id = relator.idRelator });
        //    }
        //    return View(relator);
        //}

        // GET: Relators/CreateSin
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/", "/Relator/Create/" })]
        public ActionResult Create(int? id)
        {
            //ViewBag.usuarios = GetUsuariosSinContacto();
            return View();
        }

        // POST: Relators/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/", "/Relator/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "contacto,vinculadoSENCE,idImageFirma,idImageDocumentoAutorizacion,idImageCedula")] Relator relator)
        {
            //ViewBag.usuarios = GetUsuariosSinContacto();
            //// validar usuario
            //if (Request["usuario"] == "")
            //{
            //    ModelState.AddModelError("usuario", "El campo Usuario es obligatorio");
            //}
            // validar fecha nacimiento no futura
            if (relator.contacto.fechaNacimiento != null)
            {
                if (DateTime.Compare(relator.contacto.fechaNacimiento.Value, DateTime.Now) > 0)
                {
                    ModelState.AddModelError("contacto.fechaNacimiento", "El campo Fecha de Nacimiento no es válido");
                    return View(relator);
                }
            }
            // guardar archivos
            HttpPostedFileBase fileFirma = Request.Files["fileImagenFirma"];
            HttpPostedFileBase fileCedula = Request.Files["fileImagenCedula"];
            HttpPostedFileBase fileDocumentoAutorizacion = Request.Files["fileDocumentoAutorizacion"];
            // validar extenciones y tamaño maximo de los archivos
            if (fileFirma.ContentLength > 0)
            {
                var archivoValido = Files.ArchivoValido(fileFirma, new[] { ".png" }, 20);
                if (archivoValido != "")
                {
                    ModelState.AddModelError("imagenFirma", archivoValido);
                }
            }
            if (fileCedula.ContentLength > 0)
            {
                var archivoValido = Files.ArchivoValido(fileCedula, new[] { ".pdf" }, 3 * 1024);
                if (archivoValido != "")
                {
                    ModelState.AddModelError("imagenCedula", archivoValido);
                }
            }
            if (fileDocumentoAutorizacion.ContentLength > 0)
            {
                var archivoValido = Files.ArchivoValido(fileDocumentoAutorizacion, new[] { ".pdf" }, 3 * 1024);
                if (archivoValido != "")
                {
                    ModelState.AddModelError("imagenDocumentoAutorizacion", archivoValido);
                }
            }
            // validar que exista la configuracion del relator
            ExisteConfiguracionRelator();
            if (ModelState.IsValid)
            {
                // guardar archivos
                if (fileFirma.ContentLength > 0)
                {
                    relator.imagenFirma = await Files.CrearArchivoAsync(fileFirma, "relatores/firma/");
                    if (relator.imagenFirma == null)
                    {
                        ModelState.AddModelError("imagenFirma", "No se pudo guardar el archivo seleccionado.");
                    }
                }
                if (fileCedula.ContentLength > 0)
                {
                    relator.imagenCedula = await Files.CrearArchivoAsync(fileCedula, "relatores/cedula/");
                    if (relator.imagenCedula == null)
                    {
                        ModelState.AddModelError("imagenCedula", "No se pudo guardar el archivo seleccionado.");
                    }
                }
                if (fileDocumentoAutorizacion.ContentLength > 0)
                {
                    relator.imagenDocumentoAutorizacion = await Files.CrearArchivoAsync(fileDocumentoAutorizacion, "relatores/documento-autorizacion/");
                    if (relator.imagenDocumentoAutorizacion == null)
                    {
                        ModelState.AddModelError("", "No se pudo guardar el archivo seleccionado.");
                    }
                }
            }
            if (ModelState.IsValid)
            {
                relator.contacto.usuario = db.AspNetUsers.Find(CrearUsuarioRelator(relator.contacto));
            }
            if (ModelState.IsValid)
            {
                // datos contacto
                //AspNetUsers usuario = db.AspNetUsers.Find(Request["usuario"]);
                //relator.contacto.usuario = usuario;
                relator.contacto.vigente = true;
                relator.contacto.usuarioCreador = User.Identity.Name;
                relator.contacto.fechaCreacion = DateTime.Now;
                relator.contacto.softDelete = false;
                relator.contacto.tipoContacto = TipoContacto.Relator;
                // datos relator
                relator.usuarioCreador = User.Identity.Name;
                relator.fechaCreacion = DateTime.Now;
                relator.softDelete = false;
                //db.Contacto.Add(relator.contacto);
                relator.contacto.usuario.tipo = TipoUsuario.parcial;
                db.Relators.Add(relator);
                db.SaveChanges();
                return RedirectToAction("Edit", new { id = relator.idRelator });
            }
            return View(relator);
        }

        // GET: Relators/Perfil
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/Perfil/" })]
        public async Task<ActionResult> Perfil()
        {
            string id = User.Identity.GetUserId();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // obtener el relator del a base de datos
            Relator relator = db.Relators
                .Where(r => r.contacto.usuario.Id == id)
                .FirstOrDefault();
            if (relator == null)
            {
                return RedirectToAction("Index", "Home");
                //return HttpNotFound();
            }
            Files.borrarArchivosLocales();
            await Files.BajarArchivoADirectorioLocalAsync(relator.imagenFirma);
            return View(relator);
        }

        // POST: Relators/Perfil/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/Perfil/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Perfil([Bind(Include = "idRelator,contacto,vinculadoSENCE,idImageFirma,idImageDocumentoAutorizacion,idImageCedula")] Relator relator)
        {
            Relator relatorOriginal = db.Relators
                .Where(r => r.idRelator == relator.idRelator)
                .FirstOrDefault();
            // validar fecha nacimiento no futura
            if (relator.contacto.fechaNacimiento != null)
            {
                if (DateTime.Compare(relator.contacto.fechaNacimiento.Value, DateTime.Now) > 0)
                {
                    ModelState.AddModelError("contacto.fechaNacimiento", "El campo Fecha de Nacimiento no es válido");
                }
            }
            // guardar archivos
            var imagenFirmaAntiguas = relator.imagenFirma;
            var imagenCedulaAntiguas = relator.imagenCedula;
            var imagenDocumentoAutorizacionAntiguas = relator.imagenDocumentoAutorizacion;
            HttpPostedFileBase fileFirma = Request.Files["fileImagenFirma"];
            HttpPostedFileBase fileCedula = Request.Files["fileImagenCedula"];
            HttpPostedFileBase fileDocumentoAutorizacion = Request.Files["fileDocumentoAutorizacion"];
            // validar extenciones y tamaño maximo de los archivos
            if (fileFirma.ContentLength > 0)
            {
                var archivoValido = Files.ArchivoValido(fileFirma, new[] { ".png" }, 20);
                if (archivoValido != "")
                {
                    ModelState.AddModelError("imagenFirma", archivoValido);
                }
            }
            if (fileCedula.ContentLength > 0)
            {
                var archivoValido = Files.ArchivoValido(fileCedula, new[] { ".pdf" }, 3 * 1024);
                if (archivoValido != "")
                {
                    ModelState.AddModelError("imagenCedula", archivoValido);
                }
            }
            if (fileDocumentoAutorizacion.ContentLength > 0)
            {
                var archivoValido = Files.ArchivoValido(fileDocumentoAutorizacion, new[] { ".pdf" }, 3 * 1024);
                if (archivoValido != "")
                {
                    ModelState.AddModelError("imagenDocumentoAutorizacion", archivoValido);
                }
            }
            if (ModelState.IsValid)
            {
                // guardar archivos
                if (fileFirma.ContentLength > 0)
                {
                    relatorOriginal.imagenFirma = await Files.RemplazarArchivoAsync(relatorOriginal.imagenFirma, fileFirma, "relatores/firma/");
                    if (relatorOriginal.imagenFirma == null)
                    {
                        ModelState.AddModelError("imagenFirma", "No se pudo guardar el archivo seleccionado.");
                    }
                    else
                    {
                        if (imagenFirmaAntiguas != null)
                        {
                            db.Storages.Remove(imagenFirmaAntiguas);
                        }
                    }
                }
                if (fileCedula.ContentLength > 0)
                {
                    relatorOriginal.imagenCedula = await Files.RemplazarArchivoAsync(relatorOriginal.imagenCedula, fileCedula, "relatores/cedula/");
                    if (relatorOriginal.imagenCedula == null)
                    {
                        ModelState.AddModelError("imagenCedula", "No se pudo guardar el archivo seleccionado.");
                    }
                    else
                    {
                        if (imagenCedulaAntiguas != null)
                        {
                            db.Storages.Remove(imagenCedulaAntiguas);
                        }
                    }
                }
                if (fileDocumentoAutorizacion.ContentLength > 0)
                {
                    relatorOriginal.imagenDocumentoAutorizacion = await Files.RemplazarArchivoAsync(relatorOriginal.imagenDocumentoAutorizacion, fileDocumentoAutorizacion, "relatores/documento-autorizacion/");
                    if (relatorOriginal.imagenDocumentoAutorizacion == null)
                    {
                        ModelState.AddModelError("", "No se pudo guardar el archivo seleccionado.");
                    }
                    else
                    {
                        if (imagenDocumentoAutorizacionAntiguas != null)
                        {
                            db.Storages.Remove(imagenDocumentoAutorizacionAntiguas);
                        }
                    }
                }
                // datos contacto
                relatorOriginal.contacto.nombres = relator.contacto.nombres;
                relatorOriginal.contacto.apellidoPaterno = relator.contacto.apellidoPaterno;
                relatorOriginal.contacto.apellidoMaterno = relator.contacto.apellidoMaterno;
                relatorOriginal.contacto.run = relator.contacto.run;
                relatorOriginal.contacto.fechaNacimiento = relator.contacto.fechaNacimiento;
                relatorOriginal.contacto.telefono = relator.contacto.telefono;
                relatorOriginal.contacto.correo = relator.contacto.correo;
                relatorOriginal.contacto.direccion = relator.contacto.direccion;
                relatorOriginal.contacto.estadoCivil = relator.contacto.estadoCivil;
                relatorOriginal.contacto.usuarioCreador = User.Identity.Name;
                relatorOriginal.contacto.fechaCreacion = DateTime.Now;
                // datos relator
                relatorOriginal.vinculadoSENCE = relator.vinculadoSENCE;
                relatorOriginal.usuarioCreador = User.Identity.Name;
                relatorOriginal.fechaCreacion = DateTime.Now;
                relatorOriginal.contacto.usuario.UserName = relator.contacto.correo;
                relatorOriginal.contacto.usuario.Email = relator.contacto.correo;
                db.Entry(relatorOriginal).State = EntityState.Modified;
                db.SaveChanges();
            }
            Files.borrarArchivosLocales();
            await Files.BajarArchivoADirectorioLocalAsync(relatorOriginal.imagenFirma);
            return View(relatorOriginal);
        }

        // GET: Relators/Edit/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/" })]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // obtener el relator del a base de datos
            Relator relator = db.Relators
                .Where(r => r.idRelator == id)
                .FirstOrDefault();
            if (relator == null)
            {
                return HttpNotFound();
            }
            Files.borrarArchivosLocales();
            await Files.BajarArchivoADirectorioLocalAsync(relator.imagenFirma);
            return View(relator);
        }

        // POST: Relators/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "idRelator,contacto,vinculadoSENCE,idImageFirma,idImageDocumentoAutorizacion,idImageCedula")] Relator relator)
        {
            Relator relatorOriginal = db.Relators
                .Where(r => r.idRelator == relator.idRelator)
                .FirstOrDefault();
            // validar fecha nacimiento no futura
            if (relator.contacto.fechaNacimiento != null)
            {
                if (DateTime.Compare(relator.contacto.fechaNacimiento.Value, DateTime.Now) > 0)
                {
                    relatorOriginal.contacto = relator.contacto;
                    ModelState.AddModelError("contacto.fechaNacimiento", "El campo Fecha de Nacimiento no es válido");
                    return View(relatorOriginal);
                }
            }
            // guardar archivos
            var imagenFirmaAntiguas = relator.imagenFirma;
            var imagenCedulaAntiguas = relator.imagenCedula;
            var imagenDocumentoAutorizacionAntiguas = relator.imagenDocumentoAutorizacion;
            HttpPostedFileBase fileFirma = Request.Files["fileImagenFirma"];
            HttpPostedFileBase fileCedula = Request.Files["fileImagenCedula"];
            HttpPostedFileBase fileDocumentoAutorizacion = Request.Files["fileDocumentoAutorizacion"];
            // validar extenciones y tamaño maximo de los archivos
            if (fileFirma.ContentLength > 0)
            {
                var archivoValido = Files.ArchivoValido(fileFirma, new[] { ".png" }, 20);
                if (archivoValido != "")
                {
                    ModelState.AddModelError("imagenFirma", archivoValido);
                }
            }
            if (fileCedula.ContentLength > 0)
            {
                var archivoValido = Files.ArchivoValido(fileCedula, new[] { ".pdf" }, 3 * 1024);
                if (archivoValido != "")
                {
                    ModelState.AddModelError("imagenCedula", archivoValido);
                }
            }
            if (fileDocumentoAutorizacion.ContentLength > 0)
            {
                var archivoValido = Files.ArchivoValido(fileDocumentoAutorizacion, new[] { ".pdf" }, 3 * 1024);
                if (archivoValido != "")
                {
                    ModelState.AddModelError("imagenDocumentoAutorizacion", archivoValido);
                }
            }
            if (ModelState.IsValid)
            {
                // guardar archivos
                if (fileFirma.ContentLength > 0)
                {
                    relatorOriginal.imagenFirma = await Files.RemplazarArchivoAsync(relatorOriginal.imagenFirma, fileFirma, "relatores/firma/");
                    if (relatorOriginal.imagenFirma == null)
                    {
                        ModelState.AddModelError("imagenFirma", "No se pudo guardar el archivo seleccionado.");
                    }
                    else
                    {
                        if (imagenFirmaAntiguas != null)
                        {
                            db.Storages.Remove(imagenFirmaAntiguas);
                        }
                    }
                }
                if (fileCedula.ContentLength > 0)
                {
                    relatorOriginal.imagenCedula = await Files.RemplazarArchivoAsync(relatorOriginal.imagenCedula, fileCedula, "relatores/cedula/");
                    if (relatorOriginal.imagenCedula == null)
                    {
                        ModelState.AddModelError("imagenCedula", "No se pudo guardar el archivo seleccionado.");
                    }
                    else
                    {
                        if (imagenCedulaAntiguas != null)
                        {
                            db.Storages.Remove(imagenCedulaAntiguas);
                        }
                    }
                }
                if (fileDocumentoAutorizacion.ContentLength > 0)
                {
                    relatorOriginal.imagenDocumentoAutorizacion = await Files.RemplazarArchivoAsync(relatorOriginal.imagenDocumentoAutorizacion, fileDocumentoAutorizacion, "relatores/documento-autorizacion/");
                    if (relatorOriginal.imagenDocumentoAutorizacion == null)
                    {
                        ModelState.AddModelError("", "No se pudo guardar el archivo seleccionado.");
                    }
                    else
                    {
                        if (imagenDocumentoAutorizacionAntiguas != null)
                        {
                            db.Storages.Remove(imagenDocumentoAutorizacionAntiguas);
                        }
                    }
                }
            }
            if (ModelState.IsValid)
            {
                // datos contacto
                relatorOriginal.contacto.nombres = relator.contacto.nombres;
                relatorOriginal.contacto.apellidoPaterno = relator.contacto.apellidoPaterno;
                relatorOriginal.contacto.apellidoMaterno = relator.contacto.apellidoMaterno;
                relatorOriginal.contacto.run = relator.contacto.run;
                relatorOriginal.contacto.fechaNacimiento = relator.contacto.fechaNacimiento;
                relatorOriginal.contacto.telefono = relator.contacto.telefono;
                relatorOriginal.contacto.correo = relator.contacto.correo;
                relatorOriginal.contacto.direccion = relator.contacto.direccion;
                relatorOriginal.contacto.estadoCivil = relator.contacto.estadoCivil;
                //AspNetUsers usuario = db.AspNetUsers.Find(User.Identity.GetUserId());
                //relatorOriginal.contacto.usuario = relatorOriginal.contacto.usuario;
                relatorOriginal.contacto.usuarioCreador = User.Identity.Name;
                relatorOriginal.contacto.fechaCreacion = DateTime.Now;
                // datos relator
                relatorOriginal.vinculadoSENCE = relator.vinculadoSENCE;
                relatorOriginal.usuarioCreador = User.Identity.Name;
                relatorOriginal.fechaCreacion = DateTime.Now;
                relatorOriginal.contacto.usuario.UserName = relator.contacto.correo;
                relatorOriginal.contacto.usuario.Email = relator.contacto.correo;
                //db.Entry(relatorOriginal.contacto).State = EntityState.Modified;
                // TODO: Guardar archivos
                db.Entry(relatorOriginal).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            Files.borrarArchivosLocales();
            await Files.BajarArchivoADirectorioLocalAsync(relatorOriginal.imagenFirma);
            return View(relatorOriginal);
        }

        // GET: Relators/DatosBancarios/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/", "/Relator/Perfil/" })]
        public ActionResult DatosBancarios(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // obtener el relator del a base de datos
            Relator relator = db.Relators
                .Where(r => r.idRelator == id)
                .FirstOrDefault();
            if (relator == null)
            {
                return HttpNotFound();
            }
            ViewBag.urlAnterior = Request.UrlReferrer.ToString();
            return View(relator);
        }

        // POST: Relators/DatosBancarios
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/", "/Relator/Perfil/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DatosBancarios([Bind(Include = "datosBancarios,idRelator")] Relator relator, string urlAnterior)
        {
            Relator relatorOriginal = db.Relators
                .Where(r => r.idRelator == relator.idRelator)
                .FirstOrDefault();
            if (ModelState.IsValid)
            {
                // datos bancarios
                relatorOriginal.datosBancarios = relator.datosBancarios;
                relatorOriginal.datosBancarios.usuarioCreador = User.Identity.Name;
                relatorOriginal.datosBancarios.fechaCreacion = DateTime.Now;
                //db.DatosBancarios.Add(relatorOriginal.datosBancarios);
                db.Entry(relatorOriginal).State = EntityState.Modified;
                db.SaveChanges();
                if (urlAnterior.Contains("Edit"))
                {
                    return RedirectToAction("Edit", new { id = relatorOriginal.idRelator });
                }
                else
                {
                    return RedirectToAction("Perfil");
                }
            }
            ViewBag.urlAnterior = urlAnterior;
            return View(relatorOriginal);
        }

        // GET: Relators/DatosCurriculares/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/", "/Relator/Perfil/" })]
        public ActionResult DatosCurriculares(int? id, string urlAnterior)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // obtener el relator del a base de datos
            Relator relator = db.Relators
                .Where(r => r.idRelator == id)
                .FirstOrDefault();
            if (relator == null)
            {
                return HttpNotFound();
            }
            ViewModelDatoCurricular datoCurricularVM = new ViewModelDatoCurricular();
            datoCurricularVM.relator = relator;
            if (urlAnterior != null)
            {
                ViewBag.urlAnterior = urlAnterior;
            }
            else
            {
                ViewBag.urlAnterior = Request.UrlReferrer.ToString();
            }
            return View(datoCurricularVM);
        }

        // POST: Relators/DatosCurriculares
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/", "/Relator/Perfil/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DatosCurriculares([Bind(Include = "idRelator")] Relator relator, string urlAnterior)
        {
            ViewModelDatoCurricular datoCurricularVM = new ViewModelDatoCurricular();
            Relator relatorOriginal = db.Relators
                .Where(r => r.idRelator == relator.idRelator)
                .FirstOrDefault();
            // datos titulo curricular
            TituloCurricular tituloCurricular = new TituloCurricular();
            tituloCurricular.nombreTitulo = (TipoTitulo)int.Parse(Request["tituloCurricular.nombreTitulo"]);
            tituloCurricular.fecha = Request["tituloCurricular.fecha"];
            tituloCurricular.institucion = Request["tituloCurricular.institucion"];
            tituloCurricular.descripcion = Request["tituloCurricular.descripcion"];
            // validar año no futuro
            if (Request["tituloCurricular.fecha"] != "")
            {
                if (int.Parse(Request["tituloCurricular.fecha"]) < 1900 || int.Parse(Request["tituloCurricular.fecha"]) > int.Parse(DateTime.Now.ToString("yyyy")))
                {
                    ModelState.AddModelError("datosCurriculares", "No se pudo ingresar el dato curricular");
                    ModelState.AddModelError("tituloCurricular.fecha", "El campo Año no es válido");
                }
            }
            // guardar archivo
            HttpPostedFileBase file = Request.Files["fileTituloCurricular"];
            // verificar que se selecciono un archivo
            if (file.ContentLength <= 0)
            {
                ModelState.AddModelError("datosCurriculares", "No se pudo ingresar el dato curricular");
                ModelState.AddModelError("documento", "Se debe seleccionar un archivo.");
            }
            else
            {
                // validar extenciones y tamaño maximo de los archivos
                var archivoValido = Files.ArchivoValido(file, new[] { ".pdf" }, 3 * 1024);
                if (archivoValido != "")
                {
                    ModelState.AddModelError("documento", archivoValido);
                }
            }
            if (ModelState.IsValid)
            {
                tituloCurricular.usuarioCreador = User.Identity.Name;
                tituloCurricular.fechaCreacion = DateTime.Now;
                // validar tituloCurricular
                var context = new ValidationContext(tituloCurricular, serviceProvider: null, items: null);
                var results = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(tituloCurricular, context, results, true);
                if (isValid)
                {
                    tituloCurricular.documento = await Files.CrearArchivoAsync(file, "relatores/titulo-curricular/");
                    if (tituloCurricular.documento == null)
                    {
                        ModelState.AddModelError("datosCurriculares", "No se pudo guardar el archivo seleccionado.");
                    }
                    // datos relator
                    relatorOriginal.tituloCurricular.Add(tituloCurricular);
                    db.Entry(relatorOriginal).State = EntityState.Modified;
                    db.SaveChanges();
                    datoCurricularVM.relator = relatorOriginal;
                    ViewBag.urlAnterior = urlAnterior;
                    return View(datoCurricularVM);
                }
                else
                {
                    ModelState.AddModelError("datosCurriculares", "No se pudo ingresar el dato curricular");
                }
                // agregar los mensajes de error del titulo curricular al modelState
                foreach (var result in results)
                {
                    if (result.MemberNames.Contains("descripcion"))
                    {
                        ModelState.AddModelError("tituloCurricular.descripcion", result.ErrorMessage);
                    }
                    if (result.MemberNames.Contains("institucion"))
                    {
                        ModelState.AddModelError("tituloCurricular.institucion", result.ErrorMessage);
                    }
                    if (result.MemberNames.Contains("fecha"))
                    {
                        ModelState.AddModelError("tituloCurricular.fecha", result.ErrorMessage);
                    }
                }
            }
            datoCurricularVM.relator = relatorOriginal;
            datoCurricularVM.tituloCurricular = tituloCurricular;
            ViewBag.urlAnterior = urlAnterior;
            return View(datoCurricularVM);
        }

        // POST: Relators/BorrarDatoCurricular
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/", "/Relator/Perfil/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BorrarDatoCurricular([Bind(Include = "idRelator")] Relator relator, string urlAnterior)
        {
            Relator relatorOriginal = db.Relators
                .Where(r => r.idRelator == relator.idRelator)
                .FirstOrDefault();
            TituloCurricular tituloCurricular = db.TituloCurriculars.Find(int.Parse(Request["item.idTituloCurricular"]));
            Files.BorrarArchivo(tituloCurricular.documento);
            db.Storages.Remove(tituloCurricular.documento);
            db.TituloCurriculars.Remove(tituloCurricular);
            db.SaveChanges();
            return RedirectToAction("DatosCurriculares", new { id = relatorOriginal.idRelator, urlAnterior });
        }

        // GET: Relators/ExperienciaLaboral/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/", "/Relator/Perfil/" })]
        public ActionResult ExperienciaLaboral(int? id, string urlAnterior)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // obtener el relator del a base de datos
            Relator relator = db.Relators
                .Where(r => r.idRelator == id)
                .FirstOrDefault();
            if (relator == null)
            {
                return HttpNotFound();
            }
            ViewModelExperienciaLaboral experienciaLaboralVM = new ViewModelExperienciaLaboral();
            experienciaLaboralVM.relator = relator;
            if (urlAnterior != null)
            {
                ViewBag.urlAnterior = urlAnterior;
            }
            else
            {
                ViewBag.urlAnterior = Request.UrlReferrer.ToString();
            }
            return View(experienciaLaboralVM);
        }

        // POST: Relators/ExperienciaLaboral
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/", "/Relator/Perfil/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExperienciaLaboral([Bind(Include = "idRelator")] Relator relator, string urlAnterior)
        {
            ViewModelExperienciaLaboral experienciaLaboralVM = new ViewModelExperienciaLaboral();
            Relator relatorOriginal = db.Relators
                .Where(r => r.idRelator == relator.idRelator)
                .FirstOrDefault();
            // datos experiencia laboral
            ExperienciaLaboral experienciaLaboral = new ExperienciaLaboral();
            experienciaLaboral.cargo = Request["experienciaLaboral.cargo"];
            experienciaLaboral.empresa = Request["experienciaLaboral.empresa"];
            experienciaLaboral.faena = Request["experienciaLaboral.faena"];
            if (Request["experienciaLaboral.fechaInicio"] != "")
            {
                experienciaLaboral.fechaInicio = Convert.ToDateTime(Request["experienciaLaboral.fechaInicio"]);
            }
            if (Request["experienciaLaboral.fechaTermino"] != "")
            {
                experienciaLaboral.fechaTermino = Convert.ToDateTime(Request["experienciaLaboral.fechaTermino"]);
            }
            if (Request["experienciaLaboral.fechaInicio"] != "" && Request["experienciaLaboral.fechaTermino"] != "")
            {
                // validar fecha termino no futura
                if (DateTime.Compare((DateTime)experienciaLaboral.fechaTermino, DateTime.Now) > 0)
                {
                    ModelState.AddModelError("experienciaLaboral", "No se pudo ingresar la experiencia laboral");
                    ModelState.AddModelError("experienciaLaboral.fechaTermino", "El campo Fecha de Término no es válido");
                }
                // validar fecha inicio menor a fecha termino no futura
                if (DateTime.Compare((DateTime)experienciaLaboral.fechaInicio, (DateTime)experienciaLaboral.fechaTermino) > 0)
                {
                    ModelState.AddModelError("experienciaLaboral", "No se pudo ingresar la experiencia laboral");
                    ModelState.AddModelError("experienciaLaboral.fechaInicio", "El campo Fecha de Inicio no es válido");
                }
            }
            // guardar archivo
            HttpPostedFileBase file = Request.Files["fileExperienciaLaboral"];
            // verificar que se selecciono un archivo
            if (file.ContentLength <= 0)
            {
                ModelState.AddModelError("experienciaLaboral", "No se pudo ingresar la experiencia laboral");
                ModelState.AddModelError("documento", "Se debe seleccionar un archivo.");
            }
            else
            {
                // validar extenciones y tamaño maximo de los archivos
                if (file.ContentLength > 0)
                {
                    var archivoValido = Files.ArchivoValido(file, new[] { ".pdf" }, 3 * 1024);
                    if (archivoValido != "")
                    {
                        ModelState.AddModelError("documento", archivoValido);
                    }
                }
            }
            if (ModelState.IsValid)
            {
                experienciaLaboral.usuarioCreador = User.Identity.Name;
                experienciaLaboral.fechaCreacion = DateTime.Now;
                // validar tituloCurricular
                var context = new ValidationContext(experienciaLaboral, serviceProvider: null, items: null);
                var results = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(experienciaLaboral, context, results, true);
                if (isValid)
                {
                    experienciaLaboral.documento = await Files.CrearArchivoAsync(file, "relatores/experiencia-laboral/");
                    if (experienciaLaboral.documento == null)
                    {
                        ModelState.AddModelError("experienciaLaboral", "No se pudo guardar el archivo seleccionado.");
                    }
                    // datos relator
                    relatorOriginal.experienciaLaboral.Add(experienciaLaboral);
                    db.Entry(relatorOriginal).State = EntityState.Modified;
                    db.SaveChanges();
                    experienciaLaboralVM.relator = relatorOriginal;
                    ViewBag.urlAnterior = urlAnterior;
                    return View(experienciaLaboralVM);
                }
                // agregar los mensajes de error del titulo curricular al modelState
                foreach (var result in results)
                {
                    if (result.MemberNames.Contains("fechaInicio"))
                    {
                        ModelState.AddModelError("experienciaLaboral.fechaInicio", result.ErrorMessage);
                    }
                    if (result.MemberNames.Contains("fechaTermino"))
                    {
                        ModelState.AddModelError("experienciaLaboral.fechaTermino", result.ErrorMessage);
                    }
                    if (result.MemberNames.Contains("cargo"))
                    {
                        ModelState.AddModelError("experienciaLaboral.cargo", result.ErrorMessage);
                    }
                    if (result.MemberNames.Contains("empresa"))
                    {
                        ModelState.AddModelError("experienciaLaboral.empresa", result.ErrorMessage);
                    }
                    if (result.MemberNames.Contains("faena"))
                    {
                        ModelState.AddModelError("experienciaLaboral.faena", result.ErrorMessage);
                    }
                    ModelState.AddModelError("experienciaLaboral", "No se pudo ingresar la experiencia laboral");
                }
            }
            experienciaLaboralVM.relator = relatorOriginal;
            experienciaLaboralVM.experienciaLaboral = experienciaLaboral;
            ViewBag.urlAnterior = urlAnterior;
            return View(experienciaLaboralVM);
        }

        // POST: Relators/BorrarExperienciaLaboral
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/", "/Relator/Perfil/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BorrarExperienciaLaboral([Bind(Include = "idRelator")] Relator relator, string urlAnterior)
        {
            Relator relatorOriginal = db.Relators
                .Where(r => r.idRelator == relator.idRelator)
                .FirstOrDefault();
            ExperienciaLaboral experienciaLaboral = db.ExperienciaLaborals.Find(int.Parse(Request["item.idExperienciaLaboral"]));
            Files.BorrarArchivo(experienciaLaboral.documento);
            db.Storages.Remove(experienciaLaboral.documento);
            db.ExperienciaLaborals.Remove(experienciaLaboral);
            db.SaveChanges();
            return RedirectToAction("ExperienciaLaboral", new { id = relatorOriginal.idRelator, urlAnterior });
        }

        // GET: Relators/CursosHablitados/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/", "/Relator/Perfil/" })]
        public ActionResult CursosHablitados(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // obtener el relator del la base de datos
            Relator relator = db.Relators
                .Where(r => r.idRelator == id)
                .FirstOrDefault();
            if (relator == null)
            {
                return HttpNotFound();
            }
            return View(relator);
        }

        // GET: Relators/CursosRealizar/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/Perfil/" })]
        public ActionResult CursosRealizar(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // obtener el relator del la base de datos
            Relator relator = db.Relators
                .Where(r => r.idRelator == id)
                .FirstOrDefault();
            if (relator == null)
            {
                return HttpNotFound();
            }
            List<RelatorCurso> relatorCurso = db.RelatorCurso.Where(rc => rc.idRelator == relator.idRelator).ToList();
            var comercializaciones = new List<Comercializacion>();
            List<Bloque> bloques = new List<Bloque>();
            foreach (var item in relatorCurso)
            {
                foreach (var itemC in item.comercializaciones
                    .Where(x => x.softDelete == false)
                    //.Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.En_Proceso)
                    .ToList())
                {
                    comercializaciones.Add(itemC);
                    //bloques.AddRange(itemC.bloques);
                }
            }
            comercializaciones.OrderByDescending(x => x.fechaInicio);
            ViewBag.idRelator = relator.idRelator;
            return View(comercializaciones);
        }

        // GET: Relators/Delete/5
        //[CustomAuthorize(new string[] { "/Relator/" })]
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Relator relator = db.Relators
        //        .Where(r => r.idRelator == id)
        //        .FirstOrDefault();
        //    if (relator == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(relator);
        //}

        // POST: Relators/Delete/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Relator relator = db.Relators
                .Where(r => r.idRelator == id)
                .FirstOrDefault();
            //relator.contacto.vigente = false;
            //db.Entry(relator.contacto).State = EntityState.Modified;
            //db.DatosBancarios.Remove(relator.datosBancarios);
            //int cantTitulosCurriculares = relator.tituloCurricular.Count();
            //for (int i = 0; i < cantTitulosCurriculares; i++)
            //{
            //    db.TituloCurriculars.Remove(relator.tituloCurricular.ElementAt<TituloCurricular>(0));
            //}
            //int cantExperienciasLaborales = relator.experienciaLaboral.Count();
            //for (int i = 0; i < cantExperienciasLaborales; i++)
            //{
            //    db.ExperienciaLaborals.Remove(relator.experienciaLaboral.ElementAt<ExperienciaLaboral>(0));
            //}
            //db.Relators.Remove(relator);
            //db.Contacto.Remove(relator.contacto); // TODO: softDelete contacto
            relator.contacto.softDelete = true;
            relator.softDelete = true;
            var usuario = relator.contacto.usuario;
            relator.contacto.usuario = null;
            if (relator.imagenFirma != null)
            {
                Files.BorrarArchivo(relator.imagenFirma);
                db.Storages.Remove(relator.imagenFirma);
            }
            if (relator.imagenCedula != null)
            {
                Files.BorrarArchivo(relator.imagenCedula);
                db.Storages.Remove(relator.imagenCedula);
            }
            if (relator.imagenDocumentoAutorizacion != null)
            {
                Files.BorrarArchivo(relator.imagenDocumentoAutorizacion);
                db.Storages.Remove(relator.imagenDocumentoAutorizacion);
            }
            db.Entry(relator).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Relators/Descargar/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/", "/Relator/Perfil/" })]
        public async Task<ActionResult> Descargar(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var storage = db.Storages.Find(id);
            if (storage == null)
            {
                return HttpNotFound();
            }
            return await Files.BajarArchivoDescargarAsync(storage);
        }

        public SelectList GetUsuariosSinContacto()
        {
            string q = "select u.* from DB_SGC.dbo.AspNetUsers u left join DB_SGC.dbo.Contacto c on c.usuario_Id = u.Id left join DB_SGC.dbo.Relators r on r.contacto_idContacto = c.idContacto where ISNULL(c.idContacto,'')=''";
            return new SelectList(db.AspNetUsers.SqlQuery(q).Select(u => new SelectListItem
            {
                Text = u.UserName,
                Value = u.Id.ToString()
            }).ToList(), "Value", "Text");
        }

        // GET: Relators/R16/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/" })]
        public ActionResult R16(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // obtener el relator del a base de datos
            Relator relator = db.Relators.Find(id);
            if (relator == null)
            {
                return HttpNotFound();
            }
            return View(relator);
        }

        // GET: Relator/LlenarR16/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/" })]
        public async Task<ActionResult> LlenarR16(int? id, int? id2)
        {
            if (id == null || id2 == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var relator = db.Relators.Find(id2);
            if (relator == null)
            {
                return HttpNotFound();
            }
            var r16 = db.R16.Find(id);
            if (r16 == null)
            {
                r16 = new R16();
                r16.encuesta = new Encuesta();
                r16.encuesta.seccionEncuesta = new List<SeccionEncuesta>();
                r16.relator = relator;
                var seccionEncuesta = new SeccionEncuesta();
                seccionEncuesta.formulario = db.Formulario.Where(r => r.tipoFormulario == TipoFormulario.R16).Where(x => x.softDelete == false).FirstOrDefault();
                seccionEncuesta.posicion = 0;
                r16.encuesta.seccionEncuesta.Add(seccionEncuesta);
                r16.encuesta.respuestas = new List<RespuestasContestadasFormulario>();
                if (seccionEncuesta.formulario == null)
                {
                    ModelState.AddModelError("", "No existe un formulario r16");
                    Files.borrarArchivosLocales();
                    await Files.BajarArchivoADirectorioLocalAsync(relator.imagenFirma);
                    return View("Details", relator);
                }
            }
            return View(r16);
        }

        // POST: Relator/LlenarR16
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LlenarR16(int idR16, int idRelator)
        {
            var r16 = db.R16.Find(idR16);
            if (r16 == null)
            {
                r16 = new R16();
                r16.encuesta = new Encuesta();
                r16.encuesta.seccionEncuesta = new List<SeccionEncuesta>();
                r16.relator = db.Relators.Find(idRelator);
                var seccionEncuesta = new SeccionEncuesta();
                seccionEncuesta.formulario = db.Formulario.Where(r => r.tipoFormulario == TipoFormulario.R16).Where(x => x.softDelete == false).FirstOrDefault();
                seccionEncuesta.posicion = 0;
                r16.encuesta.seccionEncuesta.Add(seccionEncuesta);
                r16.encuesta.respuestas = new List<RespuestasContestadasFormulario>();
            }
            foreach (var seccionEncuesta in r16.encuesta.seccionEncuesta)
            {
                foreach (var pregunta in seccionEncuesta.formulario.preguntasFormularios)
                {
                    if ((Request[pregunta.idPreguntasFormulario.ToString()] == null
                        || Request[pregunta.idPreguntasFormulario.ToString()] == "") && pregunta.obligatoria)
                    {
                        ModelState.AddModelError("", "Se deben responder todas las preguntas con *");
                        return View(r16);
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
                    respuesta.contacto = r16.relator.contacto;
                    respuesta.pregunta = pregunta;
                    r16.encuesta.respuestas.Add(respuesta);
                    db.RespuestasContestadasFormulario.Add(respuesta);
                    // eliminar respuesta si ya existe
                    var respuestaBD = db.RespuestasContestadasFormulario
                        .Where(r => r.pregunta.idPreguntasFormulario == pregunta.idPreguntasFormulario)
                        .Where(r => r.encuesta.idEncuesta == r16.encuesta.idEncuesta)
                        .FirstOrDefault();
                    if (respuestaBD != null)
                    {
                        db.RespuestasContestadasFormulario.Remove(respuestaBD);
                    }
                }
            }
            r16.fecha = DateTime.Now;
            if (idR16 == 0)
            {
                db.R16.Add(r16);
            }
            else
            {
                db.Entry(r16).State = EntityState.Modified;
            }
            db.SaveChanges();
            return RedirectToAction("r16", "Relator", new { id = r16.relator.idRelator });
        }

        public object DataR16(R16 r16)
        {
            var nuevoSi = "";
            var nuevoNo = "";
            if (r16.relator.r16.OrderBy(re => re.fecha).FirstOrDefault().idR16 == r16.idR16)
            {
                nuevoSi = "X";
            }
            else
            {
                nuevoNo = "X";
            }
            var r = new List<string>();
            foreach (var item in r16.encuesta.seccionEncuesta.FirstOrDefault().formulario.preguntasFormularios)
            {
                if (item.tipo == TipoPregunta.Alternativa)
                {
                    if (r16.encuesta.respuestas.Where(re => re.pregunta.idPreguntasFormulario == item.idPreguntasFormulario).FirstOrDefault().respuesta == "100")
                    {
                        r.Add("X");
                    }
                    else
                    {
                        r.Add("_");
                    }
                }
                else
                {
                    r.Add(r16.encuesta.respuestas.Where(re => re.pregunta.idPreguntasFormulario == item.idPreguntasFormulario).FirstOrDefault().respuesta);
                }
            }

            var data = new
            {
                fechaHoy = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                fecha = r16.fecha.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                r16.relator.contacto.nombreCompleto,
                r16.relator.contacto.run,
                nuevoSi,
                nuevoNo,
                r
            };
            return data;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/" })]
        public ActionResult DescargarR16(int? id)
        {
            var r16 = db.R16.Find(id);
            if (r16 == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r16")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r16\" y tipo \"word\".");
                return View("R16", r16.relator);
            }
            return RedirectToAction("GenerarReporteR16", new { id });
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/" })]
        [EnableJsReport()]
        public async Task<ActionResult> GenerarReporteR16(int? id)
        {
            var r16 = db.R16.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r16")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
            var report = HttpContext
                .JsReportFeature()
                .Recipe(Recipe.Docx)
                .Engine(Engine.Handlebars)
                .Configure((r) => r.Template.Docx = new Docx
                {
                    TemplateAsset = new Asset
                    {
                        Content = base64,
                        Encoding = "base64"
                    }
                })
                .Configure((r) => r.Data = DataR16(r16))
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"r16_" + r16.idR16 + "_" + r16.relator.contacto.run + ".docx\"");
            return null;
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/" })]
        public ActionResult GenerarPdfR16(int? id)
        {
            var r16 = db.R16.Find(id);
            if (r16 == null)
            {
                return HttpNotFound();
            }
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r16")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                // indicar q hubo un error
                ModelState.AddModelError("", "No se encontro el template para generar el reporte, debe existir un template con el nombre \"r16\" y tipo \"word\".");
                return View("R16", r16.relator);
            }

            string hash = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                hash = Utils.Utils.GetHash(sha256Hash, DateTime.Now.ToString());
            }

            string createRequest = Url.Action("GenerarReportePdfR16", "Relator", new { id, id2 = hash }, Request.Url.Scheme);
            // Generate Request
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(createRequest);
            req.Method = "GET";

            // Get the Response
            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException e)
            {
                return View("Error", (object)"No se pudo generar el documento.");
            }

            var path = directory + hash;
            Byte[] bytes = System.IO.File.ReadAllBytes(path + ".pdf");

            System.IO.File.Delete(path + ".pdf");

            Response.ContentType = "application/pdf";
            Response.AppendHeader("Content-Disposition", "attachment; filename=\"r16_" + r16.idR16 + "_" + r16.relator.contacto.run + ".pdf\"");

            return new FileContentResult(bytes, "application/pdf");
        }

        [EnableJsReport()]
        public async Task<ActionResult> GenerarReportePdfR16(int? id, string id2)
        {
            var r16 = db.R16.Find(id);
            // descargar template
            var template = db.Template
                .Where(t => t.nombre == "r16")
                .Where(t => t.tipo == TipoTemplate.word)
                .FirstOrDefault();
            if (template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var archivoTemplate = await Files.BajarArchivoBytesAsync(template.template);
            var base64 = System.Convert.ToBase64String(archivoTemplate, 0, archivoTemplate.Length);
            var report = HttpContext
                .JsReportFeature()
                .Recipe(Recipe.Docx)
                .Engine(Engine.Handlebars)
                .Configure((r) => r.Template.Docx = new Docx
                {
                    TemplateAsset = new Asset
                    {
                        Content = base64,
                        Encoding = "base64"
                    }
                })
                .Configure((r) => r.Data = DataR16(r16))
                .OnAfterRender((r) =>
                {
                    var path = directory + id2;
                    using (var file = System.IO.File.Open(path + ".docx", FileMode.Create))
                    {
                        r.Content.CopyTo(file);
                    }
                    var appWord = new Microsoft.Office.Interop.Word.Application();
                    var wordDocument = appWord.Documents.Open(path + ".docx");
                    wordDocument.ExportAsFixedFormat(path + ".pdf", Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF);
                    wordDocument.Close();
                    appWord.Quit();
                    System.IO.File.Delete(path + ".docx");
                });
            return null;
        }

        // POST: Relators/SubirR16/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubirR16(int idR16)
        {
            var r16 = db.R16.Find(idR16);
            // guardar archivos
            var archivo = r16.archivo;
            HttpPostedFileBase file = Request.Files["file"];
            // validar extenciones y tamaño maximo de los archivos
            if (file.ContentLength > 0)
            {
                var archivoValido = Files.ArchivoValido(file, new[] { ".pdf", ".png", ".jpg", ".jpeg" }, 3 * 1024);
                if (archivoValido == "")
                {
                    // guardar archivos
                    if (file.ContentLength > 0)
                    {
                        r16.archivo = await Files.RemplazarArchivoAsync(r16.archivo, file, "relatores/capacitacion-administrativa/");
                        if (r16.archivo == null)
                        {
                            ModelState.AddModelError("", "No se pudo guardar el archivo seleccionado.");
                        }
                        else
                        {
                            if (archivo != null)
                            {
                                db.Storages.Remove(archivo);
                            }
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", archivoValido);
                }
            }
            else
            {
                ModelState.AddModelError("", "Se debe seleccionar un archivo.");
            }
            db.Entry(r16).State = EntityState.Modified;
            db.SaveChanges();
            return View("R16", r16.relator);
        }

        // GET: Relators/DescargarArchivoR16/5
        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/" })]
        public async Task<ActionResult> DescargarArchivoR16(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var storage = db.Storages.Find(id);
            if (storage == null)
            {
                return HttpNotFound();
            }
            return await Files.BajarArchivoDescargarAsync(storage);
        }

        // GET: Relators/R53/5
        public ActionResult R53(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // obtener el cliente del a base de datos
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }
            return View(comercializacion);
        }

        // GET: Relator/LlenarR53/5
        public ActionResult LlenarR53(int? id, int? id2)
        {
            if (id == null || id2 == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var relator = db.Relators.Find(id2);
            var comercializacion = db.Comercializacion.Find(id);
            if (relator == null || comercializacion == null)
            {
                return HttpNotFound();
            }
            var r53 = db.R53
                .Where(x => x.relator.idRelator == relator.idRelator)
                .Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion)
                .FirstOrDefault();
            if (r53 == null)
            {
                r53 = new R53();
                r53.encuesta = new Encuesta();
                r53.encuesta.seccionEncuesta = new List<SeccionEncuesta>();
                r53.relator = relator;
                r53.comercializacion = comercializacion;
                var idUsuario = User.Identity.GetUserId();
                var seccionEncuesta = new SeccionEncuesta();
                seccionEncuesta.formulario = db.Formulario.Where(r => r.tipoFormulario == TipoFormulario.R53).Where(x => x.softDelete == false).FirstOrDefault();
                seccionEncuesta.posicion = 0;
                r53.encuesta.seccionEncuesta.Add(seccionEncuesta);
                r53.encuesta.respuestas = new List<RespuestasContestadasFormulario>();
                if (seccionEncuesta.formulario == null)
                {
                    return RedirectToAction("CursosRealizar", "Relator", new { id = relator.idRelator });
                }
            }
            return View(r53);
        }

        // POST: Relator/LlenarR53
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LlenarR53(int idR53, int idRelator, int idComercializacion)
        {
            var r53 = db.R53.Find(idR53);
            if (r53 == null)
            {
                r53 = new R53();
                r53.encuesta = new Encuesta();
                r53.encuesta.seccionEncuesta = new List<SeccionEncuesta>();
                r53.relator = db.Relators.Find(idRelator);
                r53.comercializacion = db.Comercializacion.Find(idComercializacion);
                var seccionEncuesta = new SeccionEncuesta();
                seccionEncuesta.formulario = db.Formulario.Where(r => r.tipoFormulario == TipoFormulario.R53).Where(x => x.softDelete == false).FirstOrDefault();
                seccionEncuesta.posicion = 0;
                r53.encuesta.seccionEncuesta.Add(seccionEncuesta);
                r53.encuesta.respuestas = new List<RespuestasContestadasFormulario>();
            }
            foreach (var seccionEncuesta in r53.encuesta.seccionEncuesta)
            {
                foreach (var pregunta in seccionEncuesta.formulario.preguntasFormularios)
                {
                    if ((Request[pregunta.idPreguntasFormulario.ToString()] == null
                        || Request[pregunta.idPreguntasFormulario.ToString()] == "") && pregunta.obligatoria)
                    {
                        ModelState.AddModelError("", "Se deben responder todas las preguntas con *");
                        return View(r53);
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
                    respuesta.contacto = r53.relator.contacto;
                    respuesta.pregunta = pregunta;
                    r53.encuesta.respuestas.Add(respuesta);
                    db.RespuestasContestadasFormulario.Add(respuesta);
                    // eliminar respuesta si ya existe
                    var respuestaBD = db.RespuestasContestadasFormulario
                        .Where(r => r.pregunta.idPreguntasFormulario == pregunta.idPreguntasFormulario)
                        .Where(r => r.encuesta.idEncuesta == r53.encuesta.idEncuesta)
                        .FirstOrDefault();
                    if (respuestaBD != null)
                    {
                        db.RespuestasContestadasFormulario.Remove(respuestaBD);
                    }
                }
            }
            r53.fecha = DateTime.Now;
            if (idR53 == 0)
            {
                db.R53.Add(r53);
            }
            else
            {
                db.Entry(r53).State = EntityState.Modified;
            }
            db.SaveChanges();
            AlertaR53Llenado(r53);
            return RedirectToAction("CursosRealizar", "Relator", new { id = idRelator });
        }

        private void AlertaR53Llenado(R53 r53)
        {
            // notificar curso con relator sin sence
            //var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta R53 Encuesta Aplicación Metodología").FirstOrDefault();
            //if (notificacionConfig != null)
            //{
            //    notificacionConfig.CrearNotificacion(db, r53.comercializacion.cotizacion.codigoCotizacion, r53.comercializacion.idComercializacion.ToString(), User.Identity.GetUserId());
            //}
        }

        // GET: Relator/VerR53/5
        public ActionResult VerR53(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var r53 = db.R53.Find(id);
            if (r53 == null)
            {
                return HttpNotFound();
            }
            return View(r53);
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
