using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Dapper;
using Cobalto.SQL.Core.Models;

namespace Cobalto.SQL.Core.BLL
{
    public class ItemEvaluacionBLL : BaseBLL<ItemEvaluacion>
    {
        public ItemEvaluacionBLL(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<ItemEvaluacion>> ObtenerItemsEvaluacionLikerPorId(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var algo = await connection.QueryAsync<ItemEvaluacion>($"SELECT * FROM ItemEvaluacion WHERE IdEvaluacion = {id} AND EsSiNo = 0");
                return algo.ToList();
            }
        }


    }
}