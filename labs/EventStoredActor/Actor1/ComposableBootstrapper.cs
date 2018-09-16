using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Common.ServiceFabric.Extensions;
using Composable.DependencyInjection;
using Composable.DependencyInjection.Persistence;
using Composable.Messaging.Buses;
using Composable.System.Configuration;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Actor1
{
    internal class ComposableBootstrapper
    {
        private readonly CommunicationListenerImpl _communicationListener;
        
        public ComposableBootstrapper(ServiceContext context, string endpointName)
        {
            _communicationListener = new CommunicationListenerImpl(context, endpointName);
        }

        public ICommunicationListener CommunicationListener => _communicationListener;
        public IEndpoint Endpoint => _communicationListener.Endpoint;

        private class CommunicationListenerImpl : ICommunicationListener
        {
            private readonly ServiceContext _context;
            private readonly string _endpointName;
            private IEndpointHost _host;

            public IEndpoint Endpoint { get; private set; }

            public CommunicationListenerImpl(ServiceContext context, string endpointName)
            {
                _context = context;
                _endpointName = endpointName;
            }

            public async Task<string> OpenAsync(CancellationToken cancellationToken)
            {
                // This sets up the container when secondary is promoted.
                _host = EndpointHost.Production.Create(DependencyInjectionContainer.Create);
                Endpoint = Registrar.RegisterWith(_host, _context);

                await _host.StartAsync();
                return _endpointName;

            }

            public Task CloseAsync(CancellationToken cancellationToken)
            {
                // This disposes the container when primary is demoted.
                _host.Dispose();
                return Task.CompletedTask;
            }

            public void Abort()
            {
                CloseAsync(CancellationToken.None);
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Registrar
        {
            public static IEndpoint RegisterWith(IEndpointHost host, ServiceContext context)
            {
                return host.RegisterClientEndpoint(setup: builder =>
                {
                    RegisterComponents(builder, context);
                    MapTypes(builder);
                });
            }

            private static void RegisterComponents(IEndpointBuilder builder, ServiceContext context)
            {
                builder.RegisterSqlServerEventStore()
                    .HandleAggregate<Domain.Foo, Domain.Events.IFooEvent>(builder.RegisterHandlers);

                builder.Container.Register(Singleton.For<IConfigurationParameterProvider>().CreatedBy(() => new ConfigurationParameterProvider(context)));
            }

            private static void MapTypes(IEndpointBuilder builder)
            {
                builder.TypeMapper
                    .Map<Domain.Foo>("c2ca53e0-ee6d-4725-8bf8-c13b680d0ac5")
                    .Map<Domain.Events.IBarAdded>("d893e1a7-fce9-4e13-8a56-013d7f488f77")
                    .Map<Domain.Events.IBarEvent>("673e8067-c89e-44d4-bb03-8be4d183f80c")
                    .Map<Domain.Events.IFooEvent>("8c39e985-470b-4523-a3c4-ae50fa9c6479")
                    .Map<Domain.Events.IFooNamePropertyUpdated>("03d1386c-be84-422c-8f5e-7b2c4d3edbb9")
                    .Map<Domain.Events.ICreatedEvent>("32829f32-4721-46eb-9a4e-ce253302ee9a")
                    .Map<Domain.Events.IRenamedEvent>("0f8ddcb1-7020-472a-b30f-330f26dca469")
                    .Map<Domain.Events.Implementation.CreatedEvent>("d1437586-b262-4d2b-a2b9-7e914d7d0d2d")
                    .Map<Domain.Events.Implementation.BarAddedEvent>("6f399893-41f6-43a9-b09f-6d5757f0f782")
                    .Map<Domain.Events.Implementation.BarEvent>("f90efde9-ab8d-4e04-825d-ef34f0d89bc1")
                    .Map<Domain.Events.Implementation.BarEvent.IdGetterSetter>("7bdbb866-6150-499f-9e6e-68c9899b21cc")
                    .Map<Domain.Events.Implementation.FooEvent>("a2950ca6-3dc2-48d9-a083-c2a799220aff")
                    .Map<Domain.Events.Implementation.RenamedEvent>("93e6f0c9-0ad6-4f54-ac86-eeea85584a47")
                    ;
            }
        }

        private class ConfigurationParameterProvider : SettingsProviderBase, IConfigurationParameterProvider
        {
            public string GetString(string parameterName, string valueIfMissing = null) => this[parameterName];

            public ConfigurationParameterProvider(ServiceContext context) : base(context) => Configure().FromSettings("Composable");
        }
    }
}