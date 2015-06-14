using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Biggy.Core;
using Biggy.Data.Json;
using Cockerel.Models;
using System.ServiceModel.Channels;

namespace Cockerel.Controllers
{
    public class WakeOnLanController : ApiController
    {

        private static Lazy<BiggyList<MachineOutput>> machines
            = new Lazy<BiggyList<MachineOutput>>(InitialiseMachineStore, false);
        private static BiggyList<MachineOutput> InitialiseMachineStore()
        {
            return new BiggyList<MachineOutput>(
                                new JsonStore<MachineOutput>(Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data"), "Cockerel", "Machines"));
        }

        [Route("api/machines")]
        [HttpPost]
        public void Post(Machine machine)
        {
            if (machines.Value.Any(m => m.Name == machine.Name))
                PutStatus(machine.Name, machine.Status);
            else
                machines.Value.Add(MachineOutput.CreateFrom(machine));
        }

        [Route("api/machines-example")]
        [HttpGet]
        public IEnumerable<MachineOutput> GetExample()
        {
            return new[]
            {
                new MachineOutput
                {
                    Name = "1",
                    Status = MachineStatus.Online,
                    IPAddresses = new List<string> { "ip1", "ip2" },
                    LastUpdated = new DateTime(2005,2,3)
                },
                new MachineOutput
                {
                    Name = "2",
                    Status = MachineStatus.Offline,
                    IPAddresses = new List<string> { "ip12", "ipB" },
                    LastUpdated = new DateTime(2006,2,3)
                }
            };
        }

        [Route("api/machines")]
        [HttpGet]
        public IEnumerable<MachineOutput> Get()
        {
            return machines.Value;
        }

        [Route("api/machines/{machineName}")]
        [HttpGet]
        public MachineOutput Get(String machineName)
        {
            var machine = machines.Value.SingleOrDefault(x => x.Name == machineName);

            if (machine == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            return machine;
        }

        [Route("api/machines/{machineName}/status")]
        [HttpGet]
        public MachineStatus GetStatus(String machineName)
        {
            return Get(machineName).Status;
        }


        [Route("api/machines/{machineName}/status")]
        [HttpPut]
        public void PutStatus(String machineName, [FromBody]MachineStatus status)
        {
            MachineOutput machine = Get(machineName);
            String ip = GetClientIp(Request);
            if(!String.IsNullOrEmpty(ip))
            {
                machine.IPAddresses.Add(ip);
                machine.IPAddresses = machine.IPAddresses.Distinct().ToList();
            }
            machine.LastUpdated = DateTime.UtcNow;
            machine.Status = status;

            machines.Value.Update(machine);
        }


        [Route("api/machines/{machineName}")]
        [HttpDelete]
        public void Delete(String machineName)
        {
            machines.Value.Remove(Get(machineName));
        }

        private const string HttpContext = "MS_HttpContext";
        private const string RemoteEndpointMessage =
            "System.ServiceModel.Channels.RemoteEndpointMessageProperty";

        private string GetClientIp(HttpRequestMessage request)
        {
            // Web-hosting
            if (request.Properties.ContainsKey(HttpContext))
            {
                HttpContextWrapper ctx =
                    (HttpContextWrapper)request.Properties[HttpContext];
                if (ctx != null)
                {
                    return ctx.Request.UserHostAddress;
                }
            }

            // Self-hosting
            if (request.Properties.ContainsKey(RemoteEndpointMessage))
            {
                RemoteEndpointMessageProperty remoteEndpoint =
                    (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessage];
                if (remoteEndpoint != null)
                {
                    return remoteEndpoint.Address;
                }
            }

            return null;
        }
    }
}
