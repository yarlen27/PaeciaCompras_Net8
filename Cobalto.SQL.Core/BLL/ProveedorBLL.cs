using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cobalto.SQL.Core.Models;
using Core.BLL;
using Microsoft.Data.SqlClient;
using Dapper;
using Core.Models;

namespace Cobalto.SQL.Core.BLL
{
    public class ProveedorSQLBLL : BaseBLL<ProveedorSQL>
    {
        private ProveedorBLL proveedorMongoBLL;
        private ProveedorXProyectoBLL _proveedorXProyectoBLL;
        private OrdenCompraBLL ocBLL;

        public ProveedorSQLBLL(IConfiguration configuration, ProveedorBLL proveedorMongoBLL, OrdenCompraBLL ocBLL, ProveedorXProyectoBLL proveedorXProyectoBLL) : base(configuration)
        {
            this.proveedorMongoBLL = proveedorMongoBLL;
            this._proveedorXProyectoBLL = proveedorXProyectoBLL;
            this.ocBLL = ocBLL;
        }

        public int Sincronizar(List<Guid> idsProyectos)
        {

            if (idsProyectos.Any())
            {
                foreach (var item in idsProyectos)
                {
                    _ = this.SincronizarProveedoresProyectoAsync(item);
                }
            }
            else
            {
                _ = this.SincronizarTodos();

            }

            return 0;
        }

        public async void SincronizarProveedor(Guid id)
        {

            var proveedorMongo = await this.proveedorMongoBLL.GetById(id);

            if (proveedorMongo != null)
            {
                var proveedorSql = new ProveedorSQL
                {
                    IdCompras = proveedorMongo.id,
                    Nombre = proveedorMongo.nombre,
                    Nit = proveedorMongo.nit,
                    Correo = proveedorMongo?.email ?? "",
                    userId = proveedorMongo?.userid ?? "",
                    UserName = proveedorMongo?.UserName ?? ""
                };
                proveedorSql.Id = (this.Insertar(proveedorSql)).Value;

            }

        }

        public  async Task<List<ProveedorSQL>> PorProyecto(Guid idProyecto)
        {
            var proveedores = await this.ocBLL.ProveedoresXProyecto(idProyecto);
            var proveedoresSQL = PorIdsCompras(proveedores);

            return  await proveedoresSQL;


        }

        internal async  Task<List<ProveedorSQL>> PorIdsCompras(IEnumerable<Guid> idList)
        {


            var proveedoresMongo = await this.proveedorMongoBLL.PorLista(idList);
            using (var connection = new SqlConnection(_connectionString))
            {

                List<ProveedorSQL> entidad = new List<ProveedorSQL>();
                var splittedArray = Split<Guid>(idList.ToArray());
                foreach (var ids in splittedArray)
                {
                    entidad.AddRange(connection.GetList<ProveedorSQL>("where IdCompras IN @ids ", new { ids }));
                }

                foreach (var item in entidad)
                {
                    item.Nombre = proveedoresMongo.FirstOrDefault(x => x.id == item.IdCompras).nombre;
                }

                return entidad;


            }
        }




