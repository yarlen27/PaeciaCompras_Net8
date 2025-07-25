using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cobalto.SQL.Core.Models;
using Microsoft.Data.SqlClient;
using Dapper;
using AutoMapper;
using Core.BLL;
using AspNetCore.Identity.MongoDB;
using Microsoft.AspNetCore.Identity;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Cobalto.SQL.Core.BLL
{
    public class EvaluacionBLL : BaseBLL<Evaluacion>
    {
        private ProveedorSQLBLL _proveedorBLL;
        private ItemEvaluarLikerBLL _itemEvaluarLikerBLL;
        private ItemEvaluarSiNoBLL _itemEvaluarSiNoBLL;
        private CriterioBLL _criterioBLL;
        private TipologiaBLL _tipologiaBLL;
        private ItemEvaluacionBLL _itemEvaluacionBLL;
        private ProveedorBLL proveedorMongoBLL;
        private UserManager<MongoIdentityUser> _userManager;

        public EvaluacionBLL(IConfiguration configuration,
            ProveedorSQLBLL proveedorBLL,
            ItemEvaluarLikerBLL itemEvaluarLikerBLL,
            ItemEvaluarSiNoBLL itemEvaluarSiNoBLL,
            ItemEvaluacionBLL itemEvaluacionBLL,
            CriterioBLL criterioBLL,
            ProveedorBLL proveedorMongoBLL,
            UserManager<MongoIdentityUser> _userManager,
        TipologiaBLL tipologiaBLL) : base(configuration)
        {
            this._proveedorBLL = proveedorBLL;
            this._itemEvaluarLikerBLL = itemEvaluarLikerBLL;
            this._itemEvaluarSiNoBLL = itemEvaluarSiNoBLL;
            this._criterioBLL = criterioBLL;
            this._tipologiaBLL = tipologiaBLL;
            this._itemEvaluacionBLL = itemEvaluacionBLL;
            this.proveedorMongoBLL = proveedorMongoBLL;

            this._userManager = _userManager;
        }

        public async Task<List<RegistroReporteEvaluacion>> ObtenerReporte(FiltroEvaluacion filtro)
        {

            List<RegistroReporteEvaluacion> result = new List<RegistroReporteEvaluacion>();

            var itemsLIker = this._itemEvaluarLikerBLL.Todos();

            var itemsAmbiental = itemsLIker.Where(x => x.Ambiental == true).Select(x => x.Id).ToList();
            var itemsSST = itemsLIker.Where(x => x.SST == true).Select(x => x.Id).ToList();
            var itemsCalidad = itemsLIker.Where(x => x.Calidad == true).Select(x => x.Id).ToList();


            var proveedor = this._proveedorBLL.Filtrar(new { IdCompras = filtro.IdsProveedores.First() }).First();

            IEnumerable<Evaluacion> evaluaciones;
            using (var connection = new SqlConnection(_connectionString))
            {



                evaluaciones = connection.GetList<Evaluacion>(" where Fecha > @FechaInicial and Fecha < @FechaFinal and IdProveedor = " + proveedor.Id, filtro);


            }
            var idsProveedores = from e in evaluaciones select e.IdProveedor;
            var idsProyectos = from e in evaluaciones select e.IdProyecto;

            var proveedores = this._proveedorBLL.PorIds(idsProveedores);



            var listaUsuarios = new List<MongoIdentityUser>();
            listaUsuarios = _userManager.Users.ToList();

            


            foreach (var item in evaluaciones)
            {
                List<int> valoresSST = new List<int>();
                List<int> valoresCalidad = new List<int>();
                List<int> valoresAmbiental = new List<int>();

                RegistroReporteEvaluacion registro = new RegistroReporteEvaluacion();

                registro.FechaEvaluacion = item.Fecha;

                if (item.EsSeleccion == true)
                {
                    registro.TipoEvaluacion = "SELECCIÓN";
                }
                else
                {
                    registro.TipoEvaluacion = "PERIODICA";

                }

                //var proveedor = proveedores.FirstOrDefault(x => x.Id == item.IdProveedor);

                var proveedorMongo = await this.proveedorMongoBLL.GetById(proveedor.IdCompras);


                registro.NIT = proveedorMongo.nit;

                registro.Proveedor = proveedor.Nombre;

                registro.Evaluador = item.Evaluador;

                var respuestas = await this._itemEvaluacionBLL.ObtenerItemsEvaluacionLikerPorId(item.Id);

                valoresAmbiental.AddRange((from r in respuestas
                                           where r.Valor > 0 && itemsAmbiental.Contains(r.IdItem)
                                           select r.Valor));

                valoresSST.AddRange((from r in respuestas
                                     where r.Valor > 0 && itemsSST.Contains(r.IdItem)
                                     select r.Valor));



                valoresCalidad.AddRange((from r in respuestas
                                         where r.Valor > 0 && itemsCalidad.Contains(r.IdItem)
                                         select r.Valor));


                if (valoresAmbiental.Count > 0)
                {
                    registro.PuntajeEvaluacionAmbiental = valoresAmbiental.Average();
                }
                if (valoresSST.Count > 0)
                {
                    registro.PuntajeEvaluacionSST = valoresSST.Average();

                }

                if (valoresCalidad.Count > 0)
                {
                    registro.PuntajeEvaluacionCalidad = valoresCalidad.Average();
                }

                registro.Evaluador = listaUsuarios.FirstOrDefault(x => x.Id == registro.Evaluador).UserName;

                registro.Id = item.Id;
                result.Add(registro);
            }

            return result;


        }

        public async Task<List<RegistroReporteConsolidado>> ObtenerReporteConsolidado(FiltroEvaluacion filtro)
        {

            var itemsLIker = this._itemEvaluarLikerBLL.Todos();

            var itemsAmbiental = itemsLIker.Where(x => x.Ambiental == true).Select(x => x.Id).ToList();
            var itemsSST = itemsLIker.Where(x => x.SST == true).Select(x => x.Id).ToList();
            var itemsCalidad = itemsLIker.Where(x => x.Calidad == true).Select(x => x.Id).ToList();
            List<RegistroReporteConsolidado> result = new List<RegistroReporteConsolidado>();

            foreach (var proveedor in filtro.IdsProveedores)
            {



                var proveedorSQL = this._proveedorBLL.Filtrar(new { IdCompras = proveedor }).FirstOrDefault();

                if (proveedorSQL == null)
                {
                    continue;
                }


                IEnumerable<Evaluacion> evaluaciones;
                using (var connection = new SqlConnection(_connectionString))
                {

                    evaluaciones = connection.GetList<Evaluacion>(" where IdProveedor = " + proveedorSQL.Id, filtro);

                }

                var primeroJunioAnoAnterior = new DateTime(DateTime.Now.Year - 1, 6, 1);
                var mayo31 = new DateTime(DateTime.Now.Year, 5, 31);

                evaluaciones = evaluaciones.Where(x=> x.Fecha >= primeroJunioAnoAnterior && x.Fecha <= mayo31);

                var listaUsuarios = new List<MongoIdentityUser>();
                listaUsuarios = _userManager.Users.ToList();


                RegistroReporteConsolidado registro = new RegistroReporteConsolidado();
                registro.Id = proveedor;

                var proveedorMongo = await this.proveedorMongoBLL.GetById(proveedorSQL.IdCompras);


                registro.NIT = proveedorMongo.nit;
                registro.Proveedor = proveedorSQL.Nombre;

                List<int> valoresSST = new List<int>();
                List<int> valoresCalidad = new List<int>();
                List<int> valoresAmbiental = new List<int>();

                foreach (var item in evaluaciones)
                {
                    var respuestas = await this._itemEvaluacionBLL.ObtenerItemsEvaluacionLikerPorId(item.Id);

                    if (item.EsSeleccion == true)
                    {
                        registro.FechaEvaluacionSeleccion = item.Fecha;

                        var respuestasSeleccionAmbiental = (from r in respuestas
                                                            where r.Valor > 0 && itemsAmbiental.Contains(r.IdItem)
                                                            select r.Valor);
                        if (respuestasSeleccionAmbiental.Count() > 0)
                        {
                            registro.PuntajeSeleccionAmbiental = respuestasSeleccionAmbiental.Average();

                        }


                        var respuestasSST = (from r in respuestas
                                             where r.Valor > 0 && itemsSST.Contains(r.IdItem)
                                             select r.Valor);

                        if (respuestasSST.Count() > 0)
                        {
                            registro.PuntajeSeleccionSST = respuestasSST.Average();

                        }

                        var respuestasSelecccionCalida = (from r in respuestas
                                                          where r.Valor > 0 && itemsCalidad.Contains(r.IdItem)
                                                          select r.Valor);

                        if (respuestasSelecccionCalida.Count() > 0)
                        {
                            registro.PuntajeSeleccionCalidad = respuestasSelecccionCalida.Average();

                        }




                    }
                    else
                    {
                        registro.FechaUltimaEvaluacion = item.Fecha;
                        valoresAmbiental.AddRange((from r in respuestas
                                                   where r.Valor > 0 && itemsAmbiental.Contains(r.IdItem)
                                                   select r.Valor));

                        valoresSST.AddRange((from r in respuestas
                                             where r.Valor > 0 && itemsSST.Contains(r.IdItem)
                                             select r.Valor));



                        valoresCalidad.AddRange((from r in respuestas
                                                 where r.Valor > 0 && itemsCalidad.Contains(r.IdItem)
                                                 select r.Valor));



                    }
                }
                if (valoresAmbiental.Count > 0)
                {
                    registro.PuntajeEvaluacionAmbiental = valoresAmbiental.Average();
                }
                if (valoresSST.Count > 0)
                {
                    registro.PuntajeEvaluacionSST = valoresSST.Average();

                }

                if (valoresCalidad.Count > 0)
                {
                    registro.PuntajeEvaluacionCalidad = valoresCalidad.Average();
                }





                result.Add(registro);


            }


            return result;
        }

        public int GuardarEvaluacionXProveedor(EvaluacionRealizada evaluacionRealizada)
        {
            int? respuesta = 0;
            var evaluacion = new Evaluacion
            {
                Ambiental = evaluacionRealizada.Ambiental,
                SST = evaluacionRealizada.SST,
                Calidad = evaluacionRealizada.Calidad,

                Fecha = DateTime.Now,
                EsPeriodica = evaluacionRealizada.EsPeriodica,
                EsSeleccion = evaluacionRealizada.EsSeleccion,

                Evaluador = evaluacionRealizada.Evaluador,
                Observaciones = evaluacionRealizada.Observaciones,
                IdProveedor = evaluacionRealizada.IdProveedor,

                Puntaje = evaluacionRealizada.Puntaje,
                IdProyecto = evaluacionRealizada.IdProyecto
            };

            if (evaluacionRealizada.Id > 0)
            {
                var evaluacionAnterior = this.PorId(evaluacionRealizada.Id);
                evaluacion.Fecha = evaluacionAnterior.Fecha;
                this.Actualizar(evaluacion);

                respuesta = evaluacionRealizada.Id;

            }
            else
            {

                respuesta = this.Insertar(evaluacion);


            }


            //borrar respuestas anteriores
            var sqlBorrarPorIdEvaluacion = "DELETE FROM ItemEvaluacion WHERE IdEvaluacion = @IdEvaluacion";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(sqlBorrarPorIdEvaluacion, new { IdEvaluacion = respuesta.Value });
            }



            evaluacionRealizada.PreguntasAmbiental = evaluacionRealizada?.PreguntasAmbiental ?? new List<ItemEvaluarLikerCriterios>();
            _ = InsertarRespuestas(evaluacionRealizada.PreguntasAmbiental, respuesta.Value);


            evaluacionRealizada.PreguntasSST = evaluacionRealizada?.PreguntasSST ?? new List<ItemEvaluarLikerCriterios>();
            _ = InsertarRespuestas(evaluacionRealizada.PreguntasSST, respuesta.Value);


            evaluacionRealizada.PreguntasCalidad = evaluacionRealizada?.PreguntasCalidad ?? new List<ItemEvaluarLikerCriterios>();
            _ = InsertarRespuestas(evaluacionRealizada.PreguntasCalidad, respuesta.Value);

            if (evaluacionRealizada.EsSeleccion)
            {
                var proveedor = this._proveedorBLL.PorId(evaluacion.IdProveedor);
                this.proveedorMongoBLL.EvaluarSeleccion(proveedor.IdCompras);
            }


            return respuesta.Value;
        }

        private int InsertarRespuestas(IEnumerable<IItemsEvaluar> respuestas, int idEvaluacion)
        {
            int count = 0;
            foreach (var item in respuestas)
            {
                var sino = new ItemEvaluacion
                {
                    IdEvaluacion = idEvaluacion,
                    Aspecto = item.Aspecto,
                    Valor = item.Respuesta,
                    EsSiNo = item.EsSiNo,
                    IdItem = item.Id
                };
                _ = this._itemEvaluacionBLL.Insertar(sino);
                count++;
            }
            return count;
        }


        public EvaluacionVacia ObtenerEvaluacionCompletaXId(int id)
        {

            EvaluacionVacia evaluacion = EvaluacionVacia.ConvertObject(this.PorId(id));

            var todosLiker = this._itemEvaluarLikerBLL.Todos();//.Where(x => x.EsSeleccion == false);

            var respuestasEvaluacion = this._itemEvaluacionBLL.Filtrar(new { IdEvaluacion = id });



            if (evaluacion.PreguntasAmbiental == null)
            {
                evaluacion.PreguntasAmbiental = new List<IItemsEvaluar>();
            }


            var itemslikert = todosLiker.Where(iL => iL.Ambiental);


            foreach (var item in AsignarPreguntasTipologiaLiker(itemslikert))
            {

                var respuesta = respuestasEvaluacion.FirstOrDefault(x => x.IdItem == item.Id);

                if (respuesta != null)
                {
                    item.Respuesta = respuesta.Valor;

                    evaluacion.PreguntasAmbiental.Add(item);


                }

            }



            evaluacion.PreguntasSST = evaluacion?.PreguntasSST ?? new List<IItemsEvaluar>();

            itemslikert = todosLiker.Where(iL => iL.SST);


            foreach (var item in AsignarPreguntasTipologiaLiker(itemslikert))
            {

                var respuesta = respuestasEvaluacion.FirstOrDefault(x => x.IdItem == item.Id);

                if (respuesta != null)
                {
                    item.Respuesta = respuesta.Valor;
                    evaluacion.PreguntasSST.Add(item);

                }

            }



            evaluacion.PreguntasCalidad = evaluacion?.PreguntasCalidad ?? new List<IItemsEvaluar>();

            itemslikert = todosLiker.Where(iL => iL.Calidad);



            foreach (var item in AsignarPreguntasTipologiaLiker(itemslikert))
            {

                var respuesta = respuestasEvaluacion.FirstOrDefault(x => x.IdItem == item.Id);

                if (respuesta != null)
                {
                    item.Respuesta = respuesta.Valor;
                    evaluacion.PreguntasCalidad.Add(item);

                }

            }

            return evaluacion;

        }



        public EvaluacionVacia ObtenerEvaluacionXProveedor(int idProveedor)
        {
            EvaluacionVacia evaluacion = new EvaluacionVacia();

            //using (var connection = new SqlConnection(this._connectionString))
            //{
            //    //string query = "SELECT * FROM Tipologia where Id = @IdTipologia";
            //    //var tipologia = await connection.QueryFirstAsync<Tipologia>(query, new { IdProveedor = proveedor.IdTipologia });
            //    connection.Open();



            //}
            var proveedor = this._proveedorBLL.PorId(idProveedor);



            var todosLikerDb = this._itemEvaluarLikerBLL.Todos();
            var todosLikerPeriodica = this._itemEvaluarLikerBLL.Todos().Where(x => x.EsSeleccion == false);

            var todosLiker = new List<ItemEvaluarLiker>();

            var evaluacionSeleccion = this.Filtrar(new
            {

                IdProveedor = idProveedor,
                EsSeleccion = true
            }).OrderBy(x=> x.Fecha).LastOrDefault();


            if (evaluacionSeleccion != null)
            {


                var itemsLikerSeleccion = this._itemEvaluacionBLL.Filtrar(new { IdEvaluacion = evaluacionSeleccion.Id });


                int calidad = 0;
                int sst = 0;
                int ambiental = 0;

                foreach (var item in itemsLikerSeleccion)
                {
                    var plantilla = todosLikerDb.FirstOrDefault(x => x.Id == item.IdItem);
                    if (item.Valor > 0)
                    {
                        if (plantilla.Ambiental == true)
                        {
                            ambiental++;
                        }
                        if (plantilla.Calidad == true)
                        {
                            calidad++;
                        }
                        if (plantilla.SST == true)
                        {
                            sst++;
                        }
                    }

                }

                foreach (var item in todosLikerPeriodica)
                {
                    if (item.Ambiental == true)
                    {
                        if (ambiental > 0)
                        {
                            todosLiker.Add(item);
                        }
                    }
                    if (item.Calidad == true)
                    {
                        if (calidad > 0)
                        {
                            todosLiker.Add(item);
                        }
                    }
                    if (item.SST == true)
                    {
                        if (sst > 0)
                        {
                            todosLiker.Add(item);
                        }
                    }
                }


                //var itemsLikerSeleccionNoAplicaCalidad = itemsLIkerSeleccion.Where(x => x.Valor == 0 && x.);
                //var itemsLikerSeleccionNoAplicaSST = itemsLIkerSeleccion.Where(x => x.Valor == 0);
                //var itemsLikerSeleccionNoAplicaAmbiental= itemsLIkerSeleccion.Where(x => x.Valor == 0);



                //foreach (var item in todosLikerDb)
                //{
                //    var itemEnSeleccion = itemsLIkerSeleccionNoAplica.Where(x=> x.IdItem == item.Id).Count();

                //    if (itemEnSeleccion == 0)
                //    {
                //        todosLiker.Add(item);
                //    }

                //}



            }


            var todosSino = this._itemEvaluarSiNoBLL.Todos();



            evaluacion.PreguntasAmbiental = evaluacion?.PreguntasAmbiental ?? new List<IItemsEvaluar>();

            var itemslikert = todosLiker.Where(iL => iL.Ambiental);
            evaluacion.PreguntasAmbiental.AddRange(AsignarPreguntasTipologiaLiker(itemslikert));

            var itemsSiNo = todosSino.Where(iSiNo => iSiNo.Ambiental);
            evaluacion.PreguntasAmbiental.AddRange(AsignarPreguntasTipologiaSiNo(itemsSiNo));


            evaluacion.PreguntasSST = evaluacion?.PreguntasSST ?? new List<IItemsEvaluar>();

            itemslikert = todosLiker.Where(iL => iL.SST);
            evaluacion.PreguntasSST.AddRange(AsignarPreguntasTipologiaLiker(itemslikert));

            itemsSiNo = todosSino.Where(iSiNo => iSiNo.SST);
            evaluacion.PreguntasSST.AddRange(AsignarPreguntasTipologiaSiNo(itemsSiNo));

            evaluacion.PreguntasCalidad = evaluacion?.PreguntasCalidad ?? new List<IItemsEvaluar>();

            itemslikert = todosLiker.Where(iL => iL.Calidad);
            evaluacion.PreguntasCalidad.AddRange(AsignarPreguntasTipologiaLiker(itemslikert));

            itemsSiNo = todosSino.Where(iSiNo => iSiNo.Calidad);
            evaluacion.PreguntasCalidad.AddRange(AsignarPreguntasTipologiaSiNo(itemsSiNo));











            return evaluacion;

        }
        public EvaluacionVacia SeleccionPorProveedor(Guid idProveedor)
        {
            EvaluacionVacia evaluacion = new EvaluacionVacia();

            //using (var connection = new SqlConnection(this._connectionString))
            //{
            //    //string query = "SELECT * FROM Tipologia where Id = @IdTipologia";
            //    //var tipologia = await connection.QueryFirstAsync<Tipologia>(query, new { IdProveedor = proveedor.IdTipologia });
            //    connection.Open();



            //}
            var proveedor = this._proveedorBLL.PorId(idProveedor);



            var evaluacionSeleccion = this.Filtrar(new
            {

                IdProveedor = proveedor.Result.Id,
                EsSeleccion = true
            }).OrderBy(x => x.Fecha).LastOrDefault();


            if (evaluacionSeleccion != null)
            {

                return this.ObtenerEvaluacionCompletaXId(evaluacionSeleccion.Id);
            }


            return this.ObtenerEvaluacionXProveedor(idProveedor);



        }



        public EvaluacionVacia ObtenerEvaluacionXProveedor(Guid idProveedor)
        {
            EvaluacionVacia evaluacion = new EvaluacionVacia();

            //using (var connection = new SqlConnection(this._connectionString))
            //{
            //    //string query = "SELECT * FROM Tipologia where Id = @IdTipologia";
            //    //var tipologia = await connection.QueryFirstAsync<Tipologia>(query, new { IdProveedor = proveedor.IdTipologia });
            //    connection.Open();



            //}
            var proveedor = this._proveedorBLL.PorId(idProveedor);


            //var tipologia = this._tipologiaBLL.PorId(proveedor.IdTipologia.Value);

            var todosLiker = this._itemEvaluarLikerBLL.Todos().Where(x => x.EsSeleccion == true);
            var todosSino = this._itemEvaluarSiNoBLL.Todos();



            evaluacion.PreguntasAmbiental = evaluacion?.PreguntasAmbiental ?? new List<IItemsEvaluar>();

            var itemslikert = todosLiker.Where(iL => iL.Ambiental);
            evaluacion.PreguntasAmbiental.AddRange(AsignarPreguntasTipologiaLiker(itemslikert));

            var itemsSiNo = todosSino.Where(iSiNo => iSiNo.Ambiental);
            evaluacion.PreguntasAmbiental.AddRange(AsignarPreguntasTipologiaSiNo(itemsSiNo));


            evaluacion.PreguntasSST = evaluacion?.PreguntasSST ?? new List<IItemsEvaluar>();

            itemslikert = todosLiker.Where(iL => iL.SST);
            evaluacion.PreguntasSST.AddRange(AsignarPreguntasTipologiaLiker(itemslikert));

            itemsSiNo = todosSino.Where(iSiNo => iSiNo.SST);
            evaluacion.PreguntasSST.AddRange(AsignarPreguntasTipologiaSiNo(itemsSiNo));

            evaluacion.PreguntasCalidad = evaluacion?.PreguntasCalidad ?? new List<IItemsEvaluar>();

            itemslikert = todosLiker.Where(iL => iL.Calidad);
            evaluacion.PreguntasCalidad.AddRange(AsignarPreguntasTipologiaLiker(itemslikert));

            itemsSiNo = todosSino.Where(iSiNo => iSiNo.Calidad);
            evaluacion.PreguntasCalidad.AddRange(AsignarPreguntasTipologiaSiNo(itemsSiNo));






            return evaluacion;

        }


        private List<IItemsEvaluar> AsignarPreguntasTipologiaLiker(IEnumerable<ItemEvaluarLiker> itemslikert)
        {
            List<IItemsEvaluar> items = new List<IItemsEvaluar>();

            var todoCriterios = this._criterioBLL.Todos();
            foreach (var itemlikert in itemslikert)
            {
                ItemEvaluarLikerCriterios itemEvaluarLikerCriterios = new ItemEvaluarLikerCriterios
                {
                    Id = itemlikert.Id,
                    Aspecto = itemlikert.Aspecto,
                    Calidad = itemlikert.Calidad,
                    SST = itemlikert.SST,
                    Ambiental = itemlikert.Ambiental,
                    Borrado = itemlikert.Borrado,
                    EsSiNo = false

                };

                var criterios = todoCriterios.Where(i => i.IdItemEvaluarLiker == itemlikert.Id).ToList();
                itemEvaluarLikerCriterios.Criterios = criterios;
                items.Add(itemEvaluarLikerCriterios);
            }
            return items;
        }
        private List<IItemsEvaluar> AsignarPreguntasTipologiaSiNo(IEnumerable<ItemEvaluarSiNo> itemsSiNo)
        {
            List<IItemsEvaluar> items = new List<IItemsEvaluar>();

            foreach (var preguntaSiNo in itemsSiNo)
            {

                ItemEvaluarSiNoVacio itemEvaluarSiNoVacio = new ItemEvaluarSiNoVacio
                {
                    Id = preguntaSiNo.Id,
                    Aspecto = preguntaSiNo.Aspecto,
                    Calidad = preguntaSiNo.Calidad,
                    SST = preguntaSiNo.SST,
                    Ambiental = preguntaSiNo.Ambiental,
                    EsSiNo = true,

                };
                items.Add(itemEvaluarSiNoVacio);
            }
            return items;
        }
    }
}