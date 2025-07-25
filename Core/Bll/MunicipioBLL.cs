using Cobalto.Mongo.Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.BLL
{
    public class MunicipioBLL : BaseBLL<Municipio>
    {
        public MunicipioBLL(IConfiguration configuration, IHttpContextAccessor httpContext) : base(configuration, httpContext)
        {

        }
    }
}
