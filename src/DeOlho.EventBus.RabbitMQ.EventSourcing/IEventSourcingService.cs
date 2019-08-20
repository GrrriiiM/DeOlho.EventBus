using System.Threading;
using System.Threading.Tasks;
using DeOlho.EventBus.Message;
using Microsoft.EntityFrameworkCore.Storage;

namespace DeOlho.EventBus.RabbitMQ.EventSourcing
{
    public interface IEventSourcingService
    {
        Task SaveEventLogAsync(EventBusMessage message, IDbContextTransaction transaction, CancellationToken cancellationToken = default(CancellationToken));
        Task MigrateAsync(CancellationToken cancellationToken);
    }
}