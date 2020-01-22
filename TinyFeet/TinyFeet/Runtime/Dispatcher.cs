namespace TinyFeet.Runtime
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;

    using TinyFeet.Interfaces.Query;
    using TinyFeet.Interfaces.Command;
    
    public class Dispatcher : IDispatcher
    {
        #region Fields

        private readonly IServiceProvider serviceProvider;

        #endregion Fields

        #region Constructor

        public Dispatcher(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        #endregion Constructor

        #region Methods

        public async Task<ICommandResult> HandleAsync<T>(T command) where T : ICommand
        {
            var commandHandler = this.serviceProvider.GetService(typeof(ICommandHandler<T>)) as ICommandHandler<T>;
            if (commandHandler == null)
            {
                throw new NullReferenceException(message: "No command handler found");
            }

            return await commandHandler.HandleAsync(command: command);
        }

        public async Task<TQueryResult> HandleAsync<TQuery, TQueryResult>(TQuery query)
            where TQuery : IQuery
            where TQueryResult : IQueryResult
        {
            var queryHandler = this.serviceProvider.GetServices<IQueryHandler<TQuery, TQueryResult>>().FirstOrDefault();
            if (queryHandler == null)
            {
                throw new NullReferenceException(message: "No query handler found");
            }

            return await queryHandler.HandleAsync(query: query);
        }

        #endregion Methods
    }
}
