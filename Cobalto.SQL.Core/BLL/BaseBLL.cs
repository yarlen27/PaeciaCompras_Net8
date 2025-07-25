using Cobalto.SQL.Core.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Cobalto.SQL.Core.BLL
{


    public class GenericBLL<T, U>
        //where U : class
        where T : GenericTable
    {


        public string _connectionString { get; set; }

        public GenericBLL(IConfiguration configuration)
        {
            this._connectionString = configuration.GetSection("ConnectionStrings")["WebApiDatabase"];

        }

        public virtual U Insertar(T entidad)
        {

            using (var connection = new SqlConnection(_connectionString))
            {

                U newId = connection.Insert<U, T>(entidad);

                return newId;
            }

        }

        public virtual IEnumerable<T> Todos()
        {
            using (var connection = new SqlConnection(_connectionString))
            {

                var entidad = connection.GetList<T>().Where(x=> x.Borrado == false);
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

        public virtual T PorId(U id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {

                var entidad = connection.Get<T>(id);
                return entidad;
            }
        }

        public virtual int? Borrar(U id)
        {

            using (var connection = new SqlConnection(_connectionString))
            {

                var entidad = connection.Get<T>(id);
                entidad.Borrado = true;
                var newId = connection.Update(entidad);
                return newId;
            }

        }

        public virtual IEnumerable<T> Filtrar(object o)
        {
            using (var connection = new SqlConnection(_connectionString))
            {

                var entidad = connection.GetList<T>(o);
                return entidad;
            }
        }

        //internal IEnumerable<T> PorIds(IEnumerable<int> ids)
        //{
        //    using (var connection = new SqlConnection(_connectionString))
        //    {


        //        var entidad = connection.GetList<T>("where id IN @ids ", new { ids });
        //        return entidad;


        //    }
        //}


        public IEnumerable<IEnumerable<T>> Split<T>(T[] array)
        {
            int size = 100;

            for (var i = 0; i < (float)array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size);
            }
        }

        internal IEnumerable<T> PorIds(IEnumerable<int> idList)
        {
            using (var connection = new SqlConnection(_connectionString))
            {

                List<T> entidad = new List<T>();
                var splittedArray = Split<int>(idList.ToArray());
                foreach (var ids in splittedArray)
                {
                    entidad.AddRange(connection.GetList<T>("where id IN @ids ", new { ids }));
                }

                return entidad;


            }
        }

    }

    public class BaseBLL<T> : GenericBLL<T, int?> where T : BaseTable
    {
        public BaseBLL(IConfiguration configuration) : base(configuration)
        {

        }

    }


    public class BaseStringBLL<T> : GenericBLL<T, string> where T : BaseStringTable
    {
        public BaseStringBLL(IConfiguration configuration) : base(configuration)
        {

        }

    }




}
