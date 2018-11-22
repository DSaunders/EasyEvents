using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyEvents.Core.ClientInterfaces;
using EasyEvents.Core.Stores;

namespace EasyEvents.Core.Tests
{
    public class TestEventStore : IStore
    {
        public readonly List<IEvent> Events;

        public Func<IEvent, Task> EventHandler { get; set; }

        public TestEventStore()
        {
            Events = new List<IEvent>();
        }

        public Task RaiseEventAsync(IEvent @event)
        {
            Events.Add(@event);
            return EventHandler(@event);
        }

        public async Task ReplayAllEvents()
        {
            foreach (var e in Events)
            {
                await EventHandler(e).ConfigureAwait(false);
            }
        }
    }
}