using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalto.SQL.Core.Models
{
    public class IngresoDesdeOC: IngresoSinOC
    {
        public int IdOrdenCompra { get;  set; }
        public string   Remision { get;  set; }


    }


    public class IngresoSinOC
    {
        public IEnumerable<IngresoInventario> Items { get; set; }
        public string Usuario { get; set; }
        public Guid IdProyecto { get; set; }

        public Guid IdProveedor { get; set; }


    }


    public class SalidaDesdeFrente
    {
        public IEnumerable<Salida> Items { get; set; }
        public string Usuario { get; set; }
       
        public Guid IdProyecto { get; set; }

        public int IdFrente { get; set; }


    }

    public class Salida {

        public Guid IdInsumo { get; set; }
        public double Cantidad { get; set; }

        public int IdItemContrato { get; set; }


    }



    public class SalidaSinFrente
    {

        public Guid IdInsumo { get; set; }
        public double Cantidad { get; set; }
        public Guid IdProyecto { get; set; }
        public string Usuario { get; set; }

        public string  Causa { get; set; }
        public string Observaciones { get; set; }
        public int? IdFrente { get;  set; }
    }





}
