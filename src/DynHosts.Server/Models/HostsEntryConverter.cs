using System;
using System.Collections.Generic;
using System.Linq;

namespace DynHosts.Server.Models
{
    internal static class HostsEntryConverter
    {
        internal static HostsEntryDto ToDto(IEnumerable<HostsEntry> hostsEntries)
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

        internal static HostsEntryDto[] ToDtos(IEnumerable<HostsEntry> hostsEntries)
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

        internal static HostsEntry[] ToModels(HostsEntryDto dto)
        {
            if (dto.Host == null)
            {
                throw new ArgumentException($"{nameof(dto.Host)} cannot be null", nameof(dto));
            }

            if (dto.IpAddresses == null)
            {
                throw new ArgumentException($"{nameof(dto.IpAddresses)} cannot be null", nameof(dto));
            }

            if (dto.IpAddresses.Any(ip => ip == null))
            {
                throw new ArgumentException($"{nameof(dto.IpAddresses)} cannot contain a null value", nameof(dto));
            }

            HostsEntry[] models = dto.IpAddresses
                .Select(ip => new HostsEntry(dto.Host, ip))
                .ToArray();
            return models;
        }
    }
}
