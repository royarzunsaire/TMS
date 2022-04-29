using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Notificacion
    {
        [Key]
        public int idNotificacion { get; set; }

        public string titulo { get; set; }
        public string mensaje { get; set; }
        public string url { get; set; }
        public string tipo { get; set; }
        public ColorNotificacion color { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime fechaCreacion { get; set; }
        public string usuarioCreador { get; set; }

        public virtual ICollection<EstadoNotificacion> estado { get; set; }
        public virtual AspNetUsers usuario { get; set; }


        public Notificacion()
        {

        }


        public Notificacion(string titulo, string mensaje, string url, string tipo, ColorNotificacion color, string usuarioCreador, AspNetUsers usuario)
        {
            this.titulo = titulo;
            this.mensaje = mensaje;
            this.url = url;
            this.tipo = tipo;
            this.color = color;
            EstadoNotificacion estado = new EstadoNotificacion();
            estado.nombre = NombreEstadoNotificacion.Enviado;
            estado.fecha = DateTime.Now;
            this.estado = new List<EstadoNotificacion>();
            this.estado.Add(estado);
            this.fechaCreacion = DateTime.Now;
            this.usuarioCreador = usuarioCreador;
            this.usuario = usuario;
        }
    }

    public enum ColorNotificacion
    {
        [Display(Name = "Blanco")] white,
        [Display(Name = "Azul")] blue,
        [Display(Name = "Rojo")] red,
        [Display(Name = "Naranjo")] orange,
        [Display(Name = "Verde")] green
    }
}