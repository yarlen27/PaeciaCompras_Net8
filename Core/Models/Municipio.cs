using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    [CollectionName("municipio")]
    public class Municipio : CollectionDTO
    {
        public string codDepartamento { get; set; }
        public string codigo { get; set; }
        public string nombre { get; set; }


    }
}
