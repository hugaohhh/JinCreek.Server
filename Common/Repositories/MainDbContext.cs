using JinCreek.Server.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace JinCreek.Server.Common.Repositories
{
    public class MainDbContext : DbContext
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<Sim> Sim { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<SimGroup> SimGroup { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<Device> Device { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<AdDevice> AdDevice { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<AdDeviceSettingOfflineWindowsSignIn> AdDeviceSettingOfflineWindowsSignIn { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<DeviceGroup> DeviceGroup { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<Lte> Lte { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<SimDevice> SimDevice { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<FactorCombination> FactorCombination { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<AuthenticationState> AuthenticationState { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<AuthenticationLog> AuthenticationLog { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<MultiFactorAuthenticationLogSuccess> MultiFactorAuthenticationLogSuccess { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<DeauthenticationAuthenticationLogSuccess> Deauthentication { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<SimDeviceAuthenticationLogSuccess> SimDeviceAuthenticationLogSuccess { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<MultiFactorAuthenticationStateDone> MultiFactorAuthenticationStateDone { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<SimDeviceAuthenticationStateDone> SimDeviceAuthenticationStateDone { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<SimDeviceAuthenticationLogFail> SimDeviceAuthenticationLogFail { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<MultiFactorAuthenticationLogFail> MultiFactorAuthenticationLogFail { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<Organization> Organization { get; set; }

        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<UserGroup> UserGroup { get; set; }
        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<Domain> Domain { get; set; }
        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<User> User { get; set; }
        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<AdminUser> AdminUser { get; set; }

        //DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<GeneralUser> GeneralUser { get; set; }

        //DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<EndUser> EndUser { get; set; }

        //DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<SuperAdminUser> SuperAdminUser { get; set; }

        public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new AuthenticationRepository.EfLoggerProvider());
            optionsBuilder.UseLoggerFactory(loggerFactory);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SimGroup>()
                .HasAlternateKey(sg => new
                {
                    sg.OrganizationCode,
                    sg.SimGroupName
                }).HasName("SimGroup_Code_SimGroupName_UQ");

            modelBuilder.Entity<Sim>()
                .HasAlternateKey(s => s.Msisdn)
                .HasName("Sim_Msisdn_UQ");


            modelBuilder.Entity<DeviceGroup>()
                .HasAlternateKey(dg => new
                {
                    dg.OrganizationCode,
                    dg.Os,
                    dg.Version
                }).HasName("DeviceGroup_Code_Os_Version_UQ");

            modelBuilder.Entity<Device>()
                .HasAlternateKey(d => d.DeviceName)
                .HasName("Device_DeviceName_UQ");

            modelBuilder.Entity<Lte>()
                .HasAlternateKey(l => l.LteName)
                .HasName("Lte_LteName_UQ");

            modelBuilder.Entity<Domain>()
                .HasAlternateKey(d => d.DomainName)
                .HasName("Domain_DomainName_UQ");


            modelBuilder.Entity<User>()
                .HasAlternateKey(u => u.AccountName)
                .HasName("User_AccountName_UQ");

            modelBuilder.Entity<UserGroup>()
                .HasAlternateKey(ug => new
                {
                    ug.DomainId,
                    ug.UserGroupName
                })
                .HasName("UserGroup_UserGroupName_UQ");


            modelBuilder.Entity<SimDevice>()
                .HasAlternateKey(sd => new
                {
                    sd.SimId,
                    sd.DeviceId
                })
                .HasName("SimDevice_SimId_DeviceId_UQ");

            modelBuilder.Entity<FactorCombination>()
                .HasAlternateKey(mf => new
                {
                    mf.SimDeviceId,
                    mf.EndUserId
                })
                .HasName("MultiFactor_SimDeviceId_EndUserId_UQ");


            modelBuilder.Entity<AuthenticationLog>(authenticationLog =>
            {
                authenticationLog.Property(ac => ac.Id).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<Domain>(domain =>
            {
                domain.Property(d => d.Id).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<UserGroup>(userGroup =>
            {
                userGroup.Property(ug => ug.Id).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<Sim>(sim =>
            {
                sim.Property(s => s.Id).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<SimGroup>(simGroup =>
            {
                simGroup.Property(sg => sg.Id).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<Device>(device =>
            {
                device.Property(d => d.Id).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<AdDeviceSettingOfflineWindowsSignIn>(deviceSetting =>
            {
                deviceSetting.Property(ds => ds.Id).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<DeviceGroup>(deviceGroup =>
            {
                deviceGroup.Property(dg => dg.Id).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<Lte>(lte =>
            {
                lte.Property(l => l.Id).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<SimDevice>(simDevice =>
            {
                simDevice.Property(sd => sd.Id).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<FactorCombination>(factorCombination =>
            {
                factorCombination.Property(fc => fc.Id).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<AuthenticationState>(authState =>
            {
                authState.Property(authenticationState => authenticationState.Id).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<User>(user =>
            {
                user.Property(u => u.Id).HasValueGenerator<GuidValueGenerator>();
            });


            modelBuilder.Entity<MultiFactorAuthenticationLogSuccess>()
                .HasOne(da => da.FactorCombination)
                .WithMany(fc => fc.MultiFactorAuthenticationLogSuccesses)
                .HasForeignKey(fc => fc.FactorCombinationId);

            modelBuilder.Entity<DeauthenticationAuthenticationLogSuccess>()
                .HasOne(da => da.FactorCombination)
                .WithMany(fc => fc.Deauthentications)
                .HasForeignKey(fc => fc.FactorCombinationId);

            modelBuilder.Entity<MultiFactorAuthenticationStateDone>()
                .HasOne(mfae => mfae.FactorCombination)
                .WithOne(fc => fc.MultiFactorAuthenticationStateDone)
                .HasForeignKey<MultiFactorAuthenticationStateDone>(mfae => mfae.FactorCombinationId);

            modelBuilder.Entity<SimDeviceAuthenticationStateDone>()
                .HasOne(asde => asde.SimDevice)
                .WithOne(sd => sd.SimDeviceAuthenticationStateDone)
                .HasForeignKey<SimDeviceAuthenticationStateDone>(asde => asde.SimDeviceId);

            modelBuilder.Entity<SimDeviceAuthenticationLogFail>()
                .HasOne(asd => asd.Sim)
                .WithMany(sd => sd.SimDeviceAuthenticationLogFails)
                .HasForeignKey(fc => fc.SimId);

            modelBuilder.Entity<MultiFactorAuthenticationLogFail>()
                .HasOne(asd => asd.SimDevice)
                .WithMany(sd => sd.MultiFactorAuthenticationLogFails)
                .HasForeignKey(fc => fc.SimDeviceId);

            modelBuilder.Entity<SimDeviceAuthenticationLogSuccess>()
                .HasOne(asd => asd.SimDevice)
                .WithMany(sd => sd.SimDeviceAuthenticationLogSuccesses)
                .HasForeignKey(fc => fc.SimDeviceId);

            modelBuilder.Entity<FactorCombination>()
                .HasOne(fc => fc.EndUser)
                .WithMany(u => u.FactorCombinations)
                .HasForeignKey(fc => fc.EndUserId);

            modelBuilder.Entity<FactorCombination>()
                .HasOne(fc => fc.SimDevice)
                .WithMany(sd => sd.FactorCombinations)
                .HasForeignKey(fc => fc.SimDeviceId);

            modelBuilder.Entity<Device>()
                .HasOne(d => d.DeviceGroup)
                .WithMany(dg => dg.Devices)
                .HasForeignKey(d => d.DeviceGroupId);

            modelBuilder.Entity<Device>()
                .HasOne(d => d.Lte)
                .WithMany(l => l.Devices)
                .HasForeignKey(d => d.LteId);

            modelBuilder.Entity<AdDevice>()
                .HasOne(ad => ad.Domain)
                .WithMany(d => d.AdDevices)
                .HasForeignKey(ad => ad.DomainId);

            modelBuilder.Entity<AdDeviceSettingOfflineWindowsSignIn>()
                .HasOne(ds => ds.AdDevice)
                .WithOne(ad => ad.AdDeviceSettingOfflineWindowsSignIn)
                .HasForeignKey<AdDeviceSettingOfflineWindowsSignIn>(ds => ds.AdDeviceId);

            modelBuilder.Entity<DeviceGroup>()
                .HasOne(dg => dg.Organization)
                .WithMany(o => o.DeviceGroups)
                .HasForeignKey(dg => dg.OrganizationCode);

            modelBuilder.Entity<Sim>()
                .HasOne(s => s.SimGroup)
                .WithMany(sg => sg.Sims)
                .HasForeignKey(s => s.SimGroupId);

            modelBuilder.Entity<SimGroup>()
                .HasOne(sg => sg.Organization)
                .WithMany(o => o.SimGroups)
                .HasForeignKey(sg => sg.OrganizationCode);

            modelBuilder.Entity<SimDevice>()
                .HasOne(sad => sad.Sim)
                .WithOne(s => s.SimDevice)
                .HasForeignKey<SimDevice>(sad => sad.SimId);

            modelBuilder.Entity<SimDevice>()
                .HasOne(sad => sad.Device)
                .WithOne(d => d.SimDevice)
                .HasForeignKey<SimDevice>(sad => sad.DeviceId);

            modelBuilder.Entity<Domain>()
                .HasOne(c => c.Organization)
                .WithMany(c => c.Domains)
                .HasForeignKey(d => d.OrganizationCode);

            modelBuilder.Entity<EndUser>()
                .HasOne(d => d.Domain)
                .WithMany(d => d.EndUsers)
                .HasForeignKey(u => u.DomainId);

            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<AdminUser>("admin")
                .HasValue<GeneralUser>("general")
                .HasValue<SuperAdminUser>("superAdmin");


            modelBuilder.Entity<EndUser>()
                .HasOne(u => u.UserGroup)
                .WithMany(ug => ug.EndUsers)
                .HasForeignKey(u => u.UserGroupId);

            modelBuilder.Entity<UserGroup>()
                .HasOne(d => d.Domain)
                .WithMany(d => d.UserGroups)
                .HasForeignKey(d => d.DomainId);


            modelBuilder.Entity<AdDevice>()
                .HasBaseType<Device>();

            modelBuilder.Entity<AdminUser>()
                .HasBaseType<EndUser>();

            modelBuilder.Entity<GeneralUser>()
                .HasBaseType<EndUser>();

            modelBuilder.Entity<SuperAdminUser>()
                .HasBaseType<User>();

            modelBuilder.Entity<EndUser>()
                .HasBaseType<User>();

            modelBuilder.Entity<SimDeviceAuthenticationLogFail>()
                .HasBaseType<AuthenticationLog>();

            modelBuilder.Entity<MultiFactorAuthenticationLogFail>()
                .HasBaseType<AuthenticationLog>();

            modelBuilder.Entity<SimDeviceAuthenticationLogSuccess>()
                .HasBaseType<AuthenticationLog>();

            modelBuilder.Entity<MultiFactorAuthenticationLogSuccess>()
                .HasBaseType<AuthenticationLog>();

            modelBuilder.Entity<DeauthenticationAuthenticationLogSuccess>()
                .HasBaseType<AuthenticationLog>();

            modelBuilder.Entity<SimDeviceAuthenticationStateDone>()
                .HasBaseType<AuthenticationState>();

            modelBuilder.Entity<MultiFactorAuthenticationStateDone>()
                .HasBaseType<AuthenticationState>();

        }
    }
}
