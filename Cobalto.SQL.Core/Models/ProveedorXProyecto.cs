using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalto.SQL.Core.Models
{
    public class ProveedorXProyecto : BaseTable
    {
        public Guid IdProyectoCompras { get; set; }
        public Guid IdProveedorCompras { get; set; }
        public int IdProveedor { get; set; }
    }
}
