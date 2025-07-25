using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    [CollectionName("departamento")]
    public class Departamento: CollectionDTO
    {
        public string codigo { get; set; }
        public string nombre { get; set; }

    }
}
