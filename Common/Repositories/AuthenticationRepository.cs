using System;
using System.Collections.Generic;
using System.Linq;
using JinCreek.Server.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog.LayoutRenderers.Wrappers;

namespace JinCreek.Server.Common.Repositories
{
    public class AuthenticationRepository
    {
        private readonly MainDbContext _dbContext;

        public class EFLoggerProvider : ILoggerProvider
        {
            public ILogger CreateLogger(string categoryName) => new EFLogger(categoryName);
            public void Dispose() { }
        }

        public class EFLogger : ILogger
        {
            private readonly string categoryName;

            public EFLogger(string categoryName) => this.categoryName = categoryName;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                //ef core执行数据库查询时的categoryName为Microsoft.EntityFrameworkCore.Database.Command,日志级别为Information
                if (categoryName == "Microsoft.EntityFrameworkCore.Database.Command"
                    && logLevel == LogLevel.Information)
                {
                    var logContent = formatter(state, exception);
                    //TODO: 
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(logContent);
                    Console.ResetColor();
                }
            }

            public IDisposable BeginScope<TState>(TState state) => null;
        }

        public AuthenticationRepository(MainDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public SimDevice QuerySimDevice(string simMsisdn,string simImsi,string simIccId,string deviceImei)
        {

            var simDevice = _dbContext.SimDevice
                .Include(sd => sd.Sim)
                .Include(sd => sd.Device)
                .Include(sd => sd.SimDeviceAuthenticationStateDone)
                .Where(sd => 
                    sd.Device.DeviceImei == deviceImei
                    && sd.Sim.Imsi == simImsi
                    && sd.Sim.Msisdn == simMsisdn
                    && sd.Sim.IccId == simIccId)
                .FirstOrDefault();

            return simDevice;
        }

        public SimDevice QuerySimDevice(Guid id)
        {
            return _dbContext.SimDevice
                .Include(sd => sd.Sim)
                .Include(sd => sd.Device)
                .Include(sd => sd.FactorCombinations)
                .Include(sd => sd.SimDeviceAuthenticationStateDone)
                .Where(sd => sd.SimDeviceAuthenticationStateDone.Id == id)
                .FirstOrDefault();

        }

        public void Create(Organization organization, SimGroup simGroup, Sim sim, DeviceGroup deviceGroup, 
            Device device, Lte lte, SimDevice simDevice,Domain domain,UserGroup userGroup)
        {
            //_dbContext.Organization.Add(organization);
            _dbContext.AddRange(organization,lte,deviceGroup,simGroup,device,sim,simDevice,domain,userGroup);
            _dbContext.SaveChanges();
        }

        public void Create(User user)
        {
            _dbContext.User.Add(user);
            _dbContext.SaveChanges();
        }

        public int Create(SimDeviceAuthenticationLogSuccess simDeviceAuthentication)
        {
            _dbContext.SimDeviceAuthenticationLogSuccess.Add(simDeviceAuthentication);
            return _dbContext.SaveChanges();
        }

        public int Create(MultiFactorAuthenticationLogSuccess multiFactorAuthenticationLogSuccess)
        {
            _dbContext.MultiFactorAuthenticationLogSuccess.Add(multiFactorAuthenticationLogSuccess);
            return _dbContext.SaveChanges();
        }

        public int Create(SimDeviceAuthenticationLogFail simDeviceAuthenticationLogFail)
        {
            _dbContext.SimDeviceAuthenticationLogFail.Add(simDeviceAuthenticationLogFail);
            return _dbContext.SaveChanges();
        }

        public int Create(MultiFactorAuthenticationLogFail multiFactorAuthenticationLogFail)
        {
            _dbContext.MultiFactorAuthenticationLogFail.Add(multiFactorAuthenticationLogFail);
            return _dbContext.SaveChanges();
        }

        public int Create(SimDeviceAuthenticationStateDone simDeviceAuthenticationEnd)
        {
            _dbContext.SimDeviceAuthenticationStateDone.Add(simDeviceAuthenticationEnd);
            return _dbContext.SaveChanges();
        }

        public int Create(MultiFactorAuthenticationStateDone multiFactorAuthenticationStateDone)
        {
            _dbContext.MultiFactorAuthenticationStateDone.Add(multiFactorAuthenticationStateDone);
            return _dbContext.SaveChanges();
        }

        public Sim QuerySim(string simMsisdn, string simImsi, string simIccId)
        {
            return _dbContext.Sim
                .Include(s=>s.SimDevice)
                .Where(s => s.IccId == simIccId
                            && s.Msisdn == simMsisdn
                            && s.Imsi == simImsi)
                .FirstOrDefault();
        }

        public Organization GetOrganization(string organizationcode1)
        {
            return _dbContext.Organization.SingleOrDefault(o => o.Code == organizationcode1);
        }

        public void Create(FactorCombination factorCombination)
        {
            _dbContext.FactorCombination.Add(factorCombination);
            _dbContext.SaveChanges();
        }

        public List<string> QueryLoginUsers(SimDevice simDevice)
        {
            var factorCombinations = _dbContext.FactorCombination
                .Include(f => f.EndUser)
                .Where(f =>f.SimDeviceId==simDevice.Id
                            && f.StartDay <= DateTime.Now
                            && (f.EndDay==null || f.EndDay >= DateTime.Now))
                .ToList();
            var canLogonUsers = factorCombinations.Select(f => f.EndUser.AccountName).ToList();
            return canLogonUsers;
        }


        public FactorCombination QueryFactorCombination(string account, Guid authId)
        {
            var factorCombination = _dbContext.FactorCombination
                .Include(fc => fc.SimDevice)
                .Include(fc => fc.EndUser)
                .Include(fc => fc.MultiFactorAuthenticationStateDone)
                .Include(fc => fc.SimDevice.SimDeviceAuthenticationStateDone)
                .Include(fc => fc.SimDevice.Sim)
                .Where(fc => fc.EndUser.AccountName == account
                             && fc.SimDevice.SimDeviceAuthenticationStateDone.Id == authId
                             && fc.StartDay <= DateTime.Now
                             &&(fc.EndDay==null || fc.EndDay >= DateTime.Now))
                .FirstOrDefault();
            return factorCombination;
        }

        public UserGroup GetUserGroup(string usergroup1)
        {
            return _dbContext.UserGroup
                .Include(ug => ug.Domain)
                .Where(ug => ug.UserGroupName == usergroup1)
                .FirstOrDefault();
        }

        public List<EndUser> GetEndUser()
        {
            return _dbContext.EndUser.ToList();
        }

        public AdDevice QueryAdDevice(Guid deviceId)
        {
            var adDevice = _dbContext.AdDevice
                .Include(ad => ad.AdDeviceSettingOfflineWindowsSignIn)
                .Include(ad => ad.Domain)
                .Where(ad => ad.Id == deviceId)
                .SingleOrDefault();
            return adDevice;
        }

        public void Update(SimDeviceAuthenticationStateDone simDeviceAuthenticationStateDone)
        {
            _dbContext.SimDeviceAuthenticationStateDone.Update(simDeviceAuthenticationStateDone);
            _dbContext.SaveChanges();
        }

        public void Update(MultiFactorAuthenticationStateDone multiFactorAuthenticationStateDone)
        {
            _dbContext.MultiFactorAuthenticationStateDone.Update(multiFactorAuthenticationStateDone);
            _dbContext.SaveChanges();
        }
    }
}
