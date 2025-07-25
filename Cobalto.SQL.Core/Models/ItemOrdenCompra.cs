using Core.Models;
using System;

namespace Cobalto.SQL.Core.Models
{



    public class ItemOrdenCompra : BaseTable
    {

        public Guid IdInsumo { get; set; }

        
        public int IdMaterial { get; set; }
        public int IdOrden { get; set; }
        public string Referencia { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public double Cantidad { get; set; }
        public string Observaciones { get; set; }

        public double ValorUnitario { get; set; }
        public Guid IdCategoria { get; private set; }

        internal static ItemOrdenCompra FromMongo(PedidoMaterialDetalle item)
        {
            var result = new ItemOrdenCompra();



            result.IdInsumo = item.id;
            result.Referencia = item.referencia;
            result.Nombre = item.nombre;
            result.Descripcion = item.descripcion;
            result.Unidad = item.unidad;
            result.Cantidad = item.cantidad;
            result.Observaciones = item.observaciones;
            result.ValorUnitario = item.valorUnitario;
            result.IdCategoria = item.idCategoria;


            return result;
        }




    }


}


