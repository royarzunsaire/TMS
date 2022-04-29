using SGC.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace SGC.Utils
{
    public class Utils
    {
        private static readonly string email = ConfigurationManager.AppSettings["emailNoreply"];
        private static readonly string emailPassword = ConfigurationManager.AppSettings["emailPasswordNoreply"];
        public static readonly string domain = ConfigurationManager.AppSettings["domain"];
 

        public static List<Contacto> GetContactosDesocupados(InsecapContext db)
        {
            var contactosOcupados = new List<Contacto>();
            List<EncargadoPago> encargadosPago = db.EncargadoPago.Include("contacto").ToList();
            foreach (var item in encargadosPago)
            {
                contactosOcupados.Add(item.contacto);
            }
            List<RepresentanteLegal> representantesLegales = db.RepresentanteLegal.Include("contacto").ToList();
            foreach (var item in representantesLegales)
            {
                contactosOcupados.Add(item.contacto);
            }
            List<Otic> otics = db.Otic.Include("contacto").ToList();
            foreach (var item in otics)
            {
                contactosOcupados.Add(item.contacto);
            }
            List<ClienteContacto> clienteContacto = db.ClienteContacto.Include("contacto").ToList();
            foreach (var item in clienteContacto)
            {
                contactosOcupados.Add(item.contacto);
            }
            List<ClienteContactoCotizacion> clienteContactoCotizacion = db.ClienteContactoCotizacion.Include("contacto").ToList();
            foreach (var item in clienteContactoCotizacion)
            {
                contactosOcupados.Add(item.contacto);
            }
            var contactosTodos = db.Contacto
                .Where(c => c.softDelete == false)
                .Where(c => c.tipoContacto == TipoContacto.Cliente)
                .ToList();
            var contactos = new List<Contacto>();
            foreach (var item in contactosTodos)
            {
                if (!contactosOcupados.Contains(item))
                {
                    contactos.Add(item);
                }
            }
            return contactos;
        }

        public static string NumeroALetras(double value)
        {
            string num2Text; value = Math.Truncate(value);
            if (value == 0) num2Text = "CERO";
            else if (value == 1) num2Text = "UNO";
            else if (value == 2) num2Text = "DOS";
            else if (value == 3) num2Text = "TRES";
            else if (value == 4) num2Text = "CUATRO";
            else if (value == 5) num2Text = "CINCO";
            else if (value == 6) num2Text = "SEIS";
            else if (value == 7) num2Text = "SIETE";
            else if (value == 8) num2Text = "OCHO";
            else if (value == 9) num2Text = "NUEVE";
            else if (value == 10) num2Text = "DIEZ";
            else if (value == 11) num2Text = "ONCE";
            else if (value == 12) num2Text = "DOCE";
            else if (value == 13) num2Text = "TRECE";
            else if (value == 14) num2Text = "CATORCE";
            else if (value == 15) num2Text = "QUINCE";
            else if (value < 20) num2Text = "DIECI" + NumeroALetras(value - 10);
            else if (value == 20) num2Text = "VEINTE";
            else if (value < 30) num2Text = "VEINTI" + NumeroALetras(value - 20);
            else if (value == 30) num2Text = "TREINTA";
            else if (value == 40) num2Text = "CUARENTA";
            else if (value == 50) num2Text = "CINCUENTA";
            else if (value == 60) num2Text = "SESENTA";
            else if (value == 70) num2Text = "SETENTA";
            else if (value == 80) num2Text = "OCHENTA";
            else if (value == 90) num2Text = "NOVENTA";
            else if (value < 100) num2Text = NumeroALetras(Math.Truncate(value / 10) * 10) + " Y " + NumeroALetras(value % 10);
            else if (value == 100) num2Text = "CIEN";
            else if (value < 200) num2Text = "CIENTO " + NumeroALetras(value - 100);
            else if ((value == 200) || (value == 300) || (value == 400) || (value == 600) || (value == 800)) num2Text = NumeroALetras(Math.Truncate(value / 100)) + "CIENTOS";
            else if (value == 500) num2Text = "QUINIENTOS";
            else if (value == 700) num2Text = "SETECIENTOS";
            else if (value == 900) num2Text = "NOVECIENTOS";
            else if (value < 1000) num2Text = NumeroALetras(Math.Truncate(value / 100) * 100) + " " + NumeroALetras(value % 100);
            else if (value == 1000) num2Text = "MIL";
            else if (value < 2000) num2Text = "MIL " + NumeroALetras(value % 1000);
            else if (value < 1000000)
            {
                num2Text = NumeroALetras(Math.Truncate(value / 1000)) + " MIL";
                if ((value % 1000) > 0)
                {
                    num2Text = num2Text + " " + NumeroALetras(value % 1000);
                }
            }
            else if (value == 1000000)
            {
                num2Text = "UN MILLON";
            }
            else if (value < 2000000)
            {
                num2Text = "UN MILLON " + NumeroALetras(value % 1000000);
            }
            else if (value < 1000000000000)
            {
                num2Text = NumeroALetras(Math.Truncate(value / 1000000)) + " MILLONES ";
                if ((value - Math.Truncate(value / 1000000) * 1000000) > 0)
                {
                    num2Text = num2Text + " " + NumeroALetras(value - Math.Truncate(value / 1000000) * 1000000);
                }
            }
            else if (value == 1000000000000) num2Text = "UN BILLON";
            else if (value < 2000000000000) num2Text = "UN BILLON " + NumeroALetras(value - Math.Truncate(value / 1000000000000) * 1000000000000);
            else
            {
                num2Text = NumeroALetras(Math.Truncate(value / 1000000000000)) + " BILLONES";
                if ((value - Math.Truncate(value / 1000000000000) * 1000000000000) > 0)
                {
                    num2Text = num2Text + " " + NumeroALetras(value - Math.Truncate(value / 1000000000000) * 1000000000000);
                }
            }
            return num2Text;
        }

        public static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static byte[] ImageToByte2(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public static void CerrarNode()
        {
            foreach (Process proc in Process.GetProcessesByName("jsreport"))
            {
                proc.Kill();
            }
        }

        public static string SendMail(MailAddress receiverEmail, String subject, String body, MailAddress copy = null, MailPriority priority = MailPriority.Normal)
        {
            string message = "ok";
            try
            {
               

                var senderEmail = new MailAddress(email, "Insecap Capacitación");
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = true,
                    
                    //Credentials = new NetworkCredential("contacto@insecap.email", "cobremrsh")
                    Credentials = new NetworkCredential(email, emailPassword)
                };
                using (var mess = new MailMessage(senderEmail, receiverEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                    ,Priority = priority
                })
                {
                    if(copy != null)
                        mess.CC.Add(copy);
                    smtp.Send(mess);
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            return message;
        }
    }
}