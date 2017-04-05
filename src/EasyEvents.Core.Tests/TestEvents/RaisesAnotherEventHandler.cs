namespace EasyEvents.Core.Tests.TestEvents
{
    using System.Threading.Tasks;
    using ClientInterfaces;

    public class RaisesAnotherEventHandler : IEventHandler<RaisesAnotherEvent>
    {
        private readonly IEasyEvents _easyEvents;

        public RaisesAnotherEventHandler(IEasyEvents easyEvents)
        {
            _easyEvents = easyEvents;
        }

        public async Task HandleEventAsync(RaisesAnotherEvent @event)
        {
            await _easyEvents.RaiseEventAsync(new NullEvent());
        }
    }
}