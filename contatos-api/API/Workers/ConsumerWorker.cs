using API.Consumers;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace API.Workers
{
    public class ConsumerWorker : BackgroundService
    {
        private readonly UsuarioAtualizadoEventConsumer usuarioAtualizadoEventConsumer;
        private readonly AreaAtualizadaEventConsumer areaAtualizadaEventConsumer;

        public ConsumerWorker(UsuarioAtualizadoEventConsumer usuarioAtualizadoEventConsumer, AreaAtualizadaEventConsumer areaAtualizadaEventConsumer)
        {
            this.usuarioAtualizadoEventConsumer = usuarioAtualizadoEventConsumer;
            this.areaAtualizadaEventConsumer = areaAtualizadaEventConsumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var enviroment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            if (enviroment.ToUpper() == "DEVELOPMENT") return;
            await usuarioAtualizadoEventConsumer.StartConsumingAsync();
            await areaAtualizadaEventConsumer.StartConsumingAsync();
        }
    }
}
