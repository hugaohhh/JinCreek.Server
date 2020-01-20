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
    [Route("api/sim_device/authentication")]
    [ApiController]
    public class SimDeviceAuthenticationController : ControllerBase
    {
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        private readonly ILogger<SimDeviceAuthenticationController> _logger;
        private readonly AuthenticationRepository _authenticationRepository;
        private readonly RadiusRepository _radiusDbRepository;
        private readonly IConfiguration _configuration;

        public SimDeviceAuthenticationController(ILogger<SimDeviceAuthenticationController> logger,
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
            var deviceImei = simDeviceAuthenticationRequest.DeviceImei;

            var simDevice = _authenticationRepository.GetSimDevice(simMsisdn, simImsi, simIccId, deviceImei);
            if (simDevice == null)
            {
                Sim sim = _authenticationRepository.GetSim(simMsisdn, simImsi, simIccId);
                CreateSimDeviceAuthenticationFail(sim);
                _radiusDbRepository.UpdateRadreply(sim, false);
                return Unauthorized(NotMatchSimDevice);
            }
            // 認証成功のSimDeviceによって　それに対応する LoginできるUser を検索します
            var canLogonUsers = _authenticationRepository.GetLoginUsers(simDevice);
            _radiusDbRepository.UpdateRadreply(simDevice.Sim, true);
            CreateSimDeviceAuthenticationSuccess(simDevice);

            // SimDeviceによって　認証状態を検索する　すでに登録したら　SimDeviceAuthenticationStateDone　を更新します
            var simDeviceAuthenticationStateDone = CreateSimDeviceAuthenticationStateDone(simDevice);

            var simDeviceAuthenticationResponse =
                CreateSimDeviceAuthenticationResponse(simDeviceAuthenticationStateDone, simDevice, canLogonUsers);

            return Ok(simDeviceAuthenticationResponse);
        }

        private SimDeviceAuthenticationResponse CreateSimDeviceAuthenticationResponse(SimDeviceAuthenticationStateDone simDeviceAuthenticationStateDone, SimDevice simDevice, List<string> canLogonUsers)
        {
            var simDeviceAuthenticationResponse = new SimDeviceAuthenticationResponse
            {
                AuthId = simDeviceAuthenticationStateDone.Id.ToString(),
                CanLogonUsers = canLogonUsers,
                AssignDeviceIpAddress = simDevice.Nw2IpAddressPool
            };
            // AdDevice がある場合　AdDeviceSettingOfflineWindowsSignIn　を設置します　逆にない場合　設置しない
            AdDevice adDevice = _authenticationRepository.GetAdDevice(simDevice.Device.Id);
            if (adDevice != null && adDevice.AdDeviceSettingOfflineWindowsSignIn != null)
            {
                simDeviceAuthenticationResponse.SimDeviceConfigureDictionary
                    = new Dictionary<string, string>
                    {
                        {
                            "windows_signIn_cache_days",
                            adDevice.AdDeviceSettingOfflineWindowsSignIn.WindowsSignInListCacheDays.ToString()
                        }
                    };
            }
            return simDeviceAuthenticationResponse;
        }

        private SimDeviceAuthenticationStateDone CreateSimDeviceAuthenticationStateDone(SimDevice simDevice)
        {
            string startTime = _configuration.GetSection("Auth:ExpireHour").Value;
            var simDeviceAuthenticationStateDone = simDevice.SimDeviceAuthenticationStateDone;
            if (simDeviceAuthenticationStateDone == null)
            {
                simDeviceAuthenticationStateDone = new SimDeviceAuthenticationStateDone
                {
                    SimDevice = simDevice,
                    TimeLimit = DateTime.Now.AddHours(double.Parse(startTime))
                };
                _authenticationRepository.Create(simDeviceAuthenticationStateDone);
            }
            else
            {
                simDeviceAuthenticationStateDone.TimeLimit =
                    DateTime.Now.AddHours(double.Parse(startTime));
                _authenticationRepository.Update(simDeviceAuthenticationStateDone);
            }
            return simDeviceAuthenticationStateDone;
        }

        private void CreateSimDeviceAuthenticationSuccess(SimDevice simDevice)
        {
            var simDeviceAuthentication = new SimDeviceAuthenticationLogSuccess
            {
                SimDevice = simDevice,
                ConnectionTime = DateTime.Now
            };
            _authenticationRepository.Create(simDeviceAuthentication);
        }

        private void CreateSimDeviceAuthenticationFail(Sim sim)
        {
            var simDeviceAuthenticationFalse = new SimDeviceAuthenticationLogFail()
            {
                Sim = sim,
                ConnectionTime = DateTime.Now
            };
            _authenticationRepository.Create(simDeviceAuthenticationFalse);
        }
    }
}
