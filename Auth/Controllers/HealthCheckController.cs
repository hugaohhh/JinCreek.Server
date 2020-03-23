using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using System.Linq;

namespace JinCreek.Server.Auth.Controllers
{
    [OpenApiTag("ヘルスチェック", Description = "ヘルスチェックを行う。(DBと接続し組織とradgroupcheckを取得する)")]
    [Route("api/healthcheck")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private readonly ILogger<HealthCheckController> _logger;

        private readonly UserRepository _userRepository;

        private readonly RadiusRepository _radiusRepository;

        public HealthCheckController(ILogger<HealthCheckController> logger, UserRepository userRepository, RadiusRepository radiusRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
            _radiusRepository = radiusRepository;
        }

        [HttpGet]
        //[SwaggerResponse(StatusCodes.Status200OK, typeof(NoContentResult))]
        public IActionResult HealthCheck()
        {
            var organizations = _userRepository.GetOrganization();
            _logger.LogDebug($"organization:{organizations.ToList().Count}");

            var radgroupcheckList = _radiusRepository.GetRadgroupcheckList();
            _logger.LogDebug($"radgroupcheck:{radgroupcheckList.ToList().Count}");

            return Ok();
        }
    }
}
