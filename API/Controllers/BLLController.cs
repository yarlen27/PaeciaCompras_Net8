using Cobalto.Mongo.Core.BLL;
using Cobalto.Mongo.Core.DTO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class BLLController<T>:Controller where T : CollectionDTO
    {

        public BaseBLL<T> BLL { get; set; }
        public BLLController(BaseBLL<T> bll)
        {
            this.BLL = bll;
        }


        // POST api/<controller>
        [HttpPost]
        public virtual async Task<T> PostAsync([FromBody] T entity)
        {

            var insert = await this.BLL.Insert(entity);
            if (insert.id != null)
            {
                return insert;
            }
            else
            {
                return null;
            }
        }
        // GET: api/<controller>
        [HttpGet]
        public virtual async Task<List<T>> Get()
        {
            return await this.BLL.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<T> GetById([FromRoute] Guid id)
        {
            return await this.BLL.GetById(id);
        }

        

        [HttpPut]
        public virtual async Task<bool> Put([FromBody] T entity)
        {
            await this.BLL.Update(entity);
            return true;
        }

        [HttpDelete("{id}")]
        public async Task<bool> Delete([FromRoute]Guid Id)
        {
            var item = await this.BLL.GetById(Id);
            item.erased = true;
            await this.BLL.Update(item);
            return true;
        }
    }
}
