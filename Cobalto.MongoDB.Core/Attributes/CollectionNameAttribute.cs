using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cobalto.Mongo.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CollectionNameAttribute : Attribute
    {
        public readonly string collectionName;

        public CollectionNameAttribute(string collection)
        {
            this.collectionName = collection;
        }
    }
}
