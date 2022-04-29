using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private InsecapContext db = new InsecapContext();
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var user = db.AspNetUsers.Find(userId);
            var contacto = db.Contacto.Where(x => x.usuario.Id == userId).FirstOrDefault();
            if (contacto == null)
            {
                // administrador
                foreach (AspNetRoles role in user.AspNetRoles)
                {
                    foreach (Permission permission in role.Permission)
                    {
                        foreach (var url in new string[] { "/PanelGerencia/" })
                        {
                            if (permission.Menu.MenuURL == url)
                            {
                                return RedirectToAction("Index", "PanelGerencia");
                            }
                        }
                    }
                }
                foreach (CustomPermission customPermission in user.CustomPermission)
                {
                    foreach (var url in new string[] { "/PanelGerencia/" })
                    {
                        if (customPermission.Menu.MenuURL == url)
                        {
                            return RedirectToAction("Index", "PanelGerencia");
                        }
                    }
                }
                // facturacion
                foreach (AspNetRoles role in user.AspNetRoles)
                {
                    foreach (Permission permission in role.Permission)
                    {
                        foreach (var url in new string[] { "/Factura/Facturable/" })
                        {
                            if (permission.Menu.MenuURL == url)
                            {
                                return RedirectToAction("Facturable", "Factura");
                            }
                        }
                    }
                }
                foreach (CustomPermission customPermission in user.CustomPermission)
                {
                    foreach (var url in new string[] { "/Factura/Facturable/" })
                    {
                        if (customPermission.Menu.MenuURL == url)
                        {
                            return RedirectToAction("Facturable", "Factura");
                        }
                    }
                }
                // vendedor
                foreach (AspNetRoles role in user.AspNetRoles)
                {
                    foreach (Permission permission in role.Permission)
                    {
                        foreach (var url in new string[] { "/Cotizacion_R13/" })
                        {
                            if (permission.Menu.MenuURL == url)
                            {
                                return RedirectToAction("Index", "Cotizacion_R13");
                            }
                        }
                    }
                }
                foreach (CustomPermission customPermission in user.CustomPermission)
                {
                    foreach (var url in new string[] { "/Cotizacion_R13/" })
                    {
                        if (customPermission.Menu.MenuURL == url)
                        {
                            return RedirectToAction("Index", "Cotizacion_R13");
                        }
                    }
                }
                // otro
                return PartialView();
            }
            // cliente
            if (contacto.tipoContacto == TipoContacto.Cliente)
            {
                return RedirectToAction("LandingPage", "ClienteContacto");
            }
            // relator
            if (contacto.tipoContacto == TipoContacto.Relator)
            {
                var relator = db.Relators.Where(x => x.contacto.idContacto == contacto.idContacto).FirstOrDefault();
                return RedirectToAction("CursosRealizar", "Relator", new { id = relator.idRelator });
            }
            // participante
            if (contacto.tipoContacto == TipoContacto.Participante)
            {
                return RedirectToAction("LandingPage", "Participante");
            }
            return PartialView();
        }

        [Authorize]
        public ActionResult Contact()
        {
            return PartialView();
        }

        [ChildActionOnly]
        public ActionResult LoadMenu()
        {
            string a = User.Identity.GetUserId();
            System.Collections.Generic.IEnumerable<MenuTemp> loadMenu = db.Database.SqlQuery<MenuTemp>("SP_Load_Menu @UserID ='" + a + "'").ToList();
            return View(loadMenu);
        }

        [ChildActionOnly]
        public ActionResult LoadMenuLandingPage()
        {
            string a = User.Identity.GetUserId();
            System.Collections.Generic.IEnumerable<MenuTemp> loadMenu = db.Database.SqlQuery<MenuTemp>("SP_Load_Menu @UserID ='" + a + "'").ToList();
            return View(loadMenu);
        }

        [ChildActionOnly]
        public ActionResult Avisos()
        {
            var id = User.Identity.GetUserId();
            // obtener notificaciones
            List<Notificacion> notificaciones = db.Notificacion
                .Where(n => n.usuario.Id == id)
                .ToList();
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

        public ActionResult Ayuda()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/AporteCapacitacion/" })]
        public ActionResult AporteCapacitacion()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cliente/" })]
        public ActionResult Cliente()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Contacto/" })]
        public ActionResult Contacto()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Cotizacion_R13/" })]
        public ActionResult Cotizacion()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Giro/" })]
        public ActionResult Giro()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Mandante/" })]
        public ActionResult Mandante()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/TiposDocumentosPago/" })]
        public ActionResult TiposDocumentosPago()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Comercializacions()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/", "/Comercializacions/Create/" })]
        public ActionResult ComercializacionsCreate()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/LugarAlmuerzoes/" })]
        public ActionResult LugarAlmuerzoes()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Otics/" })]
        public ActionResult Otics()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public ActionResult Participantes()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Salas/" })]
        public ActionResult Salas()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/ClienteContacto/" })]
        public ActionResult ClienteContacto()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/SalidaTerreno/MisSalidas/" })]
        public ActionResult SalidaTerrenoMisSalidas()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/SalidaTerreno/", "/SalidaTerreno/Calendario/" })]
        public ActionResult SalidaTerreno()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/Perfil/" })]
        public ActionResult RelatorPerfil()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Relator/" })]
        public ActionResult Relator()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/RelatorCurso/" })]
        public ActionResult RelatorCurso()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/CategoriaR11/" })]
        public ActionResult CategoriaR11()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Checklists/" })]
        public ActionResult Checklists()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/ListaDetalleCosto/" })]
        public ActionResult ListaDetalleCosto()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Curso/" })]
        public ActionResult Curso()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Calendarizacions/" })]
        public ActionResult Calendarizacions()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/RelatorCurso/Solicitudes/" })]
        public ActionResult RelatorCursoSolicitudes()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Factura/Facturable/" })]
        public ActionResult Factura()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/MetasVendedor/Vendedores/" })]
        public ActionResult MetasVendedor()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/MetasSucursal/Sucursales/" })]
        public ActionResult MetasSucursal()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Templates/" })]
        public ActionResult Templates()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Formularios/" })]
        public ActionResult Formularios()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/ValidarCredenciales/" })]
        public ActionResult ValidarCredenciales()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/ParametrosMoodles/Create/" })]
        public ActionResult ParametrosMoodles()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/ConfiguracionUsuarioParticipante/" })]
        public ActionResult ConfiguracionUsuarioParticipante()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/ConfiguracionUsuarioRelator/" })]
        public ActionResult ConfiguracionUsuarioRelator()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Permissions/" })]
        public ActionResult Permissions()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/AdminRoles/" })]
        public ActionResult AdminRoles()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Sucursal/" })]
        public ActionResult Sucursal()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/AdminUsers/" })]
        public ActionResult AdminUsers()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/LandingPageParticipante/" })]
        public ActionResult LandingPageParticipante()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Categoria/" })]
        public ActionResult Categoria()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/Inventario/" })]
        public ActionResult Inventario()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }

        [Authorize]
        [CustomAuthorize(new string[] { "/InventarioCaracteristicas/" })]
        public ActionResult InventarioCaracteristicas()
        {
            var urls = new List<string>();
            var userId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(userId);
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    urls.Add(permission.Menu.MenuURL);
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                urls.Add(customPermission.Menu.MenuURL);
            }
            return View(urls);
        }
    }
}