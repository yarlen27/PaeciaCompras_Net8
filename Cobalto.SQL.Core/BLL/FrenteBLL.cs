using Cobalto.SQL.Core.Models;
using Microsoft.Extensions.Configuration;

namespace Cobalto.SQL.Core.BLL
{
    public class FrenteBLL : BaseBLL<Frente>
    {
        public FrenteBLL(IConfiguration configuration) : base(configuration)
        {
        }
    }



}
