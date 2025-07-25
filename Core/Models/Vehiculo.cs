using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    [CollectionName("vehiculo")]
    public class Vehiculo:CollectionDTO
    {
        public string placa { get; set; }
        public string descripcion { get; set; }


    }
}
