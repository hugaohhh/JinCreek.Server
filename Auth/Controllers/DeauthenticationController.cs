using JinCreek.Server.Interfaces;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JinCreek.Server.Auth.Controllers
{
    [Route("api/deauthentication")]
    [ApiController]
    public class DeauthenticationController : ControllerBase
    {
        private readonly ILogger<DeauthenticationController> _logger;

        private readonly MainDbRepository _mainDbRepository;

        // private readonly RadiusDbRepository _radiusDbRepository;


        public DeauthenticationController(ILogger<DeauthenticationController> logger,
            MainDbRepository mainDbRepository)
        {
            _logger = logger;
            _mainDbRepository = mainDbRepository;
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
