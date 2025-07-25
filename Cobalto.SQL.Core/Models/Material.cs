using Dapper;
using DocumentFormat.OpenXml.Vml;
using System;

namespace Cobalto.SQL.Core.Models
{
    public class Material : BaseTable
    {

        public Guid IdInsumo { get; set; }
        public Guid IdProyecto { get; set; }
        public Guid IdCategoria { get; set; }
        public string Referencia { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }

        [NotMapped]
        public int IdFrente { get; set; }




    }


    public class CantidadActual : Material
    {

        public double Cantidad { get; set; }

        public DateTime Fecha { get; set; }
        public double? ValorUnitario { get; set; }
        public double? ValorTotal { get; set; }
        public double? Iva { get; set; }
        public double? ValorTotalConIva { get; set; }

        public string Frente { get; set; }

    }

    public class CantidadActualFrente : Material
    {

        public double Cantidad { get; set; }

        public DateTime Fecha { get; set; }
        public double? ValorUnitario { get; set; }
        public double? ValorTotal { get; set; }
        public double? Iva { get; set; }
        public double? ValorTotalConIva { get; set; }

        public string Frente { get; set; }
        public int? IdFrente { get; set; }
        

    }

}


