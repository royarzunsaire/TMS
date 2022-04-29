using SGC.Models;
using SGC.Utils;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SGC.Controllers
{
    public class ValidarCredencialesController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: ValidarCredenciales
        public ActionResult Index()
        {
            return View();
        }

        // GET: ValidarCredenciales/Validar
        public async System.Threading.Tasks.Task<ActionResult> Validar(string id, string id2)
        {

            if (id == null || id2 == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            id = id.Split('-')[0];
            //var participante = db.Participante
            // .Where(x => x.comercializacion.cotizacion.codigoCotizacion.Contains(id))
            // .Where(x => x.contacto.run == id2)
            // .FirstOrDefault();

            var comercializacion = db.Comercializacion
                 .Where(x => x.cotizacion.codigoCotizacion.Contains(id)).FirstOrDefault();
            Participante participante = null;
            if (comercializacion != null)
            {
                participante = comercializacion.participantes.Where(x => x.contacto.runCompleto == id2).FirstOrDefault();
            }
            if (participante == null)
            {
                ModelState.AddModelError("", "Participante no encontrado.");
            }
            else if (comercializacion.cotizacion.cliente.situacionComercial == SituacionComercial.Pendiente)
            {
                ModelState.AddModelError("", "El Cliente " + comercializacion.cotizacion.cliente.nombreEmpresa + " tiene el estado comercial Pendiente por tanto sus certificados quedaron deshabilitados temporalmente");
            }
            double nota = 0;
            int numero = 0;
            var notaTeorica = 0.0;
            var contTeorica = 0;
            bool aprobado = false;
            var notaPractica = 0.0;
            var contPractica = 0;
            if (participante != null)
            {
                foreach (var evaluacion in participante.comercializacion.evaluaciones)
                {
                    if (evaluacion.categoria == CategoriaEvaluacion.Teorico)
                    {
                        if (participante.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault() != null)
                        {
                            if (participante.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != ""
                                && participante.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != "-")
                            {
                                notaTeorica += double.Parse(participante.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota);
                            }

                        }
                        contTeorica++;
                    }
                }
                if (contTeorica > 0)
                {
                    notaTeorica = notaTeorica / contTeorica;
                   
                }
               
                foreach (var evaluacion in participante.comercializacion.evaluaciones)
                {
                    if (evaluacion.categoria == CategoriaEvaluacion.Practico)
                    {
                        if (participante.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault() != null)
                        {
                            if (participante.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != ""
                                && participante.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota != "-")
                            {
                                notaPractica += double.Parse(participante.notas.Where(n => n.evaluacion.idEvaluacion == evaluacion.idEvaluacion).FirstOrDefault().nota);
                            }

                        }
                        contPractica++;
                    }
                }
                if (contPractica > 0)
                {
                    notaPractica = notaPractica / contPractica;
                   
                }

            }
            int cont = 0;
            if (contPractica > 0)
            {
                cont++;
            }
            if (contTeorica > 0)
            {
                cont++;
            }

            nota  = (notaPractica + notaTeorica) / cont;
            aprobado = nota >= 5.0;
            if (participante != null && aprobado )
            {
                Files.borrarArchivosLocales();
                await Files.BajarArchivoADirectorioLocalAsync(participante.credenciales);
                return View(participante);
            }

           
            else if (!aprobado)
            {
                ModelState.AddModelError("", "El alumno está reprobado");
            }
           
            //  else if (participante.comercializacion.comercializacionEstadoComercializacion.LastOrDefault().EstadoComercializacion != EstadoComercializacion.Terminada_SENCE
            //|| participante.comercializacion.comercializacionEstadoComercializacion.LastOrDefault().EstadoComercializacion != EstadoComercializacion.Terminada)
            //  {
            //      ModelState.AddModelError("", "La comercialización ingresada no se encuentra en estado terminada.");
            //  }

            else
            {
                ModelState.AddModelError("", "No se encontraron las credenciales ingresadas.");
            }

            participante = new Participante();
            participante.contacto = new Contacto();
            participante.contacto.run = id2;
            participante.comercializacion = new Comercializacion();
            participante.comercializacion.cotizacion = new Cotizacion_R13();
            participante.comercializacion.cotizacion.codigoCotizacion = id;
            return View("Index", participante);
        }
    }
}