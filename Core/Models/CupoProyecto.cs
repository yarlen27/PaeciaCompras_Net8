using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class CupoDisponibleProyecto
    {
        public Guid idProyecto { get; set; }
        public Guid idProveedor { get; set; }

        public double TotalMateriales { get; set; }
        public double TotalServicios { get; set; }

        public double? CupoProveedor { get; set; }

        public double? CupoDisponible { get; set; }

    }
}
