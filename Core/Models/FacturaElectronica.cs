using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    [CollectionName("FacturaElectronica")]
    public class FacturaElectronica : CollectionDTO
    {
        public string nitProveedor { get; set; }
        public double? total { get; set; }
        public string numeroFactura { get; set; }
        public string pdf { get; set; }
        public string pdfFileName { get; set; }
        public Guid idPdfUploaded { get; set; }
        public bool procesado { get; set; }
        public DateTime? fechaElaboracion { get; set; }
        public DateTime? fechaVencimiento { get; set; }
        public DateTime? fechaAgregado { get; set; }
        public string nitProyecto { get; set; }
    }


}
