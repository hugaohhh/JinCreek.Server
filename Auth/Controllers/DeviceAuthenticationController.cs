using JinCreek.Server.Auth.Interfaces;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace JinCreek.Server.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceAuthenticationController : ControllerBase
    {
        private readonly ILogger<DeviceAuthenticationController> _logger;

        private readonly MainDbRepository _repository;

        //private readonly MdbContext _context;

        //public DeviceAuthenticationController(MdbContext context)
        //{
        //    _context = context;
        //}

        public DeviceAuthenticationController(ILogger<DeviceAuthenticationController> logger, MainDbRepository repository)
        {
            _logger = logger;
            _repository = repository;
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

            Task.Delay(new Random().Next(100) * 1000);
            _logger.LogDebug("HELLO");


            //            var domain = new Domain { DomainName = "some.jp" };
            var userGroupName = "group name";


            //_repository.Create(userGroup);

            var userGroup2 = _repository.GetUserGroup(userGroupName);


            var admin = new AdminUser
            {
                Domain = userGroup2.Domain,
                UserGroup = userGroup2,
                LastName = "管理人",
                FirstName = "一郎",
                Password = "password"
            };

            var general = new GeneralUser
            {
                Domain = userGroup2.Domain,
                UserGroup = userGroup2,
                LastName = "一般",
                FirstName = "次郎",
            };

            _repository.Create(admin);
            _repository.Create(general);

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
