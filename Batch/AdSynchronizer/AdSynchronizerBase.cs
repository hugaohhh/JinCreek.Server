using System;
using System.Collections.Generic;
using System.Linq;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.Logging;

namespace JinCreek.Server.Batch.AdSynchronizer
{
    abstract class AdSynchronizerBase
    {
        protected readonly ILogger<Deauthentication> Logger;
        protected readonly UserRepository UserRepository;
        protected readonly AuthenticationRepository AuthenticationRepository;
        protected readonly int OrganizaionCode;
        protected readonly IEnumerable<IActiveDirectorySynchronizable> Dbs;
        protected readonly IEnumerable<ILdap> Ldaps;

        protected AdSynchronizerBase(ILogger<Deauthentication> logger, UserRepository userRepository, AuthenticationRepository authenticationRepository, int organizaionCode, IEnumerable<IActiveDirectorySynchronizable> dbs, IEnumerable<ILdap> ldaps)
        {
            Logger = logger;
            this.UserRepository = userRepository;
            this.AuthenticationRepository = authenticationRepository;
            this.OrganizaionCode = organizaionCode;
            this.Dbs = dbs;
            this.Ldaps = ldaps;
        }

        private void DebugPrintForDiffset(
            (List<Guid>, List<Guid>, IEnumerable<Guid>, IEnumerable<Guid>, IEnumerable<Guid>) diffSet)
        {
            var (dbsGuidList, ldapsGuidList, dbOnly, ldapOnly, same) = diffSet;

            Logger.LogDebug($"db: {dbsGuidList.Count()}");
            Logger.LogDebug($"ldap: {ldapsGuidList.Count()}");
            Logger.LogDebug($"db only {dbOnly.Count()}");
            Logger.LogDebug($"ldap only {ldapOnly.Count()}");
            Logger.LogDebug($"same {same.Count()}");
        }

        private (IEnumerable<Guid>, IEnumerable<Guid>, IEnumerable<Guid>) GetDiffSetGuidList()
        {
            List<Guid> dbsGuidList = Dbs.Select(d => d.AdObjectId).ToList();
            List<Guid> ldapsGuidList = Ldaps.Select(l => l.ObjectGuid).ToList();
            var dbOnly = dbsGuidList.Except(ldapsGuidList);
            var ldapOnly = ldapsGuidList.Except(dbsGuidList);
            var same = dbsGuidList.Intersect(ldapsGuidList);

            IEnumerable<Guid> dbOnlyEnumerable = dbOnly as Guid[] ?? dbOnly.ToArray();
            IEnumerable<Guid> ldapOnlyEnumerable = ldapOnly as Guid[] ?? ldapOnly.ToArray();
            IEnumerable<Guid> sameEnumerable = same as Guid[] ?? same.ToArray();
            DebugPrintForDiffset((dbsGuidList, ldapsGuidList, dbOnlyEnumerable, ldapOnlyEnumerable, sameEnumerable));
            return (dbOnlyEnumerable, ldapOnlyEnumerable, sameEnumerable);
        }

        public void Synchronize()
        {
            var (dbOnly, ldapOnly, same) = GetDiffSetGuidList();


            var ldapOnlyList = Ldaps.Where(r => ldapOnly.Contains(r.ObjectGuid)).ToList();
            ProcessLdapOnly(ldapOnlyList);

            var sameList = same.Select(guid =>
            {
                var ldap = Ldaps.Where(r => r.ObjectGuid == guid).FirstOrDefault();
                var db = Dbs.Where(r => r.AdObjectId == guid).FirstOrDefault();
                return (ldap, db);
            }).ToList();
            ProcessSame(sameList);

            var dbOnlyList = Dbs.Where(r => dbOnly.Contains(r.AdObjectId)).ToList();
            ProcessDbOnly(dbOnlyList);
        }

        protected abstract void ProcessLdapOnly(List<ILdap> ldapList);
        protected abstract void ProcessSame(List<(ILdap, IActiveDirectorySynchronizable)> ldapDbTupleList);
        protected abstract void ProcessDbOnly(List<IActiveDirectorySynchronizable> dbList);
    }
}
