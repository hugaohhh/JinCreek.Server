using System;
using JinCreek.Server.Common.Models;
using System.Collections.Generic;

namespace JinCreek.Server.Common.Repositories
{
    public interface IOrganizationRepository
    {
        void Add(Organization organization);
        IEnumerable<Organization> GetAll();
        Organization Get(Guid id);
        Organization Remove(Guid id);
        void Update(Organization organization);
    }
}
