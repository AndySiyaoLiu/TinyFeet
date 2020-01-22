namespace TinyFeet.Runtime
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Loader;
    using System.Collections.Generic;
    using Microsoft.Extensions.DependencyModel;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using TinyFeet.Interfaces.Query;
    using TinyFeet.Interfaces.Command;

    class Runtime
    {
        private static HashSet<string> ReferenceAssemblies { get; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "TinyFeet"
        };
        private readonly IServiceCollection serviceCollection;
        private readonly IServiceProvider serviceProvider;

        #region Constructor

        public Runtime(IServiceCollection services, IServiceProvider serviceProvider)
        {
            this.serviceCollection = services;
            this.serviceProvider = serviceProvider;
        }

        #endregion Constructor

        #region Methods

        public void Initialize()
        {
            var assemblyDictionary = new Dictionary<string, Assembly>();

            // TODO: Workaround for known bug: https://github.com/dotnet/cli/issues/4037
            // If this is removed, the test's will not pass, as the handlers are not detected
            // which of course will prevent users of this library from testing as well
#if (NETFULL)
            var applicationDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            applicationDomainAssemblies.ForEach(x => assemblyDictionary.TryAdd(x.GetName().Name, x));
#endif

            var discoveredLibraries = this.LoadReferencingLibraries();
            foreach (var discoveredAssembly in discoveredLibraries)
            {
                var assemblyName = discoveredAssembly.GetName();

                if (ReferenceAssemblies.Contains(assemblyName.Name))
                {
                    continue;
                }

                if (!assemblyDictionary.ContainsKey(assemblyName.Name))
                {
                    assemblyDictionary.Add(assemblyName.Name, discoveredAssembly);
                }
            }

            var loadedAssemblies = this.LoadFromLocations();
            if (loadedAssemblies != null && loadedAssemblies.Length > 0)
            {
                foreach (var loadedAssembly in loadedAssemblies)
                {
                    assemblyDictionary.Add(loadedAssembly.GetName().Name, loadedAssembly);
                }
            }

            var assembliesToIgnore = new List<string>();
            foreach (var assembly in assemblyDictionary)
            {
                if (assembly.Key.StartsWith("Microsoft.") ||
                    assembly.Key.StartsWith("System.") ||
                    assembly.Key.StartsWith("Newtonsoft.") ||
                    assembly.Key.StartsWith("NuGet.") ||
                    assembly.Key.StartsWith("xunit.") ||
                    assembly.Key.StartsWith("Serilog.") ||
                    assembly.Key.StartsWith("Newtonsoft."))
                {
                    assembliesToIgnore.Add(assembly.Key);
                }
            }

            foreach (var ignoredAssembly in assembliesToIgnore)
            {
                if (assemblyDictionary.ContainsKey(ignoredAssembly))
                {
                    assemblyDictionary.Remove(ignoredAssembly);
                }
            }

            var finalAssemblyList = assemblyDictionary.Select(x => x.Value).ToArray();
            foreach (var assembly in finalAssemblyList)
            {
                var types = assembly.DefinedTypes.ToArray();
                foreach (var type in types.Where(x => x.ImplementedInterfaces.Any(y => y.GenericTypeArguments.Any())))
                {
                    if (type.IsAbstract)
                    {
                        continue;
                    }
                    var isCommandHandler = type.AsType().IsGenericTypeOf(typeof(ICommandHandler<ICommand>));
                    var isQueryHandler = type.AsType().IsGenericTypeOf(typeof(IQueryHandler<IQuery, IQueryResult>));

                    if (isCommandHandler)
                    {
                        var commandHandlerInterfaces = type.ImplementedInterfaces.Where(x => x.IsGenericTypeOf(typeof(ICommandHandler<ICommand>)));
                        foreach (var commandHandlerInterface in commandHandlerInterfaces)
                        {
                            var genericArguments = commandHandlerInterface.GetGenericArguments();

                            if (genericArguments != null && genericArguments.Count() == 1)
                            {
                            }

                            serviceCollection.TryAddTransient(commandHandlerInterface, type.AsType());
                        }
                    }

                    if (isQueryHandler)
                    {
                        var queryHandlerInterfaces = type.ImplementedInterfaces.Where(x => x.IsGenericTypeOf(typeof(IQueryHandler<IQuery, IQueryResult>)));
                        foreach (var queryHandlerInterface in queryHandlerInterfaces)
                        {
                            var genericArguments = queryHandlerInterface.GetGenericArguments();

                            if (genericArguments != null && genericArguments.Count() == 2)
                            {
                            }

                            serviceCollection.TryAddTransient(queryHandlerInterface, type.AsType());
                        }
                    }
                }
            }
        }

        #endregion Methods

        #region Private Methods

        private Assembly[] LoadFromLocations()
        {
            var loadedAssemblyList = new List<Assembly>();
            var currentAssemblyLocation = Assembly.GetExecutingAssembly().Location;
            if (Directory.Exists(currentAssemblyLocation))
            {
                var files = Directory.GetFiles(currentAssemblyLocation, "*.dll");
                foreach (var file in files)
                {
                    try
                    {
#if !(NETFULL)
                        var loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
#else
                        var loadedAssembly = Assembly.LoadFile(file);
#endif
                        loadedAssemblyList.Add(loadedAssembly);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }

            return loadedAssemblyList.ToArray();
        }

        private Assembly[] LoadReferencingLibraries()
        {
            var assemblies = this.GetReferencingLibraries();

            return assemblies == null ? new Assembly[] { } : assemblies.ToArray();
        }

        private IEnumerable<Assembly> GetReferencingLibraries()
        {
            try
            {
                var assemblies = new List<Assembly>();

                var dependencies = DependencyContext.Default.RuntimeLibraries;

                foreach (var library in dependencies)
                {
                    if (IsCandidateLibrary(library))
                    {
                        var assembly = Assembly.Load(new AssemblyName(library.Name));
                        assemblies.Add(assembly);
                    }
                }

                return assemblies;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private bool IsCandidateLibrary(RuntimeLibrary library)
        {
            return ReferenceAssemblies.Contains(library.Name) || library.Dependencies.Any(x => ReferenceAssemblies.Any(y => y.StartsWith(x.Name)));
        }

        #endregion Private Methods
    }
}
