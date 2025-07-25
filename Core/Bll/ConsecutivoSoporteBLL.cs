using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Bll
{
    public class ConsecutivoSoporteBLL : BaseBLLSQL<ConsecutivoDocumentoSoporte>
    {
        public ConsecutivoSoporteBLL(IConfiguration configuration) : base(configuration)
        {
        }

        internal async Task<string> ObtenerConsecutivo(Guid idFactura)
        {
            var result = new List<VwConsecutivosSoporte>();

            using (var connection = new SqlConnection(_connectionString))
            {
                string query = $"SELECT [Consecutivo],[Id],[Nit] ,[IdDocumento] FROM[dbo].[VwConsecutivosSoporte] where idDocumento = '{idFactura}'";

                var consecutivo = await connection.QueryAsync<VwConsecutivosSoporte>(query);
                result = consecutivo.ToList();
            }


            return result.FirstOrDefault().Consecutivo;
        }
    }


    public class VwConsecutivosSoporte
    {
        public string Nit { get; set; }

        public Guid IdDocumento { get; set; }

        public string Consecutivo { get; set; }
        public int Id { get; set; }
    }

    public class ConsecutivoDocumentoSoporte : BaseTable
    {
       public  string Nit { get; set; }
       public  Guid IdDocumento { get; set; }
    }
}
