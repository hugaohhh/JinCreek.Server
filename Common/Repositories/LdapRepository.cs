using JinCreek.Server.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JinCreek.Server.Common.Repositories
{
    public class LdapRepository
    {
        private LdapContext _ldapContext;

        public LdapRepository(LdapContext ldapContext)
        {
            _ldapContext = ldapContext;
        }


        public IEnumerable<ILdap> GetDomain(string domainName)
        {
            return _ldapContext.GetDomainSearchResults(domainName);
        }

        public IEnumerable<ILdap> GetDeviceGroupWithDevices(string domainName, string[] groupGuIdArray)
        {
            var deviceGroupSearchResults = _ldapContext.GetDeviceGroupSearchResults(domainName, groupGuIdArray);
            var groupSearchResults = deviceGroupSearchResults.ToList();
            var groupDistinguishedNameList = groupSearchResults.Select(d => d.DistinguishedName).ToList();
            var deviceSearchResults = _ldapContext.GetDeviceSearchResults(domainName, groupDistinguishedNameList);

            var deviceGroupWithUsers = groupSearchResults.Select(dg =>
            {
                var distinguishedName = ((LdapDeviceGroup)dg).DistinguishedName;
                List<LdapDevice> list = new List<LdapDevice>();
                foreach (var ldap in deviceSearchResults)
                {
                    var d = (LdapDevice)ldap;
                    if (d.MemberOf != null && d.MemberOf.Equals(distinguishedName, StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(d);
                    }
                }
                ((LdapDeviceGroup)dg).DeviceList = list;
                return dg;
            }).ToList();

            return deviceGroupWithUsers;
        }

        public IEnumerable<ILdap> GetUserGroupWithUsers(string domainName, string[] groupGuIdArray)
        {
            var userGroupSearchResults = _ldapContext.GetUserGroupSearchResults(domainName, groupGuIdArray);
            var groupSearchResults = userGroupSearchResults.ToList();
            var groupDistinguishedNameList = groupSearchResults.Select(d => d.DistinguishedName).ToList();
            var userSearchResults = _ldapContext.GetUserSearchResults(domainName, groupDistinguishedNameList);

            var userGroupWithUsers = groupSearchResults.Select(ug =>
            {
                var distinguishedName = ((LdapUserGroup)ug).DistinguishedName;
                List<LdapUser> list = new List<LdapUser>();
                foreach (var ldap in userSearchResults)
                {
                    var u = (LdapUser)ldap;
                    if (u.MemberOf != null && u.MemberOf.Equals(distinguishedName, StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(u);
                    }
                }
                ((LdapUserGroup)ug).UserList = list;
                return ug;
            }).ToList();

            return userGroupWithUsers;
        }
    }
}
