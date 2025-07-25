using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{

    [CollectionName("ConsecutivoCausacion")]
    public class ConsecutivoCausacion : CollectionDTO
    {

        public string nit { get; set; }

        public int consecutivoDatosCotables { get; set; }

        public string fechaDatosContables { get; set; }

    }
}
