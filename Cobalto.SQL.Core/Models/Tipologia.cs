using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cobalto.SQL.Core.Models
{
    public class Tipologia : BaseTable
    {

        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Calidad { get; set; }
        public bool SST { get; set; }
        public bool Ambiental { get; set; }

    }
}