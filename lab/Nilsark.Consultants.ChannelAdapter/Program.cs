using System;
using Composable.DependencyInjection;
using Composable.Messaging.Buses;
using Microsoft.Azure.ServiceBus;

namespace Nilsark.Consultants.ChannelAdapter
{
    internal class Program
    {
        const string ServiceBusConnectionString = "Endpoint=sb://sflabs.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=cWLqCMjm/4SDre8cK5l5G5Bnp+Krd0IZo1bkponagnU=";
        const string QueueName = "test2";

        private static void Main(string[] args)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
            
            var host = EndpointHost.Production.Create(DependencyInjectionContainer.Create);
            new Bootstrapper(queueClient).RegisterWith(host);
            host.Start();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }       
    }
}