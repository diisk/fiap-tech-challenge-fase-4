using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace AzureFunctions
{
    public class ConsultarContatosFunction
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public ConsultarContatosFunction(ILoggerFactory loggerFactory, HttpClient httpClient)
        {
            _logger = loggerFactory.CreateLogger<LerDlqFunction>();
            _httpClient = httpClient;
        }

        [Function("ConsultarContatosFunction")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "contatos")] HttpRequestData req, int? codigoArea)
        {
            var url = Environment.GetEnvironmentVariable("CONTATOS_URL") ?? "http://localhost:8000/api/contatos";
            if (codigoArea.HasValue)
                url += "?codigoArea=" + codigoArea.Value;

            var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJJZGVudGlmaWNhZG9yIjoiMVNxNEdkcUdJTkJaZmtjQk1leFFra2tWL0M4RTMzemVrU0Fva2x3K0VUWT0iLCJuYmYiOjE3NDIxMzE0MTcsImV4cCI6MzMxOTk2ODIxNywiaWF0IjoxNzQyMTMxNDE3fQ.Vb_YTdBvGR6oBNeqkWoPqakPAaBx9vaGrd3mi5tZvXI"; // Substitua pelo seu token Bearer

            _logger.LogInformation($"Making request to: {url}");

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(requestMessage);
            var content = await response.Content.ReadAsStringAsync();

            var responseData = req.CreateResponse(response.StatusCode);
            await responseData.WriteStringAsync(content);

            return responseData;
        }
    }
}
