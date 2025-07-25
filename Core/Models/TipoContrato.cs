using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    [CollectionName("TipoContrato")]

    public class TipoContrato : CollectionDTO
    {

        public string nombre { get; set; }

        public string utilidad { get; set; }

    }
}
