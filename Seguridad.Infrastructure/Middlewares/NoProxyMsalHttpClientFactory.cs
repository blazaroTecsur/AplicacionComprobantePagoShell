using Microsoft.Identity.Client;

namespace Seguridad.Infrastructure.Middlewares
{
    public class NoProxyMsalHttpClientFactory : IMsalHttpClientFactory
    {
        public HttpClient GetHttpClient()
        {
            var handler = new HttpClientHandler
            {
                UseProxy = false,
                Proxy = null
            };
            return new HttpClient(handler);
        }
    }
}