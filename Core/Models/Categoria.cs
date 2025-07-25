using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{

    [CollectionName("categoria")]
    public class Categoria: CollectionDTO
    {

        public string Name { get; set; }

    }
}
