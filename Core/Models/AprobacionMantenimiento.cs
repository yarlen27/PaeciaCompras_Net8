using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{



    [BsonKnownTypes(typeof(RechazoAdministradorMantenimiento))]
    [BsonKnownTypes(typeof(RechazoCoordinadorMantenimiento))]
    
    public class AprobacionMantenimiento
    {
        public DateTime? Fecha { get; set; }
        public string IdUsuario { get; set; }
        public Guid IdFactura { get; set; }
        public string Comentarios { get; set; }
        public string usuario { get; set; }

    }
}
