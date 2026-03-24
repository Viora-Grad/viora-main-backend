using MediatR;
using Viora.Domain.Abstraction;

namespace Viora.Application.Abstraction.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }