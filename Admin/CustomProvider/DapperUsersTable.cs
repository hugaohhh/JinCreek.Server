using JinCreek.Server.Common.Exceptions;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using System;

namespace JinCreek.Server.Admin.CustomProvider
{
    public class DapperUsersTable
    {
        private readonly UserRepository _userRepository;

        public DapperUsersTable(UserRepository userRepository)
        {
            _userRepository = userRepository;
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
                AccountName = user.AccountName,
                Role = user.GetType().Name,
                PasswordHash = user.GetType() switch
                {
                    var type when type == typeof(UserAdmin) => ((UserAdmin)user).Password,
                    var type when type == typeof(SuperAdmin) => ((SuperAdmin)user).Password,
                    _ => "",
                },
            };
        }

        public ApplicationUser FindByName(string userName)
        {
            try
            {
                var user = _userRepository.GetUserByName(userName);
                return new ApplicationUser
                {
                    Id = user.Id,
                    AccountName = user.AccountName,
                    Role = user.GetType().Name,
                    PasswordHash = user.GetType() switch
                    {
                        var type when type == typeof(UserAdmin) => ((UserAdmin)user).Password,
                        var type when type == typeof(SuperAdmin) => ((SuperAdmin)user).Password,
                        _ => "",
                    },
                };
            }
            catch (EntityNotFoundException)
            {
                return null;
            }
        }
    }
}
