using System;
using System.Collections.Generic;

namespace Cobalto.SQL.Core.Models
{
    public class IngresoItemSinOC
    {

        public Guid IdInsumo { get; set; }
        public string NumeroPedido { get; set; }


        public double Cantidad { get; set; }

        public string Usuario { get; set; }

        public string Observaciones { get; set; }

        public Guid IdProyecto { get; set; }
        public Guid IdProveedor { get; set; }
        public double ValorUnitario { get; set; }
        public Guid IdCategoria { get;  set; }
    }

    public class AprobacionMovimiento {

        public string Usuario { get; set; }

        public string Observaciones { get; set; }

        public int IdMovimiento { get; set; }


    }

    public class FiltroKardex
    {

        public DateTime? FechaInicial { get; set; }
        public DateTime? FechaFinal { get; set; }

        public Guid? IdProyecto { get; set; }

        public int? IdFrente { get; set; }



    }




    public class Aprobacion:BaseTable
    {

        public string Usuario { get; set; }
        public DateTime Fecha { get; set; }


        public int IdMovimiento { get; set; }



    }

    public class Item
    {
        public Guid IdInsumo { get; set; }
        public int Cantidad { get; set; }
    }

    public class ReingresoDevolucion
    {
        public List<Item> Items { get; set; }
        public string Usuario { get; set; }
        public Guid IdProyecto { get; set; }
        public int IdFrente { get; set; }
    }


}
