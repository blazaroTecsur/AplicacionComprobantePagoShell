using Microsoft.AspNetCore.Hosting;
using Notificacion.Abstractions;

namespace Notificacion.Infrastructure.Email
{
    public class TemplateService : ITemplateService
    {
        private readonly IWebHostEnvironment _env;
        public TemplateService(IWebHostEnvironment env)
        {
            _env = env;
        }
        public async Task<string> GetTemplateAsync(string archivo)
        {
            var ruta = Path.Combine(_env.ContentRootPath, "Templates", archivo);
            return await File.ReadAllTextAsync(ruta);
        }
    }
}