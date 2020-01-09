using JinCreek.Server.Common.Models;
using System.Collections.Generic;

namespace JinCreek.Server.Common.Repositories
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
