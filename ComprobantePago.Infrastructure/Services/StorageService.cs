using ComprobantePago.Application.Interfaces.Services;
using ComprobantePago.Application.Settings;
using Microsoft.Extensions.Options;

namespace ComprobantePago.Infrastructure.Services
{
    public class StorageService(IOptions<StorageSettings> options) : IStorageService
    {
        private readonly string _raiz = options.Value.Path;

        public async Task<string> GuardarAsync(string carpeta, string nombreArchivo, byte[] contenido)
        {
            var dirAbsoluto = Path.Combine(_raiz, carpeta);
            Directory.CreateDirectory(dirAbsoluto);

            var rutaAbsoluta = Path.Combine(dirAbsoluto, nombreArchivo);
            await File.WriteAllBytesAsync(rutaAbsoluta, contenido);

            // Ruta relativa a la raíz — es lo que se guarda en BD
            return Path.Combine(carpeta, nombreArchivo);
        }

        public async Task<byte[]> LeerAsync(string rutaRelativa)
        {
            var rutaAbsoluta = Path.Combine(_raiz, rutaRelativa);
            if (!File.Exists(rutaAbsoluta))
                throw new FileNotFoundException($"Archivo no encontrado: {rutaRelativa}");
            return await File.ReadAllBytesAsync(rutaAbsoluta);
        }

        public Task EliminarAsync(string rutaRelativa)
        {
            var rutaAbsoluta = Path.Combine(_raiz, rutaRelativa);
            if (File.Exists(rutaAbsoluta))
                File.Delete(rutaAbsoluta);
            return Task.CompletedTask;
        }
    }
}
