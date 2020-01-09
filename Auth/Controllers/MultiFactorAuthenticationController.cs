using JinCreek.Server.Interfaces;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;

namespace JinCreek.Server.Auth.Controllers
{
    [Route("api/multi_factor/authentication")]
    [ApiController]
    public class MultiFactorAuthenticationController : ControllerBase
    {
        private readonly ILogger<MultiFactorAuthenticationController> _logger;

        private readonly UserRepository _mainDbRepository;

        // private readonly RadiusDbRepository _radiusDbRepository;


        public MultiFactorAuthenticationController(ILogger<MultiFactorAuthenticationController> logger,
            UserRepository mainDbRepository)
        {
            _logger = logger;
            _mainDbRepository = mainDbRepository;
        }


        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(MultiFactorAuthenticationResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ErrorResponse))]
        public IActionResult Authentication(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest)
        {
            _logger.LogDebug("hello");
            return Ok(
                    new MultiFactorAuthenticationResponse
                    {
                    });
        }
    }
}
