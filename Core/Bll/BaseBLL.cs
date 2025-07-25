using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Core.Bll
{
    public class BaseBLLSQL<T> where T : BaseTable
    {

        public string _connectionString { get; set; }

        public BaseBLLSQL(IConfiguration configuration)
        {
            this._connectionString = configuration.GetSection("ConnectionStrings")["WebApiDatabase"];

        }

        public virtual int? Insertar(T entidad)
        {

            using (var connection = new SqlConnection(_connectionString))
            {

                var newId = connection.Insert(entidad);

                return newId;
            }

        }

        public virtual IEnumerable<T> Todos()
        {
            using (var connection = new SqlConnection(_connectionString))
            {

                var entidad = connection.GetList<T>();
                return entidad;
            }
        }

        public virtual void Actualizar(T value)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var newId = connection.Update(value);
            }
        }

        public virtual T PorId(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {

                var entidad = connection.Get<T>(id);
                return entidad;
            }
        }

    }


    public class BaseTable
    {

        public int Id { get; set; }
    }
}
