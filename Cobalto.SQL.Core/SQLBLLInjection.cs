using Cobalto.SQL.Core.BLL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cobalto.SQL.Core
{
    public static class SQLBLLInjection
    {
        public static void AddSQLBLL(this IServiceCollection services, IConfiguration configuration)
        {

            var allProviderTypes = System.Reflection.Assembly.GetExecutingAssembly()
               .GetTypes().Where(t => t.Namespace != null && t.Namespace.Contains("Cobalto.SQL.Core.BLL")); 

            foreach (var intfc in allProviderTypes)
            {        

                services.AddTransient(intfc);

            }

        }
    }
}
