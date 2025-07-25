using Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cobalto.SQL.Core.Models
{

    [Table("OrdenCompra")]
    public class OrdenCompraSQL : BaseTable
    {


        public Guid IdCompras { get; set; }
        public Guid IdProyecto { get; set; }
        public Guid IdPedido { get; set; }
        public DateTime FechaGenerado { get; set; }
        public string IdAnalistaCompras { get; set; }
        public string DireccionFacturacion { get; set; }
        public int Consecutivo { get; set; }
        public Guid IdProveedor { get; set; }
        public string Usuario { get; set; }
        public DateTime Fecha { get; set; }

        internal static OrdenCompraSQL FromMongo(OrdenCompra ocMongo)
        {


            var result = new OrdenCompraSQL();


            result.IdCompras = ocMongo.id;
            result.IdProyecto = ocMongo.proyecto;
            result.IdPedido = ocMongo.idPedido;
            result.FechaGenerado = ocMongo.fechaGenerado;
            result.IdAnalistaCompras = ocMongo.analistaCompras;
            result.DireccionFacturacion = ocMongo.direccionFacturacion;
            result.Consecutivo = ocMongo.consecutivo;
            result.IdProveedor = ocMongo.proveedor;
            result.Fecha = DateTime.Now;


            return result;
        }
    }


    public class OrdenCompraSQLDTO: OrdenCompraSQL
    {
        public List<ItemOrdenCompra> Items { get; set; }
    }

}
