using System;

namespace JinCreek.Server.Common.Models
{
    public class LdapDomain : ILdap
    {
        public Guid ObjectGuid { get; set; }
        public string DistinguishedName { get; set; }
        public string Name { get; set; }
        public string ObjectClass { get; set; }
    }
}
