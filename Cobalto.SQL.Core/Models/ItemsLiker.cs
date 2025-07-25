using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalto.SQL.Core.Models
{
    public class ItemsLiker : ItemEvaluarLiker
    {
        public List<Criterio> Detalles { get; set; }
    }
}
