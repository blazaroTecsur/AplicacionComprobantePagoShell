namespace ComprobantePago.Application.Interfaces.Services
{
    public interface IStorageService
    {
        /// <summary>Guarda los bytes en disco y retorna la ruta relativa almacenada en BD.</summary>
        Task<string> GuardarAsync(string carpeta, string nombreArchivo, byte[] contenido);

        /// <summary>Lee el archivo desde disco y retorna sus bytes.</summary>
        Task<byte[]> LeerAsync(string rutaRelativa);

        /// <summary>Elimina el archivo del disco si existe.</summary>
        Task EliminarAsync(string rutaRelativa);
    }
}
