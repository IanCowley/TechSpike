using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyModel;

namespace MicroCQRS
{
    internal class CommandHandlerDiscovery
    {
        public static Dictionary<Type, CommandHandlerMap> GetAllCommandHandlers(
            IAggregateRepository aggregateRepository, 
            Func<Type, ICommandHandler> activator = null)
        {
            try
            {
                if (activator == null)
                {
                    activator = DefaultActivator;
                }

                return GetHandlers()
                    .ToDictionary(
                        map => map.Key,
                        map => CreateCommandHandlerMap(activator, map.Key, map.Value, aggregateRepository));
            }
            catch (ArgumentException ex)
            {
                throw new MicroCQRSConfigurationException($"There were duplicate command handlers found {ex}");
            }
        }

        static ICommandHandler DefaultActivator(Type type)
        {
            return (ICommandHandler) Activator.CreateInstance(type);
        }

        static Dictionary<Type, Type> GetHandlers()
        {
            var handlers = FindAllCommandHandlerTypes()
                .Select(x => new
                {
                    type = x,
                    interfaces = x.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleCommand<>))
                })
                .Where(x => x.interfaces.Any());

            var handlerMappings = new Dictionary<Type, Type>();

            foreach (var handler in handlers)
            {
                foreach (var @interface in handler.interfaces)
                {
                    var commandType = @interface.GenericTypeArguments[0];

                    if (handlerMappings.ContainsKey(commandType))
                    {
                        throw new MicroCQRSConfigurationException($"Duplicate handler registration {handlerMappings[commandType]} and {handler.type}");
                    }

                    handlerMappings.Add(commandType, handler.type);
                }
            }

            return handlerMappings;
        }

        static IEnumerable<TypeInfo> FindAllCommandHandlerTypes()
        {
            var runtimeId = RuntimeEnvironment.GetRuntimeIdentifier();
            var assemblyNames = DependencyContext.Default.GetRuntimeAssemblyNames(runtimeId);
            var assemblies = assemblyNames.Select(x => Assembly.Load(new AssemblyName(x.Name)));

            return assemblies
                .SelectMany(x => x.GetTypes())
                .Select(x => x.GetTypeInfo())
                .Where(t => t.GetInterfaces().Contains(typeof(ICommandHandler)));
        }

        static CommandHandlerMap CreateCommandHandlerMap(
            Func<Type, ICommandHandler> activator, 
            Type commandType, 
            Type commandHandlerType, 
            IAggregateRepository aggregateRepository)
        {
            ICommandHandler instance = activator(commandHandlerType);
            instance.AggregateRepository = aggregateRepository;
            var handlesMethod = instance.GetType().GetMethod("HandlesAsync", new[] {commandType});
            return new CommandHandlerMap(instance, handlesMethod);
        }
    }
}