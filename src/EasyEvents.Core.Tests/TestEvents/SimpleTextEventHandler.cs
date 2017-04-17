namespace EasyEvents.Core.Tests.TestEvents
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ClientInterfaces;

    public class SimpleTextEventHandler : IEventHandler<SimpleTextEvent>
    {
        private readonly IList<SimpleTextEvent> _eventList;

        public SimpleTextEventHandler(IList<SimpleTextEvent> eventList)
        {
            _eventList = eventList;
        }

        public Task HandleEventAsync(SimpleTextEvent @event)
        {
            _eventList.Add(@event);
            return Task.CompletedTask;
        }
    }
}