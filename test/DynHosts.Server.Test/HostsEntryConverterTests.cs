using DynHosts.Server.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Xunit;

namespace DynHosts.Server.Test
{
    public class HostsEntryConverterTests
    {
        [Fact]
        public void ToDtoCreatesADto()
        {
            HostsEntry model = GetTestModel();
            HostsEntryDto dto = HostsEntryConverter.ToDto(new[] { model });
            Assert.Equal(model.Host, dto.Host);
            Assert.Equal(model.IpAddress, IPAddress.Parse(dto.IpAddresses[0]));
        }

        [Fact]
        public void ToDtosCreatesDtos()
        {
            HostsEntry model = GetTestModel();
            HostsEntryDto[] dtos = HostsEntryConverter.ToDtos(new[] { model, model });
            Assert.Equal(model.Host, dtos[0].Host);
            Assert.Equal(model.IpAddress, IPAddress.Parse(dtos[0].IpAddresses[0]));
            Assert.Equal(model.IpAddress, IPAddress.Parse(dtos[0].IpAddresses[1]));
        }

        [Fact]
        public void ToModelsCreatesModels()
        {
            HostsEntryDto dto = GetTestDto();
            HostsEntry[] models = HostsEntryConverter.ToModels(dto);
            Assert.Equal(dto.Host, models[0].Host);
            Assert.Equal(dto.IpAddresses[0], models[0].IpAddress.ToString());
        }

        private HostsEntryDto GetTestDto() => new HostsEntryDto
        {
            Host = "localhost",
            IpAddresses = new [] { "127.0.0.1" },
        };

        private HostsEntry GetTestModel() => new HostsEntry("locahost", "127.0.0.1");
    }
}
