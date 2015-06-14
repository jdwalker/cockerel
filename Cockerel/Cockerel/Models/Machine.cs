using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Cockerel.Models
{
    public class Machine
    {
        public String Name { get; set; }
       
        public MachineStatus Status { get; set; }

        public override bool Equals(object obj)
        {
            var machine2 = obj as Machine;
            return machine2 != null && Name == machine2.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

    }

    public class MachineOutput : Machine
    {
        

        public static MachineOutput CreateFrom(Machine machine)
        {
            return new MachineOutput
            {
                Name = machine.Name,
                Status = machine.Status
            };
        }

        public List<String> IPAddresses { get; set; } = new List<String>();
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;

    }

    public enum MachineStatus
    {
        Offline,
        StartupRequested,
        StartupOngoing,
        Online
    }
}