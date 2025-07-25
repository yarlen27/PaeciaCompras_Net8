using Cobalto.Mongo.Core.BLL;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.BLL
{
    public class ClientBLL : BaseBLL<Client>
    {

        public ClientBLL(IConfiguration configuration, IHttpContextAccessor httpContext) : base(configuration, httpContext)
        {

        }
    }
}
