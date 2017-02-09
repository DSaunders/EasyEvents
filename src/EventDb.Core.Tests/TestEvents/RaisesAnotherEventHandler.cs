namespace EventDb.Core.Tests.TestEvents
{
    using System.Threading.Tasks;
    using ClientInterfaces;

    public class RaisesAnotherEventHandler : IEventHandler<RaisesAnotherEvent>
    {
        private readonly IEventDb _eventDb;

        public RaisesAnotherEventHandler(IEventDb eventDb)
        {
            _eventDb = eventDb;
        }

        public async Task HandleEventAsync(RaisesAnotherEvent @event)
        {
            await _eventDb.RaiseEventAsync(new NullEvent());
        }
    }
}