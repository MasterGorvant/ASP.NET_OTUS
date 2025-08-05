using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Pcf.ReceivingFromPartner.Integration.Producers
{
    public class Producer
    {
        private readonly string _exchangeName;
        private readonly string _routingKey;
        private readonly IChannel _channel;

        public Producer(string exchangeType, string exchangeName,
            string routingKey, IChannel channel)
        {
            _exchangeName = exchangeName;
            _routingKey = routingKey;
            _channel = channel;
            _channel.ExchangeDeclareAsync(_exchangeName, exchangeType);
        }

        public async Task Produce(object msg)
        {
            var messageSerialized = JsonSerializer.Serialize(msg);
            var body = Encoding.UTF8.GetBytes(messageSerialized);

            await _channel.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: _routingKey,
                body: body);

            Console.WriteLine($"Message {messageSerialized} is sent into exchange: {_exchangeName}");
        }
    }
}