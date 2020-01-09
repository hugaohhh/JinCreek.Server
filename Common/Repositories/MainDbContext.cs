using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System.Diagnostics.CodeAnalysis;
using JinCreek.Server.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

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
        public DbSet<DeviceSetting> DeviceSetting { get; set; }

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
        public DbSet<Authentication> Authentication { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<MultiFactorAuthentication> MultiFactorAuthentication { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<Deauthentication> Deauthentication { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<SimDeviceAuthentication> SimDeviceAuthentication { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<MultiFactorAuthenticationEnd> MultiFactorAuthenticationEnd { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<SimDeviceAuthenticationEnd> SimDeviceAuthenticationEnd { get; set; }

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
        // DbSetアクセスのため自動プロパティを利用
        //[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        //public DbSet<GeneralUser> GeneralUser { get; set; }

        public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new AuthenticationRepository.EFLoggerProvider());
            optionsBuilder.UseLoggerFactory(loggerFactory);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Organization>()
                .HasAlternateKey(c => c.Code)
                .HasName("Organization_Code_UQ");

            //modelBuilder.Entity<DeviceGroup>()
            //    .HasAlternateKey(dg => new {
            //        dg.OrganizationId, dg.OsType, dg.Version
            //    }).HasName("DeviceGroup_Code_UQ");

            modelBuilder.Entity<AuthenticationLog>(authenticationLog =>
            {
                authenticationLog.Property(ac => ac.Id).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<Organization>(organization =>
            {
                organization.Property(o => o.Id).HasValueGenerator<GuidValueGenerator>();
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

            modelBuilder.Entity<DeviceSetting>(deviceSetting =>
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

            modelBuilder.Entity<UserSetting>()
                .HasOne(us => us.User)
                .WithOne(u => u.UserSetting)
                .HasForeignKey<UserSetting>(us => us.UserId);

            modelBuilder.Entity<MultiFactorAuthentication>()
                .HasOne(da => da.FactorCombination)
                .WithMany(fc => fc.MultiFactorAuthentications)
                .HasForeignKey(fc => fc.FactorCombinationId);

            modelBuilder.Entity<Deauthentication>()
                .HasOne(da => da.FactorCombination)
                .WithMany(fc => fc.Deauthentications)
                .HasForeignKey(fc => fc.FactorCombinationId);

            modelBuilder.Entity<MultiFactorAuthenticationEnd>()
                .HasOne(mfae => mfae.FactorCombination)
                .WithOne(fc => fc.MultiFactorAuthenticationEnd)
                .HasForeignKey<MultiFactorAuthenticationEnd>(mfae => mfae.FactorCombinationId);

            modelBuilder.Entity<SimDeviceAuthenticationEnd>()
                .HasOne(asde => asde.SimDevice)
                .WithOne(sd => sd.SimDeviceAuthenticationEnd)
                .HasForeignKey<SimDeviceAuthenticationEnd>(asde => asde.SimDeviceId);

            modelBuilder.Entity<SimDeviceAuthentication>()
                .HasOne(asd => asd.SimDevice)
                .WithMany(sd => sd.SimDeviceAuthentications)
                .HasForeignKey(fc => fc.SimDeviceId);

            modelBuilder.Entity<FactorCombination>()
                .HasOne(fc => fc.User)
                .WithMany(u => u.FactorCombinations)
                .HasForeignKey(fc => fc.UserId);

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

            modelBuilder.Entity<DeviceSetting>()
                .HasOne(ds => ds.AdDevice)
                .WithOne(ad => ad.DeviceSetting)
                .HasForeignKey<DeviceSetting>(ds => ds.AdDeviceId);

            modelBuilder.Entity<DeviceGroup>()
                .HasOne(dg => dg.Organization)
                .WithMany(o => o.DeviceGroups)
                .HasForeignKey(dg => dg.OrganizationId);

            modelBuilder.Entity<Sim>()
                .HasOne(s => s.SimGroup)
                .WithMany(sg => sg.Sims)
                .HasForeignKey(s => s.SimGroupId);

            modelBuilder.Entity<SimGroup>()
                .HasOne(sg => sg.Organization)
                .WithMany(o => o.SimGroups)
                .HasForeignKey(sg => sg.OrganizationId);

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
                .HasForeignKey(d => d.OrganizationId);

            modelBuilder.Entity<User>()
                .HasOne(d => d.Domain)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DomainId);

            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<AdminUser>("admin")
                .HasValue<User>("general")
                .HasValue<SuperAdminUser>("superAdmin");

            modelBuilder.Entity<User>(user =>
            {
                user.Property(u => u.Id).HasValueGenerator<GuidValueGenerator>();
            });


            modelBuilder.Entity<User>()
                .HasOne(u => u.UserGroup)
                .WithMany(ug => ug.Users)
                .HasForeignKey(u => u.UserGroupId);

            modelBuilder.Entity<UserGroup>()
                .HasOne(d => d.Domain)
                .WithMany(d => d.UserGroups)
                .HasForeignKey(d => d.DomainId);

            modelBuilder.Entity<AdminUser>()
                .HasBaseType<User>();

            modelBuilder.Entity<AdDevice>()
                .HasBaseType<Device>();

            modelBuilder.Entity<SuperAdminUser>()
                .HasBaseType<AdminUser>();

        }
    }
}
