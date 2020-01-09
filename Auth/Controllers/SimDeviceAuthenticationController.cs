using JinCreek.Server.Interfaces;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using JinCreek.Server.Common.Models;

namespace JinCreek.Server.Auth.Controllers
{
    [Route("api/sim_device/authentication")]
    [ApiController]
    public class SimDeviceAuthenticationController : ControllerBase
    {
        private readonly ILogger<SimDeviceAuthenticationController> _logger;

        private readonly AuthenticationRepository _authenticationRepository;

        private readonly RadiusRepository _radiusDbRepository;


        public SimDeviceAuthenticationController(ILogger<SimDeviceAuthenticationController> logger,
            AuthenticationRepository authenticationRepository,
            RadiusRepository radiusRepository)
        {
            _logger = logger;
            _authenticationRepository = authenticationRepository;
            _radiusDbRepository = radiusRepository;
        }

        public void InsertTestData()
        {
            var organization = new Organization
            {
                Code = "OrganizationCode1",
                Name = "OrganizationName1",
                Address = "OrganizationAddress1",
                DelegatePhone = "123465789",
                AdminPhone = "987654321",
                AdminMail = "Organization1@xx.com",
                StartDay = DateTime.Now,
                EndDay = DateTime.Now,
                Url = "Organization1.co.jp",
                IsValid = true,
            };
            var deviceGroup = new DeviceGroup
            {
                Version = "1.1",
                OsType = "Window10",
                Organization = organization
            };
            var lte = new Lte
            {
                LteName = "Lte1",
                LteAdapter = "LteAdapter1",
                SoftwareRadioState = true
            };
            var device = new Device
            {
                DeviceName = "Device1",
                DeviceImei = "DeviceImei1",
                ManagerNumber = "DeviceManager1",
                Type = "DeviceType1",
                Lte = lte,
                DeviceGroup = deviceGroup
            };
            var simGroup = new SimGroup
            {
                SimGroupName = "SimGroup1",
                Organization = organization,
                PrimaryDns = "255.0.0.0",
                SecondDns = "255.0.0.1",
                Apn = "SimGroupApn1",
                NasAddress = "127.0.0.0",
                Nw1AddressPool = "127.0.0.2",
                Nw1AddressRange = "127.0.0.2-127.1.1.2",
                ServerAddress = "127.0.0.1"
            };
            var sim = new Sim
            {
                Msisdn = "02017911000",
                Imsi = "440103213100000",
                IccId = "8981100005819480000",
                SimGroup = simGroup,
                Password = "123456",
                UserName = "user1"
            };
            var simDevice = new SimDevice
            {
                Sim = sim,
                Device = device,
                Nw2AddressPool = "127.1.1.3",
                StartDay = DateTime.Now,
                EndDay = DateTime.Now,
                AuthPeriod = 1
            };

            _authenticationRepository.Create(organization, simGroup, sim, deviceGroup, device, lte, simDevice);

        }

        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(SimDeviceAuthenticationResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ErrorResponse))]
        public IActionResult Authentication(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest)
        {
            _logger.LogDebug("hello");

            //InsertTestData();

            var simMsisdn = simDeviceAuthenticationRequest.SimMsisdn;
            var simImsi = simDeviceAuthenticationRequest.SimImsi;
            var simIccId = simDeviceAuthenticationRequest.SimIccId;
            var deviceImei = simDeviceAuthenticationRequest.DeviceImei;

            
            var list = _authenticationRepository.QuerySimDevice(simMsisdn, simImsi, simIccId, deviceImei);
            if (list.Count <= 0)
            {
                return Ok(new ErrorResponse
                {
                    ErrorCode = "10001",
                    ErrorMessage = "Not found record"
                });
            }
            var canLogonUsers = new List<string>();
            foreach (var simDevi in list)
            {
                canLogonUsers.Add(simDevi.Sim.UserName);
            }



            return Ok(
                new SimDeviceAuthenticationResponse
                {
                    AuthId = Guid.NewGuid().ToString(),
                    CanLogonUsers = canLogonUsers,
                    SimDeviceConfigureDictionary = new Dictionary<string, string>
                        {{"is_disconnect_network_screen_lock", true.ToString()}}
                }
            );
            //return null;
        }
    }
}
