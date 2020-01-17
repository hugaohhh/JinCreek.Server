using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Admin.Services
{
    public class OrganizationAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Organization>
    {
        private readonly UserRepository _userRepository;

        public OrganizationAuthorizationHandler(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Organization resource)
        {
            // TODO: 実装 see https://docs.microsoft.com/ja-jp/aspnet/core/security/authorization/resourcebased?view=aspnetcore-3.1#write-a-resource-based-handler
            if (requirement.Name == Operations.Create.Name)
            {
                context.Succeed(requirement);
            }
            if (requirement.Name == Operations.Read.Name)
            {
                var role = context.User.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Role)?.Value;
                if (role == "SuperAdminUser") // TODO: extract const
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                // TODO: 一括読み込みする？ see https://docs.microsoft.com/ja-jp/ef/core/querying/related-data
                var user = (EndUser)_userRepository.GetUser(Guid.Parse(context.User.Identity.Name));
                var domain = _userRepository.GetDomain(user.DomainId);
                if (domain?.OrganizationCode == resource.Code)
                {
                    context.Succeed(requirement);
                }
            }
            if (requirement.Name == Operations.Update.Name)
            {
                context.Succeed(requirement);
            }
            if (requirement.Name == Operations.Delete.Name)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }

    public static class Operations
    {
        public static OperationAuthorizationRequirement Create = new OperationAuthorizationRequirement { Name = nameof(Create) };
        public static OperationAuthorizationRequirement Read = new OperationAuthorizationRequirement { Name = nameof(Read) };
        public static OperationAuthorizationRequirement Update = new OperationAuthorizationRequirement { Name = nameof(Update) };
        public static OperationAuthorizationRequirement Delete = new OperationAuthorizationRequirement { Name = nameof(Delete) };
    }
}
