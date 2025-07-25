using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    [CollectionName("pedidoAdicionServicio")]
    public class AdicionPedidoServicio : CollectionDTO, IPedidoServicio, IPolizas
    {

        public Guid idOrden { get; set; }
        public List<Poliza> poliza { get; set; }
        public string archivoCotizacion { get; set; }
        public string archivoContrato { get; set; }
        public string archivoContratoFirmado { get; set; }
        public List<string> otrosArchivos { get; set; }
        public string alcance { get; set; }
        public string plazo { get; set; }
        public double montoTotal { get; set; }

        public DateTime? fechaCreacion { get; set; }
    }
}
