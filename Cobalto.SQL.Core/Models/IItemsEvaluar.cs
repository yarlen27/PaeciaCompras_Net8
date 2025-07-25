using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalto.SQL.Core.Models
{
    public interface IItemsEvaluar
    {
        int Id { get; set; }

        string Aspecto { get; set; }
        bool Calidad { get; set; }
        bool SST { get; set; }
        bool Ambiental { get; set; }
        bool EsSiNo { get; set; }
        int IdItem { get; set; }
        int Respuesta { get; set; }
        
    }
}
