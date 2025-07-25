using Cobalto.Mongo.Core.BLL;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.BLL
{
    public class RemisionBLL : BaseBLL<Remision>
    {

        public RemisionBLL(IConfiguration configuration, IHttpContextAccessor httpContext) : base(configuration, httpContext)
        {

        }
    }
}
