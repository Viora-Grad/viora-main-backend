using MediatR;
using Viora.Domain.Abstractions;

namespace Viora.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }