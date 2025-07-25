using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cobalto.SQL.Core.Models
{
    public class ItemEvaluarSiNo : BaseTable
    {

        public string Aspecto { get; set; }
        public bool Calidad { get; set; }
        public bool SST { get; set; }
        public bool Ambiental { get; set; }
        

    }
    public class ItemEvaluarSiNoVacio : ItemEvaluarSiNo, IItemsEvaluar
    {
        public bool EsSiNo { get; set; }
        public int IdItem { get; set; }
        public int Respuesta { get; set; }
    }


}