using Microsoft.AspNetCore.Mvc.Filters;

namespace Shell.Web.Middleware
{
    public class NoCacheFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var headers = context.HttpContext.Response.Headers;

            headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            headers["Pragma"] = "no-cache";
            headers["Expires"] = "0";
        }
        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
