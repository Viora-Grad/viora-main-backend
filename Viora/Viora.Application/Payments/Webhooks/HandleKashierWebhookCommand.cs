using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Payments.Webhooks;

public enum WebhookKind
{
    Subscription,
    Addon,
}

// Raw webhook delivery from Kashier. Kind comes from which route received it
// (the dynamic serverWebhook we set when creating the session).
public sealed record HandleKashierWebhookCommand(WebhookKind Kind, string RawBody, string SignatureHeader) : ICommand;
