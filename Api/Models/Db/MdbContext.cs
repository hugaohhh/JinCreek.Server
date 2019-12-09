using Microsoft.EntityFrameworkCore;

namespace Api.Models.Db
{
    public class MdbContext : DbContext
    {
        public DbSet<Company> Company { get; set; }

        public MdbContext(DbContextOptions<MdbContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>()
                .HasAlternateKey(c => c.Code)
                .HasName("Company_Code_UQ");
        }
    }
}
