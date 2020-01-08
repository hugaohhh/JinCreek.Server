namespace JinCreek.Server.Common.Repositories
{
    public class RadiusRepository
    {
        private readonly RadiusDbContext _dbContext;

        public RadiusRepository(RadiusDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
