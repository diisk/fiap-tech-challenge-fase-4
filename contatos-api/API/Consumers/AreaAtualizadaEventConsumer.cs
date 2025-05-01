using Domain.Entities;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace API.Consumers
{
    public class AreaAtualizadaEventConsumer
    {
        private const string QUEUE_NAME = "AreaAtualizadaQueue";
        private const string EXCHANGE_NAME = "AreaAtualizadaExchange";
        private const string DLQ_EXCHANGE_NAME = "AreaAtualizadaDlqExchange";
        private const string DLQ_QUEUE_NAME = "AreaAtualizadaDlqQueue";
        private readonly IServiceScopeFactory scopeFactory;

        public AreaAtualizadaEventConsumer(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        public async Task StartConsumingAsync()
        {
            var hostname = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
            var factory = new ConnectionFactory() { HostName = hostname };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(exchange: DLQ_EXCHANGE_NAME, type: "fanout");
            await channel.QueueDeclareAsync(queue: DLQ_QUEUE_NAME, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await channel.QueueBindAsync(queue: DLQ_QUEUE_NAME, exchange: DLQ_EXCHANGE_NAME, routingKey: "");

            var arguments = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", DLQ_EXCHANGE_NAME },
                { "x-message-ttl", 20000 },
                { "x-max-length", 6 }
            };

            await channel.ExchangeDeclareAsync(exchange: EXCHANGE_NAME, type: "fanout");

            await channel.QueueDeclareAsync(queue: QUEUE_NAME,
                                            durable: true,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

            await channel.QueueBindAsync(queue: QUEUE_NAME, exchange: EXCHANGE_NAME, routingKey: "");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var area = JsonSerializer.Deserialize<Area>(message);

                    if (area != null)
                    {
                        using var scope = scopeFactory.CreateScope();
                        var readDbContext = scope.ServiceProvider.GetRequiredService<OnlyReadDbContext>();
                        var writeDbContext = scope.ServiceProvider.GetRequiredService<OnlyWriteDbContext>();

                        var existingArea = await readDbContext.AreaSet.AsNoTracking()
                            .FirstOrDefaultAsync(a => a.Codigo == area.Codigo);

                        if (existingArea == null)
                        {
                            await writeDbContext.AreaSet.AddAsync(area);
                        }
                        else
                        {
                            var trackedArea = await writeDbContext.AreaSet.FirstOrDefaultAsync(a => a.Codigo == area.Codigo);
                            if (trackedArea != null)
                            {
                                writeDbContext.Entry(trackedArea).CurrentValues.SetValues(area);
                            }
                        }

                        await writeDbContext.SaveChangesAsync();
                    }
                    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                    await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }

            };

            await channel.BasicConsumeAsync(queue: QUEUE_NAME,
                                            autoAck: false,
                                            consumer: consumer);
        }
    }
}