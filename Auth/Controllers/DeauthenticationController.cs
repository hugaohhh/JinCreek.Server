using JinCreek.Server.Common.Repositories;
using JinCreek.Server.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using System.Net.Mime;

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

        private readonly RadiusRepository _radiusRepository;


        public DeauthenticationController(ILogger<DeauthenticationController> logger,
            AuthenticationRepository authenticationRepository, RadiusRepository radiusRepository)
        {
            _logger = logger;
            _authenticationRepository = authenticationRepository;
            _radiusRepository = radiusRepository;
        }


        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(void), Description = "認証解除成功")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ValidationProblemDetails), Description = "リクエスト内容不正")]
        public IActionResult Deauthentication(DeauthenticationRequest deauthenticationRequest)
        {
            return Ok();
        }
    }
}
