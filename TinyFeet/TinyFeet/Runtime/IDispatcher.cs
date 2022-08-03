using System.Threading;
using System.Threading.Tasks;

using TinyFeet.Interfaces.Query;
using TinyFeet.Interfaces.Command;

namespace TinyFeet.Runtime;

public interface IDispatcher
{
    Task<ICommandResult> HandleAsync<T>(T command, CancellationToken cancellationToken) where T : ICommand;

    Task<TQueryResult> HandleAsync<TQuery, TQueryResult>(TQuery query, CancellationToken cancellationToken)
        where TQuery : IQuery
        where TQueryResult : IQueryResult;
}
