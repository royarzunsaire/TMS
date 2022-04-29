using System.Collections.Generic;

namespace SGC.Models
{
    public class ViewModelCalendario
    {
        public Calendarizacion calendarizacion { get; set; }
        public CalendarizacionAbierta calendarizacionAbierta { get; set; }
        public List<Calendarizacion> calendarizaciones { get; set; }
        public string eventosJson { get; set; }
    }
}