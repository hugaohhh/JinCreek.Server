using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using System;

namespace JinCreek.Server.Batch.Repositories
{
    class BatchTestSetupRepository : BatchTestRepository
    {
        private static readonly DateTime AncientDateTime = DateTime.Now.AddDays(-3.00).Date;
        private static readonly DateTime PastDateTime = DateTime.Now.AddDays(-2.00).Date;
        private static readonly DateTime TodayDateTime = DateTime.Now.Date;
        private static readonly DateTime FutureDateTime = DateTime.Now.AddDays(2.00).Date;

        protected static readonly (DateTime, DateTime) CurrentDateTimeForLessThanStart = (FutureDateTime, FutureDateTime);
        protected static readonly (DateTime, DateTime) CurrentDateTimeForLaterEnd = (PastDateTime, PastDateTime);
        protected static readonly (DateTime, DateTime) CurrentDateTimeForStart = (TodayDateTime, FutureDateTime);
        protected static readonly (DateTime, DateTime) CurrentDateTimeForEnd = (PastDateTime, TodayDateTime);

        protected static readonly (DateTime, DateTime) AncientDateTimeForLaterEnd = (AncientDateTime, AncientDateTime);


        public BatchTestSetupRepository(MainDbContext mainDbContext, RadiusDbContext radiusDbContext) : base(mainDbContext, radiusDbContext)
        {
        }

        public int OrganizationCode = 4;
        public int OtherOrganizationCode = 9001;
        protected Organization Organization;
        protected OrganizationClientApp OrganizationClientApp;
        protected Domain Domain;

        protected GeneralUser GeneralUser1;

        protected Device Device1;

        protected Sim Sim1;
        protected SimGroup SimGroup1;

        public SimAndDevice SimAndDevice1;

        public MultiFactor MultiFactor1;

        protected virtual void CreateBaseData()
        {
            CreateOrganization();
            CreateClientAppRecords();
            CreateDomainRecords();

            CreateSimRecords();
        }

        private void CreateOrganization()
        {
            Organization = new Organization()
            {
                Code = OrganizationCode,
                Name = "TestOrganization",
                Address = "Chuo-ku, Tokyo",
                Phone = "03-1234-5678",
                Url = "http://www.example.com",
                AdminPhone = "045-123-4567",
                AdminMail = "admin@example.com",
                StartDate = CurrentDateTimeForStart.Item1,
                EndDate = CurrentDateTimeForStart.Item2,
                IsValid = true,
                DistributionServerIp = "127.0.0.1"
            };

            MainDbContext.AddRange(Organization);
        }

        private void CreateClientAppRecords()
        {
            var clientOs = new ClientOs()
            {
                Name = "Windows"
            };

            var clientApp = new ClientApp()
            {
                ClientOs = clientOs,
                Version = "1.0.0"
            };

            OrganizationClientApp = new OrganizationClientApp()
            {
                Organization = Organization,
                ClientApp = clientApp
            };

            MainDbContext.AddRange(clientOs, clientApp, OrganizationClientApp);
        }

        private void CreateDomainRecords()
        {
            Domain = new Domain()
            {
                Organization = Organization,
                Name = "jincreek.jp",
                AdObjectId = Guid.Parse("5a363ada-f328-436b-a597-6f2871f450c0")
            };

            MainDbContext.AddRange(Domain);
        }

        private void CreateSimRecords()
        {
            SimGroup1 = new SimGroup()
            {
                Organization = Organization,
                Name = "SimGroup1",
                Apn = "apn.example.com",
                NasIp = "192.168.0.1",
                IsolatedNw1IpPool = "ip_pool",
                IsolatedNw1IpRange = "192.168.1.0/24",
                AuthenticationServerIp = "172.16.0.1",
                PrimaryDns = "192.168.0.2",
                SecondaryDns = "192.168.0.3",
                UserNameSuffix = "jincreek"
            };

            Sim1 = new Sim()
            {
                Msisdn = "9999999",
                Imsi = "8888888",
                IccId = "4399333",
                UserName = "jincreek",
                Password = "password",
                SimGroup = SimGroup1
            };

            MainDbContext.AddRange(SimGroup1, Sim1);
        }
    }
}
