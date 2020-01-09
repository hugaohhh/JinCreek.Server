using System.Collections.Generic;

namespace Admin.Models
{
    public interface IOrganizationRepository
    {
        void Add(Organization organization);
        IEnumerable<Organization> GetAll();
        Organization Get(string key);
        Organization Remove(string key);
        void Update(Organization organization);
    }
}
