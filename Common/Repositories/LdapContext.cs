using JinCreek.Server.Common.Models;
using Novell.Directory.Ldap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JinCreek.Server.Common.Repositories
{
    public abstract class SearchQuery
    {
        protected readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        protected string BaseString { get; set; }
        protected int Scope { get; set; }
        protected string Filter { get; set; }
        protected string[] Attrs { get; set; }
        protected bool TypesOnly { get; set; }
        protected LdapSearchConstraints Constraints { get; set; }

        public abstract IEnumerable<ILdap> Search(LdapConnection ldapConnection);

        //protected void PrintDebugLdapEntryAttribute(LdapEntry entry)
        //{
        //    var name = entry.getAttribute("Name").StringValue;
        //    Console.WriteLine();
        //    foreach (LdapAttribute attribute in entry.getAttributeSet())
        //    {
        //        Console.WriteLine($"[{name}]{attribute.Name}={attribute.StringValue}");
        //    }
        //    Console.WriteLine();
        //}
    }

    public class LdapEntryEnumrator : IEnumerable<LdapEntry>
    {
        private LdapSearchResults _ldapSearchResults;

        public LdapEntryEnumrator(LdapSearchResults ldapSearchResults)
        {
            _ldapSearchResults = ldapSearchResults;
        }


        public IEnumerator<LdapEntry> GetEnumerator()
        {
            //List<LdapEntry> list = new List<LdapEntry>();
            while (_ldapSearchResults.HasMore())
            {
                LdapEntry entry;
                try
                {
                    entry = _ldapSearchResults.Next();
                    //Console.WriteLine($"#######{entry.getAttribute("Name").StringValue}");
                }
                catch (LdapException)
                {
                    //IGNORE (https://stackoverflow.com/questions/46052873/a-list-of-all-users-ldap-referral-error-ldapreferralexception, https://www.novell.com/documentation/developer/ldapcsharp/?page=/documentation/developer/ldapcsharp/cnet/data/bow8fjp.html)
                    //Console.WriteLine($"{e.Message}");
                    //_logger.Warn(e.Message, e);
                    continue;
                }
                yield return entry;
                //list.Add(entry);
            }
            //return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public class DomainSearchQuery : SearchQuery
    {
        private string DomainName { get; }

        public DomainSearchQuery(string domainName)
        {
            this.DomainName = domainName;
        }

        public override IEnumerable<ILdap> Search(LdapConnection ldapConnection)
        {
            BaseString = "dc=" + string.Join(",dc=", DomainName.Split("."));
            Scope = LdapConnection.SCOPE_SUB;
            Filter = "(objectClass=domain)";
            Attrs = new string[] { };
            TypesOnly = false;
            Constraints = null;

            var searchResults = ldapConnection.Search(BaseString, Scope, Filter, Attrs, TypesOnly, Constraints);

            //foreach (var entry in new LdapEntryEnumrator(searchResults))
            //{
            //    foreach (LdapAttribute attr in entry.getAttributeSet())
            //    {
            //        Console.WriteLine($">>> {attr.Name}={attr.StringValue}");
            //    }
            //}

            var enumerable = new LdapEntryEnumrator(searchResults)
                .Where(entry => entry != null)
                .Select(entry =>
                {
                    var distinguishedName = entry.getAttribute("distinguishedName")?.StringValue;
                    var domainName = distinguishedName?.Replace("DC=", "").Replace(",", ".");

                    return new LdapDomain()
                    {
                        ObjectGuid = new Guid((byte[]) (Array) entry.getAttribute("objectGUID")?.ByteValue ?? throw new InvalidOperationException()),
                        DistinguishedName = distinguishedName,
                        //Name = entry.getAttribute("name")?.StringValue,
                        Name = domainName,
                        ObjectClass = entry.getAttribute("objectClass")?.StringValue
                    };
                }).ToList();
            // 即時評価のためのToList呼び出し。遅延評価だとLdapConnectionクローズ後に取得しようとし失敗
            return enumerable;
        }
    }

    public class DeviceGroupSearchQuery : SearchQuery
    {
        private string DomainName { get; }
        private string[] GroupGuIdArray { get; }

        public DeviceGroupSearchQuery(string domainName, string[] groupGuIdArray)
        {
            this.DomainName = domainName;
            this.GroupGuIdArray = groupGuIdArray;
        }

        public override IEnumerable<ILdap> Search(LdapConnection ldapConnection)
        {
            var objectGuidFilterStringList = GroupGuIdArray.Select(gid =>
            {
                return $"(ObjectGUID=\\{BitConverter.ToString(new Guid(gid).ToByteArray()).Replace("-", "\\")})";
            }).ToList();
            var objectGuidFilter = $"(|{string.Join("", objectGuidFilterStringList)})";

            BaseString = "cn=computers,dc=" + string.Join(",dc=", DomainName.Split("."));
            Scope = LdapConnection.SCOPE_ONE;
            Filter = $"(&(objectClass=group){objectGuidFilter})";
            Attrs = new string[] { };
            TypesOnly = false;
            Constraints = null;

            var searchResults = ldapConnection.Search(BaseString, Scope, Filter, Attrs, TypesOnly, Constraints);
            var enumerable = new LdapEntryEnumrator(searchResults)
                .Where(entry => entry != null)
                .Select(entry => new LdapDeviceGroup()
                {
                    ObjectGuid = new Guid((byte[])(Array)entry.getAttribute("objectGUID")?.ByteValue ?? throw new InvalidOperationException()),
                    DistinguishedName = entry.getAttribute("distinguishedName")?.StringValue,
                    Name = entry.getAttribute("name")?.StringValue,
                    ObjectClass = entry.getAttribute("objectClass")?.StringValue
                }).ToList();
            // 即時評価のためのToList呼び出し。遅延評価だとLdapConnectionクローズ後に取得しようとし失敗
            return enumerable;
        }
    }

    public class DeviceSearchQuery : SearchQuery
    {
        private string DomainName { get; }
        private List<string> GroupDistinguishedNameList { get; }

        public DeviceSearchQuery(string domainName, List<string> groupDistinguishedNameList)
        {
            this.DomainName = domainName;
            this.GroupDistinguishedNameList = groupDistinguishedNameList;
        }

        public override IEnumerable<ILdap> Search(LdapConnection ldapConnection)
        {
            var memberOfFilter = $"(|{string.Join("", GroupDistinguishedNameList.Select(s => $"(memberOf={s})"))})";

            BaseString = "cn=computers,dc=" + string.Join(",dc=", DomainName.Split("."));
            Scope = LdapConnection.SCOPE_ONE;
            Filter = $"(&(objectClass=user){memberOfFilter})";
            Attrs = new string[] { };
            TypesOnly = false;
            Constraints = null;

            var searchResults = ldapConnection.Search(BaseString, Scope, Filter, Attrs, TypesOnly, Constraints);
            var enumerable = new LdapEntryEnumrator(searchResults)
                .Where(entry => entry != null)
                .Select(entry => new LdapDevice()
                {
                    ObjectGuid = new Guid((byte[])(Array)entry.getAttribute("objectGUID")?.ByteValue ?? throw new InvalidOperationException()),
                    DistinguishedName = entry.getAttribute("distinguishedName")?.StringValue,
                    Name = entry.getAttribute("name")?.StringValue,
                    ObjectClass = entry.getAttribute("objectClass")?.StringValue,
                    MemberOf = entry.getAttribute("memberOf")?.StringValue
                }).ToList();
            // 即時評価のためのToList呼び出し。遅延評価だとLdapConnectionクローズ後に取得しようとし失敗
            return enumerable;
        }
    }

    public class UserGroupSearchQuery : SearchQuery
    {
        private string DomainName { get; }
        private string[] GroupGuIdArray { get; }

        public UserGroupSearchQuery(string domainName, string[] groupGuIdArray)
        {
            this.DomainName = domainName;
            this.GroupGuIdArray = groupGuIdArray;
        }

        public override IEnumerable<ILdap> Search(LdapConnection ldapConnection)
        {
            var objectGuidFilterStringList = GroupGuIdArray.Select(gid =>
            {
                return $"(ObjectGUID=\\{BitConverter.ToString(new Guid(gid).ToByteArray()).Replace("-", "\\")})";
            }).ToList();
            var objectGuidFilter = $"(|{string.Join("", objectGuidFilterStringList)})";

            BaseString = "cn=users,dc=" + string.Join(",dc=", DomainName.Split("."));
            Scope = LdapConnection.SCOPE_SUB;
            Filter = $"(&(objectClass=group){objectGuidFilter})";
            Attrs = new string[] { };
            TypesOnly = false;
            Constraints = null;

            var searchResults = ldapConnection.Search(BaseString, Scope, Filter, Attrs, TypesOnly, Constraints);
            var enumerable = new LdapEntryEnumrator(searchResults)
                .Where(entry => entry != null)
                .Select(entry => new LdapUserGroup()
                {
                    ObjectGuid = new Guid((byte[])(Array)entry.getAttribute("objectGUID")?.ByteValue ?? throw new InvalidOperationException()),
                    DistinguishedName = entry.getAttribute("distinguishedName")?.StringValue,
                    Name = entry.getAttribute("name")?.StringValue,
                    ObjectClass = entry.getAttribute("objectClass")?.StringValue
                }).ToList();
            // 即時評価のためのToList呼び出し。遅延評価だとLdapConnectionクローズ後に取得しようとし失敗
            return enumerable;
        }
    }

    public class UserSearchQuery : SearchQuery
    {
        private string DomainName { get; }
        private List<string> GroupDistinguishedNameList { get; }

        public UserSearchQuery(string domainName, List<string> groupDistinguishedNameList)
        {
            this.DomainName = domainName;
            this.GroupDistinguishedNameList = groupDistinguishedNameList;
        }

        public override IEnumerable<ILdap> Search(LdapConnection ldapConnection)
        {
            var memberOfFilter = $"(|{string.Join("", GroupDistinguishedNameList.Select(s => $"(memberOf={s})"))})";

            BaseString = "cn=users,dc=" + string.Join(",dc=", DomainName.Split("."));
            Scope = LdapConnection.SCOPE_SUB;
            Filter = $"(&(objectClass=user){memberOfFilter})";
            Attrs = new string[] { };
            TypesOnly = false;
            Constraints = null;

            var searchResults = ldapConnection.Search(BaseString, Scope, Filter, Attrs, TypesOnly, Constraints);
            var enumerable = new LdapEntryEnumrator(searchResults)
                .Where(entry => entry != null)
                .Select(entry => new LdapUser()
                {
                    ObjectGuid = new Guid((byte[])(Array)entry.getAttribute("objectGUID")?.ByteValue ?? throw new InvalidOperationException()),
                    DistinguishedName = entry.getAttribute("distinguishedName")?.StringValue,
                    Name = entry.getAttribute("name")?.StringValue,
                    ObjectClass = entry.getAttribute("objectClass")?.StringValue,
                    MemberOf = entry.getAttribute("memberOf")?.StringValue,
                    UserPrincipalName = entry.getAttribute("userPrincipalName")?.StringValue
                }).ToList();
            // 即時評価のためのToList呼び出し。遅延評価だとLdapConnectionクローズ後に取得しようとし失敗
            return enumerable;
        }
    }



    public class LdapContext
    {
        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();


        private string Server { get; }
        private int Port { get; }
        private string DomainAndUser { get; }
        private string Password { get; }

        public LdapContext(string server, int port, string domainAndUser, string password)
        {
            this.Server = server;
            this.Port = port;
            this.DomainAndUser = domainAndUser;
            this.Password = password;
        }

        public IEnumerable<ILdap> GetDomainSearchResults(string domainName)
        {
            var domainSearchQuery = new DomainSearchQuery(domainName);
            return Get(domainSearchQuery);
        }

        public IEnumerable<ILdap> GetDeviceGroupSearchResults(string domainName, string[] groupGuIdArray)
        {
            var deviceGroupSearchQuery = new DeviceGroupSearchQuery(domainName, groupGuIdArray);
            return Get(deviceGroupSearchQuery);
        }

        public IEnumerable<ILdap> GetDeviceSearchResults(string domainName, List<string> groupDistinguishedNameList)
        {
            var deviceSearchQuery = new DeviceSearchQuery(domainName, groupDistinguishedNameList);
            return Get(deviceSearchQuery);
        }

        public IEnumerable<ILdap> GetUserGroupSearchResults(string domainName, string[] groupGuIdArray)
        {
            var userGroupSearchQuery = new UserGroupSearchQuery(domainName, groupGuIdArray);
            return Get(userGroupSearchQuery);
        }

        public IEnumerable<ILdap> GetUserSearchResults(string domainName, List<string> groupDistinguishedNameList)
        {
            var userSearchQuery = new UserSearchQuery(domainName, groupDistinguishedNameList);
            return Get(userSearchQuery);
        }


        private IEnumerable<ILdap> Get(SearchQuery searchQuery)
        {
            using (var ldapConnection = new LdapConnection())
            {
                try
                {
                    ldapConnection.Connect(Server, Port);
                    ldapConnection.Bind(DomainAndUser, Password);

                    //var sort = new LdapSortControl(new[] { new LdapSortKey("sn") }, true);
                    //var cons = ldapConnection.SearchConstraints;
                    //cons.setControls(sort);
                    //ldapConnection.Constraints = cons;

                    return searchQuery.Search(ldapConnection);
                }
                catch (Exception e)
                {
                    _logger.Error(e, e.Message);
                    throw;
                }
                finally
                {
                    ldapConnection.Disconnect();
                }
            }
        }
    }
}
