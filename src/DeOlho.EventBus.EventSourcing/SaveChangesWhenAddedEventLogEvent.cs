using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace DeOlho.EventBus.EventSourcing
{
    public class SaveChangesWhenAddedEventLogEvent : INotification
    {
        
    }

    public class SaveChangesWhenAddedEventLogEventHandler : INotificationHandler<SaveChangesWhenAddedEventLogEvent>
    {

        readonly EventSourcingDbContext _eventSourcingDbContext;

        public SaveChangesWhenAddedEventLogEventHandler(EventSourcingDbContext eventSourcingDbContext)
        {
            _eventSourcingDbContext = eventSourcingDbContext;
        }

        public Task Handle(SaveChangesWhenAddedEventLogEvent notification, CancellationToken cancellationToken)
        {
            return _eventSourcingDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}