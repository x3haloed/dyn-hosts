using DynHosts.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DynHosts.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HostsController : ControllerBase
    {
        // GET api/hosts/COMPUTER1
        [HttpGet("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<HostsEntry>> Get([FromRoute] string name)
        {
            using (var fileStream = System.IO.File.Open(Program.PathToHostsFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                string fileText;

                var utf8EncodingWithoutBom = new UTF8Encoding(false);

                using (var sr = new StreamReader(fileStream, utf8EncodingWithoutBom, false, 4096, true))
                {
                    fileText = await sr.ReadToEndAsync();
                }

                var hostRegex = new Regex(@"^\s*([^#][^\s^#]+)\s*([^#][^\s^#]+)", RegexOptions.Multiline);

                var ipAddresses = hostRegex.Matches(fileText)
                    .Select(m => m.Captures[0].Value)
                    .ToArray();

                var result = new HostsEntry { Host = name, IpAddresses = ipAddresses };

                return Ok(result);
            }
        }

        // PUT api/hosts/COMPUTER1
        [HttpPut("{name}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<HostsEntry>> Put([FromRoute] string name, [FromBody] HostsEntry hostsEntry)
        {
            if (name != hostsEntry.Host)
            {
                return BadRequest();
            }

            using (var fileStream = System.IO.File.Open(Program.PathToHostsFile, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                string[] fileLines;

                var utf8EncodingWithoutBom = new UTF8Encoding(false);

                using (var sr = new StreamReader(fileStream, utf8EncodingWithoutBom, false, 4096, true))
                {
                    fileLines = (await sr.ReadToEndAsync())
                        .Split('\n')
                        .Select(s => s.Trim('\r'))
                        .ToArray();
                }

                //search through each line in the HOSTS file to find the host to edit
                var hostRegex = new Regex(@"^\s*([^#][^\s^#]+)\s*" + name, RegexOptions.IgnoreCase);
                // var hostRegex = new Regex(@"^\s*([^#][^\s^#]+)\s*([^#][^\s^#]+)"); // captures both IP and hostname

                //remove all matched host lines
                fileLines = fileLines.Where(l => !hostRegex.IsMatch(l)).ToArray();

                //add new lines
                fileLines = fileLines
                    .Concat(hostsEntry.IpAddresses.Select(ip => $"\t{ip}\t{name}"))
                    .ToArray();

                //write out file data
                fileStream.Position = 0;
                using (var sw = new StreamWriter(fileStream, utf8EncodingWithoutBom, 4096, false))
                {
                    await sw.WriteAsync(string.Join(Environment.NewLine, fileLines) + Environment.NewLine);
                    await fileStream.FlushAsync();
                }
            }

            return CreatedAtAction(nameof(Get), new { name = name }, hostsEntry);
        }
    }
}