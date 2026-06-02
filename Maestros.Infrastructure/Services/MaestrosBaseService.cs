using Microsoft.AspNetCore.Http;

namespace Maestros.Infrastructure.Services
{
    public abstract class MaestrosBaseService
    {
        private readonly IHttpContextAccessor _ctx;

        protected MaestrosBaseService(IHttpContextAccessor ctx)
        {
            _ctx = ctx;
        }

        protected void PropagateHeaders(HttpRequestMessage request)
        {
            var headers = _ctx.HttpContext?.Request.Headers;
            if (headers is null) return;

            foreach (var key in new[]
            {
                "X-User-Oid", "X-Tenant-Id", "X-User-Email",
                "X-User-Name", "X-Session-Id", "X-Schema"
            })
            {
                if (headers.TryGetValue(key, out var val))
                    request.Headers.TryAddWithoutValidation(key, (string?)val);
            }
        }
    }
}
