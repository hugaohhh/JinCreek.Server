using JinCreek.Server.Common.Repositories;
using JinCreek.Server.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using System.Net.Mime;

namespace JinCreek.Server.Auth.Controllers
{
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [Route("api/multi_factor/authentication")]
    [ApiController]
    public class MultiFactorAuthenticationController : ControllerBase
    {
        private readonly ILogger<MultiFactorAuthenticationController> _logger;

        private readonly AuthenticationRepository _authenticationRepository;

        private readonly RadiusRepository _radiusRepository;


        public MultiFactorAuthenticationController(ILogger<MultiFactorAuthenticationController> logger,
            AuthenticationRepository authenticationRepository, RadiusRepository radiusRepository)
        {
            _logger = logger;
            _authenticationRepository = authenticationRepository;
            _radiusRepository = radiusRepository;
        }


        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(MultiFactorAuthenticationResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ErrorResponse))]
        public IActionResult Authentication(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest)
        {
            _logger.LogDebug("hello");

            var account = multiFactorAuthenticationRequest.Account;
            var authId = multiFactorAuthenticationRequest.AuthId;

            

            return Ok(
                    new MultiFactorAuthenticationResponse());
        }
    }
}
