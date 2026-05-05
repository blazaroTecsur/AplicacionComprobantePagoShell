using Microsoft.AspNetCore.Authorization;

namespace ComprobantePago.Web.Authorization
{
    public class PermissionAttribute : AuthorizeAttribute
    {
        public PermissionAttribute(string permission)
        {
            Policy = $"PERMISSION:{permission}";
        }
    }
}
