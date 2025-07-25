
using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    [CollectionName("remision")]
    public class Remision : CollectionDTO
    {
        public DateTime fecha { get; set; }
        public string numeroRemision { get; set; }
        public Guid archivo { get; set; }
        public bool ereased { get; set; }
        public Guid idOrdenCompra { get; set; }
        public Guid? idFacturaAsociada { get; set; }

    }
}
