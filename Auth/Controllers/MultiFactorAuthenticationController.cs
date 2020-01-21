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
    [OpenApiTag("多要素認証", Description = "多要素認証を行う。成功時はユーザーに関わるサーバー側に保持する動的な設定項目を返却する。")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [Route("api/multi_factor/authentication")]
    [ApiController]
    public class MultiFactorAuthenticationController : ControllerBase
    {
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        private readonly ILogger<MultiFactorAuthenticationController> _logger;

        private readonly AuthenticationRepository _authenticationRepository;

        private readonly RadiusRepository _radiusRepository;
        private readonly IConfiguration _configuration;

        public MultiFactorAuthenticationController(ILogger<MultiFactorAuthenticationController> logger,
            AuthenticationRepository authenticationRepository, RadiusRepository radiusRepository,
            IConfiguration configuration)
        {
            _logger = logger;
            _authenticationRepository = authenticationRepository;
            _radiusRepository = radiusRepository;
            _configuration = configuration;
        }

        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(SimDeviceAuthenticationResponse), Description = "認証成功")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ValidationProblemDetails), Description = "リクエスト内容不正")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ErrorResponse), Description = "認証失敗")]
        public IActionResult Authentication(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest)
        {
            var account = multiFactorAuthenticationRequest.Account;
            Guid authId = multiFactorAuthenticationRequest.AuthId;

            var factorCombination = _authenticationRepository.GetFactorCombination(account, authId);
            if (factorCombination == null)
            {
                var simDevice = _authenticationRepository.GetSimDevice(authId);
                if (simDevice == null)
                {
                    return Unauthorized(NotMatchAuthId);
                }
                _radiusRepository.UpdateRadreply(simDevice, null, false);
                CreateMultiFactorAuthenticationLogFail(simDevice);
                return Unauthorized(NotMatchMultiFactor);
            }

            _radiusRepository.UpdateRadreply(factorCombination.SimDevice, factorCombination, true);
            CreateMultiFactorAuthenticationLogSuccess(factorCombination);

            // factorCombination によって　認証状態を検索する　すでに登録したら　MultiFactorAuthenticationStateDone　を更新します
            CreateMultiFactorAuthenticationStateDone(factorCombination);

            var multiFactorAuthenticationResponse = CreateMultiFactorAuthenticationResponse(factorCombination);
            return Ok(multiFactorAuthenticationResponse);
        }

        private MultiFactorAuthenticationResponse CreateMultiFactorAuthenticationResponse(FactorCombination factorCombination)
        {
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
            return multiFactorAuthenticationResponse;
        }

        private void CreateMultiFactorAuthenticationStateDone(FactorCombination factorCombination)
        {
            string startTime = _configuration.GetSection("Auth:ExpireHour").Value;
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
            else
            {
                multiFactorAuthenticationStateDone.TimeLimit =
                    DateTime.Now.AddHours(double.Parse(startTime));
                _authenticationRepository.Update(multiFactorAuthenticationStateDone);
            }
        }

        private void CreateMultiFactorAuthenticationLogSuccess(FactorCombination factorCombination)
        {
            var multiFactorAuthenticationLogSuccess = new MultiFactorAuthenticationLogSuccess
            {
                ConnectionTime = DateTime.Now,
                FactorCombination = factorCombination
            };
            _authenticationRepository.Create(multiFactorAuthenticationLogSuccess);
        }

        private void CreateMultiFactorAuthenticationLogFail(SimDevice simDevice)
        {
            var multiFactorAuthenticationLogFail = new MultiFactorAuthenticationLogFail
            {
                SimDevice = simDevice,
                ConnectionTime = DateTime.Now
            };
            _authenticationRepository.Create(multiFactorAuthenticationLogFail);
        }
    }
}
