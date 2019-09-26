using DynHosts.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynHosts.Server.Services
{
    internal interface IHostsManager
    {
        HostsEntryDto GetHostEntries(string hostName);
        HostsEntryDto SetHostEntries(string hostName, params string[] ipAddresses);
        HostsEntryDto SetHostEntries(HostsEntryDto hostsEntry);
    }
}
