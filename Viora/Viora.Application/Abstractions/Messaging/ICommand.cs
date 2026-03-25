using MediatR;
using Viora.Domain.Abstractions;

namespace Viora.Application.Abstractions.Messaging;

public interface ICommand<TReponse> : IRequest<Result<TReponse>>, IBaseCommand { }

public interface ICommand : IRequest<Result>, IBaseCommand { }

public interface IBaseCommand { }