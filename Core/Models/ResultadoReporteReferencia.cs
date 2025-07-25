using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class ResultadoReporteReferencia
    {

        public string referencia { get; set; }
        public string nombreReferencia { get; set; }
        public string categoria { get; set; }

        public string proyecto { get; set; }
        public double cantidad { get; set; }
        public string unidad { get; set; }
        public double valorUnitario { get; set; }
        public string proveedor { get; set; }
        public double total { get; set; }
        public Guid idProyecto { get; set; }
    }
}
