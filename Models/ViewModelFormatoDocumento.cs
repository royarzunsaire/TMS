using System;
using System.Collections.Generic;

namespace SGC.Models
{
    public class ViewModelFormatoDocumento
    {
        public FormatoDocumento formatoDocumento { get; set; }
        public IEnumerable<String> tiposArchivos { get; set; }

    }
}