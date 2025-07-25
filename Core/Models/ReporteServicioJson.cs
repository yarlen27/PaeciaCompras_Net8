using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class ReporteServicioJson
    {
        public int consecutivo { get; set; }
        public string proyecto { get; set; }
        public string proveedor { get; set; }
        public DateTime fechaGenerado { get; set; }
        public string analistaCompras { get; set; }
        public bool urgente { get; set; }
        public string direccionFacturacion { get; set; }
        public string objeto { get; set; }
        public string alcance { get; set; }
        public string plazo { get; set; }
        public double montoTotal { get; set; }
        public bool anticipo { get; set; }
        public string montoAnticipo { get; set; }
        public string porcentajeAnticipo { get; set; }
        public DateTime? fechaAnticipo { get; set; }
        public DateTime fechaInicio { get; set; }
        public string utilidad { get; set; }
        public string unidad { get; set; }
        public string tipoContrato { get; set; }
        public string formaPago { get; set; }
        public string observaciones { get; set; }
        public string actividades { get; set; }
        public string unidad2 { get; set; }
        public string cantidad { get; set; }
        public string valorUnidad { get; set; }
        public string valorTotal { get; set; }
    }
}