        private async Task<Task> SincronizarTodos()
        {
            var todasOrdenesCompra = await this.ocBLL.GetAll();
            var gruposXProyectos = todasOrdenesCompra.Select(oc => new { Proyecto = oc.proyecto, Proveedor = oc.proveedor }).Distinct().GroupBy(a => a.Proyecto);

            foreach (var grupoProveedores in gruposXProyectos)
            {
                foreach (var proveedor in grupoProveedores)
                {
                    try
                    {
                        using (var conexion = new SqlConnection(this._connectionString))
                        {
                            conexion.Open();
                            var query = "SELECT * FROM Proveedor WHERE IdCompras = @IdCompras";
                            var proveedorSql = (await conexion.QueryAsync<ProveedorSQL>(query, new { IdCompras = proveedor.Proveedor }))?.FirstOrDefault();

                            if (proveedorSql == null)
                            {
                                var proveedorMongo = await this.proveedorMongoBLL.GetById(proveedor.Proveedor);

                                if (proveedorMongo != null)
                                {
                                    proveedorSql = new ProveedorSQL
                                    {
                                        IdCompras = proveedorMongo.id,
                                        Nombre = proveedorMongo.nombre,
                                        Nit = proveedorMongo.nit,
                                        Correo = proveedorMongo?.email ?? ""
                                    };
                                    proveedorSql.Id = (this.Insertar(proveedorSql)).Value;

                                }


                            }

                            var queryPP = "SELECT * FROM ProveedorXProyecto WHERE IdProyectoCompras = @IdProyectoCompras AND IdProveedorCompras = @IdProveedorCompras";
                            var proveedorProyectoSql = (await conexion.QueryAsync<ProveedorXProyecto>(queryPP, new { IdProyectoCompras = proveedor.Proyecto, IdProveedorCompras = proveedor.Proveedor })).FirstOrDefault();

                            if (proveedorProyectoSql == null && proveedorSql != null)
                            {
                                var nuevoProveedorProyectoSql = new ProveedorXProyecto
                                {
                                    IdProveedor = proveedorSql.Id,
                                    IdProveedorCompras = proveedor.Proveedor,
                                    IdProyectoCompras = proveedor.Proyecto,

                                };

                                var idSql = this._proveedorXProyectoBLL.Insertar(nuevoProveedorProyectoSql);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }



            return Task.CompletedTask;

        }

        private async Task<Task> SincronizarProveedoresProyectoAsync(Guid item)
        {
            var proveedores = await this.ocBLL.ProveedoresXProyecto(item);

            foreach (var id in proveedores)
            {
                try
                {
                    using (var conexion = new SqlConnection(this._connectionString))
                    {
                        conexion.Open();
                        var query = "SELECT * FROM Proveedor WHERE IdCompras = @IdCompras";
                        var proveedorSql = (await conexion.QueryAsync<ProveedorSQL>(query, new { IdCompras = id }))?.FirstOrDefault();

                        if (proveedorSql == null)
                        {
                            var proveedorMongo = await this.proveedorMongoBLL.GetById(id);

                            if (proveedorMongo != null)
                            {
                                proveedorSql = new ProveedorSQL
                                {
                                    IdCompras = proveedorMongo.id,
                                    Nombre = proveedorMongo.nombre,
                                    Nit = proveedorMongo.nit,
                                    Correo = proveedorMongo?.email ?? ""
                                };
                                proveedorSql.Id = (this.Insertar(proveedorSql)).Value;

                            }


                        }

                        var queryPP = "SELECT * FROM ProveedorXProyecto WHERE IdProyectoCompras = @IdProyectoCompras AND IdProveedorCompras = @IdProveedorCompras";
                        var proveedorProyectoSql = (await conexion.QueryAsync<ProveedorXProyecto>(queryPP, new { IdProyectoCompras = item, IdProveedorCompras = id })).FirstOrDefault();

                        if (proveedorProyectoSql == null && proveedorSql != null)
                        {
                            var nuevoProveedorProyectoSql = new ProveedorXProyecto
                            {
                                IdProveedor = proveedorSql.Id,
                                IdProveedorCompras = id,
                                IdProyectoCompras = item,

                            };

                            var idSql = this._proveedorXProyectoBLL.Insertar(nuevoProveedorProyectoSql);
                        }
                    }
                }
                catch (Exception ex)
                {
                }

            }
            return Task.CompletedTask;
        }

        public async Task<ProveedorSQL> PorId(Guid idProveedor)
        {
            var proveedor = this.Filtrar(new { IdCompras  = idProveedor}).FirstOrDefault();

            if (proveedor == null)
            {
                var proveedorMongo = await this.proveedorMongoBLL.GetById(idProveedor);

                if (proveedorMongo != null)
                {
                    proveedor = new ProveedorSQL()
                    {
                        Borrado = false,
                        IdCompras = idProveedor,
                        Nit = proveedorMongo.nit,
                        Correo = proveedorMongo.email,
                        Nombre = proveedorMongo.nombre,
                        IdTipologia = null
                    };

                    proveedor.Id = this.Insertar(proveedor).Value;
                }
            }


            return proveedor;
        }

        public override ProveedorSQL PorId(int? id)
        {
            var proveedor = base.PorId(id);
            var proveedorMongo = this.proveedorMongoBLL.GetById(proveedor.IdCompras).Result;
            proveedor.Nit = proveedorMongo.nit;
            return proveedor;
        }
    }
}