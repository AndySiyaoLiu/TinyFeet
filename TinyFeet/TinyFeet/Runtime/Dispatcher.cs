namespace TinyFeet.Runtime
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.DependencyInjection;

    using TinyFeet.Interfaces.Query;
    using TinyFeet.Interfaces.Command;
    
    public class Dispatcher : IDispatcher
    {
        #region Fields

        private readonly IServiceProvider serviceProvider;
        private readonly ILogger logger;

        #endregion Fields

        #region Constructor

        public Dispatcher(IServiceProvider serviceProvider, ILogger logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
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
            this.logger.LogInformation(message: $"Found command handler: {commandHandler.GetType().Name}");

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
            this.logger.LogInformation(message: $"Found query handler: {queryHandler.GetType().Name}");

            return await queryHandler.HandleAsync(query: query);
        }

        #endregion Methods
    }
}
