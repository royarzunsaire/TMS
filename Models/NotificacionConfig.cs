using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class NotificacionConfig
    {
        [Key]
        public int idNotificacionConfig { get; set; }

        public string nombre { get; set; }

        public string titulo { get; set; }

        public string mensaje { get; set; }

        public string url { get; set; }

        public string tipo { get; set; }

        public ColorNotificacion color { get; set; }

        public virtual List<AspNetRoles> roles { get; set; }

        internal void CrearNotificacion(InsecapContext db, string mensaje, string url, string idUser)
        {
            foreach (var rol in this.roles)
            {
                foreach (var user in rol.AspNetUsers)
                {
                    Notificacion notificacion = new Notificacion(
                        this.titulo,
                        string.Format(this.mensaje, mensaje),
                        string.Format(this.url, url),
                        this.tipo,
                        this.color,
                        idUser,
                        user
                        );
                    db.Notificacion.Add(notificacion);
                    db.SaveChanges();
                }
            }
        }

        internal void CrearNotificacionUsuario(InsecapContext db, string mensaje, string url, string idUser, AspNetUsers user)
        {
            Notificacion notificacion = new Notificacion(
                this.titulo,
                string.Format(this.mensaje, mensaje),
                string.Format(this.url, url),
                this.tipo,
                this.color,
                idUser,
                user
                );
            db.Notificacion.Add(notificacion);
            db.SaveChanges();
        }
    }
}