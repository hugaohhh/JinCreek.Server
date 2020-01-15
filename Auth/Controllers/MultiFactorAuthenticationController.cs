using System;
using System.Collections.Generic;
using System.Linq;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Interfaces;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using static JinCreek.Server.Interfaces.ErrorResponse;
namespace JinCreek.Server.Auth.Controllers
{
    [Route("api/multi_factor/authentication")]
    [ApiController]
    public class MultiFactorAuthenticationController : ControllerBase
    {
        private readonly ILogger<MultiFactorAuthenticationController> _logger;

        private readonly AuthenticationRepository _authenticationRepository;

        private readonly RadiusRepository _radiusRepository;
        public IConfiguration Configuration { get; }

        public MultiFactorAuthenticationController(ILogger<MultiFactorAuthenticationController> logger,
            AuthenticationRepository authenticationRepository, RadiusRepository radiusRepository,
            IConfiguration configuration)
        {
            _logger = logger;
            _authenticationRepository = authenticationRepository;
            _radiusRepository = radiusRepository;
            Configuration = configuration;
        }

        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(MultiFactorAuthenticationResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ErrorResponse))]
        public IActionResult Authentication(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest)
        {
            _logger.LogDebug("hello");

            var account = multiFactorAuthenticationRequest.Account;
            var authId = multiFactorAuthenticationRequest.AuthId;

            var factorCombination = _authenticationRepository.QueryFactorCombination(account, Guid.Parse(authId));
            string startTime = Configuration.GetSection("Auth:ExpireHour").Value;
            if (factorCombination  == null)
            {
                var simDevice = _authenticationRepository.QuerySimDevice(Guid.Parse(authId));
                if (simDevice == null) return Unauthorized(NotMatchAuthId);
                _radiusRepository.MultiFactorAuthUpdateRadreply(simDevice, null,false);

                var multiFactorAuthenticationLogFail = new MultiFactorAuthenticationLogFail
                {
                    SimDevice = simDevice,
                    ConnectionTime = DateTime.Now
                };
                _authenticationRepository.Create(multiFactorAuthenticationLogFail);
                return Unauthorized(NotMatchMultiFactor);
            }

            _radiusRepository.MultiFactorAuthUpdateRadreply(factorCombination.SimDevice,factorCombination, true);

            var multiFactorAuthenticationLogSuccess = new MultiFactorAuthenticationLogSuccess
            {
                ConnectionTime = DateTime.Now,
                FactorCombination = factorCombination
            };
            _authenticationRepository.Create(multiFactorAuthenticationLogSuccess);
            var multiFactorAuthenticationStateDone = factorCombination.MultiFactorAuthenticationStateDone;

            if (multiFactorAuthenticationStateDone == null)
            {
                multiFactorAuthenticationStateDone = new MultiFactorAuthenticationStateDone
                {
                    FactorCombination = factorCombination,
                    TimeLimit = DateTime.Now.AddHours(double.Parse(startTime))
                };
                _authenticationRepository.Create(multiFactorAuthenticationStateDone);
            }
            else if (multiFactorAuthenticationStateDone != null)
            {
                multiFactorAuthenticationStateDone.TimeLimit =
                    DateTime.Now.AddHours(double.Parse(startTime));
                _authenticationRepository.Update(multiFactorAuthenticationStateDone);
            }

            var multiFactorAuthenticationResponse = new MultiFactorAuthenticationResponse
            {
                UserConfigureDictionary = new Dictionary<string, string>
                {   
                    {
                        "is_disconnect_network_screen_lock",
                        factorCombination.EndUser.IsDisconnectWhenScreenLock.ToString()
                    }
                }
            };
            return Ok(multiFactorAuthenticationResponse);
        }
    }
}
