using System;
using System.Collections.Generic;
using System.Text;

namespace JinCreek.Server.Common.Models
{
    public class LdapUserGroup : ILdap
    {
        public Guid ObjectGuid { get; set; }
        public string DistinguishedName { get; set; }
        public string Name { get; set; }
        public string ObjectClass { get; set; }

        public List<LdapUser> UserList { get; set; } = new List<LdapUser>();

        public void AddUserList(LdapUser user)
        {
            UserList.Add(user);
        }
    }
}
