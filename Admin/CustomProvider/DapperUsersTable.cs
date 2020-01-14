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
            var superAdminUser = new SuperAdminUser
            {
                Id = new Guid(), // TODO: ここで生成するの？
                AccountName = applicationUser.NormalizedUserName,
                Password = applicationUser.PasswordHash,
            };
            _userRepository.Create(superAdminUser);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public async Task<ApplicationUser> FindByIdAsync(Guid userId)
        {
            var user = _userRepository.GetUser(userId);
            if (user == null)
            {
                return null;
            }

            return new ApplicationUser
            {
                Id = user.Id,
                Name = user.AccountName,
                Role = user.GetType().Name,
                PasswordHash = user.GetType() switch
                {
                    var type when type == typeof(AdminUser) => ((AdminUser)user).Password,
                    var type when type == typeof(SuperAdminUser) => ((SuperAdminUser)user).Password,
                    _ => "",
                },
            };
        }

        public async Task<ApplicationUser> FindByNameAsync(string userName)
        {
            var user = _userRepository.GetUserByName(userName);
            if (user == null)
            {
                return null;
            }

            return new ApplicationUser
            {
                Id = user.Id,
                Name = user.AccountName,
                Role = user.GetType().Name,
                PasswordHash = user.GetType() switch
                {
                    var type when type == typeof(AdminUser) => ((AdminUser)user).Password,
                    var type when type == typeof(SuperAdminUser) => ((SuperAdminUser)user).Password,
                    _ => "",
                },
            };
        }
    }
}
