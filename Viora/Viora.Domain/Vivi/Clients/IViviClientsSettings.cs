namespace Viora.Domain.Vivi.Clients;

public interface IViviClientsSettings
{
    public string ClientChatApiKey { get; set; }
    public string ClientModel { get; set; }
    public string BaseUrl { get; set; }
}
