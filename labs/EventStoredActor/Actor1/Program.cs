using System;
using System.Fabric;
using System.Threading;
using Common.ServiceFabric.Extensions;
using Composable.DependencyInjection;
using Composable.DependencyInjection.Persistence;
using Composable.Messaging.Buses;
using Composable.System.Configuration;
using Composable.System.Data.SqlClient;
using Domain;
using Domain.Events;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Actor1
{
    internal static class Program
    {
        private static void Main()
        {
            try
            {
                var host = EndpointHost.Production.Create((DependencyInjectionContainer.Create));
                var composableEndpoint = new DomainBootstrapper().RegisterWith(host);
                host.Start();

                ActorRuntime.RegisterActorAsync<Actor1>(
                   (context, actorType) => new Actor1ActorService(context, actorType, composableEndpoint)).GetAwaiter().GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }

        public class DomainBootstrapper
        {
            public IEndpoint RegisterWith(IEndpointHost host)
            {
                return host.RegisterEndpoint(name: "Foo",
                    id: new EndpointId(Guid.Parse(input: "1A1BE9C8-C8F6-4E38-ABFB-F101E5EDB00D")),
                    setup: RegisterDomainComponents,
                    isPureClientEndpoint: true);
            }

            static void RegisterDomainComponents(IEndpointBuilder builder)
            {
                builder.TypeMapper
                    .Map<Domain.Foo>("c2ca53e0-ee6d-4725-8bf8-c13b680d0ac5")
                    .Map<Domain.Events.IBarAdded>("d893e1a7-fce9-4e13-8a56-013d7f488f77")
                    .Map<Domain.Events.IBarEvent>("673e8067-c89e-44d4-bb03-8be4d183f80c")
                    .Map<Domain.Events.IFooEvent>("8c39e985-470b-4523-a3c4-ae50fa9c6479")
                    .Map<Domain.Events.IFooNamePropertyUpdated>("03d1386c-be84-422c-8f5e-7b2c4d3edbb9")
                    .Map<Domain.Events.Implementation.BarAdded>("6f399893-41f6-43a9-b09f-6d5757f0f782")
                    .Map<Domain.Events.Implementation.BarEvent>("f90efde9-ab8d-4e04-825d-ef34f0d89bc1")
                    .Map<Domain.Events.Implementation.BarEvent.IdGetterSetter>("7bdbb866-6150-499f-9e6e-68c9899b21cc")
                    .Map<Domain.Events.Implementation.FooEvent>("a2950ca6-3dc2-48d9-a083-c2a799220aff")
                    .Map<Domain.Events.Implementation.NameSet>("93e6f0c9-0ad6-4f54-ac86-eeea85584a47")
                    .Map<Domain.Events.INameSetEvent>("0f8ddcb1-7020-472a-b30f-330f26dca469");

                builder.Container.RegisterSqlServerEventStore(builder.Configuration.ConnectionStringName)
                    .HandleAggregate<Foo, IFooEvent>(builder.RegisterHandlers);

                builder.Container.RegisterSqlServerDocumentDb(builder.Configuration.ConnectionStringName);
            }
        }
    }
}
