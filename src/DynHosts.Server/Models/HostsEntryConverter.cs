using System;
using System.Collections.Generic;
using System.Linq;

namespace DynHosts.Server.Models
{
    internal static class HostsEntryConverter
    {
        public static HostsEntryDto ToDto(IEnumerable<HostsEntry> hostsEntries)
        {
            HostsEntry[] hostsEntryArray = hostsEntries.ToArray();

            // ensure that the hosts entries are all for the same host name
            if (hostsEntryArray.Select(he => he.Host).Distinct(StringComparer.OrdinalIgnoreCase).Count() != 1)
            {
                throw new ArgumentException("expected one unique host name", nameof(hostsEntries));
            }

            var result = new HostsEntryDto
            {
                Host = hostsEntryArray[0].Host,
                IpAddresses = hostsEntryArray.Select(he => he.IpAddress.ToString()).ToArray()
            };

            return result;
        }

        public static HostsEntryDto[] ToDtos(IEnumerable<HostsEntry> hostsEntries)
        {
            HostsEntryDto[] results = hostsEntries
                .GroupBy(he => he.Host, StringComparer.OrdinalIgnoreCase)
                .Select(eg => new HostsEntryDto
                {
                    Host = eg.Key,
                    IpAddresses = eg.Select(he => he.IpAddress.ToString()).ToArray()
                })
                .ToArray();

            return results;
        }
    }
}
