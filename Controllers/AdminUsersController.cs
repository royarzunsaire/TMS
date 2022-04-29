using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SGC.CustomAuthorize;
using SGC.Models;
using SGC.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class AdminUsersController : Controller
    {
        private InsecapContext db = new InsecapContext();
        private ApplicationDbContext context = new ApplicationDbContext();
        public AdminUsersController() { }

        public AdminUsersController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
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

        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        [HttpGet]
        public ActionResult Index1()
        {

            //var a = UserManager.Users.ToListAsync();
            var usersWithRoles = (from user in context.Users
                                  select new
                                  {
                                      UserId = user.Id,
                                      Username = user.UserName,
                                      Email = user.Email,
                                      RoleNames = (from userRole in user.Roles
                                                   join role in context.Roles on userRole.RoleId
                                                   equals role.Id
                                                   select role.Name).ToList()
                                  }).ToList().Select(p => new Users_in_Role_ViewModel()
                                  {
                                      UserId = p.UserId,
                                      Username = p.Username,
                                      Email = p.Email,
                                      Role = string.Join(",", p.RoleNames)
                                  }).ToList();
            foreach (var item in usersWithRoles)
            {
                item.Tipo = db.AspNetUsers.Find(item.UserId).tipo;
            }

            return View(usersWithRoles);

        }
        [Authorize]
        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        [HttpPost]
        // GET: Comercializacions
        public ActionResult IndexData()
        {

            int start = Convert.ToInt32(Request["start"]);
            int draw = Convert.ToInt32(Request["draw"]);
            String search = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];
            int recordsTotal = 0;
            int count = Convert.ToInt32(Request["length"]);

           
            var dataDb = db.AspNetUsers.Where(x => true == true);

            string idUser = User.Identity.GetUserId();
            DateTime dateSearch = DateTime.MinValue;
            DateTime.TryParse(search, out dateSearch);

            if (string.IsNullOrEmpty(search) )
            {
                recordsTotal = dataDb.Count();

            }
            else
            {


                dataDb = dataDb.Where(x => x.Email.ToLower().Contains(search)
            || x.nombres.ToLower().Contains(search)
            || x.apellidoMaterno.ToLower().Contains(search)
            || x.apellidoPaterno.ToLower().Contains(search)
            || x.tipo.ToString().Contains(search)
            || x.AspNetRoles.Any(y => y.Name.Contains(search)));

               
                recordsTotal = dataDb.Count();
            }

            if (count == -1)
            {
                count = recordsTotal;
            }
            var data = dataDb.OrderByDescending(x => x.Email)
                .Skip(start)
                .Take(count)
                .ToList();


            List<object> resultset = new List<object>();
            foreach (AspNetUsers user in data)
            {

             
                resultset.Add(
                    new
                    {
                       
                        Username = user.UserName,
                        Role = string.Join(",", user.AspNetRoles.Select(x => x.Name).ToList()),
                        Tipo = user.tipo.ToString(),
                        menu = ConvertPartialViewToString(PartialView("IndexMenu", user)),
                    }
                    );



            }


            var jsonResult = Json(new { draw, recordsTotal, recordsFiltered = recordsTotal, data = resultset }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public string ConvertPartialViewToString(PartialViewResult partialView)
        {
            using (var sw = new StringWriter())
            {
                partialView.View = ViewEngines.Engines
                  .FindPartialView(ControllerContext, partialView.ViewName).View;

                var vc = new ViewContext(
                  ControllerContext, partialView.View, partialView.ViewData, partialView.TempData, sw);
                partialView.View.Render(vc, sw);

                var partialViewString = sw.GetStringBuilder().ToString();

                return partialViewString;
            }
        }

        public ActionResult Index()
        {
                
            return View();

        }
        //public async Task<ActionResult> Index()
        //{

        //    //var a = UserManager.Users.ToListAsync();
        //    return View(await UserManager.Users.ToListAsync());

        //}

        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        public ActionResult UsersWithRoles()
        {
            var usersWithRoles = (from user in context.Users
                                  select new
                                  {
                                      UserId = user.Id,
                                      Username = user.UserName,
                                      Email = user.Email,
                                      RoleNames = (from userRole in user.Roles
                                                   join role in context.Roles on userRole.RoleId
                                                   equals role.Id
                                                   select role.Name).ToList()
                                  }).ToList().Select(p => new Users_in_Role_ViewModel()

                                  {
                                      UserId = p.UserId,
                                      Username = p.Username,
                                      Email = p.Email,
                                      Role = string.Join(",", p.RoleNames)
                                  });


            return View(usersWithRoles);
        }

        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        public virtual ActionResult ListUser()
        {
            var users = UserManager.Users;
            var roles = new List<string>();
            foreach (var user in users)
            {
                string str = "";
                foreach (var role in UserManager.GetRoles(user.Id))
                {
                    str = (str == "") ? role.ToString() : str + " - " + role.ToString();
                }
                roles.Add(str);
            }
            var model = new ListUserViewModel()
            {
                users = users.ToList(),
                roles = roles.ToList()
            };
            return View(model);
        }

        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        [HttpPost]
        public ActionResult Index(string Search)
        {
            string errorMessage = "No se Encontraron Resultados";
            if (!String.IsNullOrEmpty(Search))
            {

                var users = (from c in UserManager.Users
                             where
                                  c.UserName.Contains(Search) || c.PhoneNumber.Contains(Search) ||
                                  c.Address.Contains(Search) || c.Email.Contains(Search)
                             select c).ToList();
                if (users.Count > 0)
                    errorMessage = "";
                return View(users);
            }
            else
            {
                var _user = UserManager.Users;
                errorMessage = "Debe escribir un valor para buscar";
                var users = _user.ToList();
                ViewBag.NotFound = errorMessage;
                return View(users);
            }

            // return View();
        }

        // GET: /Users/Details/5
        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        public async Task<ActionResult> Details(string id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var usuario = db.AspNetUsers.Find(user.Id);

            var userRoles = await UserManager.GetRolesAsync(user.Id);

            ViewBag.RoleNames = await UserManager.GetRolesAsync(user.Id);

            return View(new EditUserViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                // Include the Addresss info:
                Address = user.Address,
                City = user.City,
                State = user.State,
                PostalCode = user.PostalCode,
                nombres = usuario.nombres,
                apellidoPaterno = usuario.apellidoPaterno,
                apellidoMaterno = usuario.apellidoMaterno,
                run = usuario.run,
                telefono = usuario.telefono,
                fechaNacimiento = usuario.fechaNacimiento,
                RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
                {
                    Selected = userRoles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                })
            });
        }

        // GET: /Users/Create
        [CustomAuthorize(new string[] { "/AdminUsers/", "/AdminUsers/Create/" })]
        public async Task<ActionResult> Create()
        {
            //Get the list of Roles
            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
            return View();
        }

        // POST: /Users/Create
        [CustomAuthorize(new string[] { "/AdminUsers/", "/AdminUsers/Create/" })]
        [HttpPost]
        public async Task<ActionResult> Create(RegisterViewModel userViewModel, params string[] selectedRoles)
        {
            if (ModelState.IsValid & selectedRoles != null)
            {
                var user = new ApplicationUser
                {
                    UserName = userViewModel.Email,
                    Email = userViewModel.Email,
                    Address = userViewModel.Address,
                    City = userViewModel.City,
                    State = userViewModel.State,
                    PostalCode = userViewModel.PostalCode,
                    EmailConfirmed = true
                };

                //// Add the Address Info:
                //user.Address = userViewModel.Address;
                //user.City = userViewModel.City;
                //user.State = userViewModel.State;
                //user.PostalCode = userViewModel.PostalCode;
                //user.nombres = userViewModel.nombres;
                //user.apellidoPaterno = userViewModel.apellidoPaterno;
                //user.apellidoMaterno = userViewModel.apellidoMaterno;
                //user.run = userViewModel.run;
                //user.telefono = userViewModel.telefono;
                //user.fechaNacimiento = userViewModel.fechaNacimiento;

                // Then create:
                var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);

                //Add User to the selected Roles 
                if (adminresult.Succeeded)
                {
                    if (selectedRoles != null)
                    {
                        var result = await UserManager.AddToRolesAsync(user.Id, selectedRoles);
                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError("", result.Errors.First());
                            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
                            ViewBag.Result = "El Registro se guardo correctamente!";
                            return View("Index");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", adminresult.Errors.First());
                    ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
                    return View();

                }

                var usuario = db.AspNetUsers.Find(user.Id);
                usuario.nombres = userViewModel.nombres;
                usuario.apellidoPaterno = userViewModel.apellidoPaterno;
                usuario.apellidoMaterno = userViewModel.apellidoMaterno;
                usuario.run = userViewModel.run;
                usuario.telefono = userViewModel.telefono;
                usuario.fechaNacimiento = userViewModel.fechaNacimiento;
                usuario.tipo = TipoUsuario.completo;
                db.Entry(usuario).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            if (selectedRoles == null)
            {
                ModelState.AddModelError(string.Empty, "Debe seleccionar un Rol");
            }
            ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
            return View();
        }

        // GET: /Users/Edit/1
        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        public async Task<ActionResult> Edit(string id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var usuario = db.AspNetUsers.Find(user.Id);

            var userRoles = await UserManager.GetRolesAsync(user.Id);
            return View(new EditUserViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                // Include the Addresss info:
                Address = user.Address,
                City = user.City,
                State = user.State,
                PostalCode = user.PostalCode,
                nombres = usuario.nombres,
                apellidoPaterno = usuario.apellidoPaterno,
                apellidoMaterno = usuario.apellidoMaterno,
                run = usuario.run,
                telefono = usuario.telefono,
                fechaNacimiento = usuario.fechaNacimiento,
                RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
                {
                    Selected = userRoles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                })
            });
        }

        // POST: /Users/Edit/5
        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditUserViewModel editUser, params string[] selectedRole)
        {
            var user = await UserManager.FindByIdAsync(editUser.Id);
            var usuario = db.AspNetUsers.Find(user.Id);
            var userRoles = await UserManager.GetRolesAsync(user.Id);
            if (ModelState.IsValid & selectedRole != null)
            {
                // var user = await UserManager.FindByIdAsync(editUser.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                user.UserName = editUser.Email;
                user.Email = editUser.Email;

                // var userRoles = await UserManager.GetRolesAsync(user.Id);

                selectedRole = selectedRole ?? new string[] { };

                var result = await UserManager.AddToRolesAsync(user.Id, selectedRole.Except(userRoles).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                result = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.Except(selectedRole).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }

                // var usuario = db.AspNetUsers.Find(user.Id);
                usuario.nombres = editUser.nombres;
                usuario.apellidoPaterno = editUser.apellidoPaterno;
                usuario.apellidoMaterno = editUser.apellidoMaterno;
                usuario.run = editUser.run;
                usuario.telefono = editUser.telefono;
                usuario.fechaNacimiento = editUser.fechaNacimiento;
                db.Entry(usuario).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            if (selectedRole == null)
            {
                ModelState.AddModelError(string.Empty, "Debe seleccionar un Rol");
            }
            if (editUser.run == null)
            {
                ModelState.AddModelError(string.Empty, "El campo RUN es obligatorio");
            }
            editUser.RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
            {
                Selected = userRoles.Contains(x.Name),
                Text = x.Name,
                Value = x.Name
            });
            return View(editUser);
        }

        // GET: /Users/Delete/5
        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }
        private IAuthenticationManager AuthManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;

            }
        }

        // POST: /Users/Delete/5
        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                // borrar permisos del usuario
                var userCustom = from r in db.CustomPermission
                                 where r.UserID == id
                                 select r;
                foreach (var row in userCustom)
                {
                    db.CustomPermission.Remove(row);
                }
                // borrar usuario del contacto al eliminar el usuario
                var contactos = db.Contacto.Where(x => x.usuario.Id == user.Id).ToList();
                foreach (var contacto in contactos)
                {
                    var usuario = contacto.usuario;
                    contacto.usuario = null;
                }
                // borrar usuario creador del aporte de capacitacion al eliminar el usuario
                var aportesCapacitacion = db.AporteCapacitacion.Where(x => x.usuarioCreador.Id == user.Id).ToList();
                foreach (var aporteCapacitacion in aportesCapacitacion)
                {
                    var usuario = aporteCapacitacion.usuarioCreador;
                    aporteCapacitacion.usuarioCreador = null;
                }
                // borrar usuario creador de documentos de compromiso al eliminar el usuario
                var documentosCompromisoCreador = db.DocumentoCompromiso.Where(x => x.usuarioCreador.Id == user.Id).ToList();
                foreach (var documentoCompromiso in documentosCompromisoCreador)
                {
                    var usuario = documentoCompromiso.usuarioCreador;
                    documentoCompromiso.usuarioCreador = null;
                }
                // borrar usuario modificador de  documentos de compromiso al eliminar el usuario
                var documentoCompromisoModificador = db.DocumentoCompromiso.Where(x => x.usuarioUltimaModificacion.Id == user.Id).ToList();
                foreach (var documentoCompromiso in documentoCompromisoModificador)
                {
                    var usuario = documentoCompromiso.usuarioUltimaModificacion;
                    documentoCompromiso.usuarioUltimaModificacion = null;
                }
                // borrar usuario creador de evaluaciones al eliminar el usuario
                var evaluacionesCreador = db.Evaluacion.Where(x => x.usuarioCreacion.Id == user.Id).ToList();
                foreach (var evaluacion in evaluacionesCreador)
                {
                    var usuario = evaluacion.usuarioCreacion;
                    evaluacion.usuarioCreacion = null;
                }
                // borrar usuario modificador de evaluaciones al eliminar el usuario
                var evaluacionesModificador = db.Evaluacion.Where(x => x.usuarioModificacion.Id == user.Id).ToList();
                foreach (var evaluacion in evaluacionesModificador)
                {
                    var usuario = evaluacion.usuarioModificacion;
                    evaluacion.usuarioModificacion = null;
                }
                // borrar usuario creador de formualrios al eliminar el usuario
                var formularioCreador = db.Formulario.Where(x => x.usuarioCreacion.Id == user.Id).ToList();
                foreach (var formulario in formularioCreador)
                {
                    var usuario = formulario.usuarioCreacion;
                    formulario.usuarioCreacion = null;
                }
                // borrar usuario modificador de formularios al eliminar el usuario
                var formularioModificador = db.Formulario.Where(x => x.usuarioUltimaModificacion.Id == user.Id).ToList();
                foreach (var formulario in formularioModificador)
                {
                    var usuario = formulario.usuarioUltimaModificacion;
                    formulario.usuarioUltimaModificacion = null;
                }
                // borrar usuario modificador de template al eliminar el usuario
                var templates = db.Template.Where(x => x.usuarioUltimaModificacion.Id == user.Id).ToList();
                foreach (var template in templates)
                {
                    var usuario = template.usuarioUltimaModificacion;
                    template.usuarioUltimaModificacion = null;
                }
                // borrar usuario creador de comercializaciones al eliminar el usuario
                var comercializacionCreador = db.Comercializacion.Where(x => x.usuarioCreador.Id == user.Id).ToList();
                foreach (var comercializacion in comercializacionCreador)
                {
                    var usuario = comercializacion.usuarioCreador;
                    comercializacion.usuarioCreador = null;
                }
                // borrar usuario modificador de comercializaciones al eliminar el usuario
                var comercializacionModificador = db.Comercializacion.Where(x => x.usuarioUltimaEdicion.Id == user.Id).ToList();
                foreach (var comercializacion in comercializacionModificador)
                {
                    var usuario = comercializacion.usuarioUltimaEdicion;
                    comercializacion.usuarioUltimaEdicion = null;
                }
                // borrar usuario creador de descuentos al eliminar el usuario
                var comercializacionCreadorDescuento = db.Comercializacion.Where(x => x.usuarioCreadorDescuento.Id == user.Id).ToList();
                foreach (var comercializacion in comercializacionCreadorDescuento)
                {
                    var usuario = comercializacion.usuarioCreadorDescuento;
                    comercializacion.usuarioCreadorDescuento = null;
                }
                // borrar usuario creador de cotizaciones al eliminar el usuario
                var cotizaciones = db.Cotizacion_R13.Where(x => x.usuarioCreador.Id == user.Id).ToList();
                foreach (var cotizacion in cotizaciones)
                {
                    var usuario = cotizacion.usuarioCreador;
                    cotizacion.usuarioCreador = null;
                }
                // borrar usuario validacion material de cursos al eliminar el usuario
                var cursos = db.Curso.Where(x => x.usuarioValidacionMaterial.Id == user.Id).ToList();
                foreach (var curso in cursos)
                {
                    var usuario = curso.usuarioValidacionMaterial;
                    curso.usuarioValidacionMaterial = null;
                }
                // borrar usuario creador de facturas al eliminar el usuario
                var facturasCreador = db.Factura.Where(x => x.usuarioCreador.Id == user.Id).ToList();
                foreach (var factura in facturasCreador)
                {
                    var usuario = factura.usuarioCreador;
                    factura.usuarioCreador = null;
                }
                // borrar usuario modificador de comercializaciones al eliminar el usuario
                var facturaModificador = db.Factura.Where(x => x.usuarioUltimaModificacion.Id == user.Id).ToList();
                foreach (var factura in facturaModificador)
                {
                    var usuario = factura.usuarioUltimaModificacion;
                    factura.usuarioUltimaModificacion = null;
                }
                // borrar usuario creador de facturasEstado al eliminar el usuario
                var facturaEstados = db.FacturaEstadoFactura.Where(x => x.usuarioCreador.Id == user.Id).ToList();
                foreach (var facturaEstado in facturaEstados)
                {
                    var usuario = facturaEstado.usuarioCreador;
                    facturaEstado.usuarioCreador = null;
                }
                // borrar usuario modificador de historial comercializacion al eliminar el usuario
                var HistorialComercializaciones = db.HistorialComercializacion.Where(x => x.usuarioModificacion.Id == user.Id).ToList();
                foreach (var historialComercializacion in HistorialComercializaciones)
                {
                    var usuario = historialComercializacion.usuarioModificacion;
                    historialComercializacion.usuarioModificacion = null;
                }
                // borrar usuario creador de metas al eliminar el usuario
                var metas = db.Meta.Where(x => x.usuarioCreador.Id == user.Id).ToList();
                foreach (var meta in metas)
                {
                    var usuario = meta.usuarioCreador;
                    meta.usuarioCreador = null;
                }
                // borrar usuario vendero de metas vendedor al eliminar el usuario
                var metasVendedor = db.MetasVendedor.Where(x => x.vendedor.Id == user.Id).ToList();
                foreach (var metaVendedor in metasVendedor)
                {
                    var usuario = metaVendedor.vendedor;
                    metaVendedor.vendedor = null;
                }
                // borrar usuario creador de observaciones al eliminar el usuario
                var observaciones = db.Observacions.Where(x => x.usuarioCreador.Id == user.Id).ToList();
                foreach (var meta in metas)
                {
                    var usuario = meta.usuarioCreador;
                    meta.usuarioCreador = null;
                }
                // borrar usuario vendero de salidas a terreno al eliminar el usuario
                var salidasTerreno = db.SalidaTerreno.Where(x => x.vendedor.Id == user.Id).ToList();
                foreach (var salidaTerreno in salidasTerreno)
                {
                    var usuario = salidaTerreno.vendedor;
                    salidaTerreno.vendedor = null;
                }
                // borrar notificaciones del usuario
                var notificaciones = db.Notificacion.Where(x => x.usuario.Id == user.Id);
                foreach (var notificacion in notificaciones)
                {
                    db.EstadoNotificacion.RemoveRange(notificacion.estado);
                }
                db.Notificacion.RemoveRange(notificaciones);
                db.SaveChanges();
                // borrar usuario
                var result = await UserManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }

        // GET: Users/Firma
        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        public async Task<ActionResult> Firma(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUsers user = db.AspNetUsers.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            Files.borrarArchivosLocales();
            await Files.BajarArchivoADirectorioLocalAsync(user.firma);
            return View(user);
        }

        // POST: Users/Firma/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> FirmaPost(string Id)
        {
            var user = db.AspNetUsers.Find(Id);
            var firmaAntigua = user.firma;
            HttpPostedFileBase file = Request.Files[0];
            // verificar que se selecciono un archivo
            if (file.ContentLength <= 0)
            {
                ModelState.AddModelError("", "Se debe seleccionar un archivo.");
            }
            else
            {
                // validar extenciones y tamaño maximo del archivo
                var archivoValido = Files.ArchivoValido(file, new[] { ".png" }, 1024);
                if (archivoValido != "")
                {
                    ModelState.AddModelError("", archivoValido);
                }
                else
                {
                    user.firma = await Files.RemplazarArchivoAsync(user.firma, file, "usuarios/firmas/");
                    if (user.firma == null)
                    {
                        ModelState.AddModelError("", "No se pudo guardar el archivo seleccionado.");
                    }
                }
            }
            if (ModelState.IsValid)
            {
                if (firmaAntigua != null)
                {
                    db.Storages.Remove(firmaAntigua);
                }
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            user.firma = firmaAntigua;
            Files.borrarArchivosLocales();
            await Files.BajarArchivoADirectorioLocalAsync(user.firma);
            return View("Firma", user);
        }

        // GET: Users/Descargar/5
        [CustomAuthorize(new string[] { "/AdminUsers/" })]
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

        // GET: /Users/CambiarContrasenia/1
        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        public async Task<ActionResult> CambiarContrasenia(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewModelCambiarContraseniaUsuario userVM = new ViewModelCambiarContraseniaUsuario();
            userVM.idUsuario = user.Id;
            userVM.userName = user.UserName;
            return View(userVM);
        }

        // POST: /Users/CambiarContrasenia/5
        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CambiarContrasenia(ViewModelCambiarContraseniaUsuario user)
        {
            var applicationUser = await UserManager.FindByIdAsync(user.idUsuario);
            if (user.password != null)
            {
                if (!UserManager.CheckPassword(applicationUser, user.password))
                {
                    ModelState.AddModelError("password", "La contraseña ingresada es incorrecta");
                }
            }
            if (user.newPassword != user.newPasswordConfirm)
            {
                ModelState.AddModelError("", "Las contraseñas ingresadas no coinciden");
            }
            if (ModelState.IsValid)
            {
                var cambio = UserManager.ChangePassword(user.idUsuario, user.password, user.newPassword);
                if (cambio.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                foreach (var item in cambio.Errors)
                {
                    ModelState.AddModelError("", item);
                }
            }
            return View(user);
        }

        // GET: /Users/RestablecerContrasenia/1
        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        public async Task<ActionResult> RestablecerContrasenia(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var userVM = new ViewModelRestablecerContraseniaUsuario();
            userVM.idUsuario = user.Id;
            userVM.userName = user.UserName;
            return View(userVM);
        }

        // POST: /Users/RestablecerContrasenia/5
        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestablecerContrasenia(ViewModelRestablecerContraseniaUsuario user)
        {
            var applicationUser = await UserManager.FindByIdAsync(user.idUsuario);
            if (user.newPassword != user.newPasswordConfirm)
            {
                ModelState.AddModelError("", "Las contraseñas ingresadas no coinciden");
            }
            if (ModelState.IsValid)
            {
                var resetToken = UserManager.GeneratePasswordResetToken(user.idUsuario);
                var cambio = UserManager.ResetPassword(user.idUsuario, resetToken, user.newPassword);
                if (cambio.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                foreach (var item in cambio.Errors)
                {
                    ModelState.AddModelError("", item);
                }
            }
            return View(user);
        }
    }
}
