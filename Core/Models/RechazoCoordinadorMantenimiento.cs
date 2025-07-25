using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{

    [BsonDiscriminator("RechazoCoordinadorMantenimiento")]
    public class RechazoCoordinadorMantenimiento : AprobacionMantenimiento
    {
        public string Razon { get; set; }
    }
}
