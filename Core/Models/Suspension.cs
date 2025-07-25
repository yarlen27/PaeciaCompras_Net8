using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{

    [CollectionName("suspension")]
    public class Suspension: CollectionDTO, IPolizas
    {


        public Guid idOrden { get; set; }
        public string archivoContrato { get; set; }
        public string archivoContratoFirmado { get; set; }
        public List<string> otrosArchivos { get; set; }
        public DateTime fechaSuspension { get; set; }
        public string motivo { get; set; }
        public DateTime? fechaCreacion { get; set; }
        public List<Poliza> poliza { get; set; }

    }



}
