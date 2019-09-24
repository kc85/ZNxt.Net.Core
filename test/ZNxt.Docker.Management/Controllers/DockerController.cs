using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ZNxt.Docker.Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DockerController : ControllerBase
    {
        [HttpGet]
        public async Task<List<string>> Get()
        {
            DockerClient client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();

            var data = new List<string>();


            IList<ContainerListResponse> containers = await client.Containers.ListContainersAsync(
    new ContainersListParameters()
    {
        Limit = 10,
    });
            foreach (var item in containers)
            {
                data.Add($"{item.Names.First()} -- {item.State}");
            }
            return data;
        }
        [HttpPut]
        public async Task<string> Put(string containerName)
        {
            try
            {
                DockerClient client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();


                await client.Containers.RestartContainerAsync(containerName, new ContainerRestartParameters() { WaitBeforeKillSeconds = 10 });

                IList<ContainerListResponse> containers = await client.Containers.ListContainersAsync(
new ContainersListParameters()
{
    Limit = 10,
});
                var cont = containers.FirstOrDefault(f => f.State == "running" && f.Names.FirstOrDefault() == $"/{containerName}");
                return cont.ID;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
    }
}