using Dapper;
using System;

namespace Cobalto.SQL.Core.Models
{
    public class MovimientoInventario : BaseTable
    {


        public int? IdOrden { get; set; }
        public int IdMaterial { get; set; }

        public Guid IdInsumo { get; set; }
        public string Referencia { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public double CantidadOriginal { get; set; }
        public double CantidadMovimiento { get; set; }
        public string Observaciones { get; set; }
        public double ValorUnitario { get; set; }
        public double Ingreso { get; set; }
        public string TipoMovimiento { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
        public int? IdFrente { get; set; }
        public int? IdResponsable { get; set; }


        public Guid IdProyecto { get; set; }
        public string NumeroPedido { get; set; }
        public Guid IdProveedor { get; set; }
        public bool Aprobado { get; set; }
        public string Causa { get; set; }


        public string Remision { get; set; }
        public int IdItemContrato { get; set; }

        [NotMapped]
        public string Proveedor { get; set; }

        [NotMapped]
        public string numeroFactura { get; set; }

        [NotMapped]
        public DateTime? FechaFactura { get; set; }

        [NotMapped]
        public string OrdenCompra { get; set; }

        [NotMapped]
        public string NumeroRemision { get; set; }

        [NotMapped]
        public double?  Iva { get; set; }


        [NotMapped]
        public double? ValorTotal { get; set; }
        [NotMapped]
        public double? ValorTotalConIva { get; set; }

        [NotMapped]

        public string CodigoItemObra { get;  set; }
        [NotMapped]

        public string ItemContrato { get;  set; }

    }

}
