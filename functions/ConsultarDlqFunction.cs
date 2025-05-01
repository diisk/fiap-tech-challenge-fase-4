using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Net;
using System.Text;

namespace AzureFunctions
{
    public class ConsultarDlqFunction
    {
        private readonly ILogger _logger;

        public ConsultarDlqFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LerDlqFunction>();
        }

        [Function("ConsultarDlqFunction")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "dlq")] HttpRequestData req, string nomeFila)
        {
            var statusCode = HttpStatusCode.OK;
            var content = " Nenhuma mensagem encontrada na DLQ.";
            var hostname = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";

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

                var result = await channel.BasicGetAsync(nomeFila, autoAck: true);
                if (result != null)
                {
                    var body = result.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    content = $"Fila {nomeFila}:{message}";
                }


            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.InternalServerError;
                content = "Falha ao buscar mensagens na DLQ.";
                _logger.LogError($" Falha ao buscar mensagens: {ex.Message}");
                _logger.LogError($" StackTrace: {ex.StackTrace}");
            }

            var responseData = req.CreateResponse(statusCode);
            await responseData.WriteStringAsync(content);

            return responseData;
        }
    }
}
