using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cobalto.SQL.Core.Models
{
    public class ItemEvaluacion : BaseTable
    {

        public int IdEvaluacion { get; set; }
        public string Aspecto { get; set; }
        public int Valor { get; set; }
        public bool EsSiNo { get; set; }
        public int IdItem { get; set; }

    }
}