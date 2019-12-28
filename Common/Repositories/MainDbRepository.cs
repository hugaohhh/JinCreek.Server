using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Models.Db;

namespace Common.Repositories
{
    public class MainDbRepository
    {
        private MainDbContext _dbContext;

        public MainDbRepository(MainDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int Create(Domain domain)
        {
            using(_dbContext)
            {
                _dbContext.Domain.Add(domain);
                return _dbContext.SaveChanges();
            }
        }

        public int Create(UserGroup userGroup)
        {
            using (_dbContext)
            {
                _dbContext.Domain.Add(userGroup.Domain);
                _dbContext.UserGroup.Add(userGroup);

                return _dbContext.SaveChanges();
            }
        }
    }
}
