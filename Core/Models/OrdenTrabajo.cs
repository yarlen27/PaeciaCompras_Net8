using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    [CollectionName("OrdenTrabajo")]
    public class OrdenTrabajo : CollectionDTO
    {
        public DateTime fechaCreacion { get; set; }
        public string numeroOrden { get; set; }
        public string observacion { get; set; }
        public string idUsuario { get; set; }
        public Guid? idFactura { get; set; }
        public Guid? idProyecto { get; set; }
        public Guid? idProveedor { get; set; }
        public string idArchivo { get; set; }
        public Guid IdVehiculo { get; set; }
    }
}
