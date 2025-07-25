using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{

    [CollectionName("EncuestaProveedor")]
    public class EncuestaProveedor: CollectionDTO
    {

        public string usuario { get; set; }

        public Guid proveedor { get; set; }
        public string nombreProveedor { get; set; }

        public DateTime fecha { get; set; }

        public List<PreguntaProveedor> preguntas { get; set; }


    }
}
