using ConsoleAppFramework;
using JinCreek.Server.Batch.Repositories;
using JinCreek.Server.Common.Models;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace JinCreek.Server.Batch
{
    [Collection("Sequential")]
    public class RadiusSyncTest : BatchTestBase
    {
        private readonly BatchTestRepository _batchTestRepository;
        private readonly RadiusSyncTestSetupRepository _radiusSyncTestSetupRepository;

        public RadiusSyncTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _batchTestRepository = new BatchTestRepository(MainDbContext, RadiusDbContext);
            _radiusSyncTestSetupRepository = new RadiusSyncTestSetupRepository(MainDbContext, RadiusDbContext);
        }

        private (List<SimGroup>, List<Sim>) GetRecordsForAssert()
        {
            int targetOrganizationCode = _radiusSyncTestSetupRepository.OrganizationCode;
            var simGroups = _batchTestRepository.GetSimGroups(targetOrganizationCode);
            var sims = _batchTestRepository.GetSims(targetOrganizationCode);

            return (simGroups, sims);
        }

        private void Assert01_02_03(List<Sim> sims, List<SimGroup> simGroups)
        {
            foreach (var simGroup in simGroups)
            {
                var radgroupcheckList = _batchTestRepository.GetRadgroupcheckList(simGroup.Id);
                Assert.Empty(radgroupcheckList);
                var radgroupreplyList = _batchTestRepository.GetRadgroupreplyList(simGroup.Id);
                Assert.Empty(radgroupreplyList);
                var radippools = _batchTestRepository.GetRadippool(simGroup.IsolatedNw1IpPool);
                Assert.Empty(radippools);
            }
            foreach (var sim in sims)
            {
                var radcheckList = _batchTestRepository.GetRadcheckList(sim.UserName + "@" + sim.SimGroup.UserNameSuffix);
                Assert.Empty(radcheckList);
                var radusergroup = _batchTestRepository.GetRadusergroup(sim.UserName + "@" + sim.SimGroup.UserNameSuffix);
                Assert.Null(radusergroup);
            }
        }

        private void AssertSim(List<Sim> sims)
        {
            foreach (var sim in sims)
            {
                var radcheckList = _batchTestRepository.GetRadcheckList(sim.UserName + "@" + sim.SimGroup.UserNameSuffix);
                Assert.NotEmpty(radcheckList);
                foreach (var radcheck in radcheckList)
                {
                    if (radcheck.Attribute.Equals("Cleartext-Password") && radcheck.Op.Equals(":="))
                    {
                        Assert.Equal(radcheck.Value, sim.Password);//更新したチェック
                    }
                    if (radcheck.Attribute.Equals("Calling-Station-Id") && radcheck.Op.Equals("=="))
                    {
                        Assert.Equal(radcheck.Value, sim.Msisdn);//更新したチェック
                    }
                }
                var radusergroup = _batchTestRepository.GetRadusergroup(sim.UserName + "@" + sim.SimGroup.UserNameSuffix);
                Assert.NotNull(radusergroup);
                Assert.Equal(radusergroup.Groupname, sim.SimGroup.Id.ToString());//更新したチェック
            }
        }

        //simいないのRadiusの更新しないの確認
        private void AssertSimNotUpdate()
        {
            var radusergroup1 = _batchTestRepository.GetRadusergroup("sim@simgroup");
            Assert.NotNull(radusergroup1);
            Assert.Equal("1001", radusergroup1.Groupname);// simないの場合更新してないの確認
            var radchecks = _batchTestRepository.GetRadcheckList("sim@simgroup");
            foreach (var radgroupcheck in radchecks)
            {
                if (radgroupcheck.Attribute.Equals("Pool-Name") && radgroupcheck.Op.Equals(":="))
                {
                    Assert.Equal("", radgroupcheck.Value); // simないの場合更新してないの確認
                }
                if (radgroupcheck.Attribute.Equals("Called-Station-Id") && radgroupcheck.Op.Equals("=="))
                {
                    Assert.Equal("", radgroupcheck.Value); // simないの場合更新してないの確認
                }
                if (radgroupcheck.Attribute.Equals("NAS-IP-Address") && radgroupcheck.Op.Equals("=="))
                {
                    Assert.Equal("", radgroupcheck.Value); // simないの場合更新してないの確認
                }
            }
        }

        //simgroupいないの更新しないの確認
        private void AssertSimgroupNotUpdate()
        {
            var radgroupcheckList = _batchTestRepository.GetRadgroupcheckListByValue("");
            Assert.NotEmpty(radgroupcheckList);
            foreach (var radgroupcheck in radgroupcheckList)
            {
                if (radgroupcheck.Attribute.Equals("Pool-Name") && radgroupcheck.Op.Equals(":="))
                {
                    Assert.Equal("", radgroupcheck.Value); // 更新してないの確認
                }
                if (radgroupcheck.Attribute.Equals("Called-Station-Id") && radgroupcheck.Op.Equals("=="))
                {
                    Assert.Equal("", radgroupcheck.Value); // 更新してないの確認
                }
                if (radgroupcheck.Attribute.Equals("NAS-IP-Address") && radgroupcheck.Op.Equals("=="))
                {
                    Assert.Equal("", radgroupcheck.Value); // 更新してないの確認
                }
            }
            var radgroupreplyList = _batchTestRepository.GetRadgroupreplyListByValue("");
            Assert.NotEmpty(radgroupreplyList);
            foreach (var radgroupreply in radgroupreplyList)
            {
                if (radgroupreply.Attribute.Equals("MS-Primary-DNS-Server") && radgroupreply.Op.Equals(":="))
                {
                    Assert.Equal("", radgroupreply.Value); // 更新してないの確認
                }
                if (radgroupreply.Attribute.Equals("MS-Secondary-DNS-Server") && radgroupreply.Op.Equals(":="))
                {
                    Assert.Equal("", radgroupreply.Value); // 更新してないの確認
                }
            }
            var radippools = _batchTestRepository.GetRadippool("poolName");
            Assert.NotEmpty(radippools);
            Assert.Equal("127.0.0.1", radippools[0].Framedipaddress); //simGroupいないのIpAddress
        }

        //Radgroupcheck 同期チェック
        private void AssertRadgroupcheck(SimGroup simGroup)
        {
            var radgroupcheckList = _batchTestRepository.GetRadgroupcheckList(simGroup.Id);
            Assert.NotEmpty(radgroupcheckList);
            foreach (var radgroupcheck in radgroupcheckList)
            {
                if (radgroupcheck.Attribute.Equals("Pool-Name") && radgroupcheck.Op.Equals(":="))
                {
                    Assert.Equal(radgroupcheck.Value, simGroup.IsolatedNw1IpPool);//更新したチェック
                }
                if (radgroupcheck.Attribute.Equals("Called-Station-Id") && radgroupcheck.Op.Equals("=="))
                {
                    Assert.Equal(radgroupcheck.Value, simGroup.Apn);//更新したチェック
                }
                if (radgroupcheck.Attribute.Equals("NAS-IP-Address") && radgroupcheck.Op.Equals("=="))
                {
                    Assert.Equal(radgroupcheck.Value, simGroup.NasIp);//更新したチェック
                }
            }
        }

        // Radgroupreply 同期チェック
        private void AssertRadgroupreply(SimGroup simGroup)
        {
            var radgroupreplyList = _batchTestRepository.GetRadgroupreplyList(simGroup.Id);
            Assert.NotEmpty(radgroupreplyList);
            foreach (var radgroupreply in radgroupreplyList)
            {
                if (radgroupreply.Attribute.Equals("MS-Primary-DNS-Server") && radgroupreply.Op.Equals(":="))
                {
                    Assert.Equal(radgroupreply.Value, simGroup.PrimaryDns);//更新したチェック
                }
                if (radgroupreply.Attribute.Equals("MS-Secondary-DNS-Server") && radgroupreply.Op.Equals(":="))
                {
                    Assert.Equal(radgroupreply.Value, simGroup.SecondaryDns);//更新したチェック
                }
            }
        }

        private void Assert09_10_11(List<SimGroup> simGroups, List<Sim> sims)
        {
            foreach (var simGroup in simGroups)
            {
                AssertRadgroupcheck(simGroup);
                AssertRadgroupreply(simGroup);
                var radippools = _batchTestRepository.GetRadippool(simGroup.IsolatedNw1IpPool);
                Assert.Equal(2, radippools.Count);　// '有効ホストIP数=2個
                foreach (var radippool in radippools)
                {
                    Assert.NotEqual("192.168.1.100",radippool.Framedipaddress); // CIDR別有効ホストIP範囲に含まれないの削除確認
                }
            }
            AssertSim(sims); //simに対応の同期確認
            AssertSimNotUpdate();　//simいないのRadiusの更新しないの確認
        }

        [Fact]
        public void TestCase01()
        {
            _radiusSyncTestSetupRepository.CreateDataCase01();
            var args = new[] { "radius_sync" };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args); // COde:'不在
            Assert.True(rtn.IsCompletedSuccessfully);
            var (simGroups, sims) = GetRecordsForAssert();
            Assert01_02_03(sims, simGroups);
        }

        [Fact]
        public void TestCase02()
        {
            _radiusSyncTestSetupRepository.CreateDataCase02();
            var args = new[] { "radius_sync", "-organization_code", "aaaaaaaaa" };// COde:''数値以外
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);
            Assert.True(rtn.IsCompletedSuccessfully);
            var (simGroups, sims) = GetRecordsForAssert();
            Assert01_02_03(sims, simGroups);
        }

        [Fact]
        public void TestCase03()
        { 
            _radiusSyncTestSetupRepository.CreateDataCase03();
            var args = new[] { "radius_sync", "-organization_code", "1" };// COde:'数値(該当Organization不在)
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);
            Assert.True(rtn.IsCompletedSuccessfully);
            var (simGroups, sims) = GetRecordsForAssert();
            Assert01_02_03(sims, simGroups);
        }

        [Fact]
        public void TestCase04()
        {
            _radiusSyncTestSetupRepository.CreateDataCase04(); // StartDate:'実行日より未来
            var args = new[] { "radius_sync", "-organization_code", _radiusSyncTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);
            Assert.True(rtn.IsCompletedSuccessfully);
            var (simGroups, sims) = GetRecordsForAssert();
            Assert01_02_03(sims, simGroups);
        }

        [Fact]
        public void TestCase05()
        {
            _radiusSyncTestSetupRepository.CreateDataCase05(); // simGroup simがない　Radius正常
            var args = new[] { "radius_sync", "-organization_code", _radiusSyncTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);
            var (simGroups, sims) = GetRecordsForAssert();
            Assert.Empty(simGroups);
            Assert.Empty(sims);
            AssertSimgroupNotUpdate();
            AssertSimNotUpdate();
        }

        [Fact]
        public void TestCase06()
        {
            _radiusSyncTestSetupRepository.CreateDataCase06(); // simGroup_1 （サブネット=32bit）
            var args = new[] { "radius_sync", "-organization_code", _radiusSyncTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);
            var (simGroups, sims) = GetRecordsForAssert();
            foreach (var simGroup in simGroups)
            {
                AssertRadgroupcheck(simGroup);
                AssertRadgroupreply(simGroup);

                var radippools = _batchTestRepository.GetRadippool(simGroup.IsolatedNw1IpPool);
                Assert.Equal(1, radippools.Count);// '有効ホストIP数=1個
            }
            AssertSim(sims);
        }

        [Fact]
        public void TestCase07()
        {
            _radiusSyncTestSetupRepository.CreateDataCase07();
            var args = new[] { "radius_sync", "-organization_code", _radiusSyncTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);
            Assert.True(rtn.IsCompletedSuccessfully);
            var (simGroups, sims) = GetRecordsForAssert();
            foreach (var simGroup in simGroups)
            {
                if (simGroup.Name.Equals("SimGroup1")) //（サブネット=32bit）
                {
                    AssertRadgroupcheck(simGroup);
                    AssertRadgroupreply(simGroup);
                    var radippools = _batchTestRepository.GetRadippool(simGroup.IsolatedNw1IpPool);
                    Assert.Equal(1, radippools.Count);// '有効ホストIP数=1個
                }
                else if (simGroup.Name.Equals("SimGroup2")) //（サブネット=30bit）
                {
                    var radippools = _batchTestRepository.GetRadippool(simGroup.IsolatedNw1IpPool);
                    Assert.Equal(2, radippools.Count); // '有効ホストIP数=2個
                }
                else if(simGroup.Name.Equals("SimGroup3")) //（サブネット=31bit）
                {
                    var radippools = _batchTestRepository.GetRadippool(simGroup.IsolatedNw1IpPool);
                    Assert.Empty(radippools);
                }
            }
            AssertSimgroupNotUpdate(); // simgroupいないの更新しないの確認
            AssertSim(sims); // simに対応の同期の確認
        }

        [Fact]
        public void TestCase08()
        {
            _radiusSyncTestSetupRepository.CreateDataCase08();
            var args = new[] { "radiusSync", "-organization_code", _radiusSyncTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);
            Assert.True(rtn.IsCompletedSuccessfully);
            var (simGroups, sims) = GetRecordsForAssert();
            Assert01_02_03(sims, simGroups);
        }

        [Fact]
        public void TestCase09()
        {
            _radiusSyncTestSetupRepository.CreateDataCase09();// EndDate'実行日と同日
            var args = new[] { "radius_sync", "-organization_code", _radiusSyncTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);
            Assert.True(rtn.IsCompletedSuccessfully);
            var (simGroups, sims) = GetRecordsForAssert();
            Assert09_10_11(simGroups, sims);
        }

        [Fact]
        public void TestCase10()
        {
            _radiusSyncTestSetupRepository.CreateDataCase10();// EndDate''実行日より未来
            var args = new[] { "radius_sync", "-organization_code", _radiusSyncTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);
            Assert.True(rtn.IsCompletedSuccessfully);
            var (simGroups, sims) = GetRecordsForAssert();
            Assert09_10_11(simGroups, sims);
        }

        [Fact]
        public void TestCase11()
        {
            _radiusSyncTestSetupRepository.CreateDataCase11();// EndDate'不在
            var args = new[] { "radius_sync", "-organization_code", _radiusSyncTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);
            Assert.True(rtn.IsCompletedSuccessfully);
            var (simGroups, sims) = GetRecordsForAssert();
            Assert09_10_11(simGroups, sims);
        }

    }
}
