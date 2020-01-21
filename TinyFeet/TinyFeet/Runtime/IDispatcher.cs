namespace TinyFeet.Runtime
{
    using System.Threading.Tasks;

    using TinyFeet.Interfaces.Query;
    using TinyFeet.Interfaces.Command;

    public interface IDispatcher
    {
        Task<ICommandResult> HandleAsync<T>(T command) where T : ICommand;

        Task<TQueryResult> HandleAsync<TQuery, TQueryResult>(TQuery query)
            where TQuery : IQuery
            where TQueryResult : IQueryResult;
    }
}
