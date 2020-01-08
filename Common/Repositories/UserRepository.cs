using JinCreek.Server.Common.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace JinCreek.Server.Common.Repositories
{
    public class UserRepository
    {
        private readonly MainDbContext _dbContext;

        public UserRepository(MainDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int Create(Domain domain)
        {
            _dbContext.Domain.Add(domain);
            return _dbContext.SaveChanges();
        }

        public int Create(UserGroup userGroup)
        {
            _dbContext.Domain.Add(userGroup.Domain);
            _dbContext.UserGroup.Add(userGroup);

            return _dbContext.SaveChanges();
        }

        public int Create(User user)
        {
            _dbContext.User.Add(user);
            return _dbContext.SaveChanges();
        }

        public UserGroup GetUserGroup(string userGroupName)
        {
            var list = _dbContext.UserGroup
                .Include(ug => ug.Domain)
                .Where(ug => ug.UserGroupName == userGroupName);

            return list.First();
        }
    }
}