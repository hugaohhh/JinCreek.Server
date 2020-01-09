using Admin.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Admin.Models
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly ApplicationDbContext _context;

        public OrganizationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(Organization organization)
        {
            organization.Id = Guid.NewGuid().ToString();
            _context.Organizations.Add(organization);
            _context.SaveChanges();
        }

        public IEnumerable<Organization> GetAll()
        {
            return _context.Organizations.ToList();
        }

        public Organization Get(string key)
        {
            return _context.Organizations.Find(key);
        }

        public Organization Remove(string key)
        {
            var organization = Get(key);
            if (organization == null)
            {
                return null;
            }
            var e = _context.Organizations.Remove(organization);
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
