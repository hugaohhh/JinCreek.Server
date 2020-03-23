using ConsoleAppFramework;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace JinCreek.Server.Batch
{
    public class Deauthentication : ConsoleAppBase
    {
        // configurationを扱う定型としたいため
        // ReSharper disable once NotAccessedField.Local
        private readonly IConfiguration _configuration;
        private readonly ILogger<Deauthentication> _logger;
        private readonly AuthenticationRepository _authenticationRepository;
        private readonly RadiusRepository _radiusRepository;

        public Deauthentication(ILogger<Deauthentication> logger, IConfiguration configuration, AuthenticationRepository authenticationRepository, RadiusRepository radiusRepository)
        {
            this._logger = logger;
            this._configuration = configuration;
            this._authenticationRepository = authenticationRepository;
            this._radiusRepository = radiusRepository;
        }

        [Command("deauthentication")]
        public void Main(
            [Option("organization_code", "organization code")]
            int organizationCode
            )
        {
            _logger.LogInformation($"{GetType().FullName} Start");
            try
            {
                var expiredSimAndDeviceAuthenticatedList = _authenticationRepository.GetExpiredSimAndDeviceAuthenticatedList(organizationCode);

                foreach (var simAndDeviceAuthenticated in expiredSimAndDeviceAuthenticatedList)
                {
                    var multiFactorAuthenticatedList = _authenticationRepository.GetExpiredMultiFactorAuthenticatedListBySimAndDeviceId(simAndDeviceAuthenticated
                        .SimAndDevice.Id);
                    if (multiFactorAuthenticatedList != null)
                    {
                        foreach (var multiFactorAuthenticated in multiFactorAuthenticatedList)
                        {
                            var deauthenticationLog = CreateDeauthenticationLog(multiFactorAuthenticated);
                            _authenticationRepository.Create(deauthenticationLog);
                        }

                        _radiusRepository.UpdateRadreply(simAndDeviceAuthenticated.SimAndDevice.Sim.UserName +  "@" + simAndDeviceAuthenticated.SimAndDevice.Sim.SimGroup.UserNameSuffix,
                            simAndDeviceAuthenticated.SimAndDevice.IsolatedNw2Ip);

                        foreach (var multiFactorAuthenticated in multiFactorAuthenticatedList)
                        {
                            _authenticationRepository.DeleteAuthenticationState(multiFactorAuthenticated);
                        }
                    }
                    _authenticationRepository.DeleteAuthenticationState(simAndDeviceAuthenticated);
                }

                _logger.LogInformation($"{GetType().FullName} Success");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                _logger.LogInformation($"{GetType().FullName} Error");
            }
        }

        private DeauthenticationLog CreateDeauthenticationLog(MultiFactorAuthenticated multiFactorAuthenticated)
        {
            var deauthenticationLog = new DeauthenticationLog
            {
                MultiFactor = multiFactorAuthenticated.MultiFactor,
                Time = DateTime.Now,
                Sim = multiFactorAuthenticated.MultiFactor.SimAndDevice.Sim
            };
            return deauthenticationLog;
        }
    }
}
