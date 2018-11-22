using System;
using EasyEvents.Core.Stores;
using EasyEvents.Core.Stores.InMemory;

namespace EasyEvents.Core.Configuration
{
    public class EasyEventsConfiguration
    {
        public IStore Store { get; set; }
        public Func<Type, object> HandlerFactory { get; set; }

        public EasyEventsConfiguration()
        {
            Store = new InMemoryEventStore();
        }
    }
}