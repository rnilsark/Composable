using System;
using Composable.DependencyInjection;
using Composable.Messaging.Buses;

namespace Nilsark.Consultants.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var host = EndpointHost.Production.Create(DependencyInjectionContainer.Create);
            new Bootstrapper().RegisterWith(host);
            host.Start();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}