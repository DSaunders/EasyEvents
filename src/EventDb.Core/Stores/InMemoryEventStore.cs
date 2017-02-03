namespace EventDb.Core.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ClientInterfaces;

    public class InMemoryEventStore : IStore
    {
        private readonly List<IEvent> _events;

        public Func<IEvent, Task> EventHandler { get; set; }

        public InMemoryEventStore()
        {
            _events = new List<IEvent>();
        }
        
        public Task RaiseEventAsync(IEvent @event)
        {
            _events.Add(@event);
            return EventHandler(@event);
        }

        public async Task ReplayAllEvents()
        {
            foreach (var e in _events)
            {
                await EventHandler(e).ConfigureAwait(false);
            }
        }
    }
}