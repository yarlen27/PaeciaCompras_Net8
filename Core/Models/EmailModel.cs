using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{

    [CollectionName("emailmodel")]

    public class EmailModel :  CollectionDTO
    {

        public Guid entityId { get; set; }
    }
}
