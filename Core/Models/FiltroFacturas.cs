using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class FiltroFacturas
    {

        public string inicio { get; set; }
        public string fin { get; set; }
        public List<Guid>? proyectos { get; set; } = new List<Guid>();
        public List<Guid>? proveedor { get; set; } = new List<Guid>();
        public string estado { get; set; }

    }
}
