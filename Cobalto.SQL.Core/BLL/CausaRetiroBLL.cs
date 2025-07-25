using Cobalto.SQL.Core.Models;
using Microsoft.Extensions.Configuration;

namespace Cobalto.SQL.Core.BLL
{
    public class CausaRetiroBLL : BaseBLL<CausaRetiro>
    {
        public CausaRetiroBLL(IConfiguration configuration) : base(configuration)
        {
        }
    }



    public class AprobacionBLL : BaseBLL<Aprobacion>
    {
        public AprobacionBLL(IConfiguration configuration) : base(configuration)
        {
        }
    }



}
