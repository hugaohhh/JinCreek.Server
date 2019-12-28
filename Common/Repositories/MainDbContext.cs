using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Common.Models.Db;

namespace Common.Repositories
{
    public class MainDbContext : DbContext
    {
        public DbSet<Company> Company { get; set; }

        public DbSet<UserGroup> UserGroup { get; set; }
        public DbSet<Domain> Domain { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<AdminUser> AdminUser { get; set; }
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
                domain.Property(d => d.DomainId).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<UserGroup>(domain =>
            {
                domain.Property(ug => ug.UserGroupId).HasValueGenerator<GuidValueGenerator>();
            });

            modelBuilder.Entity<User>()
                .HasOne<Domain>(d => d.Domain)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DomainId);

            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("user_type");

            modelBuilder.Entity<User>(user =>
            {
                user.Property(u => u.UserId).HasValueGenerator<GuidValueGenerator>();
            });


            modelBuilder.Entity<User>()
                .HasOne<UserGroup>(u => u.UserGroup)
                .WithMany(ug => ug.Users)
                .HasForeignKey(u => u.UserGroupId);

            modelBuilder.Entity<UserGroup>()
                .HasOne<Domain>(d => d.Domain)
                .WithMany(d => d.UserGroups)
                .HasForeignKey(d => d.DomainId);

            modelBuilder.Entity<AdminUser>()
                .HasBaseType<User>();

            modelBuilder.Entity<GeneralUser>()
                .HasBaseType<User>();
        }
    }
}
