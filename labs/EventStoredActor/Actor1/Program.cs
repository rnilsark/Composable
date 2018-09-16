using System;
using System.Threading;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Actor1
{
    internal static class Program
    {
        private static void Main()
        {
            try
            {
                ActorRuntime.RegisterActorAsync<Actor1V2>(
                   actorServiceFactory: (context, actorType) => new Actor1V2ActorService(
                       context, 
                       actorType, 
                       new ComposableBootstrapper(context, "Foo"))).GetAwaiter().GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
