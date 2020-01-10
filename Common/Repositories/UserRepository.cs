using System;
using System.Collections.Generic;
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

        public int Create(AdminUser adminUser)
        {
            _dbContext.AdminUser.Add(adminUser);
            return _dbContext.SaveChanges();
        }

        public AdminUser GetAdminUser(Guid id)
        {
            return _dbContext.AdminUser.Find(id);
        }

        public AdminUser GetAdminUserByName(string name)
        {
            return _dbContext.AdminUser.SingleOrDefault(x => x.FirstName == name);
        }

        public UserGroup GetUserGroup(string userGroupName)
        {
            var list = _dbContext.UserGroup
                .Include(ug => ug.Domain)
                .Where(ug => ug.UserGroupName == userGroupName);

            return list.First();
        }


        //
        // Organizations
        //

        public void Create(Organization organization)
        {
            organization.Id = Guid.NewGuid();
            _dbContext.Organization.Add(organization);
            _dbContext.SaveChanges();
        }

        public IEnumerable<Organization> GetOrganization()
        {
            return _dbContext.Organization.ToList();
        }

        public Organization GetOrganization(Guid id)
        {
            return _dbContext.Organization.Find(id);
        }

        public Organization Remove(Guid id)
        {
            var organization = GetOrganization(id);
            if (organization == null)
            {
                return null;
            }
            var e = _dbContext.Organization.Remove(organization);
            _dbContext.SaveChanges();
            return e.Entity;
        }

        public void Update(Organization organization)
        {
            _dbContext.Entry(organization).State = EntityState.Modified;
            _dbContext.SaveChanges();
        }
    }
}