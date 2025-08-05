using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Pcf.ReceivingFromPartner.Core.Settings;
using Pcf.ReceivingFromPartner.Integration.Producers;
using RabbitMQ.Client;

namespace Pcf.ReceivingFromPartner.Integration
{
    public class NotificationService
    {
        private readonly RmqSettings _rmqSettings;

        private Producer _producer;
        public NotificationService(
            IOptions<RmqSettings> rmqSettings
            )
        {
            _rmqSettings = rmqSettings.Value;
            Task.Factory.StartNew(async () =>
            {
                var connection = await GetRabbitConnection();
                var channel = await connection.CreateChannelAsync();
                _producer = new Producer("topic", "exchange.topic", "PartnerManagerPromoCode", channel);
            }).Wait();
            
        }

        private async Task<IConnection> GetRabbitConnection()
        {
           
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = _rmqSettings.Host,
                VirtualHost = _rmqSettings.VHost,
                UserName = _rmqSettings.Login,
                Password = _rmqSettings.Password
            };

            return await factory.CreateConnectionAsync();
        }

        public async Task Notify(object message, string topic)
        {
            await _producer.Produce(message);
        }

    }
}
