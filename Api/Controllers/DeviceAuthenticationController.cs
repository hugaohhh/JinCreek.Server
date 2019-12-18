using System;
using Api.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceAuthenticationController : ControllerBase
    {
        private readonly ILogger<DeviceAuthenticationController> _logger;
        //private readonly MdbContext _context;

        //public DeviceAuthenticationController(MdbContext context)
        //{
        //    _context = context;
        //}

        public DeviceAuthenticationController(ILogger<DeviceAuthenticationController> logger)
        {
            _logger = logger;
        }


        [HttpGet]
        public ActionResult<DeviceAuthenticationResponse> GetDeviceAuthentication()
        {
            using (NLog.NestedDiagnosticsContext.Push("ABCDEFG"))
            {
                _logger.LogDebug("hello");
            }
            _logger.LogDebug("start");
            var auth = new DeviceAuthenticationResponse();
            auth.AuthId = Guid.NewGuid().ToString();
            //_logger.LogTrace("trace log message");
            //_logger.LogDebug("debug log message");
            //_logger.LogInformation("info log message");
            //_logger.LogWarning("warn log message");

            //_logger.LogError("error log message");
            //Console.WriteLine("console log messages");

            Task.Delay(new System.Random().Next(100) * 1000);
            _logger.LogDebug("HELLO");
            return auth;
        }

        [HttpPost]
        public ActionResult<DeviceAuthenticationResponse> PostDeviceAuthentication(DeviceAuthenticationRequest deviceRequest)
        {
            var auth = new DeviceAuthenticationResponse();
            auth.AuthId = Guid.NewGuid().ToString();
            return auth;
        }
    }
}
