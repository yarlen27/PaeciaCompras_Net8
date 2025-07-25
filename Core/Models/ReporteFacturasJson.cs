using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class ReporteFacturasJson
    {
        public string numeroFactura { get; set; }
        public DateTime? fecha { get; set; }
        public DateTime? fechaCreado { get; set; }
        public DateTime? fechaVencimiento { get; set; }
        public double valor { get; set; }
        public bool aprobada { get; set; }
        public bool rechazada { get; set; }
        public string aprobador { get; set; }
        public string observacionRechazo { get; set; }
        public string Proyecto { get; set; }

        public string Nit { get; set; }

        public string Proveedor { get; set; }
        public string NitProveedor { get; set; }

        

        public int ordenDeCompra { get; set; }
        public DateTime? fechaAprobacion { get; set; }
        public string ObservacionesAprobacion { get; set; }
        public string ObservacionesRechazo { get; set; }
        public DateTime? FechaPagado { get; set; }
        public DateTime? FechaDatosContables { get; set; }
        public DateTime? FechaImpresoContabilidad { get; set; }
        public DateTime? FechaImpresoTesoreria { get; set; }
        public DateTime? FechaSegundaAprobacion { get; set; }
        public string AprobadorMantenimiento { get; set; }
        public DateTime? FechaAprobacionMantenimiento { get; set; }
    }
}
