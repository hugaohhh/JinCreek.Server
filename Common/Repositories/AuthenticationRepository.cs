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
    }
}
