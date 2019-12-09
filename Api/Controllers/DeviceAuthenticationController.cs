using System;
using Api.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceAuthenticationController: ControllerBase
    {
        //private readonly MdbContext _context;

        //public DeviceAuthenticationController(MdbContext context)
        //{
        //    _context = context;
        //}

        [HttpGet]
        public ActionResult<DeviceAuthenticationResponse> GetDeviceAuthentication()
        {
            var auth = new DeviceAuthenticationResponse();
            auth.AuthId = Guid.NewGuid().ToString();
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
