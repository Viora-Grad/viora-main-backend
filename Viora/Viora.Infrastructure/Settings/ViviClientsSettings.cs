using Viora.Domain.Vivi.Clients;

namespace Viora.Infrastructure.Settings;

public class ViviClientsSettings : IViviClientsSettings
{
    public string ClientChatApiKey { get; set; } = default!;
    public string ClientModel { get; set; } = default!;
    public string BaseUrl { get; set; } = default!;
}
