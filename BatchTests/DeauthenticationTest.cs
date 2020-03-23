using ConsoleAppFramework;
using JinCreek.Server.Batch.Repositories;
using JinCreek.Server.Common.Models;
using Xunit;
using Xunit.Abstractions;

namespace JinCreek.Server.Batch
{
    [Collection("Sequential")]
    public class DeauthenticationTest: BatchTestBase
    {
        private readonly BatchTestRepository _batchTestRepository;
        private readonly DeauthenticationTestSetupRepository _deauthenticationTestSetupRepository;

        public DeauthenticationTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _batchTestRepository = new BatchTestRepository(MainDbContext, RadiusDbContext);
            _deauthenticationTestSetupRepository = new DeauthenticationTestSetupRepository(MainDbContext, RadiusDbContext);
        }

        private (SimAndDeviceAuthenticated, MultiFactorAuthenticated, DeauthenticationLog) GetRecordsForAssert()
        {
            int targetOrganizationCode = _deauthenticationTestSetupRepository.OrganizationCode;
            var simAndDevice1 = _deauthenticationTestSetupRepository.SimAndDevice1;
            var multiFactor1 = _deauthenticationTestSetupRepository.MultiFactor1;

            var simAndDeviceAuthenticated = _batchTestRepository.GetSimAndDeviceAuthenticatedBySimAndDeviceId(targetOrganizationCode, simAndDevice1?.Id);
            var multiFactorAuthenticated = _batchTestRepository.GetMultiFactorAuthenticatedByMultiFactorId(targetOrganizationCode, multiFactor1?.Id);
            var deauthenticationLog = _batchTestRepository.GetDeauthenticationLogByMultiFactorId(targetOrganizationCode, multiFactor1?.Id);

            return (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog);
        }

        [Fact]
        public void TestCase01()
        {
            _deauthenticationTestSetupRepository.CreateDataCase01();

            var args = new[] { "deauthentication" };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.Null(simAndDeviceAuthenticated);
            Assert.Null(multiFactorAuthenticated);
            Assert.Null(deauthenticationLog);
        }

        [Fact]
        public void TestCase02()
        {
            _deauthenticationTestSetupRepository.CreateDataCase02();

            var args = new[] { "deauthentication", "-organization_code", "1" };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.Null(simAndDeviceAuthenticated);
            Assert.Null(multiFactorAuthenticated);
            Assert.Null(deauthenticationLog);
        }

