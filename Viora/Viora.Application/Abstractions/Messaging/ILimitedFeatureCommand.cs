namespace Viora.Application.Abstractions.Messaging;

public interface ILimitedFeatureCommand : ICommand, IBaseLimitedFeatureCommand { }

public interface ILimitedFeatureCommand<TResponse> : ICommand<TResponse>, IBaseLimitedFeatureCommand { }

public interface IBaseLimitedFeatureCommand
{
    public Guid OrganizationId { get; }
    public Guid LimitedFeatureId { get; init; }
    /// <summary>
    /// represents the amount to be added or removed the quota, +ve means add an amount (restore), negative means consume amount
    /// </summary>
    public long DeltaChange { get; init; }
}