using System.Net.Http.Headers;
using System.Text;
using Reciclaje.Aplicacion.Interfaces;

namespace Reciclaje.Aplicacion.Servicios;

public class SyteLineHttpClient(IHttpClientFactory factory) : ISyteLineHttpClientFactory
{
    public async Task<string> PostFormAsync(string url, string basicAuth,
                                             Dictionary<string, string> form)
    {
        var http = factory.CreateClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", basicAuth);
        var r = await http.PostAsync(url, new FormUrlEncodedContent(form));
        return await r.Content.ReadAsStringAsync();
    }

    public async Task<string> GetAsync(string url, string bearer, string mongooseConfig)
    {
        var http = factory.CreateClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", bearer);
        http.DefaultRequestHeaders.Add("X-Infor-MongooseConfig", mongooseConfig);
        return await (await http.GetAsync(url)).Content.ReadAsStringAsync();
    }

    public async Task<string> PutAsync(string url, string bearer,
                                        string mongooseConfig, string jsonBody)
    {
        var http = factory.CreateClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", bearer);
        http.DefaultRequestHeaders.Add("X-Infor-MongooseConfig", mongooseConfig);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        return await (await http.PutAsync(url, content)).Content.ReadAsStringAsync();
    }
}