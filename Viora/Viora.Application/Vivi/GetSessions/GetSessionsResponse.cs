namespace Viora.Application.Vivi.GetSessions;

public sealed record GetSessionsResponse(Guid Id, string Name, DateTime LatestActivity);
