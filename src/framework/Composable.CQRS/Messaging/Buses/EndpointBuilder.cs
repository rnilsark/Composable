﻿using System;
using Composable.Contracts;
using Composable.DependencyInjection;
using Composable.GenericAbstractions.Time;
using Composable.Messaging.Buses.Implementation;
using Composable.Persistence.EventStore;
using Composable.Persistence.EventStore.Aggregates;
using Composable.Refactoring.Naming;
using Composable.Serialization;
using Composable.System;
using Composable.System.Configuration;
using Composable.System.Data.SqlClient;
using Composable.System.Threading;

// ReSharper disable ImplicitlyCapturedClosure it is very much intentional :)

namespace Composable.Messaging.Buses
{
    class EndpointBuilder : IEndpointBuilder
    {
        readonly IDependencyInjectionContainer _container;
        readonly TypeMapper _typeMapper;
        bool _builtSuccessfully;
        readonly ISqlConnectionProviderSource _connectionProvider;

        public IDependencyInjectionContainer Container => _container;
        public ITypeMappingRegistar TypeMapper => _typeMapper;
        public EndpointConfiguration Configuration { get; }

        public MessageHandlerRegistrarWithDependencyInjectionSupport RegisterHandlers { get; }

        public IEndpoint Build()
        {
            SetupInternalTypeMap();
            BusApi.Internal.RegisterHandlers(RegisterHandlers);
            var endpoint = new Endpoint(_container.CreateServiceLocator(), Configuration);
            _builtSuccessfully = true;
            return endpoint;
        }

        void SetupInternalTypeMap()
        {
            EventStoreApi.MapTypes(TypeMapper);
            BusApi.MapTypes(TypeMapper);
        }

        public EndpointBuilder(IEndpointHost host, IGlobalBusStateTracker globalStateTracker, IDependencyInjectionContainer container, EndpointConfiguration configuration)
        {
            _container = container;

            //todo: Find cleaner way of doing this.
            if(host is IEndpointRegistry endpointRegistry)
            {
                _container.Register(Singleton.For<IEndpointRegistry>().Instance(endpointRegistry));
            } else
            {
                _container.Register(Singleton.For<IEndpointRegistry>().CreatedBy((IConfigurationParameterProvider configurationParameterProvider) => new AppConfigEndpointRegistry(configurationParameterProvider)));
            }

            Configuration = configuration;

            _connectionProvider = container.RunMode.IsTesting
                                         ? (ISqlConnectionProviderSource)new SqlServerDatabasePoolSqlConnectionProviderSource()
                                         : new AppConfigSqlConnectionProviderSource();

            var endpointSqlConnection = new LazySqlServerConnectionProvider(
                () => _container.CreateServiceLocator().Resolve<ISqlConnectionProviderSource>().GetConnectionProvider(Configuration.ConnectionStringName).ConnectionString);

            _typeMapper = new TypeMapper(endpointSqlConnection);

            var registry = new MessageHandlerRegistry(_typeMapper);
            RegisterHandlers = new MessageHandlerRegistrarWithDependencyInjectionSupport(registry, new OptimizedLazy<IServiceLocator>(() => _container.CreateServiceLocator()));

            _container.Register(
                Singleton.For<IConfigurationParameterProvider>().CreatedBy(() => new AppConfigConfigurationParameterProvider()),
                Singleton.For<ISqlConnectionProviderSource>().CreatedBy(() => _connectionProvider).DelegateToParentServiceLocatorWhenCloning(),
                Singleton.For<ITypeMappingRegistar, ITypeMapper, TypeMapper>().CreatedBy(() => _typeMapper).DelegateToParentServiceLocatorWhenCloning(),
                Singleton.For<ITaskRunner>().CreatedBy(() => new TaskRunner()),
                Singleton.For<EndpointId>().CreatedBy(() => configuration.Id),
                Singleton.For<EndpointConfiguration>().CreatedBy(() => Configuration),
                Singleton.For<IInterprocessTransport>().CreatedBy((IUtcTimeTimeSource timeSource, ITaskRunner taskRunner, IRemotableMessageSerializer serializer) => new InterprocessTransport(globalStateTracker, timeSource, endpointSqlConnection, _typeMapper, configuration, taskRunner, serializer)),
                Singleton.For<IGlobalBusStateTracker>().CreatedBy(() => globalStateTracker),
                Singleton.For<IMessageHandlerRegistry, IMessageHandlerRegistrar, MessageHandlerRegistry>().CreatedBy(() => registry),
                Singleton.For<IEventStoreSerializer>().CreatedBy(() => new EventStoreSerializer(_typeMapper)),
                Singleton.For<IDocumentDbSerializer>().CreatedBy(() => new DocumentDbSerializer(_typeMapper)),
                Singleton.For<IRemotableMessageSerializer>().CreatedBy(() => new RemotableMessageSerializer(_typeMapper)),
                Singleton.For<IEventstoreEventPublisher>().CreatedBy((IInterprocessTransport interprocessTransport, IMessageHandlerRegistry messageHandlerRegistry) => new EventstoreEventPublisher(interprocessTransport, messageHandlerRegistry)),
                Scoped.For<IRemoteApiNavigatorSession>().CreatedBy((IInterprocessTransport interprocessTransport) => new RemoteApiBrowserSession(interprocessTransport)));

            if(configuration.HasMessageHandlers)
            {
                _container.Register(
                    Singleton.For<IInbox>().CreatedBy((IServiceLocator serviceLocator, EndpointConfiguration endpointConfiguration, ITaskRunner taskRunner, IRemotableMessageSerializer serializer) => new Inbox(serviceLocator, globalStateTracker, registry, endpointConfiguration, endpointSqlConnection, _typeMapper, taskRunner, serializer)),
                    Singleton.For<CommandScheduler>().CreatedBy((IInterprocessTransport transport, IUtcTimeTimeSource timeSource) => new CommandScheduler(transport, timeSource)),
                    Singleton.For<IAggregateTypeValidator>().CreatedBy(() => new AggregateTypeValidator(_typeMapper)),
                    Scoped.For<IServiceBusSession, ILocalApiNavigatorSession>().CreatedBy((IInterprocessTransport interprocessTransport, CommandScheduler commandScheduler, IMessageHandlerRegistry messageHandlerRegistry, IRemoteApiNavigatorSession remoteNavigator) => new ApiNavigatorSession(interprocessTransport, commandScheduler, messageHandlerRegistry, remoteNavigator))
                );
            }

            if(_container.RunMode == RunMode.Production)
            {
                _container.Register(Singleton.For<IUtcTimeTimeSource>().CreatedBy(() => new DateTimeNowTimeSource()).DelegateToParentServiceLocatorWhenCloning());
            } else
            {
                _container.Register(Singleton.For<IUtcTimeTimeSource, TestingTimeSource>().CreatedBy(() => TestingTimeSource.FollowingSystemClock).DelegateToParentServiceLocatorWhenCloning());
            }
        }

        bool _disposed;
        public void Dispose()
        {
            if(!_disposed)
            {
                _disposed = true;
                if(!_builtSuccessfully)
                {
                    (_connectionProvider as IDisposable)?.Dispose();
                    _container?.Dispose();
                }
            }
        }
    }
}
