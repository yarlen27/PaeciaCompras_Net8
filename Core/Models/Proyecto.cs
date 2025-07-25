using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    [CollectionName("proyecto")]
    public class Proyecto : CollectionDTO
    {
        public string nombre { get; set; }
        public string objeto { get; set; }
        public string empresa { get; set; }
        public string nit { get; set; }
        public string telefono { get; set; }
        public string contratante { get; set; }
        public string representanteLegal { get; set; }
        public string direccion { get; set; }
        public string direccionFacturacion { get; set; }
        public string emailOrdenes { get; set; }

        public string departamento { get; set; }
        public string municipio { get; set; }
        public string directorAlmacen { get; set; }
        public string directorProyecto { get; set; }
        public string almacenista { get; set; }
        public DateTime inicio { get; set; }
        public DateTime? fin { get; set; }
        public double limitePresupuesto { get; set; }
        public string diaPedido { get; set; }
        public string instanciasAprobacion { get; set; }
        public string segundoAprobador { get; set; }
        public int consecutivo { get; set; }
        public int consecutivoServicio { get; set; }



        public string fechaDatosContables { get; set; }
        public int consecutivoDatosCotables { get; set; }

        public List<Categoria> categorias { get; set; }
        public bool? Temporal { get; set; }

        public string numeroContrato { get; set; }
        public string interventoria { get; set; }
        public bool? usaArchivoCargaItems { get; set; }

        public string Codigo { get; set; }

    }

    public class Empresa
    {

        public string empresa { get; set; }
        public string nit { get; set; }
    }
}
