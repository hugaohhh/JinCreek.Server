using System;
using System.Net.Mime;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Interfaces;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using static JinCreek.Server.Interfaces.ErrorResponse;
namespace JinCreek.Server.Auth.Controllers
{
    [OpenApiTag("認証解除", Description = "認証解除を行う。")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [Route("api/deauthentication")]
    [ApiController]
    public class DeauthenticationController : ControllerBase
    {
        private readonly ILogger<DeauthenticationController> _logger;

        private readonly AuthenticationRepository _authenticationRepository;


        public DeauthenticationController(ILogger<DeauthenticationController> logger,
            AuthenticationRepository authenticationRepository)
        {
            _logger = logger;
            _authenticationRepository = authenticationRepository;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public IActionResult Deauthentication(DeauthenticationRequest deauthenticationRequest)
        {
            _logger.LogDebug("hello");
            var deviceImei = deauthenticationRequest.DeviceImei;
            var simMsisdn = deauthenticationRequest.SimMsisdn;
            var simImsi = deauthenticationRequest.SimImsi;
            var simIccId = deauthenticationRequest.SimIccId;
            var account = deauthenticationRequest.Account;

            var simDevice = _authenticationRepository.GetSimDevice(simMsisdn, simImsi, simIccId, deviceImei);
            if (simDevice == null)
            {
                return Unauthorized(NotMatchSimDevice);
            }
            var factorCombination = _authenticationRepository.GetFactorCombination(account, simDevice);
            if (factorCombination == null)
            {
                //"SIM&端末認証済み"の対象レコードを削除する。(併せて"SIM&端末組合せ"のレコードを更新)
                _authenticationRepository.DeleteSimDeviceAuthDone(simDevice);
                return Unauthorized(NotMatchMultiFactor);
            }
            //"多要素認証済み"の対象レコードを削除する。(併せて"認証要素組合せ"のレコードを更新)
            _authenticationRepository.DeleteMultiFactorAuthDone(factorCombination);
            CreateDeauthentication(factorCombination);
            return Ok();
        }

        private void CreateDeauthentication(FactorCombination factorCombination)
        {
            var deauthentication = new Deauthentication
            {
                FactorCombination = factorCombination,
                ConnectionTime = DateTime.Now
            };
            _authenticationRepository.Create(deauthentication);
        }
    }
}
