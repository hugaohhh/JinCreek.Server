using JinCreek.Server.Interfaces;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using JinCreek.Server.Common.Models;
using Microsoft.Extensions.Configuration;
using static JinCreek.Server.Interfaces.ErrorResponse;

namespace JinCreek.Server.Auth.Controllers
{
    [OpenApiTag("SIM＆端末認証", Description = "SIM＆端末認証を行う。成功時はログイン可能ユーザ一覧、クライアント(デバイスやSIM)機器に関わるサーバー側に保持する動的な設定項目を返却する。")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [Route("api/sim_device/authentication")]
    [ApiController]
    public class SimDeviceAuthenticationController : ControllerBase
    {
        private readonly ILogger<SimDeviceAuthenticationController> _logger;

        private readonly AuthenticationRepository _authenticationRepository;

        private readonly RadiusRepository _radiusDbRepository;
        private IConfiguration Configuration { get; }

        public SimDeviceAuthenticationController(ILogger<SimDeviceAuthenticationController> logger,
            AuthenticationRepository authenticationRepository,
            RadiusRepository radiusRepository,
            IConfiguration configuration)
        {
            _logger = logger;
            _authenticationRepository = authenticationRepository;
            _radiusDbRepository = radiusRepository;
            Configuration = configuration;
        }

        public void InsertTestData()
        {
            //var organization = new Organization
            //{
            //    Code = "OrganizationCode1",
            //    Name = "OrganizationName1",
            //    Address = "OrganizationAddress1",
            //    DelegatePhone = "123465789",
            //    AdminPhone = "987654321",
            //    AdminMail = "Organization1@xx.com",
            //    StartDay = DateTime.Now,
            //    EndDay = DateTime.Now,
            //    Url = "Organization1.co.jp",
            //    IsValid = true,
            //};
            //var deviceGroup = new DeviceGroup
            //{
            //    Version = "1.1",
            //    OsType = "Window10",
            //    Organization = organization
            //};
            //var lte = new Lte
            //{
            //    LteName = "Lte1",
            //    LteAdapter = "LteAdapter1",
            //    SoftwareRadioState = true
            //};
            //var device = new Device
            //{
            //    DeviceName = "Device1",
            //    DeviceImei = "DeviceImei1",
            //    ManagerNumber = "DeviceManager1",
            //    Type = "DeviceType1",
            //    Lte = lte,
            //    DeviceGroup = deviceGroup
            //};
            //var simGroup = new SimGroup
            //{
            //    SimGroupName = "SimGroup1",
            //    Organization = organization,
            //    PrimaryDns = "255.0.0.0",
            //    SecondDns = "255.0.0.1",
            //    Apn = "SimGroupApn1",
            //    NasAddress = "127.0.0.0",
            //    Nw1AddressPool = "127.0.0.2",
            //    Nw1AddressRange = "127.0.0.2-127.1.1.2",
            //    ServerAddress = "127.0.0.1"
            //};
            //var sim = new Sim
            //{
            //    Msisdn = "02017911000",
            //    Imsi = "440103213100000",
            //    IccId = "8981100005819480000",
            //    SimGroup = simGroup,
            //    Password = "123456",
            //    UserName = "user1"
            //};
            //var simDevice = new SimDevice
            //{
            //    Sim = sim,
            //    Device = device,
            //    Nw2AddressPool = "127.1.1.3",
            //    StartDay = DateTime.Now,
            //    EndDay = DateTime.Now,
            //    AuthPeriod = 1
            //};
            //var radreply = new Radreply
            //{
            //    Username = "user1",
            //    Op = "=",
            //    Attribute = "RadreplyAttribute1",
            //    Value = "127.0.0.2"
            //};

            //var domain = new Domain
            //{
            //    DomainName = "Domain1",
            //    Organization = organization,
            //};
            //var userGroup = new UserGroup
            //{
            //    Domain = domain,
            //    UserGroupName = "UserGroup1"
            //};
            //var admin = new AdminUser
            //{
            //    Domain = userGroup.Domain,
            //    UserGroup = userGroup,
            //    LastName = "管理人",
            //    FirstName = "一郎",
            //    Password = "password",
            //    AccountName = "AccountUser1"
            //};

            //var general = new GeneralUser
            //{
            //    Domain = userGroup.Domain,
                //    UserGroup = userGroup,
                //    LastName = "一般",
                //    FirstName = "次郎",
                //    AccountName = "AccountUser2"
                //};
                //var superAdminUser = new SuperAdminUser
                //{
                //    LastName = "Super管理人",
                //    FirstName = "太郎",
                //    Password = "password",
                //    AccountName = "AccountUser3"
                //};

            //    var userGroup = _authenticationRepository.GetUserGroup("UserGroup1");
            //    var general2 = new GeneralUser
            //{
            //    Domain = userGroup.Domain,
            //    UserGroup = userGroup,
            //    LastName = "一般",
            //    FirstName = "次郎2",
            //    AccountName = "AccountUser4",
            //    IsDisconnectWhenScreenLock = true
            //};
            //var general3 = new GeneralUser
            //{
            //    Domain = userGroup.Domain,
            //    UserGroup = userGroup,
            //    LastName = "一般",
            //    FirstName = "次郎3",
            //    AccountName = "AccountUser5",
            //    IsDisconnectWhenScreenLock = true
            //};
            //var factorCombination = new FactorCombination
            //{
            //    SimDevice = simDevice,
            //    EndUser = admin,
            //    StartDay = DateTime.Now,
            //    EndDay = DateTime.Now,
            //    NwAddress = "NwAddress",
            //};
            //var factorCombination2 = new FactorCombination
            //{
            //    SimDevice = simDevice,
            //    EndUser = general,
            //    StartDay = DateTime.Now,
            //    EndDay = DateTime.Now,
            //    NwAddress = "NwAddress",
            //};
            //_authenticationRepository.Create(organization, simGroup, sim, deviceGroup, device, lte, simDevice,domain,userGroup);
            //_authenticationRepository.Create(factorCombination);
            //_authenticationRepository.Create(factorCombination2);
            //_authenticationRepository.Create(general);
            //_authenticationRepository.Create(admin);
            //_authenticationRepository.Create(superAdminUser);
            //_radiusDbRepository.Create(radreply);
            //_authenticationRepository.Create(general2);
            //_authenticationRepository.Create(general3);

        }

        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(SimDeviceAuthenticationResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ErrorResponse))]
        public IActionResult Authentication(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest)
        {
            _logger.LogDebug("hello");

            var simMsisdn = simDeviceAuthenticationRequest.SimMsisdn;
            var simImsi = simDeviceAuthenticationRequest.SimImsi;
            var simIccId = simDeviceAuthenticationRequest.SimIccId;
            var deviceImei = simDeviceAuthenticationRequest.DeviceImei;

            var simDevice = _authenticationRepository.GetSimDevice(simMsisdn, simImsi, simIccId, deviceImei);
            if (simDevice == null)
            {
                Sim sim = _authenticationRepository.GetSim(simMsisdn, simImsi, simIccId);
                CreateSimDeviceAuthenticationFalse(sim);
                _radiusDbRepository.UpdateRadreply(sim, false);
                return Unauthorized(NotMatchSimDevice);
            }
            // 認証成功のSimDeviceによって　それに対応する LoginできるUser を検索します
            var canLogonUsers = _authenticationRepository.GetLoginUsers(simDevice);
            _radiusDbRepository.UpdateRadreply(simDevice.Sim, true);
            CreateSimDeviceAuthenticationSuccess(simDevice);

            string startTime = Configuration.GetSection("Auth:ExpireHour").Value;
            // SimDeviceによって　認証状態を検索する　すでに登録したら　SimDeviceAuthenticationStateDone　を更新します
            var simDeviceAuthenticationStateDone = CreateSimDeviceAuthenticationStateDone(simDevice,startTime);
            
            var simDeviceAuthenticationResponse = 
                CreateSimDeviceAuthenticationResponse(simDeviceAuthenticationStateDone, simDevice,canLogonUsers);

            // AdDevice がある場合　AdDeviceSettingOfflineWindowsSignIn　を設置します　逆にない場合　設置しない
            UpdateSimDeviceAuthenticationResponse(simDeviceAuthenticationResponse, simDevice);
            return Ok(simDeviceAuthenticationResponse);
        }

