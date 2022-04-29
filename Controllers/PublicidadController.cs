using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using SGC.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SGC.Controllers
{

    public class PublicidadController : Controller
    {
        private InsecapContext db = new InsecapContext();
        private Regex urlchk = new Regex(@"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?", RegexOptions.Singleline | RegexOptions.IgnoreCase);


       
        public ActionResult Slider(List<Publicidad> publicidades)
        {
            return View(publicidades);
        }
        // GET: Publicidad
        public ActionResult Index()
        {
            var publicidades = db.Publicidad.ToList();
            return View(publicidades);
        }
        public ActionResult DetailsSlider()
        {
            var publicidades = db.Publicidad.ToList();
            var clientes = db.Cliente.ToList();
            clientes.Insert(0, new Cliente { nombreEmpresa = "Todos", idCliente = 0 });
            ViewBag.clientes = clientes;
            return View(publicidades);
        }
        [HttpPost]
        public ActionResult DetailsSlider(string clientes)
        {
            var clienteId = 0;
            var now = DateTime.Now;
            if (clientes != "" && clientes != null)
            {
                clienteId = Convert.ToInt32(clientes);
            }
            else
            {
                ModelState.AddModelError("clientes", "Se debe seleccionar al menos un cliente");

            }
            List<Publicidad> publicidades = new List<Publicidad>();
            if (ModelState.IsValid && clienteId != null && clienteId != 0)
            {
                publicidades = db.Publicidad.Where(x => x.tipo.Contains("Todos") || x.publicidadClientes.Any(y => y.cliente.idCliente == clienteId))
                    .Where(x => DbFunctions.TruncateTime(x.vigencia) >= now)
                    .ToList();

            }
            else {
                publicidades = db.Publicidad.Where(x => DbFunctions.TruncateTime(x.vigencia) >= now).ToList();
            }

            
            var clientesList = db.Cliente.ToList();
            clientesList.Insert(0, new Cliente { nombreEmpresa = "Todos", idCliente = 0 });
            ViewBag.clientes = clientesList;
            return View(publicidades);
        }
        // GET: Publicidad/Details/5
        public ActionResult Details(int id)
        {
            return View(db.Publicidad.Find(id));
        }

        // GET: Publicidad/Create
        public ActionResult Create(int id = 0)
        {
            ViewBag.clientes = db.Cliente.ToList();
            var model = new Publicidad();
            model.idPublicidad = 0;
            model.vigencia = DateTime.Now;
            model.publicidadClientes = new List<PublicidadCliente>();
            if (id != 0)
            {
                 model = db.Publicidad.Find(id);
            }
          
          
        
            return View(model);
        }

        // POST: Publicidad/Create
        [HttpPost]
        public async Task<ActionResult> Create(Publicidad publicidad )
        {
            publicidad.publicidadClientes =  new List<PublicidadCliente>() ;
            var clientes = db.Cliente.ToList();
            ViewBag.clientes = clientes;
            var model = db.Publicidad.Find(publicidad.idPublicidad);
            List<string> clientesSeleccionados = new List<string>();
            if (publicidad.tipo != null && publicidad.tipo.ToLower().Contains("cliente"))
            {
                if (Request["clientes"] != "" && Request["clientes"] != null)
                {
                    clientesSeleccionados.AddRange(Request["clientes"].Split(','));
                }
                else
                {
                    ModelState.AddModelError("clientes", "Debe seleccionar Al menos un cliente");

                }

            }
            else if (publicidad.tipo == null || publicidad.tipo == "") {
                ModelState.AddModelError("tipo", "Debe seleccionar un Tipo");

            }

            try
            {
               
                HttpPostedFileBase file = Request.Files[0];
                // verificar que se selecciono un archivo
                if ((file.ContentLength <= 0 && model != null && model.foto == null) || (file.ContentLength <= 0 && model == null))
                {
                    ModelState.AddModelError("foto", "Se debe seleccionar un archivo.");
                }
                else if(file.ContentLength > 0)
                {
                    // validar extenciones y tamaño maximo del archivo
                    var archivoValido = Files.ArchivoValido(file, new[] { ".jpeg", ".jpg", ".png" }, 5 * 1024);
                    if (archivoValido != "")
                    {
                        ModelState.AddModelError("", archivoValido);
                    }
                    else
                    {
                        publicidad.foto = await Files.RemplazarArchivoPublicoAsync(publicidad.foto, file, "publicidad/foto/");
                        if (publicidad.foto == null)
                        {
                            ModelState.AddModelError("", "No se pudo guardar el archivo seleccionado.");
                        }
                    }
                }

                if (publicidad.link != null && !urlchk.IsMatch(publicidad.link))
                {
                    ModelState.AddModelError("link", "Formato incorrecto del link");
              
                }
                // TODO: Add insert logic here
               
                if (ModelState.IsValid)
                {


                    if (model == null)
                    {
                        publicidad.usuarioActualizo = db.AspNetUsers.Find(User.Identity.GetUserId());
                        publicidad.usuarioCreador = publicidad.usuarioActualizo;
                        publicidad.fechaActualizacion = DateTime.Now;
                        publicidad.fechaCreacion = publicidad.fechaActualizacion;
                        db.Publicidad.Add(publicidad);
                        db.SaveChanges();
                        foreach (var item in clientesSeleccionados) {
                            db.PublicidadCliente.Add(new PublicidadCliente
                            {
                                cliente = clientes.FirstOrDefault(x => x.idCliente == Convert.ToInt32(item)),
                                publicidad = publicidad
                            }) ;
                        }
                        db.SaveChanges();

                    }
                    else
                    {

                        model.usuarioActualizo = db.AspNetUsers.Find(User.Identity.GetUserId());
                        model.fechaActualizacion = DateTime.Now;
                        model.tipo = publicidad.tipo;
                        model.nombre = publicidad.nombre;
                        model.descripcion = publicidad.descripcion;
                        model.titulo = publicidad.titulo;
                         if (file.ContentLength > 0 )
                               model.foto = publicidad.foto;
                        model.vigencia = publicidad.vigencia;

                        db.Entry(model).State = EntityState.Modified;

                        //eliminar publicidad clientes
                        if (model.publicidadClientes != null) {
                            foreach (var item in model.publicidadClientes.ToList())
                            {
                                db.PublicidadCliente.Remove(item);
                            }
                        }
                       
                        //agregar los nuevos clientes en la publicidad
                        foreach (var item in clientesSeleccionados)
                        {
                            db.PublicidadCliente.Add(new PublicidadCliente
                            {
                                cliente = clientes.FirstOrDefault(x => x.idCliente == Convert.ToInt32(item)),
                                publicidad = model
                            });
                        }
                        db.SaveChanges();
                    }
                  
                }
                else {
                    return View(publicidad);
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);

                return View(publicidad);
            }
        }

        // GET: Publicidad/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

      

        // GET: Publicidad/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                var publicidad = db.Publicidad.Find(id);
                db.Storages.Remove(publicidad.foto);
                foreach(var item in publicidad.publicidadClientes.ToList()) {
                    db.PublicidadCliente.Remove(item);
                }
                db.Publicidad.Remove(publicidad);
                db.SaveChanges();
                ModelState.AddModelError("", "Eliminado correctamente");
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);

            }
            return RedirectToAction("Index");
        }


    }
}
