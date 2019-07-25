using Microsoft.AspNetCore.Mvc;

namespace DynHosts.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HostsController : ControllerBase
    {
        // GET api/hosts/COMPUTER1
        [HttpGet("{name}")]
        public ActionResult<string> Get(string name)
        {
            return Ok(new { host = name, ipAddress = "" });
        }

        // PUT api/hosts/COMPUTER1
        [HttpPut("{name}")]
        public ActionResult Put(string name, [FromBody] string ipAddress)
        {
            return CreatedAtAction(nameof(Get), new { host = name, ipAddress = ipAddress });
        }
    }
}