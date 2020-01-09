using JinCreek.Server.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JinCreek.Server.Common.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly MainDbContext _context;

        public OrganizationRepository(MainDbContext context)
        {
            _context = context;
        }

        public void Add(Organization organization)
        {
            organization.Id = Guid.NewGuid();
            _context.Organization.Add(organization);
            _context.SaveChanges();
        }

        public IEnumerable<Organization> GetAll()
        {
            return _context.Organization.ToList();
        }

        public Organization Get(string key)
        {
            return _context.Organization.Find(key);
        }

        public Organization Remove(string key)
        {
            var organization = Get(key);
            if (organization == null)
            {
                return null;
            }
            var e = _context.Organization.Remove(organization);
            _context.SaveChanges();
            return e.Entity;
        }

        public void Update(Organization organization)
        {
            _context.Entry(organization).State = EntityState.Modified;
            _context.SaveChanges();
        }
    }
}
