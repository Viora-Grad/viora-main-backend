using Viora.Application.Abstractions.Messaging;
using Viora.Application.Analytics.Contracts;

namespace Viora.Application.Analytics.GetDashboard;

// Org id comes from the token, not the request. From/To/Granularity are the query-string parameters.
public sealed record GetDashboardQuery(DateTime? From, DateTime? To, string? Granularity) : IQuery<DashboardData>;
