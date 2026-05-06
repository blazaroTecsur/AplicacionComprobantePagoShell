using Microsoft.AspNetCore.Authorization;

namespace Seguridad.Infrastructure.Handler.Authorization
{
    public class PermissionAttribute : AuthorizeAttribute
    {
        public PermissionAttribute(string permission)
        {
            Policy = $"PERMISSION:{permission}";
        }
    }
}
