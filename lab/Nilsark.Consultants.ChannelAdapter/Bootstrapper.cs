using System;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Composable.Messaging.Buses;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Nilsark.Consultants.ChannelAdapter.Contract;
using Nilsark.Consultants.Shared.Domain.Events;

namespace Nilsark.Consultants.ChannelAdapter
{
    public class Bootstrapper
    {
        private readonly IQueueClient _queueClient;

        public Bootstrapper(IQueueClient queueClient)
        {
            _queueClient = queueClient;
        }
        public IEndpoint RegisterWith(IEndpointHost host)
        {
            return host.RegisterEndpoint("Consultants.ChannelAdapter",
                new EndpointId(Guid.Parse("C6FE08D1-81EB-4F48-83E3-7D934A7DABA5")),
                builder =>
                {
                    Shared.TypeMapper.MapTypes(builder.TypeMapper);
                    RegisterHandlers(builder);
                });
        }
        
        private void RegisterHandlers(IEndpointBuilder builder)
        {
            builder.RegisterHandlers
                .ForEvent<IConsultantEnrolledEvent>(async e => await PushAsync(e));
        }
        
        private async Task PushAsync(IConsultantEvent e)
        {
            try
            {
                using (var s = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                {
                    var json = JsonConvert.SerializeObject(new ConsultantUpdated { ConsultantId = e.AggregateId });
                    var message = new Message(Encoding.UTF8.GetBytes(json)) {ContentType = "application/json"};

                    await _queueClient.SendAsync(message);
                    
                    s.Complete();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
                throw;
            }
        }
    }
}