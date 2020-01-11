using JinCreek.Server.Interfaces;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using JinCreek.Server.Common.Models;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Configuration;
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
        private readonly ILogger<SimDeviceAuthenticationController> _logger;

        private readonly AuthenticationRepository _authenticationRepository;

        private readonly RadiusRepository _radiusDbRepository;
        public IConfiguration Configuration { get; }

        public SimDeviceAuthenticationController(ILogger<SimDeviceAuthenticationController> logger,
            AuthenticationRepository authenticationRepository,
            RadiusRepository radiusRepository,
            IConfiguration configuration)
        {
            _logger = logger;
            _authenticationRepository = authenticationRepository;
            _radiusDbRepository = radiusRepository;
            Configuration = configuration;
        }

       

        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(SimDeviceAuthenticationResponse), Description = "認証成功")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ValidationProblemDetails), Description = "リクエスト内容不正")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ErrorResponse), Description = "認証失敗")]
        public IActionResult Authentication(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest)
        {
            _logger.LogDebug("hello");

            return Unauthorized(NotMatchSimDevice);

            //InsertTestData();

            //var simMsisdn = simDeviceAuthenticationRequest.SimMsisdn;
            //var simImsi = simDeviceAuthenticationRequest.SimImsi;
            //var simIccId = simDeviceAuthenticationRequest.SimIccId;
            //var deviceImei = simDeviceAuthenticationRequest.DeviceImei;

            //var list = _authenticationRepository.QuerySimDevice(simMsisdn, simImsi, simIccId, deviceImei);
            //string startTime = Configuration.GetSection("Auth:ExpireHour").Value;
            //if (list.Count <= 0)
            //{
            //    var simDeviceAuthenticationFalse = new SimDeviceAuthentication
            //    {
            //        SimDevice = null,
            //        IsAuthResult = false,
            //        ConnectionTime = DateTime.Now,
            //        SendByte = 0,
            //        ReceviByte = 0
            //    };

            //    //_authenticationRepository.Create(simDeviceAuthenticationFalse);
            //    Sim sim = _authenticationRepository.QuerySim(simMsisdn, simImsi, simIccId);
            //    if (sim == null)
            //    {
            //        return Unauthorized(new ErrorResponse
            //        {
            //            ErrorCode = "10001",
            //            ErrorMessage = "Not found Sim record"
            //        });
            //    }
            //    _radiusDbRepository.SimDeviceAuthUpdateRadreply(sim, false);

            //    return Unauthorized(new ErrorResponse
            //    {
            //        ErrorCode = "10001",
            //        ErrorMessage = "Not found record"
            //    });
            //}

            //// SimDeviceによって　認証状態を検索する　すでに登録したら　Errorメッセージを返事します
            ////AuthenticationState authentication =  _authenticationRepository.QueryAuthenticationStateBySimDevice(list.First());

            //// 認証成功のSimDeviceによって　それに対応する FactorCombination を検索します
            //var canLogonUsers = _authenticationRepository.QueryFactorCombination(list.First());

            //_radiusDbRepository.SimDeviceAuthUpdateRadreply(list.First().Sim, true);

            //var simDeviceAuthentication = new SimDeviceAuthentication
            //{
            //    SimDevice = list.First(),
            //    IsAuthResult = true,
            //    ConnectionTime = DateTime.Now,
            //    SendByte = 0,
            //    ReceviByte = 0
            //};

            //var simDeviceAuthenticationEnd = new SimDeviceAuthenticationEnd
            //{
            //    SimDevice = list.First(),
            //    TimeLimit = DateTime.Now.AddHours(double.Parse(startTime))
            //};
            //_authenticationRepository.Create(simDeviceAuthentication);
            //_authenticationRepository.Create(simDeviceAuthenticationEnd);
            //var simDeviceAuthenticationResponse = new SimDeviceAuthenticationResponse
            //{
            //    AuthId = Guid.NewGuid().ToString(),
            //    CanLogonUsers = canLogonUsers,
            //    SimDeviceConfigureDictionary = new Dictionary<string, string>
            //        {{"is_disconnect_network_screen_lock", true.ToString()}}
            //};

            //return Ok(simDeviceAuthenticationResponse);

            //return null;
        }
    }
}
