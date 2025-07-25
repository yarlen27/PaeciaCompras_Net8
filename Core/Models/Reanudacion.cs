using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;

namespace Core.Models
{
    [CollectionName("reanudacion")]
    public class Reanudacion : CollectionDTO, IPolizas
    {


        public Guid idOrden { get; set; }
        public string archivoContrato { get; set; }
        public string archivoContratoFirmado { get; set; }
        public List<string> otrosArchivos { get; set; }
        public DateTime fechaReanudacion { get; set; }

        public DateTime? fechaCreacion { get; set; }

        public List<Poliza> poliza { get; set; }
    }



}
