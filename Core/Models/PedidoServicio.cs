using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    [CollectionName("pedidoServicio")]
    public class PedidoServicio: CollectionDTO
    {
        public Guid proyecto { get; set; }
        public DateTime fechaSolicitado { get; set; }
        public string solicitante { get; set; }
        public bool urgente { get; set; }
        public bool rechazado { get; set; }
        public string observaciones { get; set; }
        public PedidoServicioDetalle detalle { get; set; }
        public bool ordenCompra { get; set; }


    }


    public class PedidoServicioReporte : PedidoServicio
    {
        public List<OrdenCompra> OrdenesCompra { get; set; }
    }



}
