using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cobalto.SQL.Core.Models;

namespace Cobalto.SQL.Core.BLL
{
    public class ItemEvaluarLikerBLL : BaseBLL<ItemEvaluarLiker>
    {
        private CriterioBLL _criterioBLL;

        public ItemEvaluarLikerBLL(IConfiguration configuration, CriterioBLL criterioBLL) : base(configuration)
        {
            this._criterioBLL = criterioBLL;
        }

        public  int? InsertarItemCriterios(ItemsLiker entidad)
        {
            var id = this.Insertar(new ItemEvaluarLiker
            {
                Aspecto = entidad.Aspecto,
                Ambiental = entidad.Ambiental,
                Calidad = entidad.Calidad,
                SST = entidad.SST, 
                EsSeleccion = entidad.EsSeleccion

            });

            foreach (var item in entidad.Detalles)
            {
                item.IdItemEvaluarLiker = id.Value;
                this._criterioBLL.Insertar(item);
            }

            return id;
        }
    }
}