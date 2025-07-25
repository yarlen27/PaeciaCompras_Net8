using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class Filtro
    {
        public DateTime? inicio { get; set; }
        public DateTime? fin { get; set; }
        public List<Guid> proveedor { get; set; }
        public List<Guid> proyectos { get; set; }

        public bool? paecia { get; set; }

    }

    public class FiltroReporte
    {

        public DateTime inicio { get; set; }
        public DateTime fin { get; set; }
        public List<Guid> proyectos { get; set; }
        public List<Guid> proveedores { get; set; }

    }
}
