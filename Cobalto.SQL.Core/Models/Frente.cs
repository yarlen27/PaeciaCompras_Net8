using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalto.SQL.Core.Models
{
    public class Frente : BaseTable
    {


        public Guid IdProyecto { get; set; }
        public int IdResponsable { get; set; }
        public string Nombre { get; set; }

    }

}
