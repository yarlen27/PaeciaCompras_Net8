using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cobalto.SQL.Core.Models
{
    public class Adjuntos : BaseTable
    {

        public int IdEvaluacion { get; set; }
        public Guid IdDocumento { get; set; }
        public string Titulo { get; set; }

    }
}