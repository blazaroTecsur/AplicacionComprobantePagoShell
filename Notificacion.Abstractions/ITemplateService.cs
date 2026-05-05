namespace Notificacion.Abstractions
{
    public interface ITemplateService
    {
        Task<string> GetTemplateAsync(string name);
    }
}
