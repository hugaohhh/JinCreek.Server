using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JinCreek.Server.AuthTests.Repositories
{
    public class AuthControllerTestRepository
    {
        private MainDbContext _mainDbContext;
        private RadiusDbContext _radiusDbContext;

        public AuthControllerTestRepository(MainDbContext mainDbContext, RadiusDbContext radiusDbContext)
        {
            _mainDbContext = mainDbContext;
            _radiusDbContext = radiusDbContext;
        }

        //////////////////////////////
        //// MainDbContext
        public SimAndDeviceAuthenticated GetSimAndDeviceAuthenticated(Guid? id)
        {
            return _mainDbContext.SimAndDeviceAuthenticated.AsNoTracking()
                .Include(sd => sd.SimAndDevice)
                .Include(sd => sd.SimAndDevice.Sim)
                .Include(sd => sd.SimAndDevice.Sim.SimGroup)
                .FirstOrDefault(sd => sd.Id == id);
        }

        public List<MultiFactorAuthenticationSuccessLog> GetMultiFactorAuthenticationSuccessLogByMultiFactorId(Guid? multiFactorId)
        {
            return _mainDbContext.MultiFactorAuthenticationSuccessLog.AsNoTracking()
                .Where(fl =>
                    fl.MultiFactor.Id == multiFactorId)
                .ToList();
        }

        public List<MultiFactorAuthenticationFailureLog> GetMultiFactorAuthenticationFailureLogBySimAndDeviceId(Guid? simAndDeviceId)
        {
            return _mainDbContext.MultiFactorAuthenticationFailureLog.AsNoTracking()
                .Where(fl => 
                    fl.SimAndDevice.Id == simAndDeviceId)
                .ToList();
        }


        public SimAndDeviceAuthenticated GetSimAndDeviceAuthenticated(string simMsisdn, string simImsi, string simIccId, string deviceName)
        {
            return _mainDbContext.SimAndDeviceAuthenticated.AsNoTracking()
                .Where(sda =>
                    sda.SimAndDevice.Sim.Msisdn == simMsisdn
                        && sda.SimAndDevice.Sim.Imsi == simImsi
                        && sda.SimAndDevice.Sim.IccId == simIccId
                        && sda.SimAndDevice.Device.Name == deviceName
                )
                .FirstOrDefault();
        }

        public List<SimAndDeviceAuthenticated> GetAllSimAndDeviceAuthenticated()
        {
            return _mainDbContext.SimAndDeviceAuthenticated.ToList();
        }

        public SimAndDeviceAuthenticationSuccessLog GetSimAndDeviceAuthenticationSuccessLog(Guid simAndDeviceId)
        {
            return _mainDbContext.SimAndDeviceAuthenticationSuccessLog
                .Where(l => l.SimAndDevice.Id == simAndDeviceId)
                .FirstOrDefault();
        }

        public SimAndDeviceAuthenticationFailureLog GetSimAndDeviceAuthenticationFailureLog(Guid simId)
        {
            return _mainDbContext.SimAndDeviceAuthenticationFailureLog
                .Where(l => l.Sim.Id == simId)
                .FirstOrDefault();
        }

        public MultiFactor GetFactorCombination(string account, Guid? authId)
        {
            var factorCombination = _mainDbContext.MultiFactor
                .AsNoTracking()
                .Include(fc => fc.SimAndDevice)
                .Include(fc => fc.EndUser)
                .Include(fc => fc.MultiFactorAuthenticated)
                .Include(fc => fc.SimAndDevice.SimAndDeviceAuthenticated)
                .Include(fc => fc.SimAndDevice.Sim)
                .Include(fc => fc.SimAndDevice.Sim.SimGroup)
                .Where(fc => fc.EndUser.AccountName == account
                             && fc.SimAndDevice.SimAndDeviceAuthenticated.Id == authId
                             && fc.StartDate <= DateTime.Now.Date
                             && (fc.EndDate == null || fc.EndDate >= DateTime.Now.Date))
                .FirstOrDefault();
            return factorCombination;
        }

        public List<MultiFactor> GetFactorCombinationList()
        {
            return _mainDbContext.MultiFactor.ToList();
        }
        public SimAndDevice GetSimDevice(string simMsisdn, string simImsi, string simIccId, string deviceName)
        {
            var simDevice = _mainDbContext.SimAndDevice
                .AsNoTracking()
                .Include(sd => sd.Sim)
                .Include(sd => sd.Device)
                .Include(sd => sd.SimAndDeviceAuthenticated)
                .Where(sd =>
                    sd.Device.Name == deviceName
                    && sd.Sim.Imsi == simImsi
                    && sd.Sim.Msisdn == simMsisdn
                    && sd.Sim.IccId == simIccId
                    && sd.StartDate <= DateTime.Now.Date
                    && (sd.EndDate == null || sd.EndDate >= DateTime.Now.Date))
                .FirstOrDefault();
            return simDevice;
        }
        public SimAndDevice GetSimDevice(Guid id)
        {
            return _mainDbContext.SimAndDevice
                .Include(sd => sd.Sim)
                .Include(sd => sd.Device)
                .Include(sd => sd.SimAndDeviceAuthenticated)
                .Where(sd => sd.SimAndDeviceAuthenticated.Id == id)
                .FirstOrDefault();
        }

        public List<SimAndDevice> GetSimDeviceList()
        {
            return _mainDbContext.SimAndDevice
                .ToList();
        }
        public MultiFactor GetFactorCombination(string account, SimAndDevice simAndDevice)
        {
            var factorCombination = _mainDbContext.MultiFactor
                .AsNoTracking()
                .Include(fc => fc.SimAndDevice)
                .Include(fc => fc.EndUser)
                .Include(fc => fc.MultiFactorAuthenticated)
                .Where(fc => fc.EndUser.AccountName == account
                             && fc.SimAndDevice.Id == simAndDevice.Id)
                .FirstOrDefault();
            return factorCombination;
        }

        public List<DeauthenticationLog> GetDeauthentication()
        {
            return _mainDbContext.DeauthenticationLog.ToList();
        }
        public List<MultiFactorAuthenticated> GetMultiFatorAuthenticationDone()
        {
            return _mainDbContext.MultiFactorAuthenticated.ToList();
        }
        public List<MultiFactorAuthenticationSuccessLog> GetMultiFatorAuthenticationLogSuccess()
        {
            return _mainDbContext.MultiFactorAuthenticationSuccessLog.ToList();
        }
        public List<MultiFactorAuthenticationFailureLog> GetMultiFactorAuthenticationLogFail()
        {
            return _mainDbContext.MultiFactorAuthenticationFailureLog.ToList();
        }
        public List<SimAndDeviceAuthenticationFailureLog> GetSimDeviceAuthenticationLogFail()
        {
            return _mainDbContext.SimAndDeviceAuthenticationFailureLog.ToList();
        }

        public List<SimAndDeviceAuthenticated> SimAndDeviceAuthenticated()
        {
            return _mainDbContext.SimAndDeviceAuthenticated.ToList();
        }
        public List<SimAndDeviceAuthenticationSuccessLog> GetSimDeviceAuthenticationLogSuccess()
        {
            return _mainDbContext.SimAndDeviceAuthenticationSuccessLog.ToList();
        }
        //////////////////////////////
        //// RadiusDbContext
        public Radreply GetRadreply(string username)
        {
            return _radiusDbContext.Radreply
                .AsNoTracking()
                .Where(r => r.Username == username).FirstOrDefault();
        }
        public List<Radreply> GetRadreplys(string username)
        {
            return _radiusDbContext.Radreply
                .AsNoTracking()
                .Where(r => r.Username == username
                            && r.Attribute == "Framed-IP-Address").ToList();
        }
    }
}
