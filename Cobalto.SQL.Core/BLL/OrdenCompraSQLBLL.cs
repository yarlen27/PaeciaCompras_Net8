using Cobalto.SQL.Core.Models;
using Core.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace Cobalto.SQL.Core.BLL
{
    public class OrdenCompraSQLBLL : BaseBLL<OrdenCompraSQL>
    {
        private readonly ItemOrdenCompraBLL itemOrdenCompraBLL;

        public OrdenCompraSQLBLL(IConfiguration configuration, ItemOrdenCompraBLL itemOrdenCompraBLL) : base(configuration)
        {

            this.itemOrdenCompraBLL = itemOrdenCompraBLL;
        }

        internal OrdenCompraSQL PorIdCompras(Guid ocId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {


                var ordenesDeCompra = connection.GetList<OrdenCompraSQL>("where IdCompras = @ocId", new { ocId });


                return ordenesDeCompra.FirstOrDefault();
            }
        }

        internal int? Insertar(OrdenCompra ocMOngo, string usuario)
        {

            OrdenCompraSQL orden = OrdenCompraSQL.FromMongo(ocMOngo);

            orden.Usuario = usuario;

            var id = this.Insertar(orden);


            this.itemOrdenCompraBLL.Insertar(ocMOngo, id.Value);

            return id;

        }

       
    }



}
