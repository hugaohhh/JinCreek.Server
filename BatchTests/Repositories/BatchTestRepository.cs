using System;
using System.Collections.Generic;
using System.Linq;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JinCreek.Server.Batch.Repositories
{
    class BatchTestRepository
    {
        protected MainDbContext MainDbContext;
        protected RadiusDbContext RadiusDbContext;

        public BatchTestRepository(MainDbContext mainDbContext, RadiusDbContext radiusDbContext)
        {
            MainDbContext = mainDbContext;
            RadiusDbContext = radiusDbContext;
        }

        public SimAndDeviceAuthenticated GetSimAndDeviceAuthenticatedBySimAndDeviceId(int organizationCode, Guid? simAndDeviceId)
        {
            return (simAndDeviceId == null) ? null :
                MainDbContext.SimAndDeviceAuthenticated
                .Where(r =>
                    r.SimAndDevice.Sim.SimGroup.OrganizationCode == organizationCode
                    && r.SimAndDeviceId == simAndDeviceId
                    )
                .FirstOrDefault();
        }

        public MultiFactorAuthenticated GetMultiFactorAuthenticatedByMultiFactorId(int organizationCode, Guid? multiFactorId)
        {
            return (multiFactorId == null) ? null :
                MainDbContext.MultiFactorAuthenticated
                .Where(r =>
                    r.MultiFactor.SimAndDevice.Sim.SimGroup.OrganizationCode == organizationCode
                    && r.MultiFactorId == multiFactorId
                    )
                .FirstOrDefault();
        }

        public DeauthenticationLog GetDeauthenticationLogByMultiFactorId(int organizationCode, Guid? multiFactorId)
        {
            return (multiFactorId == null) ? null :
                MainDbContext.DeauthenticationLog
                .Where(r =>
                    r.Sim.SimGroup.OrganizationCode == organizationCode
                    && r.MultiFactor.Id == multiFactorId
                    )
                .FirstOrDefault();
        }

        public List<SimGroup> GetSimGroups(int code)
        {
            return MainDbContext.SimGroup
                .Include(s => s.Organization)
                .Where(s => s.OrganizationCode == code).ToList();
        }

        public List<Sim> GetSims(int code)
        {
            return MainDbContext.Sim.Include(s => s.SimGroup)
                .Include(s => s.SimGroup.Organization)
                .Where(s => s.SimGroup.OrganizationCode == code).ToList();
        }

        public List<Radgroupcheck> GetRadgroupcheckList(Guid id)
        {
            return RadiusDbContext.Radgroupcheck.AsNoTracking().Where(r => r.Groupname == id.ToString()).ToList();
        }
        public List<Radgroupreply> GetRadgroupreplyList(Guid id)
        {
            return RadiusDbContext.Radgroupreply.AsNoTracking().Where(r => r.Groupname == id.ToString()).ToList();
        }
        public List<Radippool> GetRadippool(string simGroupIsolatedNw1IpPool)
        {
            return RadiusDbContext.Radippool.AsNoTracking().Where(r => r.PoolName == simGroupIsolatedNw1IpPool).ToList();
        }
        public List<Radcheck> GetRadcheckList(string simGroupUserNameSuffix)
        {
            return RadiusDbContext.Radcheck.AsNoTracking().Where(r => r.Username == simGroupUserNameSuffix).ToList();
        }
        public Radusergroup GetRadusergroup(string simGroupUserNameSuffix)
        {
            return RadiusDbContext.Radusergroup.AsNoTracking().Where(r => r.Username == simGroupUserNameSuffix).SingleOrDefault();
        }
        public List<Radgroupcheck> GetRadgroupcheckListByValue(string empty)
        {
            return RadiusDbContext.Radgroupcheck.AsNoTracking().Where(r => r.Value == empty).ToList();
        }
        public List<Radgroupreply> GetRadgroupreplyListByValue(string empty)
        {
            return RadiusDbContext.Radgroupreply.AsNoTracking().Where(r => r.Value == empty).ToList();
        }

        public List<EndUser> GetUsers(int targetOrganizationCode)
        {
            return MainDbContext.EndUser.AsNoTracking().Include(e => e.Domain)
                .Include(e => e.UserGroupEndUsers)
                .Include(e => e.AvailablePeriods)
                .Where(d => d.Domain.OrganizationCode == targetOrganizationCode).ToList();
        }

        public List<Device> GetDevices(int targetOrganizationCode)
        {
            return MainDbContext.Device.AsNoTracking().Include(d => d.Domain)
                .Include(d => d.OrganizationClientApp)
                .Include(d => d.LteModule)
                .OrderByDescending(d => d.Name)
                .Where(d => d.Domain.OrganizationCode == targetOrganizationCode).ToList();
        }

        public List<Domain> GetDomain(int targetOrganizationCode)
        {
            return MainDbContext.Domain.AsNoTracking()
                .Include(d => d.Organization)
                .Where(d => d.OrganizationCode == targetOrganizationCode).ToList();
        }

        public List<DeviceGroup> GetDeviceGroup(int targetOrganizationCode)
        {
           return MainDbContext.DeviceGroup.AsNoTracking().Include(d => d.Domain)
                .Where(d => d.Domain.OrganizationCode == targetOrganizationCode)
                .OrderByDescending(d => d.Name).ToList();
        }

        public List<UserGroup> GetUserGroup(int targetOrganizationCode)
        {
            return MainDbContext.UserGroup.AsNoTracking().Include(d => d.Domain)
                .Where(d => d.Domain.OrganizationCode == targetOrganizationCode)
                .OrderByDescending(d => d.Name).ToList();
        }

        public List<SimAndDevice> GetSimAndDeviceByDeviceId(Guid deviceId)
        {
            return MainDbContext.SimAndDevice.AsNoTracking().Where(s => s.DeviceId == deviceId).ToList();
        }

        public List<MultiFactor> GetMultiFactorByEndUser(Guid endUserId)
        {
            return MainDbContext.MultiFactor.AsNoTracking().Where(m => m.EndUserId == endUserId).ToList();
        }
    }
}