        private void UpdateSimDeviceAuthenticationResponse(SimDeviceAuthenticationResponse simDeviceAuthenticationResponse, SimDevice simDevice)
        {
            AdDevice adDevice = _authenticationRepository.GetAdDevice(simDevice.Device.Id);
            if (adDevice != null)
            {
                simDeviceAuthenticationResponse.SimDeviceConfigureDictionary
                    = new Dictionary<string, string>
                    {
                        {
                            "windows_signIn_cache_days",
                            adDevice.AdDeviceSettingOfflineWindowsSignIn.WindowsSignInListCacheDays.ToString()
                        }
                    };
            }
        }

        private SimDeviceAuthenticationResponse CreateSimDeviceAuthenticationResponse(SimDeviceAuthenticationStateDone simDeviceAuthenticationStateDone, SimDevice simDevice,List<string> canLogonUsers)
        {
            var simDeviceAuthenticationResponse = new SimDeviceAuthenticationResponse
            {
                AuthId = simDeviceAuthenticationStateDone.Id.ToString(),
                CanLogonUsers = canLogonUsers,
                AssignDeviceIpAddress = simDevice.Nw2AddressPool
            };
            return simDeviceAuthenticationResponse;
        }

        private SimDeviceAuthenticationStateDone CreateSimDeviceAuthenticationStateDone(SimDevice simDevice,string startTime)
        {
            var simDeviceAuthenticationStateDone = simDevice.SimDeviceAuthenticationStateDone;
            if (simDeviceAuthenticationStateDone == null)
            {
                simDeviceAuthenticationStateDone = new SimDeviceAuthenticationStateDone
                {
                    SimDevice = simDevice,
                    TimeLimit = DateTime.Now.AddHours(double.Parse(startTime))
                };
                _authenticationRepository.Create(simDeviceAuthenticationStateDone);
            }
            else
            {
                simDeviceAuthenticationStateDone.TimeLimit =
                    DateTime.Now.AddHours(double.Parse(startTime));
                _authenticationRepository.Update(simDeviceAuthenticationStateDone);
            }
            return simDeviceAuthenticationStateDone;
        }

        private void CreateSimDeviceAuthenticationSuccess(SimDevice simDevice)
        {
            var simDeviceAuthentication = new SimDeviceAuthenticationLogSuccess
            {
                SimDevice = simDevice,
                ConnectionTime = DateTime.Now
            };
            _authenticationRepository.Create(simDeviceAuthentication);
        }

        private void CreateSimDeviceAuthenticationFalse(Sim sim)
        {
            var simDeviceAuthenticationFalse = new SimDeviceAuthenticationLogFail()
            {
                Sim = sim,
                ConnectionTime = DateTime.Now
            };
            _authenticationRepository.Create(simDeviceAuthenticationFalse);
        }
    }
}
