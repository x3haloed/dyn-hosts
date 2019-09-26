using System.Net;

namespace DynHosts.Server.Models
{
    internal class HostsEntry
    {
        public HostsEntry(string host, IPAddress ipAddress)
        {
            Host = host;
            IpAddress = ipAddress;
        }

        public HostsEntry(string host, string ipAddress)
            : this(host, IPAddress.Parse(ipAddress)) { }

        public string Host { get; set; }
        public IPAddress IpAddress { get; set; }
    }
}
