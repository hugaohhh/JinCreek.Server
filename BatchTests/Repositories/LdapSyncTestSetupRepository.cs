using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;

namespace JinCreek.Server.Batch.Repositories
{
    [SuppressMessage("ReSharper", "RedundantOverriddenMember")]
    [SuppressMessage("ReSharper", "RedundantExplicitArrayCreation")]
    class LdapSyncTestSetupRepository : BatchTestSetupRepository
    {
        public LdapSyncTestSetupRepository(MainDbContext mainDbContext, RadiusDbContext radiusDbContext) : base(mainDbContext, radiusDbContext)
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
        }

        public void CreateDataCase02()
        {
            CreateBaseData();
            MainDbContext.SaveChanges();
        }

        public void CreateDataCase03()
        {
            CreateBaseData();
            MainDbContext.SaveChanges();
        }

        public void CreateDataCase04()
        {
            CreateBaseData();
            Organization.StartDate = CurrentDateTimeForStart.Item2; // '実行日より未来
            MainDbContext.SaveChanges();
        }

        public void CreateDataCase05()
        {
            CreateBaseData();
            Organization.StartDate = CurrentDateTimeForStart.Item1; // '実行日と同日
            Organization.EndDate = CurrentDateTimeForStart.Item1; // '実行日と同日
            
            //Ad-Domain-1

            //JinCreek-Domainない
            MainDbContext.SaveChanges();
        }

        public void CreateDataCase06()
        {
            CreateBaseData();
            Domain.Name = "test"; // Ad側のDomainに対応しない
            Organization.StartDate = CurrentDateTimeForStart.Item1; // '実行日と同日
            Organization.EndDate = CurrentDateTimeForStart.Item2; // '実行日より未来
            
            // ADのデータなし
            //JinCreek-Domain-6
            SetDataDeviceGroup06();
            SetDataUserGroup06();
            MainDbContext.SaveChanges();
        }

        private void SetDataDeviceGroup06()
        {
            //DeviceGroup-6
            var deviceGroup = new DeviceGroup()
            {
                Domain = Domain,
                Name = "deviceGroup01",
                AdObjectId = Guid.Parse("435cd502-5a65-4649-8a39-1e53294304f8") //削除
            };
            Device1 = new Device() // Devive-7
            {
                Domain = Domain,
                Name = "device01",
                UseTpm = true,
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-03-01"),
                ManagedNumber = "",
                SerialNumber = "",
                ProductName = "",
                AdObjectId = Guid.NewGuid(),
                OrganizationClientApp = OrganizationClientApp,
                LteModule = new LteModule()
                {
                    Name = "lte01",
                    NwAdapterName = "",
                    UseSoftwareRadioState = true
                },
                DeviceGroupDevices = new HashSet<DeviceGroupDevice>() { new DeviceGroupDevice() { DeviceGroup = deviceGroup } },
                EndDate = null // EndDate : 不在
            };
            SimAndDevice1 = new SimAndDevice()
            {
                Device = Device1,
                Sim = Sim1,
                IsolatedNw2Ip = "",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-03-01"),
                EndDate = DateTime.Parse("2021-03-02") // '存在(実行日以外)
            };
            MainDbContext.SimAndDevice.Add(new SimAndDevice()
            {
                Device = Device1,
                Sim = new Sim()
                {
                    Msisdn = "11111111112",
                    Imsi = "1111111112",
                    IccId = "1111111112",
                    UserName = "jincreek222",
                    Password = "password222",
                    SimGroup = SimGroup1
                },
                IsolatedNw2Ip = "",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-03-01") // endDate:不在
            });
            MainDbContext.AddRange(deviceGroup, Device1, SimAndDevice1);
        }

