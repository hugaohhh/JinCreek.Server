using JinCreek.Server.Auth;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace JinCreek.Server.AuthTests.Controllers
{
    [Collection("Sequential")]
    public class ValidateTests : WebApplicationBase
    {
        public ValidateTests(WebApplicationFactory<Startup> factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
        {
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum ValidateAssertType
        {
            IMEI_REQUIRED = 1,
            IMEI_INVALID = 2,
            ICCID_REQUIRED = 3,
            ICCID_INVALID = 4,
            IMSI_REQUIRED = 5,
            IMSI_INVALID = 6,
            MSISDN_REQUIRED = 7,
            MSISDN_INVALID = 8,
            IP_INVALID = 9
        }

        class TestDataClass : IEnumerable<object[]>
        {
            List<object[]> _testData = new List<object[]>();

            public TestDataClass()
            {
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest , ValidateAssertType.IMEI_REQUIRED });
                //_testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "abcdef123456", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest });
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000111", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest ,ValidateAssertType.IMEI_INVALID});

                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest ,ValidateAssertType.ICCID_REQUIRED});
                //_testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "abcdef123456", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest });
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000111", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest ,ValidateAssertType.ICCID_INVALID});

                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest ,ValidateAssertType.IMSI_REQUIRED});
                //_testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "abcdef123456", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest });
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "4401032131000001111", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest, ValidateAssertType.IMSI_INVALID });

                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "" }, HttpStatusCode.BadRequest ,ValidateAssertType.MSISDN_REQUIRED});
                //_testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "abcdef123456" }, HttpStatusCode.BadRequest });
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "020179110001111111111" }, HttpStatusCode.BadRequest,ValidateAssertType.MSISDN_INVALID });
                //case14
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() {DeviceIpAddress  = "aaabbbccc",DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest,ValidateAssertType.IP_INVALID });
            }

            public IEnumerator<object[]> GetEnumerator() => _testData.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory(DisplayName = "バリデーションテスト")]
        [ClassData(typeof(TestDataClass))]
        public void Validation(SimDeviceAuthenticationRequest request, HttpStatusCode statusCode, ValidateAssertType type)
        {
            var acualRessult = PostSimDeviceAuthentication(request);

            Assert.Equal(statusCode, acualRessult.StatusCode);  
            var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
            
            Assert.NotNull(result["traceId"]);
            Assert.Equal("One or more validation errors occurred.", result["title"]);
            Assert.Equal("400",result["status"]);
            switch (type)
            {
                case ValidateAssertType.IMEI_REQUIRED: Assert.Equal("imei_required", result["errors"]["DeviceImei"].First().ToString()); break;
                case ValidateAssertType.IMEI_INVALID: Assert.Equal("imei_invalid_length", result["errors"]["DeviceImei"].First().ToString()); break;
                case ValidateAssertType.IMSI_REQUIRED: Assert.Equal("imsi_required", result["errors"]["SimImsi"].First().ToString()); break;
                case ValidateAssertType.IMSI_INVALID: Assert.Equal("The field SimImsi must be a string with a maximum length of 15.", result["errors"]["SimImsi"].First().ToString()); break;
                case ValidateAssertType.ICCID_REQUIRED: Assert.Equal("iccid_required", result["errors"]["SimIccId"].First().ToString()); break;
                case ValidateAssertType.ICCID_INVALID: Assert.Equal("The field SimIccId must be a string with a maximum length of 19.", result["errors"]["SimIccId"].First().ToString()); break;
                case ValidateAssertType.MSISDN_REQUIRED: Assert.Equal("msisdn_required", result["errors"]["SimMsisdn"].First().ToString()); break;
                case ValidateAssertType.MSISDN_INVALID: Assert.Equal("The field SimMsisdn must be a string with a maximum length of 15.", result["errors"]["SimMsisdn"].First().ToString()); break;
                case ValidateAssertType.IP_INVALID: Assert.Equal("ip_address_invalid", result["errors"]["DeviceIpAddress"].First().ToString()); break;
            }
        }
    }
        

    [Collection("Sequential")]
    public class DbTests : WebApplicationBase
    {

        public DbTests(WebApplicationFactory<Startup> factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
        {
        }

        private Organization _organization;
        private DeviceGroup _deviceGroup;
        private Lte _lte;
        private Domain _domain;
        private UserGroup _userGroup;
        private SimGroup _simGroup;
        private Radreply _radreply;
        public void InsertTestDataCaseXx()
        {
            _organization = new Organization
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
            _deviceGroup = new DeviceGroup
            {
                Version = "1.1",
                OsType = "Window10",
                Organization = _organization
            };
            _lte = new Lte
            {
                LteName = "Lte1",
                LteAdapter = "LteAdapter1",
                SoftwareRadioState = true
            };
            _domain = new Domain
            {
                DomainName = "Domain1",
                Organization = _organization,
            };
            _userGroup = new UserGroup
            {
                Domain = _domain,
                UserGroupName = "UserGroup1"
            };
            _simGroup = new SimGroup
            {
                SimGroupName = "SimGroup1",
                Organization = _organization,
                PrimaryDns = "255.0.0.0",
                SecondDns = "255.0.0.1",
                Apn = "SimGroupApn1",
                NasAddress = "NasAddress",
                Nw1AddressPool = "Nw1AddressPool",
                Nw1AddressRange = "Nw1AddressRange",
                ServerAddress = "127.0.0.1"
            };
            _radreply = new Radreply
            {
                Username = "user1",
                Op = "=",
                Attribute = "RadreplyAttribute1",
                Value = "Nw1Address"
            };
            AuthenticationRepository.Create(_organization);
            AuthenticationRepository.Create(_simGroup);
            AuthenticationRepository.Create(_domain);
            AuthenticationRepository.Create(_userGroup);
            AuthenticationRepository.Create(_lte);
            AuthenticationRepository.Create(_deviceGroup);
            RadiusRepository.Create(_radreply);
        }

        public void InsertTestDataCase21()
        {
            InsertTestDataCaseXx();
            var admin = new AdminUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                LastName = "管理人",
                FirstName = "一郎",
                Password = "password",
                AccountName = "AccountUser1"
            };
            var general = new GeneralUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                LastName = "一般",
                FirstName = "次郎",
                AccountName = "AccountUser2"
            };
            var device = new AdDevice
            {
                DeviceName = "Device1",
                DeviceImei = "352555093320000",
                ManagerNumber = "DeviceManager1",
                Type = "DeviceType1",
                Lte = _lte,
                DeviceGroup = _deviceGroup,
                Domain = _domain
            };
            var sim = new Sim
            {
                Msisdn = "02017911000",
                Imsi = "440103213100000",
                IccId = "8981100005819480000",
                SimGroup = _simGroup,
                Password = "123456",
                UserName = "user1"
            };
            var simDevice = new SimDevice
            {
                Sim = sim,
                Device = device,
                Nw2AddressPool = "Nw2Address",
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                AuthPeriod = 1
            };
            var adDeviceSettingOfflineWindowsSignIn = new AdDeviceSettingOfflineWindowsSignIn
            {
                AdDevice = device,
                WindowsSignInListCacheDays = 1
            };
            var factorCombination = new FactorCombination
            {
                SimDevice = simDevice,
                EndUser = admin,
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                NwAddress = "NwAddress",
            };
            var factorCombination2 = new FactorCombination
            {
                SimDevice = simDevice,
                EndUser = general,
                StartDay = DateTime.Now.AddHours(-6.00),
                NwAddress = "NwAddress"
            };
            AuthenticationRepository.Create(sim);
            AuthenticationRepository.Create(general);
            AuthenticationRepository.Create(admin);
            AuthenticationRepository.Create(device);
            AuthenticationRepository.Create(simDevice);
            AuthenticationRepository.Create(adDeviceSettingOfflineWindowsSignIn);
            AuthenticationRepository.Create(factorCombination);
            AuthenticationRepository.Create(factorCombination2);
        }

        [Fact(DisplayName = "DBテストCase21")]
        public void DbTestCase21()
        {
            InsertTestDataCase21();
            var request = new SimDeviceAuthenticationRequest()
            {
                DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000"
            };
            var acualRessult = PostSimDeviceAuthentication(request);
            Assert.Equal(HttpStatusCode.OK,acualRessult.StatusCode);
            var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
            Assert.Null(result["errorCode"]);
            Assert.Null(result["errorMessage"]);
            Assert.NotNull(result["authId"]);
            Assert.NotNull(result["assignDeviceIpAddress"]);
            Assert.NotNull(result["simDeviceConfigureDictionary"]);
            var simDeviceAuthenticationStateDone = AuthenticationRepository.GetSimDeviceAuthenticationDone(Guid.Parse(result["authId"].ToString()));
            Assert.NotNull(simDeviceAuthenticationStateDone);
            Assert.NotNull(simDeviceAuthenticationStateDone.SimDevice);
            var list = result["canLogonUsers"].ToImmutableList();
            Assert.NotEmpty(list);
            var testRadreply = RadiusRepository.GetRadreply(simDeviceAuthenticationStateDone.SimDevice.Sim.UserName);
            Assert.Equal(simDeviceAuthenticationStateDone.SimDevice.Nw2AddressPool, testRadreply.Value);
        }
        public void DbTestCase13()
        {
            var request = new SimDeviceAuthenticationRequest()
            {
                DeviceImei = "352555093320000",
                SimIccId = "8981100005819480000",
                SimImsi = "440103213100000",
                SimMsisdn = "02017911000"
            };
            var acualRessult = PostSimDeviceAuthentication(request);
            Assert.Equal(HttpStatusCode.OK, acualRessult.StatusCode);
            var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
            Assert.Null(result["errorCode"]);
            Assert.Null(result["errorMessage"]);
            Assert.NotNull(result["authId"]);
            Assert.NotNull(result["assignDeviceIpAddress"]);
            var testSimDeviceAuthenticationStateDone = AuthenticationRepository.GetSimDeviceAuthenticationDone(Guid.Parse(result["authId"].ToString()));
            Assert.NotNull(testSimDeviceAuthenticationStateDone);
            Assert.NotNull(testSimDeviceAuthenticationStateDone.SimDevice);
            var testRadreply = RadiusRepository.GetRadreply(testSimDeviceAuthenticationStateDone.SimDevice.Sim.UserName);
            Assert.Equal(testSimDeviceAuthenticationStateDone.SimDevice.Nw2AddressPool, testRadreply.Value);
            var list = result["canLogonUsers"].ToImmutableList();
            Assert.Empty(list);
        }

        public void InsertTestDataCase13_1()
        {
            InsertTestDataCaseXx();
            var device = new AdDevice
            {
                DeviceName = "Device1",
                DeviceImei = "352555093320000",
                ManagerNumber = "DeviceManager1",
                Type = "DeviceType1",
                Lte = _lte,
                DeviceGroup = _deviceGroup,
                Domain = _domain
            };
            var sim = new Sim
            {
                Msisdn = "02017911000",
                Imsi = "440103213100000",
                IccId = "8981100005819480000",
                SimGroup = _simGroup,
                Password = "123456",
                UserName = "user1"
            };
            var simDevice = new SimDevice
            {
                Sim = sim,
                Device = device,
                Nw2AddressPool = "Nw2Address",
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                AuthPeriod = 1
            };
           
            var simDeviceAuthenticationStateDone = new SimDeviceAuthenticationStateDone
            {
                SimDevice = simDevice,
                TimeLimit = DateTime.Now.AddHours(1.00)
            };
            AuthenticationRepository.Create(sim);
            AuthenticationRepository.Create(device);
            AuthenticationRepository.Create(simDevice);
            AuthenticationRepository.Create(simDeviceAuthenticationStateDone);
        }

        [Fact(DisplayName = "DBテストCase13_1")]
        public void DbTestCase13_1()
        {
            InsertTestDataCase13_1();
            DbTestCase13();
        }
        public void InsertTestDataCase13_2()
        {
            InsertTestDataCaseXx();
            var admin = new AdminUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                LastName = "管理人",
                FirstName = "一郎",
                Password = "password",
                AccountName = "AccountUser1"
            };
            var general = new GeneralUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                LastName = "一般",
                FirstName = "次郎",
                AccountName = "AccountUser2"
            };
            var device = new AdDevice
            {
                DeviceName = "Device1",
                DeviceImei = "352555093320000",
                ManagerNumber = "DeviceManager1",
                Type = "DeviceType1",
                Lte = _lte,
                DeviceGroup = _deviceGroup,
                Domain = _domain
            };
            var sim = new Sim
            {
                Msisdn = "02017911000",
                Imsi = "440103213100000",
                IccId = "8981100005819480000",
                SimGroup = _simGroup,
                Password = "123456",
                UserName = "user1"
            };
            var simDevice = new SimDevice
            {
                Sim = sim,
                Device = device,
                Nw2AddressPool = "Nw2Address",
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                AuthPeriod = 1
            };
            var factorCombination = new FactorCombination
            {
                SimDevice = simDevice,
                EndUser = admin,
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(-6.00),
                NwAddress = "NwAddress",
            };
            var factorCombination2 = new FactorCombination
            {
                SimDevice = simDevice,
                EndUser = general,
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(-6.00),
                NwAddress = "NwAddress"
            };
            var simDeviceAuthenticationStateDone = new SimDeviceAuthenticationStateDone
            {
                SimDevice = simDevice,
                TimeLimit = DateTime.Now.AddHours(1.00)
            };
            AuthenticationRepository.Create(sim);
            AuthenticationRepository.Create(general);
            AuthenticationRepository.Create(admin);
            AuthenticationRepository.Create(device);
            AuthenticationRepository.Create(simDevice);
            AuthenticationRepository.Create(factorCombination);
            AuthenticationRepository.Create(factorCombination2);
            AuthenticationRepository.Create(simDeviceAuthenticationStateDone);
        }
        [Fact(DisplayName = "DBテストCase13_2")]
        public void DbTestCase13_2()
        {
            InsertTestDataCase13_2();
            DbTestCase13();
        }

        public void InsertTestDataCase13_3()
        {
            InsertTestDataCaseXx();
            var admin = new AdminUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                LastName = "管理人",
                FirstName = "一郎",
                Password = "password",
                AccountName = "AccountUser1"
            };
            var general = new GeneralUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                LastName = "一般",
                FirstName = "次郎",
                AccountName = "AccountUser2"
            };
            var device = new AdDevice
            {
                DeviceName = "Device1",
                DeviceImei = "352555093320000",
                ManagerNumber = "DeviceManager1",
                Type = "DeviceType1",
                Lte = _lte,
                DeviceGroup = _deviceGroup,
                Domain = _domain
            };
            var sim = new Sim
            {
                Msisdn = "02017911000",
                Imsi = "440103213100000",
                IccId = "8981100005819480000",
                SimGroup = _simGroup,
                Password = "123456",
                UserName = "user1"
            };
            var simDevice = new SimDevice
            {
                Sim = sim,
                Device = device,
                Nw2AddressPool = "Nw2Address",
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                AuthPeriod = 1
            };
            var factorCombination = new FactorCombination
            {
                SimDevice = simDevice,
                EndUser = admin,
                StartDay = DateTime.Now.AddHours(6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                NwAddress = "NwAddress",
            };
            var factorCombination2 = new FactorCombination
            {
                SimDevice = simDevice,
                EndUser = general,
                StartDay = DateTime.Now.AddHours(6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                NwAddress = "NwAddress"
            };
            var simDeviceAuthenticationStateDone = new SimDeviceAuthenticationStateDone
            {
                SimDevice = simDevice,
                TimeLimit = DateTime.Now.AddHours(1.00)
            };
            AuthenticationRepository.Create(sim);
            AuthenticationRepository.Create(general);
            AuthenticationRepository.Create(admin);
            AuthenticationRepository.Create(device);
            AuthenticationRepository.Create(simDevice);
            AuthenticationRepository.Create(factorCombination);
            AuthenticationRepository.Create(factorCombination2);
            AuthenticationRepository.Create(simDeviceAuthenticationStateDone);
        }
        [Fact(DisplayName = "DBテストCase13_3")]
        public void DbTestCase13_3()
        {
            InsertTestDataCase13_3();
            DbTestCase13();
        }
       
    }
}
