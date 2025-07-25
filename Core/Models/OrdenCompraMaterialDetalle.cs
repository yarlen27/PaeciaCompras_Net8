using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{

    public class OrdenCompraMaterialDetalle
    {
        public Guid proveedor { get; set; }
        public List<PedidoMaterialDetalle> detalleMaterial { get; set; }
    }
}
