using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cobalto.SQL.Core.Models;

namespace Cobalto.SQL.Core.BLL
{
    public class ItemEvaluarSiNoBLL : BaseBLL<ItemEvaluarSiNo>
    {
        public ItemEvaluarSiNoBLL(IConfiguration configuration) : base(configuration)
        {
        }
    }
}