using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System.Diagnostics.CodeAnalysis;
using JinCreek.Server.Common.Models;

namespace JinCreek.Server.Common.Repositories
{
    public class MainDbContext : DbContext
    {
        public DbSet<Company> Company { get; set; }

        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<UserGroup> UserGroup { get; set; }
        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<Domain> Domain { get; set; }
        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<User> User { get; set; }
        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<AdminUser> AdminUser { get; set; }
        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public DbSet<GeneralUser> GeneralUser { get; set; }

        public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>()
                .HasAlternateKey(c => c.Code)
                .HasName("Company_Code_UQ");

            modelBuilder.Entity<Domain>(domain =>
            {
                domain.Property(d => d.Id).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<UserGroup>(domain =>
            {
                domain.Property(ug => ug.Id).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<User>()
                .HasOne(d => d.Domain)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DomainId);

            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<AdminUser>("admin")
                .HasValue<GeneralUser>("general");

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

            modelBuilder.Entity<GeneralUser>()
                .HasBaseType<User>();
        }
    }
}
