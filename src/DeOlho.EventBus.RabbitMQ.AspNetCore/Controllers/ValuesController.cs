using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeOlho.EventBus.Abstractions;
using DeOlho.EventBus.EventSourcing;
using Microsoft.AspNetCore.Mvc;

namespace DeOlho.EventBus.RabbitMQ.AspNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromServices]IEventSourcingService eventSourcingService, [FromServices]EventSourcingDbContext dbContext, [FromBody] string value, CancellationToken cancellationToken)
        {
            using(var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                await eventSourcingService.SaveEventLogAsync(new MessageTest(Guid.NewGuid().ToString()) { Testando = value }, transaction);
                transaction.Commit();
            }
            
            //eventBus.Publish<MessageTest>(new MessageTest(Guid.NewGuid().ToString()) { Testando = value });
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
