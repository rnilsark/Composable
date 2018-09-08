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
                ActorRuntime.RegisterActorAsync<Actor1>(
                   (context, actorType) => new Actor1ActorService(context, actorType)).GetAwaiter().GetResult();

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
