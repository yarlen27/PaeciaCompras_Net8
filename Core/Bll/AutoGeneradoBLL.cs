using Cobalto.Mongo.Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Bll
{
    public class AutoGeneradoBLL : BaseBLL<AutoGenerado>
    {

        public AutoGeneradoBLL(IConfiguration configuration, IHttpContextAccessor httpContext) : base(configuration, httpContext)
        {

        }

        public async Task<long> CountAll()
        {
           return await collection.CountDocumentsAsync(new BsonDocument());
        }
    }
}
