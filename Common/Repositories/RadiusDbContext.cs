using Microsoft.EntityFrameworkCore;

namespace JinCreek.Server.Common.Repositories
{
    public class RadiusDbContext : DbContext
    {
        public RadiusDbContext(DbContextOptions<RadiusDbContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
