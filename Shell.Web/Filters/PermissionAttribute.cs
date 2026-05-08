using Microsoft.AspNetCore.Mvc;

namespace Shell.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class PermissionAttribute : TypeFilterAttribute
    {
        public PermissionAttribute(string codigo) : base(typeof(PermissionFilter))
        {
            Arguments = [codigo];
        }
    }
}
