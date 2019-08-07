using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeOlho.EventBus.Abstractions;
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
        public void Post([FromServices]IEventBus eventBus, [FromBody] string value)
        {
            eventBus.Publish<MessageTest>(new MessageTest(Guid.NewGuid().ToString()) { Testando = value });
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
