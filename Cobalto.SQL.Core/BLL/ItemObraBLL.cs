using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Models;

namespace Cobalto.SQL.Core.BLL
{
    public class ItemObraBLL : BaseBLL<ItemObra>
    {
        public ItemObraBLL(IConfiguration configuration) : base(configuration)
        {
        }

        //internal void ActualiarRegistros(List<ItemObra> registrosActualizar)
        //{

        //    foreach (var item in registrosActualizar)
        //    {
        //        var itemExistente = this.Filtrar(new { Item = item.Item, Borrado = false }).FirstOrDefault();

        //        if (itemExistente != null)
        //        {



        //            itemExistente.Descripcion = item.Descripcion;
        //            itemExistente.Cantidad = item.Cantidad;
        //            itemExistente.Moneda = item.Moneda;
        //            itemExistente.ValorUnitario = item.ValorUnitario;
        //            itemExistente.ValorTotal = item.ValorTotal;
        //            itemExistente.Norma = item.Norma;
        //            itemExistente.UnidadDeMedida = item.UnidadDeMedida;
        //            itemExistente.Fecha = DateTime.Now;
        //            itemExistente.CantidadInicial = item.Cantidad;
        //            this.Actualizar(itemExistente);

        //        }
        //    }

        //}

        //internal IEnumerable<ItemObra> ObternerItemsPorProyecto(Guid idProyecto)
        //{
        //    return this.Filtrar(new { IdProyecto = idProyecto });
        //}
    }
}