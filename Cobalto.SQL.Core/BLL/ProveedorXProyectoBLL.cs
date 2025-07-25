using System;
using System.Collections.Generic;
using System.Text;
using Cobalto.SQL.Core.Models;
using Microsoft.Extensions.Configuration;

namespace Cobalto.SQL.Core.BLL
{
    public class ProveedorXProyectoBLL : BaseBLL<ProveedorXProyecto>
    {
        public ProveedorXProyectoBLL(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
