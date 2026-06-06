namespace Viora.Application.Vivi.SendMessage;

public sealed record SendMessageResponse(Guid SessionId, IAsyncEnumerable<string> Stream);