using System;
using System.Security.Principal;

namespace Admin.CustomProvider
{
    public class ApplicationUser : IIdentity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string NormalizedUserName { get; internal set; }
        public string Role { get; set; }

        public string AuthenticationType { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }
    }
}
