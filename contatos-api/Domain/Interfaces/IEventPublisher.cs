using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(string exchangeName, string queueName, T message, CancellationToken cancellationToken = default);
    }
}
