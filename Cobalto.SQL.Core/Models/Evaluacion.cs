using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cobalto.SQL.Core.Models
{
    public class Evaluacion : BaseTable
    {

        public bool EsPeriodica { get; set; }
        public bool EsSeleccion { get; set; }
        public DateTime Fecha { get; set; }
        public string Evaluador { get; set; }
        public string Observaciones { get; set; }
        public int IdProveedor { get; set; }
        public double Puntaje { get; set; }
        public bool Calidad { get; set; }
        public bool SST { get; set; }
        public bool Ambiental { get; set; }

        public Guid? IdProyecto { get; set; }


    }
    public class EvaluacionVacia:Evaluacion
    {

        public List<IItemsEvaluar> PreguntasSST { get; set; }
        public List<IItemsEvaluar> PreguntasAmbiental { get; set; }
        public List<IItemsEvaluar> PreguntasCalidad { get; set; }

        public static EvaluacionVacia ConvertObject(Evaluacion M)
        {
            // Serialize the original object to json
            // Desarialize the json object to the new type 
            var obj = JsonConvert.DeserializeObject<EvaluacionVacia>(JsonConvert.SerializeObject(M));
            return obj;
        }
    }
    public class EvaluacionRealizada : Evaluacion
    {
        public List<ItemEvaluarLikerCriterios> PreguntasSST { get; set; }
        public List<ItemEvaluarLikerCriterios> PreguntasAmbiental { get; set; }
        public List<ItemEvaluarLikerCriterios> PreguntasCalidad { get; set; }

      

    }

    public class FiltroEvaluacion
    {

        public DateTime FechaInicial { get; set; }
        public DateTime FechaFinal { get; set; }

        public List<Guid> IdsProveedores { get; set; }
    }

    public class RegistroReporteEvaluacion
    {
        public DateTime FechaEvaluacion { get; set; }

        public string Proveedor { get; set; }

        public string NIT { get; set; }

        public string Evaluador { get; set; }



        public int Id { get; set; }
        public double PuntajeEvaluacionAmbiental { get;  set; }
        public double PuntajeEvaluacionSST { get;  set; }
        public double PuntajeEvaluacionCalidad { get;  set; }
        public string TipoEvaluacion { get;  set; }
    }

    public class RegistroReporteConsolidado
    {

        public string Proveedor { get; set; }

        public string NIT { get; set; }



        public double PuntajeSeleccionCalidad { get; set; }
        public double PuntajeSeleccionAmbiental { get; set; }
        public double PuntajeSeleccionSST { get; set; }
        public double PuntajeEvaluacionAmbiental { get; set; }
        public double PuntajeEvaluacionSST { get; set; }
        public double PuntajeEvaluacionCalidad { get; set; }

        public DateTime FechaEvaluacionSeleccion { get; set; }
        public DateTime FechaUltimaEvaluacion { get; set; }

        public Guid Id { get; set; }

    }

}