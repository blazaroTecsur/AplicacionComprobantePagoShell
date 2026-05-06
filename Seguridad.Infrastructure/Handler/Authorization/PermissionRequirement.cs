using Microsoft.AspNetCore.Authorization;

namespace Seguridad.Infrastructure.Handler.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
