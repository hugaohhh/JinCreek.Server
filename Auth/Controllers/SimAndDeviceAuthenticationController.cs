using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using JinCreek.Server.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using static JinCreek.Server.Interfaces.ErrorResponse;

namespace JinCreek.Server.Auth.Controllers
{
    [OpenApiTag("SIM＆端末認証", Description = "SIM＆端末認証を行う。成功時はログイン可能ユーザ一覧、クライアント(デバイスやSIM)機器に関わるサーバー側に保持する動的な設定項目を返却する。")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [Route("api/sim-and-device/authentication")]
    [ApiController]
    public class SimAndDeviceAuthenticationController : ControllerBase
    {
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        private readonly ILogger<SimAndDeviceAuthenticationController> _logger;
        private readonly AuthenticationRepository _authenticationRepository;
        private readonly RadiusRepository _radiusDbRepository;
        private readonly IConfiguration _configuration;

        public SimAndDeviceAuthenticationController(ILogger<SimAndDeviceAuthenticationController> logger,
            AuthenticationRepository authenticationRepository,
            RadiusRepository radiusRepository,
            IConfiguration configuration)
        {
            _logger = logger;
            _authenticationRepository = authenticationRepository;
            _radiusDbRepository = radiusRepository;
            _configuration = configuration;
        }


        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(SimDeviceAuthenticationResponse), Description = "認証成功")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ValidationProblemDetails), Description = "リクエスト内容不正")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ErrorResponse), Description = "認証失敗")]
        public IActionResult Authentication(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest)
        {
            var simMsisdn = simDeviceAuthenticationRequest.SimMsisdn;
            var simImsi = simDeviceAuthenticationRequest.SimImsi;
            var simIccId = simDeviceAuthenticationRequest.SimIccId;
            var certBase64String = simDeviceAuthenticationRequest.ClientCertificationBase64;

            var subjectCn = CertificateUtil.GetSubjectCommonNameByCertificationBase64(certBase64String);
            if (subjectCn == null)
            {
                var validationProblemDetails = ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState);
                validationProblemDetails.Errors.Add(new KeyValuePair<string, string[]>("ClientCertificationBase64", new[] { "certification_invalid" }));
                return BadRequest(validationProblemDetails);
            }

            var simDevice = _authenticationRepository.GetSimDevice(simMsisdn, simImsi, simIccId, subjectCn);
            if (simDevice == null)
            {
                Sim sim = _authenticationRepository.GetSim(simMsisdn, simImsi, simIccId);
                if (sim == null)
                {
                    _logger.LogWarning($"Not Found SIM:{simMsisdn}");
                }
                else
                {
                    CreateSimAndDeviceAuthenticationFailureLog(sim);
                    _radiusDbRepository.UpdateRadreply(sim.UserName + "@" + sim.SimGroup.UserNameSuffix);
                }
                return Unauthorized(NotMatchSimDevice);
            }
            // 認証成功のSimDeviceによって　それに対応する LoginできるUser を検索します
            var canLogonUsers = _authenticationRepository.GetLoginUsers(subjectCn);
            _radiusDbRepository.UpdateRadreply(simDevice.Sim.UserName + "@" + simDevice.Sim.SimGroup.UserNameSuffix, simDevice.IsolatedNw2Ip);
            CreateSimAndDeviceAuthenticationSuccessLog(simDevice);

            // SimDeviceによって　認証状態を検索する　すでに登録したら　SimAndDeviceAuthenticated　を更新します
            var simDeviceAuthenticationStateDone = CreateSimAndDeviceAuthenticated(simDevice);

            var simDeviceAuthenticationResponse =
                CreateSimDeviceAuthenticationResponse(simDeviceAuthenticationStateDone, simDevice, canLogonUsers);

            return Ok(simDeviceAuthenticationResponse);
        }


        private SimDeviceAuthenticationResponse CreateSimDeviceAuthenticationResponse(SimAndDeviceAuthenticated simAndDeviceAuthenticated, SimAndDevice simAndDevice, HashSet<string> canLogonUsers)
        {
            var simDeviceAuthenticationResponse = new SimDeviceAuthenticationResponse
            {
                AuthId = simAndDeviceAuthenticated.Id,
                CanLogonUsers = canLogonUsers,
                AssignDeviceIpAddress = simAndDevice.IsolatedNw2Ip,
                WindowsSignInListCacheDays = simAndDevice.Device.WindowsSignInListCacheDays,
                IsSoftwareRadioState = simAndDevice.Device.LteModule?.UseSoftwareRadioState,
                AuthenticationDuration = simAndDevice.AuthenticationDuration
            };

            if (simAndDevice.Device.OrganizationClientApp != null)
            {
                var clientInformation = new SimDeviceAuthenticationResponse.ClientInformation();
                clientInformation.Os = simAndDevice.Device.OrganizationClientApp?.ClientApp.ClientOs.Name;

                try
                {
                    clientInformation.Version = new Version(simAndDevice.Device.OrganizationClientApp?.ClientApp.Version);
                }
                catch (ArgumentException)
                {
                    _logger.LogWarning($"Invalid Version {simAndDevice.Device.OrganizationClientApp?.ClientApp.Version}");
                }

                simDeviceAuthenticationResponse.Client = clientInformation;
            }
            return simDeviceAuthenticationResponse;
        }

        private SimAndDeviceAuthenticated CreateSimAndDeviceAuthenticated(SimAndDevice simAndDevice)
        {
            string startTime = _configuration.GetSection("Auth:ExpireHour").Value;
            var simAndDeviceAuthenticated = simAndDevice.SimAndDeviceAuthenticated;
            if (simAndDeviceAuthenticated == null)
            {
                simAndDeviceAuthenticated = new SimAndDeviceAuthenticated
                {
                    Expiration = DateTime.Now.AddHours(double.Parse(startTime))
                };
                simAndDevice.SimAndDeviceAuthenticated = simAndDeviceAuthenticated;
                _authenticationRepository.Create(simAndDeviceAuthenticated);
            }
            else
            {
                simAndDeviceAuthenticated.Expiration =
                    DateTime.Now.AddHours(double.Parse(startTime));
                _authenticationRepository.Update(simAndDeviceAuthenticated);
            }
            return simAndDeviceAuthenticated;
        }

        private void CreateSimAndDeviceAuthenticationSuccessLog(SimAndDevice simAndDevice)
        {
            var simAndDeviceAuthenticationSuccessLog = new SimAndDeviceAuthenticationSuccessLog
            {
                SimAndDevice = simAndDevice,
                Time = DateTime.Now,
                Sim = simAndDevice.Sim
            };
            _authenticationRepository.Create(simAndDeviceAuthenticationSuccessLog);
        }

        private void CreateSimAndDeviceAuthenticationFailureLog(Sim sim)
        {
            var simAndDeviceAuthenticationFailureLog = new SimAndDeviceAuthenticationFailureLog()
            {
                Sim = sim,
                Time = DateTime.Now
            };
            _authenticationRepository.Create(simAndDeviceAuthenticationFailureLog);
        }
    }
}
