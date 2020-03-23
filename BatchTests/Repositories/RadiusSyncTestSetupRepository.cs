using System.Diagnostics.CodeAnalysis;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;

namespace JinCreek.Server.Batch.Repositories
{
    [SuppressMessage("ReSharper", "RedundantOverriddenMember")]
    class RadiusSyncTestSetupRepository : BatchTestSetupRepository
    {
        public RadiusSyncTestSetupRepository(MainDbContext mainDbContext, RadiusDbContext radiusDbContext) : base(mainDbContext, radiusDbContext)
        {
        }

        protected override void CreateBaseData()
        {
            base.CreateBaseData();
        }

        public void CreateDataCase01()
        {
            CreateBaseData();
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase02()
        {
            CreateBaseData();
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase03()
        {
            CreateBaseData();
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase04()
        {
            CreateBaseData();
            Organization.StartDate = CurrentDateTimeForStart.Item2; // '実行日より未来
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }
        // SimいないのRadiusデータ
        private void SetDataForNotSim()
        {
            RadiusDbContext.Radcheck.Add(new Radcheck()// simいないの　Radcheck
            {
                Username = "sim@simgroup",
                Attribute = "Cleartext-Password",
                Op = ":="
            });
            RadiusDbContext.Radcheck.Add(new Radcheck()
            {
                Username = "sim@simgroup",
                Attribute = "Calling-Station-Id",
                Op = "=="
            });
            RadiusDbContext.Radusergroup.Add(new Radusergroup()// simいないの　Radusergroup
            {
                Username = "sim@simgroup",
                Groupname = "1001"
            });
        }
        // SimgroupいないのRadiusデータ
        private void SetDataForNotSimGroup()
        {
            RadiusDbContext.Radgroupcheck.Add(new Radgroupcheck() // simGroupいないのRadgroupcheck
            {
                Attribute = "Pool-Name",
                Groupname = "1001",
                Op = ":=",
                Value = ""
            });
            RadiusDbContext.Radgroupcheck.Add(new Radgroupcheck()
            {
                Attribute = "Called-Station-Id",
                Groupname = "1001",
                Op = "==",
                Value = ""
            });
            RadiusDbContext.Radgroupcheck.Add(new Radgroupcheck()
            {
                Attribute = "NAS-IP-Address",
                Groupname = "1001",
                Op = "==",
                Value = ""
            });
            RadiusDbContext.Radgroupreply.Add(new Radgroupreply()// simGroupいないの　Radgroupreply
            {
                Attribute = "MS-Primary-DNS-Server",
                Groupname = "1001",
                Op = ":=",
                Value = ""
            });
            RadiusDbContext.Radgroupreply.Add(new Radgroupreply()
            {
                Attribute = "MS-Secondary-DNS-Server",
                Groupname = "1001",
                Op = ":=",
                Value = ""
            });
            RadiusDbContext.Radippool.Add(new Radippool()　// simGroupいないの　Radippool
            {
                Framedipaddress = "127.0.0.1",
                Nasipaddress = "",
                Calledstationid = "",
                Callingstationid = "",
                Username = "",
                Poolkey = "",
                PoolName = "poolName"
            });
        }

        public void CreateDataCase05()
        {
            CreateBaseData();
            Organization.StartDate = CurrentDateTimeForStart.Item1; // '実行日と同日
            Organization.EndDate = CurrentDateTimeForStart.Item1; // '実行日と同日
            MainDbContext.SaveChanges();
            MainDbContext.RemoveRange(MainDbContext.Sim);
            MainDbContext.RemoveRange(MainDbContext.SimGroup); // sim と　SimGroupない
            MainDbContext.SaveChanges();
            SetDataForNotSim();
            SetDataForNotSimGroup();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase06()
        {
            CreateBaseData();
            Organization.StartDate = CurrentDateTimeForStart.Item1; // '実行日と同日
            Organization.EndDate = CurrentDateTimeForStart.Item2; // '実行日より未来
            SimGroup1.IsolatedNw1IpRange = "192.168.1.128/32"; // '有効ホストIP数=1個 サブネット=32bit
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges(); // radiusのデータなし
        }

        public void CreateDataCase07()
        {
            CreateBaseData();
            Organization.StartDate = CurrentDateTimeForStart.Item1; // '実行日と同日
            Organization.EndDate = null; // 不在
            SimGroup1.IsolatedNw1IpRange = "192.168.1.128/32"; // '有効ホストIP数=1個 サブネット=32bit
            var simGroup2 = new SimGroup()
            {
                Organization = Organization,
                Name = "SimGroup2",
                Apn = "apn.example.com",
                NasIp = "192.168.0.2",
                IsolatedNw1IpPool = "ip_pool_2",
                IsolatedNw1IpRange = "192.168.1.255/30", // '有効ホストIP数=2個 サブネット=30bit）※NIP最大(要補正)アドレス
                AuthenticationServerIp = "172.16.0.1",
                PrimaryDns = "192.168.0.2",
                SecondaryDns = "192.168.0.3",
                UserNameSuffix = "jincreek2"
            };
            var simGroup3 = new SimGroup()
            {
                Organization = Organization,
                Name = "SimGroup3",
                Apn = "apn.example.com",
                NasIp = "192.168.0.2",
                IsolatedNw1IpPool = "ip_pool_3",
                IsolatedNw1IpRange = "192.168.1.64/31", // '有効ホストIP数=0個（サブネット = 31bit）
                AuthenticationServerIp = "172.16.0.1",
                PrimaryDns = "192.168.0.2",
                SecondaryDns = "192.168.0.3",
                UserNameSuffix = "jincreek3"
            };
            MainDbContext.Sim.Add(new Sim() // sim_1
            {
                Msisdn = "111111112",
                Imsi = "11111111112",
                IccId = "1111111112",
                UserName = "jincreek2",
                Password = "password2",
                SimGroup = SimGroup1
            });
            var sim2 = new Sim() // sim_2
            {
                Msisdn = "111111113",
                Imsi = "11111111113",
                IccId = "1111111113",
                UserName = "jincreek3",
                Password = "password3",
                SimGroup = simGroup2
            };
            RadiusDbContext.Radcheck.Add(new Radcheck()　// SimいるのRadcheckとRadusergroup
            {
                Username = sim2.UserName + "@" + sim2.SimGroup.UserNameSuffix,
                Attribute = "Cleartext-Password",
                Op = ":=",
                Value = sim2.Password
            });
            RadiusDbContext.Radcheck.Add(new Radcheck()
            {
                Username = sim2.UserName + "@" + sim2.SimGroup.UserNameSuffix,
                Attribute = "Calling-Station-Id",
                Op = "==",
                Value = Sim1.Msisdn
            });
            RadiusDbContext.Radusergroup.Add(new Radusergroup()
            {
                Username = sim2.UserName + "@" + sim2.SimGroup.UserNameSuffix,
                Groupname = sim2.SimGroup.Id.ToString(),
            });
            RadiusDbContext.Radippool.Add( new Radippool()
            {
                Framedipaddress = "192.168.1.100", //'simGroup3のCIDR別有効ホストIP範囲に含まれない
                Nasipaddress = "",
                Calledstationid = "",
                Callingstationid = "",
                Username = "",
                Poolkey = "",
                PoolName = simGroup3.IsolatedNw1IpPool
            });
            SetDataForNotSimGroup();
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase08()
        {
            CreateBaseData();
            Organization.StartDate = CurrentDateTimeForEnd.Item1; // '実行日より過去
            Organization.EndDate = CurrentDateTimeForEnd.Item1; // '実行日より過去
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase09()
        {
            CreateBaseData();
            Organization.StartDate = CurrentDateTimeForEnd.Item1; // '実行日より過去
            Organization.EndDate = CurrentDateTimeForStart.Item1; // '実行日と同日
            SimGroup1.IsolatedNw1IpRange = "192.168.1.0/30"; // '有効ホストIP数=2個（サブネット = 30bit）※NIP最小アドレス
            var radgroupcheck1 = new Radgroupcheck()// SimGroupいるの Radgroupcheck Radgroupreply
            {
                Attribute = "Pool-Name",
                Groupname = SimGroup1.Id.ToString(),
                Op = ":=",
                Value = SimGroup1.IsolatedNw1IpPool
            };
            var radgroupcheck2 = new Radgroupcheck()
            {
                Attribute = "Called-Station-Id",
                Groupname = SimGroup1.Id.ToString(),
                Op = "==",
                Value = SimGroup1.Apn
            };
            var radgroupcheck3 = new Radgroupcheck()
            {
                Attribute = "NAS-IP-Address",
                Groupname = SimGroup1.Id.ToString(),
                Op = "==",
                Value = SimGroup1.NasIp
            };
            var radgroupreply1 = new Radgroupreply()
            {
                Attribute = "MS-Primary-DNS-Server",
                Groupname = SimGroup1.Id.ToString(),
                Op = ":=",
                Value = SimGroup1.PrimaryDns
            };
            var radgroupreply2 = new Radgroupreply()
            {
                Attribute = "MS-Secondary-DNS-Server",
                Groupname = SimGroup1.Id.ToString(),
                Op = ":=",
                Value = SimGroup1.SecondaryDns
            };
            var radcheck1 = new Radcheck()　// SimいるのRadcheckとRadusergroup
            {
                Username = Sim1.UserName+"@"+Sim1.SimGroup.UserNameSuffix,
                Attribute = "Cleartext-Password",
                Op = ":=",
                Value = Sim1.Password
            };
            var radcheck2 = new Radcheck()
            {
                Username = Sim1.UserName + "@" + Sim1.SimGroup.UserNameSuffix,
                Attribute = "Calling-Station-Id",
                Op = "==",
                Value = Sim1.Msisdn
            };
            var radusergroup = new Radusergroup()
            {
                Username = Sim1.UserName + "@" + Sim1.SimGroup.UserNameSuffix,
                Groupname = Sim1.SimGroup.Id.ToString()
            };
            var radippool = new Radippool()
            {
                Framedipaddress = "192.168.1.1", //'Simgroup1のCIDR別有効ホストIP範囲に含まれる
                Nasipaddress = "",
                Calledstationid = "",
                Callingstationid = "",
                Username = "",
                Poolkey = "",
                PoolName = SimGroup1.IsolatedNw1IpPool
            };
            var radippool2 = new Radippool()
            {
                Framedipaddress = "192.168.1.100", //'Simgroup1のCIDR別有効ホストIP範囲に含まれない
                Nasipaddress = "",
                Calledstationid = "",
                Callingstationid = "",
                Username = "",
                Poolkey = "",
                PoolName = SimGroup1.IsolatedNw1IpPool
            };
            SetDataForNotSim();
            RadiusDbContext.AddRange(radgroupcheck1, radgroupcheck2, radgroupcheck3, radgroupreply1, radgroupreply2,
                radcheck1, radcheck2, radusergroup, radippool, radippool2);

            MainDbContext.Sim.Add(new Sim()
            {
                Msisdn = "111111111",
                Imsi = "11111111111",   
                IccId = "1111111111",
                UserName = "jincreek2",
                Password = "password2",
                SimGroup = SimGroup1
            });
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase10()
        {
            CreateDataCase09();
            Organization.EndDate = CurrentDateTimeForStart.Item2; // '実行日より未来 
            MainDbContext.SaveChanges();
        }

        public void CreateDataCase11()
        {
            CreateDataCase09();
            Organization.EndDate = null; // 不在 
            MainDbContext.SaveChanges();
        }
    }
}
