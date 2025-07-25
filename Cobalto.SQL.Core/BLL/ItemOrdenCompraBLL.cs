using Cobalto.SQL.Core.Models;
using Core.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace Cobalto.SQL.Core.BLL
{
    public class ItemOrdenCompraBLL : BaseBLL<ItemOrdenCompra>
    {
        private MaterialBLL materialBLL;

        public ItemOrdenCompraBLL(IConfiguration configuration, MaterialBLL materialBLL) : base(configuration)
        {

            this.materialBLL = materialBLL;
        }

        internal void Insertar(OrdenCompra ocMOngo, int idOrden)
        {
            foreach (var item in ocMOngo.detalle.FirstOrDefault().detalleMaterial)
            {
                var itemOc = ItemOrdenCompra.FromMongo(item);

                itemOc.IdMaterial = ObtenerIdMaterial(item, ocMOngo.proyecto);

                itemOc.IdOrden = idOrden;

                if (item.rechazado == false)
                {
                    this.Insertar(itemOc);

                }

            }
        }

        private int ObtenerIdMaterial(PedidoMaterialDetalle item, Guid idProyecto)
        {
            Material material = this.materialBLL.PorIdInsumoYProyecto(item.id, idProyecto);


            if (material == null)
            {

                material = new Material()
                {
                    IdInsumo = item.id,
                    IdProyecto = idProyecto,
                    IdCategoria = item.idCategoria,
                    Referencia = item.referencia,
                    Nombre = item.nombre,
                    Descripcion = item.descripcion
                };

                return materialBLL.Insertar(material).Value;
            }


            return material.Id;

        }

        internal List<ItemOrdenCompra> PorOrden(int ocId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {


                var itemsOrdeneCompra = connection.GetList<ItemOrdenCompra>("where IdOrden = @ocId", new { ocId });


                return itemsOrdeneCompra.ToList();
            }
        }
    }


    public class MaterialBLL : BaseBLL<Material>
    {
        public MaterialBLL(IConfiguration configuration) : base(configuration)
        {
        }

        internal Material PorIdInsumoYProyecto(Guid idInsumo, Guid idProyecto)
        {
            using (var connection = new SqlConnection(_connectionString))
            {


                var material = connection.GetList<Material>("where IdInsumo = @idInsumo and IdProyecto = @idProyecto", new { idInsumo, idProyecto });


                return material.FirstOrDefault();
            }
        }
    }

}
