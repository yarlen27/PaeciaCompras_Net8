using Cobalto.SQL.Core.Models;
using Core.BLL;
using Core.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cobalto.SQL.Core.BLL
{
    public class DocumentoSagrilafBLL : BaseBLL<DocumentoSagrilaf>
    {
        private readonly ProveedorBLL proveedorBLL;

        public DocumentoSagrilafBLL(IConfiguration configuration, ProveedorBLL proveedorBLL) : base(configuration)
        {
            this.proveedorBLL = proveedorBLL;
        }

        public void InsertarArchivos(List<DocumentoSagrilaf> entity)
        {

            //obtener archivos por proveedorid
            var archivos = this.Filtrar(new { ProveedorId = entity.FirstOrDefault().ProveedorId });

            foreach (var item in archivos)
            {
                item.Borrado = true;
                this.Actualizar(item);
            }

            //eliminar archivos


            foreach (var item in entity)
            {
                this.Insertar(item);
            }
        }

        public IEnumerable<DocumentoSagrilaf> PorProveedor(Guid id)
        {
            var archivos = this.Filtrar(new { ProveedorId = id, Borrado = false });

            return archivos;

        }

        public async Task<List<NotificacionSagrilaf>> ProximosVencimientos()
        {

            var result = new List<NotificacionSagrilaf>();

            using (var connection = new SqlConnection(_connectionString))
            {

                var sql = "select proveedorId, fecha from DocumentoSagrilaf where DATEADD(DAY, 350, fecha) < GetDate()";
                var queryResult = await connection.QueryAsync<DocumentoVencimiento>(sql);

                var agrupadoPorProveedor = queryResult.GroupBy(x => x.ProveedorId);

                foreach (var item in agrupadoPorProveedor)
                {
                    var proveedor = await this.proveedorBLL.GetById(item.Key);
                    var notificacion = new NotificacionSagrilaf
                    {
                        Fecha = DateTime.Now,
                        Leido = false,
                        Mensaje = $"El proveedor {proveedor.nombre} tiene documentos sagrilaf próximos a vencer",
                        Para = "Compras",
                        Titulo = "Documentos próximos a vencer"
                    };
                    result.Add(notificacion);

                }



                return result;
            }


        }

        public class DocumentoVencimiento
        {
            public Guid ProveedorId { get; set; }
            public DateTime Fecha { get; set; }
        }



        public class NotificacionSagrilaf : BaseTable
        {

            public string Titulo { get; set; }
            public DateTime Fecha { get; set; }
            public bool Leido { get; set; }
            public string Mensaje { get; set; }
            public string Para { get; set; }
        }


        public async Task<List<string>> SagrilafActualizacion(MemoryStream inMs)
        {
            //el archivo es un csv, con las siguientes columnas: nit, razon social, clasificacion sagrilaf
            //leemos el archivo línea por línea
            List<string> errores = new List<string>();
            var bytes = inMs.ToArray();

            var k = 0;

            var ms = new MemoryStream(bytes);
            using (var reader = new StreamReader(ms, Encoding.GetEncoding("iso-8859-1")))
            {
                while (!reader.EndOfStream)
                {

                   
                    var line = reader.ReadLine();

                    k++;
                    if (k == 1)
                    {
                        continue;
                    }


                    var values = line.Split(',');
                    //obtenemos el nit de la primera columna
                    //si el número de columnas es diferente a 3, se agrega un error
                    if (values.Count() != 3)
                    {
                        errores.Add($"Línea: {k} - El número de columnas es diferente a 3");
                        continue;
                    }

                    //se busca el proveedor por nit
                    var proveedor = await this.proveedorBLL.PorNIT(values[0]);

                    if (proveedor == null)
                    {
                        errores.Add($"Línea: {k} - No se encontró el proveedor con nit {values[0]}");
                    }
                    else
                    {
                        //se actualiza la clasificación sagrilaf
                        proveedor.ClasificacionSagrilaf = values[2];
                        await this.proveedorBLL.Update(proveedor);
                    }
                }
            }

            return errores;
        }

        public async Task<string> SagrilafValido(Guid id)
        {

            var proveedor = await this.proveedorBLL.GetByIdErased(id);
            if (proveedor.erased)
            {
                return "No requiere";

            }

            if (proveedor.nit == "900931310")
            {
                return "Tiene";
            }

            if (proveedor.ClasificacionSagrilaf == "No requiere")
            {
                return "No requiere";
            }


            if (string.IsNullOrEmpty(proveedor.ClasificacionSagrilaf))
            {
                return "No tiene";
            }


            var documentos = this.PorProveedor(id);

            if (documentos.Count() < 2)
            {
                return "No tiene";
            }
            else
            {
                foreach (var item in documentos)
                {
                    //segun la ClasificacionSagrilaf se evaluará la fecha de cada documento y se comparará con la fecha de hoy, 
                    //la clasificación puede ser: Misional, De Apoyo o No requiere                       
                    if (proveedor.ClasificacionSagrilaf == "Misional")
                    {
                        //si la fecha del documento es superior a un año, el proveedor no es válido
                        if (item.Fecha < DateTime.Now.AddYears(-1))
                        {
                            return "Renovar";
                        }
                        else
                        {
                            return "Tiene";
                        }
                    }
                    else if (proveedor.ClasificacionSagrilaf == "De Apoyo")
                    {
                        //si la fecha del documento es superior a dos años, el proveedor no es válido
                        if (item.Fecha < DateTime.Now.AddYears(-2))
                        {
                            return "Renovar";
                        }
                        else { return "Tiene"; }
                    }

                }
            }

            return "No tiene";
          

        }

        public async Task<List<SagrilafProveedor>> Sagrilaf()
        {
            var todosLosProveedores = await proveedorBLL.GetAll();

            var result = new List<SagrilafProveedor>();

            foreach (Proveedor proveedor in todosLosProveedores)
            {
                var sagrilaf =  new SagrilafProveedor();
                sagrilaf.Id = proveedor.id;
                sagrilaf.Estado = await SagrilafValido(proveedor.id);

                result.Add(sagrilaf);
            }


            return result;

        }

        public class SagrilafProveedor
        {
            public Guid Id { get; set; }
            public string Estado { get; set; }
        }
    }
}
