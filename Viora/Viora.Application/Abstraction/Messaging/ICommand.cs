using MediatR;
using Viora.Domain.Abstraction;

namespace Viora.Application.Abstraction.Messaging;

public interface ICommand<TReponse> : IRequest<Result<TReponse>>, IBaseCommand { }

public interface ICommand : IRequest<Result>, IBaseCommand { }

public interface IBaseCommand { }