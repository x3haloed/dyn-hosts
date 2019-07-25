using DynHosts.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DynHosts.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HostsController : ControllerBase
    {
        // GET api/hosts/COMPUTER1
        [HttpGet("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<HostsEntry> Get([FromRoute] string name)
        {
            return Ok(new HostsEntry { Host = name, IpAddress = "" });
        }

        // PUT api/hosts/COMPUTER1
        [HttpPut("{name}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<HostsEntry> Put([FromRoute] string name, [FromBody] HostsEntry hostsEntry)
        {
            if (name != hostsEntry.Host)
            {
                return BadRequest();
            }

            return CreatedAtAction(nameof(Get), new { name = name }, hostsEntry);
        }
    }
}