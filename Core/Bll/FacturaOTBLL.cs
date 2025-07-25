using Core.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Bll
{
    public class FacturaOTBLL
    {
        OrdenTrabajoBLL otBLl;

        public FacturaOTBLL(OrdenTrabajoBLL _otBLl)
        {
            this.otBLl = _otBLl;
        }

        public async Task<List<OrdenTrabajo>> OrdenesPorFactura(Guid idFactura)
        {

            var filter = Builders<OrdenTrabajo>.Filter.Eq("idFactura", idFactura);

            return await this.otBLl.GetByProterty(filter);

        }
    }
}

