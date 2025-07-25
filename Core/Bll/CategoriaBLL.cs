using Cobalto.Mongo.Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Bll
{
    public class CategoriaBLL : BaseBLL<Categoria>
    {

        public CategoriaBLL(IConfiguration configuration, IHttpContextAccessor httpContext) : base(configuration, httpContext)
        {

        }
    }
}
