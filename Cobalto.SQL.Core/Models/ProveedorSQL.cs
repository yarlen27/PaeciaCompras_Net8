using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Cobalto.SQL.Core.Models
{
    [Table("Proveedor")]
    public class ProveedorSQL : BaseTable
    {

        public Guid IdCompras { get; set; }
        public string Nombre { get; set; }
        public string Nit { get; set; }
        public string Correo { get; set; }
        public int? IdTipologia { get; set; }

        public string userId { get; set; }
        public string UserName { get; internal set; }
    }


}