        [Fact]
        public void TestCase03()
        {
            _deauthenticationTestSetupRepository.CreateDataCase03();

            var args = new[] { "deauthentication", "-organization_code", _deauthenticationTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.Null(simAndDeviceAuthenticated);
            Assert.NotNull(multiFactorAuthenticated);
            Assert.Null(deauthenticationLog);
        }

        [Fact]
        public void TestCase04()
        {
            _deauthenticationTestSetupRepository.CreateDataCase04();

            var args = new[] { "deauthentication", "-organization_code", _deauthenticationTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.Null(simAndDeviceAuthenticated);
            Assert.NotNull(multiFactorAuthenticated);
            Assert.Null(deauthenticationLog);
        }

        [Fact]
        public void TestCase05()
        {
            _deauthenticationTestSetupRepository.CreateDataCase05();

            var args = new[] { "deauthentication", "-organization_code", _deauthenticationTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.Null(simAndDeviceAuthenticated);
            Assert.NotNull(multiFactorAuthenticated);
            Assert.Null(deauthenticationLog);
        }

        [Fact]
        public void TestCase06()
        {
            _deauthenticationTestSetupRepository.CreateDataCase06();

            var args = new[] { "deauthentication", "-organization_code", _deauthenticationTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.Null(simAndDeviceAuthenticated);
            Assert.Null(multiFactorAuthenticated);
            Assert.Null(deauthenticationLog);
        }

        [Fact]
        public void TestCase07()
        {
            _deauthenticationTestSetupRepository.CreateDataCase07();

            var args = new[] { "deauthentication", "-organization_code", _deauthenticationTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.Null(simAndDeviceAuthenticated);
            Assert.Null(multiFactorAuthenticated);
            Assert.NotNull(deauthenticationLog);
        }

        [Fact]
        public void TestCase08()
        {
            _deauthenticationTestSetupRepository.CreateDataCase08();

            var args = new[] { "deauthentication", "-organization_code", _deauthenticationTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.Null(simAndDeviceAuthenticated);
            Assert.NotNull(multiFactorAuthenticated);
            Assert.Null(deauthenticationLog);
        }

        [Fact]
        public void TestCase09()
        {
            _deauthenticationTestSetupRepository.CreateDataCase09();

            var args = new[] { "deauthentication", "-organization_code", _deauthenticationTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.Null(simAndDeviceAuthenticated);
            Assert.NotNull(multiFactorAuthenticated);
            Assert.Null(deauthenticationLog);
        }

        [Fact]
        public void TestCase10()
        {
            _deauthenticationTestSetupRepository.CreateDataCase10();

            var args = new[] { "deauthentication", "-organization_code", _deauthenticationTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.NotNull(simAndDeviceAuthenticated);
            Assert.Null(multiFactorAuthenticated);
            Assert.Null(deauthenticationLog);
        }

        [Fact]
        public void TestCase11()
        {
            _deauthenticationTestSetupRepository.CreateDataCase11();

            var args = new[] { "deauthentication", "-organization_code", _deauthenticationTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.NotNull(simAndDeviceAuthenticated);
            Assert.NotNull(multiFactorAuthenticated);
            Assert.Null(deauthenticationLog);
        }
                [Fact]
        public void TestCase12()
        {
            _deauthenticationTestSetupRepository.CreateDataCase12();

            var args = new[] { "deauthentication", "-organization_code", _deauthenticationTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.NotNull(simAndDeviceAuthenticated);
            Assert.NotNull(multiFactorAuthenticated);
            Assert.Null(deauthenticationLog);
        }

        [Fact]
        public void TestCase13()
        {
            _deauthenticationTestSetupRepository.CreateDataCase13();

            var args = new[] { "deauthentication", "-organization_code", _deauthenticationTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.NotNull(simAndDeviceAuthenticated);
            Assert.NotNull(multiFactorAuthenticated);
            Assert.Null(deauthenticationLog);
        }

        [Fact]
        public void TestCase14()
        {
            _deauthenticationTestSetupRepository.CreateDataCase14();

            var args = new[] { "deauthentication", "-organization_code", _deauthenticationTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.NotNull(simAndDeviceAuthenticated);
            Assert.Null(multiFactorAuthenticated);
            Assert.Null(deauthenticationLog);
        }

        [Fact]
        public void TestCase15()
        {
            _deauthenticationTestSetupRepository.CreateDataCase15();

            var args = new[] { "deauthentication", "-organization_code", _deauthenticationTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.NotNull(simAndDeviceAuthenticated);
            Assert.NotNull(multiFactorAuthenticated);
            Assert.Null(deauthenticationLog);
        }

        [Fact]
        public void TestCase16()
        {
            _deauthenticationTestSetupRepository.CreateDataCase16();

            var args = new[] { "deauthentication", "-organization_code", _deauthenticationTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.NotNull(simAndDeviceAuthenticated);
            Assert.NotNull(multiFactorAuthenticated);
            Assert.Null(deauthenticationLog);
        }

        [Fact]
        public void TestCase17()
        {
            _deauthenticationTestSetupRepository.CreateDataCase17();

            var args = new[] { "deauthentication", "-organization_code", _deauthenticationTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);

            var (simAndDeviceAuthenticated, multiFactorAuthenticated, deauthenticationLog) = GetRecordsForAssert();

            Assert.NotNull(simAndDeviceAuthenticated);
            Assert.NotNull(multiFactorAuthenticated);
            Assert.Null(deauthenticationLog);
        }
    }
}
