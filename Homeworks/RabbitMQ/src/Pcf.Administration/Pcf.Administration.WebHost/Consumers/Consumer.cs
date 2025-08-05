using System.Threading.Tasks;
using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text;
using Pcf.Administration.WebHost.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.Extensions.Options;
using Pcf.Administration.Core.Settings;

namespace Pcf.Administration.WebHost.Consumers
{
    public class Consumer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RmqSettings _rmqSettings;
        public Consumer(IServiceProvider serviceProvider, IOptions<RmqSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _rmqSettings = settings.Value;
        }
        public async Task Register(IChannel channel, string exchangeName, string queueName, string routingKey)
        {
            using var scope = _serviceProvider.CreateScope();

            await channel.BasicQosAsync(0, 10, false);
            await channel.QueueDeclareAsync(queueName, false, false, false, null);
            await channel.QueueBindAsync(queueName, exchangeName, routingKey, null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, e) =>
            {
                //throw new Exception("Error has occured");
                var body = e.Body;
                var message = JsonSerializer.Deserialize<Guid>(Encoding.UTF8.GetString(body.ToArray()));
                Console.WriteLine($"{DateTime.Now} Received message: {message}");
                await channel.BasicAckAsync(e.DeliveryTag, false);

                await _serviceProvider.GetRequiredService<EmployeeService>().UpdateAppliedPromocodesAsync(message);
            };

            await channel.BasicConsumeAsync(queueName, false, consumer);
            Console.WriteLine($"Subscribed to the queue with key {routingKey} (exchange name: {exchangeName})");
            Console.ReadLine();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var connection = await GetRabbitConnection();
            int consumerNumber = 1;
            var channel = await connection.CreateChannelAsync();
            await Register(channel, $"exchange.topic", $"queue.topic_{consumerNumber}", $"PartnerManagerPromoCode");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
           
        }

        private async Task<IConnection> GetRabbitConnection()
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = _rmqSettings.Host,
                VirtualHost = _rmqSettings.VHost,
                UserName = _rmqSettings.Login,
                Password = _rmqSettings.Password,
            };
            IConnection conn = await factory.CreateConnectionAsync();
            return conn;
        }
    }
}
