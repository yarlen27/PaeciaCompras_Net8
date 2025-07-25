using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{

    [CollectionName("ReferenciaProyecto")]
    public class ReferenciaProyecto : CollectionDTO
    {

        public Guid referencia { get; set; }
        public string archivoCotizacion { get; set; }

        public Guid categoria { get; set; }

        public Guid proyecto { get; set; }
        public Guid proveedor { get; set; }

        public string unidad { get; set; }

        public double valorUnitario { get; set; }

        public double cantidad { get; set; }

        public bool aprobado { get; set; }


        [BsonIgnore]

        public Referencia objReferencia { get; set; }

    }
}
