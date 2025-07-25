using Cobalto.Mongo.Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Bll
{
    public class ReferenciaProyectoBLL : BaseBLL<ReferenciaProyecto>
    {

        public ReferenciaProyectoBLL(IConfiguration configuration, IHttpContextAccessor httpContext) : base(configuration, httpContext)
        {

        }

        public virtual async Task<List<ReferenciaProyecto>> InsertBulk(List<ReferenciaProyecto> collection, bool aprobado = false)
        {

            if (aprobado == false)
            {
                foreach (var item in collection)
                {
                    if (item.id == null || item.id == Guid.Empty)
                    {
                        await this.Insert(item);
                    }
                    else
                    {

                        await this.Update(item);

                    }
                }
            }
            else
            {
                //1 buscar todas las referencias del mismo proyecto

                var filter = Builders<ReferenciaProyecto>.Filter.Eq("proyecto", collection.First().proyecto) & Builders<ReferenciaProyecto>.Filter.Eq("referencia", collection.First().referencia) & Builders<ReferenciaProyecto>.Filter.Eq("aprobado", true);

                var referenciasxProyecto = await this.GetByProterty(filter);

                foreach (var item in referenciasxProyecto)
                {
                    item.aprobado = false;
                    await this.Update(item);
                }

                //2. actualizar
                await this.Update(collection.First());

            }
            return collection;
        }

        public async Task<ReferenciaProyecto> ObtenerReferenciaAprobadaPorProyecto(ReferenciaProyecto entity)
        {
            var filter = Builders<ReferenciaProyecto>.Filter.Eq("proyecto", entity.proyecto) & Builders<ReferenciaProyecto>.Filter.Eq("id", entity.referencia) & Builders<ReferenciaProyecto>.Filter.Eq("aprobado", true);
            //var filter = Builders<ReferenciaProyecto>.Filter.Eq("proyecto", entity.proyecto); //& Builders<ReferenciaProyecto>.Filter.Eq("referencia", entity.referencia);
            var result = await this.GetByProterty(filter);

            return result.FirstOrDefault();

        }
    }
}
