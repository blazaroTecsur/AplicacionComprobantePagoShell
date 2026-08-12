namespace Reciclaje.Aplicacion.Interfaces;

public interface ISyteLineHttpClientFactory
{
    Task<string> PostFormAsync(string url, string basicAuth,
                               Dictionary<string, string> form);
    Task<string> GetAsync(string url, string bearer, string mongooseConfig);
    Task<string> PutAsync(string url, string bearer, string mongooseConfig, string jsonBody);
}