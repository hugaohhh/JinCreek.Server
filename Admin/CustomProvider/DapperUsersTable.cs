using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Identity;
using System;

namespace Admin.CustomProvider
{
    public class DapperUsersTable
    {
        private readonly UserRepository _userRepository;

        public DapperUsersTable(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IdentityResult Create(ApplicationUser applicationUser)
        {
            _userRepository.Create(new SuperAdminUser
            {
                Id = new Guid(),
                AccountName = applicationUser.NormalizedUserName,
                Password = applicationUser.PasswordHash,
            });

            return IdentityResult.Success;
        }

        public ApplicationUser FindById(Guid userId)
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

        public ApplicationUser FindByName(string userName)
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
