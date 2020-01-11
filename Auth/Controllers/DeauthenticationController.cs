using System.Net.Mime;
using JinCreek.Server.Common.Repositories;
using JinCreek.Server.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JinCreek.Server.Auth.Controllers
{
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
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
            return Ok();
        }
    }
}
