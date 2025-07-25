using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class FiltroReporteReferencias
    {

        public DateTime? fechaInicial { get; set; }
        public DateTime? fechaFinal { get; set; }

        public List<Guid> referencia { get; set; }
        public List<Guid> categoria { get; set; }

        public List<Guid> proyecto { get; set; }

    }
}
