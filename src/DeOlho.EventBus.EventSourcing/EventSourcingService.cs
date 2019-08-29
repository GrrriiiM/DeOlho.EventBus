using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using DeOlho.EventBus.Message;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace DeOlho.EventBus.EventSourcing
{
    public class EventSourcingService : IEventSourcingService
    {
        readonly EventSourcingDbContext _eventSourcingDbContext;
        readonly DbContext _dbContext;

        public EventSourcingService(
            DbContext dbContext)
        {
            _dbContext = dbContext;
            _eventSourcingDbContext = new EventSourcingDbContext(
                new DbContextOptionsBuilder<EventSourcingDbContext>()
                    .UseMySql(_dbContext.Database.GetDbConnection())
                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .Options);
        }

        public async Task SaveEventLogAsync(EventBusMessage message, IDbContextTransaction transaction, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_eventSourcingDbContext.Database.CurrentTransaction == null 
                ||_eventSourcingDbContext.Database.CurrentTransaction.GetDbTransaction() != transaction.GetDbTransaction())
                _eventSourcingDbContext.Database.UseTransaction(transaction.GetDbTransaction());

            _eventSourcingDbContext.EventLogs.Add(new EventLog(message));

            await _eventSourcingDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task AddToDbContext(EventBusMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _dbContext.AddAsync(new EventLog(message));
        }

        public Task MigrateAsync(CancellationToken cancellationToken)
        {
            return _eventSourcingDbContext.Database.MigrateAsync(cancellationToken);
        }
    }
}