        private void SetDataUserGroup06()
        {
            var userGroup = new UserGroup()
            {
                Name = "userGroup01",
                Domain = Domain,
                AdObjectId = Guid.Parse("6cb543a0-e409-4e7f-9b7f-8dc958ff62ac") //削除
            };
            //user7-9
            var user1 = new UserAdmin()
            {
                AccountName = "accountUser01",
                Name = "user01",
                Password = "132456",
                UserGroupEndUsers = new HashSet<UserGroupEndUser>() { new UserGroupEndUser() { UserGroup = userGroup } },
                Domain = Domain,
                AvailablePeriods = new HashSet<AvailablePeriod>(new List<AvailablePeriod>(new AvailablePeriod[]
                {
                    new AvailablePeriod() { StartDate = DateTime.Parse("2020-03-01"), EndDate = CurrentDateTimeForStart.Item1 }, // 最も新しい日付(実行日より同日)
                    new AvailablePeriod() { StartDate = DateTime.Parse("2020-03-01"), EndDate = DateTime.Parse("2020-01-09") },
                })),
                AdObjectId = Guid.NewGuid()
            };
            var user2 = new UserAdmin()
            {
                AccountName = "accountUser02",
                Name = "user02",
                Password = "132456",
                UserGroupEndUsers = new HashSet<UserGroupEndUser>() { new UserGroupEndUser() { UserGroup = userGroup } },
                Domain = Domain,
                AvailablePeriods = new HashSet<AvailablePeriod>(new List<AvailablePeriod>(new AvailablePeriod[]
                {
                    new AvailablePeriod() { StartDate = DateTime.Parse("2020-03-01"), EndDate = CurrentDateTimeForStart.Item2 }, // 最も新しい日付(実行日より未来)
                    new AvailablePeriod() { StartDate = DateTime.Parse("2020-03-01"), EndDate = DateTime.Parse("2020-01-09") },
                })),
                AdObjectId = Guid.NewGuid()
            };
            var user3 = new UserAdmin()
            {
                AccountName = "accountUser03",
                Name = "user03",
                Password = "132456",
                UserGroupEndUsers = new HashSet<UserGroupEndUser>() { new UserGroupEndUser() { UserGroup = userGroup } },
                Domain = Domain,
                AvailablePeriods = new HashSet<AvailablePeriod>(new List<AvailablePeriod>(new AvailablePeriod[]
                {
                    new AvailablePeriod() { StartDate = DateTime.Parse("2020-03-01"), EndDate = null }, // 不在
                    new AvailablePeriod() { StartDate = DateTime.Parse("2020-03-01"), EndDate = DateTime.Parse("2020-01-09") },
                })),
                AdObjectId = Guid.NewGuid()
            };
            MainDbContext.MultiFactor.Add(new MultiFactor()
            {
                EndUser = user1,
                ClosedNwIp = "",
                SimAndDevice = new SimAndDevice()
                {
                    Device = new Device()
                    {
                        Domain = Domain,
                        Name = "device001",
                        UseTpm = true,
                        WindowsSignInListCacheDays = 1,
                        StartDate = DateTime.Parse("2020-03-01"),
                        ManagedNumber = "",
                        SerialNumber = "",
                        ProductName = "",
                        AdObjectId = Guid.NewGuid(),
                        OrganizationClientApp = OrganizationClientApp,
                        EndDate = null // EndDate : 不在
                    },
                    Sim = new Sim()
                    {
                        Msisdn = "111111111122",
                        Imsi = "11111111122",
                        IccId = "11111111122",
                        UserName = "jincreek2",
                        Password = "password2",
                        SimGroup = SimGroup1
                    },
                    IsolatedNw2Ip = "",
                    AuthenticationDuration = 1,
                    StartDate = DateTime.Parse("2020-03-01"),
                    EndDate = CurrentDateTimeForEnd.Item1 // endDate:'実行日より過去
                },
                StartDate = DateTime.Parse("2020-03-01"),
                EndDate = DateTime.Parse("2020-01-09") //最も古い日付
            });
            MainDbContext.MultiFactor.Add(new MultiFactor()
            {
                EndUser = user1,
                ClosedNwIp = "",
                SimAndDevice = new SimAndDevice()
                {
                    Device = new Device()
                    {
                        Domain = Domain,
                        Name = "device002",
                        UseTpm = true,
                        WindowsSignInListCacheDays = 1,
                        StartDate = DateTime.Parse("2020-03-01"),
                        ManagedNumber = "",
                        SerialNumber = "",
                        ProductName = "",
                        AdObjectId = Guid.NewGuid(),
                        OrganizationClientApp = OrganizationClientApp,
                        EndDate = null // EndDate : 不在
                    },
                    Sim = new Sim()
                    {
                        Msisdn = "1111111112",
                        Imsi = "111111112",
                        IccId = "111111112",
                        UserName = "jincreek3",
                        Password = "password3",
                        SimGroup = SimGroup1
                    },
                    IsolatedNw2Ip = "",
                    AuthenticationDuration = 1,
                    StartDate = DateTime.Parse("2020-03-01"),
                    EndDate = CurrentDateTimeForStart.Item1 // endDate:''実行日と同日
                },
                StartDate = DateTime.Parse("2020-03-01"),
                EndDate = DateTime.Parse("2020-01-09") //最も古い日付
            });

            MainDbContext.MultiFactor.Add(new MultiFactor()
            {
                EndUser = user2,
                ClosedNwIp = "",
                SimAndDevice = new SimAndDevice()
                {
                    Device = new Device()
                    {
                        Domain = Domain,
                        Name = "device003",
                        UseTpm = true,
                        WindowsSignInListCacheDays = 1,
                        StartDate = DateTime.Parse("2020-03-01"),
                        ManagedNumber = "",
                        SerialNumber = "",
                        ProductName = "",
                        AdObjectId = Guid.NewGuid(),
                        OrganizationClientApp = OrganizationClientApp,
                        EndDate = null // EndDate : 不在
                    },
                    Sim = new Sim()
                    {
                        Msisdn = "1111111113",
                        Imsi = "111111113",
                        IccId = "111111113",
                        UserName = "jincreek4",
                        Password = "password4",
                        SimGroup = SimGroup1
                    },
                    IsolatedNw2Ip = "",
                    AuthenticationDuration = 1,
                    StartDate = DateTime.Parse("2020-03-01"),
                    EndDate = CurrentDateTimeForEnd.Item1 // endDate:'実行日より過去
                },
                StartDate = DateTime.Parse("2020-03-01"),
                EndDate = DateTime.Parse("2020-01-09") //最も古い日付
            });
            MainDbContext.MultiFactor.Add(new MultiFactor()
            {
                EndUser = user2,
                ClosedNwIp = "",
                SimAndDevice = new SimAndDevice()
                {
                    Device = new Device()
                    {
                        Domain = Domain,
                        Name = "device004",
                        UseTpm = true,
                        WindowsSignInListCacheDays = 1,
                        StartDate = DateTime.Parse("2020-03-01"),
                        ManagedNumber = "",
                        SerialNumber = "",
                        ProductName = "",
                        AdObjectId = Guid.NewGuid(),
                        OrganizationClientApp = OrganizationClientApp,
                        EndDate = null // EndDate : 不在
                    },
                    Sim = new Sim()
                    {
                        Msisdn = "1111111115",
                        Imsi = "111111115",
                        IccId = "111111115",
                        UserName = "jincreek5",
                        Password = "password5",
                        SimGroup = SimGroup1
                    },
                    IsolatedNw2Ip = "",
                    AuthenticationDuration = 1,
                    StartDate = DateTime.Parse("2020-03-01"),
                    EndDate = CurrentDateTimeForStart.Item2 // endDate:''実行日と未来
                },
                StartDate = DateTime.Parse("2020-03-01"),
                EndDate = DateTime.Parse("2020-01-09") //最も古い日付
            });

            MainDbContext.MultiFactor.Add(new MultiFactor()
            {
                EndUser = user3,
                ClosedNwIp = "",
                SimAndDevice = new SimAndDevice()
                {
                    Device = new Device()
                    {
                        Domain = Domain,
                        Name = "device005",
                        UseTpm = true,
                        WindowsSignInListCacheDays = 1,
                        StartDate = DateTime.Parse("2020-03-01"),
                        ManagedNumber = "",
                        SerialNumber = "",
                        ProductName = "",
                        AdObjectId = Guid.NewGuid(),
                        OrganizationClientApp = OrganizationClientApp,
                        EndDate = null // EndDate : 不在
                    },
                    Sim = new Sim()
                    {
                        Msisdn = "1111111116",
                        Imsi = "111111116",
                        IccId = "111111116",
                        UserName = "jincreek6",
                        Password = "password6",
                        SimGroup = SimGroup1
                    },
                    IsolatedNw2Ip = "",
                    AuthenticationDuration = 1,
                    StartDate = DateTime.Parse("2020-03-01"),
                    EndDate = CurrentDateTimeForEnd.Item1 // endDate:'実行日より過去
                },
                StartDate = DateTime.Parse("2020-03-01"),
                EndDate = DateTime.Parse("2020-01-09") //最も古い日付
            });
            MainDbContext.MultiFactor.Add(new MultiFactor()
            {
                EndUser = user3,
                ClosedNwIp = "",
                SimAndDevice = new SimAndDevice()
                {
                    Device = new Device()
                    {
                        Domain = Domain,
                        Name = "device006",
                        UseTpm = true,
                        WindowsSignInListCacheDays = 1,
                        StartDate = DateTime.Parse("2020-03-01"),
                        ManagedNumber = "",
                        SerialNumber = "",
                        ProductName = "",
                        AdObjectId = Guid.NewGuid(),
                        OrganizationClientApp = OrganizationClientApp,
                        EndDate = null // EndDate : 不在
                    },
                    Sim = new Sim()
                    {
                        Msisdn = "1111111117",
                        Imsi = "111111117",
                        IccId = "111111117",
                        UserName = "jincreek7",
                        Password = "password7",
                        SimGroup = SimGroup1
                    },
                    IsolatedNw2Ip = "",
                    AuthenticationDuration = 1,
                    StartDate = DateTime.Parse("2020-03-01"),
                    EndDate = null // endDate:null
                },
                StartDate = DateTime.Parse("2020-03-01"),
                EndDate = DateTime.Parse("2020-01-09") //最も古い日付
            });
            MainDbContext.AddRange(userGroup,user1,user2,user3);
        }

