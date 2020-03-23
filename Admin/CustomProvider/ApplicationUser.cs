using System;

namespace JinCreek.Server.Admin.CustomProvider
{
    public class ApplicationUser
    {
        public Guid Id { get; set; }
        public string AccountName { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}
