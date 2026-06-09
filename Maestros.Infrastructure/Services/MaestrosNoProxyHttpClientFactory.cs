using Microsoft.Identity.Client;

namespace Maestros.Infrastructure.Services
{
    internal class MaestrosNoProxyHttpClientFactory : IMsalHttpClientFactory
    {
        public HttpClient GetHttpClient()
        {
            var handler = new HttpClientHandler
            {
                UseProxy = false,
                Proxy    = null
            };
            return new HttpClient(handler);
        }
    }
}
