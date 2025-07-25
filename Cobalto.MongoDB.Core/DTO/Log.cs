using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalto.MongoDB.Core.DTO
{
    public class Log
    {
        public Guid id { get; set; }
        public DateTime Fecha { get;  set; }
        public string Usuario { get;  set; }
        public string Entidad { get;  set; }
        public object IdEntidad { get;  set; }
        public string Json { get;  set; }
        public string Operacion { get;  set; }
    }
}
