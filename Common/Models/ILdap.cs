using System;

namespace JinCreek.Server.Common.Models
{
    public interface ILdap
    {
        public Guid ObjectGuid { get; set; }
        public string DistinguishedName { get; set; }
        public string Name { get; set; }
        public string ObjectClass { get; set; }
    }
}
