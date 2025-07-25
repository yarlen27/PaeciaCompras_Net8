using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{

    public class PedidoMaterialDetalle
    {
        public string referencia { get; set; }

        public Guid id { get; set; }

        public Guid idProyecto { get; set; }

        public Guid idCategoria { get; set; }

        public string descripcion { get; set; }
        public string unidad { get; set; }
        public double cantidad { get; set; }
        public string observaciones { get; set; }
        public bool rechazado { get; set; }
        public double valorUnitario { get; set; }
        public bool tieneOrden { get; set; }

        public string nombre { get; set; }
    }

   

}
