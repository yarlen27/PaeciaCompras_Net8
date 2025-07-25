using Cobalto.MongoDB.Core.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cobalto.MongoDB.Core.BLL
{
    public class LoggingBLL
    {
        public static string INSERT = "INSERTAR";
        public static string UPDATE = "ACTUALIZAR";
        public static string DELETE = "BORRAR";

        protected IMongoClient client;
        protected IMongoDatabase _database;
        protected IMongoCollection<Log> collection;

        private string database;
        private string connectionString;
        private IConfiguration configuration;
        protected string collectionName;
        protected IHttpContextAccessor httpContext;

        protected string clientId;

        public LoggingBLL(IConfiguration configuration, IHttpContextAccessor httpContext)
        {
            this.configuration = configuration;
            this.httpContext = httpContext;


            this.clientId = httpContext.HttpContext.Request.Headers?.GetCommaSeparatedValues("clientId")?.FirstOrDefault();

            this.database = this.configuration.GetSection("MongoDb")["DatabaseName"];
            this.connectionString = this.configuration.GetSection("MongoDb")["ConnectionString"];
            Initialize(database);
        }

        public async Task<List<Log>> GetLog(Guid id)
        {
            var filter = Builders<Log>.Filter.Eq("IdEntidad", id);
            var result = await collection.Find(filter).ToListAsync();
            return result;
        }


        public async Task<List<Log>> GetLog(string coleccion)
        {
            var filter = Builders<Log>.Filter.Eq("Entidad", coleccion);
            var result = await collection.Find(filter).ToListAsync();
            return result;
        }

        public void Initialize(string database)
        {
            if (this.clientId == null)
            {
                this.database = $"{database}";
            }
            else
            {
                this.database = $"{database}_{this.clientId}";
            }

            if (this.client == null)
            {

                this.client = new MongoClient(this.connectionString);
            }
            _database = client.GetDatabase(this.database);
            GetCollectionName();

        }


        public void GetCollectionName()
        {
            collectionName = "Log";
            collection = _database.GetCollection<Log>(collectionName);

        }

        public async Task<Log> Insert(Log item)
        {
            item.id = Guid.NewGuid();
            await collection.InsertOneAsync(item);
            return item;
        }
    }
}
