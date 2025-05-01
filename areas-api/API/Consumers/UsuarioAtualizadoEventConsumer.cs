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
    public class UsuarioAtualizadoEventConsumer
    {
        private const string QUEUE_NAME = "UsuarioAtualizadoAreaQueue";
        private const string EXCHANGE_NAME = "UsuarioAtualizadoExchange";
        private const string DLQ_EXCHANGE_NAME = "UsuarioAtualizadoDlqExchange";
        private const string DLQ_QUEUE_NAME = "UsuarioAtualizadoDlqQueue";
        private readonly IServiceScopeFactory _scopeFactory;

        public UsuarioAtualizadoEventConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
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
                                            arguments: arguments!);

            await channel.QueueBindAsync(queue: QUEUE_NAME, exchange: EXCHANGE_NAME, routingKey: "");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var usuario = JsonSerializer.Deserialize<Usuario>(message);

                    if (usuario != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var readDbContext = scope.ServiceProvider.GetRequiredService<OnlyReadDbContext>();
                        var writeDbContext = scope.ServiceProvider.GetRequiredService<OnlyWriteDbContext>();

                        var existingUsuario = await readDbContext.UsuarioSet.AsNoTracking()
                            .FirstOrDefaultAsync(a => a.ID == usuario.ID);

                        if (existingUsuario == null)
                        {
                            await writeDbContext.UsuarioSet.AddAsync(usuario);
                        }
                        else
                        {
                            var trackedUsuario = await writeDbContext.UsuarioSet.FirstOrDefaultAsync(a => a.ID == usuario.ID);
                            if (trackedUsuario != null)
                            {
                                writeDbContext.Entry(trackedUsuario).CurrentValues.SetValues(usuario);
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