        public void CreateDataCase07()
        {
            CreateBaseData();
            Organization.Code = 5;
            Organization.StartDate = CurrentDateTimeForStart.Item1; // '実行日と同日
            Organization.EndDate = null; // 不在
            //Ad-Domain-2-3-5
            //JinCreek-Domain-3-5-6
            SetDataDeviceGroup06();
            SetDataUserGroup06();
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
            //Ad-Domain-4

            //JinCreek-Domain-4
            //DeviceGroup-3~5
            var deviceGroup1 = new DeviceGroup()
            {
                Domain = Domain,
                Name = "deviceGroup01Case09",
                AdObjectId = Guid.Parse("435cd502-5a65-4649-8a39-1e53294304f8"), // 削除
            };
            Device1 = new Device() // Devive-6
            {
                Domain = Domain,
                Name = "device01",
                UseTpm = true,
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-03-01"),
                ManagedNumber = "",
                SerialNumber = "",
                ProductName = "",
                AdObjectId = Guid.NewGuid(),
                OrganizationClientApp = OrganizationClientApp,
                LteModule = new LteModule()
                {
                    Name = "lte0001",
                    NwAdapterName = "",
                    UseSoftwareRadioState = true
                },
                DeviceGroupDevices = new HashSet<DeviceGroupDevice>() { new DeviceGroupDevice() { DeviceGroup = deviceGroup1 } },
                EndDate = null // EndDate : 不在
            };
            MainDbContext.DeviceGroup.Add(new DeviceGroup() //deviceGroup-5
            {
                Domain = Domain,
                Name = "deviceGroup03"
            });
            var deviceGroup2 = new DeviceGroup()//deviceGroup-4
            {
                Domain = Domain,
                Name = "deviceGroup02",
                AdObjectId = Guid.Parse("d31166e2-7918-4608-967c-b9fea0449232") //更新
            };
            MainDbContext.Device.Add(new Device() // Devive-2
            {
                Domain = Domain,
                Name = "device02",
                UseTpm = true,
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-03-01"),
                ManagedNumber = "",
                SerialNumber = "",
                ProductName = "",
                AdObjectId = Guid.Parse("2bfbea62-566c-4006-9151-2366704d0792"),
                OrganizationClientApp = OrganizationClientApp,
                LteModule = new LteModule()
                {
                    Name = "lte05",
                    NwAdapterName = "",
                    UseSoftwareRadioState = true
                },
                DeviceGroupDevices = new HashSet<DeviceGroupDevice>() { new DeviceGroupDevice() { DeviceGroup = deviceGroup2 } },
                EndDate = CurrentDateTimeForEnd.Item1, // EndDate : '実行日より過去
            });
            MainDbContext.Device.Add(new Device() // Devive-3
            {
                Domain = Domain,
                Name = "device03",
                UseTpm = true,
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-03-01"),
                ManagedNumber = "",
                SerialNumber = "",
                ProductName = "",
                AdObjectId = Guid.Parse("bddc4bd2-b49c-46c2-8af0-28e17e22e7a5"),
                OrganizationClientApp = OrganizationClientApp,
                LteModule = new LteModule()
                {
                    Name = "lte02",
                    NwAdapterName = "",
                    UseSoftwareRadioState = true
                },
                DeviceGroupDevices = new HashSet<DeviceGroupDevice>() { new DeviceGroupDevice() { DeviceGroup = deviceGroup2 } },
                EndDate = CurrentDateTimeForStart.Item1 // EndDate : '実行日と同日
            });
            MainDbContext.Device.Add(new Device() // Devive-4
            {
                Domain = Domain,
                Name = "device04",
                UseTpm = true,
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-03-01"),
                ManagedNumber = "",
                SerialNumber = "",
                ProductName = "",
                AdObjectId = Guid.Parse("998f3dd1-2596-4c2d-8793-9b8c5ed2167d"),
                OrganizationClientApp = OrganizationClientApp,
                LteModule = new LteModule()
                {
                    Name = "lte03",
                    NwAdapterName = "",
                    UseSoftwareRadioState = true
                },
                DeviceGroupDevices = new HashSet<DeviceGroupDevice>() { new DeviceGroupDevice() { DeviceGroup = deviceGroup2 } },
                EndDate = CurrentDateTimeForStart.Item2 // EndDate : '実行日より未来
            });
            MainDbContext.Device.Add(new Device() // Devive-5
            {
                Domain = Domain,
                Name = "device05",
                UseTpm = true,
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-03-01"),
                ManagedNumber = "",
                SerialNumber = "",
                ProductName = "",
                AdObjectId = Guid.Parse("ceeb02cc-fd3d-4165-80f9-989ba64e4ffb"),
                OrganizationClientApp = OrganizationClientApp,
                LteModule = new LteModule()
                {
                    Name = "lte04",
                    NwAdapterName = "",
                    UseSoftwareRadioState = true
                },
                DeviceGroupDevices = new HashSet<DeviceGroupDevice>() { new DeviceGroupDevice() { DeviceGroup = deviceGroup2 } },
                EndDate = null // EndDate : 不在
            });
            //userGroup3~5
            var userGroup1 = new UserGroup()
            {
                AdObjectId = Guid.Parse("6cb543a0-e409-4e7f-9b7f-8dc958ff62ac"), //削除
                Domain = Domain,
                Name = "userGroup01",
            };
            MainDbContext.User.Add(new UserAdmin()
            {
                AccountName = "accountUser01",
                Name = "user01",
                Password = "132456",
                UserGroupEndUsers = new HashSet<UserGroupEndUser>() { new UserGroupEndUser() { UserGroup = userGroup1 } },
                Domain = Domain,
                AvailablePeriods = new HashSet<AvailablePeriod>(new List<AvailablePeriod>(new AvailablePeriod[]
                {
                    new AvailablePeriod() { StartDate = DateTime.Parse("2020-03-01"), EndDate = CurrentDateTimeForEnd.Item1 }, // 最も新しい日付(実行日より過去)
                    new AvailablePeriod() { StartDate = DateTime.Parse("2020-03-01"), EndDate = DateTime.Parse("2020-01-09") },
                }))
            });
            var userGroup2 = new UserGroup()
            {
                AdObjectId = Guid.Parse("bcd40870-d1ff-4f45-b9cb-e56b48d60028"), //更新　
                Domain = Domain,
                Name = "userGroup02",
            };
            MainDbContext.UserGroup.Add(new UserGroup()
            {
                AdObjectId = Guid.NewGuid(),
                Domain = Domain,
                Name = "userGroup03",
            });
            MainDbContext.User.Add(new UserAdmin()
            {
                AccountName = "accountUser02",
                Name = "user02",
                Password = "132456",
                UserGroupEndUsers = new HashSet<UserGroupEndUser>() { new UserGroupEndUser() { UserGroup = userGroup2 } },
                Domain = Domain,
                AvailablePeriods = new HashSet<AvailablePeriod>(new List<AvailablePeriod>(new AvailablePeriod[]
                {
                    new AvailablePeriod() { StartDate = DateTime.Parse("2020-03-01"), EndDate = CurrentDateTimeForEnd.Item1 }, // 最も新しい日付(実行日より過去)
                    new AvailablePeriod() { StartDate = DateTime.Parse("2020-03-01"), EndDate = DateTime.Parse("2020-01-09") },
                }))
            });
            MainDbContext.User.Add(new UserAdmin()
            {
                AccountName = "accountUser03",
                Name = "user03",
                Password = "132456",
                UserGroupEndUsers = new HashSet<UserGroupEndUser>() { new UserGroupEndUser() { UserGroup = userGroup2 } },
                Domain = Domain,
                AvailablePeriods = new HashSet<AvailablePeriod>(new List<AvailablePeriod>(new AvailablePeriod[]
                {
                    new AvailablePeriod() { StartDate = DateTime.Parse("2020-03-01"), EndDate = CurrentDateTimeForStart.Item1 }, // 最も新しい日付(実行日より同日)
                    new AvailablePeriod() { StartDate = DateTime.Parse("2020-03-01"), EndDate = DateTime.Parse("2020-01-09") },
                }))
            });
            MainDbContext.User.Add(new UserAdmin()
            {
                AccountName = "accountUser04",
                Name = "user04",
                Password = "132456",
                UserGroupEndUsers = new HashSet<UserGroupEndUser>() { new UserGroupEndUser() { UserGroup = userGroup2 } },
                Domain = Domain,
                AvailablePeriods = new HashSet<AvailablePeriod>(new List<AvailablePeriod>(new AvailablePeriod[]
                {
                    new AvailablePeriod() { StartDate = DateTime.Parse("2020-03-01"), EndDate = CurrentDateTimeForStart.Item2 }, // 最も新しい日付(実行日より未来)
                    new AvailablePeriod() { StartDate = DateTime.Parse("2020-03-01"), EndDate = DateTime.Parse("2020-01-09") },
                }))
            });
            MainDbContext.User.Add(new UserAdmin()
            {
                AccountName = "accountUser05",
                Name = "user05",
                Password = "132456",
                UserGroupEndUsers = new HashSet<UserGroupEndUser>() { new UserGroupEndUser() { UserGroup = userGroup2 } },
                Domain = Domain,
                AvailablePeriods = new HashSet<AvailablePeriod>(new List<AvailablePeriod>(new AvailablePeriod[]
                {
                    new AvailablePeriod() { StartDate = DateTime.Parse("2020-03-01"), EndDate = null }, // 不在
                    new AvailablePeriod() { StartDate = DateTime.Parse("2020-03-01"), EndDate = DateTime.Parse("2020-01-09") },
                }))
            });
            MainDbContext.AddRange(deviceGroup1,deviceGroup2,Device1,userGroup1,userGroup2);
            MainDbContext.SaveChanges();
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
