namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using TinyFeet.Runtime;

    public static class ServiceCollectionExtensions
    {
        public static void AddRuntime(this IServiceCollection serviceCollection)
        {
            var provider = serviceCollection.BuildServiceProvider();

            var runtime = ActivatorUtilities.CreateInstance<Runtime>(provider, serviceCollection);
            serviceCollection.TryAddSingleton(runtime);

            runtime.Initialize();
            serviceCollection.TryAddTransient<IDispatcher, Dispatcher>();
        }
    }
}
