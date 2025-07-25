using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cobalto.SQL.Core.Models
{
    public class ItemEvaluarLiker : BaseTable
    {

        public string Aspecto { get; set; }
        public bool Calidad { get; set; }
        public bool SST { get; set; }
        public bool Ambiental { get; set; }

        public bool EsSeleccion { get; set; }



    }

    public class ItemEvaluarLikerCriterios : ItemEvaluarLiker , IItemsEvaluar
    {
        public int IdItem { get; set; }
        public int Valor { get; set; }
        public bool EsSiNo { get; set; }
        public int Respuesta { get; set; }
        public List<Criterio> Criterios { get; set; }

    }

}