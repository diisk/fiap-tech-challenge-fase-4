using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace AzureFunctions
{
    public class LerDlqFunction
    {
        private readonly ILogger _logger;

        public LerDlqFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LerDlqFunction>();
        }

        [Function("LerDlqFunction")]
        public async Task Run([TimerTrigger("*/30 * * * * *")] TimerInfo myTimer)
        {
            var hostname = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
            var dlqName1 = "UsuarioAtualizadoDlqQueue";
            var dlqName2 = "AreaAtualizadaDlqQueue";

            _logger.LogInformation($"[DEBUG] RABBITMQ_HOST = {hostname}");
            _logger.LogInformation($">>> Azure Function disparada em: {DateTime.UtcNow:MM/dd/yyyy HH:mm:ss}");

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = hostname
                };

                _logger.LogInformation(" Tentando criar conexão com RabbitMQ...");
                await using var connection = await factory.CreateConnectionAsync();
                _logger.LogInformation(" Conexão criada com sucesso.");

                var channel = await connection.CreateChannelAsync();
                _logger.LogInformation(" Canal criado com sucesso.");

                _logger.LogInformation(" Procurando mensagens...");


                var mensagemEncontrada = false;

                var result = await channel.BasicGetAsync(dlqName1, autoAck: true);
                if (result != null)
                {
                    var body = result.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation($" Mensagem da ${dlqName1} recebida: {message}");
                }

                result = await channel.BasicGetAsync(dlqName2, autoAck: true);
                if (result != null)
                {
                    var body = result.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation($" Mensagem da ${dlqName2} recebida: {message}");
                }

                if (!mensagemEncontrada)
                    _logger.LogInformation(" Nenhuma mensagem encontrada na DLQ.");

            }
            catch (Exception ex)
            {
                _logger.LogError($" Falha ao buscar mensagens: {ex.Message}");
                _logger.LogError($" StackTrace: {ex.StackTrace}");
            }
        }
    }
}
