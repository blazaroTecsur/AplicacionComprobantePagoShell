using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace ComprobantePago.Infrastructure.Services
{
    public class TokenSeguridadService
    {
        private readonly IConfidentialClientApplication _app;
        private readonly IConfiguration _config;
        public TokenSeguridadService(IConfiguration config, IMsalHttpClientFactory httpClientFactory)
        {
            _config = config;
            _app = ConfidentialClientApplicationBuilder
                .Create(config["AzureAd:ClientId"])
                .WithClientSecret(config["AzureAd:ClientSecret"])
                .WithAuthority($"https://login.microsoftonline.com/{config["AzureAd:TenantId"]}")
                 .WithHttpClientFactory(httpClientFactory) // 🔥 CLAVE
                .Build();
        }
        public async Task<string> GetTokenAsync()
        {            
            var scope = _config["ApiSettings:Seguridad:Scope"];
            var result = await _app                
            .AcquireTokenForClient(new[] { scope })
            .ExecuteAsync();
            return result.AccessToken;
        }
    }
}
