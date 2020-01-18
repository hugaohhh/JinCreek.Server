using JinCreek.Server.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JinCreek.Server.Common.Repositories
{
    public class AuthenticationRepository
    {
        private readonly MainDbContext _dbContext;

        public class EfLoggerProvider : ILoggerProvider
        {
            public ILogger CreateLogger(string categoryName) => new EfLogger(categoryName);
            public void Dispose() { }
        }

        public class EfLogger : ILogger
        {
            private readonly string _categoryName;

            public EfLogger(string categoryName) => this._categoryName = categoryName;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                //ef core がDBを検索するときに categoryNameはMicrosoft.EntityFrameworkCore.Database.CommandとLogLevelはInformationの場合
                if (_categoryName == "Microsoft.EntityFrameworkCore.Database.Command"
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

        public SimDevice GetSimDevice(string simMsisdn, string simImsi, string simIccId, string deviceImei)
        {
            var simDevice = _dbContext.SimDevice
                .Include(sd => sd.Sim)
                .Include(sd => sd.Device)
                .Include(sd => sd.SimDeviceAuthenticationStateDone)
                .Where(sd =>
                    sd.Device.DeviceImei == deviceImei
                    && sd.Sim.Imsi == simImsi
                    && sd.Sim.Msisdn == simMsisdn
                    && sd.Sim.IccId == simIccId
                    && sd.StartDay <= DateTime.Now
                    && (sd.EndDay == null || sd.EndDay >= DateTime.Now))
                .FirstOrDefault();
            return simDevice;
        }

        public SimDevice GetSimDevice(Guid id)
        {
            return _dbContext.SimDevice
                .Include(sd => sd.Sim)
                .Include(sd => sd.Device)
                .Include(sd => sd.FactorCombinations)
                .Include(sd => sd.SimDeviceAuthenticationStateDone)
                .Where(sd => sd.SimDeviceAuthenticationStateDone.Id == id)
                .FirstOrDefault();
        }

        public void Create(Organization organization)
        {
            _dbContext.Organization.Add(organization);
            _dbContext.SaveChanges();
        }
        public void Create(SimGroup simGroup)
        {
            _dbContext.SimGroup.Add(simGroup);
            _dbContext.SaveChanges();
        }
        public void Create(DeviceGroup deviceGroup)
        {
            _dbContext.DeviceGroup.Add(deviceGroup);
            _dbContext.SaveChanges();
        }
        public void Create(Device device)
        {
            _dbContext.Device.Add(device);
            _dbContext.SaveChanges();
        }
        public void Create(Lte lte)
        {
            _dbContext.Lte.Add(lte);
            _dbContext.SaveChanges();
        }
        public void Create(Sim sim)
        {
            _dbContext.Sim.Add(sim);
            _dbContext.SaveChanges();
        }
        public void Create(SimDevice simDevice)
        {
            _dbContext.SimDevice.Add(simDevice);
            _dbContext.SaveChanges();
        }
        public void Create(Domain domain)
        {
            _dbContext.Domain.Add(domain);
            _dbContext.SaveChanges();
        }
        public void Create(UserGroup userGroup)
        {
            _dbContext.UserGroup.Add(userGroup);
            _dbContext.SaveChanges();
        }
        public void Create(AdDeviceSettingOfflineWindowsSignIn adDeviceSettingOfflineWindowsSignIn)
        {
            _dbContext.AdDeviceSettingOfflineWindowsSignIn.Add(adDeviceSettingOfflineWindowsSignIn);
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

        public void Create(Deauthentication deauthentication)
        {
            _dbContext.Deauthentication.Add(deauthentication);
            _dbContext.SaveChanges();
        }

        public Sim GetSim(string simMsisdn, string simImsi, string simIccId)
        {
            return _dbContext.Sim
                .Include(s => s.SimDevice)
                .Include(s => s.SimGroup)
                .Where(s => s.IccId == simIccId
                            && s.Msisdn == simMsisdn
                            && s.Imsi == simImsi)
                .FirstOrDefault();
        }

        public Organization GetOrganization(long code)
        {
            return _dbContext.Organization.SingleOrDefault(o => o.Code == code);
        }

        public void Create(FactorCombination factorCombination)
        {
            _dbContext.FactorCombination.Add(factorCombination);
            _dbContext.SaveChanges();
        }

        public List<string> GetLoginUsers(SimDevice simDevice)
        {
            var factorCombinations = _dbContext.FactorCombination
                .Include(f => f.EndUser)
                .Where(f => f.SimDeviceId == simDevice.Id
                            && f.StartDay <= DateTime.Now
                            && (f.EndDay == null || f.EndDay >= DateTime.Now))
                .ToList();
            if (factorCombinations.Count <= 0) return null;
            var canLogonUsers = factorCombinations.Select(f => f.EndUser.AccountName).ToList();
            return canLogonUsers;
        }

        /// <summary>
        /// 多要素認証するときに　account　と　authId　で多要素認証組合せを検索
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authId"></param>
        /// <returns></returns>
        public FactorCombination GetFactorCombination(string account, Guid authId)
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
                             && (fc.EndDay == null || fc.EndDay >= DateTime.Now))
                .FirstOrDefault();
            return factorCombination;
        }

        /// <summary>
        /// 多要素認証するときに　account　と　authId　で多要素認証組合せを検索
        /// </summary>
        /// <param name="account"></param>
        /// <param name="simDevice"></param>
        /// <returns></returns>
        public FactorCombination GetFactorCombination(string account, SimDevice simDevice)
        {
            var factorCombination = _dbContext.FactorCombination
                .Include(fc => fc.SimDevice)
                .Include(fc => fc.EndUser)
                .Include(fc => fc.MultiFactorAuthenticationStateDone)
                .Where(fc => fc.EndUser.AccountName == account
                             && fc.SimDevice.Id == simDevice.Id)
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

        public AdDevice GetAdDevice(Guid deviceId)
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

        public int DeleteSimDeviceAuthDone(SimDevice simDevice)
        {
            _dbContext.SimDeviceAuthenticationStateDone.Remove(simDevice.SimDeviceAuthenticationStateDone ?? throw new InvalidOperationException());
            simDevice.SimDeviceAuthenticationStateDone = null;
            return _dbContext.SaveChanges();
        }

        public int DeleteMultiFactorAuthDone(FactorCombination factorCombination)
        {
            _dbContext.MultiFactorAuthenticationStateDone.Remove(factorCombination.MultiFactorAuthenticationStateDone ?? throw new InvalidOperationException());
            factorCombination.MultiFactorAuthenticationStateDone = null;
            return _dbContext.SaveChanges();
        }

        public SimDeviceAuthenticationStateDone GetSimDeviceAuthenticationDone(Guid id)
        {
            return _dbContext.SimDeviceAuthenticationStateDone
                .Include(sd => sd.SimDevice)
                .Include(sd => sd.SimDevice.Sim)
                .FirstOrDefault(sd => sd.Id == id);
        }
    }
}
