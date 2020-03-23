using System;
using System.Collections.Generic;

namespace JinCreek.Server.Common.Models
{
    public class LdapDeviceGroup : ILdap
    {
        public Guid ObjectGuid { get; set; }
        public string DistinguishedName { get; set; }
        public string Name { get; set; }
        public string ObjectClass { get; set; }

        public List<LdapDevice> DeviceList { get; set; }

        public void AddUserList(LdapDevice user)
        {
            DeviceList ??= new List<LdapDevice>();
            DeviceList.Add(user);
        }
    }
}
