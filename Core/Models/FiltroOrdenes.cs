using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class FiltroOrdenes
    {

        public class FiltroFacturas
        {

            public DateTime inicio { get; set; }
            public DateTime fin { get; set; }
            public List<Guid> proyectos { get; set; }
            public List<Guid> proveedor { get; set; }
        }
    }
}
