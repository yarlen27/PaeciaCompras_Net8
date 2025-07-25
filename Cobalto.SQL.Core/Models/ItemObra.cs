using Cobalto.SQL.Core.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    [Table("ItemObra")]
    public class ItemObra : BaseTable
    {

        public string Item { get; set; }
        public string Descripcion { get; set; }
        public double Cantidad { get; set; }
        public string UnidadEmpaque { get; set; }
        public string Moneda { get; set; }
        public double ValorUnitario { get; set; }
        public double ValorTotal { get; set; }
        public Guid IdProyecto { get; set; }
        public string Norma { get; set; }
        public string UnidadDeMedida { get; set; }
        public string IdUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime? FechaInicial { get; set; }
        [IgnoreUpdate]
        public virtual double? CantidadInicial { get; set; }
    }

    //public class Modificacion
    //{

    //    public string Nombre { get; set; }
    //    public DateTime? Fecha { get; set; }
    //    public List<StockItemObra> Items { get; set; }
    //}
}