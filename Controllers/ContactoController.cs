using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using SGC.Utils;
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
    public class ContactoController : Controller
    {
        private InsecapContext db = new InsecapContext();
        //private SelectList usuarios ;
        // GET: Contacto

        [CustomAuthorize(new string[] { "/Contacto/" })]
        public ActionResult Index()
        {
            List<ViewModelContacto> lvmc = new List<ViewModelContacto>();
            var c = db.Contacto.Where(co => co.softDelete == false)
                .Where(x => x.tipoContacto == TipoContacto.Cliente).ToList();
            foreach (Contacto contacto in c)
            {
                ViewModelContacto vmc = new ViewModelContacto();
                vmc._contacto = contacto;
                var clcon = db.ClienteContacto.
                     Where(cc => cc.idContacto == contacto.idContacto)
                     .Select(v => v.idCliente).ToList();
                List<Cliente> cliente = db.Cliente.Where(cl => clcon.Contains(cl.idCliente)).ToList();
                foreach (Cliente cli in cliente)
                {
                    vmc._cliente = cli;

                }
                lvmc.Add(vmc);
            }

            return View(lvmc);
        }

        // GET: Contacto/Details/5
        [CustomAuthorize(new string[] { "/Contacto/" })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contacto contacto = db.Contacto.Find(id);
            if (contacto == null)
            {
                return HttpNotFound();
            }
            return View(contacto);
        }

        // GET: Contacto/Create
        [CustomAuthorize(new string[] { "/Contacto/", "/Contacto/Create/" })]
        public ActionResult Create()
        {
            return View();
        }

        [CustomAuthorize(new string[] { "/Cliente/", "/Cliente/Create/", "/Cotizacion_R13/", "/Cotizacion_R13/Create/", "/Otics/", "/Otics/Create/" })]
        [HttpPost]
        public ActionResult NewModalCreate(string idDropDownList, int? idCliente)
        {
            ViewBag.idDropDownList = idDropDownList;
            ViewBag.idCliente = idCliente;

            return PartialView("ModalCreate");
        }

        //[HttpPost]
        //public ActionResult ModalCreateConUsuario(String idCliente)
        //{
        //    ViewModelContacto vmc = new ViewModelContacto();
        //    string q = "SELECT anu.* FROM [DB_SGC].[dbo].[Contacto] c right join [DB_SGC].[dbo].[AspNetUsers] anu on c.usuario_Id = anu.Id where isnull(c.idContacto,1) = 1  ";

        //    usuarios = new SelectList( db.AspNetUsers.SqlQuery(q).Select(c => new SelectListItem
        //    {
        //        Text = c.Email,
        //        Value = c.Id
        //    }).ToList(),"Value","Text");
        //    vmc._cliente = db.Cliente.Find(Int32.Parse( idCliente));
        //    vmc._idCliente = idCliente;
        //    ViewBag.usuarios = this.usuarios;

        //    return PartialView("ModalCreateConUsuario",vmc);
        //}

        [CustomAuthorize(new string[] { "/Cliente/", "/Cliente/Create/", "/Cotizacion_R13/", "/Cotizacion_R13/Create/", "/Otics/", "/Otics/Create/" })]
        [HttpPost]
        public JsonResult List()
        {
            return Json((from c in Utils.Utils.GetContactosDesocupados(db)
                         let Text = "[" + c.run + "]" + " " + c.nombres + " " + c.apellidoPaterno + " " + c.apellidoMaterno
                         let Value = c.idContacto.ToString()
                         select new { Text, Value }).ToList());

        }

        [CustomAuthorize(new string[] { "/Cliente/", "/Cliente/Create/", "/Cotizacion_R13/", "/Cotizacion_R13/Create/", "/Otics/", "/Otics/Create/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ModalCreate(Contacto contacto, int? idCliente, string tipo)
        {
            if (ModelState.IsValid)
            {
                contacto.fechaCreacion = DateTime.Now;
                contacto.usuarioCreador = User.Identity.GetUserId();
                contacto.vigente = true;
                contacto.softDelete = false;
                contacto.tipoContacto = TipoContacto.Cliente;
                db.Contacto.Add(contacto);
                db.SaveChanges();

                if (idCliente != null)
                {
                    //Se relaciona con representante Legal
                    if (tipo == "idSelectRepresentanteLegal")
                    {
                        RepresentanteLegal representante = new RepresentanteLegal();
                        representante.fechaCreacion = DateTime.Now;
                        representante.idCliente = (int)idCliente;
                        representante.usuarioCreador = User.Identity.Name;
                        representante.idContacto = contacto.idContacto;

                        db.RepresentanteLegal.Add(representante);
                        db.SaveChanges();
                    }
                    //Se relaciona con encargado de pago
                    else if (tipo == "idSelectEncargadoPago" || tipo == "idSelectEncargadoPagoC")
                    {
                        EncargadoPago encargadoPago = new EncargadoPago();
                        encargadoPago.fechaCreacion = DateTime.Now;
                        encargadoPago.idCliente = (int)idCliente;
                        encargadoPago.usuarioCreador = User.Identity.Name;
                        encargadoPago.idContacto = contacto.idContacto;

                        db.EncargadoPago.Add(encargadoPago);
                        db.SaveChanges();
                    }
                    //Se relaciona con contacto
                    else if (tipo == "idSelectContacto" || tipo == "idSelectContactoC")
                    {
                        ClienteContactoCotizacion clienteContacto = new ClienteContactoCotizacion();
                        clienteContacto.fechaCreacion = DateTime.Now;
                        clienteContacto.idCliente = (int)idCliente;
                        clienteContacto.usuarioCreador = User.Identity.Name;
                        clienteContacto.idContacto = contacto.idContacto;

                        db.ClienteContactoCotizacion.Add(clienteContacto);
                        db.SaveChanges();
                    }
                }


                return new HttpStatusCodeResult(HttpStatusCode.Accepted); ;
            }
            ViewBag.idDropDownList = tipo;
            ViewBag.idCliente = idCliente;
            return PartialView("ModalCreate", contacto);
        }

        public List<Contacto> getContactos()
        {

            return db.Contacto.Where(c => c.softDelete == false).ToList();
        }

        // POST: Contacto/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Contacto/", "/Contacto/Create/" })]
        [HttpPost]
        public ActionResult Create(Contacto contacto)
        {
            if (ModelState.IsValid)
            {
                contacto.fechaCreacion = DateTime.Now;
                contacto.usuarioCreador = User.Identity.GetUserId();
                contacto.softDelete = false;
                contacto.tipoContacto = TipoContacto.Cliente;
                db.Contacto.Add(contacto);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(contacto);
        }

        //[HttpPost]
        //public ActionResult CreateConUsuario(ViewModelContacto vmc)
        //{
        //    if (vmc._idUsuario == "" || vmc._idUsuario == null)
        //    {
        //        ModelState.AddModelError("", "Se debe seleccionar un usuario");
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        vmc._contacto.usuario = db.AspNetUsers.Find(vmc._idUsuario);
        //        vmc._contacto.fechaCreacion = DateTime.Now;
        //        vmc._contacto.softDelete = false;
        //        vmc._contacto.tipoContacto = TipoContacto.Cliente;
        //        vmc._contacto.usuarioCreador = User.Identity.Name;
        //        db.Contacto.Add(vmc._contacto);
        //        db.SaveChanges();
        //        ClienteContacto cc = new ClienteContacto();
        //        cc.cliente = db.Cliente.Find(Int32.Parse(vmc. _idCliente));
        //        cc.usuarioCreador= User.Identity.Name;
        //        cc.fechaCreacion=DateTime.Now;

        //        cc.contacto = vmc._contacto;
        //        cc.vigencia = true;
        //        db.ClienteContacto.Add(cc);
        //        db.SaveChanges();
        //        return new HttpStatusCodeResult(HttpStatusCode.OK);
        //    }

        //    return PartialView(vmc);
        //}

        // GET: Contacto/Edit/5
        [CustomAuthorize(new string[] { "/Contacto/" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contacto contacto = db.Contacto.Find(id);
            if (contacto == null)
            {
                return HttpNotFound();
            }
            return View(contacto);
        }

        // POST: Contacto/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Contacto/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Contacto contacto)
        {
            var contactoBD = db.Contacto.Where(c => c.idContacto == contacto.idContacto).FirstOrDefault();
            if (contactoBD.run != contacto.run)
            {
                ModelState.AddModelError("", "No se puede Modificar el RUN");
            }
            var existMail = db.Contacto.FirstOrDefault(x => x.run != contactoBD.run && x.correo == contacto.correo);
            if (existMail != null) {
                ModelState.AddModelError("", "El contacto con RUT: "+existMail.run+" tiene el correo que esta ingresando");
            }
            contactoBD.nombres = contacto.nombres;
            contactoBD.apellidoPaterno = contacto.apellidoPaterno;
            contactoBD.apellidoMaterno = contacto.apellidoMaterno;
            contactoBD.fechaNacimiento = contacto.fechaNacimiento;
            contactoBD.telefono = contacto.telefono;
            contactoBD.correo = contacto.correo;
            if (ModelState.IsValid)
            {
                contactoBD.usuarioCreador = User.Identity.Name;
                contactoBD.fechaCreacion = DateTime.Now;
                contacto.idContacto = 0;

                db.Entry(contactoBD).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Participante", "Participante", new { id = contactoBD.idContacto});
            }
            return View(contactoBD);
        }

        //// GET: Contacto/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Contacto contacto = db.Contacto.Find(id);
        //    if (contacto == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(contacto);
        //}

        // POST: Contacto/Delete/5
        [CustomAuthorize(new string[] { "/Contacto/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Contacto contacto = db.Contacto.Find(id);

            if (contacto.tipoContacto == TipoContacto.Participante)
            {
                Moodle.EliminarUsuarioMoodle(contacto, db.ParametrosMoodles.FirstOrDefault());
            }

            //db.Contacto.Remove(contacto);
            contacto.softDelete = true;
            db.Entry(contacto).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool ValidarContactoMoodle(Contacto contactoValidar)
        {
            var validarUsuario = Moodle.ValidarSiUsuarioYaExiste(contactoValidar, db.ParametrosMoodles.FirstOrDefault());
            if (validarUsuario != "")
            {
                ModelState.AddModelError("", validarUsuario);
                return false;
            }
            var validarCorreo = Moodle.ValidarSiEmailYaExiste(contactoValidar, db.ParametrosMoodles.FirstOrDefault());
            if (validarCorreo != "")
            {
                ModelState.AddModelError("", validarCorreo);
                return false;
            }
            return true;
        }

        //private bool ValidarCorreoMoodle(Contacto contactoValidar)
        //{
        //    var validarCorreo = Moodle.ValidarSiEmailYaExiste(contactoValidar, db.ParametrosMoodles.FirstOrDefault());
        //    if (validarCorreo != "")
        //    {
        //        ModelState.AddModelError("", validarCorreo);
        //        return false;
        //    }
        //    return true;
        //}

        // GET: Contacto/AgregarUsuarioMoodle/5
        [CustomAuthorize(new string[] { "/Contacto/" })]
        public ActionResult AgregarUsuarioMoodle(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contacto contacto = db.Contacto.Find(id);
            if (contacto == null)
            {
                return HttpNotFound();
            }

            if (ValidarContactoMoodle(contacto))
            {
                contacto.idUsuarioMoodle = Moodle.CrearUsuarioMoodle(contacto, db.ParametrosMoodles.FirstOrDefault());
                var number = 0;
                if (!Int32.TryParse(contacto.idUsuarioMoodle, out number))
                {
                    ModelState.AddModelError("", contacto.idUsuarioMoodle);
                    contacto.idUsuarioMoodle = null;
                }
                else
                {
                    db.Entry(contacto).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            List<ViewModelContacto> lvmc = new List<ViewModelContacto>();
            var c = db.Contacto.Where(co => co.softDelete == false).ToList();
            foreach (Contacto item in c)
            {
                ViewModelContacto vmc = new ViewModelContacto();
                vmc._contacto = item;
                var clcon = db.ClienteContacto.
                     Where(cc => cc.idContacto == item.idContacto)
                     .Select(v => v.idCliente).ToList();
                List<Cliente> cliente = db.Cliente.Where(cl => clcon.Contains(cl.idCliente)).ToList();
                foreach (Cliente cli in cliente)
                {
                    vmc._cliente = cli;

                }
                lvmc.Add(vmc);
            }

            return View("Index", lvmc);
        }

        // GET: Contacto/ActualizarUsuarioMoodle/5
        [CustomAuthorize(new string[] { "/Contacto/" })]
        public ActionResult ActualizarUsuarioMoodle(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contacto contacto = db.Contacto.Find(id);
            if (contacto == null)
            {
                return HttpNotFound();
            }

            //if (ValidarCorreoMoodle(contacto))
            //{
            var resultado = Moodle.EditarUsuarioMoodle(contacto, db.ParametrosMoodles.FirstOrDefault());
            if (resultado != "")
            {
                ModelState.AddModelError("", resultado);
            }
            //}

            List<ViewModelContacto> lvmc = new List<ViewModelContacto>();
            var c = db.Contacto.Where(co => co.softDelete == false).ToList();
            foreach (Contacto item in c)
            {
                ViewModelContacto vmc = new ViewModelContacto();
                vmc._contacto = item;
                var clcon = db.ClienteContacto.
                     Where(cc => cc.idContacto == item.idContacto)
                     .Select(v => v.idCliente).ToList();
                List<Cliente> cliente = db.Cliente.Where(cl => clcon.Contains(cl.idCliente)).ToList();
                foreach (Cliente cli in cliente)
                {
                    vmc._cliente = cli;

                }
                lvmc.Add(vmc);
            }

            return View("Index", lvmc);
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
