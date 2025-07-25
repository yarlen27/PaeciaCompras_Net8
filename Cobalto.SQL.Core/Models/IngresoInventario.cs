using System;

namespace Cobalto.SQL.Core.Models
{
    public class IngresoInventario
    {

        public int IdItemCompra { get; set; }
        public int? IdOrden { get; set; }

        public double Cantidad { get; set; }

        public string Usuario { get; set; }

        public int? IdFrente { get; set; }
        public string Observaciones { get;  set; }

        public Guid? IdProyecto { get; set; }
        public string NumeroPedido { get; set; }

    }

}
