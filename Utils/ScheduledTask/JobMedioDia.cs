using Microsoft.AspNet.Identity;
using Quartz;
using SGC.Controllers;
using SGC.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SGC.Utils.ScheduledTask
{
    public class JobMedioDia : IJob
    {
        private static readonly string email = ConfigurationManager.AppSettings["emailNoreply"];
        private static readonly string emailPassword = ConfigurationManager.AppSettings["emailPasswordNoreply"];
        private static readonly string domain = ConfigurationManager.AppSettings["domain"];
        private InsecapContext db = new InsecapContext();

        public Task Execute(IJobExecutionContext context)
        {
            var task = Task.Run(() =>
            {
                AlertaConfirmacionCursoRelator();
            });

            return task;
        }

        // alerta confirmacion curso relator 2 dias antes de realizacion de comercializacion
        private void AlertaConfirmacionCursoRelator()
        {
            var pasadoMañana = DateTime.Now.Date.AddDays(2);
            var mañana = DateTime.Now.Date.AddDays(1);
            var comercializaciones = db.Comercializacion
                .Where(x => x.softDelete == false)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) <= pasadoMañana)
                .Where(x => DbFunctions.TruncateTime(x.fechaInicio) >= mañana)
                .Where(x => x.cotizacion.curso != null)
                .Where(x => x.relatoresCursos.Count() != 0)
                .Where(x => x.relatoresCursos.Count() > x.relatoresConfirmados.Count())
                .ToList();
            foreach (var comercializacion in comercializaciones)
            {
                foreach (var relator in comercializacion.relatoresCursos)
                {
                    // notificacion relator confirmacion curso
                    //var notificacionConfig = db.NotificacionConfig.Where(x => x.nombre == "Alerta de Confirmacion de Curso Relator").FirstOrDefault();
                    //if (notificacionConfig != null)
                    //{
                    //    notificacionConfig.CrearNotificacionUsuario(db, comercializacion.cotizacion.curso.nombreCurso, comercializacion.idComercializacion + "-" + relator.relator.idRelator, relator.relator.contacto.usuario.Id, relator.relator.contacto.usuario);
                    //}
                    EnviarMailConfirmacinCursoRelator(comercializacion, relator);
                }
            }
        }
       

        private void EnviarMailConfirmacinCursoRelator(Comercializacion comercializacion, RelatorCurso relator)
        {
            var senderEmail = new MailAddress(email, "Insecap");
            var receiverEmail = new MailAddress(relator.relator.contacto.correo, relator.relator.contacto.nombreCompleto);
            var copy = new MailAddress("insecap@gmail.com", "Insecap");

            var password = emailPassword;
            var subject = "CONFIRMACIÓN CURSO // {0} // INSECAP {1}";
            subject = String.Format(subject, comercializacion.cotizacion.curso.nombreCurso.ToUpper(), comercializacion.cotizacion.codigoCotizacion);
            var textoEmail = "Estimado/a {1},{0}{0}Favor confirmar que realizará el curso {2}, codigo {8} modalidad {7} para la empresa {4} con {5} participantes en las fechas: {0} {6} {0} , ingrese aquí {3}.{0}{0}Atte.{0}{0}Insecap";
            //var configEmail = db.TextoEmail.Where(x => x.email == "Confirmación Curso Relator").FirstOrDefault();
            String bloqueString = "Por definir";
            string tipoEjecucion = comercializacion.cotizacion.curso != null ? comercializacion.cotizacion.curso.tipoEjecucion.ToString() : " ";
            if (tipoEjecucion.ToString().Contains("Presencial") || tipoEjecucion.ToString().Contains("Recertificacion") || tipoEjecucion.ToString().Contains("Recertificacion_Sincronica") || tipoEjecucion.ToString().Contains("Elearning_Sincrono"))
            {
                bloqueString = "No hay bloques el día " + String.Format("{0:dddd d , MMMM , yyyy}", comercializacion.fechaInicio).Replace(",", "de");
                int bloqueCont = comercializacion.bloques.ToList().Count();
                if (bloqueCont > 0)
                {
                    bloqueString = "";
                    String dateString = "";
                    foreach (Bloque bloque in comercializacion.bloques)
                    {
                        String currentDate = String.Format("{0:dddd d , MMMM , yyyy}", bloque.fecha.Date).Replace(",", "de");
                        if (!dateString.Equals(currentDate))
                        {
                            bloqueString += "Bloques del día " + currentDate + ": ";
                            bloqueString += bloque.horarioInicio.ToString("HH:mm") + " - " + bloque.horarioTermino.ToString("HH:mm") + "";
                        }
                        else
                        {
                            bloqueString += bloque.horarioInicio.ToString("HH:mm") + " - " + bloque.horarioTermino.ToString("HH:mm") + "";
                        }
                        dateString = currentDate;

                    }
                }
            }

            //if (configEmail != null)
            //{
            //    subject = configEmail.motivo;
            //    textoEmail = configEmail.texto;
            //}
            var body = string.Format(textoEmail,
                Environment.NewLine,
                relator.relator.contacto.nombreCompleto,
                comercializacion.cotizacion.curso.nombreCurso,
                string.Format("{0}/Comercializacions/ConfirmarCurso/{1}-{2}",
                    domain, comercializacion.idComercializacion, relator.relator.idRelator)
                , comercializacion.cotizacion.cliente.nombreEmpresa,
                comercializacion.cotizacion.cantidadParticipante,
                bloqueString,
                tipoEjecucion.ToString(),
                comercializacion.cotizacion.codigoCotizacion
                );
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail.Address, password)
            };


            using (var mess = new MailMessage(senderEmail, receiverEmail)
            {
                Subject = subject,
                Body = body,
            }

            )
            {
                mess.CC.Add(copy);
                smtp.Send(mess);
            }

        }
    }
}