using Microsoft.AspNetCore.Authorization;

namespace Resguardo.Web.Authorization
{
    public class PermissionAttribute : AuthorizeAttribute
    {
        public PermissionAttribute(string permission)
        {
            Policy = $"PERMISSION:{permission}";
        }
    }
}
