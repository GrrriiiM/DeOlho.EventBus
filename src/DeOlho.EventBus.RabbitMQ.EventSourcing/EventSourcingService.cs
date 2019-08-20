using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using DeOlho.EventBus.Message;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace DeOlho.EventBus.RabbitMQ.EventSourcing
{
    public class EventSourcingService : IEventSourcingService
    {
        readonly EventSourcingDbContext _eventSourcingDbContext;

        public EventSourcingService(
            DbConnection dbConnection)
        {
            _eventSourcingDbContext = new EventSourcingDbContext(
                new DbContextOptionsBuilder<EventSourcingDbContext>()
                    .UseMySql(dbConnection)
                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .Options);
        }

        public Task SaveEventLogAsync(EventBusMessage message, IDbContextTransaction transaction, CancellationToken cancellationToken = default(CancellationToken))
        {
            _eventSourcingDbContext.Database.UseTransaction(transaction.GetDbTransaction());
            _eventSourcingDbContext.EventLogs.Add(new EventLog(message));
            return _eventSourcingDbContext.SaveChangesAsync(cancellationToken);
        }

        public Task MigrateAsync(CancellationToken cancellationToken)
        {
            return _eventSourcingDbContext.Database.MigrateAsync(cancellationToken);
        }
    }
}