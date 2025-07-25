using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    [CollectionName("ordenCompra")]
    public class OrdenCompra : CollectionDTO
    {
        public Guid proyecto { get; set; }
        public Guid proveedor { get; set; }
        public Guid idPedido { get; set; }
        public DateTime fechaGenerado { get; set; }
        public string analistaCompras { get; set; }
        public bool urgente { get; set; }
        public bool pedidoMaterial { get; set; }
        public List<OrdenCompraMaterialDetalle> detalle { get; set; }
        public PedidoServicioDetalle servicio { get; set; }
        public string direccionFacturacion { get; set; }

        public string direccionEntrega { get; set; }

        public bool contratoFirmado { get; set; }
        public int consecutivo { get; set; }

        public bool pagada { get; set; }
        public string estadoFacturas { get; set; }
        public string contacto { get; set; }
        public bool pendiente { get; set; }
    }
}
