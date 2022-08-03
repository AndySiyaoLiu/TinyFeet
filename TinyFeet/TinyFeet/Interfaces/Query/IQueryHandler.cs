using System.Threading;
using System.Threading.Tasks;

namespace TinyFeet.Interfaces.Query;

public interface IQueryHandler<in TQuery, TQueryResult>
    where TQuery : IQuery
    where TQueryResult : IQueryResult
{
    Task<TQueryResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
