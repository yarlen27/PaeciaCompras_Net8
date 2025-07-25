using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    [CollectionName("pedidoMaterial")]
    public class PedidoMaterial : CollectionDTO
    {
        public Guid proyecto { get; set; }
        public DateTime fechaSolicitado { get; set; }
        public string solicitante { get; set; }
        public bool urgente { get; set; }
        public List<PedidoMaterialDetalle> detalle { get; set; }
        public bool ordenCompra { get; set; }
        public string aprobador { get; set; }
        public DateTime? fechaAprobado { get; set; }
        public bool temporal { get; set; }
    }


    public class PedidoMaterialReporte : PedidoMaterial
    {
        public List<OrdenCompra> OrdenesCompra { get; set; }
    }

  

    
}
