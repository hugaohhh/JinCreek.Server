using System;
using System.Collections.Generic;
using System.Linq;
using JinCreek.Server.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        public List<SimDevice> QuerySimDevice(string simMsisdn,string simImsi,string simIccId,string deviceImei)
        {

            var list = _dbContext.SimDevice
                .Include(sd => sd.Sim)
                .Include(sd => sd.Device)
                .Include(sd=>sd.FactorCombinations)
                .Where(sd => 
                    sd.Device.DeviceImei == deviceImei
                    && sd.Sim.Imsi == simImsi
                    && sd.Sim.Msisdn == simMsisdn
                    && sd.Sim.IccId == simIccId)
                .ToList();

            return list;
        }

        public void Create(Organization organization, SimGroup simGroup, Sim sim, DeviceGroup deviceGroup, Device device, Lte lte, SimDevice simDevice)
        {
            //_dbContext.Organization.Add(organization);
            _dbContext.AddRange(organization,lte,deviceGroup,simGroup,device,sim,simDevice);
            _dbContext.SaveChanges();
        }

        //public int Create(SimDeviceAuthentication simDeviceAuthentication)
        //{
        //    _dbContext.SimDeviceAuthentication.Add(simDeviceAuthentication);
        //    return _dbContext.SaveChanges();
        //}

        //public int Create(SimDeviceAuthenticationEnd simDeviceAuthenticationEnd)
        //{
        //    _dbContext.SimDeviceAuthenticationEnd.Add(simDeviceAuthenticationEnd);
        //    return _dbContext.SaveChanges();
        //}

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

        public void Create(Domain domain, UserGroup userGroup, AdminUser admin, User user)
        {
            _dbContext.AddRange(domain,userGroup,admin,user);
            _dbContext.SaveChanges();
        }

        public void Create(FactorCombination factorCombination)
        {
            _dbContext.FactorCombination.Add(factorCombination);
            _dbContext.SaveChanges();
        }

        //public List<string> QueryFactorCombination(SimDevice simDevice)
        //{
        //    var factorCombinations = _dbContext.FactorCombination
        //        .Include(f => f.User)
        //        .Where(f => f.SimDeviceId == simDevice.Id)
        //        .ToList();
        //    var canLogonUsers = factorCombinations.Select(f => f.User.AccountName).ToList();
        //    return canLogonUsers;
        //}

        //public AuthenticationState QueryAuthenticationStateBySimDevice(SimDevice simDevice)
        //{
        //    var simDeviceAuthenticationEnd = _dbContext.SimDeviceAuthenticationEnd
        //        .Where(s => s.SimDeviceId == simDevice.DeviceId);

        //    return null;
        //}
    }
}
