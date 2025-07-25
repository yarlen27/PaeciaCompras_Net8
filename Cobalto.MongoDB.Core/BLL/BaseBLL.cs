using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using Cobalto.MongoDB.Core.BLL;
using Cobalto.MongoDB.Core.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
namespace Cobalto.Mongo.Core.BLL
{
    public class BaseBLL<T> where T : CollectionDTO
    {

        protected IMongoClient client;
        protected IMongoDatabase _database;
        protected IMongoCollection<T> collection;

        private string database;


        private string connectionString;
        protected IConfiguration configuration;



        protected string collectionName;
        protected IHttpContextAccessor httpContext;
        private LoggingBLL log;
        public string clientId;

        public BaseBLL(IConfiguration configuration, IHttpContextAccessor httpContext)
        {
            this.configuration = configuration;
            this.httpContext = httpContext;
            this.log = new LoggingBLL(configuration, httpContext);

            this.clientId = httpContext.HttpContext.Request.Headers?.GetCommaSeparatedValues("clientId")?.FirstOrDefault();

            if (clientId == null)
            {

            }

            this.database = this.configuration.GetSection("MongoDb")["DatabaseName"];
            this.connectionString = this.configuration.GetSection("MongoDb")["ConnectionString"];
            Initialize(database);
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
            GetCollectionName(typeof(T));

        }


        public void GetCollectionName(Type t)
        {
            collectionName = ((CollectionNameAttribute)Attribute.GetCustomAttribute(t, typeof(CollectionNameAttribute))).collectionName;
            collection = _database.GetCollection<T>(collectionName);

        }

        public virtual async Task<T> Insert(T item)
        {
            item.id = Guid.NewGuid();
            try
            {
                collection.InsertOne(item);
            }
            catch (Exception ex)
            {

                throw;
            }



            return item;
        }





        public virtual async Task<List<T>> GetAll()
        {

            var query = Builders<T>.Filter.Where(x => !x.erased);
            var cursor = await collection.FindAsync<T>(query);

            return cursor.ToList();
        }

        public async Task<List<T>> GetAllWithEreased()
        {

            var query = Builders<T>.Filter.Where(x => !x.erased || x.erased);
            var cursor = await collection.FindAsync<T>(query);

            return cursor.ToList();
        }

        public async Task<T> GetById(Guid id)
        {
            var query = Builders<T>.Filter.Where(x => x.id == id && !x.erased);
            var cursor = await collection.FindAsync<T>(query);

            var y = cursor.FirstOrDefault();
            return y;
        }


        public async Task<T> GetByIdErased(Guid id)
        {
            var query = Builders<T>.Filter.Where(x => x.id == id);
            var cursor = await collection.FindAsync<T>(query);

            var y = cursor.FirstOrDefault();
            return y;
        }



        public virtual async Task Update(T item)
        {
            var query = Builders<T>.Filter.Where(x => x.id == item.id);
            await collection.FindOneAndReplaceAsync(query, item);

        }

        public async Task<bool> DatabaseFound(string databaseName)
        {

            using (var cursor = await client.ListDatabasesAsync())
            {
                var databaseDocuments = await cursor.ToListAsync();
                foreach (var databaseDocument in databaseDocuments)
                {
                    if (databaseDocument["name"] == databaseName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public async Task<bool> ExistWithProperty<G>(string property, G value)
        {
            var filter = Builders<T>.Filter.Eq(property, value);
            var result = await collection.Find(filter).ToListAsync();
            return result.Count != 0;
        }

        public async Task<bool> ExistWithProperty(FilterDefinition<T> filter)
        {
            var result = await collection.Find(filter).ToListAsync();
            return result.Count != 0;
        }
        public async Task DeleteByIdFromDataBase(Guid id)
        {

            var query = Builders<T>.Filter.Where(x => x.id == id);
            await collection.DeleteOneAsync(query);

        }

        public async Task<List<T>> GetByProterty<G>(string property, G value)
        {
            var filter = Builders<T>.Filter.Eq(property, value);
            var result = await collection.Find(filter).ToListAsync();

            
            return result;
        }

        public async Task<List<T>> GetByProterty(FilterDefinition<T> filter)
        {
            var result = await collection.Find(filter).ToListAsync();
            return result;
        }

        public List<T> GetByProterty(FilterDefinition<T> filter, string sortBy, string sortDirection)
        {
            if (sortDirection == "asc")
            {
                return collection.Find<T>(filter).Sort(Builders<T>.Sort.Ascending(sortBy)).ToList();
            }
            else
            {
                return collection.Find<T>(filter).Sort(Builders<T>.Sort.Descending(sortBy)).ToList();


            }
        }

      
    }
}
