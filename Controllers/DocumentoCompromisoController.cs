using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using SGC.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    [CustomAuthorize(new string[] { "/Factura/Facturable/" })]
    public class DocumentoCompromisoController : Controller
    {
        private InsecapContext db = new InsecapContext();


        public void UpdateR24(Comercializacion comercializacion)
        {
            if (comercializacion.r24 != null)
            {

                db.R24.Add(new R24
                {
                    dateUpload = DateTime.Now,
                    comercializacion = comercializacion,
                    description = "Actualizado por el sistema",
                    file = comercializacion.r24,
                    userUpload = db.AspNetUsers.Find(User.Identity.GetUserId())
                });
                comercializacion.r24 = null;
                db.Entry(comercializacion).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
        //Jordanys new code
        // GET: Comercializacions/IngresarR24/5
        [Authorize]
        public async Task<ActionResult> VerR24(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var comercializacion = db.Comercializacion.Find(id);
            if (comercializacion == null)
            {
                return HttpNotFound();
            }

            UpdateR24(comercializacion);
            Files.borrarArchivosLocales();
            List<R24> r24s = db.R24.Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion).ToList();
            ViewBag.r24s = r24s;

            foreach (R24 r24 in r24s)
            {
                await Files.BajarArchivoADirectorioLocalAsync(r24.file);
            }

            return View(comercializacion);

        }
        //// GET: DocumentoCompromiso
        //public ActionResult Index()
        //{
        //    return View(db.DocumentoCompromiso.ToList());
        //}

        // GET: DocumentoCompromiso/DocumentosCompromisoComercializacion/5
        public ActionResult DocumentosCompromisoComercializacion(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewModelFacturacion facturacion = new ViewModelFacturacion();
            facturacion.comercializacion = db.Comercializacion.Find(id);
            facturacion.r11 = db.R11.Where(r => r.idCurso == facturacion.comercializacion.cotizacion.idCurso).FirstOrDefault();


            Contacto cliente = db.Contacto.Where(x => x.idContacto == facturacion.comercializacion.cotizacion.contacto).FirstOrDefault();
           
            List<string> codigoCot = new List<string>();
            List<string> idCom = new List<string>();
            foreach (DocumentoCompromiso documento in facturacion.comercializacion.cotizacion.documentosCompromiso)
            {
                List<DocumentoCompromiso> allDoc = db.DocumentoCompromiso
                .Where(c => c.numeroSerie == documento.numeroSerie)
              .Where(c => c.cotizacion.idCliente == facturacion.comercializacion.cotizacion.idCliente)
              .Where(c => c.cotizacion.idCotizacion_R13 != facturacion.comercializacion.cotizacion.idCotizacion_R13)
.ToList();


                string codTemp = "";
                foreach (DocumentoCompromiso documentoId in allDoc)
                {
                    Comercializacion comercializacionDoc = db.Comercializacion
              .Where(c => c.cotizacion.idCotizacion_R13 == documentoId.cotizacion.idCotizacion_R13)
              .FirstOrDefault();
                    if (codTemp != "")
                    {
                        codTemp = String.Join(",", new string[2] { codTemp, comercializacionDoc.idComercializacion.ToString() });
                    }
                    else
                    {
                        codTemp = comercializacionDoc.idComercializacion.ToString();
                    }




                }
                idCom.Add(codTemp);
                codigoCot.Add(String.Join(",", allDoc.Select(x => x.cotizacion.codigoCotizacion)));

            }

            ViewBag.codigoCot = codigoCot;
            ViewBag.idCom = idCom;
            ViewBag.Contacto = cliente;
            ViewBag.Encargado = db.Contacto.Find(facturacion.comercializacion.cotizacion.contactoEncargadoPago);
            if (facturacion.comercializacion == null)
            {
                return HttpNotFound();
            }
            return View(facturacion);
        }

        // GET: DocumentoCompromiso/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentoCompromiso documentoCompromiso = db.DocumentoCompromiso.Find(id);
            if (documentoCompromiso == null)
            {
                return HttpNotFound();
            }
            return View(documentoCompromiso);
        }

        public SelectList GetTiposDocCompromisoCliente(int idCliente)
        {
            return new SelectList(db.ClienteTipoDocumentosPago
                .Where(t => t.idCliente == idCliente)
                .Select(c => new SelectListItem
                {
                    Text = c.tipoDocumentosPago.nombre,
                    Value = c.tipoDocumentosPago.idTipoDocumentosPago.ToString()
                }).ToList(), "Value", "Text");
        }

        // GET: DocumentoCompromiso/Create
        public ActionResult Create(int? id, int? id2)
        {
            var comercializacion = db.Comercializacion.Find(id);
            ViewBag.tiposDocCompromiso = GetTiposDocCompromisoCliente(comercializacion.cotizacion.cliente.idCliente);
            ViewBag.idComercializacion = id;
            ViewBag.codigoSence = comercializacion.cotizacion.codigoSence;
            ViewBag.tieneCodigoSence = comercializacion.cotizacion.tieneCodigoSence;
            ViewBag.otics = GetOtics();
            ViewBag.idDocumentoCompromisoRemplazar = id2;
            return View();
        }

        private void ValidarMontoFinal(Comercializacion comercializacion)
        {
            int montoAcmulado = 0;
            foreach (var item in comercializacion.cotizacion.documentosCompromiso.Where(x => x.softDelete == false).ToList())
            {
                if (item.monto != null)
                {
                    montoAcmulado += (int)item.monto;
                }
            }
            if (montoAcmulado != comercializacion.valorFinal - comercializacion.descuento)
            {
                ModelState.AddModelError("", "El monto total " + ((int)comercializacion.valorFinal - comercializacion.descuento).ToString("C", CultureInfo.CurrentCulture) + " no coincide con el total de los montos ingresados " + montoAcmulado.ToString("C", CultureInfo.CurrentCulture));
            }
        }

        // POST: DocumentoCompromiso/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "idDocumentoCompromiso,numeroSerie,monto,tipoVenta")] DocumentoCompromiso documentoCompromiso, int idComercializacion, int? idDocumentoCompromisoRemplazar)
        {
            var comercializacion = db.Comercializacion.Find(idComercializacion);
            if (comercializacion.cotizacion.documentosCompromiso == null)
            {
                comercializacion.cotizacion.documentosCompromiso = new List<DocumentoCompromiso>();
            }
            if (Request["tipoDocCompromiso.idTipoDocumentosPago"] == "" && documentoCompromiso.tipoVenta.tipoPago == TipoPago.CostoEmpresa)
            {
                ModelState.AddModelError("tipoDocCompromiso", "El campo Tipo de Documento es obligatorio.");
            }
            // guardar archivos
            HttpPostedFileBase file = Request.Files["file"];
            // validar extenciones y tamaño maximo de los archivos
            if (file.ContentLength > 0)
            {
                var archivoValido = Files.ArchivoValido(file, new[] { ".pdf" }, 3 * 1024);
                if (archivoValido != "")
                {
                    ModelState.AddModelError("archivo", archivoValido);
                }
            }
            else
            {
                ModelState.AddModelError("archivo", "Se debe seleccionar un archivo.");
            }
            if (documentoCompromiso.tipoVenta.tipoPago == TipoPago.Otic)
            {
                if (Request["otics"] != null && Request["otics"] != "")
                {
                    documentoCompromiso.tipoVenta.otic = db.Otic.Find(int.Parse(Request["otics"]));
                }
            }
            documentoCompromiso.fechaCreacion = DateTime.Now;
            documentoCompromiso.fechaUltimaModificacion = DateTime.Now;
            if (documentoCompromiso.tipoVenta.tipoPago == TipoPago.CostoEmpresa)
            {
                if (Request["tipoDocCompromiso.idTipoDocumentosPago"] != null && Request["tipoDocCompromiso.idTipoDocumentosPago"] != "")
                {
                    documentoCompromiso.tipoDocCompromiso = db.TiposDocumentosPago.Find(int.Parse(Request["tipoDocCompromiso.idTipoDocumentosPago"]));
                }
            }
            documentoCompromiso.usuarioCreador = db.AspNetUsers.Find(User.Identity.GetUserId());
            documentoCompromiso.usuarioUltimaModificacion = db.AspNetUsers.Find(User.Identity.GetUserId());
            documentoCompromiso.softDelete = false;
            comercializacion.cotizacion.documentosCompromiso.Add(documentoCompromiso);
            // remplazar documento compromiso seleccionado, es opcional
            if (idDocumentoCompromisoRemplazar != null)
            {
                comercializacion.cotizacion.documentosCompromiso.Where(x => x.idDocumentoCompromiso == idDocumentoCompromisoRemplazar).FirstOrDefault().monto = 0;
            }
            ValidarMontoFinal(comercializacion);
            if (ModelState.IsValid)
            {
                // guardar archivo
                documentoCompromiso.documento = await Files.CrearArchivoAsync(file, "documento-compromiso/");
                if (documentoCompromiso.documento == null)
                {
                    ModelState.AddModelError("archivo", "No se pudo guardar el archivo seleccionado.");
                }
            }
            if (ModelState.IsValid)
            {
                //db.DocumentoCompromiso.Add(documentoCompromiso);
                comercializacion.cotizacion.sucursal = db.Sucursal.Find(comercializacion.cotizacion.sucursal.idSucursal);
                db.Entry(comercializacion.cotizacion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("DocumentosCompromisoComercializacion", "DocumentoCompromiso", new { id = idComercializacion });
            }
            ViewBag.tiposDocCompromiso = GetTiposDocCompromisoCliente(comercializacion.cotizacion.cliente.idCliente);
            ViewBag.idComercializacion = idComercializacion;
            ViewBag.codigoSence = comercializacion.cotizacion.codigoSence;
            ViewBag.tieneCodigoSence = comercializacion.cotizacion.tieneCodigoSence;
            ViewBag.otics = GetOtics();
            ViewBag.idDocumentoCompromisoRemplazar = idDocumentoCompromisoRemplazar;
            return View(documentoCompromiso);
        }

        public SelectList GetTiposDocCompromisoClienteConSeleccionado(Cliente cliente, DocumentoCompromiso documentoCompromiso)
        {
            List<ClienteTipoDocumentosPago> tipoDocPago = db.ClienteTipoDocumentosPago
                .Where(x => x.tipoDocumentosPago.softDelete == false)
                .Where(x => x.idCliente == cliente.idCliente)
                .ToList();
            var contiene = false;
            foreach (var item in tipoDocPago)
            {
                if (item.tipoDocumentosPago == documentoCompromiso.tipoDocCompromiso)
                {
                    contiene = true;
                }
            }
            if (!contiene)
            {
                var nuevoTipoDocPago = new ClienteTipoDocumentosPago();
                nuevoTipoDocPago.tipoDocumentosPago = documentoCompromiso.tipoDocCompromiso;
                tipoDocPago.Add(nuevoTipoDocPago);
            }
            return new SelectList(tipoDocPago.Select(c => new SelectListItem
            {
                Text = c.tipoDocumentosPago.nombre,
                Value = c.tipoDocumentosPago.idTipoDocumentosPago.ToString()
            }).ToList(), "Value", "Text");
        }

        // GET: DocumentoCompromiso/Edit/5
        public ActionResult Edit(int? id, int? id2)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentoCompromiso documentoCompromiso = db.DocumentoCompromiso.Find(id);
            if (documentoCompromiso == null)
            {
                return HttpNotFound();
            }
            var comercializacion = db.Comercializacion.Find(id2);
            if (documentoCompromiso.tipoDocCompromiso != null)
            {
                ViewBag.tiposDocCompromiso = GetTiposDocCompromisoClienteConSeleccionado(comercializacion.cotizacion.cliente, documentoCompromiso);
            }
            else
            {
                ViewBag.tiposDocCompromiso = GetTiposDocCompromisoCliente(comercializacion.cotizacion.cliente.idCliente);
            }
            ViewBag.idComercializacion = id2;
            ViewBag.codigoSence = comercializacion.cotizacion.codigoSence;
            ViewBag.tieneCodigoSence = comercializacion.cotizacion.tieneCodigoSence;
            ViewBag.otics = GetOtics();
            return View(documentoCompromiso);
        }

        // POST: DocumentoCompromiso/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "idDocumentoCompromiso,numeroSerie,monto,tipoVenta")] DocumentoCompromiso documentoCompromiso, int idComercializacion)
        {
            if (Request["tipoDocCompromiso.idTipoDocumentosPago"] == "" && documentoCompromiso.tipoVenta.tipoPago == TipoPago.CostoEmpresa)
            {
                ModelState.AddModelError("tipoDocCompromiso", "El campo Tipo de Documento es obligatorio.");
            }
            var documentoCompromisoBD = db.DocumentoCompromiso.Find(documentoCompromiso.idDocumentoCompromiso);
            // agregar el doc compromiso
            documentoCompromisoBD.monto = documentoCompromiso.monto;
            documentoCompromisoBD.numeroSerie = documentoCompromiso.numeroSerie;
            documentoCompromisoBD.tipoVenta = documentoCompromiso.tipoVenta;
            if (documentoCompromisoBD.tipoVenta.tipoPago == TipoPago.Otic)
            {
                if (Request["otics"] != null && Request["otics"] != "")
                {
                    documentoCompromisoBD.tipoVenta.otic = db.Otic.Find(int.Parse(Request["otics"]));
                }
            }
            if (documentoCompromisoBD.tipoVenta.tipoPago == TipoPago.CostoEmpresa)
            {
                if (Request["tipoDocCompromiso.idTipoDocumentosPago"] != null && Request["tipoDocCompromiso.idTipoDocumentosPago"] != "")
                {
                    documentoCompromisoBD.tipoDocCompromiso = db.TiposDocumentosPago.Find(int.Parse(Request["tipoDocCompromiso.idTipoDocumentosPago"]));
                }
            }
            else
            {
                var a = documentoCompromisoBD.tipoDocCompromiso;
                documentoCompromisoBD.tipoDocCompromiso = null;
            }
            documentoCompromisoBD.fechaUltimaModificacion = DateTime.Now;
            documentoCompromisoBD.usuarioUltimaModificacion = db.AspNetUsers.Find(User.Identity.GetUserId());
            var comercializacion = db.Comercializacion.Find(idComercializacion);
            var doc = comercializacion.cotizacion.documentosCompromiso.Where(x => x.idDocumentoCompromiso == documentoCompromisoBD.idDocumentoCompromiso).FirstOrDefault();
            doc = documentoCompromisoBD;
            ValidarMontoFinal(comercializacion);
            // guardar archivos
            HttpPostedFileBase file = Request.Files["file"];
            // validar extenciones y tamaño maximo de los archivos
            if (file.ContentLength > 0)
            {
                var archivoValido = Files.ArchivoValido(file, new[] { ".pdf" }, 3 * 1024);
                if (archivoValido != "")
                {
                    ModelState.AddModelError("archivo", archivoValido);
                }
                if (ModelState.IsValid)
                {
                    await Files.BorrarArchivoAsync(documentoCompromisoBD.documento);
                    db.Storages.Remove(documentoCompromisoBD.documento);
                    // guardar archivo
                    documentoCompromisoBD.documento = await Files.CrearArchivoAsync(file, "documento-compromiso/");
                    if (documentoCompromisoBD.documento == null)
                    {
                        ModelState.AddModelError("archivo", "No se pudo guardar el archivo seleccionado.");
                    }
                }
            }
            if (ModelState.IsValid)
            {
                documentoCompromisoBD.cotizacion.sucursal = db.Sucursal.Find(documentoCompromisoBD.cotizacion.sucursal.idSucursal);
                db.Entry(documentoCompromisoBD).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("DocumentosCompromisoComercializacion", "DocumentoCompromiso", new { id = idComercializacion });
            }
            if (documentoCompromiso.tipoDocCompromiso != null)
            {
                ViewBag.tiposDocCompromiso = GetTiposDocCompromisoClienteConSeleccionado(comercializacion.cotizacion.cliente, documentoCompromiso);
            }
            else
            {
                ViewBag.tiposDocCompromiso = GetTiposDocCompromisoCliente(comercializacion.cotizacion.cliente.idCliente);
            }
            ViewBag.idComercializacion = idComercializacion;
            ViewBag.codigoSence = comercializacion.cotizacion.codigoSence;
            ViewBag.tieneCodigoSence = comercializacion.cotizacion.tieneCodigoSence;
            ViewBag.otics = GetOtics();
            return View(documentoCompromisoBD);
        }

        // GET: DocumentoCompromiso/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentoCompromiso documentoCompromiso = db.DocumentoCompromiso.Find(id);
            if (documentoCompromiso == null)
            {
                return HttpNotFound();
            }
            return View(documentoCompromiso);
        }

        // POST: DocumentoCompromiso/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            DocumentoCompromiso documentoCompromiso = db.DocumentoCompromiso.Find(id);
            documentoCompromiso.softDelete = true;
            await Files.BorrarArchivoAsync(documentoCompromiso.documento);
            db.Storages.Remove(documentoCompromiso.documento);
            db.Entry(documentoCompromiso).State = EntityState.Modified;
            //db.DocumentoCompromiso.Remove(documentoCompromiso);
            db.SaveChanges();
            return RedirectToAction("DocumentosCompromisoComercializacion", "DocumentoCompromiso", new { id = int.Parse(Request["idComercializacion"]) });
        }

        // GET: DocumentoCompromiso/Descargar/5
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public SelectList GetOtics()
        {
            return new SelectList(db.Otic.Where(x => x.softDelete == false).Select(c => new SelectListItem
            {
                Text = c.nombre,
                Value = c.idOtic.ToString()
            }).ToList(), "Value", "Text");
        }
    }
}
