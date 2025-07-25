using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    [CollectionName("client")]

    public class Client: CollectionDTO
    {

        public string Name { get; set; }

    }
}
