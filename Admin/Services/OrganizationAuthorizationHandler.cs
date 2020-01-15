using JinCreek.Server.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Threading.Tasks;

namespace Admin.Services
{
    public class OrganizationAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Organization>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Organization resource)
        {
            // TODO: 実装 see https://docs.microsoft.com/ja-jp/aspnet/core/security/authorization/resourcebased?view=aspnetcore-3.1#write-a-resource-based-handler
            if (requirement.Name == Operations.Create.Name)
            {
                context.Succeed(requirement);
            }
            if (requirement.Name == Operations.Read.Name)
            {
                context.Succeed(requirement);
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
