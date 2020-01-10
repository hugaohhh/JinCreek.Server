using System;
using System.Threading.Tasks;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Admin.CustomProvider
{
    public class DapperUsersTable
    {
        private readonly UserRepository _userRepository;

        public DapperUsersTable(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser applicationUser)
        {
            var adminUser = new AdminUser
            {
                Id = new Guid(), // TODO: ここで生成するの？
                FirstName = applicationUser.NormalizedUserName,
                Password = applicationUser.PasswordHash,
                DomainId = Guid.Parse("0e92ce58-9790-4261-82ff-9e1679d4f398"), // TODO: generalize
                UserGroupId = Guid.Parse("8df0cf70-25bc-4668-8c47-1b0166761c72") // TODO: generalize
            };
            _userRepository.Create(adminUser);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public async Task<ApplicationUser> FindByIdAsync(Guid userId)
        {
            var adminUser = _userRepository.FindAdminUser(userId.ToString());
            if (adminUser == null)
            {
                return null;
            }

            return new ApplicationUser
            {
                Id = adminUser.Id,
                Name = adminUser.FirstName
            };
        }

        public async Task<ApplicationUser> FindByNameAsync(string userName)
        {
            var adminUser = _userRepository.FindAdminUserByName(userName);
            if (adminUser == null)
            {
                return null;
            }

            return new ApplicationUser
            {
                Id = adminUser.Id,
                Name = adminUser.FirstName,
                PasswordHash = adminUser.Password
            };
        }
    }
}
