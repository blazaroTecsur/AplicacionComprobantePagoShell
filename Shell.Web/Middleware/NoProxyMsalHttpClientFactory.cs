using Microsoft.Identity.Client;

namespace Shell.Web.Middleware
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