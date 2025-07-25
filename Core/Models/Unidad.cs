using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    [CollectionName("Unidad")]

    public class Unidad : CollectionDTO
    {

        public string nombre { get; set; }


    }
}
