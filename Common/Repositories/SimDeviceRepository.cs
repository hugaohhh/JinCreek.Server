namespace JinCreek.Server.Common.Repositories
{
    public class SimDeviceRepository
    {
        private readonly MainDbContext _dbContext;

        public SimDeviceRepository(MainDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
