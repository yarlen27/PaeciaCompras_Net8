using Cobalto.SQL.Core.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Cobalto.SQL.Core.BLL
{
    public class ResponsableBLL : BaseBLL<Responsable>
    {
        public ResponsableBLL(IConfiguration configuration) : base(configuration)
        {
        }

       
    }



}
