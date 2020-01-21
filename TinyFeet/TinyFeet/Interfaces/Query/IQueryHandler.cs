namespace TinyFeet.Interfaces.Query
{
    using System.Threading.Tasks;

    public interface IQueryHandler<in TQuery, TQueryResult>
        where TQuery : IQuery
        where TQueryResult : IQueryResult
    {
        Task<TQueryResult> HandleAsync(TQuery query);
    }
}
