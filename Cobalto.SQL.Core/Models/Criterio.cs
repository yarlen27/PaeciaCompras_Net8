using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cobalto.SQL.Core.Models
{
    public class Criterio : BaseTable
    {

        public int Valor { get; set; }
        public string Descripcion { get; set; }
        public int IdItemEvaluarLiker { get; set; }


    }

    public class DocumentoSagrilaf:BaseTable
    {
        public string Archivo { get; set; }
        public string Nombre { get; set; }
        public DateTime Fecha { get; set; }

        public Guid ProveedorId { get; set; }

    }


}