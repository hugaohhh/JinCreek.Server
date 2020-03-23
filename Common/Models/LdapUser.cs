﻿using System;

namespace JinCreek.Server.Common.Models
{
    public class LdapUser : ILdap
    {
        public Guid ObjectGuid { get; set; }
        public string DistinguishedName { get; set; }
        public string Name { get; set; }
        public string ObjectClass { get; set; }
        public string MemberOf { get; set; }
        public string UserPrincipalName { get; set; }
    }
}
