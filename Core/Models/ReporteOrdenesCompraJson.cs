using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class ReporteOrdenesCompraJson
    {
        public int consecutivo { get; set; }
        public string proyecto { get; set; }
        public string proveedor { get; set; }
        public DateTime fechaGenerado { get; set; }
        public string analistaCompras { get; set; }
        public bool urgente { get; set; }
        public string direccionFacturacion { get; set; }
        public string referencia { get; set; }
        public string descripcion { get; set; }
        public string unidad { get; set; }
        public double cantidad { get; set; }
        public string observaciones { get; set; }
        public bool rechazado { get; set; }
        public double valorUnitario { get; set; }
    }
}
