using System.Linq;
using JinCreek.Server.Interfaces;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static JinCreek.Server.Interfaces.ErrorResponse;
namespace JinCreek.Server.Auth.Controllers
{
    [Route("api/deauthentication")]
    [ApiController]
    public class DeauthenticationController : ControllerBase
    {
        private readonly ILogger<DeauthenticationController> _logger;

        private readonly AuthenticationRepository _authenticationRepository;

        private readonly RadiusRepository _radiusRepository;


        public DeauthenticationController(ILogger<DeauthenticationController> logger,
            AuthenticationRepository authenticationRepository, RadiusRepository radiusRepository)
        {
            _logger = logger;
            _authenticationRepository = authenticationRepository;
            _radiusRepository = radiusRepository;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public IActionResult Deauthentication(DeauthenticationRequest deauthenticationRequest)
        {
            var deviceImei = deauthenticationRequest.DeviceImei;
            var simMsisdn = deauthenticationRequest.SimMsisdn;
            var simImsi = deauthenticationRequest.SimImsi;
            var simIccId = deauthenticationRequest.SimIccId;
            var account = deauthenticationRequest.Account;

            var simDevice = _authenticationRepository.QuerySimDevice(simMsisdn, simImsi, simIccId, deviceImei);
            if (simDevice == null)
            {
                return Unauthorized(NotMatchSimDevice);
            }

            var factorCombination = _authenticationRepository.QueryFactorCombination(account,simDevice.Id);
            if (factorCombination == null)
            {

            }
            return Ok();
        }
    }
}
