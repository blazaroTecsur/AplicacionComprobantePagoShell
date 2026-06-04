using System.Net.Http.Headers;

namespace Maestros.Infrastructure.Services
{
    public abstract class MaestrosBaseService
    {
        private readonly MaestrosTokenService _tokenService;

        protected MaestrosBaseService(MaestrosTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        protected async Task AgregarAuthHeaderAsync(HttpRequestMessage request)
        {
            var token = await _tokenService.GetTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
