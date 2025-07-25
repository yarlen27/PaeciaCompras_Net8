using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cobalto.Mongo.Core.DTO
{
    public class CollectionDTO
    {
        public Guid id { get; set; }
        public bool erased { get; set; }

        public Uri GetUri(string database, string collectionName)
        {
            throw new NotImplementedException("No implementado");

            //return UriFactory.CreateDocumentUri(database, collectionName, this.id.ToString());
        }
    }
}
