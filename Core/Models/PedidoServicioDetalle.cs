using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class PedidoServicioDetalle: IPedidoServicio
    {
        public string proveedor { get; set; }
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
        public List<ItemPedidoServicio> servicio { get; set; }
        public List<Poliza> poliza { get; set; }
        public string archivoCotizacion { get; set; }
        public List<DocumentoCotizacion> archivosCotizacion { get; set; }
        public string archivoContrato { get; set; }
        public string archivoContratoFirmado { get; set; }
        public List<DocumentoAdicionalContrato> otrosArchivos { get; set; }
    }



}
