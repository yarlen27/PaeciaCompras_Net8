using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{

    [CollectionName("referencia")]

    public class Referencia : CollectionDTO
    {

        public string codigo { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string valorUnitario { get; set; }
        public string unidad { get; set; }
        public List<Categoria> categorias { get; set; }

        public double? ProcentajeIva { get; set; }


    }
}
