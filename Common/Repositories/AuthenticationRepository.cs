namespace JinCreek.Server.Common.Repositories
{
    public class AuthenticationRepository
    {
        private readonly MainDbContext _dbContext;

        public AuthenticationRepository(MainDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
