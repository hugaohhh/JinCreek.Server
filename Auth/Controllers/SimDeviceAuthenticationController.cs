using JinCreek.Server.Interfaces;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using System;
using System.Collections.Generic;

namespace JinCreek.Server.Auth.Controllers
{
    [Route("api/sim_device/authentication")]
    [ApiController]
    public class SimDeviceAuthenticationController : ControllerBase
    {
        private readonly ILogger<SimDeviceAuthenticationController> _logger;

        private readonly MainDbRepository _mainDbRepository;

        // private readonly RadiusDbRepository _radiusDbRepository;


        public SimDeviceAuthenticationController(ILogger<SimDeviceAuthenticationController> logger,
            MainDbRepository mainDbRepository)
        {
            _logger = logger;
            _mainDbRepository = mainDbRepository;
        }


        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(SimDeviceAuthenticationResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ErrorResponse))]
        public IActionResult Authentication(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest)
        {
            _logger.LogDebug("hello");
            return Ok(
                new SimDeviceAuthenticationResponse
                {
                    AuthId = Guid.NewGuid().ToString(),
                    CanLogonUsers = new List<string> { "user_aaaa", "user_bbbb" },
                    SimDeviceConfigureDictionary = new Dictionary<string, string>
                        {{"is_disconnect_network_screen_lock", true.ToString()}}
                }
            );
        }
    }
}
