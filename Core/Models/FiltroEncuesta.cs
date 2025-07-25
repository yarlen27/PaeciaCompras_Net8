using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class FiltroEncuesta
    {


        public DateTime inicio { get; set; }
        public DateTime fin { get; set; }
        public List<string> usuarios { get; set; }
        public List<Guid> proveedor { get; set; }
    }
}
