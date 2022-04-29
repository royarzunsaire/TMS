using Microsoft.AspNet.Identity;
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
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    [CustomAuthorize(new string[] { "/Factura/Facturable/" })]
    public class FacturaController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: Factura
        //public ActionResult Index()
        //{
        //    return View(db.Factura.Where(f => f.softDelete == false).ToList());
        //}

        // GET: Factura/Facturable
        public ActionResult Facturable()
        {
            ViewBag.noFacturadas = db.Comercializacion
            .Where(c => c.softDelete == false).Where(c => c.cotizacion.sucursal.nombre != "Distancia").Where(c => c.cotizacion.sucursal.nombre != "SPD")
            .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada)
            .Count(x => x.cotizacion.documentosCompromiso.All(dc => dc.softDelete == false && dc.factura == null));

            ViewBag.noFacturadasSence = db.Comercializacion
        .Where(c => c.softDelete == false && c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on")
        .Where(c => c.cotizacion.sucursal.nombre != "Distancia").Where(c => c.cotizacion.sucursal.nombre != "SPD")
        .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada)
        .Count(x => x.cotizacion.documentosCompromiso.All(dc => dc.softDelete == false && dc.factura == null));

            ViewBag.facturadasParcial = db.Comercializacion
   .Where(c => c.softDelete == false).Where(c => c.cotizacion.sucursal.nombre != "Distancia").Where(c => c.cotizacion.sucursal.nombre != "SPD")
   .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada)
    .Count(x => x.cotizacion.documentosCompromiso.Count(dc => dc.softDelete == false && dc.factura != null) < x.cotizacion.documentosCompromiso.Count(dc => dc.softDelete == false) && x.cotizacion.documentosCompromiso.Any(dc => dc.softDelete == false && dc.factura != null));


            ViewBag.noFacturadasTerminadaSence = db.Comercializacion
         .Where(c => c.softDelete == false).Where(c => c.cotizacion.sucursal.nombre != "Distancia").Where(c => c.cotizacion.sucursal.nombre != "SPD")
         .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada_SENCE)
         .Count(x => x.cotizacion.documentosCompromiso.All(dc => dc.softDelete == false && dc.factura == null));


      
            return View();
        }


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
            bool nf = Convert.ToBoolean(Request["nf"]);
            bool nfs = Convert.ToBoolean(Request["nfs"]);
            bool ts = Convert.ToBoolean(Request["ts"]);
            bool pt = Convert.ToBoolean(Request["pt"]);
            bool tst = Convert.ToBoolean(Request["tst"]);

            var dataDb = db.Comercializacion
            .Where(c => c.softDelete == false)
             .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada
             || x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada_SENCE);


            DateTime dateSearch = DateTime.MinValue;
            DateTime.TryParse(search, out dateSearch);

            List<Comercializacion> listaTerminadasTerminadasSENCE = new List<Comercializacion>();


            if (!nf && !nfs && !ts && !pt && !tst)
            {
                dataDb = dataDb.Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada
              || x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada_SENCE);

            }

            if (string.IsNullOrEmpty(search) && !nf && !nfs && !ts && !pt && !tst)
            {
                recordsTotal = dataDb.Count();
            }
            else
            {

                if (!string.IsNullOrEmpty(search))
                {
                    List<int> DocumentosList = db.DocumentoCompromiso.Where(x => x.softDelete == false && x.numeroSerie.Contains(search) || x.factura.numero.Contains(search)).Select(x => x.cotizacion.idCotizacion_R13).ToList();

                    dataDb = dataDb.Where(x => x.cotizacion.codigoCotizacion.ToLower().Contains(search)
                    || x.cotizacion.cliente.nombreEmpresa.ToLower().Contains(search)
                    || x.cotizacion.curso.nombreCurso.ToLower().Contains(search)
                    || x.cotizacion.tipoCurso.ToLower().Contains(search)
                    || x.cotizacion.curso.tipoEjecucion.ToString().ToLower().Contains(search)
                    || x.usuarioCreador.nombres.ToLower().Contains(search)
                    || x.usuarioCreador.apellidoMaterno.ToLower().Contains(search)
                    || x.usuarioCreador.apellidoPaterno.ToLower().Contains(search)
                    || DateTime.Compare(x.fechaInicio, dateSearch) == 0
                    || DateTime.Compare(x.fechaTermino, dateSearch) == 0
                    || DocumentosList.Any(y => y == x.cotizacion.idCotizacion_R13)
                    );
                }

                if (nf || nfs || ts || pt || tst)
                {
                    if (nf)
                    {

                        dataDb = dataDb.Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada
                        && x.cotizacion.documentosCompromiso.All(dc => dc.softDelete == false && dc.factura == null)
                        );


                    }
                    else if (nfs)
                    {
                        dataDb = dataDb.Where(c => c.cotizacion.codigoSence != null && c.cotizacion.codigoSence != "" && c.cotizacion.tieneCodigoSence != "on")
                        .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada)
                        .Where(x => x.cotizacion.documentosCompromiso.All(dc => dc.softDelete == false && dc.factura == null));

                    }
                    else if (ts)
                    {
                        dataDb = dataDb.Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada_SENCE)
                        .Where(x => x.cotizacion.documentosCompromiso.All(dc => dc.softDelete == false && dc.factura == null));

                    }
                    else if (pt)
                    {
                        dataDb = dataDb.Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada)
                       .Where(x => x.cotizacion.documentosCompromiso.Count(dc => dc.softDelete == false && dc.factura != null) < x.cotizacion.documentosCompromiso.Count(dc => dc.softDelete == false) && x.cotizacion.documentosCompromiso.Any(dc => dc.softDelete == false && dc.factura != null));

                    }

                    else if (tst)
                    {

                        var dataECT = db.ComercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).Where(x => x.EstadoComercializacion == EstadoComercializacion.Terminada);
                        var dataECTS = db.ComercializacionEstadoComercializacion.Where(x => x.EstadoComercializacion == EstadoComercializacion.Terminada_SENCE);

                        foreach (var ComTerminada in dataECT)
                        {
                            foreach (var ComTerminadaSENCE in dataECTS)
                            {
                                if (ComTerminada.comercializacion.idComercializacion == ComTerminadaSENCE.comercializacion.idComercializacion)
                                {
                                    listaTerminadasTerminadasSENCE.Add(ComTerminada.comercializacion);
                                }
                            }
                        }

                    }
                }



                recordsTotal = dataDb.Count();


            }

            if (count == -1)
            {
                count = recordsTotal;
            }

            var data = dataDb.OrderByDescending(x => x.fechaCreacion)
                .Skip(start)
                .Take(count)
                .ToList();


            if (tst == true)
            {
                recordsTotal = listaTerminadasTerminadasSENCE.Count();
                if (count == -1)
                {
                    count = recordsTotal;
                }
                data = listaTerminadasTerminadasSENCE.Skip(start).Take(count).ToList();

            }

            List<object> resultset = new List<object>();
            foreach (Comercializacion comercializacion in data)
            {
                var curso = "";
                if (comercializacion.cotizacion.tipoCurso != "Duplicado Credencial" && comercializacion.cotizacion.tipoCurso != "Arriendo de Sala" && comercializacion.cotizacion.tipoCurso != "Tramitación Licencia")
                {
                    curso = comercializacion.cotizacion.curso.nombreCurso;
                }
                else
                {
                    curso = comercializacion.cotizacion.tipoCurso;

                }

                if (tst == true)
                {
                    var xd = db.ComercializacionEstadoComercializacion
                        .Where(x => x.comercializacion.idComercializacion == comercializacion.idComercializacion)
                        .Where(y => y.EstadoComercializacion == EstadoComercializacion.Terminada).FirstOrDefault();
                    curso = curso + "\n[Terminado: " + xd.fechaCreacion.ToString("dd/MM/yyyy") + " ]";
                }

                var menorEstado = 999;
                var parcialmenteFacturado = false;
                var nombreMenorEstado = EstadoFactura.No_Facturado.ToString();
                foreach (var itemDoc in comercializacion.cotizacion.documentosCompromiso.Where(dc => dc.softDelete == false))
                {
                    if (itemDoc.factura != null)
                    {
                        if (itemDoc.factura.softDelete == false)
                        {
                            if ((int)itemDoc.factura.estados.LastOrDefault().estado < menorEstado)
                            {
                                menorEstado = (int)itemDoc.factura.estados.LastOrDefault().estado;
                                nombreMenorEstado = itemDoc.factura.estados.LastOrDefault().estado.ToString();
                            }
                            else
                            {
                                parcialmenteFacturado = true;
                            }
                        }
                        else
                        {
                            if (menorEstado > 0 && menorEstado < 6)
                            {
                                parcialmenteFacturado = true;
                            }
                            menorEstado = (int)EstadoFactura.No_Facturado;
                            nombreMenorEstado = EstadoFactura.No_Facturado.ToString();
                        }
                    }
                    else
                    {
                        if (menorEstado > 0 && menorEstado < 6)
                        {
                            parcialmenteFacturado = true;
                        }
                        menorEstado = (int)EstadoFactura.No_Facturado;
                        nombreMenorEstado = EstadoFactura.No_Facturado.ToString();
                    }
                }
                if (parcialmenteFacturado && menorEstado == (int)EstadoFactura.No_Facturado)
                {
                    nombreMenorEstado = "Parcialmente_Facturado";
                }

                List<DocumentoCompromiso> documentos = db.DocumentoCompromiso.Where(x => x.cotizacion.idCotizacion_R13 == comercializacion.cotizacion.idCotizacion_R13 && x.factura != null).ToList();

                string facturas = "";
                if (documentos.Count() == 0)
                {
                    facturas = "Sin Facturas";
                }
                foreach (DocumentoCompromiso documento in documentos)
                {
                    facturas += db.Factura.Where(x => x.idFactura == documento.factura.idFactura).Select(x => x.numero).FirstOrDefault();
                    if (documentos.Count() > 0)
                    {
                        facturas += " ";
                    }
                }

                resultset.Add(
                    new
                    {
                        comercializacion.cotizacion.codigoCotizacion,
                        facturas = facturas,
                        curso,
                        tipoEjecucion = comercializacion.cotizacion.curso != null ? comercializacion.cotizacion.curso.tipoEjecucion.ToString().Replace("_", " ") : " ",
                        cliente = comercializacion.cotizacion.cliente.nombreEmpresa,
                        fechaInicio = String.Format("{0:dd/MM/yyyy}", comercializacion.fechaInicio),
                        fechaTermino = String.Format("{0:dd/MM/yyyy}", comercializacion.fechaTermino),
                        sucursal = comercializacion.cotizacion.sucursal.nombre,
                        estadoFacturas = nombreMenorEstado,
                        montoTotal = @String.Format("{0:C}", comercializacion.valorFinal - comercializacion.descuento),

                        menu = ConvertPartialViewToString(PartialView("IndexMenu", comercializacion))
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
        // GET: Factura/Facturas/5
        public ActionResult Facturas(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewModelFacturacion facturacion = new ViewModelFacturacion();
            facturacion.comercializacion = db.Comercializacion.Find(id);
            facturacion.r11 = db.R11.Where(r => r.idCurso == facturacion.comercializacion.cotizacion.idCurso).FirstOrDefault();

            if (facturacion.comercializacion == null)
            {
                return HttpNotFound();
            }
            Contacto cliente = db.Contacto.Where(x => x.idContacto == facturacion.comercializacion.cotizacion.contacto).FirstOrDefault();

            ViewBag.Contacto = cliente;
            ViewBag.Encargado = db.Contacto.Find(facturacion.comercializacion.cotizacion.contactoEncargadoPago);

            return View(facturacion);
        }

        // GET: Factura/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Factura factura = db.Factura.Find(id);
            if (factura == null)
            {
                return HttpNotFound();
            }
            UpdateFacturaStorage(factura);
            ViewBag.files = db.FacturaStorage.Where(x => x.factura.idFactura == factura.idFactura).Where(x => x.file != null).ToList().Select(x => x.file).ToList();

            return View(factura);
        }
        // POST: Comercializacions/IngresarR24
        [Authorize]
        [CustomAuthorize(new string[] { "/Comercializacions/" })]
        public async Task<ActionResult> DeleteFacturaStorage(int idFactura, int idStorage, int idComercializacion)
        {

            if (idFactura == null || idStorage == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var factura = db.Factura.Find(idFactura);
            if (factura == null)
            {
                return HttpNotFound();
            }
            //Eliminar los viejos
            List<FacturaStorage> facturaStorage = db.FacturaStorage.Where(x => x.factura.idFactura == factura.idFactura && x.file.idStorage == idStorage).ToList();
            foreach (FacturaStorage oldfacturaStorage in facturaStorage.ToList())
            {
                await Files.BorrarArchivoAsync(oldfacturaStorage.file);
                ModelState.AddModelError("", "Se ha eliminado correctamente el fichero " + oldfacturaStorage.file.nombreArchivo);
                db.Storages.Remove(oldfacturaStorage.file);
                db.FacturaStorage.Remove(oldfacturaStorage);
                db.SaveChanges();

            }

            return RedirectToAction("Edit", new { id = factura.idFactura, id2 = idComercializacion });
        }
        // GET: Factura/Create/5
        public ActionResult Create(int? id, int? id2)
        {
            ViewBag.idComercializacion = id2;
            ViewBag.idDocumentoCompromiso = id;
            var documentoCompromiso = db.DocumentoCompromiso.Find(id);
            var factura = new Factura();
            factura.fechaFacturacion = DateTime.Now;
            factura.costo = documentoCompromiso.monto.Value;
            if (documentoCompromiso.tipoVenta.tipoPago == TipoPago.CostoEmpresa) factura.tipo = TipoFactura.Costo_Empresa;
            if (documentoCompromiso.tipoVenta.tipoPago == TipoPago.Otic) factura.tipo = TipoFactura.OTIC;
            if (documentoCompromiso.tipoVenta.tipoPago == TipoPago.Sence) factura.tipo = TipoFactura.SENCE;
            return View(factura);
        }
        public void UpdateFacturaStorage
            (Factura factura)
        {
            if (factura.archivo != null)
            {

                db.FacturaStorage.Add(new FacturaStorage
                {
                    dateUpload = DateTime.Now,
                    factura = factura,
                    file = factura.archivo,
                    userUpload = db.AspNetUsers.Find(User.Identity.GetUserId())
                });
                factura.archivo = null;
                db.Entry(factura).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
        // POST: Factura/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "idFactura,costo,numero,tipo,observacion,fechaFacturacion")] Factura factura, int idComercializacion, int idDocumentoCompromiso, List<HttpPostedFileBase> files)
        {


            files = files.Where(x => x != null).ToList();
            // validar extenciones y tamaño maximo de los archivos
            if (files != null && files.Count() <= 0)
            {
                ModelState.AddModelError("", "Se debe seleccionar un archivo.");
                //done = false;
            }

            else
            {
                // validar extenciones y tamaño maximo del archivo

                foreach (HttpPostedFileBase item in files)
                {
                    var archivoValido = Files.ArchivoValido(item, new[] { ".pdf" }, 10 * 1024);
                    if (archivoValido != "")
                    {
                        ModelState.AddModelError("", archivoValido + " ( " + item.FileName + " )");
                        //done = false;
                    }

                }
            }


            // estado
            var estado = new FacturaEstadoFactura();
            estado.estado = EstadoFactura.Facturado;
            estado.fechaCreacion = DateTime.Now;
            estado.usuarioCreador = db.AspNetUsers.Find(User.Identity.GetUserId());
            // datos creacion factura
            factura.estados = new List<FacturaEstadoFactura>();
            factura.estados.Add(estado);
            factura.fechaCreacion = DateTime.Now;
            factura.fechaUltimaModificacion = DateTime.Now;
            factura.usuarioCreador = db.AspNetUsers.Find(User.Identity.GetUserId());
            factura.usuarioUltimaModificacion = db.AspNetUsers.Find(User.Identity.GetUserId());
            factura.softDelete = false;
            //db.Factura.Add(factura);
            var documentoCompromiso = db.DocumentoCompromiso.Find(idDocumentoCompromiso);
            // validar monto igual al del doc de compromiso
            if (factura.costo != documentoCompromiso.monto)
            {
                ModelState.AddModelError("", "El monto no coincide con el documento de compromiso.");
            }
            documentoCompromiso.factura = factura;
            if (ModelState.IsValid)
            {

                //agregar los nuevos ficheros
                foreach (HttpPostedFileBase item in files)
                {
                    Storage facturaDoc = await Files.CrearArchivoAsync(item, "pagos/");
                    if (facturaDoc == null)
                    {
                        ModelState.AddModelError("", "No se pudo guardar el " + item.FileName + " archivo.");
                        //files.Remove(file);
                    }
                    else
                    {
                        db.FacturaStorage.Add(new FacturaStorage
                        {
                            dateUpload = DateTime.Now,
                            factura = factura,
                            file = facturaDoc,
                            userUpload = db.AspNetUsers.Find(User.Identity.GetUserId())
                        });
                        db.SaveChanges();
                    }


                }

            }
            if (ModelState.IsValid)
            {
                db.Entry(documentoCompromiso).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("DocumentosCompromisoComercializacion", "DocumentoCompromiso", new { id = idComercializacion });
            }
            ViewBag.idComercializacion = idComercializacion;
            ViewBag.idDocumentoCompromiso = idDocumentoCompromiso;
            return View(factura);
        }

        // GET: Factura/Pago/5
        public ActionResult Pago(int? id, int? id2)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Factura factura = db.Factura.Find(id);
            factura.valorPagado = 0;
            if (factura == null)
            {
                return HttpNotFound();
            }
            ViewBag.idComercializacion = id2;
            return View(factura);
        }

        // POST: Factura/Pago/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pago([Bind(Include = "idFactura,valorPagado,observacion")] Factura factura)
        {
            var facturaBD = db.Factura.Find(factura.idFactura);
            facturaBD.valorPagado = facturaBD.valorPagado + factura.valorPagado;
            facturaBD.observacion = factura.observacion;
            db.Entry(facturaBD).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Facturas", new { id = int.Parse(Request["idComercializacion"]) });
        }

        // POST: Factura/CambiarEstado
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult CambiarEstado(int id, int estado, DateTime? fechaEstado, string observacion)
        {
            int idFactura = id;
            var facturaBD = db.Factura.Find(idFactura);
            var estadoFactura = new FacturaEstadoFactura();
            estadoFactura.fechaCreacion = DateTime.Now;
            estadoFactura.usuarioCreador = db.AspNetUsers.Find(User.Identity.GetUserId());
            estadoFactura.estado = (EstadoFactura)estado;
            estadoFactura.fechaEstado = fechaEstado;
            estadoFactura.Observacion = observacion;
            facturaBD.estados.Add(estadoFactura);
            db.Entry(facturaBD).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new { HasErrors = false, responseText = "OK" });
        }

        // GET: Factura/Edit/5
        public ActionResult Edit(int? id, int? id2)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Factura factura = db.Factura.Find(id);
            if (factura.fechaFacturacion.Subtract(DateTime.MinValue).Days < 0)
                factura.fechaFacturacion = DateTime.Now;
            if (factura == null)
            {
                return HttpNotFound();
            }
            UpdateFacturaStorage(factura);
            ViewBag.files = db.FacturaStorage.Where(x => x.factura.idFactura == factura.idFactura).Where(x => x.file != null).ToList().Select(x => x.file).ToList();


            ViewBag.idComercializacion = id2;
            return View(factura);
        }

        // POST: Factura/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "idFactura,costo,numero,tipo,observacion,fechaFacturacion")] Factura factura, List<HttpPostedFileBase> files)
        {
            var facturaBD = db.Factura.Find(factura.idFactura);
            facturaBD.costo = factura.costo;
            facturaBD.numero = factura.numero;
            facturaBD.tipo = factura.tipo;
            facturaBD.fechaFacturacion = factura.fechaFacturacion;
            facturaBD.observacion = factura.observacion;
            facturaBD.fechaUltimaModificacion = DateTime.Now;
            facturaBD.usuarioUltimaModificacion = db.AspNetUsers.Find(User.Identity.GetUserId());
            // validar monto igual al del doc de compromiso
            var documentoCompromiso = db.DocumentoCompromiso.Where(x => x.factura.idFactura == factura.idFactura).FirstOrDefault();
            if (facturaBD.costo != documentoCompromiso.monto)
            {
                ModelState.AddModelError("", "El monto no coincide con el documento de compromiso.");
            }
            files = files.Where(x => x != null).ToList();
            var facturaStorage = db.FacturaStorage.Where(x => x.factura.idFactura == facturaBD.idFactura).Count();
            // validar extenciones y tamaño maximo de los archivos
            if (files != null && files.Count() <= 0 && facturaStorage <= 0)
            {
                ModelState.AddModelError("", "Se debe seleccionar un archivo.");
                //done = false;
            }

            else
            {
                // validar extenciones y tamaño maximo del archivo

                foreach (HttpPostedFileBase item in files)
                {
                    var archivoValido = Files.ArchivoValido(item, new[] { ".pdf" }, 10 * 1024);
                    if (archivoValido != "")
                    {
                        ModelState.AddModelError("", archivoValido + " ( " + item.FileName + " )");
                        //done = false;
                    }

                }
            }

            if (ModelState.IsValid)
            {
                foreach (HttpPostedFileBase item in files)
                {
                    Storage facturaDoc = await Files.CrearArchivoAsync(item, "pagos/");
                    if (facturaDoc == null)
                    {
                        ModelState.AddModelError("", "No se pudo guardar el " + item.FileName + " archivo.");
                        //files.Remove(file);
                    }
                    else
                    {
                        db.FacturaStorage.Add(new FacturaStorage
                        {
                            dateUpload = DateTime.Now,
                            factura = facturaBD,
                            file = facturaDoc,
                            userUpload = db.AspNetUsers.Find(User.Identity.GetUserId())
                        });
                        db.SaveChanges();
                    }


                }
            }
            if (ModelState.IsValid)
            {
                // estado
                //var estado = new FacturaEstadoFactura();
                //estado.estado = EstadoFactura.Refacturado;
                //estado.fechaCreacion = DateTime.Now;
                //estado.usuarioCreador = db.AspNetUsers.Find(User.Identity.GetUserId());
                //factura.estados.Add(estado);
                // datos creacion factura
                //agregar los nuevos ficheros


                db.Entry(facturaBD).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Facturas", new { id = int.Parse(Request["idComercializacion"]) });
            }
            ViewBag.idComercializacion = int.Parse(Request["idComercializacion"]);
            return View(facturaBD);
        }

        // GET: Factura/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Factura factura = db.Factura.Find(id);
            if (factura == null)
            {
                return HttpNotFound();
            }
            return View(factura);
        }

        // POST: Factura/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Factura factura = db.Factura.Find(id);
            if (factura == null)
            {
                ModelState.AddModelError("", "Factura no encontrada");
                return RedirectToAction("Facturas", new { id = int.Parse(Request["idComercializacion"]) });
            }
            factura.softDelete = true;
            var files = db.FacturaStorage.Where(x => x.factura.idFactura == factura.idFactura).Where(x => x.file != null).ToList();
            if (files.Count() > 0)
            {
                try
                {

                    foreach (var file in files)
                    {
                        await Files.BorrarArchivoAsync(file.file);
                        db.FacturaStorage.Remove(file);
                    }

                    db.Entry(factura).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                }
            }

            return RedirectToAction("Facturas", new { id = int.Parse(Request["idComercializacion"]) });
        }

        // GET: Factura/Descargar/5
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

        // GET: Factura/ReiniciarEstadoComercializacion/5
        public ActionResult ReiniciarEstadoComercializacion(int? id)
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
            // verificar si no existe almenos una factura
            var cont = 0;
            foreach (var documentoCompromiso in comercializacion.cotizacion.documentosCompromiso)
            {
                if (documentoCompromiso.factura != null)
                {
                    if (!documentoCompromiso.factura.softDelete)
                    {
                        cont++;
                    }
                }
            }
            if (cont == comercializacion.cotizacion.documentosCompromiso.Count())
            {
                ModelState.AddModelError("", "No se puede Reiniciar el Estado de la Comercialización si todos los Documentos de Compromiso tienen una Factura");
                var comercializaciones = db.Comercializacion
                    .Where(c => c.softDelete == false)
                    .Where(x => x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada
                        || x.comercializacionEstadoComercializacion.OrderByDescending(y => y.fechaCreacion).FirstOrDefault().EstadoComercializacion == EstadoComercializacion.Terminada_SENCE)
                    .ToList();
                return View("Facturable", comercializaciones);
            }
            // cambiar estado a en proceso
            ComercializacionEstadoComercializacion comercializacionEstadoComercializacion = new ComercializacionEstadoComercializacion();
            comercializacionEstadoComercializacion.EstadoComercializacion = EstadoComercializacion.En_Proceso;
            comercializacionEstadoComercializacion.fechaCreacion = DateTime.Now;
            comercializacionEstadoComercializacion.usuarioCreador = User.Identity.GetUserId();
            comercializacion.comercializacionEstadoComercializacion.Add(comercializacionEstadoComercializacion);
            // guardar cambios a la comercializacion
            db.Entry(comercializacion).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Facturable", "Factura");
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
