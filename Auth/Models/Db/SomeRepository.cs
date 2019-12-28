using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auth.Models.Db
{
    public class SomeRepository
    {
        private MdbContext _dbContext;

        public SomeRepository(MdbContext dbContext)
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
